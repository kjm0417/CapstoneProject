using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlowConnectSystem : MiniGameBase
{
    [Header("레벨 데이터")]
    [SerializeField] private FlowConnectLevelData levelData;

    [Header("그리드 설정")]
    [SerializeField] private RectTransform gridRoot;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private float cellSize = 40f;
    [SerializeField] private float cellSpacing = 2f;

    [Header("색상 설정")]
    [SerializeField] private Color emptyColor1 = new Color(0.9f, 0.9f, 0.9f);
    [SerializeField] private Color emptyColor2 = new Color(0.8f, 0.8f, 0.8f);
    [SerializeField] private Color wallColor = Color.magenta;

    [Header("플레이어")]
    [SerializeField] private Color playerHighlightColor = new Color(1f, 1f, 0f, 1f);

    [Header("UI")]
    [SerializeField] private Text statusText;

    private int gridSize = 15;
    private FlowConnectCell[,] grid;
    private Image[,] cellVisuals;
    private Vector2Int playerPos;
    private int carriedColorIndex = -1;
    private List<Vector2Int> currentPath = new List<Vector2Int>();
    private List<ColorPairState> colorPairStates = new List<ColorPairState>();

    private class ColorPairState
    {
        public Color color;
        public Vector2Int startPos;
        public Vector2Int endPos;
        public bool isConnected;
        public List<Vector2Int> path = new List<Vector2Int>();
    }

    protected override void OnStartHook()
    {
        InitializeGrid();
        InitializePlayer();
        UpdateStatusUI();
    }

    protected override void OnUpdateHook(float deltaTime)
    {
        HandleInput();
    }

    protected override bool CheckClearRealtime()
    {
        foreach (var state in colorPairStates)
        {
            if (!state.isConnected)
                return false;
        }
        return colorPairStates.Count > 0;
    }

    protected override void OnCleanupHook()
    {
    }

    private void InitializeGrid()
    {
        if (!gridRoot)
        {
            Debug.LogError("[FlowConnect] gridRoot가 설정되지 않았습니다!");
            return;
        }

        CreateDefaultLevel();

        grid = new FlowConnectCell[gridSize, gridSize];
        cellVisuals = new Image[gridSize, gridSize];

        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                Vector2Int pos = new Vector2Int(row, col);
                grid[row, col] = new FlowConnectCell(pos);

                GameObject cellObj = Instantiate(cellPrefab, gridRoot);
                cellObj.name = $"Cell_({row},{col})";
                Image cellImage = cellObj.GetComponent<Image>();
                
                cellVisuals[row, col] = cellImage;
            }
        }

        ApplyLevelData();
        RenderBoard();
    }

    private void CreateDefaultLevel()
    {
        gridSize = 15;
        colorPairStates.Clear();

        // 레벨 데이터가 있으면 사용
        if (levelData != null && levelData.colorPairs != null && levelData.colorPairs.Count > 0)
        {
            gridSize = levelData.gridSize;
            
            // 벽 배치
            foreach (var wallPos in levelData.wallPositions)
            {
                // 벽은 ApplyLevelData에서 처리
            }
            
            // 색상 쌍 복사
            foreach (var pairData in levelData.colorPairs)
            {
                colorPairStates.Add(new ColorPairState
                {
                    color = pairData.color,
                    startPos = pairData.startPos,
                    endPos = pairData.endPos,
                    isConnected = false
                });
            }
        }
        else
        {
            // 기본 레벨
            colorPairStates.Add(new ColorPairState
            {
                color = Color.red,
                startPos = new Vector2Int(2, 2),
                endPos = new Vector2Int(2, 12),
                isConnected = false
            });

            colorPairStates.Add(new ColorPairState
            {
                color = Color.green,
                startPos = new Vector2Int(12, 2),
                endPos = new Vector2Int(12, 12),
                isConnected = false
            });

            colorPairStates.Add(new ColorPairState
            {
                color = Color.blue,
                startPos = new Vector2Int(7, 7),
                endPos = new Vector2Int(10, 7),
                isConnected = false
            });
        }
    }

    private void ApplyLevelData()
    {
        // 벽 배치
        if (levelData != null && levelData.wallPositions != null)
        {
            foreach (var wallPos in levelData.wallPositions)
            {
                if (IsValidPos(wallPos))
                {
                    grid[wallPos.x, wallPos.y].type = CellType.Wall;
                    grid[wallPos.x, wallPos.y].color = wallColor;
                }
            }
        }
        
        // 색상 쌍 배치
        for (int i = 0; i < colorPairStates.Count; i++)
        {
            var state = colorPairStates[i];
            
            // 알파값 수정 (투명도 문제 해결)
            Color fixedColor = state.color;
            fixedColor.a = 1f;
            state.color = fixedColor;

            if (IsValidPos(state.startPos))
            {
                grid[state.startPos.x, state.startPos.y].type = CellType.ColorPoint;
                grid[state.startPos.x, state.startPos.y].color = state.color;
                grid[state.startPos.x, state.startPos.y].isStartPoint = true;
                grid[state.startPos.x, state.startPos.y].colorIndex = i;
            }

            if (IsValidPos(state.endPos))
            {
                grid[state.endPos.x, state.endPos.y].type = CellType.ColorPoint;
                grid[state.endPos.x, state.endPos.y].color = state.color;
                grid[state.endPos.x, state.endPos.y].isStartPoint = false;
                grid[state.endPos.x, state.endPos.y].colorIndex = i;
            }
        }
    }

    private void InitializePlayer()
    {
        playerPos = new Vector2Int(1, 1);
        carriedColorIndex = -1;
        currentPath.Clear();
        UpdatePlayerVisual();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.W))
            TryMove(new Vector2Int(-1, 0));
        else if (Input.GetKeyDown(KeyCode.S))
            TryMove(new Vector2Int(1, 0));
        else if (Input.GetKeyDown(KeyCode.A))
            TryMove(new Vector2Int(0, -1));
        else if (Input.GetKeyDown(KeyCode.D))
            TryMove(new Vector2Int(0, 1));

        if (Input.GetKeyDown(KeyCode.Space))
            HandleSpaceKey();

        if (Input.GetKeyDown(KeyCode.Q))
            ResetGame();
    }

    private void TryMove(Vector2Int direction)
    {
        Vector2Int newPos = playerPos + direction;

        if (!IsValidPos(newPos))
            return;

        FlowConnectCell targetCell = grid[newPos.x, newPos.y];

        // 벽은 못 감
        if (targetCell.type == CellType.Wall)
            return;

        // 다른 색상의 경로는 못 감
        if (targetCell.type == CellType.Path && targetCell.colorIndex != carriedColorIndex)
            return;

        // 색상을 들고 있을 때 다른 색상의 ColorPoint는 못 감
        if (carriedColorIndex >= 0 && targetCell.type == CellType.ColorPoint)
        {
            if (targetCell.colorIndex != carriedColorIndex)
                return;
        }

        playerPos = newPos;

        // 색상을 들고 있는 경우
        if (carriedColorIndex >= 0)
        {
            UpdatePath(newPos);
            
            // 같은 색상의 끝점에 도착하면 자동 연결
            if (targetCell.type == CellType.ColorPoint && targetCell.colorIndex == carriedColorIndex)
            {
                // 시작점이 아닌 경우에만 연결
                if (!targetCell.isStartPoint || currentPath.Count > 1)
                {
                    CompleteConnection();
                    return;
                }
            }
        }

        UpdatePlayerVisual();
        RenderBoard();
    }

    private void HandleSpaceKey()
    {
        FlowConnectCell currentCell = grid[playerPos.x, playerPos.y];

        if (carriedColorIndex < 0)
        {
            if (currentCell.type == CellType.ColorPoint)
            {
                carriedColorIndex = currentCell.colorIndex;
                currentPath.Clear();
                currentPath.Add(playerPos);

                ClearPathForColor(carriedColorIndex);
                colorPairStates[carriedColorIndex].isConnected = false;

                UpdatePlayerVisual();
                UpdateStatusUI();
            }
        }
        else
        {
            if (currentCell.type == CellType.ColorPoint && currentCell.colorIndex == carriedColorIndex)
            {
                CompleteConnection();
            }
            else
            {
                CancelPath();
            }
        }
    }

    private void UpdatePath(Vector2Int newPos)
    {
        int existingIndex = currentPath.IndexOf(newPos);
        if (existingIndex >= 0)
        {
            for (int i = currentPath.Count - 1; i > existingIndex; i--)
            {
                Vector2Int pathPos = currentPath[i];
                if (grid[pathPos.x, pathPos.y].type == CellType.Path)
                {
                    grid[pathPos.x, pathPos.y].type = CellType.Empty;
                    grid[pathPos.x, pathPos.y].colorIndex = -1;
                }
                currentPath.RemoveAt(i);
            }
        }
        else
        {
            currentPath.Add(newPos);

            FlowConnectCell cell = grid[newPos.x, newPos.y];
            if (cell.type != CellType.ColorPoint)
            {
                cell.type = CellType.Path;
                cell.color = colorPairStates[carriedColorIndex].color;
                cell.colorIndex = carriedColorIndex;
            }
        }
    }

    private void CompleteConnection()
    {
        colorPairStates[carriedColorIndex].isConnected = true;
        colorPairStates[carriedColorIndex].path = new List<Vector2Int>(currentPath);

        carriedColorIndex = -1;
        currentPath.Clear();
        UpdatePlayerVisual();
        UpdateStatusUI();
        RenderBoard();
    }

    private void CancelPath()
    {
        ClearPathForColor(carriedColorIndex);
        carriedColorIndex = -1;
        currentPath.Clear();

        UpdatePlayerVisual();
        UpdateStatusUI();
        RenderBoard();
    }

    private void ClearPathForColor(int colorIndex)
    {
        if (colorIndex < 0 || colorIndex >= colorPairStates.Count)
            return;

        // 전체 그리드를 스캔해서 해당 색상의 경로를 모두 제거
        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                FlowConnectCell cell = grid[row, col];
                if (cell.type == CellType.Path && cell.colorIndex == colorIndex)
                {
                    cell.type = CellType.Empty;
                    cell.colorIndex = -1;
                }
            }
        }
        
        colorPairStates[colorIndex].path.Clear();
    }

    private void ResetGame()
    {
        for (int i = 0; i < colorPairStates.Count; i++)
        {
            ClearPathForColor(i);
            colorPairStates[i].isConnected = false;
        }

        playerPos = new Vector2Int(1, 1);
        carriedColorIndex = -1;
        currentPath.Clear();

        UpdatePlayerVisual();
        UpdateStatusUI();
        RenderBoard();
    }

    private void RenderBoard()
    {
        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                FlowConnectCell cell = grid[row, col];
                Image cellImage = cellVisuals[row, col];

                if (cellImage == null)
                    continue;

                Color cellColor;
                
                if (row == playerPos.x && col == playerPos.y)
                {
                    if (carriedColorIndex >= 0)
                    {
                        cellColor = colorPairStates[carriedColorIndex].color;
                    }
                    else
                    {
                        cellColor = playerHighlightColor;
                    }
                }
                else
                {
                    switch (cell.type)
                    {
                        case CellType.Empty:
                            cellColor = ((row + col) % 2 == 0) ? emptyColor1 : emptyColor2;
                            break;

                        case CellType.Wall:
                            cellColor = wallColor;
                            break;

                        case CellType.ColorPoint:
                        case CellType.Path:
                            cellColor = cell.color;
                            break;
                            
                        default:
                            cellColor = emptyColor1;
                            break;
                    }
                }
                
                cellImage.color = cellColor;
            }
        }
    }

    private void UpdatePlayerVisual()
    {
        RenderBoard();
    }

    private void UpdateStatusUI()
    {
        if (!statusText)
            return;

        int connectedCount = 0;
        foreach (var state in colorPairStates)
        {
            if (state.isConnected)
                connectedCount++;
        }

        statusText.text = $"연결: {connectedCount} / {colorPairStates.Count}";
    }

    private bool IsValidPos(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridSize && pos.y >= 0 && pos.y < gridSize;
    }
}
