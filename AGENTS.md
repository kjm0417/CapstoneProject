# Repository Guidelines

## 프로젝트 구조 & 모듈 구성
- `Assets/Scripts`는 Event, MiniGame, Player, Quest, Singleton_Manager, UI, Util 등 기능별 폴더로 나뉘며, 각 미니게임과 공용 매니저 스크립트가 이 위치에 있습니다.
- `Assets/Scenes`에는 주요 플레이 씬이, `Assets/Prefabs`와 `Assets/Resources`에는 UI, 플레이어, 환경 프리팹과 리소스가 저장되어 있어 에셋을 재사용할 때 먼저 확인하십시오.
- Excel 기반 데이터는 `Assets/Excel/ExcelImporter`에 있으며, 관련 C# 프로젝트(`ExcelImporter.csproj`, `ExcelImporter.Editor.csproj`)는 루트에 별도 관리됩니다.
- Unity 에디터 버전은 `6000.0.38f1`이므로 같은 버전을 Unity Hub에서 선택해 열고, 빌드 설정은 `ProjectSettings` 폴더에 커밋되어 있습니다.

## 빌드, 테스트 & 개발 명령
- Windows CLI 예시: `"C:\Program Files\Unity\Hub\Editor\6000.0.38f1\Editor\Unity.exe" -batchmode -projectPath "%CD%" -quit`으로 에디터 진입 없이 임포트 오류를 점검할 수 있습니다.
- 자동 빌드는 `-executeMethod BuildScripts.PerformBuild` 패턴을 추가해 사용하며, `BuildScripts` 클래스는 `Assets/Scripts`에 위치해야 합니다.
- 데이터 툴을 갱신하려면 `dotnet build ExcelImporter.csproj`를 실행해 Excel 관련 유틸을 컴파일합니다.
- 테스트 결과를 남길 때는 `-runTests -testPlatform editmode -testResults results.xml` 플래그를 조합하여 CI 아티팩트를 생성하십시오.

## 코딩 스타일 & 네이밍 규칙
- C# 스크립트는 4칸 공백 들여쓰기, UTF-8, 파일당 하나의 공개 클래스 규칙을 유지하며, 클래스·메서드는 `PascalCase`, 필드·지역변수는 `camelCase`를 사용합니다.
- Unity 직렬화 필드는 `[SerializeField] private` 패턴을 우선하고, 인스펙터 노출 이름은 의미를 드러내도록 `spawnIntervalSeconds`처럼 작성합니다.
- 코루틴, 이벤트 핸들러 등 비동기 흐름은 `On`/`Handle` 접두사를 사용하고, ScriptableObject 자산 이름은 `SO_` 접두사를 붙여 구분합니다.
- 애니메이션, 프리팹, 씬 파일은 공백 대신 밑줄을 사용하고, 리소스 경로는 상대 구조(`Resources/UI/HUD`)를 유지해 런타임 로드를 단순화합니다.

## 테스트 지침
- 현재 저장소에는 테스트 폴더가 없으므로, 신규 테스트는 `Assets/Tests/EditMode` 또는 `Assets/Tests/PlayMode` 하위에 생성해 Unity Test Runner에서 즉시 식별되도록 합니다.
- NUnit 기반 `TestFixture`와 `UnityTest`를 사용해 미니게임 상태 머신, 스폰 타이밍 등 핵심 로직을 검증하고, 실패 시 스크린샷이 필요한 경우 `TestTools` 유틸을 추가하십시오.
- 기능 추가 시 최소 하나의 EditMode 단위 테스트를 요구하며, Scene 의존 기능은 PlayMode 시뮬레이션 테스트로 보완해 80% 이상의 커버리지를 목표로 합니다.
- CI가 없다면 로컬에서 `Unity.exe -runTests` 명령을 실행하고, `results.xml`을 PR에 아티팩트로 첨부해 검증 근거를 남깁니다.

## 커밋 & PR 가이드라인
- Git 기록은 `feat :`, `fix :`처럼 태그형 접두사를 사용하므로 동일한 양식을 유지하고, 영어 요약 뒤에 필요 시 한국어 설명을 괄호로 보충합니다.
- 작업 브랜치는 `feature/`, `bugfix/`, `hotfix/` 등 목적 기반 접두사를 사용하고, 미니게임별 작업은 `mini-game/<name>`처럼 하위 구분을 추가하십시오.
- PR에는 변경 요약, 테스트 수행 결과, 영향을 받는 씬 혹은 프리팹 목록, 필요 시 스크린샷이나 GIF를 포함해 리뷰어가 Unity에서 재현하지 않아도 이해할 수 있게 합니다.
- Excel 데이터나 빌드 파이프라인을 수정했으면 릴리스 노트 초안을 함께 갱신하고, PR 설명에 관련 Trello/Jira 티켓 또는 GitHub 이슈 링크를 명시해 추적성을 확보합니다.
