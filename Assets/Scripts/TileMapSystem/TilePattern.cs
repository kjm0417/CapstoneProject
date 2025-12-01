using System.Collections.Generic;
using UnityEditor.U2D.Aseprite;
using UnityEngine;


[System.Serializable]
public class TilePattern
{
    public string patternName = "DefaultPattern";
    public List<TileMapData> tileMapDatas = new List<TileMapData>(); // TileMapData 단위로 분리
    public List<ObjectInfo> objects = new List<ObjectInfo>();
}
