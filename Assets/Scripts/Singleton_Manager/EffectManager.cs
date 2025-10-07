using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//          ==== 자습한다 선택 && 미니게임 클리어 시 ====
//자습한다/쉰다 선택 시, 진행되는 흐름들을 정의 
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

    public static string OnRestTimeEnded = "OnRestTimeEndEvent"; //쉴 시간이 0분일 때


    private bool isDailyRoutineSelct; //false : 스토리 진행을 위한 연출 / true : 다음 패턴으로 이동하기 위한 연출 
    // 


    private void OnEnable()
    {
        EventBus.Subscribe<ChoiceData>(OnSelectEffectStarted, OnSelectEffect);

        EventBus.Subscribe<string>(OnNoneSelectEffectStarted, OnNoneSelectEffect);

        EventBus.Subscribe<bool>(OnRestTimeEnded, OnChangeDailyRoutine);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<ChoiceData>(OnSelectEffectStarted, OnSelectEffect);

        EventBus.UnSubscribe<string>(OnNoneSelectEffectStarted, OnNoneSelectEffect);

        EventBus.UnSubscribe<bool>(OnRestTimeEnded, OnChangeDailyRoutine);
    }

    private void OnSelectEffect(ChoiceData choiceData) //선택 버튼 선택이 아닌 선택 후 확정 시 호출되는 메서드
    {
        isDailyRoutineSelct = true;

        if(choiceData.ChoiceId.Equals("Study")) //미니게임 이벤트 조건(스터디 플래너 판별)
        {
            //예시. effectId(액셀의 effectID 방식) = JumpMiniGame 

            //EventBus.Publish(MiniGameManager.OnMiniGameStarted, choiceData.EffectId);
        }
        else if (choiceData.ChoiceId.Equals("Rest"))
        {
            EventBus.Publish(UIManager.OnRestQuestions, false); //얼마나 쉴지 보여주는 ui 비활성화
        }

        StartCoroutine(StartEffect(choiceData));
    }

    private void OnNoneSelectEffect(string effectId)
    {
        isDailyRoutineSelct = false;     
    }
    private void OnChangeDailyRoutine(bool isEnd)
    {
        isDailyChange = isEnd;
    }

    [SerializeField]
    bool isDailyChange = false;

    private IEnumerator StartEffect(ChoiceData choiceData) //연출 진행 -> 연출 끝 -> 암전
    {
        Debug.Log($"{choiceData.EffectId}에 맞는 연출 진행 중");
        yield return new WaitForSeconds(3.0f);
        Debug.Log($"{choiceData.EffectId}에 맞는 연출 끝");

        if(choiceData.ChoiceId.Equals("Rest"))
        {
            FadeInOutManager.Instance.FadeOutAndLoad(() =>
            {  
                if(isDailyChange == false)
                {
                    EventBus.Publish(DialougeSystem.OnLastDialougeSelected, true); //마지막 대사 활성화

                }
               else
                {
                    Debug.Log("쉴 시간이 0분이므로 다음 패턴 변경");
                    EventBus.Publish(DailyRoutineManager.OnDailyChanged, choiceData.NextPattern);
                    isDailyChange = false;
                }
                FadeInOutManager.Instance.FadeInLoad();

                Debug.Log("FadeOut 완료 . 선택 대사 재출력");
            });
        }
        else
        {
            EventBus.Publish(DailyRoutineManager.OnDailyChanged, choiceData.NextPattern);
        }
       
    }

}
