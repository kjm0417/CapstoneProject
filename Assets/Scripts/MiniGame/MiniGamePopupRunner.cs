using System.Collections;
using UnityEngine;

/// <summary>
/// PopupSlideWorld�� IMiniGame�� �����ϴ� �����(���ɽ�Ʈ������)
/// - Show() �ִϸ��̼��� ���� �� Init() -> StartGame() ȣ��
/// - ���� �߿��� �� ������ UpdateGame(deltaTime) ȣ��
/// - �Ϸ�/�ߴ� �� Hide() ȣ�� �� ����
/// - SetMiniGame(...)���� ��Ÿ�ӿ� �̴ϰ��� ��ü ����
/// </summary>
[DisallowMultipleComponent]
public class MiniGamePopupRunner : MonoBehaviour
{
    [Header("Popup / Animation")]
    [Tooltip("페이드 인/아웃 애니메이션 컨트롤러 (모든 미니게임 공통)")]
    public PopupFadeWorld fadePopup;

    [Header("Game Areas")]
    [Tooltip("일반 미니게임 영역 (MiniGame 타입일 때 활성화)")]
    public GameObject miniGameArea;

    [Tooltip("보스전 영역 (BossFight 타입일 때 활성화)")]
    public GameObject bossArea;

    [Header("Settings")]
    [Tooltip("Show 완료 후 추가 대기(선택 사항)")]
    public float extraWaitAfterShow = 0.05f;

    [Tooltip("애니메이션/미니게임이 unscaledTime 기준으로 동작할지")]
    public bool useUnscaledTime = true;

    // ���� ����
    private IMiniGame _game;
    private MonoBehaviour _gameBehaviour; // ������Ʈ ���� ����
    private bool _isRunning;
    private bool _tickEnabled;

    /// <summary>
    /// Runner�� ������ �̴ϰ����� ��Ÿ�ӿ� �����Ѵ�.
    /// </summary>
    public void SetMiniGame(MonoBehaviour miniGameBehaviour)
    {
        // ���� ���� ����
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
            Debug.LogError("[MiniGamePopupRunner] SetMiniGame: IMiniGame ������ �ƴմϴ�.");
        }
    }

    /// <summary>
    /// 외부 호출: 팝업을 표시, 애니메이션 완료 후 미니게임 시작
    /// </summary>
    public void ShowAndStart()
    {
        if (_isRunning) return;
        if (_game == null)
        {
            Debug.LogError("[MiniGamePopupRunner] 미니게임이 설정되지 않음");
            return;
        }

        // 페이드 팝업 검증
        if (fadePopup == null)
        {
            Debug.LogError("[MiniGamePopupRunner] fadePopup이 없음");
            return;
        }

        _isRunning = true;
        _tickEnabled = false;

        // 게임 타입에 따라 적절한 영역 활성화
        GameEncounterType encounterType = _game.EncounterType;
        ActivateGameArea(encounterType);

        // 페이드 애니메이션 실행
        fadePopup.Show();

        StartCoroutine(Co_StartAfterShow());
    }

    /// <summary>
    /// 외부 호출: 미니게임 강제 중단(팝업 닫기까지 처리)
    /// </summary>
    public void Abort(string reason = "USER_ABORT")
    {
        if (_game != null && _game.IsRunning)
        {
            _game.Abort(reason);
        }
        else
        {
            HidePopup();
            _isRunning = false;
            _tickEnabled = false;
        }
    }

    /// <summary>
    /// Show 애니메이션 시간 + 대기만큼 기다린 후 Init/StartGame 호출
    /// </summary>
    private IEnumerator Co_StartAfterShow()
    {
        float animDuration = fadePopup != null ? fadePopup.duration : 0f;
        float wait = animDuration + Mathf.Max(0f, extraWaitAfterShow);
        
        if (useUnscaledTime) yield return UnscaledWait(wait);
        else yield return new WaitForSeconds(wait);

        _game.Init();
        _game.StartGame();
        _tickEnabled = true;
    }

    /// <summary>
    /// �� ������ IMiniGame.UpdateGame(deltaTime) ȣ��
    /// </summary>
    private void Update()
    {
        if (!_tickEnabled || _game == null || !_game.IsRunning) return;
        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        _game.UpdateGame(dt);
    }

    /// <summary>
    /// 미니게임 완료 시 팝업 닫기 및 상태 정리
    /// </summary>
    private void HandleCompleted(IMiniGame game, MiniGameCompleteArgs args)
    {
        _tickEnabled = false;
        _isRunning = false;
        HidePopup();
    }

    /// <summary>
    /// 미니게임 중단 시 팝업 닫기 및 상태 정리
    /// </summary>
    private void HandleAborted(IMiniGame game, string reason)
    {
        _tickEnabled = false;
        _isRunning = false;
        HidePopup();
    }

    /// <summary>
    /// 팝업 숨김 처리
    /// </summary>
    private void HidePopup()
    {
        fadePopup?.Hide();
        
        // 모든 영역 비활성화
        DeactivateAllAreas();
    }

    /// <summary>
    /// 게임 타입에 따라 적절한 영역 활성화
    /// </summary>
    private void ActivateGameArea(GameEncounterType encounterType)
    {
        if (encounterType == GameEncounterType.MiniGame)
        {
            if (miniGameArea != null) miniGameArea.SetActive(true);
            if (bossArea != null) bossArea.SetActive(false);
        }
        else // BossFight
        {
            if (miniGameArea != null) miniGameArea.SetActive(false);
            if (bossArea != null) bossArea.SetActive(true);
            Debug.Log("[MiniGamePopupRunner] 보스전 영역 활성화");
        }
    }

    /// <summary>
    /// 모든 게임 영역 비활성화
    /// </summary>
    private void DeactivateAllAreas()
    {
        if (miniGameArea != null) miniGameArea.SetActive(false);
        if (bossArea != null) bossArea.SetActive(false);
    }

    /// <summary>
    /// unscaledTime ���� ��� ��ƿ
    /// </summary>
    private static IEnumerator UnscaledWait(float seconds)
    {
        float end = Time.unscaledTime + Mathf.Max(0f, seconds);
        while (Time.unscaledTime < end) yield return null;
    }
}
