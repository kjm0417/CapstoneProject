using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 2D 쯔꾸르 스타일 플레이어 컨트롤러
/// - EventBus로 IPlayerStateReadOnly를 수신(= 런타임 상태를 기준으로 캐릭터 전환)
/// - 정적 리소스(속도/애니/스프라이트)는 PlayerManager에서 PlayerData를 찾아 적용
/// - Update: 입력 전처리 및 애니 파라미터 갱신 / FixedUpdate: 물리 이동
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private Rigidbody2D _playerRb;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Data")]
    [Tooltip("게임 시작 시 사용할 기본 PlayerData (없으면 fallback 속도로 시작)")]
    [SerializeField] private PlayerData initialData;

    [Header("Movement")]
    [Tooltip("데이터 없을 때 기본 이동 속도")]
    [SerializeField] private float _fallbackMoveSpeed = 3.5f;
    [Tooltip("대각선 입력 차단 (4방향 고정)")]
    [SerializeField] private bool _blockDiagonal = true;

    [Header("Runtime (ReadOnly)")]
    [SerializeField] private bool _isMoving;
    [SerializeField] private Vector2 _lastMoveDir = Vector2.down;
    [SerializeField] private bool _canMove = true;
    private Vector2 _processedInput;

    // 입력 버퍼
    private Vector2 moveInput;

    // 런타임 상태/정적 데이터 참조
    private IPlayerStateReadOnly _state;
    private PlayerData _appliedData;

    public event Action<PlayerData> OnDataApplied;

    // 애니 파라미터 해시
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int MoveYHash = Animator.StringToHash("MoveY");

    // 스프라이트 캐시(동일 경로 재사용)
    private static readonly Dictionary<string, Sprite[]> _spriteCache = new Dictionary<string, Sprite[]>();

    // 이동 속도 계산(데이터가 없으면 폴백)
    private float Speed => _appliedData != null
        ? Mathf.Max(0.1f, _appliedData.moveSpeed)
        : _fallbackMoveSpeed;

    /// <summary>
    /// 컴포넌트 기본 링크를 자동 세팅합니다.
    /// </summary>
    private void Reset()
    {
        _playerRb = GetComponent<Rigidbody2D>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    /// <summary>
    /// 필수 링크를 보장하고 초기 데이터/스프라이트를 적용합니다.
    /// </summary>
    private void Awake()
    {
        if (_playerRb == null) _playerRb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // 초기엔 PlayerState가 없을 수 있으므로 PlayerData 기반 폴백 적용
        ApplyData(initialData);
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
    /// 입력을 전처리하고 애니메이터 파라미터를 갱신합니다.
    /// </summary>
    private void Update()
    {
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
    /// 물리 틱에서 이동을 수행합니다.
    /// </summary>
    private void FixedUpdate()
    {
        if (!_canMove) return;
        if (_processedInput.sqrMagnitude <= 1e-4f) return;

        Vector2 vel = _processedInput.normalized * Speed;
        _playerRb.MovePosition(_playerRb.position + vel * Time.fixedDeltaTime);
    }

    /// <summary>
    /// PlayerInput Move(UnityEvent)에서 호출: 입력을 버퍼에 저장합니다.
    /// </summary>
    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) moveInput = ctx.ReadValue<Vector2>();
        else if (ctx.canceled) moveInput = Vector2.zero;
    }

    /// <summary>
    /// EventBus로부터 상태를 수신했을 때 호출됩니다.
    /// - 상태는 런타임 수치(HP 등) 기준
    /// - 정적 리소스는 PlayerManager에서 PlayerData를 찾아 적용
    /// </summary>
    private void HandleStateChanged(IPlayerStateReadOnly newState)
    {
        _state = newState;

        // 정적 데이터 조회(속도/애니/스프라이트용)
        var data = PlayerManager.Instance ? PlayerManager.Instance.GetData(_state?.Id) : null;
        ApplyData(data);

        // 필요하다면 _state.OnChanged에 바인딩하여 HP 변화 등을 UI에 반영하는 것도 가능
        // (지금은 컨트롤러 이동/애니만 담당하므로 생략)
    }

    /// <summary>
    /// PlayerData를 적용하고(속도/애니/스프라이트), 필요한 경우 애니를 안전히 리바인딩합니다.
    /// </summary>
    public void ApplyData(PlayerData data)
    {
        _appliedData = data;

        // 애니메이터 컨트롤러 교체
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

        // Resources 경로 기반으로 시작 스프라이트 적용(있을 경우)
        TryApplySpriteFromResources(_appliedData);

        OnDataApplied?.Invoke(_appliedData);
    }

    /// <summary>
    /// PlayerData.resourcesAddress/defaultSpriteName을 사용해 시작 스프라이트를 적용합니다.
    /// </summary>
    private void TryApplySpriteFromResources(PlayerData data)
    {
        if (!spriteRenderer || data == null || string.IsNullOrEmpty(data.resourcesAddress))
            return;

        var sprites = LoadSpritesCached(data.resourcesAddress);
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning($"[PlayerController] Resources에서 스프라이트를 찾지 못했습니다: {data.resourcesAddress}");
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
    /// 데드존 적용: 미세한 입력 값을 0으로 스냅합니다.
    /// </summary>
    private static Vector2 Deadzone(Vector2 v, float dead = 0.2f)
    {
        if (Mathf.Abs(v.x) < dead) v.x = 0f;
        if (Mathf.Abs(v.y) < dead) v.y = 0f;
        return v;
    }

    /// <summary>
    /// 4방향 스냅: 더 큰 축을 우선(-1/0/1)로 고정합니다.
    /// </summary>
    private static Vector2 SnapDir(Vector2 v)
    {
        if (v.sqrMagnitude <= 0f) return Vector2.zero;
        return Mathf.Abs(v.x) >= Mathf.Abs(v.y)
            ? new Vector2(Mathf.Sign(v.x), 0f)
            : new Vector2(0f, Mathf.Sign(v.y));
    }

    /// <summary>
    /// 이동 입력을 종합 처리(데드존→옵션 대각선 차단→스냅)합니다.
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
    /// 외부에서 이동 가능/불가를 토글합니다(컷신/대화 등).
    /// </summary>
    public void SetMovementEnabled(bool enabled) => _canMove = enabled;
}
