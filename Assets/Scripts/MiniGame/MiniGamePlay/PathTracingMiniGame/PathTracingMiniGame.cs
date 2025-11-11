using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// GridLayoutGroup 하위 UI 셀(예: 7×5)을 자동 캐시하고,
/// 내부 격자 좌표계와 GridLayoutGroup의 실제 배치( startCorner / startAxis / constraint )를 동기화하여
/// '보이는 셀 ≡ 내부 좌표'가 정확히 일치하도록 만드는 길 추적 미니게임.
/// - 방향키로 한 칸씩 이동하며 정답 경로를 밟으면 진행, 틀리면 리셋
/// - 시작/목표/드러난 셀을 색상으로 표시(원하면 SetCellVisual 확장해서 아이콘/테두리/애니메이션 가능)
/// </summary>
public class PathTracingMiniGame : MiniGameBase
{
    // UI / 시각화 설정

    [Header("UI / Grid")]
    [Tooltip("이 오브젝트의 자식(혹은 하위)에 GridLayoutGroup가 있어야 합니다. 비워두면 자동 탐색합니다.")]
    [SerializeField] private GridLayoutGroup gridLayoutGroup;

    [Tooltip("격자의 가로/세로 칸 수. GridLayoutGroup 하위 자식 수가 width×height와 같아야 합니다.")]
    [SerializeField] private int width;
    [SerializeField] private int height;

    [Header("Cell Colors")]
    [SerializeField] private Color unknownColor = new Color(0.15f, 0.15f, 0.15f, 1f);
    [SerializeField] private Color revealedColor = new Color(0.95f, 0.95f, 0.95f, 1f);
    [SerializeField] private Color startColor = new Color(0.2f, 0.7f, 1f, 1f);
    [SerializeField] private Color goalColor = new Color(1f, 0.6f, 0.2f, 1f);
    [SerializeField] private Color wrongFlashColor = new Color(1f, 0.25f, 0.25f, 1f);
    [SerializeField] private float wrongFlashSeconds = 0.08f;

    // 규칙 / 입력 설정

    [Header("Rules / Input")]
    [Tooltip("시작 좌표(내부 격자 기준). 이 좌표가 화면의 어떤 셀인지 자동 매핑됩니다.")]
    [SerializeField] private Vector2Int startCoordinate = Vector2Int.zero;

    [Tooltip("true면 목표는 x=width-1의 랜덤 y, false면 아래 고정 좌표 사용")]
    [SerializeField] private bool randomizeGoal = true;

    [SerializeField] private Vector2Int fixedGoalCoordinate = new Vector2Int(7, 7);

    [Tooltip("키 입력 최소 간격(초)")]
    [SerializeField] private float inputCooldownSeconds = 0.1f;

    [Header("힌트 설정")]
    [Tooltip("게임 시작 시 정답 경로를 한 칸씩 보여줄지 여부")]
    [SerializeField] private bool showHintAtStart = true;

    [Tooltip("힌트로 각 칸을 보여주는 간격(초)")]
    [SerializeField] private float hintStepInterval = 0.3f;

    [Tooltip("힌트 표시 색상")]
    [SerializeField] private Color hintColor = new Color(0.5f, 0.9f, 0.5f, 1f);

    // 내부 상태

    /// <summary>네 방향(상우하좌)</summary>
    private static readonly Vector2Int[] Cardinal = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

    /// <summary>격자 칸 상태</summary>
    private enum CellState { Unknown, SolutionPath }

    /// <summary>UI 셀 캐시 [x,y]</summary>
    private Image[,] _cells;

    /// <summary>정답 경로 마스크</summary>
    private CellState[,] _gridStates;

    /// <summary>정답 경로(순서대로)</summary>
    private readonly List<Vector2Int> _solutionPath = new List<Vector2Int>();

    /// <summary>드러낸 셀 기록(리셋용)</summary>
    private readonly List<Vector2Int> _revealed = new List<Vector2Int>();

    /// <summary>목표/플레이어/진행/쿨다운</summary>
    private Vector2Int _goal;
    private Vector2Int _player;
    private int _progressIndex;
    private float _cooldown;

    /// <summary>난수(경로 셔플)</summary>
    private System.Random _rng;

    /// <summary>힌트 표시 중인지 여부</summary>
    private bool _isShowingHint = false;

    // MiniGameBase 훅

