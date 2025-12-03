using UnityEngine;

/// <summary>
/// 발사체 패턴
/// 화면 위에서 아래로 떨어지는 기본 공격
/// </summary>
public class ProjectilePattern : BossPatternBase
{
    [Header("발사체 설정")]
    [SerializeField] private int spawnCount = 3;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private bool randomX = true;

    protected override void ExecutePattern()
    {
        Vector2 playAreaSize = GetPlayAreaSize();
        
        for (int i = 0; i < spawnCount; i++)
        {
            float xPos;
            
            if (randomX)
            {
                xPos = Random.Range(-playAreaSize.x / 2 + 50, playAreaSize.x / 2 - 50);
            }
            else
            {
                float spacing = playAreaSize.x / (spawnCount + 1);
                xPos = -playAreaSize.x / 2 + spacing * (i + 1);
            }
            
            Vector2 spawnPos = new Vector2(xPos, playAreaSize.y / 2 + 50);
            GameObject attackObj = SpawnAttack(spawnPos);
            
            if (attackObj != null)
            {
                // 아래로 이동하는 Rigidbody2D 설정
                Rigidbody2D rb = attackObj.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.down * moveSpeed * 100f;
                }
            }
        }
    }
}
