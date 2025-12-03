
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �̴ϰ��ӵ��� ID�� ���/�����ϰ�, EventBus�� ���� ����/�ߴ��� �����ϴ� �Ŵ���
/// - ��ϵ� ID�� �������� ã�� �ν��Ͻ�ȭ �� Runner�� ���� �� Popup �ִϸ��̼� �Ϸ� �� ����
/// - �Ϸ�/�ߴ� �� �ν��Ͻ� ����
/// - �̺�Ʈ:
///   * ����: EventBus.Publish<string>("MiniGame.StartById", gameId)
///   * �ߴ�: EventBus.Publish("MiniGame.Abort")
/// </summary>
public class MiniGameManager : SingleTonGeneric<MiniGameManager>
{
    [Serializable]
    public class MiniGameEntry
    {
        [Tooltip("IMiniGame.Id �� �����ؾ� ��")]
        public string id;


        [Tooltip("�ش� �̴ϰ��� ������(��Ʈ�� IMiniGame ���� ������Ʈ�� �־�� ��)")]
        public GameObject prefab;
    }


    [Header("Registry")]
    [Tooltip("������Ʈ �� �̴ϰ��� ������ ��� ���̺�")]
    public List<MiniGameEntry> games = new List<MiniGameEntry>();

    [Header("Runtime Parents")]
    [Tooltip("일반 미니게임용 부모 (MiniGame 타입)")]
    public RectTransform miniGameParent;

    [Tooltip("보스전용 부모 (BossFight 타입)")]
    public RectTransform bossGameParent;

    [Header("Runner / Popup")]
    [Tooltip("�˾� �ִϸ����Ϳ� UpdateGame ƽ�� ����ϴ� �����")]
    public MiniGamePopupRunner runner;

    [Tooltip("�⺻ unscaledTime ��� ����(���ʿ��� �ݿ�)")]
    public bool useUnscaledTime = true;

    // ���� ����
    private readonly Dictionary<string, GameObject> _prefabById = new Dictionary<string, GameObject>();
    private GameObject _currentInstance;
    private IMiniGame _currentGame;

