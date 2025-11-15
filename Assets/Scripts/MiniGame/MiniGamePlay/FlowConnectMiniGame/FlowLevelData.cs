using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 레벨 데이터를 에디터에서 설정 가능하도록 관리하는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "FlowLevel", menuName = "MiniGame/FlowLevelData")]
public class FlowLevelData : ScriptableObject
{
    [Header("그리드 설정")]
    public int gridSize = 15;

    [Header("벽 위치")]
    public List<Vector2Int> walls = new List<Vector2Int>();

    [Header("색상 쌍 데이터")]
    public List<ColorPairData> colorPairs = new List<ColorPairData>();
}

/// <summary>
/// 색상 쌍의 시작점과 끝점 데이터
/// </summary>
[System.Serializable]
public class ColorPairData
{
    public Vector2Int startPos;
    public Vector2Int endPos;
}
