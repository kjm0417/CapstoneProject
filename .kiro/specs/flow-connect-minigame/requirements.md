# Requirements Document

## Introduction

Flow Connect는 15x15 그리드 기반의 퍼즐 미니게임입니다. 플레이어는 WASD 키로 이동하며, 스페이스바를 사용하여 같은 색상의 시작점과 끝점을 연결합니다. 모든 색상 쌍이 성공적으로 연결되면 게임이 클리어됩니다. 경로는 서로 겹칠 수 없으며, Q 키로 게임을 초기화할 수 있습니다.

## Glossary

- **FlowConnectGame**: 15x15 그리드 기반 색상 연결 퍼즐 미니게임 시스템
- **GameBoard**: 15x15 크기의 게임 보드, 각 셀은 빈칸, 벽, 색상 점, 또는 경로를 포함
- **Player**: WASD 키로 이동하며 색상을 선택하고 연결하는 플레이어 캐릭터
- **ColorPoint**: 연결해야 할 색상의 시작점 또는 끝점
- **Path**: 플레이어가 색상을 선택한 후 이동하면서 그려지는 경로
- **ColorPair**: 연결해야 할 같은 색상의 두 점 (시작점과 끝점)
- **Cell**: 게임 보드의 개별 칸, 빈칸/벽/색상점/경로 중 하나의 상태를 가짐
- **Wall**: 플레이어가 이동할 수 없는 장애물 셀

## Requirements

### Requirement 1: 게임 보드 초기화

**User Story:** 게임 개발자로서, 게임 시작 시 15x15 그리드 보드가 자동으로 생성되고 레벨 데이터에 따라 벽과 색상 점이 배치되기를 원합니다. 이를 통해 다양한 퍼즐 레벨을 쉽게 구성할 수 있습니다.

#### Acceptance Criteria

1. WHEN THE FlowConnectGame starts, THE GameBoard SHALL generate a 15x15 grid of Cell objects
2. WHEN THE GameBoard is generated, THE FlowConnectGame SHALL instantiate visual GameObject representations for each Cell using a prefab
3. WHEN level data is provided, THE FlowConnectGame SHALL place Wall objects at positions specified in the level data
4. WHEN level data is provided, THE FlowConnectGame SHALL place ColorPoint pairs at positions specified in the level data with unique color identifiers
5. WHERE no level data is provided, THE FlowConnectGame SHALL create a default level with three ColorPair objects

### Requirement 2: 플레이어 이동

**User Story:** 플레이어로서, WASD 키를 사용하여 그리드 상에서 상하좌우로 이동할 수 있기를 원합니다. 이를 통해 색상 점들 사이를 자유롭게 탐색할 수 있습니다.

#### Acceptance Criteria

1. WHEN THE Player presses the W key, THE Player SHALL move one Cell upward if the target Cell is not a Wall and is within bounds
2. WHEN THE Player presses the S key, THE Player SHALL move one Cell downward if the target Cell is not a Wall and is within bounds
3. WHEN THE Player presses the A key, THE Player SHALL move one Cell to the left if the target Cell is not a Wall and is within bounds
4. WHEN THE Player presses the D key, THE Player SHALL move one Cell to the right if the target Cell is not a Wall and is within bounds
5. WHEN THE Player attempts to move to a Cell occupied by a Path of a different color, THE FlowConnectGame SHALL prevent the movement
6. WHEN THE Player moves to a new Cell, THE FlowConnectGame SHALL update the Player visual position to match the new Cell location

### Requirement 3: 색상 선택 및 경로 그리기

**User Story:** 플레이어로서, 스페이스바를 눌러 색상 점에서 색상을 선택하고, 이동하면서 경로를 그릴 수 있기를 원합니다. 이를 통해 색상 쌍을 연결할 수 있습니다.

#### Acceptance Criteria

1. WHEN THE Player presses the Space key while on a ColorPoint and not carrying a color, THE FlowConnectGame SHALL select that color and mark the Player as carrying the color
2. WHEN THE Player selects a color, THE FlowConnectGame SHALL clear any existing Path for that ColorPair
3. WHEN THE Player is carrying a color and moves to a new Cell, THE FlowConnectGame SHALL add that Cell to the Path for the selected color
4. WHEN THE Player moves to a Cell already in the current Path, THE FlowConnectGame SHALL remove all Path segments after that Cell (backtracking)
5. WHEN THE Player is carrying a color, THE FlowConnectGame SHALL visually indicate the selected color on the Player representation

### Requirement 4: 색상 연결 완료

**User Story:** 플레이어로서, 색상을 선택한 후 같은 색상의 다른 점에 도착하여 스페이스바를 누르면 연결이 완료되기를 원합니다. 이를 통해 퍼즐의 일부를 해결할 수 있습니다.

#### Acceptance Criteria

