using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 4x4 슬라이딩 퍼즐 미니게임.
/// 원본 이미지를 16개 조각으로 나누고, 빈 칸을 WASD로 이동시켜 퍼즐을 완성합니다.
/// </summary>
public class SlidingPuzzleMiniGame : MiniGameBase
{
    [Header("Puzzle Settings")]
    [SerializeField] private GridLayoutGroup puzzleGrid;
    [SerializeField] private Sprite[] tileSprites; // 1~15번 타일 이미지 (16개 중 15개)
    [SerializeField] private int gridSize = 4; // 4x4 그리드
    [SerializeField] private int shuffleMoves = 50; // 초기 섞기 이동 횟수

    [Header("Visual Feedback")]
    [SerializeField] private bool useNumberMode = true; // 이미지 없이 숫자로 표시
    [SerializeField] private Color emptyTileColor = new Color(0.2f, 0.2f, 0.2f, 0.3f);
    [SerializeField] private Color tileColor = Color.white;
    [SerializeField] private int fontSize = 24;

    private readonly List<SlidingPuzzleTile> tiles = new List<SlidingPuzzleTile>();
    private int emptyTileIndex; // 빈 타일의 현재 위치 (0~15)
    private int moveCount; // 이동 횟수

    /// <summary>
    /// 게임 시작 시 초기화
    /// </summary>
    protected override void OnStartHook()
    {
        base.OnStartHook();

        // 그리드 자동 탐색
        if (puzzleGrid == null)
        {
            puzzleGrid = GetComponentInChildren<GridLayoutGroup>(includeInactive: true);
        }

        if (puzzleGrid == null)
        {
            Debug.LogError("[SlidingPuzzleMiniGame] GridLayoutGroup을 찾을 수 없습니다.");
            Abort("NO_GRID");
            return;
        }

        // 타일 초기화
        InitializeTiles();

        // 퍼즐 섞기
        ShufflePuzzle();

        moveCount = 0;
    }

    /// <summary>
    /// 매 프레임 입력 처리
    /// </summary>
    protected override void OnUpdateHook(float deltaTime)
    {
        base.OnUpdateHook(deltaTime);

        if (!IsRunning)
            return;

        HandleInput();
    }

    /// <summary>
    /// 타일 초기화 및 생성
    /// </summary>
    private void InitializeTiles()
    {
        tiles.Clear();

        // 기존 타일 제거
        foreach (Transform child in puzzleGrid.transform)
        {
            Destroy(child.gameObject);
        }

        // 숫자 모드가 아닐 때만 스프라이트 검증
        if (!useNumberMode && (tileSprites == null || tileSprites.Length < 15))
        {
            Debug.LogWarning("[SlidingPuzzleMiniGame] 타일 스프라이트가 부족합니다. 숫자 모드로 전환합니다.");
            useNumberMode = true;
        }

        // 16개 타일 생성 (1~15번 + 빈 타일)
        for (int i = 0; i < gridSize * gridSize; i++)
        {
            GameObject tileObj = new GameObject($"Tile_{i + 1}");
            tileObj.transform.SetParent(puzzleGrid.transform, false);

            // Image 컴포넌트 추가
            Image img = tileObj.AddComponent<Image>();
            
            bool isEmpty = (i == gridSize * gridSize - 1); // 마지막 타일이 빈 타일

            // 숫자 모드: Text 추가
            Text numberText = null;
            if (useNumberMode)
            {
                GameObject textObj = new GameObject("NumberText");
                textObj.transform.SetParent(tileObj.transform, false);
                
                numberText = textObj.AddComponent<Text>();
                numberText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                numberText.fontSize = fontSize;
                numberText.alignment = TextAnchor.MiddleCenter;
                numberText.color = Color.black;
                
                RectTransform textRect = textObj.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.sizeDelta = Vector2.zero;
                
                if (!isEmpty)
                {
                    numberText.text = (i + 1).ToString();
                }
                
                img.color = isEmpty ? emptyTileColor : tileColor;
            }
            else
            {
                // 이미지 모드: 스프라이트 사용
                Sprite sprite = isEmpty ? null : tileSprites[i];
                img.sprite = sprite;
                img.color = isEmpty ? emptyTileColor : Color.white;
            }
            
            // SlidingPuzzleTile 컴포넌트 추가
            SlidingPuzzleTile tile = tileObj.AddComponent<SlidingPuzzleTile>();
            Sprite tileSprite = (!useNumberMode && !isEmpty) ? tileSprites[i] : null;
            tile.Initialize(i + 1, i, tileSprite, isEmpty);

            if (isEmpty)
            {
                emptyTileIndex = i;
            }

            tiles.Add(tile);
        }

        // 레이아웃 강제 갱신
        LayoutRebuilder.ForceRebuildLayoutImmediate(puzzleGrid.GetComponent<RectTransform>());
    }

