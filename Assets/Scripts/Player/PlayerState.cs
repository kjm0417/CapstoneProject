using UnityEngine;


// 런타임에서 변하는 플레이어 상태 값 저장용 클래스
// - PlayerData(초기값)로부터 생성
// - 각 캐릭터 id 별로 PlayerManager가 관리
[System.Serializable]
public class PlayerState
{
    public string id;          // PlayerData.id 참조
    public int currentHP;      // 현재 체력
    public int stress;         // 현재 스트레스
    public int concentration;  // 현재 집중력

    // 필요하면: 불안감, 멘탈, 스태미나 등도 추가 가능
    // public int anxiety;
    // public int mental;
    // public int stamina;

    /// PlayerData의 초기값을 받아 상태를 생성
    public PlayerState(string id, PlayerData baseData)
    {
        this.id = id;

        if (baseData != null)
        {
            currentHP = Mathf.Max(1, baseData.maxHP);
            stress = Mathf.Clamp(baseData.stress, 0, 100);
            concentration = Mathf.Clamp(baseData.concentration, 0, 100);
        }
        else
        {
            // fallback 값
            currentHP = 10;
            stress = 0;
            concentration = 50;
        }
    }

    /// <summary>
    /// HP를 수정하고 최소/최대 범위를 보정
    /// </summary>
    public void ModifyHP(int delta, int maxHP)
    {
        currentHP = Mathf.Clamp(currentHP + delta, 0, Mathf.Max(1, maxHP));
    }

    public void SetStress(int value)
    {
        stress = Mathf.Clamp(value, 0, 100);
    }

    public void SetConcentration(int value)
    {
        concentration = Mathf.Clamp(value, 0, 100);
    }
}
