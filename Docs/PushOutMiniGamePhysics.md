# Push Out Mini Game 물리 충돌 정리

## UI 콜라이더가 막힘을 만드는 이유
- `Img_Player` 프리팹에는 `BoxCollider2D`, `Rigidbody2D`, `RectColliderSync2D`가 함께 붙어 있어 `RectTransform` 크기 그대로 2D 콜라이더가 만들어집니다.
- `Img_Pusher` 프리팹 역시 동일한 3개 컴포넌트를 가지고 있으며, 스폰되자마자 2D 물리 충돌에 참여합니다.
- 이 프리팹들이 Screen Space - Overlay 캔버스 안에서 생성되면, 같은 UI 레이어에 있는 다른 오브젝트(예: 전체 화면을 덮는 패널)의 박스 콜라이더와 맞부딪혀 이동이 제한됩니다.

## 프리팹 및 씬 연결 관계
- `PushOutMiniGame.prefab` 하위의 `Img_Player` 오브젝트가 위 물리 컴포넌트를 포함합니다.
- `Img_Pusher.prefab`도 동일한 물리 컴포넌트를 가지고 있으며, 미니게임 진행 중 다수 인스턴스가 생성됩니다.
- `TestScene_KJ`에서 `MiniGameManager`가 `PushOutMiniGame.prefab`을 런타임에 로드하므로, 다른 캔버스에는 콜라이더가 없어도 해당 미니게임 UI는 물리 충돌체를 생성하게 됩니다.

## 개선된 충돌 처리
- 플레이어와 푸셔는 서로를 힘으로 밀어내야 하므로 `Rigidbody2D`와 콜라이더를 유지합니다.
- 대신 `PushOutMiniGame` 스크립트가 미니게임 UI 하위의 정적 콜라이더를 탐색해 플레이어·푸셔와의 충돌만 선택적으로 무시합니다.
- 정적 UI 콜라이더와의 충돌이 꺼진 상태에서도 플레이어와 푸셔 사이 충돌은 유지되므로, 화면 UI에 막히지 않고 서로 밀어내는 플레이가 가능합니다.
