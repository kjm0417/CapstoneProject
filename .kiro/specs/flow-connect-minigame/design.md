# Design Document

## Overview

Flow Connect 미니게임은 Unity의 UI 시스템을 활용한 그리드 기반 퍼즐 게임입니다. `MiniGameBase`를 상속받아 기존 미니게임 프레임워크와 통합되며, 15x15 그리드에서 WASD 키보드 입력으로 플레이어를 이동시키고 스페이스바로 색상을 선택/연결하는 메커니즘을 구현합니다.

## Architecture

### High-Level Component Structure

```
FlowConnectMiniGame (MiniGameBase)
├─ Data Layer
│  ├─ Cell[,] board (15x15 grid data)
│  ├─ List<ColorPair> colorPairs
│  └─ FlowPlayer player
│
├─ Visual Layer
│  ├─ GameObject[,] cellObjects (UI Image grid)
│  ├─ Image playerVisual
│  └─ UI Text elements
│
├─ Input Handler
│  ├─ WASD movement
│  ├─ Space color selection
│  └─ Q reset
│
└─ Game Logic
   ├─ Board initialization
   ├─ Path management
   ├─ Collision detection
   └─ Win condition check
```

### Class Diagram

```
┌─────────────────────────┐
│   MiniGameBase          │
│  (Abstract Base Class)  │
└───────────┬─────────────┘
            │ inherits
            ▼
┌─────────────────────────┐
│ FlowConnectMiniGame     │
├─────────────────────────┤
│ - board: Cell[,]        │
│ - cellObjects: GO[,]    │
│ - colorPairs: List      │
│ - player: FlowPlayer    │
├─────────────────────────┤
│ + OnStartHook()         │
│ + OnUpdateHook()        │
│ + CheckClearRealtime()  │
│ - HandleInput()         │
│ - TryMovePlayer()       │
│ - HandleSpacePress()    │
│ - UpdateVisuals()       │
└─────────────────────────┘
            │
            │ uses
            ▼
┌─────────────────────────┐
│      Cell               │
├─────────────────────────┤
│ + position: Vector2Int  │
│ + type: CellType        │
│ + colorId: int          │
│ + pathColorId: int      │
│ + hasPlayer: bool       │
└─────────────────────────┘

┌─────────────────────────┐
│    ColorPair            │
├─────────────────────────┤
│ + colorId: int          │
│ + pointA: Vector2Int    │
│ + pointB: Vector2Int    │
│ + path: List<Vector2Int>│
│ + isConnected: bool     │
└─────────────────────────┘

┌─────────────────────────┐
│    FlowPlayer           │
├─────────────────────────┤
│ + position: Vector2Int  │
│ + selectedColorId: int  │
│ + isCarryingColor: bool │
│ + pickupStartPos: V2Int │
└─────────────────────────┘
```

## Components and Interfaces

### 1. FlowConnectMiniGame (Main Controller)

**Responsibility**: 게임의 전체 흐름을 관리하고 MiniGameBase와 통합

**Key Methods**:
- `OnStartHook()`: 게임 시작 시 보드 초기화 및 플레이어 스폰
- `OnUpdateHook(float deltaTime)`: 매 프레임 입력 처리 및 비주얼 업데이트
- `CheckClearRealtime()`: 모든 색상이 연결되었는지 확인
- `OnCleanupHook()`: 게임 종료 시 리소스 정리

**SerializeField Dependencies**:
```csharp
[SerializeField] private RectTransform boardContainer;
[SerializeField] private GameObject cellPrefab;
[SerializeField] private Image playerVisual;
[SerializeField] private Text scoreText;
[SerializeField] private Text hintText;
[SerializeField] private FlowLevelData levelData;
[SerializeField] private Color[] pathColors;
```

### 2. Cell (Data Structure)

**Responsibility**: 개별 그리드 셀의 상태를 저장

**Properties**:
- `position`: 그리드 상의 위치 (0-14, 0-14)
- `type`: CellType enum (Empty, Wall, ColorPoint)
- `colorId`: 색상 점인 경우 색상 ID (-1: 없음)
- `pathColorId`: 경로가 지나가는 경우 경로의 색상 ID (-1: 없음)
- `hasPlayer`: 플레이어가 현재 위치하는지 여부

**CellType Enum**:
```csharp
public enum CellType
{
    Empty,      // 빈 칸
    Wall,       // 벽 (이동 불가)
    ColorPoint  // 색상 시작/끝점
}
```

