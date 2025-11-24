using UnityEngine;

[System.Serializable]
public class TileInfo
{
    public int x;
    public int y;
    public int id;

    public TileInfo(int x, int y, int id)
    {
        this.x = x;
        this.y = y;
        this.id = id;
    }
}