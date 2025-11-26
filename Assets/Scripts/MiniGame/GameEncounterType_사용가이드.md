# 게임 조우 타입 시스템 (미니게임 vs 보스전)

미니게임과 보스전을 구분하여 다른 팝업 애니메이션을 적용하는 시스템입니다.

## 개요

- **MiniGame**: 일반 미니게임 (아래에서 위로 슬라이드 애니메이션)
- **BossFight**: 보스전 (페이드 인/아웃 효과)

## 파일 구조

1. **GameEncounterType.cs** - 조우 타입 열거형
2. **PopupFadeWorld.cs** - 보스전용 페이드 애니메이션 컨트롤러
3. **MiniGamePopupRunner.cs** - 수정됨 (두 타입 모두 지원)

## Unity 설정 방법

### 1. 미니게임 설정

각 미니게임 프리팹의 MiniGameBase 컴포넌트에서:
- **Encounter Type**: `MiniGame` 선택 (기본값)

MiniGamePopupRunner 컴포넌트에서:
- **Slide Popup**: PopupSlideWorld 컴포넌트 할당
- **Fade Popup**: PopupFadeWorld 컴포넌트 할당 (둘 다 할당!)

### 2. 보스전 설정

#### A. 보스전용 UI 패널 생성

1. Canvas 하위에 Panel 생성 → 이름: `BossFightPanel`
2. `BossFightPanel`에 보스전 UI 요소 배치
   - 보스 체력바
   - 플레이어 스킬 버튼
   - 전투 정보 등

#### B. PopupFadeWorld 컴포넌트 추가

1. `BossFightPanel`에 `PopupFadeWorld` 컴포넌트 추가
2. 설정:
   - **Target**: BossFightPanel의 RectTransform (자동 설정됨)
   - **Duration**: 0.5 (페이드 시간)
   - **Ease In**: OutQuad
   - **Ease Out**: InQuad
   - **Use Unscaled Time**: ✅ 체크
   - **Start Hidden**: ✅ 체크

#### C. 보스전 미니게임 만들기

보스전도 `MiniGameBase`를 상속받아 만들고, **Encounter Type을 BossFight로 설정**합니다:

```csharp
public class BossFightMiniGame : MiniGameBase
{
    // Inspector에서 Encounter Type을 BossFight로 설정!
    
    [Header("Boss Settings")]
    [SerializeField] private float bossMaxHealth = 100f;
    
    private float bossCurrentHealth;

    protected override void OnStartHook()
    {
        base.OnStartHook();
        bossCurrentHealth = bossMaxHealth;
        // 보스 초기화 로직
    }

    protected override void OnUpdateHook(float deltaTime)
    {
        base.OnUpdateHook(deltaTime);
        // 보스 전투 로직
    }

    protected override bool CheckClearRealtime()
    {
        // 보스 체력이 0이면 클리어
        return bossCurrentHealth <= 0f;
    }

    protected override bool CheckFailCondition()
    {
        // 플레이어 체력이 0이면 실패
        return PlayerManager.Instance.CurrentHealth <= 0f;
    }
}
```

## 애니메이션 차이점

### MiniGame (슬라이드)
- 화면 아래에서 위로 올라옴
- 빠르고 경쾌한 느낌
- 짧은 미니게임에 적합

### BossFight (페이드)
- 점점 나타남 (alpha 0 → 1)
- 무게감 있고 긴장감 있는 느낌
- 중요한 전투에 적합

## 코드에서 사용하기

### 미니게임 시작 (기존 방식 그대로)

```csharp
// 일반 미니게임
EventBus.Publish<string>("MiniGame.StartById", "SlidingPuzzle");

// 보스전
EventBus.Publish<string>("MiniGame.StartById", "BossFight_Dragon");
```

### MiniGameManager에 등록

MiniGameManager의 Inspector에서:
1. **Games** 리스트에 추가
2. **Id**: "BossFight_Dragon"
3. **Prefab**: 보스전 프리팹 할당

보스전 프리팹에는:
- BossFightMiniGame 컴포넌트
- 보스전 UI 요소들

## 런타임 동작 흐름

### 미니게임 (Slide)
1. `ShowAndStart()` 호출
2. `slidePopup.Show()` → 아래에서 위로 슬라이드
3. 애니메이션 완료 대기
4. `game.Init()` → `game.StartGame()`
5. 게임 진행...
6. 완료/중단 시 `slidePopup.Hide()` → 위에서 아래로 슬라이드

### 보스전 (Fade)
1. `ShowAndStart()` 호출
2. `fadePopup.Show()` → 점점 나타남 (alpha 0→1)
3. 애니메이션 완료 대기
4. `game.Init()` → `game.StartGame()`
5. 게임 진행...
6. 완료/중단 시 `fadePopup.Hide()` → 점점 사라짐 (alpha 1→0)

## 자동 타입 감지 시스템

**중요**: 이제 `MiniGamePopupRunner`는 게임의 타입을 자동으로 감지합니다!

1. 각 미니게임 프리팹의 Inspector에서 **Encounter Type** 설정
2. `MiniGamePopupRunner`에는 **slidePopup과 fadePopup 둘 다 할당**
3. 게임 시작 시 자동으로 올바른 애니메이션 선택

### 동작 원리

```csharp
// MiniGameBase에서
public GameEncounterType EncounterType => _encounterType;

// MiniGamePopupRunner에서
GameEncounterType encounterType = _game.EncounterType; // 자동 감지!
if (encounterType == GameEncounterType.MiniGame)
    slidePopup.Show();
else
    fadePopup.Show();
```

## 주의사항

1. **각 게임에서 Encounter Type 설정**: 미니게임 프리팹의 MiniGameBase 컴포넌트에서 설정
2. **두 팝업 모두 할당**: MiniGamePopupRunner에 slidePopup과 fadePopup 둘 다 할당 필요
3. **CanvasGroup 자동 추가**: PopupFadeWorld는 자동으로 CanvasGroup을 추가하므로 수동 추가 불필요
4. **DOTween 필수**: 두 팝업 모두 DOTween을 사용하므로 프로젝트에 DOTween 설치 필요

## 확장 아이디어

- 다른 애니메이션 타입 추가 (Zoom, Rotate 등)
- 보스별로 다른 페이드 효과 (색상 변화 등)
- 전투 시작 전 컷신 연출
- 보스 등장 사운드 효과
