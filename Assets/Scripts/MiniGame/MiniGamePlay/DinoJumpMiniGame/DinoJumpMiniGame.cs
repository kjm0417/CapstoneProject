using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Dino Jump 미니게임 (Physics2D 버전)
/// - 플레이어 로직을 MiniGamePlayer2D로 분리(입력/점프/지면판정 전담)
/// - 미니게임 본체는 스폰/장애물/연출/점수만 담당
/// </summary>
[DisallowMultipleComponent]
public class DinoJumpMiniGame : MiniGameBase
{
    [Header("Roots / Mask")]
    [SerializeField] private RectTransform gameRoot;

    [Header("Ground (Physics)")]
    [Tooltip("지면 충돌용 BoxCollider2D (Static)")]
    [SerializeField] private BoxCollider2D groundCollider2D;
    [Tooltip("연출용 바닥 비주얼(선택)")]
    [SerializeField] private RectTransform groundVisual;

    [Header("Player")]
    [Tooltip("미니게임 플레이어 컴포넌트")]
    [SerializeField] private DinoJumpPlayer player;

    [Header("Obstacle (Physics)")]
    [Tooltip("장애물 프리팹 (BoxCollider2D + Rigidbody2D(Kinematic) 필수)")]
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private Transform obstacleParent;
    [SerializeField] private float obstacleSpeed = 6.0f;
    [SerializeField] private Vector2 spawnIntervalRange = new Vector2(0.9f, 1.8f);
    [SerializeField] private Vector2Int batchCountRange = new Vector2Int(1, 2);
    [SerializeField] private Vector2 gapXRange = new Vector2(1.2f, 2.2f);
    [SerializeField] private float spawnStartXWorld = 0f;
    [SerializeField] private int spawnResolveMaxIterations = 4;

    [Header("Obstacle Height (World Units)")]
    [SerializeField] private Vector2 obstacleHeightMeters = new Vector2(0.6f, 2.0f);
    [SerializeField] private bool scaleObstacleVisualY = true;

    [Header("Scrolling Tiles (Visual Only)")]
    [SerializeField] private List<RectTransform> groundTiles = new List<RectTransform>();
    [SerializeField] private List<RectTransform> backTiles = new List<RectTransform>();
    [SerializeField] private float groundScrollPxPerSec = 420f;
    [SerializeField] private float backScrollPxPerSec = 180f;
    [SerializeField] private float tileRecycleWidthPx = 0f;

    [Header("Difficulty & UI")]
    [SerializeField] private float difficultyAccel = 0.0f;
    [SerializeField] private Text scoreText;

    [Header("클리어 조건")]
    [SerializeField] private int targetObstacleCount = 15; // 통과해야 할 장애물 수

    // 내부 상태
    private readonly List<Rigidbody2D> _obstacleBodies = new List<Rigidbody2D>();
    private float _spawnTimer;
    private float _nextSpawnDelay;
    private float _score;
    private float _tileWidthPx;
    private int _passedObstacleCount = 0; // 통과한 장애물 수
    private bool _hasCollided = false; // 장애물 충돌 여부

    // ================= MiniGameBase Hooks =================

    /// <summary>게임 시작: 레퍼런스 보정/플레이어 초기화/스폰·점수 초기화</summary>
    protected override void OnStartHook()
    {
        if (!gameRoot) gameRoot = (RectTransform)transform;
        if (!obstacleParent) obstacleParent = gameRoot;

        if (!player) player = GetComponentInChildren<DinoJumpPlayer>(true);
        if (!player || !groundCollider2D)
        {
            Debug.LogError("[DinoJumpMiniGame] MiniGamePlayer2D/groundCollider2D 누락");
            return;
        }

        // 플레이어 초기화 + 지면 위 배치
        player.Init();
        player.PlaceOnGround(groundCollider2D);

        // 스폰/점수 초기화
        _spawnTimer = 0f;
        _nextSpawnDelay = Random.Range(spawnIntervalRange.x, spawnIntervalRange.y);
        _score = 0f;
        _passedObstacleCount = 0;
        _hasCollided = false;
        if (scoreText) scoreText.text = $"0 / {targetObstacleCount}";

        // 연출용 타일 폭 자동
        _tileWidthPx = tileRecycleWidthPx > 0f
            ? tileRecycleWidthPx
            : (groundTiles.Count > 0 ? Mathf.Max(1f, groundTiles[0].rect.width) : 1280f);

        // 스폰 X 자동(월드)
        if (Mathf.Approximately(spawnStartXWorld, 0f))
            spawnStartXWorld = groundCollider2D.bounds.max.x + 1.0f;

        // 장애물 정리
        ClearAllObstacles();
    }

    /// <summary>프레임 갱신: 입력/연출 스크롤/점수</summary>
    protected override void OnUpdateHook(float dt)
    {
        player?.HandleInput();
        UpdateScrollingVisuals(dt);
        UpdateScore(dt);
    }

    /// <summary>15개 장애물 통과 시 클리어</summary>
    protected override bool CheckClearRealtime()
    {
        return _passedObstacleCount >= targetObstacleCount;
    }

