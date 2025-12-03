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


## X축 일자 돌진 패턴 (ChargeAttack)

### 개요
플레이어의 마지막 Y 위치를 기억하고, 그 높이로 화면 가장자리에서 일직선으로 돌진하는 패턴입니다.

### 프리팹 만들기

```
ChargeAttack (Prefab)
├─ RectTransform (크기: 100x50)
├─ Image (돌진 이펙트 스프라이트)
├─ Box Collider 2D (Is Trigger)
│   └─ Size: 100x50
└─ BossChargeAttack (Script)
    ├─ Charge Speed: 15
    ├─ Charge Delay: 0.5 (돌진 전 경고 시간)
    ├─ Lifetime: 5
    ├─ Warning Color: Yellow (경고 색상)
    └─ Charge Color: Red (돌진 색상)
```

### 동작 방식

1. **위치 결정**: 플레이어의 현재 Y 위치를 기억
2. **경고 단계** (0.5초): 노란색으로 깜빡이며 경고
3. **돌진 단계**: 빨간색으로 변하며 빠르게 돌진
4. **충돌 판정**: 플레이어와 충돌 시 시간 감소

### BossFightMiniGame에서 사용

#### 방법 1: 수동 스폰 (코드 수정 필요)

`BossFightMiniGame.cs`에 다음 메서드 추가:

```csharp
private void SpawnChargeAttack(bool fromLeft = true)
{
    if (chargeAttackPrefab == null || playArea == null) return;
    
    // 플레이어 Y 위치 가져오기
    float playerY = player.anchoredPosition.y;
    
    // 돌진 공격 생성
    GameObject attackObj = Instantiate(chargeAttackPrefab, playArea);
    BossChargeAttack charge = attackObj.GetComponent<BossChargeAttack>();
    
    if (charge != null)
    {
        charge.Initialize(playerY, fromLeft);
    }
    
    // 충돌 감지 설정
    BossAttackObject attackComponent = attackObj.GetComponent<BossAttackObject>();
    if (attackComponent != null)
    {
        attackComponent.timePenalty = 8f; // 맞으면 8초 감소
    }
}
```

#### 방법 2: 페이즈 패턴으로 사용 (간단)

기존 `StraightLine` 패턴을 활용하되, 스폰 위치를 플레이어 Y로 설정:

```
Phase 설정:
Attack Pattern:
- Type: StraightLine
- Line Angle: 0 (왼쪽→오른쪽) 또는 180 (오른쪽→왼쪽)
- Spawn Count: 1
- Spawn Interval: 3.0
- Move Speed: 15 (빠른 속도)
- Prefab: ChargeAttack
- Time Penalty: 8
```

### 사용 예시

#### 보스 Phase 3 - 돌진 공격

```
Phase 3 (40~60초):
Attack Pattern 1:
- Type: CustomBehavior (또는 StraightLine)
- Spawn Interval: 4.0
- Prefab: ChargeAttack
- 설명: 3초마다 플레이어 높이로 돌진

동작:
1. 플레이어가 (100, 200) 위치에 있음
2. 화면 왼쪽 (-500, 200)에서 ChargeAttack 스폰
3. 0.5초 동안 노란색으로 깜빡임 (경고)
4. 빨간색으로 변하며 오른쪽으로 빠르게 돌진
5. 플레이어가 피하지 못하면 8초 감소
```

#### 복합 패턴 - 양쪽 돌진

```
Phase 5 (80~100초):
Attack Pattern 1: ChargeAttack (왼쪽에서)
- Spawn Interval: 3.0
- From Left: true

Attack Pattern 2: ChargeAttack (오른쪽에서)
- Spawn Interval: 3.5
- From Left: false

결과: 양쪽에서 번갈아 돌진하는 패턴
```

### 난이도 조절

#### 쉬움
```
Charge Speed: 10
Charge Delay: 1.0 (긴 경고 시간)
Spawn Interval: 5.0
Time Penalty: 5
```

#### 보통
```
Charge Speed: 15
Charge Delay: 0.5
Spawn Interval: 3.0
Time Penalty: 8
```

#### 어려움
```
Charge Speed: 20
Charge Delay: 0.3 (짧은 경고)
Spawn Interval: 2.0
Time Penalty: 10
```

### 시각 효과 개선

#### 1. Trail Renderer 추가
```
ChargeAttack Prefab에 추가:
- Trail Renderer
  - Time: 0.5
  - Width: 1 → 0
  - Color: Yellow → Red
```

#### 2. 파티클 효과
```
- Particle System (돌진 시작 시)
  - Duration: 0.5
  - Start Speed: 5
  - Start Color: Red
```

#### 3. 사운드 효과
```
- Audio Source
  - Clip: ChargeWarning.wav (경고음)
  - Clip: ChargeImpact.wav (돌진음)
```

### 변형 패턴

#### 1. 더블 돌진
```csharp
// 위아래 동시 돌진
SpawnChargeAttack(playerY + 100, true);
SpawnChargeAttack(playerY - 100, true);
```

#### 2. 지그재그 돌진
```csharp
// 0.5초 간격으로 높이를 바꿔가며 돌진
StartCoroutine(ZigZagCharge());

IEnumerator ZigZagCharge()
{
    for (int i = 0; i < 5; i++)
    {
        float y = Random.Range(-200f, 200f);
        SpawnChargeAttack(y, i % 2 == 0);
        yield return new WaitForSeconds(0.5f);
    }
}
```

#### 3. 추적 돌진
```csharp
// 돌진 직전에 플레이어 위치 재확인
// BossChargeAttack.cs의 chargeDelay 동안 플레이어 추적
```

### 디버깅 팁

1. **경고 시간 확인**: Charge Delay를 길게 설정해서 테스트
2. **속도 조절**: Charge Speed를 낮춰서 패턴 확인
3. **충돌 범위**: Collider 크기를 Scene 뷰에서 확인
4. **로그 확인**: Debug.Log로 스폰 위치와 플레이어 위치 출력

```csharp
Debug.Log($"Charge spawned at Y: {playerY}, Player at: {player.anchoredPosition}");
```

### 완성된 보스 예시

```
물리 보스 - Phase 4 "레일건 돌진"

Attack Pattern:
- Type: ChargeAttack
- Spawn Interval: 2.5
- Charge Speed: 18
- Charge Delay: 0.4
- Prefab: LaserCharge (레이저 빔 스프라이트)
- Time Penalty: 10

연출:
- 경고: 파란색 레이저 조준선
- 돌진: 밝은 흰색 레이저 빔
- 효과: 전기 파티클 + 충격파 사운드
```
