using UnityEngine;

/// <summary>
/// 플레이어의 현재 상태를 추적하는 데이터 구조
/// </summary>
public class FlowPlayer
{
    public Vector2Int position;             // 현재 그리드 위치
    public int selectedColorId;             // 현재 선택한 색상 (-1: 선택 안함)
    public bool isCarryingColor;            // 색상을 들고 있는지 여부
    public Vector2Int pickupStartPos;       // 색상을 집은 위치 (같은 위치에서 다시 놓는 것 방지)

    public FlowPlayer()
    {
        position = Vector2Int.zero;
        selectedColorId = -1;
        isCarryingColor = false;
        pickupStartPos = Vector2Int.zero;
    }
}
