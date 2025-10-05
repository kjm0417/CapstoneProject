using static UnityEditor.Experimental.GraphView.GraphView;
using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// PlayerState의 단일 소유자. 전환과 조회를 책임지는 싱글톤 매니저.
/// - Awake: PlayerData 목록에서 PlayerState(깊은 복사) 생성, 맵 구성
/// - Start: 첫 활성 캐릭터를 선정(SetActive)
/// - SetActive: Active를 갱신하고, 이벤트(OnPlayerChanged + EventBus) 발행
/// - Get/Has/GetData: 상태/데이터 조회 유틸
/// </summary>
public sealed class PlayerManager : SingleTonGeneric<PlayerManager>
{
    [Header("Registered Players (ScriptableObjects)")]
    [SerializeField] private List<PlayerData> players = new List<PlayerData>();

    private readonly Dictionary<string, PlayerState> _states = new Dictionary<string, PlayerState>();
    private readonly Dictionary<string, PlayerData> _dataById = new Dictionary<string, PlayerData>();

    //현재 활성 플레이어(읽기 전용)
    public IPlayerStateReadOnly Active { get; private set; }

    //활성 플레이어 전환 시 발생
    public event Action<IPlayerStateReadOnly> OnPlayerChanged;

    /// <summary>
    /// PlayerData 리스트로부터 런타임 상태 딕셔너리를 초기화합니다.
    /// (이 단계에서는 이벤트를 발행하지 않습니다)
    /// </summary>
    /// 

    protected override void Awake()
    {
        base.Awake();

        _states.Clear();
        _dataById.Clear();

        foreach (var data in players)
        {
            if (data == null) continue;
            if (string.IsNullOrEmpty(data.Id))
            {
                Debug.LogWarning("PlayerData.Id 가 비어 있습니다.", data);
                continue;
            }
            var state = data.CreateRuntimeState(); // SO → State 깊은복사
            _states[data.Id] = state;
            _dataById[data.Id] = data;
        }
    }

    /// <summary>
    /// 모든 구독자(OnEnable)가 준비된 뒤 첫 활성 캐릭터를 선정하고 이벤트를 발행합니다.
    /// </summary>
    private void Start()
    {
        if (Active == null)
        {
            var first = players.Find(p => p != null && !string.IsNullOrEmpty(p.Id));
            if (first != null)
            {
                SetActive(first.Id);
            }
        }
    }

    /// <summary>
    /// 주어진 id의 상태가 존재하는지 확인합니다.
    /// </summary>
    public bool Has(string id) => !string.IsNullOrEmpty(id) && _states.ContainsKey(id);

    /// <summary>
    /// 주어진 id의 상태(읽기 전용)를 반환합니다. 없으면 null.
    /// </summary>
    public IPlayerStateReadOnly Get(string id)
    {
        return Has(id) ? _states[id] : null;
    }

    /// <summary>
    /// 주어진 id의 PlayerData(정적 데이터)를 반환합니다. 없으면 null.
    /// </summary>
    public PlayerData GetData(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return _dataById.TryGetValue(id, out var data) ? data : null;
    }

    /// <summary>
    /// 활성 캐릭터를 전환하고, 상태 이벤트와 EventBus(IPlayerStateReadOnly)를 발행합니다.
    /// </summary>
    public void SetActive(string id)
    {
        if (!Has(id))
        {
            Debug.LogWarning($"Unknown player id: {id}");
            return;
        }

        Active = _states[id];

        // 로컬 C# 이벤트
        OnPlayerChanged?.Invoke(Active);

        // EventBus로 상태 발행(핵심 변경점: PlayerData → IPlayerStateReadOnly)
        EventBus.Publish<IPlayerStateReadOnly>(EventNames.ActiveCharacterChanged, Active);
    }

    /// <summary>
    ///활동 중인 플레이어에게 미니 게임 보상을 적용
    /// </summary>
    public void ApplyReward(MiniGameReward reward)
    {
        if (Active is not PlayerState state)
        {
            Debug.LogWarning("[PlayerManager] 보상을 받을 수 있는 활성 플레이어가 없습니다.");
            return;
        }

        if (reward.amount == 0)
        {
            return;
        }

        switch (reward.type)
        {
            case RewardType.StressDown:
                state.SetStress(state.Stress - reward.amount);
                break;
            case RewardType.StressUp:
                state.SetStress(state.Stress + reward.amount);
                break;
            case RewardType.ConcentrationUp:
                state.SetConcentration(state.Concentration + reward.amount);
                break;
            case RewardType.cuncentrationDown:
                state.SetConcentration(state.Concentration - reward.amount);
                break;
            default:
                Debug.LogWarning($"[PlayerManager] Unsupported reward type: {reward.type}");
                return;
        }

        EventBus.Publish<IPlayerStateReadOnly>(EventNames.PlayerStatsChanged, state);
    }

    /// <summary>
    /// 상태 변경 헬퍼(매니저를 통해서만 상태를 바꾸도록 유도).
    /// </summary>
    public void DamageActive(int amount)
    {
        if (Active is PlayerState s) s.Damage(amount);
    }
}

