using UnityEngine;

/// <summary>
/// Flow Connect 그리드의 개별 셀 타입
/// </summary>
public enum CellType
{
    Empty,
    Wall,
    ColorPoint,
    Path
}

/// <summary>
/// Flow Connect 그리드 셀 데이터
/// </summary>
public class FlowConnectCell
{
    public CellType type;
    public Color color;
    public Vector2Int gridPos;
    public bool isStartPoint;
    public int colorIndex;

    public FlowConnectCell(Vector2Int pos)
    {
        gridPos = pos;
        type = CellType.Empty;
        color = Color.white;
        isStartPoint = false;
        colorIndex = -1;
    }
}
