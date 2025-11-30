using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 2D 탑다운 스타일 플레이어 컨트롤러
/// - EventBus와 IPlayerStateReadOnly를 사용(= 메타데이터 상태를 받아와서 캐릭터 전환)
/// - 실제 리소스(속도/애니메이션/스프라이트)는 PlayerManager에서 PlayerData를 찾아 적용
/// - Update: 입력 처리와 애니메이션 파라미터 설정 / FixedUpdate: 물리 이동
/// - IPausable 인터페이스 구현: PauseManager에서 일시정지 관리/해제 가능
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour, IPausable
{
    [Header("Links")]
    [SerializeField] private Rigidbody2D _playerRb;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Data")]
    [Tooltip("매니저 연결 전 사용할 기본 PlayerData (없으면 fallback 속도만 사용)")]
    [SerializeField] private PlayerData initialData;

    [Header("Movement")]
    [Tooltip("데이터가 없을 때 기본 이동 속도")]
    [SerializeField] private float _fallbackMoveSpeed = 3.5f;
    [Tooltip("대각선 입력 차단 (4방향 이동)")]
    [SerializeField] private bool _blockDiagonal = true;

    [Header("Runtime (ReadOnly)")]
    [SerializeField] private bool _isMoving;
    [SerializeField] private Vector2 _lastMoveDir = Vector2.down;
    [SerializeField] private bool _canMove = true;
    [SerializeField] private bool _isPaused = false;
    private Vector2 _processedInput;

    // 입력 관련
    private Vector2 moveInput;

    // 메타데이터 상태/실제 리소스 관련
    private IPlayerStateReadOnly _state;
    private PlayerData _appliedData;

    public event Action<PlayerData> OnDataApplied;

    // 애니메이션 파라미터 해시
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int MoveYHash = Animator.StringToHash("MoveY");

    // 스프라이트 캐시(중복 로드 방지)
    private static readonly Dictionary<string, Sprite[]> _spriteCache = new Dictionary<string, Sprite[]>();

    // 이동 속도 계산(데이터가 있으면 사용)
    private float Speed => _appliedData != null
        ? Mathf.Max(0.1f, _appliedData.moveSpeed)
        : _fallbackMoveSpeed;

    /// <summary>
    /// 컴포넌트 기본 링크를 자동 설정합니다.
    /// </summary>
    private void Reset()
    {
        _playerRb = GetComponent<Rigidbody2D>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    /// <summary>
    /// 필수 링크를 설정하고 초기 데이터/스프라이트를 적용합니다.
    /// </summary>
    private void Awake()
    {
        if (_playerRb == null) _playerRb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // 초기엔 PlayerState가 없을 수 있으므로 PlayerData 먼저 적용 시도
        ApplyData(initialData);
    }

    /// <summary>
    /// Start에서 PauseManager에 등록 (Awake 실행 순서 문제 해결)
    /// </summary>
    private void Start()
    {
        // PauseManager에 등록하여 일시정지 관리 받기
        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.RegisterPausable(this);
        }
    }

    /// <summary>
    /// 활성 캐릭터 변경(상태) 이벤트를 구독합니다.
    /// </summary>
    private void OnEnable()
    {
        EventBus.Subscribe<IPlayerStateReadOnly>(EventNames.ActiveCharacterChanged, HandleStateChanged);
    }

    /// <summary>
    /// 구독을 해제합니다.
    /// </summary>
    private void OnDisable()
    {
        EventBus.UnSubscribe<IPlayerStateReadOnly>(EventNames.ActiveCharacterChanged, HandleStateChanged);
    }

    /// <summary>
    /// 오브젝트가 파괴될 때 PauseManager에서 등록 해제합니다.
    /// </summary>
    private void OnDestroy()
    {
        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.UnregisterPausable(this);
        }
    }

    /// <summary>
    /// 입력을 처리하고 애니메이션 파라미터를 설정합니다.
    /// </summary>
    private void Update()
    {
        // 일시정지 상태면 입력 처리 차단
        if (_isPaused)
        {
            return;
        }

        _processedInput = ProcessInput(moveInput, _blockDiagonal);
        _isMoving = _processedInput.sqrMagnitude > 0f;

        if (_isMoving)
        {
            _lastMoveDir = SnapDir(_processedInput);
        }

        if (animator)
        {
            if (HasParam(animator, IsMovingHash)) animator.SetBool(IsMovingHash, _isMoving);
            if (HasParam(animator, MoveXHash)) animator.SetFloat(MoveXHash, _lastMoveDir.x);
            if (HasParam(animator, MoveYHash)) animator.SetFloat(MoveYHash, _lastMoveDir.y);
        }
    }

    /// <summary>
    /// 물리 틱마다 이동을 처리합니다.
    /// </summary>
    private void FixedUpdate()
    {
        // 디버그: 일시정지 상태 확인
        if (_isPaused)
        {
            // Debug.Log($"[PlayerController] FixedUpdate: 일시정지 상태로 이동 차단");
            return;
        }
        
        if (!_canMove)
        {
            // Debug.Log($"[PlayerController] FixedUpdate: _canMove가 false로 이동 차단");
            return;
        }
        
        if (_processedInput.sqrMagnitude <= 1e-4f) return;

        Vector2 vel = _processedInput.normalized * Speed;
        _playerRb.MovePosition(_playerRb.position + vel * Time.fixedDeltaTime);
    }

    /// <summary>
    /// PlayerInput Move(UnityEvent)에서 호출: 입력을 변수에 저장합니다.
    /// </summary>
    public void OnMove(InputAction.CallbackContext ctx)
    {
        // 일시정지 상태면 입력 무시
        if (_isPaused)
        {
            moveInput = Vector2.zero;
            return;
        }

        if (ctx.performed) moveInput = ctx.ReadValue<Vector2>();
        else if (ctx.canceled) moveInput = Vector2.zero;
    }

    /// <summary>
    /// EventBus로부터 상태를 받아왔을 때 호출됩니다.
    /// - 상태는 메타데이터 위치(HP 등) 관련
    /// - 실제 리소스는 PlayerManager에서 PlayerData를 찾아 적용
    /// </summary>
    private void HandleStateChanged(IPlayerStateReadOnly newState)
    {
        _state = newState;

        // 실제 리소스 조회(속도/애니메이션/스프라이트등)
        var data = PlayerManager.Instance ? PlayerManager.Instance.GetData(_state?.Id) : null;
        ApplyData(data);

        // 필요하다면 _state.OnChanged를 구독하여 HP 변화 등을 UI에 반영하는 것도 가능
        // (현재는 컨트롤러 이동/애니메이션 담당하므로 생략)
    }

    /// <summary>
    /// PlayerData를 적용하고(속도/애니메이션/스프라이트), 필요한 경우 애니메이션 컨트롤러를 리로드합니다.
    /// </summary>
    public void ApplyData(PlayerData data)
    {
        _appliedData = data;

        // 애니메이션 컨트롤러 교체
        if (animator != null && _appliedData != null && _appliedData.animatorController != null)
        {
            if (animator.runtimeAnimatorController != _appliedData.animatorController)
            {
                animator.runtimeAnimatorController = _appliedData.animatorController;
                animator.Rebind();
                animator.Update(0f);
            }

            if (HasParam(animator, IsMovingHash)) animator.SetBool(IsMovingHash, _isMoving);
            if (HasParam(animator, MoveXHash)) animator.SetFloat(MoveXHash, _lastMoveDir.x);
            if (HasParam(animator, MoveYHash)) animator.SetFloat(MoveYHash, _lastMoveDir.y);
        }

        // Resources 경로 기반으로 기본 스프라이트 설정(애니메이션 없을때)
        TryApplySpriteFromResources(_appliedData);

        OnDataApplied?.Invoke(_appliedData);
    }

    /// <summary>
    /// PlayerData.resourcesAddress/defaultSpriteName을 기반으로 기본 스프라이트를 설정합니다.
    /// </summary>
    private void TryApplySpriteFromResources(PlayerData data)
    {
        if (!spriteRenderer || data == null || string.IsNullOrEmpty(data.resourcesAddress))
            return;

        var sprites = LoadSpritesCached(data.resourcesAddress);
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning($"[PlayerController] Resources에서 스프라이트를 찾을 수 없습니다: {data.resourcesAddress}");
            return;
        }

        Sprite chosen = null;
        if (!string.IsNullOrEmpty(data.defaultSpriteName))
        {
            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i] && sprites[i].name == data.defaultSpriteName)
                {
                    chosen = sprites[i]; break;
                }
            }
        }
        if (chosen == null) chosen = sprites[0];

        spriteRenderer.sprite = chosen;
    }

    /// <summary>
    /// Resources.LoadAll<Sprite>(path)를 캐시와 함께 사용합니다.
    /// </summary>
    private static Sprite[] LoadSpritesCached(string path)
    {
        if (string.IsNullOrEmpty(path)) return Array.Empty<Sprite>();

        if (_spriteCache.TryGetValue(path, out var cached) && cached != null && cached.Length > 0)
            return cached;

        var loaded = Resources.LoadAll<Sprite>(path);
        if (loaded == null || loaded.Length == 0)
            return Array.Empty<Sprite>();

        _spriteCache[path] = loaded;
        return loaded;
    }

    /// <summary>
    /// 데드존 적용: 미세한 입력 값을 0으로 만듭니다.
    /// </summary>
    private static Vector2 Deadzone(Vector2 v, float dead = 0.2f)
    {
        if (Mathf.Abs(v.x) < dead) v.x = 0f;
        if (Mathf.Abs(v.y) < dead) v.y = 0f;
        return v;
    }

    /// <summary>
    /// 4방향 스냅: 더 큰 축을 우선(-1/0/1)로 만듭니다.
    /// </summary>
    private static Vector2 SnapDir(Vector2 v)
    {
        if (v.sqrMagnitude <= 0f) return Vector2.zero;
        return Mathf.Abs(v.x) >= Mathf.Abs(v.y)
            ? new Vector2(Mathf.Sign(v.x), 0f)
            : new Vector2(0f, Mathf.Sign(v.y));
    }

    /// <summary>
    /// 이동 입력을 최종 처리(데드존+옵션별 대각선 차단)합니다.
    /// </summary>
    private static Vector2 ProcessInput(Vector2 raw, bool blockDiagonal)
    {
        var input = Deadzone(raw);
        if (input.sqrMagnitude <= 0f) return Vector2.zero;
        if (!blockDiagonal) return input;
        return SnapDir(input);
    }

    /// <summary>
    /// 현재 애니메이터에 해당 파라미터가 존재하는지 확인합니다.
    /// </summary>
    private static bool HasParam(Animator anim, int nameHash)
    {
        if (!anim) return false;
        var ps = anim.parameters;
        for (int i = 0; i < ps.Length; i++)
            if (ps[i].nameHash == nameHash) return true;
        return false;
    }

    /// <summary>
    /// 외부에서 이동 가능/불가능을 설정합니다(컷신/대화 등).
    /// </summary>
    public void SetMovementEnabled(bool enabled) => _canMove = enabled;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Door_Interact"))
        {
            InteractObject objectInfo = collision.GetComponent<InteractObject>();

            EventBus.Publish(StoryTriggerManager.OnPlayerCollided, objectInfo);
        }
    }

    #region IPausable Implementation

    /// <summary>
    /// IPausable: 일시정지 상태를 확인합니다.
    /// </summary>
    public bool IsPaused => _isPaused;

    /// <summary>
    /// IPausable: 플레이어 이동과 애니메이션을 일시정지합니다.
    /// </summary>
    public void Pause()
    {
        if (!_isPaused)
        {
            _isPaused = true;
            
            // 입력 초기화
            moveInput = Vector2.zero;
            _processedInput = Vector2.zero;
            _isMoving = false;
            
            // Rigidbody2D 속도 정지 및 물리 비활성화
            if (_playerRb != null)
            {
                _playerRb.linearVelocity = Vector2.zero;
                _playerRb.simulated = false;
            }
            
            // 애니메이션을 Idle 상태로 고정
            if (animator != null)
            {
                if (HasParam(animator, IsMovingHash)) animator.SetBool(IsMovingHash, false);
                animator.speed = 0f;
            }
        }
    }

    /// <summary>
    /// IPausable: 플레이어 이동과 애니메이션을 재개합니다.
    /// </summary>
    public void Resume()
    {
        if (_isPaused)
        {
            _isPaused = false;
            
            // 입력 초기화 (재개 시 깨끗한 상태로 시작)
            moveInput = Vector2.zero;
            _processedInput = Vector2.zero;
            
            // Rigidbody2D 물리 재활성화
            if (_playerRb != null)
            {
                _playerRb.simulated = true;
            }
            
            // 애니메이션 재개
            if (animator != null)
            {
                animator.speed = 1f;
            }
        }
    }
    
    #endregion
}