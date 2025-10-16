using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Push Out 미니게임
/// - 중앙 플레이어가 4방향에서 밀려오는 푸셔를 버티면 성공
/// - 제한 시간 동안 아레나 밖으로 밀려나면 실패
/// </summary>
[DisallowMultipleComponent]
public class PushOutMiniGame : MiniGameBase
{
    [Header("Roots")]
    [Tooltip("게임 UI 루트(선택)")]
    [SerializeField] private RectTransform gameRoot;

    [Header("Arena")]
    [Tooltip("아레나 중심")]
    [SerializeField] private Transform arenaCenter;
    [Tooltip("실패 판정 반경(미터)")]
    [SerializeField] private float failRadius = 4f;
    [Tooltip("경고 UI 반경(미터)")]
    [SerializeField] private float warningRadius = 2.8f;
    [Tooltip("플레이어가 경고 영역에 가까워질 때 채워지는 UI (선택)")]
    [SerializeField] private Image warningFill;

    [Header("Player")]
    [Tooltip("미니게임 플레이어")]
    [SerializeField] private PushOutPlayer player;

    [Header("Pushers")]
    [Tooltip("푸셔 프리팹")]
    [SerializeField] private PushOutPusher pusherPrefab;
    [Tooltip("푸셔 부모 트랜스폼(선택)")]
    [SerializeField] private Transform pusherParent;
    [Tooltip("4방향 푸셔 스폰 설정")]
    [SerializeField] private List<PusherLane> lanes = new List<PusherLane>();

    [Header("Difficulty Curves")]
    [Tooltip("시간 진행(0~1)에 따라 스폰 간격에 곱해지는 배율")]
    [SerializeField] private AnimationCurve intervalMultiplierByProgress = AnimationCurve.Linear(0f, 1f, 1f, 0.55f);
    [Tooltip("시간 진행(0~1)에 따라 푸셔 속도에 곱해지는 배율")]
    [SerializeField] private AnimationCurve speedMultiplierByProgress = AnimationCurve.Linear(0f, 1f, 1f, 1.35f);

    private readonly List<PushOutPusher> _alivePushers = new List<PushOutPusher>();
    private readonly List<Collider2D> _staticObstacleColliders = new List<Collider2D>();
    private float _elapsed;
    private float _globalSpawnTimer;
    private int _currentLaneIndex = -1;

    [System.Serializable]
    private class PusherLane
    {
        [Tooltip("식별용 이름")]
        public string laneName = "Lane";
        [Tooltip("스폰 위치")]
        public Transform spawnPoint;
        [Tooltip("푸셔가 이동할 기본 방향(선택). 미지정 시 중심점 방향 자동 계산")]
        public Vector2 direction = Vector2.zero;
        [Tooltip("게임 시작 후 첫 스폰까지 딜레이")]
        public float initialDelay = 0.5f;
        [Tooltip("스폰 간격 범위(초)")]
        public Vector2 intervalRange = new Vector2(1.1f, 1.8f);
        [Tooltip("푸셔 이동 속도 범위")]
        public Vector2 speedRange = new Vector2(3f, 4.5f);
        [Tooltip("플레이어에게 가해질 임펄스 힘")]
        public float impulseStrength = 3f;
        [Tooltip("푸셔 생존 시간(초)")]
        public float lifetime = 4.5f;
        [Tooltip("스폰 시 트리거를 재생할 애니메이터(선택)")]
        public Animator spawnAnimator;
        [Tooltip("스폰 애니메이터에 설정할 트리거 이름(선택)")]
        public string spawnTriggerName = "Spawn";
        [Tooltip("푸셔 스폰 시 호출되는 이벤트(선택)")]
        public UnityEvent onSpawn = new UnityEvent();

        [HideInInspector] public bool hasSpawned;
    }

    // ================= MiniGameBase Hooks =================

