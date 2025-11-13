# Requirements Document

## Introduction

Flow Connect 미니게임은 15x15 그리드 보드에서 플레이어가 WASD 키로 이동하며, 같은 색상의 시작점과 끝점을 경로로 연결하는 퍼즐 게임입니다. 플레이어는 스페이스바로 색상을 선택하고, 이동하면서 경로를 그리며, 같은 색상의 다른 점에 도착하여 스페이스바를 다시 누르면 연결이 완료됩니다. 모든 색상 쌍(3개)을 성공적으로 연결하면 게임이 클리어됩니다.

## Glossary

- **FlowConnectSystem**: 15x15 그리드 보드, 플레이어 이동, 색상 선택 및 경로 연결 로직을 관리하는 미니게임 시스템
- **Player**: WASD 키로 그리드 상에서 이동하며 색상을 선택하고 경로를 그리는 게임 내 캐릭터
- **ColorPoint**: 그리드 상에 배치된 색상의 시작점 또는 끝점
- **Path**: 플레이어가 색상을 선택한 후 이동하면서 그려지는 연결 경로
- **Grid**: 15x15 크기의 게임 보드
- **Cell**: 그리드를 구성하는 개별 칸 (Empty, Wall, ColorPoint, Path 타입 존재)
- **ColorPair**: 연결해야 할 같은 색상의 시작점과 끝점 쌍

## Requirements

### Requirement 1: 그리드 보드 초기화

**User Story:** 게임 개발자로서, 게임 시작 시 15x15 그리드 보드가 자동으로 생성되고 레벨 데이터에 따라 벽과 색상 점이 배치되기를 원합니다. 이를 통해 다양한 레벨을 쉽게 구성할 수 있습니다.

#### Acceptance Criteria

1. WHEN THE FlowConnectSystem starts, THE FlowConnectSystem SHALL create a 15x15 grid of cells
2. WHEN THE FlowConnectSystem creates the grid, THE FlowConnectSystem SHALL instantiate visual cell objects using a cell prefab for each grid position
3. WHEN THE FlowConnectSystem loads level data, THE FlowConnectSystem SHALL place walls at positions specified in the level data
4. WHEN THE FlowConnectSystem loads level data, THE FlowConnectSystem SHALL place color points at start and end positions for each color pair
5. WHEN THE FlowConnectSystem initializes the board, THE FlowConnectSystem SHALL apply a checkerboard pattern to empty cells using two alternating colors

### Requirement 2: 플레이어 이동

**User Story:** 플레이어로서, WASD 키를 사용하여 그리드 상에서 상하좌우로 이동할 수 있기를 원합니다. 이를 통해 원하는 위치로 자유롭게 이동할 수 있습니다.

#### Acceptance Criteria

1. WHEN THE Player presses the W key, THE FlowConnectSystem SHALL move THE Player one cell upward if the target cell is not a wall and is within grid bounds
2. WHEN THE Player presses the S key, THE FlowConnectSystem SHALL move THE Player one cell downward if the target cell is not a wall and is within grid bounds
3. WHEN THE Player presses the A key, THE FlowConnectSystem SHALL move THE Player one cell to the left if the target cell is not a wall and is within grid bounds
4. WHEN THE Player presses the D key, THE FlowConnectSystem SHALL move THE Player one cell to the right if the target cell is not a wall and is within grid bounds
5. WHEN THE Player attempts to move to a cell occupied by another color's path, THE FlowConnectSystem SHALL prevent the movement
6. WHEN THE Player moves successfully, THE FlowConnectSystem SHALL update the player visual position to the new cell location

### Requirement 3: 색상 선택 및 경로 그리기

**User Story:** 플레이어로서, 색상 점에서 스페이스바를 눌러 색상을 선택하고, 이동하면서 자동으로 경로가 그려지기를 원합니다. 이를 통해 직관적으로 경로를 만들 수 있습니다.

#### Acceptance Criteria

1. WHEN THE Player presses the Space key while on a ColorPoint and not carrying a color, THE FlowConnectSystem SHALL select that color and mark THE Player as carrying the color
2. WHEN THE Player selects a color, THE FlowConnectSystem SHALL clear any existing path for that color
3. WHEN THE Player moves while carrying a color, THE FlowConnectSystem SHALL add the current cell to the path for that color
4. WHEN THE Player moves to a cell already in the current path, THE FlowConnectSystem SHALL remove all path segments after that cell (backtracking)
5. WHEN THE Player presses the Space key while carrying a color on a different ColorPoint of the same color, THE FlowConnectSystem SHALL complete the connection and mark the color pair as connected
6. WHEN THE Player presses the Space key while carrying a color on an empty cell or different color point, THE FlowConnectSystem SHALL cancel the path and clear the carried color

