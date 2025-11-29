# 보스전 미니게임

시간 기반 페이즈 전환 보스전입니다. 플레이어는 공격을 피하며 제한 시간 동안 생존해야 합니다.

## 게임 구조

```
보스전 (120초)
├─ Phase 1 (0~20초)   - 쉬운 패턴
├─ Phase 2 (20~40초)  - 중간 난이도
├─ Phase 3 (40~60초)  - 어려운 패턴
├─ Phase 4 (60~80초)  - 복합 패턴
├─ Phase 5 (80~100초) - 고난이도
└─ Phase 6 (100~120초) - 최종 페이즈
```

## 게임 규칙

- **목표**: 모든 페이즈를 버티고 생존
- **조작**: WASD 또는 방향키로 이동
- **피격 시**: 남은 시간 감소 (패널티)
- **클리어**: 마지막 페이즈 완료 시

## 파일 구조

1. **BossPhase.cs** - 페이즈 데이터 (지속 시간, 공격 패턴)
2. **BossAttackPattern.cs** - 공격 패턴 데이터 (타입, 스폰 간격)
3. **BossAttackObject.cs** - 공격 오브젝트 (이동, 충돌 처리)
4. **BossFightMiniGame.cs** - 메인 게임 로직

## Unity 설정 방법

### 1. 보스전 게임 오브젝트 생성

1. Hierarchy에서 `Create Empty` → 이름: `BossFightGame`
2. `BossFightMiniGame` 컴포넌트 추가
3. Inspector 설정:
   - **Id**: "BossFight_Dragon"
   - **Encounter Type**: `BossFight` ✅
   - **Duration**: 120 (전체 게임 시간)

### 2. UI 구조 생성

```
Canvas
└─ BossFightPanel (PopupFadeWorld 컴포넌트)
    ├─ Background (Image) - 배경
    ├─ PlayArea (RectTransform) - 플레이 영역
    │   └─ Player (Image) - 플레이어
    ├─ PhaseText (Text) - 페이즈 표시
    └─ TimeText (Text) - 남은 시간 표시
```

### 3. 플레이어 설정

1. `PlayArea` 하위에 `Player` 오브젝트 생성
2. Image 컴포넌트 추가 (플레이어 스프라이트)
3. Tag를 `Player`로 설정 ✅
4. Collider2D 추가 (Circle Collider 2D 권장)
5. Rigidbody2D 추가 (Kinematic 체크)

### 4. 공격 프리팹 만들기

#### 예시: 투사체 공격

1. `Create Empty` → 이름: `Projectile_Attack`
2. Image 컴포넌트 추가 (빨간 원 등)
3. Circle Collider 2D 추가
   - Is Trigger: ✅ 체크
4. `BossAttackObject` 컴포넌트 추가
5. Prefab으로 저장

### 5. 페이즈 설정

`BossFightMiniGame` 컴포넌트 Inspector에서:

#### Phases 배열 (6개 페이즈 예시)

**Phase 1 (0~20초)**
- Phase Name: "Phase 1 - 시작"
- Duration: 20
- Phase Start Text: "보스전 시작!"
- Phase Color: 흰색
- Attack Patterns:
  - Pattern Type: Projectile
  - Spawn Interval: 2.0
  - Spawn Count: 1
  - Attack Prefab: Projectile_Attack
  - Move Speed: 5
  - Lifetime: 5
  - Time Penalty: 3

**Phase 2 (20~40초)**
- Phase Name: "Phase 2 - 강화"
- Duration: 20
- Phase Start Text: "패턴 강화!"
- Phase Color: 노란색
- Attack Patterns:
  - Pattern Type: Projectile
  - Spawn Interval: 1.5
  - Spawn Count: 2
  - Move Speed: 6
  - Time Penalty: 4

**Phase 3~6**: 점점 어렵게 설정

### 6. 컴포넌트 연결

`BossFightMiniGame` Inspector:
- **Player**: PlayArea/Player 드래그
- **Player Move Speed**: 10
- **Play Area**: PlayArea RectTransform 드래그
- **Phase Text**: PhaseText 드래그
- **Background Image**: Background Image 드래그

## 공격 패턴 타입

### Projectile (투사체)
- 위에서 아래로 떨어지는 공격
- 간단하고 예측 가능

### GroundAOE (바닥 장판)
- 특정 위치에 생성되어 정지
- 범위 회피 필요

### WavePattern (파동)
- 좌우에서 플레이어를 향해 이동
- 유도 공격

### RandomSpawn (랜덤)
- 랜덤 위치에서 랜덤 방향으로 이동
- 예측 불가

## 난이도 조절 방법

### 쉽게 만들기
- Spawn Interval 증가 (3~5초)
- Spawn Count 감소 (1개)
- Move Speed 감소 (3~4)
- Time Penalty 감소 (2~3초)

### 어렵게 만들기
- Spawn Interval 감소 (0.5~1초)
- Spawn Count 증가 (3~5개)
- Move Speed 증가 (8~10)
- Time Penalty 증가 (5~10초)

## 페이즈 전환 예시 (120초 게임)

```csharp
Phase 1: 0~20초   - 워밍업 (느린 투사체 1개)
Phase 2: 20~40초  - 증가 (투사체 2개)
Phase 3: 40~60초  - 복합 (투사체 + 장판)
Phase 4: 60~80초  - 고난이도 (파동 패턴)
Phase 5: 80~100초 - 혼돈 (모든 패턴 혼합)
Phase 6: 100~120초 - 최종 (빠르고 많은 공격)
```

## 보상 설정

Inspector에서:
- **Success Reward**: 스트레스 -20, 집중력 +10
- **Fail Reward**: 스트레스 +10
- **Perfect Clear Reward**: 추가 보상 (한 번도 안 맞음)

## 테스트 방법

1. 플레이 모드 진입
2. EventBus로 보스전 시작:
   ```csharp
   EventBus.Publish<string>("MiniGame.StartById", "BossFight_Dragon");
   ```
3. WASD로 이동하며 공격 회피
4. 페이즈 전환 확인
5. 120초 생존 시 클리어

## 확장 아이디어

- 보스 체력바 추가 (시각적 효과)
- 페이즈별 배경 음악 변경
- 특수 스킬 (무적, 대시 등)
- 보스 애니메이션 추가
- 페이즈 전환 컷신
- 공격 패턴 경고 표시
- 콤보 시스템 (연속 회피)
