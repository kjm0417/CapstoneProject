using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DailySelectButtonManager : MonoBehaviour
{
    public static string OnReturnButtonSelected = "OnReturnButtonSelectEvent";

    public static string OnCreateButtonSelected = "OnCreateButtonSelectEvent";



    private ObjectPool<DailyChangeSelect> choiceBtnItemPool;

    private List<DailyChangeSelect> choiceBtnItemList = new List<DailyChangeSelect>();

    [SerializeField] private DailyChangeSelect choiceButtonPrefab;
    [SerializeField] private Transform choiceButtonParent; // 버튼들을 담을 부모

    [SerializeField]
    private string currentPlayerId;

    private int currentIndex = 0; // 현재 선택된 버튼 인덱스

    private  void Awake()
    {
        choiceBtnItemPool = new ObjectPool<DailyChangeSelect>(choiceButtonPrefab, 3, choiceButtonParent);

    }
    private void Update()
    {
        if (choiceBtnItemList.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveSelection(1);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveSelection(-1);
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            choiceBtnItemList[currentIndex].PressButton();
        }
    }

    private void MoveSelection(int direction)
    {
        // 기존 선택 버튼 효과 해제
        choiceBtnItemList[currentIndex].SetHighlight(false);

        currentIndex += direction;

        if (currentIndex < 0)
        {
            Debug.Log("버튼 인덱스가 0보다 작아질 경우 / 현재 버튼인덱스는 버튼 리스트의 마지막값으로");
            currentIndex = choiceBtnItemList.Count - 1;
        }
            
        else if (currentIndex >= choiceBtnItemList.Count) 
            //생성된 버튼 리스트의 최대개수를 넘어가면 인덱스 초과 오류뜨므로, 0으로 초기화
        {
            Debug.Log("버튼 인덱스 넘어감");
            currentIndex = 0;
        }
           

        // 새 버튼 강조
        choiceBtnItemList[currentIndex].SetHighlight(true);

        ChoiceData choiceData = choiceBtnItemList[currentIndex].GetChoiceData();

        if(choiceData.ChoiceId.Equals("Rest")) //ChoiceId가 Rest이면 - 액셀 참고
        {
            EventBus.Publish(UIManager.OnRestQuestions, true);
        }
        else if (choiceData.ChoiceId.Equals("Study")) //ChoiceId가 Study이면 - 액셀 참고
        {
            EventBus.Publish(UIManager.OnRestQuestions, false);
        }
    }

    private void OnEnable()
    {
        EventBus.Subscribe<IPlayerStateReadOnly>(EventNames.ActiveCharacterChanged, OnPlayerChanged);

        EventBus.Subscribe<List<ChoiceData>>(OnCreateButtonSelected, CreateChoiceButtons);

        EventBus.Subscribe(OnReturnButtonSelected, ReturnChoiceButtons); //선택 버튼 클릭 이벤트 구독
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<IPlayerStateReadOnly>(EventNames.ActiveCharacterChanged, OnPlayerChanged);

        EventBus.UnSubscribe<List<ChoiceData>>(OnCreateButtonSelected, CreateChoiceButtons);

        EventBus.UnSubscribe(OnReturnButtonSelected, ReturnChoiceButtons); //선택 버튼 클릭 이벤트 구독 해제
    }
    private void OnPlayerChanged(IPlayerStateReadOnly state)
    {
        currentPlayerId = state.Id;
    }

    private void CreateChoiceButtons(List<ChoiceData> choices)
    {
        ReturnChoiceButtons();

        DailyRoutine currentRoutine = DailyRoutineManager.Instance.DailyRoutine;

        for (int i = 0; i < choices.Count; i++)
        {
            if (currentRoutine == DailyRoutine.Room)
            {
                OnRoomSelect(choices[i]);
                break;
            }

            CreateButton(choices[i],i);
        }


    }

    /// <summary>
    /// 패턴이 방 패턴일 때 조건에 따라 버튼 판별하는 메서드 정의
    /// </summary>
    private void OnRoomSelect(ChoiceData choice)
    {
        bool isMatch = false;

        if (currentPlayerId == "강민주" && choice.EffectId == "car")
        {
            isMatch = true;
            CreateButton(choice,0);
        }
        else if ((currentPlayerId == "윤채은" || currentPlayerId == "한민주") && choice.EffectId == "bus")
        {
            isMatch = true;
            CreateButton(choice,0);
        }
    }

    /// <summary>
    /// 버튼 생성 공통 메서드
    /// </summary>
    private void CreateButton(ChoiceData choice,int index)
    {
        DailyChangeSelect choiceBtn = choiceBtnItemPool.Get();
        choiceBtn.InitButtonText(choice);

        choiceBtn.transform.SetSiblingIndex(index);

        choiceBtnItemList.Add(choiceBtn);

        // 첫 버튼 강조
        if (choiceBtnItemList.Count > 0)
        {
            // 모든 버튼의 하이라이트 초기화 (전부 false)
            for (int i = 0; i < choiceBtnItemList.Count; i++)
            {
                choiceBtnItemList[i].SetHighlight(false);
            }

            // 첫 번째 버튼만 선택 활성화
            currentIndex = 0;
            choiceBtnItemList[currentIndex].SetHighlight(true);
        }
    }

    /// <summary>
    /// 버튼 풀 반환 메서드
    /// </summary>
    private void ReturnChoiceButtons()
    {
        foreach (DailyChangeSelect choiceBtnItem in choiceBtnItemList)
        {
            choiceBtnItemPool.Return(choiceBtnItem);
        }

        choiceBtnItemList.Clear();

    }


}
