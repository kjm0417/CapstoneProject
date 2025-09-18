using UnityEngine;

//플레이어 기본 베이스 데이터 변경 x

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData")]
public class PlayerData : ScriptableObject
{
    public string id;
    public float moveSpeed = 3.5f;
    public int maxHP = 10;
    public int stress = 15;
    public int concentration = 75;
    //public RuntimeAnimatorController animatorController;
}
