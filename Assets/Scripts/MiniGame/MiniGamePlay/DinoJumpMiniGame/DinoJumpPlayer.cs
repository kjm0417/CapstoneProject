using UnityEngine;

/// <summary>
/// 공룡 점프 미니게임용 2D 플레이어 컨트롤러(점프 전용)
/// - 입력 처리, 코요테 타임, 점프 버퍼(키 입력 및 터치 지원) 구현
/// - 바닥 체크(박스 Overlap + 아래쪽 Raycast 병행)
/// - 매 프레임 Update/FixedUpdate에서 HandleInput(), FixedTick() 호출 필요
/// - 초기 위치는 PlaceOnGround(groundCollider2D)로 지정
/// </summary>
[DisallowMultipleComponent]
public class DinoJumpPlayer : MonoBehaviour
{
    [Header("References")]
    [Tooltip("플레이어 Rigidbody2D (Dynamic)")]
    [SerializeField] private Rigidbody2D body;
    [Tooltip("플레이어 BoxCollider2D")]
    [SerializeField] private BoxCollider2D col;

    [Header("Physics")]
    [Tooltip("점프 초기 속도 (위 방향 +, m/s)")]
    [SerializeField] private float jumpVelocity = 14f;
    [Tooltip("중력 배율")]
    [SerializeField] private float gravityScale = 4.5f;
    [Tooltip("바닥 레이어 마스크")]
    [SerializeField] private LayerMask groundMask = ~0;

    [Header("Leniency")]
    [Tooltip("점프 입력 버퍼(초)")]
    [SerializeField] private float jumpBufferTime = 0.12f;
    [Tooltip("코요테 타임(초)")]
    [SerializeField] private float coyoteTime = 0.12f;

    [Header("Ground Check")]
    [Tooltip("바닥 박스 오버랩 높이(플레이어 아래, m)")]
    [SerializeField] private float groundOverlapHeight = 0.08f;
    [Tooltip("아래로 쏘는 Raycast 길이(m)")]
    [SerializeField] private float groundRayLength = 0.15f;

    // 내부 상태
    private float _lastJumpPressed = -999f;
    private float _lastGrounded = -999f;
    private bool _jumpLockedUntilRelease = false;
    private bool _isGrounded = false;

    public Rigidbody2D Body => body;
    public BoxCollider2D Collider => col;
    public bool IsGrounded => IsPlayerGrounded();

    /// <summary>초기화: 컴포넌트 연결 및 파라미터 설정</summary>
    public void Init()
    {
        if (!body) body = GetComponent<Rigidbody2D>();
        if (!col) col = GetComponent<BoxCollider2D>();

        if (!body || !col)
        {
            Debug.LogError("[MiniGamePlayer2D] Rigidbody2D/BoxCollider2D가 필요합니다.");
            return;
        }

        body.simulated = true;
        body.gravityScale = gravityScale;
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.linearVelocity = Vector2.zero;

        _lastGrounded = Time.time; 
        _jumpLockedUntilRelease = false;
    }

    /// <summary>입력 처리(키보드, 마우스, 터치)</summary>
    public void HandleInput()
    {
        bool down = Input.GetKeyDown(KeyCode.Space)
                 || Input.GetKeyDown(KeyCode.UpArrow)
                 || Input.GetMouseButtonDown(0)
                 || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);

        if (down)
        {
            _lastJumpPressed = Time.time;
            Debug.Log($"[DinoJumpPlayer] 점프 입력 감지! Time: {Time.time}");
        }
    }

    /// <summary>물리 프레임 처리: 점프, 코요테 타임, 버퍼 적용</summary>
    public void FixedTick()
    {
        if (!body || !col) return;

        // 바닥 체크
        bool grounded = IsPlayerGrounded();
        
        if (grounded)
        {
            _lastGrounded = Time.time;
            // 바닥에 닿으면 점프 잠금 해제 (다음 점프 준비)
            _jumpLockedUntilRelease = false;
        }

        // 점프 입력 버퍼 체크
        bool wantJump = (Time.time - _lastJumpPressed) <= jumpBufferTime;
        // 코요테 타임 체크 (바닥에서 떨어진 직후에도 점프 가능)
        bool coyote = (Time.time - _lastGrounded) <= coyoteTime;

        // 점프 실행 조건: 입력 있음 + 바닥 근처 + 잠금 안됨
        if (wantJump && coyote && !_jumpLockedUntilRelease)
        {
            // 점프 입력 소비
            _lastJumpPressed = -999f;
            // 점프 잠금 (공중에서 다시 점프 방지)
            _jumpLockedUntilRelease = true;

            // 점프 속도 적용
            Vector2 v = body.linearVelocity;
            v.y = jumpVelocity;
            body.linearVelocity = v;
            
            // Debug.Log($"[DinoJumpPlayer] ✓ 점프 실행! velocity.y = {jumpVelocity}");
        }

        // 안전장치: 플레이어가 너무 아래로 떨어지면 경고
        if (body.position.y < -10f)
        {
            Debug.LogWarning($"[DinoJumpPlayer] 플레이어가 바닥 아래로 떨어짐! Y: {body.position.y}, Grounded: {_isGrounded}");
        }
    }

    /// <summary>플레이어를 바닥 위에 정확히 위치시킴</summary>
    public void PlaceOnGround(BoxCollider2D groundCollider2D)
    {
        if (!body || !col || !groundCollider2D) return;

        float topY = groundCollider2D.bounds.max.y;
        float halfH = col.bounds.extents.y;

        Vector2 p = body.position;
        p.y = topY + halfH - 0.005f; 
        body.position = p;
        body.linearVelocity = Vector2.zero;
        Physics2D.SyncTransforms();
    }

    /// <summary>정리: Rigidbody2D 시뮬레이션 중지</summary>
    public void Cleanup()
    {
        if (body) body.simulated = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 바닥인지 확인
        if (IsGroundObject(collision.gameObject))
        {
            _isGrounded = true;
            // Debug.Log("[DinoJumpPlayer] 바닥 충돌 감지 (OnCollisionEnter2D)");
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // 바닥에서만 떨어졌을 때만 grounded 해제
        if (IsGroundObject(collision.gameObject))
        {
            _isGrounded = false;
            // Debug.Log("[DinoJumpPlayer] 바닥에서 떨어짐 (OnCollisionExit2D)");
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // 바닥과 계속 접촉 중이면 grounded 유지
        if (IsGroundObject(collision.gameObject))
        {
            _isGrounded = true;
        }
    }

    /// <summary>바닥 오브젝트인지 확인</summary>
    private bool IsGroundObject(GameObject obj)
    {
        if (obj == null) return false;

        int groundLayer = LayerMask.NameToLayer("Ground");
        
        // 레이어 또는 태그로 확인
        return (groundLayer != -1 && obj.layer == groundLayer) || obj.CompareTag("Ground");
    }

    /// <summary>바닥 체크: 충돌 이벤트만 사용 (가장 확실함)</summary>
    private bool IsPlayerGrounded()
    {
        // 충돌 이벤트만 사용 (OnCollisionEnter/Stay/Exit로 관리)
        return _isGrounded;
    }
}
