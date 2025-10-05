
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 미니게임들을 ID로 등록/관리하고, EventBus를 통해 실행/중단을 지휘하는 매니저
/// - 등록된 ID로 프리팹을 찾아 인스턴스화 → Runner에 주입 → Popup 애니메이션 완료 후 시작
/// - 완료/중단 시 인스턴스 정리
/// - 이벤트:
///   * 시작: EventBus.Publish<string>("MiniGame.StartById", gameId)
///   * 중단: EventBus.Publish("MiniGame.Abort")
/// </summary>
public class MiniGameManager : SingleTonGeneric<MiniGameManager>
{
    [Serializable]
    public class MiniGameEntry
    {
        [Tooltip("IMiniGame.Id 와 동일해야 함")]
        public string id;

        [Tooltip("해당 미니게임 프리팹(루트에 IMiniGame 구현 컴포넌트가 있어야 함)")]
        public GameObject prefab;
    }

    [Header("Registry")]
    [Tooltip("프로젝트 내 미니게임 프리팹 등록 테이블")]
    public List<MiniGameEntry> games = new List<MiniGameEntry>();

    [Header("Runtime Parents")]
    [Tooltip("미니게임을 담을 부모(검은 패널 RectMask2D 아래의 gameRoot 등)")]
    public RectTransform miniGameParent;

    [Header("Runner / Popup")]
    [Tooltip("팝업 애니메이터와 UpdateGame 틱을 담당하는 실행기")]
    public MiniGamePopupRunner runner;

    [Tooltip("기본 unscaledTime 사용 여부(러너에도 반영)")]
    public bool useUnscaledTime = true;

    // 내부 상태
    private readonly Dictionary<string, GameObject> _prefabById = new Dictionary<string, GameObject>();
    private GameObject _currentInstance;
    private IMiniGame _currentGame;

    /// <summary>
    /// 레지스트리 맵 구성 및 EventBus 구독
    /// </summary>
    //Todo : Awake -> Start 변경 고려, EventBus 구독 
    private void Awake()
    {
        // 테이블 -> 맵
        _prefabById.Clear();
        foreach (var e in games)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.id) || e.prefab == null) continue;
            _prefabById[e.id] = e.prefab;
        }

        if (runner == null) runner = GetComponentInChildren<MiniGamePopupRunner>();
        if (runner != null) runner.useUnscaledTime = useUnscaledTime;

        // EventBus 구독
        EventBus.Subscribe<string>("MiniGame.StartById", HandleStartById);
        EventBus.Subscribe("MiniGame.Abort", HandleAbort);
    }

    /// <summary>
    /// 파괴 시 EventBus 구독 해제
    /// </summary>
    private void OnDestroy()
    {
        EventBus.UnSubscribe<string>("MiniGame.StartById", HandleStartById);
        EventBus.UnSubscribe("MiniGame.Abort", HandleAbort);
    }

    /// <summary>
    /// EventBus: 특정 ID의 미니게임을 실행 요청 받았을 때 처리
    /// - 기존 인스턴스 정리
    /// - 프리팹 스폰 및 IMiniGame 확인
    /// - Runner에 연결 후 ShowAndStart()
    /// </summary>
    private void HandleStartById(string gameId)
    {
        if (string.IsNullOrWhiteSpace(gameId))
        {
            Debug.LogWarning("[MiniGameManager] 빈 gameId 요청");
            return;
        }

        if (!_prefabById.TryGetValue(gameId, out var prefab) || prefab == null)
        {
            Debug.LogWarning($"[MiniGameManager] 등록되지 않은 gameId: {gameId}");
            return;
        }

        // 기존 정리
        CleanupCurrent();

        // 새 인스턴스 스폰
        var parent = miniGameParent != null ? miniGameParent : (runner != null ? (RectTransform)runner.transform : (RectTransform)transform);
        _currentInstance = Instantiate(prefab, parent);
        var mb = _currentInstance.GetComponent<MonoBehaviour>();
        var game = mb as IMiniGame;
        if (game == null)
        {
            // 루트에 없으면 자식들 중에서 찾기
            game = _currentInstance.GetComponentInChildren<IMiniGame>(true);
            mb = game as MonoBehaviour;
        }

        if (game == null)
        {
            Debug.LogError($"[MiniGameManager] 프리팹 '{gameId}' 에 IMiniGame 구현이 없습니다.");
            Destroy(_currentInstance);
            _currentInstance = null;
            return;
        }

        // ID 매칭 검증(선택)
        if (!string.IsNullOrEmpty(game.Id) && !string.Equals(game.Id, gameId, StringComparison.Ordinal))
        {
            Debug.LogWarning($"[MiniGameManager] 요청 ID({gameId})와 컴포넌트 Id({game.Id})가 다릅니다.");
        }

        _currentGame = game;

        // Runner에 주입하고 Show -> Start
        if (runner == null)
        {
            Debug.LogError("[MiniGameManager] runner 참조 없음");
            return;
        }

        runner.SetMiniGame(mb);
        runner.ShowAndStart();
    }

    /// <summary>
    /// EventBus: 미니게임 강제 중단 처리
    /// </summary>
    private void HandleAbort()
    {
        if (runner != null) runner.Abort("EVENT_ABORT");
        // 인스턴스는 runner/게임에서 정리 훅을 타게 두고, 여기선 즉시 파괴하진 않음.
    }

    /// <summary>
    /// 현재 인스턴스/참조 정리
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
}

//using System.Collections.Generic;
//using UnityEngine;

//public class MiniGameManager : MonoBehaviour
////EffectManager로 부터 받은 effectId(미니게임 ID)와 매핑하여 해당 미니게임 시작 
//{
//    public static string OnMiniGameStarted = "OnMiniGameStartEvent";

//    private Dictionary<string, MiniGameBase> miniGameMap = new Dictionary<string, MiniGameBase>();

//    private void Awake()
//    {
//        MiniGameBase[] allMiniGames = FindObjectsOfType<MiniGameBase>(true); //비활성화된 오브젝트도 찾음
//        foreach (var game in allMiniGames)
//        {
//            if (!miniGameMap.ContainsKey(game.Id))
//            {
//                miniGameMap.Add(game.Id, game);
//            }
//            else
//            {
//                Debug.LogWarning($"중복된 MiniGame ID 발견: {game.Id}");
//            }
//        }

//    }

//    private void OnEnable()
//    {
//        EventBus.Subscribe<string>(OnMiniGameStarted, StartMiniGameById);
//    }

//    private void OnDisable()
//    {
//        EventBus.UnSubscribe<string>(OnMiniGameStarted, StartMiniGameById);
//    }



//    private void StartMiniGameById(string id)
//    {
//        if (miniGameMap.TryGetValue(id, out var miniGame))
//        {
//            Debug.Log($"미니게임 시작: {miniGame.Id}");
//            miniGame.Init();
//            miniGame.StartGame();
//        }
//        else
//        {
//            Debug.LogWarning($"MiniGame ID {id} 에 해당하는 미니게임 없음");
//        }
//     }
