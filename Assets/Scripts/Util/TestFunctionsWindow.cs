#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Character Switch Window (+ MiniGame Test)
/// - Tools -> Test Functions -> Character Switch Window 로 열 수 있는 에디터 전용 테스트 창
/// - PlayerManager의 players 리스트(씬의 설정)를 읽어 ID 목록을 만들고,
///   선택한 항목으로 SetActive 또는 EventBus Publish(IPlayerStateReadOnly)를 수행
/// - (추가) MiniGameManager 레지스트리에서 미니게임 ID를 읽어와
///   버튼 클릭으로 EventBus를 통해 팝업 후 실행/중단을 테스트할 수 있음
/// </summary>
public sealed class CharacterSwitchWindow : EditorWindow
{
    // ===== EventBus 기본 키 =====
    private const string DefaultEventName_ActiveCharChanged = "ActiveCharacterChanged";
    private const string DefaultEventName_MiniGameStartById = "MiniGame.StartById";
    private const string DefaultEventName_MiniGameAbort = "MiniGame.Abort";

    // ===== 공통 UI 상태 =====
    private Vector2 _scroll;

    // ===== 캐릭터 전환 테스트 상태 =====
    private string _eventName = DefaultEventName_ActiveCharChanged;
    private int _selectedIndex = -1;
    private List<PlayerData> _managerPlayers = new List<PlayerData>();

    // ===== 미니게임 테스트 상태 =====
    private string _miniGameStartEvent = DefaultEventName_MiniGameStartById;
    private string _miniGameAbortEvent = DefaultEventName_MiniGameAbort;
    private List<string> _miniGameIds = new List<string>();
    private int _miniGameSelectedIndex = -1;
    private string _miniGameIdManual = ""; // 드롭다운에 없을 때 수동 입력용
    private double _lastMiniGameRefreshTime;

    /// <summary>
    /// 메뉴 항목을 추가하고 창을 엽니다.
    /// </summary>
    [MenuItem("Tools/Test Functions/Character Switch Window", priority = 10)]
    private static void Open()
    {
        var win = GetWindow<CharacterSwitchWindow>("Character Switch");
        win.minSize = new Vector2(520, 380);
        win.Show();
    }