### 3. ColorPair (Data Structure)

**Responsibility**: 연결해야 할 색상 쌍의 정보와 현재 경로 상태를 관리

**Properties**:
- `colorId`: 고유 색상 식별자 (0, 1, 2, ...)
- `pointA`, `pointB`: 연결해야 할 두 점의 위치
- `path`: 현재 그려진 경로 (Vector2Int 리스트)
- `isConnected`: 완전히 연결되었는지 여부

**Key Operations**:
- 경로 추가: `path.Add(position)`
- 경로 제거 (백트래킹): `path.RemoveRange(index, count)`
- 경로 초기화: `path.Clear()`

### 4. FlowPlayer (Data Structure)

**Responsibility**: 플레이어의 현재 상태를 추적

**Properties**:
- `position`: 현재 그리드 위치
- `selectedColorId`: 현재 선택한 색상 (-1: 선택 안함)
- `isCarryingColor`: 색상을 들고 있는지 여부
- `pickupStartPos`: 색상을 집은 위치 (같은 위치에서 다시 놓는 것 방지)

### 5. FlowLevelData (ScriptableObject)

**Responsibility**: 레벨 데이터를 에디터에서 설정 가능하도록 관리

**Structure**:
```csharp
[CreateAssetMenu(fileName = "FlowLevel", menuName = "MiniGame/FlowLevelData")]
public class FlowLevelData : ScriptableObject
{
    public int gridSize = 15;
    public List<Vector2Int> walls;
    public List<ColorPairData> colorPairs;
}

[System.Serializable]
public class ColorPairData
{
    public Vector2Int startPos;
    public Vector2Int endPos;
}
```

## Data Models

### Board Representation

**2D Array Structure**:
```
board[x, y] where:
- x: 0 (left) to 14 (right)
- y: 0 (bottom) to 14 (top)
```

**Memory Layout**:
- Total cells: 15 × 15 = 225 cells
- Each cell: ~20 bytes (Vector2Int + enums + bools)
- Total memory: ~4.5 KB (negligible)

**Visual Representation**:
- Unity UI GridLayoutGroup으로 자동 배치
- 각 셀은 독립적인 GameObject + Image 컴포넌트
- 셀 크기: 50×50 픽셀 (조정 가능)

### Path Storage

**List-based Path**:
```csharp
List<Vector2Int> path = new List<Vector2Int>();
// Example: [(0,1), (1,1), (2,1), (2,2), ...]
```

**Advantages**:
- 순서 보장 (백트래킹 구현 용이)
- 동적 크기 조정
- 경로 길이 제한 없음

**Path Operations**:
1. **추가**: O(1) - `path.Add(newPos)`
2. **백트래킹**: O(n) - `path.RemoveRange(index, count)`
3. **검색**: O(n) - `path.IndexOf(pos)`
4. **초기화**: O(1) - `path.Clear()`

## Error Handling

### Input Validation

**Movement Validation**:
```csharp
private bool CanMoveTo(Vector2Int targetPos)
{
    // 1. 범위 체크
    if (!IsInBounds(targetPos))
    {
        Debug.LogWarning($"[FlowConnect] Out of bounds: {targetPos}");
        return false;
    }
    
    // 2. 벽 체크
    Cell targetCell = board[targetPos.x, targetPos.y];
    if (targetCell.type == CellType.Wall)
    {
        Debug.LogWarning($"[FlowConnect] Cannot move to wall at {targetPos}");
        return false;
    }
    
    // 3. 다른 색상 경로 체크
    if (targetCell.pathColorId != -1 && 
        targetCell.pathColorId != player.selectedColorId)
    {
        Debug.LogWarning($"[FlowConnect] Path collision at {targetPos}");
        return false;
    }
    
    return true;
}
```

### Color Selection Validation

**Space Key Validation**:
```csharp
private bool CanSelectColor(Vector2Int pos)
{
    Cell cell = board[pos.x, pos.y];
    
    // 색상 점이 아니면 선택 불가
    if (cell.type != CellType.ColorPoint)
    {
        Debug.LogWarning("[FlowConnect] Not a color point");
        return false;
    }
    
    return true;
}

private bool CanCompleteConnection(Vector2Int pos, int colorId)
{
    Cell cell = board[pos.x, pos.y];
    
    // 같은 색상 점인지 확인
    if (cell.type != CellType.ColorPoint || cell.colorId != colorId)
    {
        return false;
    }
    
    // 시작점과 다른 위치인지 확인
    if (pos == player.pickupStartPos)
    {
        Debug.LogWarning("[FlowConnect] Cannot connect at same position");
        return false;
    }
    
    return true;
}
```

