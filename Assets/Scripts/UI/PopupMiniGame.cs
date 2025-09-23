using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 언더테일 스타일의 배틀 오버레이:
/// - 전체 화면을 검게 페이드(모달) + 중앙에 고정 크기 박스(RectTransform) 표시
/// - Show(prefab): 박스 안에 MiniGameBase 파생 프리팹 인스턴스 → 시작
/// - Close: 미니게임 종료/중단 후 페이드 아웃, 정리
/// - (옵션) 월드 일시정지(Time.timeScale=0) 지원 + 미니게임은 unscaled delta로 갱신
/// </summary>
[RequireComponent(typeof(Canvas))]
public class PopupMiniGame : MonoBehaviour
{
    [Header("UI (Assign in Prefab)")]
    [SerializeField] private CanvasGroup _dim;           // 화면 전체 딤(검은 배경)
    [SerializeField] private Image _dimImage;            // 색상/알파 제어용(선택)
    [SerializeField] private RectTransform _frame;       // 중앙 박스(흰 테두리 등)
    [SerializeField] private CanvasGroup _frameCg;       // 박스 페이드/인터랙션
    [SerializeField] private RectTransform _gameArea;    // 미니게임 넣을 컨테이너

    [Header("Layout")]
    [SerializeField] private Vector2 _frameSize = new Vector2(640, 360); // 고정 크기
    [SerializeField] private bool keepAspect = true;

    [Header("Tween")]
    [SerializeField] private float _fadeDuration = 0.18f;
    [SerializeField] private float _scaleDuration = 0.22f;
    [SerializeField] private float _startScale = 0.85f;
    [SerializeField] private Ease _inEase = Ease.OutBack;
    [SerializeField] private Ease _outEase = Ease.InBack;

    [Header("World Pause")]
    [SerializeField] private bool _pauseWorldOnShow = true;

    private float _prevTimeScale = 1f;
    private MiniGameBase _activeGame;
    private Sequence _seq;

    /// <summary>컴포넌트 기본값을 세팅</summary>
    private void Reset()
    {
        _dim = GetComponentInChildren<CanvasGroup>(true);
        if (_dimImage == null)
        {
            _dimImage = _dim ? _dim.GetComponent<Image>() : null;
        }

        if (_frameCg == null && _frame != null)
        {
            _frameCg = _frame.GetComponent<CanvasGroup>();
        }
    }

    /// <summary>초기 상태: 비활성화/투명</summary>
    private void Awake()
    {
        if (_frame != null)
        {
            _frame.sizeDelta = _frameSize;
        }

        SetGroup(_dim, 0f, false);
        if (_frame != null)
        {
            if (_frameCg == null)
            {
                _frameCg = _frame.gameObject.AddComponent<CanvasGroup>();
            }

            SetGroup(_frameCg, 0f, false);
            _frame.localScale = Vector3.one * _startScale;
        }

        gameObject.SetActive(false);
    }

    /// <summary>CanvasGroup 상태를 일괄 설정</summary>
    private static void SetGroup(CanvasGroup cg, float alpha, bool interact)
    {
        if (!cg) return;

        cg.alpha = alpha;
        cg.interactable = interact;
        cg.blocksRaycasts = interact;
    }

    /// <summary>윈도우 크기 변경 시 중앙 박스 크기를 보정(선택)</summary>
    private void OnRectTransform_dimensionsChange()
    {
        if (!_frame) return;

        _frame.sizeDelta = _frameSize;

        if (!keepAspect) return;

        // 해상도에 따라 프레임이 너무 크게 나가지 않도록 살짝 스케일 보정(옵션)
        var canvas = GetComponent<Canvas>();
        if (canvas && canvas.renderMode != RenderMode.WorldSpace)
        {
            var screen = new Vector2(Screen.width, Screen.height);
            float scale = Mathf.Min(1f, Mathf.Min(screen.x / (_frameSize.x + 40f), screen.y / (_frameSize.y + 40f)));
            _frame.localScale = Vector3.one * scale;
        }
    }

