# 여러 보스 만들기 가이드

화학 보스, 물리 보스, 생명 보스 등 여러 보스를 만드는 방법입니다.

## 기본 구조

각 보스는 **별도의 프리팹**으로 만들고, 같은 `BossFightMiniGame` 스크립트를 사용합니다.

```
보스 시스템
├─ ChemistryBoss (화학 보스 프리팹)
│   ├─ BossFightMiniGame 컴포넌트
│   ├─ 화학 테마 UI
│   └─ 화학 공격 패턴들
│
├─ PhysicsBoss (물리 보스 프리팹)
│   ├─ BossFightMiniGame 컴포넌트
│   ├─ 물리 테마 UI
│   └─ 물리 공격 패턴들
│
└─ BiologyBoss (생명 보스 프리팹)
    ├─ BossFightMiniGame 컴포넌트
    ├─ 생명 테마 UI
    └─ 생명 공격 패턴들
```

## 확장된 공격 패턴

### 1. TrackingHand (추적하는 손)
플레이어를 계속 바라보며 따라옵니다.

**설정 예시:**
```
Pattern Type: TrackingHand
Spawn Interval: 3.0
Spawn Count: 1
Tracking Speed: 3.0 (추적 속도)
Tracking Duration: 5.0 (추적 지속 시간)
Lifetime: 8.0
Time Penalty: 8
```

**동작:**
- 화면 가장자리에서 스폰
- 5초 동안 플레이어를 추적
- 이후 직선 이동

### 2. StraightLine (일직선 공격)
화면을 가로질러 일직선으로 지나갑니다.

**설정 예시:**
```
Pattern Type: StraightLine
Spawn Interval: 2.0
Spawn Count: 3 (동시에 3개)
Line Angle: 0 (오른쪽으로)
Move Speed: 8.0
Lifetime: 5.0
Time Penalty: 5
```

**각도 설정:**
- 0° = 오른쪽 →
- 90° = 위 ↑
- 180° = 왼쪽 ←
- 270° = 아래 ↓
- 45° = 대각선 ↗

### 3. CircularPattern (원형 패턴)
중앙에서 사방으로 퍼집니다.

**설정 예시:**
```
Pattern Type: CircularPattern
Spawn Interval: 4.0
Circular Count: 8 (8방향)
Move Speed: 5.0
Lifetime: 5.0
Time Penalty: 4
```

### 4. SpiralPattern (나선형 패턴)
회전하며 사방으로 퍼집니다.

**설정 예시:**
```
Pattern Type: SpiralPattern
Spawn Interval: 0.5 (빠르게 스폰)
Circular Count: 6
Spiral Rotation Speed: 90 (회전 속도)
Move Speed: 4.0
Lifetime: 5.0
Time Penalty: 3
```

### 5. CustomBehavior (커스텀 동작)
프리팹에 직접 스크립트를 작성하여 복잡한 동작 구현.

## 보스별 프리팹 만들기

### 화학 보스 (ChemistryBoss)

#### 1. 프리팹 구조
```
ChemistryBoss (GameObject)
├─ BossFightMiniGame (Script)
├─ Background (Image) - 녹색 테마
├─ PlayArea (RectTransform)
│   └─ Player (Image + Collider2D)
├─ PhaseText (Text)
└─ BossVisual (Image) - 화학 보스 이미지
```

#### 2. 페이즈 설정 (120초, 6페이즈)

**Phase 1 (0~20초) - 산성 비**
```
Attack Pattern 1:
- Type: Projectile
- Spawn Interval: 2.0
- Spawn Count: 2
- Move Speed: 5
- Prefab: GreenDroplet (녹색 물방울)
```

**Phase 2 (20~40초) - 독가스**
```
Attack Pattern 1:
- Type: GroundAOE
- Spawn Interval: 3.0
- Spawn Count: 3
- Lifetime: 4.0
- Prefab: PoisonCloud (독구름)
```

**Phase 3 (40~60초) - 화학 반응**
```
Attack Pattern 1:
- Type: CircularPattern
- Spawn Interval: 5.0
- Circular Count: 8
- Prefab: ChemicalBall (화학 구체)
```

**Phase 4 (60~80초) - 추적 촉수**
```
Attack Pattern 1:
- Type: TrackingHand
- Spawn Interval: 4.0
- Tracking Speed: 3.5
- Tracking Duration: 6.0
- Prefab: ChemicalTentacle (화학 촉수)
```

**Phase 5 (80~100초) - 복합 공격**
```
Attack Pattern 1: Projectile (산성 비)
Attack Pattern 2: GroundAOE (독가스)
```