### Null Reference Protection

**Component Validation**:
```csharp
protected override void OnStartHook()
{
    // 필수 컴포넌트 체크
    if (boardContainer == null)
    {
        Debug.LogError("[FlowConnect] BoardContainer is null!");
        Complete(false);
        return;
    }
    
    if (cellPrefab == null)
    {
        Debug.LogError("[FlowConnect] CellPrefab is null!");
        Complete(false);
        return;
    }
    
    // 초기화 진행
    InitializeBoard();
}
```

## Testing Strategy

### Unit Testing Approach

**Core Logic Tests** (EditMode):

1. **Board Initialization Tests**:
```csharp
[Test]
public void Board_ShouldBe15x15_WhenInitialized()
{
    var game = CreateFlowConnectGame();
    game.Init();
    
    Assert.AreEqual(15, game.GetBoardWidth());
    Assert.AreEqual(15, game.GetBoardHeight());
}

[Test]
public void ColorPairs_ShouldBe3_WhenDefaultLevelLoaded()
{
    var game = CreateFlowConnectGame();
    game.Init();
    
    Assert.AreEqual(3, game.GetColorPairCount());
}
```

2. **Movement Logic Tests**:
```csharp
[Test]
public void Player_ShouldNotMove_WhenTargetIsWall()
{
    var game = CreateFlowConnectGame();
    game.PlaceWallAt(new Vector2Int(1, 0));
    game.SetPlayerPosition(new Vector2Int(0, 0));
    
    game.TryMovePlayer(Vector2Int.right);
    
    Assert.AreEqual(new Vector2Int(0, 0), game.GetPlayerPosition());
}

[Test]
public void Player_ShouldNotMove_WhenTargetHasDifferentColorPath()
{
    var game = CreateFlowConnectGame();
    game.SetPathAt(new Vector2Int(1, 0), colorId: 1);
    game.SetPlayerCarryingColor(colorId: 2);
    
    game.TryMovePlayer(Vector2Int.right);
    
    Assert.AreEqual(new Vector2Int(0, 0), game.GetPlayerPosition());
}
```

3. **Path Management Tests**:
```csharp
[Test]
public void Path_ShouldBeCleared_WhenColorSelected()
{
    var game = CreateFlowConnectGame();
    game.CreatePathForColor(0, new List<Vector2Int> { /* ... */ });
    
    game.SelectColorAt(new Vector2Int(0, 1));
    
    Assert.AreEqual(0, game.GetPathLength(0));
}

[Test]
public void Path_ShouldBacktrack_WhenPlayerReturnsToExistingCell()
{
    var game = CreateFlowConnectGame();
    game.SelectColorAt(new Vector2Int(0, 1));
    game.MovePlayer(Vector2Int.right); // (1,1)
    game.MovePlayer(Vector2Int.right); // (2,1)
    
    game.MovePlayer(Vector2Int.left);  // Back to (1,1)
    
    Assert.AreEqual(2, game.GetPathLength(0)); // Should remove (2,1)
}
```

4. **Win Condition Tests**:
```csharp
[Test]
public void Game_ShouldClear_WhenAllColorsConnected()
{
    var game = CreateFlowConnectGame();
    game.ConnectColor(0);
    game.ConnectColor(1);
    game.ConnectColor(2);
    
    Assert.IsTrue(game.CheckClearRealtime());
}

[Test]
public void Game_ShouldNotClear_WhenOneColorNotConnected()
{
    var game = CreateFlowConnectGame();
    game.ConnectColor(0);
    game.ConnectColor(1);
    // Color 2 not connected
    
    Assert.IsFalse(game.CheckClearRealtime());
}
```

### Integration Testing Approach

**PlayMode Tests**:

1. **Full Gameplay Simulation**:
```csharp
[UnityTest]
public IEnumerator FullGame_ShouldComplete_WhenAllPathsDrawn()
{
    var gameObject = CreateFlowConnectGameObject();
    var game = gameObject.GetComponent<FlowConnectMiniGame>();
    game.StartGame();
    
    yield return new WaitForSeconds(0.1f);
    
    // Simulate player actions
    SimulateKeyPress(KeyCode.Space); // Select color
    SimulateKeyPress(KeyCode.D); // Move right
    // ... more moves
    SimulateKeyPress(KeyCode.Space); // Complete connection
    
    yield return new WaitForSeconds(0.1f);
    
    Assert.IsTrue(game.IsRunning == false); // Game completed
}
```

