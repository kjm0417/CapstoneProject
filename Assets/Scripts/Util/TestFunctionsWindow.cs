#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Character Switch Window
/// - Tools ▶ Test Functions ▶ Character Switch Window 로 열 수 있는 에디터 전용 테스트 창
/// - PlayerManager의 players 리스트(씬의 설정)를 읽어 ID 목록을 만들고,
///   선택한 항목으로 SetActive 또는 EventBus Publish(IPlayerStateReadOnly)를 수행
/// - 플레이 중에 캐릭터 전환/배선(EventBus) 상태를 빠르게 검증할 수 있음
/// </summary>
public sealed class CharacterSwitchWindow : EditorWindow
{
    // EventBus 키(프로젝트 상수와 동일한 문자열을 사용하세요)
    private const string DefaultEventName = "ActiveCharacterChanged";

    // UI/상태
    private Vector2 _scroll;
    private string _eventName = DefaultEventName;
    private int _selectedIndex = -1;

    // 매니저에서 읽어온 PlayerData 목록 캐시
    private List<PlayerData> _managerPlayers = new List<PlayerData>();

    /// <summary>
    /// 메뉴 항목을 추가하고 창을 엽니다.
    /// </summary>
    [MenuItem("Tools/Test Functions/Character Switch Window", priority = 10)]
    private static void Open()
    {
        var win = GetWindow<CharacterSwitchWindow>("Character Switch");
        win.minSize = new Vector2(460, 300);
        win.Show();
    }

    /// <summary>
    /// 창이 켜질 때 매니저 리스트를 갱신하고, 플레이 모드 변화 감시를 시작합니다.
    /// </summary>
    private void OnEnable()
    {
        RefreshFromManager();
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
        RefreshFromManager();
        Repaint();
    }

    /// <summary>
    /// 에디터 프레임마다 약간의 자동 리프레시를 걸어줍니다(플레이 중 상태 변화 반영).
    /// </summary>
    private void OnInspectorUpdate()
    {
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
                RefreshFromManager();
            }
            return;
        }

        DrawPlayerList();
        DrawActions();
    }

    /// <summary>
    /// 상단 툴바(이벤트명/리프레시)를 렌더링합니다.
    /// </summary>
    private void DrawToolbar()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            _eventName = GUILayout.TextField(_eventName, EditorStyles.toolbarTextField, GUILayout.Width(240));
            GUILayout.Label("(Event Name)", EditorStyles.miniLabel, GUILayout.Width(90));
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                RefreshFromManager();
            }
        }
    }

    /// <summary>
    /// PlayerManager의 players 리스트를 읽어와 내부 캐시를 갱신합니다.
    /// - private [SerializeField] players를 Reflection으로 읽습니다(런타임 데이터는 변경하지 않음)
    /// </summary>
    private void RefreshFromManager()
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
    /// 매니저에서 읽어온 PlayerData 목록을 리스트로 표시하고 선택을 처리합니다.
    /// </summary>
    private void DrawPlayerList()
    {
        EditorGUILayout.LabelField("Registered Players (from PlayerManager)", EditorStyles.boldLabel);

        if (_managerPlayers.Count == 0)
        {
            EditorGUILayout.HelpBox("PlayerManager.players 가 비어 있습니다.", MessageType.Warning);
            return;
        }

        _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.ExpandHeight(true));
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
                EditorGUILayout.ObjectField(pd, typeof(PlayerData), false, GUILayout.Width(220));
            }
        }
        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// 선택 항목에 대해 SetActive 및 EventBus Publish(상태)를 수행하는 액션 버튼들을 렌더링합니다.
    /// </summary>
    private void DrawActions()
    {
        if (_selectedIndex < 0 || _selectedIndex >= _managerPlayers.Count) return;

        var pd = _managerPlayers[_selectedIndex];

        using (new EditorGUILayout.VerticalScope(GUI.skin.box))
        {
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("SetActive via Manager", GUILayout.Height(28)))
                {
                    DoManagerSetActive(pd.Id);
                }

                if (GUILayout.Button("Publish State via EventBus", GUILayout.Height(28)))
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

        Debug.Log($"[CharacterSwitchWindow] Active Snapshot → Id={id}, HP={st?.CurrentHP}/{st?.MaxHP}, Stress={st?.Stress}, Conc={st?.Concentration}, Speed={speed}, Anim={anim}");
    }
}
#endif
