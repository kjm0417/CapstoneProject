using UnityEngine;

/// <summary>
/// �̴ϰ��� ���� 2D �÷��̾� ��Ʈ�ѷ�(���� ���)
/// - �Է� ����, �ڿ��� Ÿ��, ������(Ű ���� �� ������ ����) ����
/// - ���� ����(���� OverlapBox + �ϰ� Raycast ����)
/// - �ܺο��� Update/FixedUpdate�� ���� ȣ���ϴ� ����: HandleInput(), FixedTick()
/// - �ʱ� ��ġ�� PlaceOnGround(groundCollider2D)�� ����
/// </summary>
[DisallowMultipleComponent]
public class DinoJumpPlayer : MonoBehaviour
{
    [Header("References")]
    [Tooltip("�÷��̾� Rigidbody2D (Dynamic)")]
    [SerializeField] private Rigidbody2D body;
    [Tooltip("�÷��̾� BoxCollider2D")]
    [SerializeField] private BoxCollider2D col;

    [Header("Physics")]
    [Tooltip("���� �ʱ� �ӵ� (���� +, m/s)")]
    [SerializeField] private float jumpVelocity = 14f;
    [Tooltip("�߷� ������")]
    [SerializeField] private float gravityScale = 4.5f;
    [Tooltip("���� ���̾� ����ũ")]
    [SerializeField] private LayerMask groundMask = ~0;

    [Header("Leniency")]
    [Tooltip("���� �Է� ����(��)")]
    [SerializeField] private float jumpBufferTime = 0.12f;
    [Tooltip("�ڿ��� Ÿ��(��)")]
    [SerializeField] private float coyoteTime = 0.12f;

    [Header("Ground Check")]
    [Tooltip("�ظ� �ڽ� ������ ����(�������� ����, m)")]
    [SerializeField] private float groundOverlapHeight = 0.08f;
    [Tooltip("�ϰ� �� ���� Raycast ����(m)")]
    [SerializeField] private float groundRayLength = 0.15f;

    // ���� ����
    private float _lastJumpPressed = -999f;
    private float _lastGrounded = -999f;
    private bool _jumpLockedUntilRelease = false;

    /// <summary>���� Rigidbody2D ���� ��ȯ</summary>
    public Rigidbody2D Body => body;
    /// <summary>���� BoxCollider2D ���� ��ȯ</summary>
    public BoxCollider2D Collider => col;
    /// <summary>���鿡 ��Ҵ��� ����(������ ���� ����� �ƴ϶� ��� ������)</summary>
    public bool IsGrounded => IsPlayerGrounded();

    /// <summary>�ʱ� ����: �ʼ� ������Ʈ ���� �� ���� �Ķ���� ����</summary>
    public void Init()
    {
        if (!body) body = GetComponent<Rigidbody2D>();
        if (!col) col = GetComponent<BoxCollider2D>();

        if (!body || !col)
        {
            Debug.LogError("[MiniGamePlayer2D] Rigidbody2D/BoxCollider2D�� �ʿ��մϴ�.");
            return;
        }

        body.simulated = true;
        body.gravityScale = gravityScale;
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.linearVelocity = Vector2.zero;

        _lastGrounded = Time.time; // ���� �� �ڿ��� ���
        _jumpLockedUntilRelease = false;
    }

    /// <summary>�ܺ� �Է� ó��(���۸� + Ű ���� ������ ����)</summary>
    public void HandleInput()
    {
        bool down = Input.GetKeyDown(KeyCode.Space)
                 || Input.GetKeyDown(KeyCode.UpArrow)
                 || Input.GetMouseButtonDown(0)
                 || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);

        bool held = Input.GetKey(KeyCode.Space)
                 || Input.GetKey(KeyCode.UpArrow)
                 || Input.GetMouseButton(0)
                 || (Input.touchCount > 0);

        if (down) _lastJumpPressed = Time.time;
        if (!held) _jumpLockedUntilRelease = false; // Ű�� ������ ���� ���� ���� ���
    }

    /// <summary>���� ƽ���� ȣ��: �������� + ����/�ڿ���/�� ��Ģ���� ���� ����</summary>
    public void FixedTick()
    {
        if (!body || !col) return;

        // ���� üũ
        if (IsPlayerGrounded())
        {
            _lastGrounded = Time.time;
            _jumpLockedUntilRelease = false; // ���� �� �� ����
        }

        // ���� (���� + �ڿ��� + ������)
        bool wantJump = (Time.time - _lastJumpPressed) <= jumpBufferTime;
        bool coyote = (Time.time - _lastGrounded) <= coyoteTime;

        if (wantJump && coyote && !_jumpLockedUntilRelease)
        {
            _lastJumpPressed = -999f;
            _jumpLockedUntilRelease = true;

            Vector2 v = body.linearVelocity;
            v.y = jumpVelocity;
            body.linearVelocity = v;
        }
    }

    /// <summary>�÷��̾ ������ ���� �ݶ��̴� ��ܿ� ��Ȯ�� �ø�</summary>
    public void PlaceOnGround(BoxCollider2D groundCollider2D)
    {
        if (!body || !col || !groundCollider2D) return;

        float topY = groundCollider2D.bounds.max.y;
        float halfH = col.bounds.extents.y;

        Vector2 p = body.position;
        p.y = topY + halfH - 0.005f; // ��¦ �İ���� ����ȭ
        body.position = p;
        body.linearVelocity = Vector2.zero;
        Physics2D.SyncTransforms();
    }

    /// <summary>����: ���� ����</summary>
    public void Cleanup()
    {
        if (body) body.simulated = false;
    }

    /// <summary>���� ����(���� OverlapBox + �ϰ� Raycast ����)</summary>
    private bool IsPlayerGrounded()
    {
        if (!col) return false;

        // 1) ���� OverlapBox
        Bounds b = col.bounds;
        Vector2 size = new Vector2(b.size.x * 0.95f, Mathf.Max(0.02f, groundOverlapHeight));
        Vector2 center = new Vector2(b.center.x, b.min.y + size.y * 0.5f);

        Collider2D[] hits = new Collider2D[2];
        int n = Physics2D.OverlapBoxNonAlloc(center, size, 0f, hits, groundMask);
        if (n > 0) return true;

        // 2) �ϰ� �� Raycast ����
        if (body.linearVelocity.y <= 0f)
        {
            Vector2 origin = new Vector2(b.center.x, b.min.y + 0.01f);
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundRayLength, groundMask);
            if (hit.collider != null) return true;
        }

        return false;
    }
}
