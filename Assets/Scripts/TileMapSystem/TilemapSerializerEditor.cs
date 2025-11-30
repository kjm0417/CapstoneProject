#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileMapSaveLoad))]
public class TilemapSerializerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TileMapSaveLoad tool = (TileMapSaveLoad)target;

        if (tool != null && GUILayout.Button("Save Tilemap to JSON"))
        {
            tool.Save();
        }

        //if (tool != null && GUILayout.Button("Load Tilemap from JSON"))
        //{
        //    tool.Load();
        //}
    }
}
#endif