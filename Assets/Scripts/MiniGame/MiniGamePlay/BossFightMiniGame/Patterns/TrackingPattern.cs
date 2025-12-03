using UnityEngine;

/// <summary>
/// 추적 패턴
/// 플레이어를 일정 시간 동안 추적하는 공격
/// </summary>
public class TrackingPattern : BossPatternBase
{
    [Header("추적 설정")]
    [SerializeField] private float trackingSpeed = 3f;
    [SerializeField] private float trackingDuration = 5f;
    [SerializeField] private int spawnCount = 1;

    protected override void ExecutePattern()
    {
        Vector2 playAreaSize = GetPlayAreaSize();
        
        for (int i = 0; i < spawnCount; i++)
        {
            // 화면 가장자리에서 랜덤 스폰
            Vector2 spawnPos = GetRandomEdgePosition(playAreaSize);
            GameObject attackObj = SpawnAttack(spawnPos);
            
            if (attackObj != null)
            {
                BossTrackingAttack tracking = attackObj.GetComponent<BossTrackingAttack>();
                if (tracking == null)
                {
                    tracking = attackObj.AddComponent<BossTrackingAttack>();
                }
                
                tracking.Initialize(player, trackingSpeed, trackingDuration);
            }
        }
    }

    private Vector2 GetRandomEdgePosition(Vector2 playAreaSize)
    {
        int edge = Random.Range(0, 4);
        Vector2 pos = Vector2.zero;
        
        switch (edge)
        {
            case 0: // 위
                pos = new Vector2(Random.Range(-playAreaSize.x / 2, playAreaSize.x / 2), playAreaSize.y / 2 + 50);
                break;
            case 1: // 아래
                pos = new Vector2(Random.Range(-playAreaSize.x / 2, playAreaSize.x / 2), -playAreaSize.y / 2 - 50);
                break;
            case 2: // 왼쪽
                pos = new Vector2(-playAreaSize.x / 2 - 50, Random.Range(-playAreaSize.y / 2, playAreaSize.y / 2));
                break;
            case 3: // 오른쪽
                pos = new Vector2(playAreaSize.x / 2 + 50, Random.Range(-playAreaSize.y / 2, playAreaSize.y / 2));
                break;
        }
        
        return pos;
    }
}

/// <summary>
/// 추적 공격 동작 컴포넌트
/// </summary>
public class BossTrackingAttack : MonoBehaviour
{
    private RectTransform target;
    private RectTransform rectTransform;
    private float speed;
    private float duration;
    private float elapsedTime;
    private bool isTracking = true;

    public void Initialize(RectTransform target, float speed, float duration)
    {
        this.target = target;
        this.speed = speed;
        this.duration = duration;
        rectTransform = GetComponent<RectTransform>();
        elapsedTime = 0f;
    }

    private void Update()
    {
        if (target == null || rectTransform == null) return;

        elapsedTime += Time.deltaTime;

        if (elapsedTime < duration && isTracking)
        {
            // 플레이어 추적
            Vector2 direction = (target.anchoredPosition - rectTransform.anchoredPosition).normalized;
            rectTransform.anchoredPosition += direction * speed * 100f * Time.deltaTime;
        }
        else if (isTracking)
        {
            // 추적 종료 후 직진
            isTracking = false;
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 direction = (target.anchoredPosition - rectTransform.anchoredPosition).normalized;
                rb.linearVelocity = direction * speed * 100f;
            }
        }
    }
}
