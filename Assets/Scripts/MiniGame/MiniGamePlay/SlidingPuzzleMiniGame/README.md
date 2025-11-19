# 슬라이딩 퍼즐 미니게임

4x4 그리드 슬라이딩 퍼즐 게임입니다. 원본 이미지를 16개 조각으로 나누고, 빈 칸을 WASD로 이동시켜 퍼즐을 완성합니다.

## 게임 규칙

- 4x4 그리드에 1~15번 타일과 1개의 빈 칸이 있습니다
- **WASD** 또는 **방향키**로 빈 칸과 인접한 타일을 이동시킵니다
- 모든 타일이 정답 위치에 오면 클리어됩니다
- 제한 시간 내에 완성해야 합니다

## Unity 설정 방법

### 1. 씬에 게임 오브젝트 생성

1. Hierarchy에서 우클릭 → `Create Empty` → 이름을 `SlidingPuzzleGame`으로 변경
2. `SlidingPuzzleGame`에 `SlidingPuzzleMiniGame` 컴포넌트 추가

### 2. UI 구조 생성

1. `SlidingPuzzleGame` 하위에 Canvas 생성 (없으면)
2. Canvas 하위에 `Panel` 생성 → 이름을 `PuzzleGrid`로 변경
3. `PuzzleGrid`에 `Grid Layout Group` 컴포넌트 추가
4. Grid Layout Group 설정:
   - Constraint: **Fixed Column Count**
   - Constraint Count: **4**
   - Cell Size: 적절한 크기 (예: 100x100)
   - Spacing: 타일 간격 (예: 5, 5)

### 3. 컴포넌트 설정

`SlidingPuzzleMiniGame` 컴포넌트 Inspector에서:

- **Id**: `SlidingPuzzle` (고유 식별자)
- **Duration**: 게임 제한 시간 (초, 예: 120)
- **Puzzle Grid**: 위에서 만든 `PuzzleGrid` 오브젝트 드래그
- **Tile Sprites**: 크기를 15로 설정하고, 1~15번 타일 이미지 할당
  - 원본 이미지를 16등분한 스프라이트 15개 필요
  - 16번째 칸은 빈 칸이므로 스프라이트 불필요
- **Grid Size**: 4 (4x4 그리드)
- **Shuffle Moves**: 50 (초기 섞기 이동 횟수)
- **Empty Tile Color**: 빈 타일 색상 (기본: 반투명 회색)

### 4. 보상 설정

- **Success Reward**: 성공 시 보상 (예: 스트레스 -10, 집중력 +5)
- **Fail Reward**: 실패 시 보상 (예: 스트레스 +5)
- **Perfect Clear Reward**: 완벽한 클리어 보상 (선택사항)

## 타일 이미지 준비

1. 원본 이미지를 4x4 (16등분)로 자릅니다
2. 각 조각을 스프라이트로 임포트합니다
3. 스프라이트 이름을 `Tile_01`, `Tile_02`, ... `Tile_15`로 지정
4. Inspector에서 Tile Sprites 배열에 순서대로 할당

### 이미지 자르기 팁

- Photoshop/GIMP: 가이드를 사용해 정확히 4등분
- Unity Sprite Editor: Slice → Grid By Cell Count (4x4)
- 온라인 도구: 이미지 그리드 분할 도구 사용

## 조작 방법

- **W / ↑**: 위쪽 타일을 빈 칸으로 이동
- **S / ↓**: 아래쪽 타일을 빈 칸으로 이동
- **A / ←**: 왼쪽 타일을 빈 칸으로 이동
- **D / →**: 오른쪽 타일을 빈 칸으로 이동

## 코드 구조

### SlidingPuzzleMiniGame.cs
- 메인 게임 로직
- 타일 생성 및 초기화
- 입력 처리 (WASD)
- 타일 이동 및 교환
- 퍼즐 섞기
- 클리어 조건 체크

### SlidingPuzzleTile.cs
- 개별 타일 컴포넌트
- 타일 번호 및 정답 위치 관리
- 빈 타일 여부 체크
- 정답 위치 확인

## 확장 아이디어

- 난이도 선택 (3x3, 4x4, 5x5)
- 이동 횟수 제한 모드
- 힌트 기능 (정답 미리보기)
- 타일 이동 애니메이션
- 사운드 효과 추가
- 여러 이미지 선택 가능