    /// <summary>
    /// 시작 시: GridLayoutGroup 찾기 → 셀 캐시(레이아웃 설정 반영) → 검증 → 격자/경로 생성 → 시각 리셋 → 진행 리셋
    /// </summary>
    protected override void OnStartHook()
    {
        base.OnStartHook();

        if (width <= 0 || height <= 0)
        {
            Debug.LogError("[PathTracingMiniGame] width/height는 1 이상이어야 합니다.");
            Abort("INVALID_GRID");
            return;
        }

        // GridLayoutGroup 자동 탐색(미지정 시)
        if (gridLayoutGroup == null)
            gridLayoutGroup = GetComponentInChildren<GridLayoutGroup>(includeInactive: true);

        if (gridLayoutGroup == null)
        {
            Debug.LogError("[PathTracingMiniGame] GridLayoutGroup을 찾지 못했습니다.");
            Abort("NO_GRID");
            return;
        }

        // 셀 캐시(※ 여기서 GridLayoutGroup의 startCorner/startAxis/constraint를 읽어 실제 (x,y)로 매핑)
        if (!CacheCellImagesFromLayout())
        {
            Abort("CELL_CACHE_FAIL");
            return;
        }

        if (!IsInside(startCoordinate))
        {
            Debug.LogError($"[PathTracingMiniGame] 시작 좌표 {startCoordinate} 가 범위를 벗어났습니다.");
            Abort("INVALID_START");
            return;
        }

        _rng = new System.Random(Guid.NewGuid().GetHashCode());
        AllocateGrid();
        BuildSolutionPath();

        ResetVisualAll();
        ResetProgress(initializing: true);

        // 힌트 표시
        if (showHintAtStart)
        {
            StartCoroutine(ShowPathHint());
        }
    }

    /// <summary>
    /// 매 프레임: 입력 쿨다운 관리 → 1회 방향 입력 → 전진 시도
    /// </summary>
    protected override void OnUpdateHook(float deltaTime)
    {
        base.OnUpdateHook(deltaTime);
        if (!IsRunning) return;

        // 힌트 표시 중에는 입력 무시
        if (_isShowingHint) return;

        if (_cooldown > 0f) _cooldown -= deltaTime;

        Vector2Int? dir = ReadArrowOnce();
        if (!dir.HasValue || _cooldown > 0f) return;

        _cooldown = inputCooldownSeconds;
        TryAdvance(dir.Value);
    }

    /// <summary>
    /// 종료/중단 시 정리
    /// </summary>
    protected override void OnCleanupHook()
    {
        base.OnCleanupHook();
        _solutionPath.Clear();
        _revealed.Clear();
        _cells = null;
        _gridStates = null;
    }

    // 레이아웃 인식형 셀 캐시

    /// <summary>
    /// GridLayoutGroup 하위 자식들의 '하이어라키 인덱스'를 '격자 (x,y)'로 변환해 Image[,]에 저장.
    /// - Unity의 GridLayoutGroup은 startCorner / startAxis / constraint 에 따라 배치 순서가 달라짐.
    /// - 여기서 그 규칙을 그대로 복원하여 내부 좌표와 '보이는 자리'를 1:1로 맞춥니다.
    /// </summary>
    private bool CacheCellImagesFromLayout()
    {
        int expected = width * height;
        int actual = gridLayoutGroup.transform.childCount;
        if (actual != expected)
        {
            Debug.LogError($"[PathTracingMiniGame] 셀 개수 불일치: 기대 {expected}, 실제 {actual}. " +
                           "GridLayoutGroup 하위에 정확히 width×height개의 자식을 배치하세요.");
        }

        _cells = new Image[width, height];

        for (int i = 0; i < Math.Min(actual, expected); i++)
        {
            // 인덱스 i(0..n-1)가 GridLayoutGroup 설정상 어느 (x,y)인지 계산
            (int x, int y) = IndexToXY_ByLayout(i, gridLayoutGroup, width, height);

            Transform t = gridLayoutGroup.transform.GetChild(i);
            Image img = t.GetComponent<Image>();
            if (img == null)
            {
                Debug.LogError($"[PathTracingMiniGame] 자식 {i}에 Image가 없습니다.");
                return false;
            }

            _cells[x, y] = img;
        }

        // 누락 체크
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (_cells[x, y] == null)
                {
                    Debug.LogError($"[PathTracingMiniGame] 좌표({x},{y})에 해당하는 Image가 매핑되지 않았습니다. " +
                                   "GridLayoutGroup의 startCorner/startAxis/constraint 설정과 width/height를 확인하세요.");
                    return false;
                }

        return true;
    }

