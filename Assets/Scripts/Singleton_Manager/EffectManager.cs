using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EffectManager : MonoBehaviour //연출 매니저.
                                           //기본 흐름 : 연출 진행 -> 연출 끝 -> ( 어떠한 스토리 진행 ) -> 다음 패턴
{
    public static string OnSelectEffectStarted = "OnEffectStartEvent"; //선택에 따른 연출 

    public static string OnNoneSelectEffectStarted = "OnEffectStartEvent"; //선택과 관계 없는 연출

    private bool isSelect;


    private void OnEnable()
    {
        EventBus.Subscribe<string, string>(OnSelectEffectStarted, OnSelectEffect);

        EventBus.Subscribe<string>(OnNoneSelectEffectStarted, OnNoneSelectEffect);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<string, string>(OnSelectEffectStarted, OnSelectEffect);

        EventBus.UnSubscribe<string>(OnNoneSelectEffectStarted, OnNoneSelectEffect);
    }

    private void OnSelectEffect(string effectId, string NextPattern)
    {
        isSelect = true;

        if(effectId.Contains("Study")) //미니게임 이벤트 조건(스터디 플래너 판별)
        {
            //예시 effectId(액셀의 effectID 방식) = Study_JumpMiniGame 형식 유지할 것.
            string[] effectIds = effectId.Split('_');

            string miniGameId = effectIds[1];

            //EventBus.Publish(MiniGameManager.OnMiniGameStarted, miniGameId);
        }
        else
        {
            StartCoroutine(StartEffect(effectId, NextPattern));
        }
    }

    private IEnumerator StartEffect(string effectId, string NextPattern = "")
    {
        Debug.Log($"{effectId}에 맞는 연출 진행 중");
        yield return new WaitForSeconds(3.0f);
        Debug.Log($"{effectId}에 맞는 연출 끝");

        if(isSelect) //다음 일상 패턴 선택에 맞는 연출이라면, 다음 일상 패턴으로 전환 
        {
            OnDailyRoutineChange(NextPattern);
        }
    }

    private void OnNoneSelectEffect(string effectId)
    {
        isSelect = false;
        StartCoroutine(StartEffect(effectId));
    }

    private void OnDailyRoutineChange(string NextPattern) //이 메서드는 추후에 DailyRoutineManager로 옮길 것이고
                                                          //->
                                                          //패턴마다 구독하는 방식은 없애고, 일반 메서드로 호출
    {
        switch (NextPattern)
        {
            case "GotoSchool":
                EventBus.Publish(DailyRoutineManager.OnDailyGotoSchool);
                break;
            case "ArriveSchool_Morning":
                EventBus.Publish(DailyRoutineManager.OnDailySchool_Morning);
                break;
            case "ArriveSchool_Lanch":
                EventBus.Publish(DailyRoutineManager.OnDailySchool_Lanch);
                break;
            case "ArriveSchool_Afternoon":
                EventBus.Publish(DailyRoutineManager.OnDailySchool_Afternoon);
                break;
            case "Parm":
                EventBus.Publish(DailyRoutineManager.OnDailyEndedParm);
                break;
            case "AfterSchool":
                EventBus.Publish(DailyRoutineManager.OnDailyEndedAfterSchool);
                break;
        }
    }


}
