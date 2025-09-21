using static UnityEditor.Experimental.GraphView.GraphView;
using System.Collections.Generic;
using System;
using UnityEngine;

/// PlayerState의 단일 소유자. 전환과 조회를 책임짐.
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

    protected override  void  Awake()
    {

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
            // SO로부터 ‘깊은 복사’된 런타임 상태 생성
            var state = data.CreateRuntimeState();
            _states[data.Id] = state;
            _dataById[data.Id] = data; // 나중에 EventBus로 그대로 쏘기 위함

        }

    }

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

    public bool Has(string id) => !string.IsNullOrEmpty(id) && _states.ContainsKey(id);

    public IPlayerStateReadOnly Get(string id)
    {
        return Has(id) ? _states[id] : null;
    }

    public void SetActive(string id)
    {
        if (!Has(id))
        {
            Debug.LogWarning($"Unknown player id: {id}");
            return;
        }

        Active = _states[id];
        OnPlayerChanged?.Invoke(Active);

        // PlayerController가 구독하고 있는 형식으로 발행
        if (_dataById.TryGetValue(id, out var data))
        {
            EventBus.Publish<PlayerData>(EventNames.ActiveCharacterChanged, data);
        }
    }

    // 상태 변경 헬퍼(매니저를 통해서만 상태를 바꾸도록 유도)
    public void DamageActive(int amount)
    {
        if (Active is PlayerState s) s.Damage(amount);
    }
}