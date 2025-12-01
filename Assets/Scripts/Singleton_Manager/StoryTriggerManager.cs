using System;
using System.Collections.Generic;
using UnityEngine;

public class StoryTriggerManager : SingleTonGeneric<StoryTriggerManager> //스토리 대사 내용 출력 조건 이벤트를 이 클래스에서 관리
{
    //예시 스토리 이벤트들 나중에 수정. 현재는 테스트용
    public static string OnPlayerCollided = "OnPlayerCollidedEvent"; //어떤 플레이어와 충돌 하거나
                                                                     //다른 일반 NPC(캐릭터) 또는 오브젝트 충돌했을 경우
    [SerializeField]
    DialougeSystem dialougeSystem;

    [SerializeField]
    private int currentDay;

    private void Start()
    {
        Init();
    }
    private void OnEnable()
    {
        EventBus.Subscribe<GameDate>(TimeManager.OnDayStarted, OnDayStarted); //하루 시작 이벤트   

        EventBus.Subscribe<InteractObject>(OnPlayerCollided, ObjectDialougeDataLoad);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<GameDate>(TimeManager.OnDayStarted, OnDayStarted); //하루 시작 이벤트   
        EventBus.UnSubscribe<InteractObject>(OnPlayerCollided, ObjectDialougeDataLoad);
    }

    private void OnDayStarted(GameDate date)
    {
        currentDay = date.Day;
    }


    [SerializeField]
    DailyDialougeExcel DailyDialougeDB;


    private bool isStoryCacheBuilt;


    private readonly Dictionary< string, List<DialogData>> InteractCache =
       new Dictionary<string, List<DialogData>>();

    void ObjectDialougeDataLoad(InteractObject interactObject)
    {
        BuildDailyPattenrCache();

        string objectID = interactObject.objectInfo.ObjectID;

        if (!InteractCache.TryGetValue(objectID, out var dialogDatas))
        {
            Debug.LogWarning($"ObjectID {objectID}의 대사가 없음");
            dialougeSystem.InitSelectDialouge(Array.Empty<DialogData>(), true, DialougeType.StroyTrigger);
            return;
        }

        Debug.Log($"ObjectID {objectID}의 대사 {dialogDatas.Count}개 로드");

        dialougeSystem.InitSelectDialouge(dialogDatas.ToArray(), true, DialougeType.StroyTrigger);
    }

    private void BuildDailyPattenrCache()
    {
        if (isStoryCacheBuilt || DailyDialougeDB == null)
        {
            return;
        }

        InteractCache.Clear();

        // InteractObject 데이터 처리 (Day 없음)
        for (int i = 0; i < DailyDialougeDB.InteractObject.Count; i++)
        {
            var row = DailyDialougeDB.InteractObject[i];

            string MasterId = row.MasterId;


            // ObjectID별 DialogData 리스트 생성
            if (!InteractCache.TryGetValue(MasterId, out var dialogDatas))
            {
                dialogDatas = new List<DialogData>();
                InteractCache[MasterId] = dialogDatas;
            }

            // 선택지 데이터 수집 (FK 매칭)
            List<ChoiceData> choices = new List<ChoiceData>();
            foreach (var selectDialogue in DailyDialougeDB.InteractSelectDialouge)
            {
                if (selectDialogue.MasterId == row.MasterId)
                   {
                    choices.Add(new ChoiceData
                    {
                        Text = selectDialogue.InteractSelectDialouge,
                        ChoiceId = selectDialogue.ChoiceId,
                        EffectId = selectDialogue.EffectId,
                        NextPattern = selectDialogue.NextPattern
                    });
                }
            }

            // DialogData 생성 및 추가
            dialogDatas.Add(new DialogData
            {
                speakerIndex = row.PlayerId,
                name = row.PlayerName,
                dialogue = row.InteractDialouge,
                choices = choices,
            });

            Debug.Log($"캐시 빌드: ObjectID={MasterId}, 대사='{row.InteractDialouge}', 선택지={choices.Count}개");
        }

        isStoryCacheBuilt = true;
        Debug.Log($"InteractObject 캐시 빌드 완료: 총 {InteractCache.Count}개 오브젝트");
    }
}