    /// <summary>
    /// WASD 입력 처리
    /// </summary>
    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            TryMoveTile(Direction.Up);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            TryMoveTile(Direction.Down);
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            TryMoveTile(Direction.Left);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            TryMoveTile(Direction.Right);
        }
    }

    /// <summary>
    /// 타일 이동 시도
    /// </summary>
    private void TryMoveTile(Direction direction)
    {
        int targetIndex = GetAdjacentIndex(emptyTileIndex, direction);

        if (targetIndex < 0)
            return; // 이동 불가

        // 타일 교환
        SwapTiles(emptyTileIndex, targetIndex);
        moveCount++;

        // 클리어 체크는 CheckClearRealtime에서 자동 처리됨
    }

    /// <summary>
    /// 인접한 타일 인덱스 계산
    /// </summary>
    private int GetAdjacentIndex(int currentIndex, Direction direction)
    {
        int row = currentIndex / gridSize;
        int col = currentIndex % gridSize;

        switch (direction)
        {
            case Direction.Up:
                if (row > 0) return currentIndex - gridSize;
                break;
            case Direction.Down:
                if (row < gridSize - 1) return currentIndex + gridSize;
                break;
            case Direction.Left:
                if (col > 0) return currentIndex - 1;
                break;
            case Direction.Right:
                if (col < gridSize - 1) return currentIndex + 1;
                break;
        }

        return -1; // 이동 불가
    }

    /// <summary>
    /// 두 타일의 위치 교환
    /// </summary>
    private void SwapTiles(int indexA, int indexB)
    {
        if (indexA < 0 || indexA >= tiles.Count || indexB < 0 || indexB >= tiles.Count)
            return;

        // 리스트에서 교환
        SlidingPuzzleTile temp = tiles[indexA];
        tiles[indexA] = tiles[indexB];
        tiles[indexB] = temp;

        // Transform 순서 교환 (화면 위치 변경)
        tiles[indexA].transform.SetSiblingIndex(indexA);
        tiles[indexB].transform.SetSiblingIndex(indexB);

        // 빈 타일 인덱스 갱신
        if (tiles[indexA].IsEmpty)
            emptyTileIndex = indexA;
        else if (tiles[indexB].IsEmpty)
            emptyTileIndex = indexB;

        // 레이아웃 갱신
        LayoutRebuilder.ForceRebuildLayoutImmediate(puzzleGrid.GetComponent<RectTransform>());
    }

    /// <summary>
    /// 퍼즐 섞기 (랜덤 이동)
    /// </summary>
    private void ShufflePuzzle()
    {
        Direction[] directions = { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
        Direction lastDirection = Direction.Up;

        for (int i = 0; i < shuffleMoves; i++)
        {
            // 이전 이동의 반대 방향은 제외 (무의미한 왕복 방지)
            Direction randomDirection;
            do
            {
                randomDirection = directions[Random.Range(0, directions.Length)];
            }
            while (IsOppositeDirection(randomDirection, lastDirection));

            int targetIndex = GetAdjacentIndex(emptyTileIndex, randomDirection);
            if (targetIndex >= 0)
            {
                SwapTiles(emptyTileIndex, targetIndex);
                lastDirection = randomDirection;
            }
        }
    }

    /// <summary>
    /// 반대 방향 체크
    /// </summary>
    private bool IsOppositeDirection(Direction dir1, Direction dir2)
    {
        return (dir1 == Direction.Up && dir2 == Direction.Down) ||
               (dir1 == Direction.Down && dir2 == Direction.Up) ||
               (dir1 == Direction.Left && dir2 == Direction.Right) ||
               (dir1 == Direction.Right && dir2 == Direction.Left);
    }

    /// <summary>
    /// 클리어 조건 체크 (모든 타일이 정답 위치에 있는지)
    /// </summary>
    protected override bool CheckClearRealtime()
    {
        if (!IsRunning || tiles.Count == 0)
            return false;

        for (int i = 0; i < tiles.Count; i++)
        {
            if (!tiles[i].IsInCorrectPosition(i))
                return false;
        }

        return true; // 모든 타일이 정답 위치
    }

    /// <summary>
    /// 정리
    /// </summary>
    protected override void OnCleanupHook()
    {
        base.OnCleanupHook();
        tiles.Clear();
        moveCount = 0;
    }

    /// <summary>
    /// 이동 방향 열거형
    /// </summary>
    private enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
}
