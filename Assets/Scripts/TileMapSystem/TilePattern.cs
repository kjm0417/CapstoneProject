using System.Collections.Generic;
using UnityEditor.U2D.Aseprite;
using UnityEngine;


[System.Serializable]
public class TilePattern
{
    public string patternName = "DefaultPattern";

    //중심 좌표 계산 용도
    public float centerX;
    public float centerY;

    public List<TileMapData> tileMapDatas = new List<TileMapData>(); // TileMapData 단위로 분리
    public List<ObjectInfo> objects = new List<ObjectInfo>();

}
