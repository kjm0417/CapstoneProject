using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor.U2D.Aseprite;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using static TileMapSaveLoad;

public class TileMapSaveLoad : MonoBehaviour
{
   // public static string OnTileMapChange = "OnTileMapChange";

    [Header("Tile")]
    [SerializeField] private List<Tilemap> tilemaps;
    [SerializeField] private TileDataBase database; 
    [SerializeField] private string SaveFileName = "TilePattern_01";
    [Tooltip("SD")]
    [SerializeField] private string LoadFileName  = "TilePattern_01";

    [Header("Objects")]
    //[SerializeField] private Transform objectRoot;   // 여기 자식 오브젝트들을 저장
    [SerializeField] private ObjectDataBase objectDatabase;
    [SerializeField] private List<PatternRootMapping> patternMappings;

    private int previousDay = 1;
    private DailyRoutine previousRoutine;

    [System.Serializable]
    public class PatternRootMapping
    {
        public int day;
        public DailyRoutine routine;
        public Transform objectRoot;

        public PatternRootMapping(int day, DailyRoutine routine)
        {
            this.day = day;
            this.routine = routine;
        }
    }

    //private void OnEnable()
    //{
    //   EventBus.Subscribe<int,DailyRoutine>(OnTileMapChange, Load);
    //}

    //private void OnDisable()
    //{
    //   EventBus.UnSubscribe<int,DailyRoutine>(OnTileMapChange, Load);
    //}


    public void Save()
    {
        TilePattern pattern = new TilePattern();

        pattern.patternName = SaveFileName; // 1_ROOM

        string nameWithoutChunk = SaveFileName;
        int chunkIndex = -1;

        if(SaveFileName.Contains("_Chunk"))
        {
            int chunkPos = SaveFileName.LastIndexOf("_Chunk");
            nameWithoutChunk = SaveFileName.Substring(0, chunkPos);

            string chunkPart = SaveFileName.Substring(chunkPos + 6);
            int.TryParse(chunkPart, out chunkIndex);
        }
        
        string[] parts = nameWithoutChunk.Split(new char[] { '_' }, 2);


        if (parts.Length < 2)
        {
            Debug.LogError($"잘못된 패턴 이름 형식: {pattern.patternName}");
            return;
        }

        if (!int.TryParse(parts[0], out int day))
        {
            Debug.LogError($"Day 파싱 실패: {parts[0]}");
            return;
        }

        if (!Enum.TryParse(parts[1], true, out DailyRoutine routine))
        {
            Debug.LogError($"Routine 파싱 실패: {parts[1]}");
            return;
        }

        Debug.Log($"파싱 성공 - Day: {day}, Routine: {routine}, Chunk: {chunkIndex}");

        SaveTile(pattern);

        SaveObject(pattern, day, routine);

        string folderPath = Path.Combine(Application.dataPath, "0_TilePatterns");
        string filePath = Path.Combine(folderPath);

        Directory.CreateDirectory(filePath);

        string path = Path.Combine(filePath, SaveFileName + ".json");
        File.WriteAllText(path, JsonConvert.SerializeObject(pattern, Formatting.Indented));

        Debug.Log($"Tilemap 저장 완료, 경로: {filePath}");
    }

    //일반 로드 
    public void Load(int Day, DailyRoutine routine, Vector2 offset)
    {
        string folderPath = Path.Combine(Application.dataPath, "0_TilePatterns");
        
        string filePath = Path.Combine(folderPath, $"{Day}_{routine}.json");

        if (!File.Exists(filePath))
        {
            Debug.LogError($"TilePattern 파일을 찾을 수 없습니다: {filePath}");
            return;
        }

        string json = File.ReadAllText(filePath);
        TilePattern pattern = JsonConvert.DeserializeObject<TilePattern>(json);

        var patternMapping = new PatternRootMapping(Day, routine);

        ClearTile(previousRoutine);

        LoadTile(pattern, patternMapping, offset);
        LoadObject(pattern, patternMapping, offset);

        previousDay = Day;
        previousRoutine = routine;

    }
    //청크 로드 
    public void LoadToChunk(int Day, DailyRoutine routine, Vector2 offset, int chunkIndex)
    {
        string folderPath = Path.Combine(Application.dataPath, "0_TilePatterns");
        string fileName = $"{Day}_{routine}_Chunk{chunkIndex}.json";
        string filePath = Path.Combine(folderPath, fileName);

        if (!File.Exists(filePath))
        {
            Debug.LogError($"청크 파일을 찾을 수 없습니다: {filePath}");
            return;
        }

        string json = File.ReadAllText(filePath);
        TilePattern pattern = JsonConvert.DeserializeObject<TilePattern>(json);

        var patternMapping = new PatternRootMapping(Day, routine);

        ClearTile(patternMapping.routine);

        // 일반 Load 메서드 호출 (추적 없음)
        LoadTile(pattern, patternMapping, offset);
        LoadObject(pattern, patternMapping, offset);

        Debug.Log($"청크 {chunkIndex}에 해당하는 맵 로드 완료");
    }


