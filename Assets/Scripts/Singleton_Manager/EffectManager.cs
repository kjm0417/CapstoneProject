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



    private bool isNextPattern; //false : 스토리 진행을 위한 연출 / true : 다음 패턴으로 이동하기 위한 연출 
    // 

    private void OnEnable()
    {
        EventBus.Subscribe<ChoiceData,bool>(OnSelectEffectStarted, OnSelectEffect);

        EventBus.Subscribe<string>(OnNoneSelectEffectStarted, OnNoneSelectEffect);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<ChoiceData,bool>(OnSelectEffectStarted, OnSelectEffect);

        EventBus.UnSubscribe<string>(OnNoneSelectEffectStarted, OnNoneSelectEffect);

    }
    /// <summary>
    /// 버튼에 맞는 연출 실행하는 메서드
    /// </summary>
    private void OnSelectEffect(ChoiceData choiceData,bool isNextPattern) 
    {
        StartCoroutine(StartEffect(choiceData, isNextPattern));
    }

    private void OnNoneSelectEffect(string effectId)
    {
        isNextPattern = false;     
    }

    /// <summary>
    /// 실제 연출 진행하는 코루틴
    /// </summary>

    private IEnumerator StartEffect(ChoiceData choiceData,bool isNext) //연출 진행 -> 연출 끝 -> 암전
    {
        if(choiceData.ChoiceId == "StoryNo")
        {
            Debug.Log("대사를 통해 스토리를 진행 하지 않음 / 플레이어 자유");
            EventBus.Publish(EventNames.OnPlayerNotInput, true); //플레이어 이동 제한 해제
            yield break;
        }
        Debug.Log($"{choiceData.EffectId}에 맞는 연출 진행 중");
        yield return new WaitForSeconds(3.0f);
        Debug.Log($"{choiceData.EffectId}에 맞는 연출 끝");

        if(choiceData.ChoiceId.Equals("Rest"))
        {
            FadeInOutManager.Instance.FadeOutAndLoad(() => //암전
            {
                if (isNext == false)
                {
                    EventBus.Publish(DialougeSystem.OnLastDialougeSelected, true); //마지막 대사 활성화
                }
                else
                {
                    EventBus.Publish(DailyRoutineManager.OnDailyChanged, choiceData.NextPattern);
                }

                FadeInOutManager.Instance.FadeInLoad(); //암전 해제
            });
        }
        else if (choiceData.ChoiceId.Equals("Study"))
        {
           
        }
        else if(choiceData.ChoiceId.Equals("StoryYes"))
        {
            FadeInOutManager.Instance.FadeOutAndLoad(() => //암전
            {
                EventBus.Publish(DailyRoutineManager.OnDailyChanged, choiceData.NextPattern);

                FadeInOutManager.Instance.FadeInLoad(); //암전 해제
            });
        }
        else
        {
            EventBus.Publish(DailyRoutineManager.OnDailyChanged, choiceData.NextPattern);
        }
       
    }

}