### Requirement 4: 경로 충돌 방지

**User Story:** 플레이어로서, 다른 색상의 경로와 겹치지 않도록 이동이 제한되기를 원합니다. 이를 통해 퍼즐의 난이도와 전략성을 유지할 수 있습니다.

#### Acceptance Criteria

1. WHEN THE Player attempts to move to a cell with another color's path, THE FlowConnectSystem SHALL prevent the movement
2. WHEN THE Player moves to a cell with the currently selected color's path, THE FlowConnectSystem SHALL allow the movement and trigger backtracking
3. WHEN THE Player moves to an empty cell or wall, THE FlowConnectSystem SHALL validate the move based on cell type and bounds

### Requirement 5: 게임 클리어 조건

**User Story:** 플레이어로서, 모든 색상 쌍(3개)을 성공적으로 연결했을 때 게임이 클리어되기를 원합니다. 이를 통해 명확한 목표를 달성할 수 있습니다.

#### Acceptance Criteria

1. WHEN THE FlowConnectSystem checks clear conditions, THE FlowConnectSystem SHALL verify that all color pairs are marked as connected
2. WHEN all color pairs are connected, THE FlowConnectSystem SHALL trigger the game completion with success status
3. WHEN THE FlowConnectSystem updates the game state, THE FlowConnectSystem SHALL display the number of connected color pairs out of the total

### Requirement 6: 게임 초기화

**User Story:** 플레이어로서, Q 키를 눌러 현재 진행 중인 모든 경로를 지우고 처음부터 다시 시작할 수 있기를 원합니다. 이를 통해 실수를 쉽게 수정할 수 있습니다.

#### Acceptance Criteria

1. WHEN THE Player presses the Q key, THE FlowConnectSystem SHALL clear all paths for all color pairs
2. WHEN THE Player presses the Q key, THE FlowConnectSystem SHALL reset THE Player position to the starting position (0, 0)
3. WHEN THE Player presses the Q key, THE FlowConnectSystem SHALL reset THE Player carried color state to not carrying
4. WHEN THE Player presses the Q key, THE FlowConnectSystem SHALL mark all color pairs as not connected
5. WHEN THE FlowConnectSystem resets the game, THE FlowConnectSystem SHALL update all visual elements to reflect the reset state

### Requirement 7: 비주얼 렌더링

**User Story:** 플레이어로서, 그리드의 각 셀이 현재 상태(빈칸, 벽, 색상 점, 경로)에 따라 적절한 색상으로 표시되기를 원합니다. 이를 통해 게임 상태를 명확하게 파악할 수 있습니다.

#### Acceptance Criteria

1. WHEN THE FlowConnectSystem renders the board, THE FlowConnectSystem SHALL display walls with the wall color (magenta)
2. WHEN THE FlowConnectSystem renders the board, THE FlowConnectSystem SHALL display color points with their assigned color
3. WHEN THE FlowConnectSystem renders the board, THE FlowConnectSystem SHALL display path cells with the color of the path
4. WHEN THE FlowConnectSystem renders the board, THE FlowConnectSystem SHALL display empty cells with a checkerboard pattern
5. WHEN THE FlowConnectSystem updates player position, THE FlowConnectSystem SHALL move the player visual to the current cell position
6. WHEN THE Player is carrying a color, THE FlowConnectSystem SHALL display the player visual with the carried color
7. WHEN THE Player is not carrying a color, THE FlowConnectSystem SHALL display the player visual with the default player color

### Requirement 8: 레벨 데이터 관리

**User Story:** 게임 개발자로서, ScriptableObject를 사용하여 레벨 데이터(벽 위치, 색상 쌍 위치)를 쉽게 생성하고 관리할 수 있기를 원합니다. 이를 통해 다양한 난이도의 레벨을 빠르게 제작할 수 있습니다.

#### Acceptance Criteria

1. WHEN THE FlowConnectSystem loads a level, THE FlowConnectSystem SHALL read wall positions from the level data ScriptableObject
2. WHEN THE FlowConnectSystem loads a level, THE FlowConnectSystem SHALL read color pair positions from the level data ScriptableObject
3. WHERE no level data is assigned, THE FlowConnectSystem SHALL create a default level with 3 color pairs
4. WHEN THE FlowConnectSystem creates a default level, THE FlowConnectSystem SHALL place color points at predefined positions
