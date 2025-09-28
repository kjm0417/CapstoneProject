using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

/// <summary>
/// World Space(또는 Screen Space) 캔버스에서 UI 패널을 아래->위로 슬라이드 인/아웃하는 컨트롤러.
/// - World Space: localPosition.y를 기준으로 이동(DOLocalMove)
/// - Screen Space: anchoredPosition.y를 기준으로 이동(DOAnchorPos)
/// - duration은 DOTween SetUpdate(true)로 unscaledTime 사용(전역 timeScale=0이어도 동작)
/// - 패널 높이 + margin만큼 '아래(off-screen)' 위치를 자동 계산
/// </summary>
[DisallowMultipleComponent]
public class PopupSlideWorld : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("슬라이드 시킬 패널 RectTransform(지정 없으면 자기 자신)")]
    public RectTransform target;

    [Header("Animation")]
    [Tooltip("애니메이션 시간(초)")]
    public float duration = 0.35f;
    [Tooltip("이징 함수")]
    public Ease easeIn = Ease.OutCubic;
    [Tooltip("숨길 때 이징 함수")]
    public Ease easeOut = Ease.InCubic;
    [Tooltip("timeScale=0에서도 트윈 유지")]
    public bool useUnscaledTime = true;
    [Tooltip("처음에 자동으로 숨긴 상태에서 시작")]
    public bool startHidden = true;

    [Header("Offsets")]
    [Tooltip("패널 높이 + margin 만큼 아래로 내림(자동 계산)")]
    public bool autoComputeOffscreen = true;
    [Tooltip("자동 계산시 추가 마진(로컬 단위)")]
    public float extraMargin = 50f;
    [Tooltip("수동 모드일 때 시작 오프셋(아래로 음수)")]
    public float manualStartYOffset = -800f;

    // 내부 상태
    private Vector3 _onLocalPos; // 보이는 위치(local)
    private Vector3 _offLocalPos; // 아래 숨긴 위치(local)
    private Vector2 _onAnchored; // 스크린 스페이스용
    private Vector2 _offAnchored;
    private Tween _tween;
    private Canvas _canvas;
    private bool _isWorldSpace;

    /// <summary>
    /// 컴포넌트 초기화 및 위치 계산.
    /// </summary>
    private void Awake()
    {
        if (target == null) target = GetComponent<RectTransform>();
        _canvas = target.GetComponentInParent<Canvas>();
        _isWorldSpace = _canvas != null && _canvas.renderMode == RenderMode.WorldSpace;

        if (_isWorldSpace)
        {
            // 기준(보이는) 위치
            _onLocalPos = target.localPosition;

            // 패널 높이(로컬 단위) + 마진만큼 아래로
            float h = target.rect.height;
            float offset = autoComputeOffscreen ? (h + extraMargin) : -manualStartYOffset; // manual은 음수 기대
            _offLocalPos = _onLocalPos + new Vector3(0f, -offset, 0f);

            if (startHidden)
                target.localPosition = _offLocalPos;
        }
        else
        {
            // 스크린 스페이스 대응(혹시 전환될 수 있으니)
            _onAnchored = target.anchoredPosition;
            float h = target.rect.height;
            float offset = autoComputeOffscreen ? (h + extraMargin) : -manualStartYOffset;
            _offAnchored = _onAnchored + new Vector2(0f, -offset);

            if (startHidden)
                target.anchoredPosition = _offAnchored;
        }
    }

    /// <summary>
    /// 아래에서 위로 슬라이드 인(표시).
    /// </summary>
    public void Show()
    {
        KillTween();

        if (_isWorldSpace)
        {
            _tween = target.DOLocalMove(_onLocalPos, duration)
                           .SetEase(easeIn)
                           .SetUpdate(useUnscaledTime);
        }
        else
        {
            _tween = target.DOAnchorPos(_onAnchored, duration)
                           .SetEase(easeIn)
                           .SetUpdate(useUnscaledTime);
        }
    }

    /// <summary>
    /// 다시 아래로 슬라이드 아웃(숨김).
    /// </summary>
    public void Hide()
    {
        KillTween();

        if (_isWorldSpace)
        {
            _tween = target.DOLocalMove(_offLocalPos, duration)
                           .SetEase(easeOut)
                           .SetUpdate(useUnscaledTime);
        }
        else
        {
            _tween = target.DOAnchorPos(_offAnchored, duration)
                           .SetEase(easeOut)
                           .SetUpdate(useUnscaledTime);
        }
    }

    /// <summary>
    /// 진행 중 트윈 안전 종료.
    /// </summary>
    private void KillTween()
    {
        if (_tween != null && _tween.IsActive())
        {
            _tween.Kill();
            _tween = null;
        }
    }

    /// <summary>
    /// (선택) 현재 위치를 '보이는 위치'로 재기준 설정하고 오프스크린을 다시 계산.
    /// 레이아웃이 바뀐 뒤 호출하면 유용.
    /// </summary>
    public void RecomputeFromCurrent()
    {
        if (_isWorldSpace)
        {
            _onLocalPos = target.localPosition;
            float h = target.rect.height;
            float offset = autoComputeOffscreen ? (h + extraMargin) : -manualStartYOffset;
            _offLocalPos = _onLocalPos + new Vector3(0f, -offset, 0f);
        }
        else
        {
            _onAnchored = target.anchoredPosition;
            float h = target.rect.height;
            float offset = autoComputeOffscreen ? (h + extraMargin) : -manualStartYOffset;
            _offAnchored = _onAnchored + new Vector2(0f, -offset);
        }
    }
}
