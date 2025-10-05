using UnityEngine;

public class StoryTriggerManager : SingleTonGeneric<StoryTriggerManager> //스토리 대사 내용 출력 조건 이벤트를 이 클래스에서 관리
{
    public static string OnStoryRequested = "OnStoryRequested"; //스토리 대사 진행 응답 이벤트

    //예시 스토리 이벤트들 나중에 수정. 현재는 테스트용
    public static string OnNpcInteracted = "OnNpcInteractedEvent"; //플레이어가 아닌 다른 일반 NPC(캐릭터)와 대화 했을 경우
    public static string OnPlayerCollided = "OnPlayerCollidedEvent"; //어떤 플레이어와 충돌 하거나 다른 일반 NPC(캐릭터)와 충돌했을 경우
    public static string OnMiniGameEnded = "OnMiniGameEndedEvent"; //미니 게임이 끝났을 경우
    public static string OnButtonPressed = "OnButtonPressedEvent"; //어떤 버튼을 눌렀을 경우

    private void OnEnable()
    {
        EventBus.Subscribe<DailyRoutine>(OnNpcInteracted, RequestStory);
        EventBus.Subscribe<DailyRoutine>(OnPlayerCollided, RequestStory);
        EventBus.Subscribe<DailyRoutine>(OnMiniGameEnded, RequestStory);
        EventBus.Subscribe<DailyRoutine>(OnButtonPressed,  RequestStory);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<DailyRoutine>(OnNpcInteracted, RequestStory);
        EventBus.UnSubscribe<DailyRoutine>(OnPlayerCollided,  RequestStory);
        EventBus.UnSubscribe<DailyRoutine>(OnMiniGameEnded,  RequestStory);
        EventBus.UnSubscribe<DailyRoutine>(OnButtonPressed, RequestStory);
    }

    private void RequestStory(DailyRoutine routineState)
    {
        Debug.Log($"[Trigger] 스토리 요청: {routineState}");
        EventBus.Publish(OnStoryRequested, routineState);
    }
}
