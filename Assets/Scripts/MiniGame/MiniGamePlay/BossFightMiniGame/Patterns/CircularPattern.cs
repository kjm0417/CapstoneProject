using UnityEngine;

/// <summary>
/// 원형 패턴
/// 중앙에서 사방으로 퍼지는 공격
/// </summary>
public class CircularPattern : BossPatternBase
{
    [Header("원형 설정")]
    [SerializeField] private int bulletCount = 8;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Vector2 centerOffset = Vector2.zero;

    protected override void ExecutePattern()
    {
        Vector2 centerPos = centerOffset;
        float angleStep = 360f / bulletCount;
        
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = angleStep * i;
            Vector2 direction = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );
            
            GameObject attackObj = SpawnAttack(centerPos);
            
            if (attackObj != null)
            {
                Rigidbody2D rb = attackObj.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = direction * moveSpeed * 100f;
                }
            }
        }
    }
}
