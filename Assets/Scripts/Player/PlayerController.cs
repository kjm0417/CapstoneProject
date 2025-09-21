using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 2D 쯔꾸르 스타일 플레이어 컨트롤러
/// - PlayerInput(Invoke Unity Events)에서 넘어오는 Move(UnityEvent)만 처리
/// - PlayerManager가 캐릭터 교체 시 PlayerData를 적용
/// - "이벤트 버스"를 이용해 활성 캐릭터 변경 이벤트를 구독/해제
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private Rigidbody2D _playerRb;
    [SerializeField] private Animator animator;

    [Header("Movement")]
    [Tooltip("데이터 없을 때 기본 이동 속도")]
    [SerializeField] private float _fallbackMoveSpeed = 3.5f;
    [Tooltip("대각선 입력 차단 (4방향 고정)")]
    [SerializeField] private bool _blockDiagonal = true;

    [Header("Runtime (ReadOnly)")]
    [SerializeField] private bool _isMoving;
    [SerializeField] private Vector2 _lastMoveDir = Vector2.down;

    // --- 데이터/스탯 ---
    private PlayerData currentData;
    private float moveSpeed;
    private int maxHP;
    private int currentHP;

    // --- 입력 버퍼 ---
    private Vector2 moveInput; // PlayerInput UnityEvent로부터 매 프레임 갱신됨

    // 외부(예: UI디버깅)에서 쓰고 싶으면 사용 - 이벤트 버스 생성되면 변경예정
    public event Action<PlayerData> OnDataApplied;

    private void Reset()
    {
        _playerRb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Awake()
    {
        if (_playerRb == null) _playerRb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        ApplyData(null); // 기본값으로 초기화
    }

    private void OnEnable()
    {
        // --- [이벤트 버스] 활성 캐릭터 변경 구독 ---
        // PlayerManager에서 EventBus.Publish<PlayerData>(EventNames.ActiveCharacterChanged, data) 호출을 가정
        EventBus.Subscribe<PlayerData>(EventNames.ActiveCharacterChanged, HandleCharacterChanged);
    }

    private void OnDisable()
    {
        // --- [이벤트 버스] 구독 해제 ---
        EventBus.UnSubscribe<PlayerData>(EventNames.ActiveCharacterChanged, HandleCharacterChanged);
    }

    private void Update()
    {
        // 1) 데드존
        const float DEAD = 0.2f;
        Vector2 input = moveInput;
        if (Mathf.Abs(input.x) < DEAD) input.x = 0f;
        if (Mathf.Abs(input.y) < DEAD) input.y = 0f;

        // 2) 대각선 차단 + 방향 스냅(-1/0/1)
        if (_blockDiagonal && input.sqrMagnitude > 0f)
        {
            if (Mathf.Abs(input.x) >= Mathf.Abs(input.y))
                input = new Vector2(Mathf.Sign(input.x), 0f);
            else
                input = new Vector2(0f, Mathf.Sign(input.y));
        }

        _isMoving = input.sqrMagnitude > 0f;

        // ★ 스냅된 값을 그대로 마지막 방향으로 사용(정규화 X)
        if (_isMoving)
            _lastMoveDir = input;

        // 3) 애니메이터 즉시 반영
        if (animator)
        {
            animator.SetBool("IsMoving", _isMoving);
            animator.SetFloat("MoveX", _lastMoveDir.x); // -1/0/1로 깔끔
            animator.SetFloat("MoveY", _lastMoveDir.y);
        }
    }

    private void FixedUpdate()
    {
        Vector2 input = moveInput;
        if (_blockDiagonal && input.sqrMagnitude > 1e-4f)
        {
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                input.y = 0f;
            }
            else
            {
                input.x = 0f;
            }
        }

        Vector2 vel = input.normalized * moveSpeed;
        _playerRb.MovePosition(_playerRb.position + vel * Time.fixedDeltaTime);
    }

    // PlayerInput(Unity Events) → OnMove 연결
    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            moveInput = ctx.ReadValue<Vector2>();
        else if (ctx.canceled)
            moveInput = Vector2.zero;
    }

    // --- 데이터 적용/교체(EventBus에서 수신) ---
    private void HandleCharacterChanged(PlayerData newData)
    {
        ApplyData(newData);
        // 필요하면 여기서 스프라이트, 콜라이더, 위치 보정 등도 교체
    }

    public void ApplyData(PlayerData data)
    {
        currentData = data;

        moveSpeed = currentData ? Mathf.Max(0.1f, currentData.moveSpeed) : _fallbackMoveSpeed;
        maxHP = currentData ? Mathf.Max(1, currentData.maxHP) : 10;
        if (currentHP <= 0 || currentHP > maxHP) currentHP = maxHP;

        // if (currentData /* && currentData.animatorController */)
        // {
        //     animator.runtimeAnimatorController = currentData.animatorController;
        // }

        OnDataApplied?.Invoke(currentData);
    }
}
