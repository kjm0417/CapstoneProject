using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

/// <summary>
/// DOTween을 이용해 UI 패널을 아래에서 위로 슬라이드 인/아웃하는 간단한 컨트롤러
/// </summary>
public class PopupMiniGame : MonoBehaviour
{
    [Header("Animation Target")]
    [Tooltip("슬라이드 시킬 대상 RectTransform (본인 오브젝트)")]
    public RectTransform target;

    [Header("Animation Settings")]
    [Tooltip("애니메이션 시간(초)")]
    public float duration = 0.35f;
    [Tooltip("이징 함수")]
    public Ease ease = Ease.OutCubic;
    [Tooltip("timeScale=0 상태에서도 동작하도록 할지 여부")]
    public bool useUnscaledTime = true;

    [Header("Test Button (Optional)")]
    public Button testButton1;
    public Button testButton2;

    private Vector2 _onPos;   // 원래 자리
    private Vector2 _offPos;  // 화면 아래쪽 자리

    private void Awake()
    {
        if (target == null) target = GetComponent<RectTransform>();

        // 현재 anchoredPosition을 "원래 자리"로 저장
        _onPos = target.anchoredPosition;

        // 화면 아래로 살짝 숨긴 위치 계산
        float h = target.rect.height;
        _offPos = _onPos + new Vector2(0f, -(h + 50f));

        // 처음엔 아래에 숨겨둔다
        target.anchoredPosition = _offPos;
    }

    /// <summary>
    /// 아래에서 위로 올라오는 애니메이션 실행
    /// </summary>
    public void Show()
    {
        target.DOAnchorPos(_onPos, duration)
              .SetEase(ease)
              .SetUpdate(useUnscaledTime);
    }

    /// <summary>
    /// 다시 아래로 내려가는 애니메이션 실행
    /// </summary>
    public void Hide()
    {
        target.DOAnchorPos(_offPos, duration)
              .SetEase(Ease.InCubic)
              .SetUpdate(useUnscaledTime);
    }
}
