using System;
using UnityEngine;
using UnityEngine.InputSystem; 

/// <summary>
/// 2D 쯔꾸르 스타일 플레이어 컨트롤러
/// - PlayerInput(Invoke Unity Events)에서 넘어오는 Move(UnityEvent)만 처리
/// - PlayerManager가 캐릭터 교체 시 PlayerData를 적용
/// - 나중에 "이벤트 버스"로 바꿀 곳은 TODO로 표기

public class PlayerController : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private Rigidbody2D _playerRb;
    //[SerializeField] private Animator animator;

    [Header("Movement")]
    [Tooltip("데이터 없을 때 기본 이동 속도")]
    [SerializeField] private float _fallbackMoveSpeed = 3.5f;
    [Tooltip("대각선 입력 차단 (4방향 고정)")]
    [SerializeField] private bool _blockDiagonal = true;

    [Header("Runtime (ReadOnly)")]
    [SerializeField] private bool _isMoving;
    [SerializeField] private Vector2 _lastMoveDir = Vector2.down;

    // --- 데이터/스탯 ---
    // PlayerState라는 데이터를 각 플레이어에게 넘겨줘서 수치 변경용으로 사용하고 저장할 떄 PlayerManager에서 저장하고 변경할 떄 저장 후 변경
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
        //animator = GetComponent<Animator>();
    }

    private void Awake()
    {
         _playerRb = GetComponent<Rigidbody2D>();
         //animator = GetComponent<Animator>();

        ApplyData(null); // 기본값으로 초기화
    }

    private void OnEnable()
    {
        // --- 현재는 C# 이벤트로 PlayerManager 변화 구독 ---
        //PlayerManager.ActiveCharacterChanged += HandleCharacterChanged;

        // --- [향후 전환] 이벤트 버스 구독 (구현되면 아래 줄로 교체) ---
        // TODO(EventBus): EventBus.Subscribe<PlayerData>("ActiveCharacterChanged", HandleCharacterChanged);
        //  - 만약 제네릭 이벤트 타입을 쓰면:
        //    EventBus.Subscribe<CharacterChangedEvent>(e => HandleCharacterChanged(e.Data));
    }

    private void OnDisable()
    {
        //PlayerManager.ActiveCharacterChanged -= HandleCharacterChanged;

        // TODO(EventBus): EventBus.Unsubscribe<PlayerData>("ActiveCharacterChanged", HandleCharacterChanged);
        // 혹은 제네릭 이벤트 언서브스크라이브
    }

    private void Update()
    {
        // 쯔꾸르: 대각선 차단(큰 축만 남긴다)
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

        // 애니메이터 파라미터

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
        _playerRb.MovePosition( _playerRb.position + vel * Time.fixedDeltaTime);
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            moveInput = ctx.ReadValue<Vector2>();
        }
        else if (ctx.canceled)
        {
            moveInput = Vector2.zero;
        }
    }


    // 데이터 적용/교체
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

        if (currentData)//currentData.animatorController)
        {
            //animator.runtimeAnimatorController = currentData.animatorController;
        }

        OnDataApplied?.Invoke(currentData);
    }

}

