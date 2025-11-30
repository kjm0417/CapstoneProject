using UnityEngine;

[SerializeField]
public class ObjectInfo
{
    public int x;
    public int y;
    public int id;

    public ObjectInfo(int x, int y, int id)
    {
        this.x = x;
        this.y = y;
        this.id = id;
    }
}
