using UnityEngine;

/// <summary>
/// Push Out 미니게임 플레이어 컨트롤러
/// - WASD/화살표/조이스틱 입력으로 이동
/// - 푸셔 충돌 시 임펄스 적용
/// </summary>
[DisallowMultipleComponent]
public class PushOutPlayer : MonoBehaviour
{
    [Header("References")]
    [Tooltip("플레이어 Rigidbody2D (Dynamic)")]
    [SerializeField] private Rigidbody2D body;
    [Tooltip("플레이어 Collider2D")]
    [SerializeField] private Collider2D hitCollider;

    [Header("Movement")]
    [Tooltip("이동 속도")]
    [SerializeField] private float moveSpeed = 6f;
    [Tooltip("최대 속도")]
    [SerializeField] private float maxSpeed = 7.5f;
    [Tooltip("가감속 보간 계수")]
    [SerializeField] private float acceleration = 12f;

    private Vector2 _input;

    public Rigidbody2D Body => body;
    public Collider2D Collider => hitCollider;
    public Vector2 Position => body ? body.position : (Vector2)transform.position;

    /// <summary>플레이어 초기화 및 시작 위치 지정</summary>
    public void Init(Vector2 startPosition)
    {
        if (!body) body = GetComponent<Rigidbody2D>();
        if (!hitCollider) hitCollider = GetComponent<Collider2D>();

        if (!body)
        {
            Debug.LogError("[PushOutPlayer] Rigidbody2D가 필요합니다.");
            return;
        }

        body.simulated = true;
        body.gravityScale = 0f;
        body.linearDamping = 0f;
        body.angularDamping = 0f;
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.position = startPosition;
        body.linearVelocity = Vector2.zero;
        _input = Vector2.zero;
    }

    /// <summary>입력 처리</summary>
    public void HandleInput(float deltaTime)
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        // 패드/키보드 혼합 지원
        if (Mathf.Approximately(x, 0f))
        {
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) x = -1f;
            else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) x = 1f;
        }

        if (Mathf.Approximately(y, 0f))
        {
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) y = -1f;
            else if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) y = 1f;
        }

        Vector2 input = new Vector2(x, y);
        if (input.sqrMagnitude > 1f) input.Normalize();
        _input = input;
    }

    /// <summary>물리 갱신</summary>
    public void FixedTick(float deltaTime)
    {
        if (!body) return;

        Vector2 targetVelocity = _input * moveSpeed;
        Vector2 current = body.linearVelocity;
        current = Vector2.MoveTowards(current, targetVelocity, acceleration * deltaTime);
        current = Vector2.ClampMagnitude(current, maxSpeed);
        body.linearVelocity = current;
    }

    /// <summary>푸셔 충돌 시 임펄스를 적용</summary>
    public void ApplyImpulse(Vector2 impulse)
    {
        if (!body) return;
        body.AddForce(impulse, ForceMode2D.Impulse);
    }

    /// <summary>정리</summary>
    public void Cleanup()
    {
        if (body) body.simulated = false;
    }
}
