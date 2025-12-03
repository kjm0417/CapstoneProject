using UnityEngine;

/// <summary>
/// 보스 공격 오브젝트.
/// 플레이어와 충돌 시 데미지를 주고 자동으로 삭제됩니다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class BossAttackObject : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("공격 패턴 타입")]
    public AttackPatternType patternType = AttackPatternType.Projectile;

    [Tooltip("이동 방향 (정규화된 벡터)")]
    public Vector2 moveDirection = Vector2.down;

    [Tooltip("이동 속도")]
    public float moveSpeed = 5f;

    [Tooltip("생존 시간 (초)")]
    public float lifetime = 5f;

    [Tooltip("시간 패널티 (초)")]
    public float timePenalty = 5f;

    [Tooltip("완벽한 클리어 실패 여부")]
    public bool failsPerfectClear = true;

    [Header("Tracking Settings")]
    [Tooltip("추적 속도")]
    public float trackingSpeed = 3f;

    [Tooltip("추적 지속 시간")]
    public float trackingDuration = 3f;

    private float spawnTime;
    private BossFightMiniGame bossGame;
    private Transform playerTransform;
    private bool isTracking;

    /// <summary>
    /// 초기화
    /// </summary>
    public void Initialize(BossFightMiniGame game, AttackPatternType type, Vector2 direction, float speed, float life, float penalty, bool failPerfect, float trackSpeed = 3f, float trackDuration = 3f)
    {
        bossGame = game;
        patternType = type;
        moveDirection = direction.normalized;
        moveSpeed = speed;
        lifetime = life;
        timePenalty = penalty;
        failsPerfectClear = failPerfect;
        trackingSpeed = trackSpeed;
        trackingDuration = trackDuration;
        spawnTime = Time.time;

        // 추적 패턴이면 플레이어 찾기
        if (patternType == AttackPatternType.TrackingHand)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                isTracking = true;
            }
        }
    }

    private void Update()
    {
        float elapsed = Time.time - spawnTime;

        // 패턴별 이동 처리
        switch (patternType)
        {
            case AttackPatternType.TrackingHand:
                UpdateTracking(elapsed);
                break;

            case AttackPatternType.StraightLine:
            case AttackPatternType.Projectile:
            case AttackPatternType.WavePattern:
            case AttackPatternType.RandomSpawn:
                // 일반 이동
                transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
                break;

            case AttackPatternType.GroundAOE:
                // 정지 (이동 안 함)
                break;

            case AttackPatternType.CircularPattern:
            case AttackPatternType.SpiralPattern:
                // 일반 이동 (방향은 스폰 시 설정됨)
                transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
                break;

            case AttackPatternType.CustomBehavior:
                // 커스텀 동작은 프리팹의 다른 스크립트에서 처리
                break;
        }

        // 생존 시간 체크
        if (elapsed >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 플레이어 추적 업데이트
    /// </summary>
    private void UpdateTracking(float elapsed)
    {
        if (!isTracking || playerTransform == null)
        {
            // 추적 실패 시 일반 이동
            transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
            return;
        }

        // 추적 시간 체크
        if (elapsed >= trackingDuration)
        {
            isTracking = false;
            return;
        }

        // 플레이어 방향으로 이동
        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
        transform.Translate(directionToPlayer * trackingSpeed * Time.deltaTime, Space.World);

        // 회전 (플레이어를 바라봄)
        float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어와 충돌 체크
        if (collision.CompareTag("Player"))
        {
            // TODO: 충돌 시 처리 로직 추가 예정
            // 예: 이펙트, 사운드, 애니메이션 등
        }
    }

    private void OnBecameInvisible()
    {
        // 화면 밖으로 나가면 삭제
        Destroy(gameObject);
    }
}
