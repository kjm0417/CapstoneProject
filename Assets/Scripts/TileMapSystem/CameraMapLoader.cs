using System.Collections.Generic;
using UnityEngine;

public class CameraMapLoader : MonoBehaviour
{
    public static string OnTileMapChange = "OnTileMapChange";

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform; //카메라


    [Header("Chunk Settings")]
    [SerializeField] private float chunkWidth = 50f;   // 청크 가로 크기
    [SerializeField] private float chunkHeight = 50f;  // 청크 세로 크기
    [SerializeField] private int maxChunksX = 8;  // 가로 최대 개수
    [SerializeField] private int maxChunksY = 3;  // 세로 최대 개수
    private int maxChunks => maxChunksX * maxChunksY; // 총 24개
    [Header("References")]
    [SerializeField] private TileMapSaveLoad tileMapSaveLoad;

    private Vector2Int currentChunk;     // 현재 청크 좌표

    private int currentDay = 1;
    private DailyRoutine currentRoutine = DailyRoutine.Room;

    private Vector2Int previousChunk; // 이전 청크 좌표

    // 로드된 청크 추적 (로드 순서 유지)
    private Queue<Vector2Int> loadedChunksQueue = new Queue<Vector2Int>();
    private HashSet<Vector2Int> loadedChunksSet = new HashSet<Vector2Int>();

    // 각 줄(y좌표)별 로드된 청크 개수 추적
    private Dictionary<int, int> chunksPerRow = new Dictionary<int, int>();

    private Dictionary<int, int> rowStartX = new Dictionary<int, int>();

    private void OnEnable()
    {
        EventBus.Subscribe<int, DailyRoutine>(OnTileMapChange, SetDayRoutine);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<int, DailyRoutine>(OnTileMapChange, SetDayRoutine);
    }

    private bool isInfiniteMapMode = false;
    public void SetDayRoutine(int day, DailyRoutine routine)
    {
        currentDay = day;
        currentRoutine = routine;

        if (currentRoutine == DailyRoutine.ArriveSchool_Breakfast)
        {
            isInfiniteMapMode = true;

            //청크 정보만 초기화 (맵은 그대로)
            loadedChunksQueue.Clear();
            loadedChunksSet.Clear();
            chunksPerRow.Clear(); // 줄별 카운트 초기화
            rowStartX.Clear(); // 

            UpdateCurrentChunk(); //이 메서드에서 currentChunk 계산

            previousChunk = currentChunk; //이전 청크에 currentChunk 저장

            LoadChunk(currentChunk); //여기에서 실제 맵 로드 진행하는 코드 있음
        }
        else
        {

            isInfiniteMapMode = false;

            //청크 정보만 초기화 (맵은 그대로)
            loadedChunksQueue.Clear();
            loadedChunksSet.Clear();
            chunksPerRow.Clear();
            rowStartX.Clear();

            tileMapSaveLoad.Load(currentDay, currentRoutine,Vector2.zero );
        }
    }

    private void Update()
    {
        if(isInfiniteMapMode)
        {
            Vector2Int newChunk = GetCurrentChunk(); //카메라 위치 확인하여 newChunk 계산

            // 청크가 변경되었으면 맵 로드
            if (newChunk != previousChunk)
            {
                LoadChunk(newChunk);

                previousChunk = newChunk;
                currentChunk = newChunk;
            }
        }
      
    }
    private void UpdateCurrentChunk()
    {
        currentChunk = GetCurrentChunk();

        Debug.Log($"현재 청크: {currentChunk}");
    }

    private Vector2Int GetCurrentChunk()
    {
        int chunkX = Mathf.FloorToInt(cameraTransform.position.x / chunkWidth);
        int chunkY = Mathf.FloorToInt(cameraTransform.position.y / chunkHeight);

        return new Vector2Int(chunkX, chunkY);
    }
    private void LoadChunk(Vector2Int chunk)
    {
        //int chunkIndex = Mathf.Abs(chunk.x) % maxChunks;

        int chunkIndex = GetChunkIndex(chunk);

        if (loadedChunksSet.Contains(chunk))
        {
            Debug.Log($"청크 {chunk}는 이미 로드됨 (스킵)");
            return;
        }
        // 해당 줄(y)의 로드된 청크 개수 확인
        int rowY = chunk.y;
        if (!chunksPerRow.ContainsKey(rowY))
        {
            chunksPerRow[rowY] = 0;
            rowStartX[rowY] = chunk.x;
        }

        // 해당 줄이 최대 개수 도달했는지 확인
        if (chunksPerRow[rowY] >= maxChunksX)
        {
            Debug.Log($"줄 y={rowY}의 최대 청크 개수 도달 ({maxChunksX}개) - 추가 로드 중단");
            return;
        }
        Debug.Log($"청크 {chunk} 로드 (현재 {loadedChunksQueue.Count}/{maxChunks})");

        //해당 줄의 시작점으로 부터 상대 위치 계싼
        int relativeX = chunk.x - rowStartX[rowY];

        Vector2 chunkOffset = new Vector2(
            relativeX * chunkWidth,
            chunk.y * chunkHeight
        );

        // 로드된 청크로 등록
        loadedChunksQueue.Enqueue(chunk);
        loadedChunksSet.Add(chunk);
        chunksPerRow[rowY]++;

        //청크에 맵 로드 (이전 것 유지, 계속 누적)
        tileMapSaveLoad.LoadToChunk(currentDay, currentRoutine, chunkOffset, chunkIndex);
    }
    private int GetChunkIndex(Vector2Int chunk)
    {
        // x, y 좌표를 0~maxChunks 범위로 변환
        int normalizedX = ((chunk.x % maxChunksX) + maxChunksX) % maxChunksX; // 0~7
        int normalizedY = ((chunk.y % maxChunksY) + maxChunksY) % maxChunksY; // 0~2

        // 2D → 1D 인덱스 변환
        int index = normalizedY * maxChunksX + normalizedX;

        return index;
    }

}
