using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class CameraMapLoader : MonoBehaviour
{
    public static string OnTileMapChange = "OnTileMapChange";

    private bool isInfiniteMapMode = false;

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform; //카메라
    [SerializeField] private Transform playerCameraPos; //카메라 위치 설정
    [Header("Chunk Settings")]
    [SerializeField] private float chunkWidth = 26f;   // 청크 가로 크기
    [SerializeField] private float chunkHeight = 18f;  // 청크 세로 크기
    [SerializeField] private int maxChunksX = 11;  // 가로 최대 개수
    [SerializeField] private int maxChunksY = 3;  // 세로 최대 개수

    [Header("References")]
    [SerializeField] private TileMapSaveLoad tileMapSaveLoad;

    [Header("Start Settings")]
    [SerializeField] private int startChunkIndex = 0; //시작 청크 선택 (0~23)

    private Vector2Int currentChunk;     // 현재 청크 좌표

    private int currentDay = 1;
    private DailyRoutine currentRoutine = DailyRoutine.Room;

    private Vector2Int previousChunk; // 이전 청크 좌표

    [SerializeField]
    private HashSet<Vector2Int> loadedChunksSet = new HashSet<Vector2Int>();


    [Header("Load Settings")]
    [SerializeField] private float loadDistance = 40f;  // 로드 거리 조절 변수

    [SerializeField]
    bool isCameraInit = true; //청크 로드할 때 최초 1회 카메라 설정 bool 변수

    [Header("임시_기말고사 이후에 카메라 시스템으로 변경")]
    [SerializeField]
    GameObject testSitDownPlayer;
    [SerializeField]
    GameObject testOriginPlayer;
    [SerializeField]
    CinemachineCamera cinemachine;

    private void OnEnable()
    {
        EventBus.Subscribe<int, DailyRoutine>(OnTileMapChange, SetDayRoutine);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<int, DailyRoutine>(OnTileMapChange, SetDayRoutine);
    }

    public void SetDayRoutine(int day, DailyRoutine routine)
    {
        currentDay = day;
        currentRoutine = routine;

        if (currentRoutine == DailyRoutine.ArriveSchool_Breakfast || currentRoutine == DailyRoutine.ArriveSchool_Morning)
        {
            if (isCameraInit)
            {
                //테스트
                //cameraTransform.transform.position = playerCameraPos.transform.position;
                testOriginPlayer.gameObject.SetActive(false);
                testSitDownPlayer.gameObject.SetActive(true);
                testSitDownPlayer.transform.position = playerCameraPos.transform.position;
                cinemachine.Follow = testSitDownPlayer.transform;
                cinemachine.LookAt = testSitDownPlayer.transform;

                isCameraInit = false;
            }

            isInfiniteMapMode = true;
            loadedChunksSet.Clear();

            tileMapSaveLoad.LoadToColliderStair(currentDay, currentRoutine); //복도 로드

            Vector2Int startChunkCoord = GetChunkCoordFromIndex(startChunkIndex);

            currentChunk = startChunkCoord;
            previousChunk = currentChunk;

            LoadChunk(currentChunk);
        }
        else
        {

            isInfiniteMapMode = false;

            loadedChunksSet.Clear();

            tileMapSaveLoad.Load(currentDay, currentRoutine);
        }
    }

    private void Update()
    {
        if (isInfiniteMapMode && !isCameraInit)
        {
            Vector2Int newChunk = GetCurrentChunk();

            if (newChunk != previousChunk)
            {
                currentChunk = newChunk;
                previousChunk = currentChunk;
            }

            Vector2Int[] neighbors = new Vector2Int[]
            {
            currentChunk,
            currentChunk + Vector2Int.up,
            currentChunk + Vector2Int.down,
            currentChunk + Vector2Int.left,
            currentChunk + Vector2Int.right,
            };

            foreach (var chunk in neighbors)
            {
                int idx = GetChunkIndex(chunk);
                Vector3 center = GetChunkCenter(idx);

                float dist = Vector2.Distance(cameraTransform.position, center);

                if (dist <= loadDistance)
                {
                    LoadChunk(chunk);
                }
            }
        }

    }

    private Vector2Int GetCurrentChunk()
    {
       
        Vector2 cameraPos = new Vector2(cameraTransform.position.x, cameraTransform.position.y);

        // ? 모든 청크를 순회하며 가장 가까운 청크 찾기
        int closestChunkX = 0;
        int closestChunkY = 0;
        float minDistance = float.MaxValue;

        for (int y = 0; y < maxChunksY; y++)
        {
            for (int x = 0; x < maxChunksX; x++)
            {
                Vector2Int chunkCoord = new Vector2Int(x, y);
                int chunkIndex = GetChunkIndex(chunkCoord);

                // 청크 중심 좌표 가져오기 (캐시 또는 JSON에서)
                Vector3 chunkCenter = GetChunkCenter(chunkIndex);

                float distance = Vector2.Distance(cameraPos, chunkCenter);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestChunkX = x;
                    closestChunkY = y;
                }
            }
        }

        Vector2Int result = new Vector2Int(closestChunkX, closestChunkY);

        return result;
    }


    private Vector3 GetChunkCenter(int chunkIndex)
    {
        Vector2Int coord = GetChunkCoordFromIndex(chunkIndex);

        float cx = coord.x * chunkWidth + chunkWidth * 0.5f;
        float cy = coord.y * chunkHeight + chunkHeight * 0.5f;

        return new Vector3(cx, cy, -10);
    }


    private void LoadChunk(Vector2Int chunk)
    {

        int chunkIndex = GetChunkIndex(chunk);
        // 청크 좌표가 범위 밖이면 바로 리턴
        if (chunk.x < 0 || chunk.x >= maxChunksX ||
            chunk.y < 0 || chunk.y >= maxChunksY)
        {        
            return;
        }

        // 이미 로드된 청크면 스킵
        if (loadedChunksSet.Contains(chunk))
        {
            return;
        }

        tileMapSaveLoad.LoadToChunk(currentDay, currentRoutine, chunkIndex);

        loadedChunksSet.Add(chunk);

        Debug.Log($"청크 {chunk} (Chunk{chunkIndex}.json) 로드 완료");
    }
    // 청크 인덱스 -> 청크 좌표 변환
    private Vector2Int GetChunkCoordFromIndex(int index)
    {
        int x = index % maxChunksX;       // X 증가 -> 오른쪽
        int y = index / maxChunksX;       // Y 증가 -> 위쪽

        return new Vector2Int(x, y);
    }
    private int GetChunkIndex(Vector2Int chunk)
    {
        return chunk.y * maxChunksX + chunk.x;
    }
    //private int GetChunkIndex(Vector2Int chunk)
    //{
    //    int normalizedX = ((chunk.x % maxChunksX) + maxChunksX) % maxChunksX;
    //    int normalizedY = ((chunk.y % maxChunksY) + maxChunksY) % maxChunksY;

    //    int index = normalizedY * maxChunksX + normalizedX;

    //    return index;
    //}


}