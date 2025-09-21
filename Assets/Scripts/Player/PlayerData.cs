using UnityEngine;

//플레이어 기본 베이스 데이터 변경 x

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData")]
public class PlayerData : ScriptableObject
{
    public string id;               //캐릭터 데이터 불러올 때 사용하는 id
    public float moveSpeed = 3.5f;  //스피드
    public int maxHP = 10;          //체력
    public int stress = 15;         //스트레스
    public int concentration = 75;  //집중력
    public RuntimeAnimatorController animatorController;
}
