using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//          ==== 자습한다 선택 && 미니게임 클리어 시 ====
//자습한다/쉰다 선택 시,
//0. 자습한다/쉰다 선택지 활성화
//1. 키보드 조작을 이용하여 자습한다 선택 버튼에 갖다 댄다.
//2. 할 일 목록 UI에서 미니게임을 선택한다.
//3. 해당 미니게임 실행한다.
//4. 클리어한다
//5. EventBus.Publish(DailyRoutineManager.OnDailyChanged,NextPattern); 호출하여 다음 패턴으로 이동

//           ==== 자습한다 선택 && 미니게임 미 클리어 시  ====

//0. 자습한다/쉰다 선택지 활성화
//1. 키보드 조작을 이용하여 자습한다 선택 버튼에 갖다 댄다.
//2. 할 일 목록 UI에서 미니게임을 선택한다.
//3. 해당 미니게임 실행한다.
//4. 미니게임 제한 시간 초과 하였다.
//5. EventBus.Publish(DailyRoutineManager.OnDailyChanged,NextPattern); 호출하여 다음 패턴

//              ==== 쉰다 선택 ====
//0. 자습한다/쉰다 선택지 활성화
//1. 키보드 조작을 이용하여 쉰다 선택 버튼에 갖다 댄다.
//2. 몇분 쉴지 보여주는 UI 활성화 한다. 
//3. 몇분 쉴지 결정했으면 쉰다 선택 버튼 누른다.
//4. 책을 덮는 연출 한다.
//5. 서서히 암전 된다
//6. 0번으로 돌아간다.

public class EffectManager : MonoBehaviour //연출 매니저.
                                           //기본 흐름 : 연출 진행 -> 연출 끝 -> ( 어떠한 스토리 진행 ) -> 다음 패턴
{
    public static string OnSelectEffectStarted = "OnEffectStartEvent"; //선택에 따른 연출 

    public static string OnNoneSelectEffectStarted = "OnNoneSelectEffectStartEvent"; //선택과 관계 없는 연출

    public static string OnRestTimeEnded = "OnRestTimeEndEvent"; //몇분 쉴지 선택했을 때 이벤트


    private bool isNextPattern; //false : 스토리 진행을 위한 연출 / true : 다음 패턴으로 이동하기 위한 연출 
    // 


    private void OnEnable()
    {
        EventBus.Subscribe<ChoiceData>(OnSelectEffectStarted, OnSelectEffect);

        EventBus.Subscribe<string>(OnNoneSelectEffectStarted, OnNoneSelectEffect);

        EventBus.Subscribe<bool,int>(OnRestTimeEnded, OnDailyRest);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<ChoiceData>(OnSelectEffectStarted, OnSelectEffect);

        EventBus.UnSubscribe<string>(OnNoneSelectEffectStarted, OnNoneSelectEffect);

        EventBus.UnSubscribe<bool,int>(OnRestTimeEnded, OnDailyRest);
    }
    /// <summary>
    /// 버튼에 맞는 연출 실행하는 메서드
    /// </summary>
    private void OnSelectEffect(ChoiceData choiceData) 
    {
        isNextPattern = true;

        StartCoroutine(StartEffect(choiceData));
    }

    private void OnNoneSelectEffect(string effectId)
    {
        isNextPattern = false;     
    }
    private void OnDailyRest(bool isEnd,int selectRestTime)
    {
        currentRestTime = selectRestTime;
        isDailyChange = isEnd;
    }

    [SerializeField]
    bool isDailyChange = false; //다음 패턴으로 갈지에 대한 여부. 쉴 시간이 0분이면 다음 패턴 이동 true

    [SerializeField]
    int currentRestTime = 0; // 이거 이용해서 몇분 쉬냐에 따라 집중력 올라가게끔 하면 될듯

    /// <summary>
    /// 실제 연출 진행하는 코루틴
    /// </summary>

    private IEnumerator StartEffect(ChoiceData choiceData) //연출 진행 -> 연출 끝 -> 암전
    {
        Debug.Log($"{choiceData.EffectId}에 맞는 연출 진행 중");
        yield return new WaitForSeconds(3.0f);
        Debug.Log($"{choiceData.EffectId}에 맞는 연출 끝");

        if(choiceData.ChoiceId.Equals("Rest"))
        {
            FadeInOutManager.Instance.FadeOutAndLoad(() => //암전
            {  
                if(isDailyChange == false)
                {
                    EventBus.Publish(DialougeSystem.OnLastDialougeSelected, true); //마지막 대사 활성화
                }
               else
                {
                    EventBus.Publish(DailyRoutineManager.OnDailyChanged, choiceData.NextPattern);
                    isDailyChange = false;
                }

                FadeInOutManager.Instance.FadeInLoad(); //암전 해제
            });
        }
        else
        {
            EventBus.Publish(DailyRoutineManager.OnDailyChanged, choiceData.NextPattern);
        }
       
    }

}
