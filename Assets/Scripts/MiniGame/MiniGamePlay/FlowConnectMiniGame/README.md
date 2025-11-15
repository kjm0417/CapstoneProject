# Flow Connect 미니게임

15x15 그리드에서 같은 색상의 시작점과 끝점을 경로로 연결하는 퍼즐 게임

## 조작법
- **WASD**: 플레이어 이동
- **Space**: 색상 선택 / 연결 완료 / 취소
- **Q**: 게임 리셋

## Unity 설정

### 1. 씬 구성
1. 빈 GameObject 생성 → `FlowConnectMiniGame`
2. `FlowConnectSystem` 컴포넌트 추가

### 2. Canvas 하위 요소
- `GridRoot` (RectTransform + Grid Layout Group) - 그리드 셀 부모
  - Grid Layout Group 설정:
    - Constraint: Fixed Column Count = 15
    - Cell Size: 40x40
    - Spacing: 2x2
    - Start Corner: Upper Left
    - Start Axis: Horizontal
- `StatusText` (Text) - 연결 상태 표시

### 3. 셀 프리팹
1. Canvas 하위에 Image 생성 → `FlowConnectCell`
2. 크기: 40x40
3. 프리팹으로 저장

### 4. Inspector 설정
- Grid Root: GridRoot 할당 (Grid Layout Group 컴포넌트 필수)
- Cell Prefab: 셀 프리팹 할당
- Cell Size: 40 (Grid Layout과 동일하게)
- Cell Spacing: 2 (Grid Layout과 동일하게)
- Player Highlight Color: 플레이어 위치 표시 색상 (기본: 노란색 반투명)
- Status Text: StatusText 할당

### 5. 플레이어 표시 방식
- 별도의 PlayerVisual 오브젝트 없음
- 플레이어가 있는 셀의 색상을 변경하여 표시
- 색상을 들고 있지 않을 때: 노란색 하이라이트
- 색상을 들고 있을 때: 해당 색상으로 표시

## 게임 플레이
1. 색상 점에서 Space로 색상 선택
2. WASD로 이동하면 경로 자동 생성
3. 같은 색상 끝점에서 Space로 연결 완료
4. 모든 색상 쌍(3개) 연결 시 클리어

## 스크립트
- **FlowConnectSystem.cs**: 메인 게임 로직
- **FlowConnectCell.cs**: 셀 데이터 구조
- **FlowConnectLevelData.cs**: 레벨 데이터 ScriptableObject