2. **Visual Rendering Tests**:
```csharp
[UnityTest]
public IEnumerator CellVisuals_ShouldUpdate_WhenPathDrawn()
{
    var game = CreateFlowConnectGameObject();
    yield return null;
    
    var cellImage = game.GetCellImage(1, 1);
    var initialColor = cellImage.color;
    
    game.DrawPathAt(new Vector2Int(1, 1), colorId: 0);
    yield return null;
    
    Assert.AreNotEqual(initialColor, cellImage.color);
}
```

### Manual Testing Checklist

- [ ] WASD 키로 모든 방향 이동 가능
- [ ] 벽에 막혀서 이동 불가
- [ ] 스페이스로 색상 선택 가능
- [ ] 이동 시 경로가 그려짐
- [ ] 같은 색상 점에서 연결 완료
- [ ] 다른 색상 경로와 겹치지 않음
- [ ] 백트래킹 (이전 경로로 돌아가기) 동작
- [ ] Q 키로 초기화 동작
- [ ] 3색 모두 연결 시 클리어
- [ ] UI에 연결 상태 표시

## Performance Considerations

### Optimization Strategies

1. **Object Pooling** (Optional):
   - 현재 구조는 225개 셀을 한 번만 생성하므로 풀링 불필요
   - 레벨 전환이 빈번하다면 셀 오브젝트 재사용 고려

2. **Update Loop Optimization**:
```csharp
protected override void OnUpdateHook(float deltaTime)
{
    // 입력은 매 프레임 체크 (키 입력은 GetKeyDown이므로 부하 낮음)
    HandleInput();
    
    // 비주얼은 상태 변경 시에만 업데이트 (플래그 사용)
    if (isDirty)
    {
        UpdateVisuals();
        isDirty = false;
    }
}
```

3. **Raycast Optimization**:
```csharp
// Image 컴포넌트의 Raycast Target을 false로 설정
// (마우스 입력을 사용하지 않으므로)
cellImage.raycastTarget = false;
```

4. **Memory Allocation Reduction**:
```csharp
// Vector2Int는 struct이므로 박싱 방지
// List 용량 미리 할당
path = new List<Vector2Int>(capacity: 50);
```

### Expected Performance

- **Frame Rate**: 60 FPS 유지 (입력 처리와 비주얼 업데이트만 수행)
- **Memory**: ~5 MB (225개 GameObject + Image 컴포넌트)
- **Load Time**: <0.5초 (보드 생성 및 초기화)

## Implementation Notes

### Unity Editor Setup

1. **Hierarchy 구조**:
```
FlowConnectMiniGame
└─ Canvas (World Space)
   └─ GameRoot
      ├─ BoardContainer (GridLayoutGroup)
      ├─ PlayerVisual (Image)
      └─ UI
         ├─ ScoreText
         └─ HintText
```

2. **Prefab 설정**:
   - CellPrefab: GameObject + RectTransform + Image
   - 크기: 50×50
   - Pivot: (0.5, 0.5)

3. **GridLayoutGroup 설정**:
   - Cell Size: (50, 50)
   - Spacing: (0, 0)
   - Constraint: Fixed Column Count = 15
   - Child Alignment: Middle Center

### Code Organization

**File Structure**:
```
Assets/Scripts/MiniGame/MiniGamePlay/FlowConnectMiniGame/
├─ FlowConnectMiniGame.cs (main controller)
├─ FlowPlayer.cs (player data structure)
├─ Cell.cs (cell data structure)
├─ ColorPair.cs (color pair data structure)
└─ FlowLevelData.cs (ScriptableObject)
```

### Integration with MiniGameBase

**Lifecycle Hooks**:
- `OnStartHook()`: 보드 초기화, 플레이어 스폰
- `OnUpdateHook(deltaTime)`: 입력 처리, 비주얼 업데이트
- `CheckClearRealtime()`: 클리어 조건 체크
- `OnCleanupHook()`: 리소스 정리

**Event Flow**:
```
MiniGameManager.StartMiniGame()
  → FlowConnectMiniGame.StartGame()
    → OnStartHook()
      → InitializeBoard()
      → SpawnPlayer()
  → (Game Loop)
    → OnUpdateHook()
      → HandleInput()
      → UpdateVisuals()
    → CheckClearRealtime()
      → Complete(true) if all connected
  → OnCleanupHook()
```