    /// <summary>
    /// 스트레스 증가: 시간 초과 후에만 3초당 +2 증가
    /// </summary>
    protected override void ApplyStressOverTime(float deltaTime)
    {
        // 시간 초과 후에만 스트레스 증가: 3초당 2 = 초당 0.67
        if (RemainingTime <= 0f)
        {
            var playerManager = PlayerManager.Instance;
            if (playerManager == null)
            {
                Debug.LogWarning("[DinoJump] PlayerManager가 없습니다!");
                return;
            }

            float stressAmount = (2f / 3f) * deltaTime;
            playerManager.AddStress(stressAmount);
        }
    }

    /// <summary>정리: 장애물 제거 및 플레이어 물리 중지</summary>
    protected override void OnCleanupHook()
    {
        ClearAllObstacles();
        player?.Cleanup();
    }

    // ================= Unity Physics Tick =================

    /// <summary>물리 틱: 플레이어 점프 처리 + 장애물 이동/스폰</summary>
    private void FixedUpdate()
    {
        if (!IsRunning) return;

        player?.FixedTick();
        UpdateObstaclesFixed();
        UpdateSpawnTimerFixed();
    }

    // ================= Obstacles (Physics) =================

    /// <summary>물리 틱: 장애물 이동/파괴/충돌 실패</summary>
    private void UpdateObstaclesFixed()
    {
        float leftBound = -1300f;
        float playerCenterX = player ? player.Body.position.x : 0f;

        for (int i = _obstacleBodies.Count - 1; i >= 0; i--)
        {
            var rb = _obstacleBodies[i];
            if (!rb)
            {
                _obstacleBodies.RemoveAt(i);
                continue;
            }

            // 왼쪽 고정 속도 (velocity 사용)
            rb.linearVelocity = new Vector2(-obstacleSpeed, 0f);

            // AABB 보조 충돌 체크 (통과 체크보다 먼저 처리)
            var col = rb.GetComponent<Collider2D>();
            if (player && player.Collider && col && player.Collider.bounds.Intersects(col.bounds))
            {
                // 충돌한 장애물 마킹 (통과 카운트 제외용)
                if (!rb.gameObject.GetComponent<ObstacleCollided>())
                {
                    rb.gameObject.AddComponent<ObstacleCollided>();
                    FailPerfectClear(); // 완벽한 클리어 실패
                    Debug.Log("[DinoJump] 장애물 충돌! 완벽한 클리어(◇) 실패 (게임은 계속)");
                }
                // 게임은 계속 진행 - Complete 호출 안 함
            }

            // 플레이어를 지나쳤는지 체크 (통과 카운트)
            // 충돌하지 않았고, 아직 통과 처리 안 된 장애물만 카운트
            if (!rb.gameObject.GetComponent<ObstaclePassed>() && !rb.gameObject.GetComponent<ObstacleCollided>())
            {
                if (rb.position.x < playerCenterX - 0.5f) // 플레이어 뒤로 지나감
                {
                    rb.gameObject.AddComponent<ObstaclePassed>(); // 중복 카운트 방지
                    _passedObstacleCount++;
                    UpdateScoreUI();
                    Debug.Log($"[DinoJump] 장애물 통과! ({_passedObstacleCount}/{targetObstacleCount})");
                }
            }

            // 화면 밖 제거
            if (rb.position.x < leftBound)
            {
                Destroy(rb.gameObject);
                _obstacleBodies.RemoveAt(i);
                continue;
            }
        }
    }

    /// <summary>통과한 장애물 표시용 마커 컴포넌트</summary>
    private class ObstaclePassed : MonoBehaviour { }

    /// <summary>충돌한 장애물 표시용 마커 컴포넌트 (통과 카운트 제외)</summary>
    private class ObstacleCollided : MonoBehaviour { }

    /// <summary>물리 틱: 스폰 타이머 갱신 후 배치 스폰</summary>
    private void UpdateSpawnTimerFixed()
    {
        _spawnTimer += Time.fixedDeltaTime;
        if (_spawnTimer >= _nextSpawnDelay)
        {
            _spawnTimer = 0f;
            _nextSpawnDelay = Random.Range(spawnIntervalRange.x, spawnIntervalRange.y);
            // Debug.Log($"[DinoJump] 장애물 생성 - 다음 생성까지: {_nextSpawnDelay:F2}초");
            SpawnBurst();
        }
    }