**Phase 6 (100~120초) - 최종 패턴**
```
Attack Pattern 1: SpiralPattern
- Circular Count: 12
- Spiral Rotation Speed: 120
Attack Pattern 2: TrackingHand
```

#### 3. Inspector 설정
```
BossFightMiniGame:
- Id: "BossFight_Chemistry"
- Encounter Type: BossFight
- Duration: 120
- Phases: 6개 설정
- Player Move Speed: 10
```

### 물리 보스 (PhysicsBoss)

#### 페이즈 설정 (120초, 6페이즈)

**Phase 1 - 중력파**
```
Attack Pattern:
- Type: StraightLine
- Line Angle: 270 (아래로)
- Spawn Count: 5
- Spawn Interval: 2.0
```

**Phase 2 - 전자기장**
```
Attack Pattern:
- Type: CircularPattern
- Circular Count: 12
- Move Speed: 6
```

**Phase 3 - 레이저**
```
Attack Pattern:
- Type: StraightLine
- Line Angle: 0, 90, 180, 270 (4방향)
- Move Speed: 15 (빠름)
```

**Phase 4 - 자기장 손**
```
Attack Pattern:
- Type: TrackingHand
- Tracking Speed: 4.0
- Prefab: MagneticHand
```

**Phase 5~6 - 복합 패턴**

### 생명 보스 (BiologyBoss)

#### 페이즈 설정 (120초, 6페이즈)

**Phase 1 - 세포 분열**
```
Attack Pattern:
- Type: RandomSpawn
- Spawn Count: 4
- Spawn Interval: 2.5
```

**Phase 2 - DNA 나선**
```
Attack Pattern:
- Type: SpiralPattern
- Circular Count: 2
- Spiral Rotation Speed: 60
```

**Phase 3 - 바이러스**
```
Attack Pattern:
- Type: TrackingHand
- Spawn Count: 2
- Tracking Duration: 8.0
```

**Phase 4~6 - 복합 패턴**

## MiniGameManager에 등록

```
MiniGameManager Inspector:

Games 리스트:
1. Id: "BossFight_Chemistry"
   Prefab: ChemistryBoss

2. Id: "BossFight_Physics"
   Prefab: PhysicsBoss

3. Id: "BossFight_Biology"
   Prefab: BiologyBoss
```

## 보스 시작하기

```csharp
// 화학 보스
EventBus.Publish<string>("MiniGame.StartById", "BossFight_Chemistry");

// 물리 보스
EventBus.Publish<string>("MiniGame.StartById", "BossFight_Physics");

// 생명 보스
EventBus.Publish<string>("MiniGame.StartById", "BossFight_Biology");
```

## 공격 프리팹 만들기

### 추적하는 손 (TrackingHand)

```
HandAttack (Prefab)
├─ Image (손 모양 스프라이트)
├─ Circle Collider 2D (Is Trigger)
├─ BossAttackObject (Script)
└─ Trail Renderer (선택, 궤적 표시)
```

### 일직선 공격 (StraightLine)

```
LaserBeam (Prefab)
├─ Image (레이저 빔)
├─ Box Collider 2D (Is Trigger)
└─ BossAttackObject (Script)
```

## 테마별 색상 추천

### 화학 보스
- 배경: 녹색 (#00FF00)
- 공격: 독성 녹색, 노란색

### 물리 보스
- 배경: 파란색 (#0080FF)
- 공격: 전기 파란색, 흰색

### 생명 보스
- 배경: 분홍색 (#FF80C0)
- 공격: 빨간색, 보라색

## 난이도 밸런싱

### 쉬운 보스
- Spawn Interval: 3~5초
- Move Speed: 3~5
- Tracking Speed: 2~3
- Time Penalty: 3~5초

### 보통 보스
- Spawn Interval: 2~3초
- Move Speed: 5~7
- Tracking Speed: 3~4
- Time Penalty: 5~7초

### 어려운 보스
- Spawn Interval: 1~2초
- Move Speed: 7~10
- Tracking Speed: 4~5
- Time Penalty: 7~10초

## 팁

1. **페이즈 전환 연출**: Phase Start Text로 "Phase 2 시작!" 표시
2. **색상 변화**: Phase Color로 페이즈마다 다른 분위기
3. **패턴 조합**: 한 페이즈에 여러 Attack Pattern 추가 가능
4. **점진적 난이도**: 초반 페이즈는 쉽게, 후반으로 갈수록 어렵게
5. **테마 일관성**: 보스마다 고유한 색상과 공격 스타일 유지
