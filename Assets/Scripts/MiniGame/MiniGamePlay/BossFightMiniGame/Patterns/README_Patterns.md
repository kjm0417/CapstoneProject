# 보스 패턴 시스템 가이드

## 개요

각 보스 패턴은 독립적인 스크립트로 구현되어 있으며, 필요한 설정만 Inspector에서 조정할 수 있습니다.

## 패턴 구조

```
BossPatternBase (추상 클래스)
├─ ChargePattern (돌진 패턴)
├─ ProjectilePattern (발사체 패턴)
├─ CircularPattern (원형 패턴)
├─ TrackingPattern (추적 패턴)
└─ ... (추가 패턴)
```

## 사용 방법

### 1. 보스 GameObject에 패턴 추가

```
BossObject (GameObject)
├─ BossFightMiniGame (Script)
├─ ChargePattern (Script)
├─ ProjectilePattern (Script)
└─ CircularPattern (Script)
```

### 2. 각 패턴 설정

#### ChargePattern (돌진 패턴)

```
Inspector 설정:
- Attack Prefab: ChargeAttack 프리팹
- Spawn Interval: 3.0 (3초마다 실행)
- Time Penalty: 8 (맞으면 8초 감소)
- Charge Speed: 15 (돌진 속도)
- Charge Delay: 0.5 (경고 시간)
- Alternate Direction: true (좌우 번갈아)
```

**특징:**
- 플레이어의 Y 위치를 기억
- 화면 가장자리에서 일직선 돌진
- 경고 후 돌진

#### ProjectilePattern (발사체 패턴)

```
Inspector 설정:
- Attack Prefab: Projectile 프리팹
- Spawn Interval: 2.0
- Time Penalty: 5
- Spawn Count: 3 (동시에 3개)
- Move Speed: 5 (낙하 속도)
- Random X: true (랜덤 위치)
```

**특징:**
- 화면 위에서 아래로 낙하
- 여러 개 동시 스폰 가능
- 랜덤 또는 균등 배치

#### CircularPattern (원형 패턴)

```
Inspector 설정:
- Attack Prefab: Bullet 프리팹
- Spawn Interval: 4.0
- Time Penalty: 4
- Bullet Count: 8 (8방향)
- Move Speed: 5
- Center Offset: (0, 0) (중앙 위치)
```

**특징:**
- 중앙에서 사방으로 퍼짐
- 방향 개수 조절 가능
- 중심 위치 조정 가능

#### TrackingPattern (추적 패턴)

```
Inspector 설정:
- Attack Prefab: TrackingHand 프리팹
- Spawn Interval: 5.0
- Time Penalty: 8
- Tracking Speed: 3 (추적 속도)
- Tracking Duration: 5 (추적 시간)
- Spawn Count: 1
```

**특징:**
- 화면 가장자리에서 스폰
- 일정 시간 플레이어 추적
- 추적 종료 후 직진

## 페이즈별 패턴 활성화

### BossFightMiniGame에서 제어

```csharp
public class BossFightMiniGame : MiniGameBase
{
    [Header("패턴 참조")]
    [SerializeField] private ChargePattern chargePattern;
    [SerializeField] private ProjectilePattern projectilePattern;
    [SerializeField] private CircularPattern circularPattern;
    
    private void OnPhaseChanged(int newPhase)
    {
        // 모든 패턴 중지
        StopAllPatterns();
        
        // 페이즈별 패턴 활성화
        switch (newPhase)
        {
            case 1:
                projectilePattern?.StartPattern();
                break;
                
            case 2:
                projectilePattern?.StartPattern();
                circularPattern?.StartPattern();
                break;
                
            case 3:
                chargePattern?.StartPattern();
                break;
                
            case 4:
                chargePattern?.StartPattern();
                projectilePattern?.StartPattern();
                break;
        }
    }
    
    private void StopAllPatterns()
    {
        chargePattern?.StopPattern();
        projectilePattern?.StopPattern();
        circularPattern?.StopPattern();
    }
}
```

## 새로운 패턴 만들기

### 1. BossPatternBase 상속

```csharp
using UnityEngine;

public class MyCustomPattern : BossPatternBase
{
    [Header("커스텀 설정")]
    [SerializeField] private float customValue = 10f;
    
    protected override void ExecutePattern()
    {
        // 패턴 로직 구현
        Vector2 playerPos = GetPlayerPosition();
        Vector2 playAreaSize = GetPlayAreaSize();
        
        // 공격 생성
        GameObject attackObj = SpawnAttack(playerPos);
        
        // 추가 로직...
    }
}
```

### 2. 보스에 추가

```
1. BossObject에 MyCustomPattern 컴포넌트 추가
2. Inspector에서 설정 조정
3. BossFightMiniGame에서 참조 및 활성화
```

## 프리팹 요구사항

### 공격 프리팹 구조

```
AttackPrefab
├─ RectTransform
├─ Image (스프라이트)
├─ Collider2D (Is Trigger)
├─ Rigidbody2D (Kinematic 또는 Dynamic)
└─ BossAttackObject (Script) - 충돌 처리용
```

### BossAttackObject 스크립트

```csharp
public class BossAttackObject : MonoBehaviour
{
    public float timePenalty = 5f;
    public float lifetime = 10f;
    
    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 플레이어 피격 처리
            EventBus.Publish("BossFight.PlayerHit", timePenalty);
            Destroy(gameObject);
        }
    }
}
```

## 예시: 화학 보스

```
ChemistryBoss (GameObject)
├─ BossFightMiniGame
├─ ProjectilePattern (Phase 1)
│   ├─ Attack Prefab: AcidDrop
│   ├─ Spawn Interval: 2.0
│   └─ Spawn Count: 3
├─ CircularPattern (Phase 2)
│   ├─ Attack Prefab: PoisonBall
│   ├─ Spawn Interval: 4.0
│   └─ Bullet Count: 8
└─ ChargePattern (Phase 3)
    ├─ Attack Prefab: ChemicalBeam
    ├─ Spawn Interval: 3.0
    └─ Charge Speed: 15
```

## 장점

1. **독립성**: 각 패턴이 독립적으로 동작
2. **재사용성**: 여러 보스에서 같은 패턴 사용 가능
3. **확장성**: 새 패턴 추가가 쉬움
4. **설정 간편**: Inspector에서 필요한 값만 설정
5. **유지보수**: 패턴별로 코드 분리되어 관리 용이

## 디버깅

### 패턴 활성화 확인

```csharp
private void Update()
{
    if (Input.GetKeyDown(KeyCode.Alpha1))
    {
        chargePattern?.StartPattern();
        Debug.Log("Charge Pattern Started");
    }
    
    if (Input.GetKeyDown(KeyCode.Alpha2))
    {
        chargePattern?.StopPattern();
        Debug.Log("Charge Pattern Stopped");
    }
}
```

### 스폰 위치 시각화

```csharp
private void OnDrawGizmos()
{
    if (playArea == null) return;
    
    Gizmos.color = Color.red;
    Vector2 size = playArea.rect.size;
    Gizmos.DrawWireCube(playArea.position, size);
}
```

## 성능 최적화

1. **Object Pooling**: 자주 스폰되는 공격은 풀링 사용
2. **Lifetime 설정**: 화면 밖 공격은 자동 제거
3. **Collider 최적화**: 필요한 크기만 사용
4. **Update 최소화**: 필요할 때만 Update 실행