    /// <summary>
    /// 하이어라키 인덱스 i → (x,y) 변환.
    /// GridLayoutGroup의 startCorner / startAxis / constraint를 고려해 실제 배치와 동일한 좌표를 돌려줍니다.
    /// </summary>
    private static (int x, int y) IndexToXY_ByLayout(int i, GridLayoutGroup glg, int width, int height)
    {
        // 1) startAxis에 따라 기본 (col,row) 계산 (UpperLeft 기준, 행/열 증가 방향은 아래에서 보정)
        int col, row;
        if (glg.startAxis == GridLayoutGroup.Axis.Horizontal)
        {
            // 가로로 먼저 채움: 한 줄에 width개
            row = i / width;
            col = i % width;
        }
        else // Vertical
        {
            // 세로로 먼저 채움: 한 컬럼에 height개
            col = i / height;
            row = i % height;
        }

        // 2) startCorner 반영
        //    UpperLeft:  X 좌→우, Y 상→하
        //    UpperRight: X 우→좌, Y 상→하
        //    LowerLeft:  X 좌→우, Y 하→상
        //    LowerRight: X 우→좌, Y 하→상
        bool flipX = (glg.startCorner == GridLayoutGroup.Corner.UpperRight ||
                      glg.startCorner == GridLayoutGroup.Corner.LowerRight);
        bool flipY = (glg.startCorner == GridLayoutGroup.Corner.LowerLeft ||
                      glg.startCorner == GridLayoutGroup.Corner.LowerRight);

        if (flipX) col = (width - 1) - col;
        if (flipY) row = (height - 1) - row;

        // 3) (col,row) = (x,y)
        return (Mathf.Clamp(col, 0, width - 1), Mathf.Clamp(row, 0, height - 1));
    }

    // 시각화 유틸

    /// <summary>
    /// 전체 셀을 unknown으로 칠하고, 시작/목표만 강조.
    /// </summary>
    private void ResetVisualAll()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                SetCellVisual(new Vector2Int(x, y), unknownColor);

