using UnityEngine;


[CreateAssetMenu(fileName = "ObjectData", menuName = "Game/ObjectDataBase")]
public class ObjectDataBase : ScriptableObject
{
    public GameObject[] prefabs;

    public GameObject GetPrefab(int id)
    {
        if (id < 0 || id >= prefabs.Length) return null;
        return prefabs[id];
    }

    public int GetPrefabIndex(string name)
    {
        for (int i = 0; i < prefabs.Length; i++)
        {
            if (prefabs[i].name == name)
                return i;
        }

        return -1;
    }
}