    /// <summary>
    /// ������Ʈ�� �� ���� �� EventBus ����
    /// </summary>
    //Todo : Awake -> Start ���� ����, EventBus ���� 
    private void Awake()
    {
        // ���̺� -> ��
        _prefabById.Clear();
        foreach (var e in games)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.id) || e.prefab == null) continue;
            _prefabById[e.id] = e.prefab;
        }

        if (runner == null) runner = GetComponentInChildren<MiniGamePopupRunner>();
        if (runner != null) runner.useUnscaledTime = useUnscaledTime;

        // EventBus ����
        EventBus.Subscribe<string>("MiniGame.StartById", HandleStartById);
        EventBus.Subscribe("MiniGame.Abort", HandleAbort);
    }

    /// <summary>
    /// �ı� �� EventBus ���� ����
    /// </summary>
    private void OnDestroy()
    {
        EventBus.UnSubscribe<string>("MiniGame.StartById", HandleStartById);
        EventBus.UnSubscribe("MiniGame.Abort", HandleAbort);
    }

    /// <summary>
    /// EventBus: Ư�� ID�� �̴ϰ����� ���� ��û �޾��� �� ó��
    /// - ���� �ν��Ͻ� ����
    /// - ������ ���� �� IMiniGame Ȯ��
    /// - Runner�� ���� �� ShowAndStart()
    /// </summary>
    private void HandleStartById(string gameId)
    {
        if (string.IsNullOrWhiteSpace(gameId))
        {
            Debug.LogWarning("[MiniGameManager] �� gameId ��û");
            return;
        }

        if (!_prefabById.TryGetValue(gameId, out var prefab) || prefab == null)
        {
            Debug.LogWarning($"[MiniGameManager] ��ϵ��� ���� gameId: {gameId}");
            return;
        }

        // ���� ����
        CleanupCurrent();

        // 프리팹에서 IMiniGame 컴포넌트를 먼저 찾아서 타입 확인
        var tempMb = prefab.GetComponent<MonoBehaviour>();
        var tempGame = tempMb as IMiniGame;
        if (tempGame == null)
        {
            tempGame = prefab.GetComponentInChildren<IMiniGame>(true);
        }

        if (tempGame == null)
        {
            Debug.LogError($"[MiniGameManager] 프리팹 '{gameId}' 에 IMiniGame 컴포넌트가 없습니다.");
            return;
        }

        // 게임 타입에 따라 적절한 부모 선택
        RectTransform targetParent = tempGame.EncounterType == GameEncounterType.BossFight 
            ? bossGameParent 
            : miniGameParent;

        if (targetParent == null)
        {
            Debug.LogError($"[MiniGameManager] {tempGame.EncounterType} 타입의 부모가 설정되지 않았습니다.");
            return;
        }

        // �� �ν��Ͻ� ����
        _currentInstance = Instantiate(prefab, targetParent);
        var mb = _currentInstance.GetComponent<MonoBehaviour>();
        var game = mb as IMiniGame;
        if (game == null)
        {
            // ��Ʈ�� ������ �ڽĵ� �߿��� ã��
            game = _currentInstance.GetComponentInChildren<IMiniGame>(true);
            mb = game as MonoBehaviour;
        }

        if (game == null)
        {
            Debug.LogError($"[MiniGameManager] ������ '{gameId}' �� IMiniGame ������ �����ϴ�.");
            Destroy(_currentInstance);
            _currentInstance = null;
            return;
        }

        Debug.Log($"[MiniGameManager] '{gameId}' ({game.EncounterType}) 생성 완료 - 부모: {targetParent.name}");

        // ID ��Ī ����(����)
        if (!string.IsNullOrEmpty(game.Id) && !string.Equals(game.Id, gameId, StringComparison.Ordinal))
        {
            Debug.LogWarning($"[MiniGameManager] ��û ID({gameId})�� ������Ʈ Id({game.Id})�� �ٸ��ϴ�.");
        }

        _currentGame = game;

        // Runner�� �����ϰ� Show -> Start
        if (runner == null)
        {
            Debug.LogError("[MiniGameManager] runner ���� ����");
            return;
        }

        runner.SetMiniGame(mb);
        runner.ShowAndStart();
    }

    /// <summary>
    /// EventBus: �̴ϰ��� ���� �ߴ� ó��
    /// </summary>
    private void HandleAbort()
    {
        if (runner != null) runner.Abort("EVENT_ABORT");
        // �ν��Ͻ��� runner/���ӿ��� ���� ���� Ÿ�� �ΰ�, ���⼱ ��� �ı����� ����.
    }

    /// <summary>
    /// ���� �ν��Ͻ�/���� ����
    /// </summary>
    private void CleanupCurrent()
    {
        if (_currentInstance != null)
        {
            Destroy(_currentInstance);
            _currentInstance = null;
        }
        _currentGame = null;
    }

    /// <summary>
    /// ID �ϳ��� ���� Duration ��ȸ (��ϵ� �������� ������Ʈ���� �ٷ� ����).
    /// - ���� �� true/���� �� false
    public bool TryGetDuration(string gameIdRaw, out float duration)
    {
        duration = 0f;
        var gameId = gameIdRaw;
        if (string.IsNullOrEmpty(gameId)) return false;

        if (!_prefabById.TryGetValue(gameId, out var prefab) || prefab == null)
            return false;


        var miniBase = prefab.GetComponentInChildren<MiniGameBase>(true);
        if (miniBase != null)
        {
            duration = miniBase.Duration; // public float Duration => _duration;
            return true;
        }

        return false;
    }

    /// <summary>
    /// ��ϵ� ��� �̴ϰ����� Duration ���̺�(ID��Duration)�� �����.
    public IReadOnlyDictionary<string, float> GetAllDurations()
    {
        var map = new Dictionary<string, float>();
        foreach (var kv in _prefabById)
        {
            var id = kv.Key;
            var prefab = kv.Value;
            if (prefab == null) continue;

            float d;
            if (TryGetDuration(id, out d))
            {
                map[id] = d;
            }
            else
            {
                Debug.LogWarning($"[MiniGameManager] '{id}' �����տ��� Duration�� ã�� ���߽��ϴ�. (IMiniGame/MiniGameBase Ȯ�� �ʿ�)");
            }
        }
        return map;
    }
}