        SetCellVisual(startCoordinate, startColor);
        SetCellVisual(_goal, goalColor);
    }

    /// <summary>
    /// (x,y) 셀의 색을 설정(필요하면 여기서 아이콘/테두리/애니메이션도 처리).
    /// </summary>
    private void SetCellVisual(Vector2Int c, Color color)
    {
        if (!IsInside(c) || _cells == null) return;
        _cells[c.x, c.y].color = color;
    }

    // 격자/경로 생성

    /// <summary>격자 상태를 Unknown으로 초기화</summary>
    private void AllocateGrid()
    {
        _gridStates = new CellState[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                _gridStates[x, y] = CellState.Unknown;
    }

    /// <summary>
    /// 목표 좌표 결정 후 DFS 백트래킹으로 start→goal 단일 경로 생성,
    /// 생성된 경로를 _gridStates에 SolutionPath로 표시.
    /// </summary>
    private void BuildSolutionPath()
    {
        _solutionPath.Clear();
        _goal = ResolveGoal();

        if (_goal == startCoordinate)
        {
            _goal = new Vector2Int(Mathf.Clamp(startCoordinate.x + 1, 0, width - 1), startCoordinate.y);
        }

        _solutionPath.Add(startCoordinate);
        var visited = new HashSet<Vector2Int> { startCoordinate };

        if (!TryBuildPath(startCoordinate, _goal, visited))
        {
            Debug.LogError("[PathTracingMiniGame] 경로 생성 실패");
            Abort("PATH_FAIL");
            return;
        }

        foreach (var cell in _solutionPath)
            _gridStates[cell.x, cell.y] = CellState.SolutionPath;
    }

    /// <summary>
    /// 목표 좌표 결정: randomizeGoal이면 (width-1, 랜덤 y), 아니면 고정(Clamp)
    /// </summary>
    private Vector2Int ResolveGoal()
    {
        if (randomizeGoal)
        {
            int gx = width - 1;
            int gy = _rng.Next(height);
            return new Vector2Int(gx, gy);
        }
        return new Vector2Int(
            Mathf.Clamp(fixedGoalCoordinate.x, 0, width - 1),
            Mathf.Clamp(fixedGoalCoordinate.y, 0, height - 1)
        );
    }

    /// <summary>
    /// DFS 백트래킹으로 current→goal 경로 1개 생성(막히면 되돌아감).
    /// </summary>
    private bool TryBuildPath(Vector2Int current, Vector2Int goal, HashSet<Vector2Int> visited)
    {
        if (current == goal) return true;

        foreach (var dir in ShuffledCardinal())
        {
            var next = current + dir;
            if (!IsInside(next) || visited.Contains(next)) continue;

            visited.Add(next);
            _solutionPath.Add(next);

            if (TryBuildPath(next, goal, visited)) return true;

            visited.Remove(next);
            _solutionPath.RemoveAt(_solutionPath.Count - 1);
        }
        return false;
    }

    /// <summary>네 방향을 Fisher–Yates로 셔플</summary>
    private Vector2Int[] ShuffledCardinal()
    {
        var a = new Vector2Int[Cardinal.Length];
        Array.Copy(Cardinal, a, a.Length);
        for (int i = a.Length - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (a[i], a[j]) = (a[j], a[i]);
        }
        return a;
    }

    // 진행/입력

    /// <summary>
    /// 진행 상태를 초기화(플레이어를 시작칸으로, 드러난 셀 초기화)하고 시작/목표 강조 유지.
    /// </summary>
    private void ResetProgress(bool initializing)
    {
        foreach (var c in _revealed)
            if (c != startCoordinate && c != _goal)
                SetCellVisual(c, unknownColor);
        _revealed.Clear();

        _player = startCoordinate;
        _progressIndex = 0;
        _cooldown = 0f;

        _revealed.Add(_player);
        SetCellVisual(_player, startColor);
        SetCellVisual(_goal, goalColor);
    }

    /// <summary>
    /// 한 칸 전진 시도: 범위/길이/정답 체크 후 반영 또는 리셋(번쩍 연출).
    /// </summary>
    private void TryAdvance(Vector2Int dir)
    {
        var target = _player + dir;

        if (!IsInside(target))
        {
            ReduceRemainingTime(5f);
            StartCoroutine(FlashWrongThenReset());
            return;
        }

        int nextIndex = _progressIndex + 1;
        if (nextIndex >= _solutionPath.Count)
        {
            ReduceRemainingTime(5f);
            StartCoroutine(FlashWrongThenReset());
            return;
        }

        if (_solutionPath[nextIndex] != target ||
            _gridStates[target.x, target.y] != CellState.SolutionPath)
        {
            ReduceRemainingTime(5f);
            StartCoroutine(FlashWrongThenReset(target));
            return;
        }

        _progressIndex = nextIndex;
        _player = target;
        _revealed.Add(target);

        if (target == _goal) SetCellVisual(target, goalColor);
        else if (target == startCoordinate) SetCellVisual(target, startColor);
        else SetCellVisual(target, revealedColor);

        if (_progressIndex == _solutionPath.Count - 1)
        {
            Complete(true);
        }
    }

    /// <summary>방향키 1회 입력 읽기</summary>
    private Vector2Int? ReadArrowOnce()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) return Vector2Int.down;
        if (Input.GetKeyDown(KeyCode.DownArrow)) return Vector2Int.up;
        if (Input.GetKeyDown(KeyCode.LeftArrow)) return Vector2Int.left;
        if (Input.GetKeyDown(KeyCode.RightArrow)) return Vector2Int.right;
        return null;
    }

    /// <summary>좌표 범위 체크</summary>
    private bool IsInside(Vector2Int c) => c.x >= 0 && c.x < width && c.y >= 0 && c.y < height;

    /// <summary>
    /// 남은 시간을 줄입니다 (틀렸을 때 패널티)
    /// </summary>
    private void ReduceRemainingTime(float seconds)
    {
        ReduceTime(seconds);
    }

    // 힌트 표시

    /// <summary>
    /// 게임 시작 시 정답 경로를 한 칸씩 보여주는 힌트
    /// </summary>
    private IEnumerator ShowPathHint()
    {
        _isShowingHint = true;

        // 시작과 목표는 이미 표시되어 있으므로 중간 경로만 표시
        for (int i = 0; i < _solutionPath.Count; i++)
        {
            var cell = _solutionPath[i];
            
            // 시작과 목표는 원래 색상 유지
            if (cell == startCoordinate)
            {
                SetCellVisual(cell, startColor);
            }
            else if (cell == _goal)
            {
                SetCellVisual(cell, goalColor);
            }
            else
            {
                SetCellVisual(cell, hintColor);
            }

            yield return new WaitForSeconds(hintStepInterval);
        }

        // 힌트 표시 완료 후 다시 숨기기
        yield return new WaitForSeconds(0.5f);

        // 시작과 목표를 제외한 모든 칸을 다시 숨김
        for (int i = 1; i < _solutionPath.Count - 1; i++)
        {
            var cell = _solutionPath[i];
            SetCellVisual(cell, unknownColor);
        }

        _isShowingHint = false;
    }

    // 피드백(잘못 칸 번쩍 + 리셋)

    /// <summary>
    /// 잘못 입력 시 잠깐 번쩍 후 전체 리셋. target 주면 해당 칸만 번쩍.
    /// </summary>
    private IEnumerator FlashWrongThenReset(Vector2Int? target = null)
    {
        if (wrongFlashSeconds > 0f)
        {
            if (target.HasValue && IsInside(target.Value))
            {
                var c = target.Value;
                var img = _cells[c.x, c.y];
                var prev = img.color;
                img.color = wrongFlashColor;
                yield return new WaitForSecondsRealtime(wrongFlashSeconds);
                img.color = prev;
            }
            else
            {
                var prevs = new Dictionary<Image, Color>();
                foreach (var img in _cells) prevs[img] = img.color;
                foreach (var img in _cells) img.color = wrongFlashColor;
                yield return new WaitForSecondsRealtime(wrongFlashSeconds);
                foreach (var kv in prevs) kv.Key.color = kv.Value;
            }
        }
        ResetProgress(initializing: false);
    }
}