    protected override void OnStartHook()
    {
        if (!gameRoot) gameRoot = (RectTransform)transform;
        if (!arenaCenter) arenaCenter = transform;
        if (!pusherParent) pusherParent = transform;
        if (!player) player = GetComponentInChildren<PushOutPlayer>(true);

        if (!player)
        {
            Debug.LogError("[PushOutMiniGame] 플레이어 컴포넌트가 필요합니다.");
            return;
        }

        _elapsed = 0f;
        ClearPushers();

        Vector2 centerPos = GetArenaCenter();
        player.Init(centerPos);
        RefreshStaticObstacleColliders();
        ApplyCollisionIgnores(player.Collider);

        foreach (var lane in lanes)
        {
            if (lane == null) continue;
            lane.hasSpawned = false;
        }

        _currentLaneIndex = SelectRandomLaneIndex(-1);
        _globalSpawnTimer = 0f;
        if (_currentLaneIndex >= 0)
        {
            PusherLane initialLane = lanes[_currentLaneIndex];
            if (initialLane != null)
            {
                _globalSpawnTimer = Mathf.Max(0f, initialLane.initialDelay);
            }
        }

        UpdateWarningUI(0f);
    }

    protected override void OnUpdateHook(float deltaTime)
    {
        _elapsed += deltaTime;
        player?.HandleInput(deltaTime);
        UpdateWarningUI(deltaTime);
        CheckFailCondition();
        PruneDestroyedPushers();

    }

    protected override bool CheckClearOnTimeout() => true;

    protected override void OnCleanupHook()
    {
        ClearPushers();
        player?.Cleanup();
        UpdateWarningUI(0f);
        _currentLaneIndex = -1;
        _globalSpawnTimer = 0f;
    }

    private void FixedUpdate()
    {
        if (!IsRunning) return;

        float dt = Time.fixedDeltaTime;
        player?.FixedTick(dt);
        UpdateLaneSpawns(dt);
    }

    // ================= Lane & Spawning =================

    private void UpdateLaneSpawns(float dt)
    {
        if (lanes == null || lanes.Count == 0) return;

        float progress = Duration > 0f ? Mathf.Clamp01(_elapsed / Duration) : 0f;
        float intervalMultiplier = intervalMultiplierByProgress != null
            ? Mathf.Max(0.15f, intervalMultiplierByProgress.Evaluate(progress))
            : 1f;
        float speedMultiplier = speedMultiplierByProgress != null
            ? Mathf.Max(0.05f, speedMultiplierByProgress.Evaluate(progress))
            : 1f;

        if (_currentLaneIndex < 0 || _currentLaneIndex >= lanes.Count || lanes[_currentLaneIndex] == null)
        {
            _currentLaneIndex = SelectRandomLaneIndex(-1);
            if (_currentLaneIndex < 0) return;
        }

        _globalSpawnTimer -= dt;
        if (_globalSpawnTimer > 0f) return;

        PusherLane laneToSpawn = lanes[_currentLaneIndex];
        if (laneToSpawn == null)
        {
            _currentLaneIndex = SelectRandomLaneIndex(-1);
            return;
        }

        SpawnPusher(laneToSpawn, speedMultiplier);
        laneToSpawn.hasSpawned = true;

        float baseInterval = Random.Range(laneToSpawn.intervalRange.x, laneToSpawn.intervalRange.y);
        _globalSpawnTimer = Mathf.Max(0.1f, baseInterval * intervalMultiplier);

        int previousLaneIndex = _currentLaneIndex;
        _currentLaneIndex = SelectRandomLaneIndex(previousLaneIndex);
        if (_currentLaneIndex >= 0)
        {
            PusherLane nextLane = lanes[_currentLaneIndex];
            if (nextLane != null && !nextLane.hasSpawned)
            {
                _globalSpawnTimer = Mathf.Max(_globalSpawnTimer, Mathf.Max(0f, nextLane.initialDelay));
            }
        }
    }

