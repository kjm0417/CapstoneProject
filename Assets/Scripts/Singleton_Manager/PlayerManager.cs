using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// 플레이어 데이터/상태의 "단일 소스" + 이벤트 발행자(Publisher).
/// - PlayerData(불변 베이스 값)는 ScriptableObject로 보유
/// - PlayerState(런타임 변동 값)는 id별로 보존
/// - 캐릭터 교체 시 EventBus로 PlayerData 방송
/// </summary>
public class PlayerManager : SingleTonGeneric<PlayerManager>
{
    [Header("Registered Characters")]
    [Tooltip("게임에서 사용할 PlayerData 목록 (각 data.id는 고유해야 함)")]
    public List<PlayerData> playerDataList = new();

    [Tooltip("게임 시작 시 활성화할 캐릭터 id (비워두면 첫 항목)")]
    public string startCharacterId;

    [Tooltip("Start에서 활성 캐릭터를 이벤트로 즉시 방송")]
    public bool autoPublishOnStart = true;

    // --- 런타임 사전 ---
    private readonly Dictionary<string, PlayerData> _dataById = new();
    private readonly Dictionary<string, PlayerState> _stateById = new();

    // --- 현재 활성 ---
    public string CurrentId { get; private set; }
    public PlayerData CurrentData { get; private set; }
    public PlayerState CurrentState { get; private set; }

    // ===== Unity lifecycle =====
    private void Awake()
    {
        BuildLookup();
    }

    private void Start()
    {
        // 시작 캐릭터 결정
        if (!string.IsNullOrEmpty(startCharacterId) && _dataById.ContainsKey(startCharacterId)) //문자열이 빈어 있는지아닌지 확인
        {
            SetActiveCharacter(startCharacterId);
        }
        else if (playerDataList.Count > 0)
        {
            SetActiveCharacter(playerDataList[0].id);
        }
        else
        {
            Debug.LogWarning("[PlayerManager] playerDataList가 없음.");
        }

        // 선택적으로 즉시 방송 (씬의 구독자들이 초기 데이터 받도록)
        if (autoPublishOnStart && CurrentData != null)
        {
            EventBus.Publish<PlayerData>(EventNames.ActiveCharacterChanged, CurrentData);
        }
    }

    private void OnValidate()
    {
        // 에디터에서 중복 id 경고
        if (playerDataList == null) return;
        var set = new HashSet<string>();
        foreach (var d in playerDataList)
        {
            if (d == null) continue;
            if (string.IsNullOrWhiteSpace(d.id))
            {
                Debug.LogWarning($"[PlayerManager] 빈 id를 가진 PlayerData가 있습니다: {d.name}");
                continue;
            }
            if (!set.Add(d.id))
                Debug.LogWarning($"[PlayerManager] 중복 id 발견: {d.id}");
        }
    }

    // ===== Public API =====

    /// <summary>
    /// 활성 캐릭터 전환. 성공 시 이벤트를 발행합니다.
    /// </summary>
    public void SetActiveCharacter(string id)
    {
        if (!_dataById.TryGetValue(id, out var data))
        {
            Debug.LogWarning($"[PlayerManager] Unknown character id: {id}");
            return;
        }

        CurrentId = id;
        CurrentData = data;

        // 상태 생성 또는 복원
        if (!_stateById.TryGetValue(id, out var state))
        {
            state = new PlayerState(id, data);
            _stateById[id] = state;
        }
        CurrentState = state;

        // TODO: 필요하면 저장(세이브) 훅 추가 (예: 세션 persist)
        // SaveSystem.SaveState(CurrentState);

        // 구독자들에게 방송
        EventBus.Publish<PlayerData>(EventNames.ActiveCharacterChanged, data);
    }

    /// <summary>
    /// (디버그용) 다음 캐릭터로 순환 전환.
    /// </summary>
    [ContextMenu("Cycle To Next Character")]
    public void CycleToNextCharacter()
    {
        if (playerDataList.Count == 0) return;
        int idx = Mathf.Max(0, playerDataList.FindIndex(d => d != null && d.id == CurrentId));
        for (int step = 1; step <= playerDataList.Count; step++)
        {
            int next = (idx + step) % playerDataList.Count;
            var d = playerDataList[next];
            if (d == null) continue;
            SetActiveCharacter(d.id);
            break;
        }
    }

    // ===== 상태 접근/수정 유틸 =====

    public PlayerState GetState(string id)
    {
        _stateById.TryGetValue(id, out var s);
        return s;
    }

    public void ModifyHP(int delta)
    {
        if (CurrentState == null) return;
        int max = CurrentData != null ? Mathf.Max(1, CurrentData.maxHP) : 10;
        CurrentState.currentHP = Mathf.Clamp(CurrentState.currentHP + delta, 0, max);
        // 필요시 상태 변경 이벤트 추가 가능: EventBus.Publish(EventNames.StateChanged, CurrentState);
    }

    public void SetStress(int value)
    {
        if (CurrentState == null) return;
        CurrentState.stress = Mathf.Clamp(value, 0, 100);
    }

    public void SetConcentration(int value)
    {
        if (CurrentState == null) return;
        CurrentState.concentration = Mathf.Clamp(value, 0, 100);
    }

    // ===== Internal =====

    private void BuildLookup()
    {
        _dataById.Clear();
        if (playerDataList == null) return;

        foreach (var data in playerDataList)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.id)) continue;
            if (_dataById.ContainsKey(data.id))
            {
                Debug.LogWarning($"[PlayerManager] 중복 id: {data.id} (마지막 항목으로 덮어씀)");
            }
            _dataById[data.id] = data;
        }
    }
}
