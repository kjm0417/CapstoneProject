# MiniGameBase 수정 사항 및 사용 가이드

## 주요 변경 사항

### 1. 완벽한 클리어(◇) 시스템 추가
- **새로운 필드**: `_perfectClearReward` - 완벽한 클리어 시 추가 보상
- **새로운 속성**: `IsPerfectClear` - 완벽한 클리어 달성 여부 추적
- **사용법**: 자식 클래스에서 조건 실패 시 `FailPerfectClear()` 호출

```csharp
// 예시: 장애물에 부딪혔을 때
void OnCollisionEnter(Collision collision)
{
    if (collision.gameObject.CompareTag("Obstacle"))
    {
        FailPerfectClear(); // 완벽한 클리어 조건 실패
        // 게임은 계속 진행됨
    }
}
```

### 2. 스트레스 지속 증가 시스템
- **새로운 필드**: `_stressPerSecond` - 초당 증가하는 스트레스 양
- **자동 처리**: `UpdateGame()`에서 매 프레임 자동으로 스트레스 증가
- **커스터마이징**: `ApplyStressOverTime()` 오버라이드로 커스텀 로직 구현 가능

```csharp
protected override void ApplyStressOverTime(float deltaTime)
{
    base.ApplyStressOverTime(deltaTime);
    // 추가 스트레스 로직 (예: 특정 상황에서 2배 증가)
}
```

### 3. 각 미니게임별 고유 실패 조건
- **새로운 메서드**: `CheckFailCondition()` - 실시간 실패 조건 체크
- **사용법**: 자식 클래스에서 오버라이드하여 고유한 실패 조건 구현

```csharp
// 예시 1: 체력이 0이 되면 실패
protected override bool CheckFailCondition()
{
    return currentHealth <= 0;
}

// 예시 2: 3번 실수하면 실패
protected override bool CheckFailCondition()
{
    return mistakeCount >= 3;
}
```

### 4. 시간 제한 유연화
- **기존**: 시간 종료 시 무조건 `CheckClearOnTimeout()` 호출
- **변경**: `OnTimeOut()` 메서드로 각 게임이 타임아웃 처리 방식 결정
- **Duration = 0**: 시간 제한 없는 미니게임 (클리어 조건만으로 종료)

```csharp
// 예시 1: 시간 종료 시 자동 실패
protected override void OnTimeOut()
{
    Complete(false);
}

// 예시 2: 시간 종료 시 현재 점수로 성공/실패 판정
protected override void OnTimeOut()
{
    bool success = currentScore >= targetScore;
    Complete(success);
}

// 예시 3: 시간 제한 없음 (Duration = 0으로 설정)
// OnTimeOut()을 구현하지 않으면 시간과 무관하게 동작
```

### 5. 실시간 클리어 조건
- **기존 메서드 유지**: `CheckClearRealtime()` 
- **사용법**: 목표 달성 시 즉시 클리어

```csharp
// 예시: 모든 아이템 수집 시 클리어
protected override bool CheckClearRealtime()
{
    return collectedItems >= totalItems;
}
```

## 보상 시스템 구조

### Inspector 설정
```
[성공 보상] - 일반 클리어 시 지급
├─ 스트레스 -5
└─ 집중력 +3

[실패 보상] - 실패 시 지급 (패널티)
└─ 스트레스 +10

[완벽한 클리어 보상] - 성공 + 완벽한 클리어 조건 달성 시 추가 지급
├─ 스트레스 -3 (추가)
├─ 집중력 +2 (추가)
└─ 아이템 획득
```

### MiniGameCompleteArgs 구조
```csharp
public struct MiniGameCompleteArgs
{
    public bool success;           // 성공 여부
    public float usedTime;         // 소요 시간
    public float remainingTime;    // 남은 시간
    public bool isPerfectClear;    // 완벽한 클리어 달성 여부 (NEW!)
}
```

## 실제 구현 예시

### 예시 1: 장애물 피하기 게임
```csharp
public class ObstacleAvoidGame : MiniGameBase
{
    private int hitCount = 0;
    
    // 장애물에 한 번도 안 맞으면 완벽한 클리어
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            hitCount++;
            FailPerfectClear(); // 완벽한 클리어 실패
        }
    }
    
    // 3번 맞으면 게임 실패
    protected override bool CheckFailCondition()
    {
        return hitCount >= 3;
    }
    
    // 목표 지점 도착 시 클리어
    protected override bool CheckClearRealtime()
    {
        return reachedGoal;
    }
    
    // 시간 종료 시 실패
    protected override void OnTimeOut()
    {
        Complete(false);
    }
}
```

### 예시 2: 아이템 수집 게임 (시간 제한 없음)
```csharp
public class ItemCollectionGame : MiniGameBase
{
    // Inspector에서 Duration = 0으로 설정
    
    private int collectedCount = 0;
    private int targetCount = 10;
    
    // 모든 아이템 수집 시 클리어
    protected override bool CheckClearRealtime()
    {
        return collectedCount >= targetCount;
    }
    
    // 실패 조건 없음 (시간 제한도 없음)
    // OnTimeOut()도 구현하지 않음
}
```

### 예시 3: 타이밍 게임
```csharp
public class TimingGame : MiniGameBase
{
    private int perfectHits = 0;
    private int totalHits = 0;
    private int requiredHits = 5;
    
    public void OnButtonPress(float timing)
    {
        totalHits++;
        
        if (Mathf.Abs(timing) < 0.1f) // Perfect 타이밍
        {
            perfectHits++;
        }
        else
        {
            FailPerfectClear(); // 완벽하지 않은 타이밍
        }
    }
    
    // 필요한 횟수만큼 성공하면 클리어
    protected override bool CheckClearRealtime()
    {
        return totalHits >= requiredHits;
    }
    
    // 시간 종료 시 현재 진행도로 판정
    protected override void OnTimeOut()
    {
        bool success = totalHits >= requiredHits;
        Complete(success);
    }
}
```

## 주의사항

1. **IsPerfectClear는 자동으로 true로 시작**됩니다
   - 조건 실패 시 `FailPerfectClear()` 호출 필요

2. **Complete() 호출 시 자동으로 보상 지급**됩니다
   - 성공 시: 기본 보상 + (완벽한 클리어 시 추가 보상)
   - 실패 시: 실패 보상

3. **시간 제한 없는 게임**은 Duration = 0으로 설정

4. **스트레스는 게임 진행 중 자동 증가**
   - PlayerManager에 AddStress() 메서드 구현 필요

5. **CheckFailCondition()과 CheckClearRealtime()은 매 프레임 체크**됩니다
   - 무거운 연산은 피하세요
