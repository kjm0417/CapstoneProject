using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FlowConnectLevel", menuName = "MiniGame/FlowConnect/LevelData")]
public class FlowConnectLevelData : ScriptableObject
{
    [Serializable]
    public struct ColorPairData
    {
        public Color color;
        public Vector2Int startPos;
        public Vector2Int endPos;
    }

    [Header("그리드 설정")]
    public int gridSize = 15;

    [Header("벽 위치")]
    public List<Vector2Int> wallPositions = new List<Vector2Int>();

    [Header("색상 쌍")]
    public List<ColorPairData> colorPairs = new List<ColorPairData>();
}
