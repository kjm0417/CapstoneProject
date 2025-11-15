# Implementation Plan

- [x] 1. 데이터 구조 및 기본 클래스 생성




  - Cell, ColorPair, FlowPlayer 데이터 클래스 작성
  - CellType enum 정의
  - FlowLevelData ScriptableObject 생성
  - _Requirements: 1.1, 1.2_

- [ ] 2. FlowConnectMiniGame 메인 컨트롤러 기본 구조 작성
  - MiniGameBase 상속
  - SerializeField 변수 선언 (boardContainer, cellPrefab, playerVisual, UI 요소, 색상 배열)
  - 내부 데이터 변수 선언 (board, cellObjects, colorPairs, player)
  - _Requirements: 1.1, 1.4_

- [ ] 3. 보드 초기화 시스템 구현
- [ ] 3.1 InitializeBoard 메서드 구현
  - 15x15 Cell 배열 생성
  - GameObject 배열 초기화
  - GridLayoutGroup 설정 (셀 크기, 간격, 제약 조건)
  - 225개 셀 GameObject 생성 및 체커보드 패턴 적용
  - _Requirements: 1.1, 1.2, 8.1_

- [ ] 3.2 LoadLevelData 메서드 구현
  - FlowLevelData에서 벽 위치 로드 및 배치
  - ColorPair 데이터 로드 및 색상 점 배치
  - colorPairs 리스트 초기화
  - _Requirements: 1.3, 1.4_

- [ ] 3.3 CreateDefaultLevel 메서드 구현
  - 레벨 데이터가 없을 때 기본 3색 레벨 생성
  - 초록/빨강/파랑 색상 점 배치
  - _Requirements: 1.5_

- [ ] 4. 플레이어 시스템 구현
- [ ] 4.1 SpawnPlayer 메서드 구현
  - FlowPlayer 인스턴스 생성 및 초기화
  - 시작 위치 (0, 0) 설정
  - board[0, 0].hasPlayer = true 설정
  - _Requirements: 2.6_

- [ ] 4.2 UpdatePlayerVisualPosition 메서드 구현
  - 플레이어 비주얼을 현재 셀 위치로 이동
  - 색상 들고 있을 때 비주얼 색상 변경
  - _Requirements: 2.6, 3.5_

- [ ] 5. 입력 처리 시스템 구현
- [ ] 5.1 HandleInput 메서드 구현
  - WASD 키 입력 감지 및 방향 벡터 생성
  - TryMovePlayer 호출
  - Space 키 입력 감지 및 HandleSpacePress 호출
  - Q 키 입력 감지 및 ResetGame 호출
  - _Requirements: 2.1, 2.2, 2.3, 2.4, 3.1, 7.1_

- [ ] 5.2 TryMovePlayer 메서드 구현
  - 목표 위치 계산 및 범위 체크
  - 벽 충돌 체크
  - 다른 색상 경로 충돌 체크
  - 이동 가능 시 플레이어 위치 업데이트
  - 색상 들고 있으면 경로에 추가
  - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 3.3_

- [ ] 5.3 HandleSpacePress 메서드 구현
  - Case 1: 색상 선택 (색상 점에서 Space 누름)
  - Case 2: 연결 완료 (같은 색상 점에 도착해서 Space 누름)
  - Case 3: 연결 취소 (다른 곳에서 Space 누름)
  - _Requirements: 3.1, 3.2, 4.1, 4.3, 4.4_

- [ ] 6. 경로 관리 시스템 구현
- [ ] 6.1 AddToPath 메서드 구현
  - 경로에 새 위치 추가
  - 백트래킹 감지 (이미 경로에 있는 셀로 이동)
  - 백트래킹 시 이후 경로 제거
  - Cell의 pathColorId 업데이트
  - _Requirements: 3.3, 3.4_

- [ ] 6.2 ClearPath 메서드 구현
  - 특정 색상의 모든 경로 제거
  - Cell의 pathColorId 초기화
  - ColorPair의 path 리스트 클리어
  - isConnected 플래그 false로 설정
  - _Requirements: 3.2, 4.4_

- [ ] 6.3 CompleteConnection 메서드 구현
  - ColorPair의 isConnected 플래그 true로 설정
  - 연결 카운트 UI 업데이트
  - _Requirements: 4.1, 4.2, 4.5_

- [ ] 7. 충돌 감지 및 검증 시스템 구현
- [ ] 7.1 IsInBounds 메서드 구현
  - 위치가 0-14 범위 내인지 확인
  - _Requirements: 2.1, 2.2, 2.3, 2.4_

- [ ] 7.2 CanMoveTo 메서드 구현
  - 범위 체크
  - 벽 체크
  - 다른 색상 경로 충돌 체크
  - _Requirements: 2.5, 5.1, 5.2, 5.3, 5.4, 5.5_

- [ ] 7.3 CanSelectColor 메서드 구현
  - 현재 위치가 색상 점인지 확인
  - _Requirements: 3.1_

