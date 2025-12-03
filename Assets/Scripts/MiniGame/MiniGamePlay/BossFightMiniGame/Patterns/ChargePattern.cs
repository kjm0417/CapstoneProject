using UnityEngine;

/// <summary>
/// X축 일자 돌진 패턴
/// 플레이어의 Y 위치를 기억하고 화면 가장자리에서 일직선으로 돌진
/// </summary>
public class ChargePattern : BossPatternBase
{
    [Header("돌진 설정")]
    [SerializeField] private float chargeSpeed = 15f;
    [SerializeField] private float chargeDelay = 0.5f;
    [SerializeField] private bool alternateDirection = true;
    
    private bool nextFromLeft = true;

    protected override void OnPatternStart()
    {
        Debug.Log("[ChargePattern] 돌진 패턴 시작!");
    }

    protected override void ExecutePattern()
    {
        Vector2 playerPos = GetPlayerPosition();
        Vector2 playAreaSize = GetPlayAreaSize();
        
        Vector2 spawnPos;
        
        if (nextFromLeft)
        {
            spawnPos = new Vector2(-playAreaSize.x / 2 - 100, playerPos.y);
        }
        else
        {
            spawnPos = new Vector2(playAreaSize.x / 2 + 100, playerPos.y);
        }
        
        GameObject attackObj = SpawnAttack(spawnPos);
        
        if (attackObj != null)
        {
            BossChargeAttack charge = attackObj.GetComponent<BossChargeAttack>();
            if (charge != null)
            {
                charge.Initialize(playerPos.y, nextFromLeft);
            }
        }
        
        if (alternateDirection)
        {
            nextFromLeft = !nextFromLeft;
        }
    }

    protected override void CompletePattern()
    {
        Debug.Log("[ChargePattern] 돌진 패턴 완료!");
        base.CompletePattern();
    }
}
