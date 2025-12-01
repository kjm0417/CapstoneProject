using UnityEngine;

[SerializeField]
public class ObjectInfo
{
    public float x;
    public float y;
    public int id;

    public ObjectInfo(float x, float y, int id)
    {
        this.x = x;
        this.y = y;
        this.id = id;
    }
}