    /// <summary>장애물 1~2개를 지면 위 높이로 스폰(높이 랜덤, 겹침 보정)</summary>
    private void SpawnBurst()
    {
        if (!obstaclePrefab || !groundCollider2D) return;

        int count = Random.Range(batchCountRange.x, batchCountRange.y + 1);
        float x = spawnStartXWorld;
        float topY = groundCollider2D.bounds.max.y;

        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(obstaclePrefab, obstacleParent);
            var rb = go.GetComponent<Rigidbody2D>() ?? go.AddComponent<Rigidbody2D>();
            var col = go.GetComponent<BoxCollider2D>() ?? go.AddComponent<BoxCollider2D>();

            // Kinematic 설정
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true;
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // 높이 랜덤 적용
            float h = Random.Range(obstacleHeightMeters.x, obstacleHeightMeters.y);
            Vector2 size = col.size;
            size.y = Mathf.Max(0.05f, h);
            col.size = size;

            // 비주얼 스케일 보정(선택)
            if (scaleObstacleVisualY)
            {
                if (go.transform.childCount > 0)
                {
                    var vis = go.transform.GetChild(0);
                    var s = vis.localScale; s.y = h; vis.localScale = s;
                }
                else
                {
                    var s = go.transform.localScale; s.y = h; go.transform.localScale = s;
                }
            }

            Physics2D.SyncTransforms();

            // 지면에 정확히 붙이기
            float halfH = col.bounds.extents.y;
            float halfW = col.bounds.extents.x;
            Vector2 pos = new Vector2(x, topY + halfH - 0.005f);
            rb.position = pos;
            Physics2D.SyncTransforms();

            // 겹침 체크 및 보정 (장애물 레이어만 체크)
            int obstacleLayer = LayerMask.GetMask("Obstacle");
            if (obstacleLayer == 0) obstacleLayer = ~0; // Obstacle 레이어가 없으면 모든 레이어
            
            bool hasOverlap = false;
            for (int iter = 0; iter < spawnResolveMaxIterations; iter++)
            {
                // 현재 위치에서 장애물과의 겹침만 체크
                Collider2D overlap = Physics2D.OverlapBox(
                    col.bounds.center, 
                    col.bounds.size * 0.9f, 
                    0f,
                    obstacleLayer
                );

                // 자기 자신이 아닌 다른 장애물과 겹치는지 확인
                if (overlap != null && overlap != col)
                {
                    hasOverlap = true;
                    // 겹친 장애물의 오른쪽으로 이동
                    float overlapRight = overlap.bounds.max.x;
                    pos.x = overlapRight + halfW + 0.2f; // 여유 공간
                    rb.position = pos;
                    Physics2D.SyncTransforms();
                }
                else
                {
                    hasOverlap = false;
                    break;
                }
            }

            // 겹침이 해결되지 않으면 생성 취소
            if (hasOverlap)
            {
                Debug.LogWarning("[DinoJump] 장애물 겹침 해결 실패 - 생성 취소");
                Destroy(go);
                continue;
            }

            // 이동 시작 (velocity 사용)
            rb.linearVelocity = new Vector2(-obstacleSpeed, 0f);
            _obstacleBodies.Add(rb);

            // 같은 배치 다음 X 간격 (현재 장애물 너비 고려)
            x = pos.x + halfW + Random.Range(gapXRange.x, gapXRange.y);
        }
    }

    /// <summary>모든 장애물 제거</summary>
    private void ClearAllObstacles()
    {
        for (int i = 0; i < _obstacleBodies.Count; i++)
        {
            if (_obstacleBodies[i])
                Destroy(_obstacleBodies[i].gameObject);
        }
        _obstacleBodies.Clear();
    }


    /// <summary>연출용 타일 스크롤(충돌과 무관)</summary>
    private void UpdateScrollingVisuals(float dt)
    {
        if (groundTiles.Count == 0 && backTiles.Count == 0) return;

        float g = groundScrollPxPerSec + difficultyAccel * (Time.timeSinceLevelLoad * 0.02f);
        float b = Mathf.Max(0f, backScrollPxPerSec);

        float w = tileRecycleWidthPx > 0f
            ? tileRecycleWidthPx
            : (groundTiles.Count > 0 ? Mathf.Max(1f, groundTiles[0].rect.width) : 1280f);

        for (int i = 0; i < groundTiles.Count; i++)
        {
            var t = groundTiles[i];
            if (!t) continue;

            t.localPosition += new Vector3(-g * dt, 0f, 0f);

            float rightEdgeX = t.localPosition.x + w * (1f - t.pivot.x);
            if (rightEdgeX <= -w * 0.5f)
                t.localPosition += new Vector3(w * groundTiles.Count, 0f, 0f);
        }

        for (int i = 0; i < backTiles.Count; i++)
        {
            var t = backTiles[i];
            if (!t) continue;

            t.localPosition += new Vector3(-b * dt, 0f, 0f);

            float rightEdgeX = t.localPosition.x + w * (1f - t.pivot.x);
            if (rightEdgeX <= -w * 0.5f)
                t.localPosition += new Vector3(w * backTiles.Count, 0f, 0f);
        }
    }

    /// <summary>생존 시간 점수</summary>
    private void UpdateScore(float dt)
    {
        _score += dt * 10f;
        UpdateScoreUI();
    }

    /// <summary>점수 UI 업데이트</summary>
    private void UpdateScoreUI()
    {
        if (scoreText)
        {
            scoreText.text = $"{_passedObstacleCount} / {targetObstacleCount}";
        }
    }
}
