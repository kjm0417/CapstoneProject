using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;


[CreateAssetMenu(fileName = "ObjectData", menuName = "Game/ObjectDataBase")]
public class ObjectDataBase : ScriptableObject
{

    public GameObject[] prefabs;

    public int GetPrefabCount()
    {
        return prefabs != null ? prefabs.Length : 0;
    }
    public GameObject GetPrefab(int id)
    {
        if (id < 0 || id >= prefabs.Length) return null;
        return prefabs[id];
    }

    public int GetPrefabIndex(string name)
    {
        // 1) "(숫자)" 제거
        string cleaned = Regex.Replace(name, @"\s*\(\d+\)$", "");


        for (int i = 0; i < prefabs.Length; i++)
        {
            if (prefabs[i].name == cleaned)
            {
                return i;
            }
               
        }

        return -1;
    }
}
