using System.Collections;
using UnityEngine;

/// <summary>
/// PopupSlideWorld와 IMiniGame을 연결하는 실행기(오케스트레이터)
/// - Show() 애니메이션이 끝난 후 Init() -> StartGame() 호출
/// - 실행 중에는 매 프레임 UpdateGame(deltaTime) 호출
/// - 완료/중단 시 Hide() 호출 및 정리
/// - SetMiniGame(...)으로 런타임에 미니게임 교체 가능
/// </summary>
[DisallowMultipleComponent]
public class MiniGamePopupRunner : MonoBehaviour
{
    [Header("Popup / Animation")]
    [Tooltip("아래->위 슬라이드 애니메이션 컨트롤러")]
    public PopupSlideWorld slide;

    [Tooltip("Show 완료 후 추가 대기(연출 여유)")]
    public float extraWaitAfterShow = 0.05f;

    [Tooltip("애니메이션/미니게임을 unscaledTime 기준으로 돌릴지")]
    public bool useUnscaledTime = true;

    // 내부 상태
    private IMiniGame _game;
    private MonoBehaviour _gameBehaviour; // 컴포넌트 참조 유지
    private bool _isRunning;
    private bool _tickEnabled;

    /// <summary>
    /// Runner가 제어할 미니게임을 런타임에 주입한다.
    /// </summary>
    public void SetMiniGame(MonoBehaviour miniGameBehaviour)
    {
        // 이전 구독 해제
        if (_game != null)
        {
            _game.OnCompleted -= HandleCompleted;
            _game.OnAborted -= HandleAborted;
        }

        _gameBehaviour = miniGameBehaviour;
        _game = _gameBehaviour as IMiniGame;

        if (_game != null)
        {
            _game.OnCompleted += HandleCompleted;
            _game.OnAborted += HandleAborted;
        }
        else
        {
            Debug.LogError("[MiniGamePopupRunner] SetMiniGame: IMiniGame 구현이 아닙니다.");
        }
    }

    /// <summary>
    /// 외부 호출: 팝업을 띄우고, 애니메이션 종료 후 미니게임 시작
    /// </summary>
    public void ShowAndStart()
    {
        if (_isRunning) return;
        if (slide == null) { Debug.LogError("[MiniGamePopupRunner] slide 참조 없음"); return; }
        if (_game == null) { Debug.LogError("[MiniGamePopupRunner] 미니게임이 세팅되지 않음"); return; }

        _isRunning = true;
        _tickEnabled = false;

        slide.Show();
        StartCoroutine(Co_StartAfterShow());
    }

    /// <summary>
    /// 외부 호출: 미니게임 강제 중단(팝업 닫기까지 수행)
    /// </summary>
    public void Abort(string reason = "USER_ABORT")
    {
        if (_game != null && _game.IsRunning) _game.Abort(reason);
        else
        {
            slide?.Hide();
            _isRunning = false;
            _tickEnabled = false;
        }
    }

    /// <summary>
    /// Show 애니메이션 시간 + 여유만큼 기다린 뒤 Init/StartGame 호출
    /// </summary>
    private IEnumerator Co_StartAfterShow()
    {
        float wait = (slide != null ? slide.duration : 0f) + Mathf.Max(0f, extraWaitAfterShow);
        if (useUnscaledTime) yield return UnscaledWait(wait);
        else yield return new WaitForSeconds(wait);

        _game.Init();
        _game.StartGame();
        _tickEnabled = true;
    }

    /// <summary>
    /// 매 프레임 IMiniGame.UpdateGame(deltaTime) 호출
    /// </summary>
    private void Update()
    {
        if (!_tickEnabled || _game == null || !_game.IsRunning) return;
        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        _game.UpdateGame(dt);
    }

    /// <summary>
    /// 미니게임 완료 시 팝업 닫기 및 상태 리셋
    /// </summary>
    private void HandleCompleted(IMiniGame game, MiniGameCompleteArgs args)
    {
        _tickEnabled = false;
        _isRunning = false;
        slide?.Hide();
    }

    /// <summary>
    /// 미니게임 중단 시 팝업 닫기 및 상태 리셋
    /// </summary>
    private void HandleAborted(IMiniGame game, string reason)
    {
        _tickEnabled = false;
        _isRunning = false;
        slide?.Hide();
    }

    /// <summary>
    /// unscaledTime 기준 대기 유틸
    /// </summary>
    private static IEnumerator UnscaledWait(float seconds)
    {
        float end = Time.unscaledTime + Mathf.Max(0f, seconds);
        while (Time.unscaledTime < end) yield return null;
    }
}
