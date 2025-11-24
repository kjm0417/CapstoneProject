using UnityEngine;

[CreateAssetMenu(fileName = "InteractObjectInfo", menuName = "Scriptable Objects/InteractObjectInfo")]
public class InteractObjectInfo : ScriptableObject
{
    public string ObjectID; //오브젝트 ID

    public string ObjectName; //오브젝트 이름
}