1. WHEN THE Player presses the Space key while on a ColorPoint matching the carried color and at a different position than the pickup point, THE FlowConnectGame SHALL mark the ColorPair as connected
2. WHEN a ColorPair is connected, THE FlowConnectGame SHALL preserve the Path between the two ColorPoint objects
3. WHEN a ColorPair is connected, THE FlowConnectGame SHALL set the Player state to not carrying a color
4. WHEN THE Player presses the Space key while carrying a color but not on a matching ColorPoint, THE FlowConnectGame SHALL cancel the connection and clear the Path
5. WHEN a ColorPair connection is completed or cancelled, THE FlowConnectGame SHALL update the connection count display

### Requirement 5: 경로 충돌 방지

**User Story:** 플레이어로서, 다른 색상의 경로와 겹치지 않도록 경로를 그릴 수 있기를 원합니다. 이를 통해 퍼즐의 난이도와 전략성을 유지할 수 있습니다.

#### Acceptance Criteria

1. WHEN THE Player attempts to move to a Cell with a Path of a different color, THE FlowConnectGame SHALL prevent the movement
2. WHEN THE Player moves to a Cell with a Path of the same color being drawn, THE FlowConnectGame SHALL allow the movement and perform backtracking
3. WHEN THE Player moves to an empty Cell or a Cell with the current color's Path, THE FlowConnectGame SHALL allow the movement
4. WHEN THE Player moves to a Wall Cell, THE FlowConnectGame SHALL prevent the movement
5. WHEN a movement is prevented, THE Player SHALL remain at the current Cell position

### Requirement 6: 게임 클리어 조건

**User Story:** 플레이어로서, 모든 색상 쌍을 성공적으로 연결하면 게임이 클리어되기를 원합니다. 이를 통해 퍼즐을 완료했음을 확인할 수 있습니다.

#### Acceptance Criteria

1. WHEN all ColorPair objects are marked as connected, THE FlowConnectGame SHALL trigger the game clear condition
2. WHEN the game clear condition is triggered, THE FlowConnectGame SHALL invoke the Complete method with success parameter set to true
3. WHEN a ColorPair connection status changes, THE FlowConnectGame SHALL check the clear condition in real-time
4. WHEN the clear condition is checked, THE FlowConnectGame SHALL update the UI to display the number of connected ColorPair objects
5. WHEN the game is cleared, THE FlowConnectGame SHALL display a success message to the Player

### Requirement 7: 게임 초기화

**User Story:** 플레이어로서, Q 키를 눌러 현재 진행 상황을 초기화하고 처음부터 다시 시작할 수 있기를 원합니다. 이를 통해 실수를 수정하거나 새로운 전략을 시도할 수 있습니다.

#### Acceptance Criteria

1. WHEN THE Player presses the Q key, THE FlowConnectGame SHALL clear all Path objects from all ColorPair objects
2. WHEN the game is reset, THE FlowConnectGame SHALL reset the Player position to the starting position (0, 0)
3. WHEN the game is reset, THE FlowConnectGame SHALL set the Player state to not carrying a color
4. WHEN the game is reset, THE FlowConnectGame SHALL mark all ColorPair objects as not connected
5. WHEN the game is reset, THE FlowConnectGame SHALL update all visual representations to reflect the reset state

### Requirement 8: 비주얼 렌더링

**User Story:** 플레이어로서, 게임 보드의 각 요소(빈칸, 벽, 색상 점, 경로, 플레이어)가 명확하게 구분되는 색상으로 표시되기를 원합니다. 이를 통해 게임 상태를 쉽게 파악할 수 있습니다.

#### Acceptance Criteria

1. WHEN a Cell is empty, THE FlowConnectGame SHALL render the Cell with a checkerboard pattern using two alternating colors
2. WHEN a Cell is a Wall, THE FlowConnectGame SHALL render the Cell with a distinct wall color (magenta)
3. WHEN a Cell is a ColorPoint, THE FlowConnectGame SHALL render the Cell with the color assigned to that ColorPair
4. WHEN a Cell contains a Path, THE FlowConnectGame SHALL render the Cell with the color of the Path's ColorPair
5. WHEN THE Player position changes, THE FlowConnectGame SHALL update the Player visual to the new Cell position
6. WHEN THE Player is carrying a color, THE FlowConnectGame SHALL render the Player visual with the selected color
7. WHEN THE Player is not carrying a color, THE FlowConnectGame SHALL render the Player visual with a default player color

### Requirement 9: UI 정보 표시

**User Story:** 플레이어로서, 현재 연결된 색상 수와 조작 방법을 화면에서 확인할 수 있기를 원합니다. 이를 통해 게임 진행 상황과 조작법을 쉽게 파악할 수 있습니다.

#### Acceptance Criteria

1. WHEN the game is running, THE FlowConnectGame SHALL display the number of connected ColorPair objects and total ColorPair count
2. WHEN a ColorPair connection status changes, THE FlowConnectGame SHALL update the connection count display within 0.1 seconds
3. WHEN the game starts, THE FlowConnectGame SHALL display control hints showing "Space: 선택/놓기, Q: 초기화"
4. WHEN the game is reset, THE FlowConnectGame SHALL update the hint text to indicate the reset action
5. WHEN the game is cleared, THE FlowConnectGame SHALL display a clear message in the UI