    /// <summary>
    /// 창이 켜질 때 매니저 리스트를 갱신하고, 플레이 모드 변화 감시를 시작합니다.
    /// </summary>
    private void OnEnable()
    {
        RefreshFromPlayerManager();
        RefreshMiniGameIdsFromManager();
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    /// <summary>
    /// 창이 꺼질 때 플레이 모드 변화 감시를 해제합니다.
    /// </summary>
    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    /// <summary>
    /// 플레이 모드가 바뀔 때 리스트를 재갱신하고 UI를 갱신합니다.
    /// </summary>
    private void OnPlayModeStateChanged(PlayModeStateChange change)
    {
        RefreshFromPlayerManager();
        RefreshMiniGameIdsFromManager();
        Repaint();
    }

    /// <summary>
    /// 에디터 프레임마다 약간의 자동 리프레시를 걸어줍니다(플레이 중 상태 변화 반영).
    /// </summary>
    private void OnInspectorUpdate()
    {
        // MiniGameManager 레지스트리 변경을 가끔 반영(너무 자주 호출 방지)
        if (EditorApplication.isPlaying)
        {
            if (EditorApplication.timeSinceStartup - _lastMiniGameRefreshTime > 2.0)
            {
                _lastMiniGameRefreshTime = EditorApplication.timeSinceStartup;
                RefreshMiniGameIdsFromManager();
            }
        }
        Repaint();
    }

    /// <summary>
    /// 전체 GUI를 렌더링합니다.
    /// </summary>
    private void OnGUI()
    {
        DrawToolbar();

        if (!EditorApplication.isPlaying)
        {
            EditorGUILayout.HelpBox("Play 모드에서 테스트할 수 있습니다.", MessageType.Info);
            if (GUILayout.Button(" Enter Play Mode", GUILayout.Height(24)))
            {
                EditorApplication.isPlaying = true;
            }
            return;
        }

        if (PlayerManager.Instance == null)
        {
            EditorGUILayout.HelpBox("씬에 PlayerManager가 없습니다. 배치 후 다시 시도하세요.", MessageType.Warning);
            if (GUILayout.Button(" Refresh", GUILayout.Height(22)))
            {
                RefreshFromPlayerManager();
                RefreshMiniGameIdsFromManager();
            }
            return;
        }

        // 스크롤 시작
        _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.ExpandHeight(true));

        // 섹션: 플레이어 목록/액션
        DrawPlayerListSection();
        GUILayout.Space(8);
        DrawPlayerActionsSection();

        GUILayout.Space(16);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Space(6);

        // 섹션: 미니게임 테스트
        DrawMiniGameTestSection();

        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// 상단 툴바(이벤트명/리프레시)를 렌더링합니다.
    /// </summary>
    private void DrawToolbar()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            _eventName = GUILayout.TextField(_eventName, EditorStyles.toolbarTextField, GUILayout.Width(240));
            GUILayout.Label("(ActiveCharacterChanged Event)", EditorStyles.miniLabel, GUILayout.Width(210));

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                RefreshFromPlayerManager();
                RefreshMiniGameIdsFromManager();
            }
        }
    }

    /// <summary>
    /// PlayerManager의 players 리스트를 읽어와 내부 캐시를 갱신합니다.
    /// - private [SerializeField] players를 Reflection으로 읽습니다(런타임 데이터는 변경하지 않음)
    /// </summary>
    private void RefreshFromPlayerManager()
    {
        _managerPlayers.Clear();
        _selectedIndex = -1;

        if (PlayerManager.Instance == null) return;

        try
        {
            var field = typeof(PlayerManager).GetField("players", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                Debug.LogError("[CharacterSwitchWindow] PlayerManager.players 필드를 찾을 수 없습니다.");
                return;
            }

            var list = field.GetValue(PlayerManager.Instance) as IList;
            if (list == null) return;

            foreach (var item in list)
            {
                if (item is PlayerData pd && !string.IsNullOrEmpty(pd.Id))
                {
                    _managerPlayers.Add(pd);
                }
            }

            // 현재 Active가 있으면 그 인덱스로 포커스
            if (PlayerManager.Instance.Active != null)
            {
                var idx = _managerPlayers.FindIndex(p => p.Id == PlayerManager.Instance.Active.Id);
                _selectedIndex = Mathf.Max(0, idx);
            }
            else if (_managerPlayers.Count > 0)
            {
                _selectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CharacterSwitchWindow] 매니저 리스트 읽기 실패: {ex.Message}");
        }
    }

    /// <summary>
    /// MiniGameManager의 레지스트리에서 게임 ID 목록을 읽어옵니다(가능하면).
    /// - 씬 내 MiniGameManager를 찾아 public List&lt;MiniGameEntry&gt; games 의 id를 수집
    /// - 없으면 드롭다운은 비워지고, 수동 입력으로 동작
    /// </summary>
    private void RefreshMiniGameIdsFromManager()
    {
        _miniGameIds.Clear();
        _miniGameSelectedIndex = -1;

        // 씬에서 MiniGameManager 찾기 (Editor 전용 안전 탐색)
#if UNITY_2023_1_OR_NEWER
        var mgm = UnityEngine.Object.FindFirstObjectByType<MiniGameManager>(FindObjectsInactive.Exclude);
#else
        var mgm = UnityEngine.Object.FindObjectOfType<MiniGameManager>();
#endif
        if (mgm == null) return;

        try
        {
            // 공개 필드 games를 그대로 접근
            var list = mgm.games;
            if (list != null)
            {
                foreach (var e in list)
                {
                    if (e != null && !string.IsNullOrWhiteSpace(e.id))
                        _miniGameIds.Add(e.id);
                }
            }

            if (_miniGameIds.Count > 0)
                _miniGameSelectedIndex = 0;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[CharacterSwitchWindow] MiniGameManager 레지스트리 읽기 실패: {ex.Message}");
        }
    }

    /// <summary>
    /// 매니저에서 읽어온 PlayerData 목록을 리스트로 표시하고 선택을 처리합니다.
    /// </summary>
    private void DrawPlayerListSection()
    {
        EditorGUILayout.LabelField("Registered Players (from PlayerManager)", EditorStyles.boldLabel);

        if (_managerPlayers.Count == 0)
        {
            EditorGUILayout.HelpBox("PlayerManager.players 가 비어 있습니다.", MessageType.Warning);
            return;
        }

        for (int i = 0; i < _managerPlayers.Count; i++)
        {
            var pd = _managerPlayers[i];
            using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
            {
                // 라디오 선택
                bool sel = (i == _selectedIndex);
                bool clicked = GUILayout.Toggle(sel, GUIContent.none, GUILayout.Width(18));
                if (clicked && !sel) _selectedIndex = i;

                // 정보
                GUILayout.Label($"Id: {pd.Id}", GUILayout.Width(200));
                GUILayout.Label($"Speed: {pd.moveSpeed}", GUILayout.Width(100));
                GUILayout.FlexibleSpace();
                EditorGUILayout.ObjectField(pd, typeof(PlayerData), false, GUILayout.Width(240));
            }
        }
    }

    /// <summary>
    /// 선택 항목에 대해 SetActive 및 EventBus Publish(상태)를 수행하는 액션 버튼들을 렌더링합니다.
    /// </summary>
    private void DrawPlayerActionsSection()
    {
        if (_selectedIndex < 0 || _selectedIndex >= _managerPlayers.Count) return;

        var pd = _managerPlayers[_selectedIndex];

        using (new EditorGUILayout.VerticalScope(GUI.skin.box))
        {
            EditorGUILayout.LabelField("Character Actions", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("SetActive via Manager", GUILayout.Height(26)))
                {
                    DoManagerSetActive(pd.Id);
                }

                if (GUILayout.Button("Publish State via EventBus", GUILayout.Height(26)))
                {
                    DoPublishState(pd.Id);
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Re-Select Active", GUILayout.Height(22)))
                {
                    // Active 기준으로 라디오 인덱스 동기화
                    var a = PlayerManager.Instance.Active;
                    if (a != null)
                    {
                        var idx = _managerPlayers.FindIndex(x => x.Id == a.Id);
                        if (idx >= 0) _selectedIndex = idx;
                    }
                }

                if (GUILayout.Button("Log Active Snapshot", GUILayout.Height(22)))
                {
                    LogActiveSnapshot();
                }
            }
        }
    }

    /// <summary>
    /// MiniGame 테스트 섹션 UI 렌더링 및 동작
    /// - 드롭다운에서 ID 선택(없으면 수동 입력)
    /// - Start: EventBus.Publish&lt;string&gt;("MiniGame.StartById", gameId)
    /// - Abort: EventBus.Publish("MiniGame.Abort")
    /// </summary>
    private void DrawMiniGameTestSection()
    {
        using (new EditorGUILayout.VerticalScope(GUI.skin.box))
        {
            EditorGUILayout.LabelField("MiniGame Test (EventBus)", EditorStyles.boldLabel);

            // 이벤트 키 입력(변경 가능)
            using (new EditorGUILayout.HorizontalScope())
            {
                _miniGameStartEvent = EditorGUILayout.TextField("Start Event", _miniGameStartEvent);
                _miniGameAbortEvent = EditorGUILayout.TextField("Abort Event", _miniGameAbortEvent);
            }

            // ID 선택(드롭다운 + 수동 입력 보조)
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Game Id", GUILayout.Width(64));

                if (_miniGameIds.Count > 0)
                {
                    _miniGameSelectedIndex = EditorGUILayout.Popup(_miniGameSelectedIndex, _miniGameIds.ToArray());
                    GUILayout.Space(8);
                    _miniGameIdManual = EditorGUILayout.TextField("(Manual Override)", _miniGameIdManual);
                }
                else
                {
                    EditorGUILayout.HelpBox("MiniGameManager의 레지스트리를 찾지 못했습니다. 아래에 ID를 직접 입력하세요.", MessageType.None);
                    _miniGameIdManual = EditorGUILayout.TextField("Manual Id", _miniGameIdManual);
                }
            }

            GUILayout.Space(4);

            // 액션 버튼
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Start MiniGame (Show -> Start)", GUILayout.Height(28)))
                {
                    string idToStart = ResolveMiniGameIdToStart();
                    if (!string.IsNullOrWhiteSpace(idToStart))
                    {
                        EventBus.Publish<string>(_miniGameStartEvent, idToStart);
                        Debug.Log($"[CharacterSwitchWindow] Publish StartMiniGame: {_miniGameStartEvent}('{idToStart}')");
                    }
                    else
                    {
                        Debug.LogWarning("[CharacterSwitchWindow] 시작할 MiniGame Id가 비어 있습니다.");
                    }
                }

                if (GUILayout.Button("Abort", GUILayout.Height(28), GUILayout.Width(120)))
                {
                    EventBus.Publish(_miniGameAbortEvent);
                    Debug.Log($"[CharacterSwitchWindow] Publish AbortMiniGame: {_miniGameAbortEvent}");
                }

                if (GUILayout.Button(" Refresh IDs", GUILayout.Height(28), GUILayout.Width(120)))
                {
                    RefreshMiniGameIdsFromManager();
                }
            }

            // 도움말
            EditorGUILayout.HelpBox(
                "Start 버튼 -> EventBus로 MiniGame.StartById(gameId) 발행 -> MiniGameManager가 해당 프리팹을 스폰하고 " +
                "PopupSlideWorld.Show()가 끝난 뒤 Init/StartGame을 호출합니다. Abort는 즉시 중단 이벤트를 발행합니다.",
                MessageType.Info
            );
        }
    }

    /// <summary>
    /// 드롭다운/수동 입력 상태를 바탕으로 실제로 시작할 gameId를 결정합니다.
    /// </summary>
    private string ResolveMiniGameIdToStart()
    {
        string id = null;
        if (_miniGameIds.Count > 0 && _miniGameSelectedIndex >= 0 && _miniGameSelectedIndex < _miniGameIds.Count)
            id = _miniGameIds[_miniGameSelectedIndex];

        // 수동 입력이 있으면 우선
        if (!string.IsNullOrWhiteSpace(_miniGameIdManual))
            id = _miniGameIdManual.Trim();

        return id;
    }

    /// <summary>
    /// PlayerManager.SetActive(id)를 호출하여 정식 전환을 수행합니다.
    /// </summary>
    private void DoManagerSetActive(string id)
    {
        if (!EditorApplication.isPlaying || PlayerManager.Instance == null) return;

        PlayerManager.Instance.SetActive(id);
        Debug.Log($"[CharacterSwitchWindow] SetActive(\"{id}\") 호출");
    }

    /// <summary>
    /// 주어진 id의 상태를 PlayerManager에서 찾아 EventBus로 발행합니다(IPlayerStateReadOnly).
    /// </summary>
    private void DoPublishState(string id)
    {
        if (!EditorApplication.isPlaying || PlayerManager.Instance == null) return;

        var st = PlayerManager.Instance.Get(id);
        EventBus.Publish<IPlayerStateReadOnly>(_eventName, st);
        Debug.Log($"[CharacterSwitchWindow] Publish State({_eventName}): id={id}, state={(st == null ? "NULL" : "OK")}");
    }

    /// <summary>
    /// 현재 Active의 상태/데이터 스냅샷을 콘솔에 로그로 남깁니다.
    /// </summary>
    private void LogActiveSnapshot()
    {
        if (PlayerManager.Instance == null)
        {
            Debug.LogWarning("[CharacterSwitchWindow] PlayerManager.Instance 가 없습니다.");
            return;
        }

        var st = PlayerManager.Instance.Active;
        var id = st?.Id ?? "(null)";
        var data = PlayerManager.Instance.GetData(st?.Id);
        var speed = data?.moveSpeed.ToString() ?? "(n/a)";
        var anim = data?.animatorController ? data.animatorController.name : "(none)";

        Debug.Log($"[CharacterSwitchWindow] Active Snapshot -> Id={id}, HP={st?.CurrentHP}/{st?.MaxHP}, Stress={st?.Stress}, Conc={st?.Concentration}, Speed={speed}, Anim={anim}");
    }
}
#endif
