using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMapSaveLoad : MonoBehaviour
{
    public static string OnTileMapChange = "OnTileMapChange";

    [Header("Tile")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileDataBase database; 
    [SerializeField] private string SaveFileName = "TilePattern_01";
    [Tooltip("SD")]
    [SerializeField] private string LoadFileName  = "TilePattern_01";

    [Header("Objects")]
    [SerializeField] private Transform objectRoot;   // 여기 자식 오브젝트들을 저장
    [SerializeField] private ObjectDataBase objectDatabase;

    private void OnEnable()
    {
        EventBus.Subscribe<int,DailyRoutine>(OnTileMapChange, Load);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<int,DailyRoutine>(OnTileMapChange, Load);
    }

    public void Save()
    {
        TilePattern pattern = new TilePattern();
        pattern.patternName = SaveFileName;

        SaveTile(pattern);

        SaveObject(pattern);

        string folderPath = Path.Combine(Application.dataPath, "0_TilePatterns");
        string filePath = Path.Combine(folderPath);

        Directory.CreateDirectory(filePath);

        string path = Path.Combine(filePath, SaveFileName + ".json");
        File.WriteAllText(path, JsonConvert.SerializeObject(pattern, Formatting.Indented));

        Debug.Log($"Tilemap 저장 완료, 경로: {filePath}");
    }
    public void Load(int Day, DailyRoutine routine)
    {
        //Application.dataPath -> Assets 폴더
        //0_TilePatterns -> 0_TilePatterns 폴더
        //LoadFileName -> 사용자가 지정한 로드할 파일 이름
        //.json -> json형식의 파일만 

        string folderPath = Path.Combine(Application.dataPath, "0_TilePatterns");

        string filePath = Path.Combine(folderPath, $"{Day.ToString()}_{routine.ToString()}"  + ".json");

        if (!File.Exists(filePath))
        {
            Debug.LogError("TilePattern 파일을 찾을 수 없습니다: " + filePath);
            return;
        }

        string json = File.ReadAllText(filePath);
        TilePattern pattern = JsonConvert.DeserializeObject<TilePattern>(json);

        LoadTile(pattern);

        LoadObject(pattern);

        Debug.Log($"Tilemap 로드 완료, 경로: {filePath}");
    }

    private void SaveTile(TilePattern pattern)
    {
        BoundsInt bounds = tilemap.cellBounds;
        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

        int index = 0;


        for (int y = 0; y < bounds.size.y; y++)
        {
            for (int x = 0; x < bounds.size.x; x++)
            {
                TileBase tile = allTiles[index];
                if (tile != null)
                {
                    int tileID = database.GetTileId(tile);

                    pattern.tiles.Add(new TileInfo(
                        bounds.x + x,
                        bounds.y + y,
                        tileID
                    ));
                }

                index++;
            }
        }
    }

    private void SaveObject(TilePattern pattern)
    {
        foreach (Transform child in objectRoot)
        {
            Vector3Int cell = tilemap.WorldToCell(child.position);
            int objId = objectDatabase.GetPrefabIndex(child.name);

            pattern.objects.Add(new ObjectInfo(cell.x, cell.y, objId));
        }
    }
    private void LoadTile(TilePattern pattern)
    {
        tilemap.ClearAllTiles(); //기존 타일 삭제.

        foreach (TileInfo info in pattern.tiles)
        {
            TileBase tile = database.GetTile(info.id);
            if (tile != null)
            {
                tilemap.SetTile(new Vector3Int(info.x, info.y, 0), tile);
            }
        }
    }

    private void LoadObject(TilePattern pattern)
    {
        foreach (Transform child in objectRoot)
            DestroyImmediate(child.gameObject);

        // 새로운 오브젝트 배치
        foreach (var o in pattern.objects)
        {
            GameObject prefab = objectDatabase.GetPrefab(o.id);
            if (prefab != null)
            {
                Vector3 pos = tilemap.CellToWorld(new Vector3Int(o.x, o.y, 0));
                Instantiate(prefab, pos, Quaternion.identity, objectRoot);
            }
        }

    }
}