    private void SaveTile(TilePattern pattern)
    {
        pattern.tileMapDatas.Clear(); // 기존 데이터 초기화

        //Chunk 제거용도
        string nameWithoutChunk = pattern.patternName;
        if (pattern.patternName.Contains("_Chunk"))
        {
            int chunkPos = pattern.patternName.LastIndexOf("_Chunk");
            nameWithoutChunk = pattern.patternName.Substring(0, chunkPos);
        }

        for (int i = 0; i < tilemaps.Count; i++)
        {
            Tilemap map = tilemaps[i];

            //map.name = 1_ArriveSchool_Breakfast 
            //pattern.patternName = 1_ArriveSchool_Breakfast_Chunk0
            if (map.name.Contains(nameWithoutChunk))
            {
                BoundsInt bounds = map.cellBounds;
                TileBase[] allTiles = map.GetTilesBlock(bounds);

                TileMapData mapData = new TileMapData(); // 타일맵 단위 MapData 생성
                mapData.mapName = map.name;
                mapData.tiles = new List<TileInfo>();


                int index = 0;

                for (int y = 0; y < bounds.size.y; y++)
                {
                    for (int x = 0; x < bounds.size.x; x++, index++)
                    {
                        TileBase tile = allTiles[index];
                        if (tile == null) continue;

                        int id = database.GetTileId(tile);

                        mapData.tiles.Add(new TileInfo(
                            bounds.x + x,
                            bounds.y + y,
                            id
                        ));

                    }
                }

                pattern.tileMapDatas.Add(mapData); // MapData 단위로 추가
            }
     
        }
    }

    private void SaveObject(TilePattern pattern,int day ,DailyRoutine routine)
    {
        pattern.objects.Clear();

        Transform targetRoot = GetObjectRoot(day, routine);

        if (targetRoot == null)
        {
            Debug.LogError($"Day {day}, Routine {routine}에 해당하는 objectRoot가 없습니다!");
            return;
        }

        // objectRoot의 자식들을 저장
        foreach (Transform child in targetRoot)
        {
            int id = objectDatabase.GetPrefabIndex(child.name);

            if (id == -1)
            {
                Debug.LogWarning($"오브젝트 ID를 찾을 수 없음: {child.name}");
                continue;
            }

            pattern.objects.Add(new ObjectInfo(child.position.x, child.position.y, id));
        }

        Debug.Log($"세이브 완료: {pattern.objects.Count}개");
    }

    private void LoadTile(TilePattern pattern, PatternRootMapping patternRootMapping,Vector2 offset)
    {
             

        for (int i = 0; i < tilemaps.Count; i++)
        {
            Tilemap map = tilemaps[i];

            foreach (var mapData in pattern.tileMapDatas)
            {
                if (map.name == mapData.mapName)
                {
                    
                    foreach (var tileInfo in mapData.tiles)
                    {
                        TileBase tile = database.GetTile(tileInfo.id);

                        if (tile != null)
                        {
                            // 오프셋 적용
                            int newX = tileInfo.x + Mathf.RoundToInt(offset.x);
                            int newY = tileInfo.y + Mathf.RoundToInt(offset.y);

                            Vector3Int tilePos = new Vector3Int(newX, newY, 0);
                            map.SetTile(tilePos, tile);

                        }

                       
                    }

                    break; // 매칭되는 mapData 적용 후 종료
                }
            }
        }
    }


    private void LoadObject(TilePattern pattern, PatternRootMapping patternRootMapping,Vector2 offset)
    {
        //1. 매핑에서 해당하는 이전 매핑 찾고 모두 삭제

        Transform prevRoot = GetObjectRoot(previousDay, previousRoutine);

        for (int i = prevRoot.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(prevRoot.GetChild(i).gameObject);
        }


        //2. 전달 받은 현재 매핑 찾고 Load
        Transform targetRoot = GetObjectRoot(patternRootMapping.day, patternRootMapping.routine);
        if (targetRoot == null)
        {
            Debug.LogError($"Day {patternRootMapping.day}, Routine {patternRootMapping.routine}에 해당하는 objectRoot가 없습니다!");
            return;
        }

        Debug.Log($"로드 대상: {targetRoot.name} (Day {patternRootMapping.day}, Routine {patternRootMapping.routine})");

        //3. 실제 로드는 여기에서 
        foreach (var o in pattern.objects)
        {
            GameObject prefab = objectDatabase.GetPrefab(o.id);

            if (prefab != null)
            {
                Vector3 pos = new Vector3(o.x + offset.x, o.y + offset.y, 0);
                Instantiate(prefab, pos, Quaternion.identity, targetRoot);
            }
            else
            {
                Debug.LogWarning($"Prefab ID {o.id}를 찾을 수 없음");
            }
        }
    }

    /// <summary>
    /// 다음 패턴 로드 전에 이전 패턴의 타일들 전부 삭제
    /// </summary>
    /// <param name="previousRoutine"></param>
    private void ClearTile(DailyRoutine previousRoutine)
    {
        if (previousRoutine != DailyRoutine.ArriveSchool_Breakfast)
        {
            foreach (var map in tilemaps)
            {
                map.ClearAllTiles();
            }
        }
    }

    private Transform GetObjectRoot(int day, DailyRoutine routine)
    {
        foreach (var mapping in patternMappings)
        {
            if (mapping.day == day && mapping.routine == routine)
            {
                return mapping.objectRoot;
            }
        }
        return null;
    }

    private PatternRootMapping GetPatternMapping(int day, DailyRoutine routine)
    {
        foreach (var mapping in patternMappings)
        {
            if (mapping.day == day && mapping.routine == routine)
            {
                return mapping;
            }
        }
        return null;
    }
}
