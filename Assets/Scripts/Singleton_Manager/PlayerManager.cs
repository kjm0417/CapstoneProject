using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// PlayerState를 관리 매니저. 전환과 조회를 책임지는 싱글톤 매니저.
/// - Awake: PlayerData 목록에서 PlayerState(런타임 상태) 생성, 딕 등록
/// - Start: 첫 활성 캐릭터를 지정(SetActive)
/// - SetActive: Active를 교체하고, 이벤트(OnPlayerChanged + EventBus) 발행
/// - Get/Has/GetData: 상태/데이터 조회 유틸
/// </summary>
public sealed class PlayerManager : SingleTonGeneric<PlayerManager>
{
    [Header("Registered Players (ScriptableObjects)")]
    [SerializeField] private List<PlayerData> players = new List<PlayerData>();

    private readonly Dictionary<string, PlayerState> _states = new Dictionary<string, PlayerState>();
    private readonly Dictionary<string, PlayerData> _dataById = new Dictionary<string, PlayerData>();

    // 현재 활성 플레이어 (읽기 전용)
    public IPlayerStateReadOnly Active { get; private set; }

    // 활성 플레이어 전환 시 발생
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
            var state = data.CreateRuntimeState(); // SO 로 State 인스턴스화
            _states[data.Id] = state;
            _dataById[data.Id] = data;
        }
    }

    /// <summary>
    /// 모든 시스템(OnEnable)이 준비된 후 첫 활성 캐릭터를 지정하고 이벤트를 발행합니다.
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
    /// 주어진 id의 PlayerData(원본 데이터)를 반환합니다. 없으면 null.
    /// </summary>
    public PlayerData GetData(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return _dataById.TryGetValue(id, out var data) ? data : null;
    }

    /// <summary>
    /// 활성 캐릭터를 전환하고, 관련 이벤트를 EventBus(IPlayerStateReadOnly)로 발행합니다.
    /// </summary>
    public void SetActive(string id)
    {
        if (!Has(id))
        {
            Debug.LogWarning($"Unknown player id: {id}");
            return;
        }

        Active = _states[id];

        // ���� C# �̺�Ʈ
        OnPlayerChanged?.Invoke(Active);

        // EventBus�� ���� ����(�ٽ� ������: PlayerData �� IPlayerStateReadOnly)
        EventBus.Publish<IPlayerStateReadOnly>(EventNames.ActiveCharacterChanged, Active);
    }

    /// <summary>
    ///Ȱ�� ���� �÷��̾�� �̴� ���� ������ ����
    /// </summary>
    public void ApplyReward(MiniGameReward reward)
    {
        if (Active is not PlayerState state)
        {
            Debug.LogWarning("[PlayerManager] ������ ���� �� �ִ� Ȱ�� �÷��̾ �����ϴ�.");
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
            case RewardType.ConcentrationDown:
                state.SetConcentration(state.Concentration - reward.amount);
                break;
            default:
                Debug.LogWarning($"[PlayerManager] Unsupported reward type: {reward.type}");
                return;
        }

        EventBus.Publish<IPlayerStateReadOnly>(EventNames.PlayerStatsChanged, state);

        //ex) ��� ���� : PlayerManager.Instance.ApplyReward(new MiniGameReward { type = RewardType.StressDown, amount = 10 });
    }

    /// <summary>
    /// ���� ���� ����(�Ŵ����� ���ؼ��� ���¸� �ٲٵ��� ����).
    /// </summary>
    public void DamageActive(int amount)
    {
        if (Active is PlayerState s) s.Damage(amount);
    }

    /// <summary>
    /// 현재 활성 플레이어의 스트레스를 증가시킵니다 (float 값 지원)
    /// </summary>
    public void AddStress(float stressAmount)
    {
        if (Active is not PlayerState state)
        {
            Debug.LogWarning("[PlayerManager] 스트레스를 추가할 수 있는 활성 플레이어가 없습니다.");
            return;
        }

        // float를 int로 반올림하여 적용
        int stressToAdd = Mathf.RoundToInt(stressAmount);
        if (stressToAdd == 0) return;

        state.SetStress(state.Stress + stressToAdd);
        
        // 스탯 변경 이벤트 발행
        EventBus.Publish<IPlayerStateReadOnly>(EventNames.PlayerStatsChanged, state);
    }

    /// <summary>
    /// 현재 활성 플레이어의 집중력을 증가시킵니다 (float 값 지원)
    /// </summary>
    public void AddConcentration(float concentrationAmount)
    {
        if (Active is not PlayerState state)
        {
            Debug.LogWarning("[PlayerManager] 집중력을 추가할 수 있는 활성 플레이어가 없습니다.");
            return;
        }

        int concentrationToAdd = Mathf.RoundToInt(concentrationAmount);
        if (concentrationToAdd == 0) return;

        state.SetConcentration(state.Concentration + concentrationToAdd);
        
        // 스탯 변경 이벤트 발행
        EventBus.Publish<IPlayerStateReadOnly>(EventNames.PlayerStatsChanged, state);
    }
}

