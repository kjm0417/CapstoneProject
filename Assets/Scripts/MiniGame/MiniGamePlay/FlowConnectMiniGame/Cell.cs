using UnityEngine;

/// <summary>
/// 게임 보드의 개별 셀 데이터 구조
/// </summary>
public class Cell
{
    public Vector2Int position;
    public CellType type;
    public int colorId;        // 색상 점인 경우 색상 ID (-1: 없음)
    public int pathColorId;    // 경로가 지나가는 경우 경로의 색상 ID (-1: 없음)
    public bool hasPlayer;     // 플레이어가 현재 위치하는지 여부

    public Cell(Vector2Int pos)
    {
        position = pos;
        type = CellType.Empty;
        colorId = -1;
        pathColorId = -1;
        hasPlayer = false;
    }
}