    /// <summary>외부에서 호출: 프리팹을 박스 안에 생성하고 오버레이를 연다.</summary>
    public void Show(MiniGameBase minigamePrefab)
    {
        if (minigamePrefab == null) { Debug.LogError("[BattleOverlay] minigamePrefab is null"); return; }

        gameObject.SetActive(true);

        // 월드 일시정지
        if (_pauseWorldOnShow)
        {
            _prevTimeScale = Time.timeScale;
            Time.timeScale = 0f; // 월드 멈춤
        }

        // 초기값
        SetGroup(_dim, 0f, false);
        SetGroup(_frameCg, 0f, false);
        _frame.localScale = Vector3.one * _startScale;

        // 트윈: 딤 → 박스
        _seq?.Kill();
        _seq = DOTween.Sequence()
            .Append(_dim.DOFade(1f, _fadeDuration))
            .Join(_frame.DOScale(1f, _scaleDuration).SetEase(_inEase))
            .Join(_frameCg.DOFade(1f, _fadeDuration))
            .OnComplete(() =>
            {
                SetGroup(_dim, 1f, true);
                SetGroup(_frameCg, 1f, true);
                StartGameInternal(minigamePrefab);
            });
    }

    /// <summary>외부에서 호출: 강제 닫기(진행 중 게임도 Abort)</summary>
    public void ForceClose(string reason = "USER_FORCE")
    {
        if (_activeGame != null && _activeGame.IsRunning)
        {
            _activeGame.Abort(reason);
        }
        CloseVisual();
    }

    /// <summary>닫히는 연출 + 자식/상태 정리 + 타임스케일 복원</summary>
    private void CloseVisual()
    {
        SetGroup(_dim, _dim ? _dim.alpha : 0f, false);
        SetGroup(_frameCg, _frameCg ? _frameCg.alpha : 0f, false);

        _seq?.Kill();
        _seq = DOTween.Sequence()
            .Append(_frame.DOScale(_startScale, _scaleDuration).SetEase(_outEase))
            .Join(_frameCg.DOFade(0f, _fadeDuration))
            .Join(_dim.DOFade(0f, _fadeDuration))
            .OnComplete(() =>
            {
                // 미니게임 오브젝트 정리
                if (_activeGame != null)
                {
                    Destroy(_activeGame.gameObject);
                    _activeGame = null;
                }

                // 타임스케일 복원
                if (_pauseWorldOnShow)
                {
                    Time.timeScale = _prevTimeScale;
                }

                    gameObject.SetActive(false);
            });
    }

    /// <summary>프리팹을 _gameArea에 인스턴스하고 이벤트를 연결한다.</summary>
    private void StartGameInternal(MiniGameBase prefab)
    {
        // 안전 정리
        if (_activeGame != null) { Destroy(_activeGame.gameObject); _activeGame = null; }

        var go = Instantiate(prefab.gameObject, _gameArea);
        _activeGame = go.GetComponent<MiniGameBase>();

        // 라이프사이클 이벤트 연결
        _activeGame.Init();
        _activeGame.OnCompleted += HandleCompleted;
        _activeGame.OnAborted += HandleAborted;

        // unscaled 업데이트 루프를 위해 드라이버 추가
        var driver = go.AddComponent<UnscaledUpdateDriver>();
        driver.Bind(_activeGame);

        _activeGame.StartGame();
    }

    /// <summary>미니게임 완료 시(성공/실패) 오버레이 닫기</summary>
    private void HandleCompleted(IMiniGame game, MiniGameCompleteArgs args)
    {
        CloseVisual();
    }

    /// <summary>미니게임 중단 시 오버레이 닫기</summary>
    private void HandleAborted(IMiniGame game, string reason)
    {
        CloseVisual();
    }

    // ===== 내부 유틸 =====

    /// <summary>
    /// Time.timeScale=0에서도 미니게임이 unscaled Delta로 동작하도록 해주는 드라이버.
    /// - 자신은 MonoBehaviour.Update에서 Time.unscaledDeltaTime을 읽어
    ///   MiniGameBase.UpdateGame(unscaledDelta)를 호출한다.
    /// </summary>
    private sealed class UnscaledUpdateDriver : MonoBehaviour
    {
        private MiniGameBase _game;

        /// <summary>타겟 미니게임을 바인딩</summary>
        public void Bind(MiniGameBase game) => _game = game;

        /// <summary>unscaledDeltaTime으로 갱신을 위임</summary>
        private void Update()
        {
            if (_game != null && _game.IsRunning)
            {
                float dt = Time.unscaledDeltaTime;
                _game.UpdateGame(dt);
            }
        }
    }
}
