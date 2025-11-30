using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

/// <summary>
/// World Space(�Ǵ� Screen Space) ĵ�������� UI �г��� �Ʒ�->���� �����̵� ��/�ƿ��ϴ� ��Ʈ�ѷ�.
/// - World Space: localPosition.y�� �������� �̵�(DOLocalMove)
/// - Screen Space: anchoredPosition.y�� �������� �̵�(DOAnchorPos)
/// - duration�� DOTween SetUpdate(true)�� unscaledTime ���(���� timeScale=0�̾ ����)
/// - �г� ���� + margin��ŭ '�Ʒ�(off-screen)' ��ġ�� �ڵ� ���
/// </summary>
[DisallowMultipleComponent]
public class PopupSlideWorld : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("�����̵� ��ų �г� RectTransform(���� ������ �ڱ� �ڽ�)")]
    public RectTransform target;

    [Header("Animation")]
    [Tooltip("�ִϸ��̼� �ð�(��)")]
    public float duration = 0.35f;
    [Tooltip("��¡ �Լ�")]
    public Ease easeIn = Ease.OutCubic;
    [Tooltip("���� �� ��¡ �Լ�")]
    public Ease easeOut = Ease.InCubic;
    [Tooltip("timeScale=0������ Ʈ�� ����")]
    public bool useUnscaledTime = true;
    [Tooltip("ó���� �ڵ����� ���� ���¿��� ����")]
    public bool startHidden = true;

    [Header("Offsets")]
    [Tooltip("�г� ���� + margin ��ŭ �Ʒ��� ����(�ڵ� ���)")]
    public bool autoComputeOffscreen = true;
    [Tooltip("�ڵ� ���� �߰� ����(���� ����)")]
    public float extraMargin = 50f;
    [Tooltip("���� ����� �� ���� ������(�Ʒ��� ����)")]
    public float manualStartYOffset = -800f;

    // ���� ����
    private Vector3 _onLocalPos; // ���̴� ��ġ(local)
    private Vector3 _offLocalPos; // �Ʒ� ���� ��ġ(local)
    private Vector2 _onAnchored; // ��ũ�� �����̽���
    private Vector2 _offAnchored;
    private Tween _tween;
    private Canvas _canvas;
    private bool _isWorldSpace;

    /// <summary>
    /// ������Ʈ �ʱ�ȭ �� ��ġ ���.
    /// </summary>
    private void Awake()
    {
        if (target == null) target = GetComponent<RectTransform>();
        _canvas = target.GetComponentInParent<Canvas>();
        _isWorldSpace = _canvas != null && _canvas.renderMode == RenderMode.WorldSpace;

        if (_isWorldSpace)
        {
            // ����(���̴�) ��ġ
            _onLocalPos = target.localPosition;

            // �г� ����(���� ����) + ������ŭ �Ʒ���
            float h = target.rect.height;
            float offset = autoComputeOffscreen ? (h + extraMargin) : -manualStartYOffset; // manual�� ���� ���
            _offLocalPos = _onLocalPos + new Vector3(0f, -offset, 0f);

            if (startHidden)
                target.localPosition = _offLocalPos;
        }
        else
        {
            // ��ũ�� �����̽� ����(Ȥ�� ��ȯ�� �� ������)
            _onAnchored = target.anchoredPosition;
            float h = target.rect.height;
            float offset = autoComputeOffscreen ? (h + extraMargin) : -manualStartYOffset;
            _offAnchored = _onAnchored + new Vector2(0f, -offset);

            if (startHidden)
                target.anchoredPosition = _offAnchored;
        }
    }

    /// <summary>
    /// Start에서 강제로 숨김 처리 (Awake에서 안될 경우 대비)
    /// </summary>
    private void Start()
    {
        if (startHidden)
        {
            HideImmediate();
        }
    }

    /// <summary>
    /// 즉시 숨김 (애니메이션 없이)
    /// </summary>
    public void HideImmediate()
    {
        KillTween();
        
        if (_isWorldSpace)
        {
            target.localPosition = _offLocalPos;
        }
        else
        {
            target.anchoredPosition = _offAnchored;
        }
    }

    /// <summary>
    /// �Ʒ����� ���� �����̵� ��(ǥ��).
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
    /// �ٽ� �Ʒ��� �����̵� �ƿ�(����).
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
    /// ���� �� Ʈ�� ���� ����.
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
    /// (����) ���� ��ġ�� '���̴� ��ġ'�� ����� �����ϰ� ������ũ���� �ٽ� ���.
    /// ���̾ƿ��� �ٲ� �� ȣ���ϸ� ����.
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
