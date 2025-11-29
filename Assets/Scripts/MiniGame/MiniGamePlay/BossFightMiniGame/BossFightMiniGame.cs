using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 시간 기반 페이즈 전환 보스전 미니게임.
/// 플레이어는 공격을 피하며 제한 시간 동안 생존해야 합니다.
/// </summary>
public class BossFightMiniGame : MiniGameBase
{
    [Header("Boss Fight Settings")]
    [SerializeField] private BossPhase[] phases; // 페이즈 배열
    [SerializeField] private Transform attackSpawnParent; // 공격 오브젝트 부모

    [Header("Player")]
    [SerializeField] private Transform player; // 플레이어 Transform
    [SerializeField] private float playerMoveSpeed = 10f; // 플레이어 이동 속도
    [SerializeField] private RectTransform playArea; // 플레이 영역 (이동 제한)

    [Header("UI")]
    [SerializeField] private Text phaseText; // 페이즈 표시 텍스트
    [SerializeField] private Image backgroundImage; // 배경 이미지 (색상 변경용)

    // 내부 상태
    private int currentPhaseIndex = -1;
    private float phaseStartTime;
    private readonly Dictionary<BossAttackPattern, float> patternLastSpawnTime = new Dictionary<BossAttackPattern, float>();
    private readonly List<Coroutine> activeCoroutines = new List<Coroutine>();

    /// <summary>
    /// 게임 시작 시 초기화
    /// </summary>
    protected override void OnStartHook()
    {
        base.OnStartHook();

        // 플레이어 자동 탐색
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        // 공격 스폰 부모 생성
        if (attackSpawnParent == null)
        {
            GameObject spawnParent = new GameObject("AttackSpawnParent");
            spawnParent.transform.SetParent(transform);
            attackSpawnParent = spawnParent.transform;
        }

        // 페이즈 검증
        if (phases == null || phases.Length == 0)
        {
            Debug.LogError("[BossFightMiniGame] 페이즈가 설정되지 않았습니다.");
            Abort("NO_PHASES");
            return;
        }

        // 첫 페이즈 시작
        StartPhase(0);
    }

    /// <summary>
    /// 매 프레임 업데이트
    /// </summary>
    protected override void OnUpdateHook(float deltaTime)
    {
        base.OnUpdateHook(deltaTime);

        if (!IsRunning)
            return;

        // 플레이어 이동 처리
        HandlePlayerMovement(deltaTime);

        // 페이즈 전환 체크
        CheckPhaseTransition();

        // 공격 패턴 스폰
        SpawnAttackPatterns();
    }

    /// <summary>
    /// 플레이어 이동 처리 (WASD)
    /// </summary>
    private void HandlePlayerMovement(float deltaTime)
    {
        if (player == null)
            return;

        Vector3 movement = Vector3.zero;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            movement.y += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            movement.y -= 1f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            movement.x -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            movement.x += 1f;

        if (movement.magnitude > 0f)
        {
            movement.Normalize();
            player.Translate(movement * playerMoveSpeed * deltaTime, Space.World);

            // 플레이 영역 제한
            ClampPlayerPosition();
        }
    }

    /// <summary>
    /// 플레이어 위치를 플레이 영역 내로 제한
    /// </summary>
    private void ClampPlayerPosition()
    {
        if (player == null || playArea == null)
            return;

        Vector3 pos = player.localPosition;
        Rect rect = playArea.rect;

        pos.x = Mathf.Clamp(pos.x, rect.xMin, rect.xMax);
        pos.y = Mathf.Clamp(pos.y, rect.yMin, rect.yMax);

        player.localPosition = pos;
    }

    /// <summary>
    /// 페이즈 전환 체크
    /// </summary>
    private void CheckPhaseTransition()
    {
        if (currentPhaseIndex < 0 || currentPhaseIndex >= phases.Length)
            return;

        BossPhase currentPhase = phases[currentPhaseIndex];
        float phaseElapsed = Time.time - phaseStartTime;

        // 페이즈 시간 종료 시 다음 페이즈로
        if (phaseElapsed >= currentPhase.duration)
        {
            int nextPhaseIndex = currentPhaseIndex + 1;
            if (nextPhaseIndex < phases.Length)
            {
                StartPhase(nextPhaseIndex);
            }
        }
    }

    /// <summary>
    /// 페이즈 시작
    /// </summary>
    private void StartPhase(int phaseIndex)
    {
        if (phaseIndex < 0 || phaseIndex >= phases.Length)
            return;

        currentPhaseIndex = phaseIndex;
        phaseStartTime = Time.time;
        patternLastSpawnTime.Clear();

        BossPhase phase = phases[phaseIndex];

        // UI 업데이트
        if (phaseText != null && !string.IsNullOrEmpty(phase.phaseStartText))
        {
            phaseText.text = phase.phaseStartText;
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = phase.phaseColor;
        }

        Debug.Log($"[BossFightMiniGame] {phase.phaseName} 시작!");
    }

