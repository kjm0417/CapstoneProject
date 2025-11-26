using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

/// <summary>
/// 보스전용 페이드 인/아웃 팝업 컨트롤러.
/// CanvasGroup의 alpha를 조절하여 점점 나타나고 사라지는 효과를 제공합니다.
/// </summary>
[DisallowMultipleComponent]
public class PopupFadeWorld : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("페이드 효과를 적용할 패널 RectTransform")]
    public RectTransform target;

    [Header("Animation")]
    [Tooltip("페이드 애니메이션 시간(초)")]
    public float duration = 0.5f;
    [Tooltip("페이드 인 이징 함수")]
    public Ease easeIn = Ease.OutQuad;
    [Tooltip("페이드 아웃 이징 함수")]
    public Ease easeOut = Ease.InQuad;
    [Tooltip("timeScale=0에서도 트윈 동작")]
    public bool useUnscaledTime = true;
    [Tooltip("처음에 자동으로 숨김 상태에서 시작")]
    public bool startHidden = true;

    // 내부 상태
    private CanvasGroup _canvasGroup;
    private Tween _tween;

    /// <summary>
    /// 컴포넌트 초기화
    /// </summary>
    private void Awake()
    {
        if (target == null)
            target = GetComponent<RectTransform>();

        // CanvasGroup 자동 추가
        _canvasGroup = target.GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = target.gameObject.AddComponent<CanvasGroup>();
        }

        if (startHidden)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
        else
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }
    }

    /// <summary>
    /// 페이드 인 (점점 나타남)
    /// </summary>
    public void Show()
    {
        KillTween();

        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;

        _tween = _canvasGroup.DOFade(1f, duration)
                             .SetEase(easeIn)
                             .SetUpdate(useUnscaledTime);
    }

    /// <summary>
    /// 페이드 아웃 (점점 사라짐)
    /// </summary>
    public void Hide()
    {
        KillTween();

        _tween = _canvasGroup.DOFade(0f, duration)
                             .SetEase(easeOut)
                             .SetUpdate(useUnscaledTime)
                             .OnComplete(() =>
                             {
                                 _canvasGroup.interactable = false;
                                 _canvasGroup.blocksRaycasts = false;
                             });
    }

    /// <summary>
    /// 즉시 표시 (애니메이션 없이)
    /// </summary>
    public void ShowImmediate()
    {
        KillTween();
        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }

    /// <summary>
    /// 즉시 숨김 (애니메이션 없이)
    /// </summary>
    public void HideImmediate()
    {
        KillTween();
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// 진행 중인 트윈 정리
    /// </summary>
    private void KillTween()
    {
        if (_tween != null && _tween.IsActive())
        {
            _tween.Kill();
            _tween = null;
        }
    }

    private void OnDestroy()
    {
        KillTween();
    }
}