    private void SpawnPusher(PusherLane lane, float speedMultiplier)
    {
        if (!pusherPrefab)
        {
            Debug.LogWarning("[PushOutMiniGame] 푸셔 프리팹이 지정되지 않았습니다.");
            return;
        }

        Transform spawnPoint = lane.spawnPoint ? lane.spawnPoint : arenaCenter;
        Vector3 spawnPosition = spawnPoint ? spawnPoint.position : GetArenaCenter();
        PushOutPusher instance = Instantiate(pusherPrefab, spawnPosition, Quaternion.identity, pusherParent);

        Vector2 direction = lane.direction.sqrMagnitude > 0.001f
            ? lane.direction.normalized
            : (GetArenaCenter() - (Vector2)spawnPosition).normalized;
        if (direction.sqrMagnitude < 0.001f) direction = Vector2.right;

        float speed = Random.Range(lane.speedRange.x, lane.speedRange.y) * speedMultiplier;
        float lifetime = Mathf.Max(0.5f, lane.lifetime);
        instance.Configure(direction, speed, lane.impulseStrength, lifetime);
        ApplyCollisionIgnores(instance.Collider);

        if (lane.spawnAnimator)
        {
            if (!string.IsNullOrEmpty(lane.spawnTriggerName))
            {
                lane.spawnAnimator.SetTrigger(lane.spawnTriggerName);
            }
            else
            {
                Debug.LogWarning($"[PushOutMiniGame] Lane '{lane.laneName}' 애니메이터에 사용할 트리거 이름이 비어 있습니다.");
            }
        }
        else if (!string.IsNullOrEmpty(lane.spawnTriggerName))
        {
            Debug.LogWarning($"[PushOutMiniGame] Lane '{lane.laneName}'에 트리거 '{lane.spawnTriggerName}'가 지정되었지만 애니메이터가 비어 있습니다.");
        }

        lane.onSpawn?.Invoke();

        _alivePushers.Add(instance);
    }

    private int SelectRandomLaneIndex(int excludeIndex)
    {
        if (lanes == null || lanes.Count == 0) return -1;

        List<int> candidates = new List<int>();
        for (int i = 0; i < lanes.Count; i++)
        {
            if (lanes[i] == null) continue;
            if (excludeIndex >= 0 && lanes.Count > 1 && i == excludeIndex) continue;
            candidates.Add(i);
        }

        if (candidates.Count == 0) return -1;
        int randomIndex = Random.Range(0, candidates.Count);
        return candidates[randomIndex];
    }

    private void ClearPushers()
    {
        for (int i = _alivePushers.Count - 1; i >= 0; i--)
        {
            if (_alivePushers[i])
            {
                Destroy(_alivePushers[i].gameObject);
            }
        }
        _alivePushers.Clear();
    }

    private void PruneDestroyedPushers()
    {
        for (int i = _alivePushers.Count - 1; i >= 0; i--)
        {
            if (!_alivePushers[i])
            {
                _alivePushers.RemoveAt(i);
            }
        }
    }

    private void RefreshStaticObstacleColliders()
    {
        _staticObstacleColliders.Clear();
        if (!gameRoot) return;

        Collider2D[] colliders = gameRoot.GetComponentsInChildren<Collider2D>(true);
        foreach (var col in colliders)
        {
            if (!col || !col.enabled) continue;
            if (player && (col == player.Collider || col.GetComponent<PushOutPlayer>())) continue;
            if (col.GetComponent<PushOutPusher>()) continue;
            if (col.isTrigger) continue;

            Rigidbody2D attached = col.attachedRigidbody;
            if (attached && attached.bodyType != RigidbodyType2D.Static) continue;

            _staticObstacleColliders.Add(col);
        }
    }

    private void ApplyCollisionIgnores(Collider2D actorCollider)
    {
        if (!actorCollider) return;

        foreach (var obstacle in _staticObstacleColliders)
        {
            if (!obstacle) continue;
            Physics2D.IgnoreCollision(actorCollider, obstacle, true);
        }
    }

    // ================= Fail & UI =================

    private void CheckFailCondition()
    {
        if (!player) return;

        Vector2 center = GetArenaCenter();
        float dist = Vector2.Distance(player.Position, center);

        if (dist >= failRadius)
        {
            Complete(false);
        }
    }

    private void UpdateWarningUI(float deltaTime)
    {
        if (!warningFill || !player) return;

        float warnRadius = Mathf.Max(0.01f, warningRadius);
        float fail = Mathf.Max(warnRadius, failRadius);
        float dist = Vector2.Distance(player.Position, GetArenaCenter());
        float normalized = Mathf.InverseLerp(warnRadius, fail, dist);
        warningFill.fillAmount = Mathf.Clamp01(normalized);
    }

    private Vector2 GetArenaCenter()
    {
        return arenaCenter ? (Vector2)arenaCenter.position : (Vector2)transform.position;
    }
}