    /// <summary>
    /// 공격 패턴 스폰
    /// </summary>
    private void SpawnAttackPatterns()
    {
        if (currentPhaseIndex < 0 || currentPhaseIndex >= phases.Length)
            return;

        BossPhase currentPhase = phases[currentPhaseIndex];
        if (currentPhase.attackPatterns == null)
            return;

        foreach (BossAttackPattern pattern in currentPhase.attackPatterns)
        {
            if (pattern == null || pattern.attackPrefab == null)
                continue;

            // 마지막 스폰 시간 체크
            if (!patternLastSpawnTime.ContainsKey(pattern))
            {
                patternLastSpawnTime[pattern] = Time.time;
            }

            float timeSinceLastSpawn = Time.time - patternLastSpawnTime[pattern];
            if (timeSinceLastSpawn >= pattern.spawnInterval)
            {
                SpawnAttack(pattern);
                patternLastSpawnTime[pattern] = Time.time;
            }
        }
    }

    /// <summary>
    /// 공격 오브젝트 스폰
    /// </summary>
    private void SpawnAttack(BossAttackPattern pattern)
    {
        // 원형/나선 패턴은 특수 처리
        if (pattern.patternType == AttackPatternType.CircularPattern)
        {
            SpawnCircularPattern(pattern);
            return;
        }

        if (pattern.patternType == AttackPatternType.SpiralPattern)
        {
            SpawnSpiralPattern(pattern);
            return;
        }

        // 일반 패턴
        for (int i = 0; i < pattern.spawnCount; i++)
        {
            Vector3 spawnPos = GetSpawnPosition(pattern.patternType, i, pattern.spawnCount);
            Vector2 direction = GetAttackDirection(pattern.patternType, spawnPos, pattern.lineAngle);

            GameObject attackObj = Instantiate(pattern.attackPrefab, spawnPos, Quaternion.identity, attackSpawnParent);
            BossAttackObject attack = attackObj.GetComponent<BossAttackObject>();

            if (attack == null)
            {
                attack = attackObj.AddComponent<BossAttackObject>();
            }

            attack.Initialize(
                this,
                pattern.patternType,
                direction,
                pattern.moveSpeed,
                pattern.lifetime,
                pattern.timePenalty,
                pattern.failsPerfectClear,
                pattern.trackingSpeed,
                pattern.trackingDuration
            );
        }
    }

    /// <summary>
    /// 원형 패턴 스폰 (사방으로 퍼짐)
    /// </summary>
    private void SpawnCircularPattern(BossAttackPattern pattern)
    {
        Vector3 centerPos = playArea != null ? playArea.position : Vector3.zero;
        float angleStep = 360f / pattern.circularCount;

        for (int i = 0; i < pattern.circularCount; i++)
        {
            float angle = angleStep * i;
            Vector2 direction = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            GameObject attackObj = Instantiate(pattern.attackPrefab, centerPos, Quaternion.identity, attackSpawnParent);
            BossAttackObject attack = attackObj.GetComponent<BossAttackObject>();

            if (attack == null)
            {
                attack = attackObj.AddComponent<BossAttackObject>();
            }

            attack.Initialize(
                this,
                AttackPatternType.CircularPattern,
                direction,
                pattern.moveSpeed,
                pattern.lifetime,
                pattern.timePenalty,
                pattern.failsPerfectClear
            );
        }
    }

    /// <summary>
    /// 나선형 패턴 스폰 (회전하며 퍼짐)
    /// </summary>
    private void SpawnSpiralPattern(BossAttackPattern pattern)
    {
        Vector3 centerPos = playArea != null ? playArea.position : Vector3.zero;
        float currentAngle = Time.time * pattern.spiralRotationSpeed;
        float angleStep = 360f / pattern.circularCount;

        for (int i = 0; i < pattern.circularCount; i++)
        {
            float angle = currentAngle + (angleStep * i);
            Vector2 direction = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            GameObject attackObj = Instantiate(pattern.attackPrefab, centerPos, Quaternion.identity, attackSpawnParent);
            BossAttackObject attack = attackObj.GetComponent<BossAttackObject>();

            if (attack == null)
            {
                attack = attackObj.AddComponent<BossAttackObject>();
            }

            attack.Initialize(
                this,
                AttackPatternType.SpiralPattern,
                direction,
                pattern.moveSpeed,
                pattern.lifetime,
                pattern.timePenalty,
                pattern.failsPerfectClear
            );
        }
    }

