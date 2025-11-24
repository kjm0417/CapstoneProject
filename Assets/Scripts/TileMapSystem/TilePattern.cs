using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class TilePattern
{
    public string patternName = "DefaultPattern";
    public List<TileInfo> tiles = new List<TileInfo>();
    public List<ObjectInfo> objects = new List<ObjectInfo>();
}