- [ ] 7.4 CanCompleteConnection 메서드 구현
  - 같은 색상 점인지 확인
  - 시작점과 다른 위치인지 확인
  - _Requirements: 4.1_

- [ ] 8. 비주얼 렌더링 시스템 구현
- [ ] 8.1 UpdateVisuals 메서드 구현
  - 모든 셀 순회하며 색상 업데이트
  - 우선순위: 벽 > 색상 점 > 경로 > 빈칸
  - 체커보드 패턴 적용
  - UpdatePlayerVisualPosition 호출
  - _Requirements: 8.1, 8.2, 8.3, 8.4, 8.5, 8.6, 8.7_

- [ ] 8.2 GetColor 헬퍼 메서드 구현
  - colorId를 받아 pathColors 배열에서 색상 반환
  - _Requirements: 8.3, 8.4_

- [ ] 9. 게임 상태 관리 구현
- [ ] 9.1 CheckClearRealtime 오버라이드 구현
  - 모든 ColorPair의 isConnected 확인
  - 연결 카운트 UI 업데이트
  - _Requirements: 6.1, 6.2, 6.3, 6.4_

- [ ] 9.2 ResetGame 메서드 구현
  - 모든 경로 제거
  - 플레이어 위치 (0, 0)으로 초기화
  - 플레이어 상태 초기화 (색상 선택 해제)
  - 모든 ColorPair isConnected false로 설정
  - 비주얼 업데이트
  - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5_

- [ ] 10. UI 시스템 구현
- [ ] 10.1 UpdateScoreUI 메서드 구현
  - 연결된 색상 수 / 전체 색상 수 표시
  - "연결: X / 3" 형식
  - _Requirements: 9.1, 9.2_

- [ ] 10.2 UpdateHintUI 메서드 구현
  - 조작 방법 표시 ("Space: 선택/놓기, Q: 초기화")
  - 게임 상태에 따른 힌트 업데이트
  - _Requirements: 9.3, 9.4_

- [ ] 10.3 게임 클리어 메시지 표시
  - CheckClearRealtime에서 클리어 시 메시지 표시
  - _Requirements: 6.5, 9.5_

- [ ] 11. MiniGameBase 통합
- [ ] 11.1 OnStartHook 오버라이드 구현
  - InitializeBoard 호출
  - SpawnPlayer 호출
  - UpdateVisuals 호출
  - UI 초기화
  - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5_

- [ ] 11.2 OnUpdateHook 오버라이드 구현
  - HandleInput 호출
  - UpdateVisuals 호출 (상태 변경 시에만)
  - _Requirements: 2.1, 2.2, 2.3, 2.4, 3.1, 7.1_

- [ ] 11.3 OnCleanupHook 오버라이드 구현
  - 생성된 GameObject 정리
  - 데이터 초기화
  - _Requirements: 1.1_

- [ ] 12. 레벨 데이터 ScriptableObject 생성
- [ ] 12.1 FlowLevelData 클래스 작성
  - gridSize, walls, colorPairs 필드 정의
  - CreateAssetMenu 어트리뷰트 추가
  - _Requirements: 1.3, 1.4_

- [ ] 12.2 ColorPairData Serializable 클래스 작성
  - startPos, endPos 필드 정의
  - _Requirements: 1.4_

- [ ] 12.3 기본 레벨 데이터 에셋 생성
  - Unity 에디터에서 FlowLevelData 에셋 생성
  - 벽 위치 설정
  - 3개 색상 쌍 위치 설정
  - _Requirements: 1.3, 1.4_

- [ ] 13. 프리팹 및 씬 설정
- [ ] 13.1 CellPrefab 생성
  - GameObject + RectTransform + Image
  - 크기 50x50, Pivot (0.5, 0.5)
  - Raycast Target false
  - _Requirements: 1.2, 8.1_

- [ ] 13.2 FlowConnectMiniGame 씬 구조 생성
  - Canvas (World Space or Screen Space)
  - GameRoot, BoardContainer, PlayerVisual, UI 요소
  - GridLayoutGroup 컴포넌트 설정
  - _Requirements: 1.1, 1.2_

- [ ] 13.3 SerializeField 연결
  - Inspector에서 모든 SerializeField 연결
  - 색상 배열 설정 (초록, 빨강, 파랑)
  - _Requirements: 1.1, 8.3, 8.4_

- [ ] 14. 디버그 및 로깅 추가
- [ ] 14.1 주요 이벤트 로깅
  - 색상 선택/해제 로그
  - 연결 완료 로그
  - 게임 초기화 로그
  - 이동 실패 로그 (벽, 충돌)
  - _Requirements: 모든 요구사항_

- [ ] 14.2 디버그 시각화
  - Gizmos로 그리드 라인 표시 (선택 사항)
  - 경로 디버그 정보 표시
  - _Requirements: 8.1, 8.2, 8.3, 8.4_