    /// <summary>
    /// 패턴 타입에 따른 스폰 위치 계산
    /// </summary>
    private Vector3 GetSpawnPosition(AttackPatternType patternType, int index, int totalCount)
    {
        if (playArea == null)
            return Vector3.zero;

        Rect rect = playArea.rect;

        switch (patternType)
        {
            case AttackPatternType.Projectile:
                // 위쪽에서 스폰
                return new Vector3(Random.Range(rect.xMin, rect.xMax), rect.yMax + 50f, 0f);

            case AttackPatternType.GroundAOE:
                // 랜덤 위치
                return new Vector3(Random.Range(rect.xMin, rect.xMax), Random.Range(rect.yMin, rect.yMax), 0f);

            case AttackPatternType.WavePattern:
                // 좌우에서 스폰
                bool fromLeft = Random.value > 0.5f;
                float x = fromLeft ? rect.xMin - 50f : rect.xMax + 50f;
                return new Vector3(x, Random.Range(rect.yMin, rect.yMax), 0f);

            case AttackPatternType.RandomSpawn:
                // 완전 랜덤
                return new Vector3(Random.Range(rect.xMin, rect.xMax), Random.Range(rect.yMin, rect.yMax), 0f);

            case AttackPatternType.TrackingHand:
                // 화면 가장자리에서 랜덤 스폰
                int edge = Random.Range(0, 4); // 0=위, 1=오른쪽, 2=아래, 3=왼쪽
                switch (edge)
                {
                    case 0: return new Vector3(Random.Range(rect.xMin, rect.xMax), rect.yMax + 50f, 0f);
                    case 1: return new Vector3(rect.xMax + 50f, Random.Range(rect.yMin, rect.yMax), 0f);
                    case 2: return new Vector3(Random.Range(rect.xMin, rect.xMax), rect.yMin - 50f, 0f);
                    case 3: return new Vector3(rect.xMin - 50f, Random.Range(rect.yMin, rect.yMax), 0f);
                }
                break;

            case AttackPatternType.StraightLine:
                // 화면 가장자리에서 일정 간격으로 스폰
                float spacing = rect.height / (totalCount + 1);
                return new Vector3(rect.xMin - 50f, rect.yMin + spacing * (index + 1), 0f);

            default:
                return Vector3.zero;
        }

        return Vector3.zero;
    }

    /// <summary>
    /// 패턴 타입에 따른 공격 방향 계산
    /// </summary>
    private Vector2 GetAttackDirection(AttackPatternType patternType, Vector3 spawnPos, float lineAngle = 0f)
    {
        switch (patternType)
        {
            case AttackPatternType.Projectile:
                return Vector2.down; // 아래로

            case AttackPatternType.GroundAOE:
                return Vector2.zero; // 정지

            case AttackPatternType.WavePattern:
                // 플레이어 방향
                if (player != null)
                    return (player.position - spawnPos).normalized;
                return Vector2.right;

            case AttackPatternType.RandomSpawn:
                // 랜덤 방향
                return Random.insideUnitCircle.normalized;

            case AttackPatternType.TrackingHand:
                // 초기 방향 (추적은 Update에서 처리)
                if (player != null)
                    return (player.position - spawnPos).normalized;
                return Vector2.down;

            case AttackPatternType.StraightLine:
                // 지정된 각도로 이동
                float rad = lineAngle * Mathf.Deg2Rad;
                return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            default:
                return Vector2.down;
        }
    }

    /// <summary>
    /// 플레이어가 공격에 맞았을 때 호출
    /// </summary>
    public void OnPlayerHit(float timePenalty, bool failPerfect)
    {
        if (!IsRunning)
            return;

        // 시간 감소
        ReduceTime(timePenalty);

        // 완벽한 클리어 실패
        if (failPerfect)
        {
            FailPerfectClear();
        }

        Debug.Log($"[BossFightMiniGame] 플레이어 피격! 시간 -{timePenalty}초");
    }

    /// <summary>
    /// 클리어 조건: 모든 페이즈를 완료하고 시간이 남아있음
    /// </summary>
    protected override bool CheckClearRealtime()
    {
        if (!IsRunning)
            return false;

        // 마지막 페이즈 완료 체크
        if (currentPhaseIndex >= phases.Length - 1)
        {
            BossPhase lastPhase = phases[currentPhaseIndex];
            float phaseElapsed = Time.time - phaseStartTime;
            return phaseElapsed >= lastPhase.duration;
        }

        return false;
    }

    /// <summary>
    /// 시간 종료 시 실패
    /// </summary>
    protected override void OnTimeOut()
    {
        Complete(false);
    }

    /// <summary>
    /// 정리
    /// </summary>
    protected override void OnCleanupHook()
    {
        base.OnCleanupHook();

        // 모든 공격 오브젝트 제거
        if (attackSpawnParent != null)
        {
            foreach (Transform child in attackSpawnParent)
            {
                Destroy(child.gameObject);
            }
        }

        patternLastSpawnTime.Clear();
        currentPhaseIndex = -1;
    }
}
