using UnityEngine;

/// <summary>
/// RectTransform의 크기/피벗을 BoxCollider2D에 동기화(월드 스페이스 UI용).
/// - 캔버스 스케일이 변해도 로컬 단위 기준으로 맞춤.
/// - 매 프레임 갱신이 부담이면 OnValidate/Start/수동 호출로 전환해도 됨.
/// </summary>
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(BoxCollider2D))]
public class RectColliderSync2D : MonoBehaviour
{
    [Tooltip("매 프레임 자동 동기화(해제 시 Start/OnValidate에서만 갱신)")]
    public bool autoUpdateEachFrame = true;

    private RectTransform _rt;
    private BoxCollider2D _col;

    /// <summary>
    /// 컴포넌트 참조 캐시.
    /// </summary>
    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _col = GetComponent<BoxCollider2D>();
        SyncNow();
    }

    /// <summary>
    /// 에디터 값 변경 시 즉시 반영.
    /// </summary>
    private void OnValidate()
    {
        if (_rt == null || _col == null) { _rt = GetComponent<RectTransform>(); _col = GetComponent<BoxCollider2D>(); }
        SyncNow();
    }

    /// <summary>
    /// 매 프레임 갱신(옵션).
    /// </summary>
    private void LateUpdate()
    {
        if (autoUpdateEachFrame) SyncNow();
    }

    /// <summary>
    /// RectTransform -> BoxCollider2D 사이즈/오프셋 동기화.
    /// </summary>
    public void SyncNow()
    {
        if (_rt == null || _col == null) return;

        Vector2 size = _rt.rect.size; // 로컬 단위
        // pivot(0~1)을 콜라이더 오프셋으로 변환(로컬 단위)
        Vector2 pivot = _rt.pivot;
        Vector2 offset = new Vector2(
            (0.5f - pivot.x) * size.x,
            (0.5f - pivot.y) * size.y
        );

        _col.size = size;
        _col.offset = offset;
    }
}
