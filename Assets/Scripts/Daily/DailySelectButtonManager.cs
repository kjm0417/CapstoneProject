using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugManager;

public class DailySelectButtonManager : MonoBehaviour
{
    public static string OnReturnButtonSelected = "OnReturnButtonSelectEvent";

    public static string OnCreateButtonSelected = "OnCreateButtonSelectEvent";

    public static string OnCurrentRestTime = "OnPossibleRestButtonEvent"; //현재 쉬는 시간 받아오기

    protected string OnUIModeChanged = "OnUIModeChangeEvent";

    private static ObjectPool<DailyChangeSelect> choiceBtnItemPool;

    protected List<DailyChangeSelect> choiceBtnItemList = new List<DailyChangeSelect>();

    [SerializeField] private DailyChangeSelect choiceButtonPrefab;
    [SerializeField] private Transform choiceButtonParent; // 버튼들을 담을 부모

    [SerializeField]
    protected int currentIndex = 0; // 현재 대사의 버튼 인덱스

    [SerializeField]
    private string currentPlayerId;

    [SerializeField]
    protected KeyCode lastArrowKey = KeyCode.None; // 마지막으로 누른 방향키

    [SerializeField]
    protected UIMode uiMode;
    public enum UIMode { None, Rest, Study, FixedStudy }

    public UIMode Mode
    {
        get => uiMode;
        protected set
        {
            if (uiMode != value)
            {
                uiMode = value;

                EventBus.Publish(OnUIModeChanged, uiMode);
            }
        }
    }


    #region RestUI 관련
    [Header("RestUI 관련")]
    [SerializeField]
    DailyRestSelectButton restButton;

    protected virtual void SelectFinshed(bool isUp) { }
    protected virtual void MoveColumn(int direction) { }
    protected virtual void SelectCloum(bool isUp) { }

    protected virtual void ClearRestColum(bool restState) { }

    protected virtual void OnRestPressButton() { }
    #endregion

    #region StudyUI 관련
    [Header("StudyUI 관련")]
    [SerializeField]
    DailyStudySelectButton studyButton;
    protected virtual void MoveStudyColum(int direction) { }
    protected virtual void SelectStudyColum() { }

    protected virtual void ClearStudyColum(bool studyState) { }
    protected virtual void OnStudyPressButton() { }
    #endregion


    private void Awake()
    {
        if(choiceBtnItemPool == null)
        {
            choiceBtnItemPool = new ObjectPool<DailyChangeSelect>(choiceButtonPrefab, 3, choiceButtonParent);
        }
      

    }
    protected virtual void OnEnable()
    {
        EventBus.Subscribe<IPlayerStateReadOnly>(EventNames.ActiveCharacterChanged, OnPlayerChanged);

        EventBus.Subscribe<List<ChoiceData>>(OnCreateButtonSelected, CreateChoiceButtons);

        EventBus.Subscribe(OnReturnButtonSelected, ReturnChoiceButtons); //선택 버튼 클릭 이벤트 구독

    }

    protected virtual void OnDisable()
    {
        EventBus.UnSubscribe<IPlayerStateReadOnly>(EventNames.ActiveCharacterChanged, OnPlayerChanged);

        EventBus.UnSubscribe<List<ChoiceData>>(OnCreateButtonSelected, CreateChoiceButtons);

        EventBus.UnSubscribe(OnReturnButtonSelected, ReturnChoiceButtons); //선택 버튼 클릭 이벤트 구독 해제

    }
    protected void OnPlayerChanged(IPlayerStateReadOnly state)
    {
        currentPlayerId = state.Id;
    }


    [SerializeField]
    protected bool isStudySelectReady;
    [SerializeField]
    protected bool isTimeSelectReady;

    private void Update()
    {
        UpdateRestMode();

        UpdateStudyMode();


        if (choiceBtnItemList.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CurrentButtonImageColorActive(true);

            if (Mode == UIMode.Study)
            {
                studyButton?.ClearStudyColum(true);
            }
            if (Mode == UIMode.Rest)
            {
                restButton?.ClearRestColum(true);

                EventBus.Publish(RestTimeQuestionItem.OnRestUIBacked, true);
            }
            isStudySelectReady = false;
            isTimeSelectReady = false;
            lastArrowKey = KeyCode.None;           
        }


        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            PressRestStudy();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (!isTimeSelectReady) //시간 증,감 버튼 누를 준비 안됐다면
            {
                MoveSelection(1);
            }
            else
            {
                MoveColumn(-1);
            }

        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) //시간 증,감 버튼 누를 준비 안됐다면
        {
            if (!isTimeSelectReady) //시간 증,감 버튼 누를 준비 안됐다면
            {
                MoveSelection(-1);
            }
            else
            {
                MoveColumn(1);
            }
        }
    }

    private void PressRestStudy()
    {
        switch(Mode)
        {
            case UIMode.Rest:
                restButton?.OnRestPressButton();
                break;
            case UIMode.Study:
                studyButton?.OnStudyPressButton();            
                break;
            case UIMode.FixedStudy:
                EventBus.Publish("MiniGame.StartById", "listening");
                HandleAfterPress();
                break;
            case UIMode.None:
                Debug.Log("None 모드");
                choiceBtnItemList[currentIndex].PressButton(Mode);
                HandleAfterPress();
                break;
        }

    }
    /// <summary>
    /// 선택(확정) 이후 공통으로 호출되는 처리
    /// </summary>
    protected virtual void HandleAfterPress()
    {
        EventBus.Publish(DialougeSystem.OnNotNextDialoue, false);
        StartCoroutine(ReturnChoiceButtonsNextFrame());
    }
    protected virtual IEnumerator ReturnChoiceButtonsNextFrame()
    {
        yield return null;
        ReturnChoiceButtons();
    }
    private void UpdateStudyMode()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (Mode == UIMode.Study)
            {
                CurrentButtonImageColorActive(false);

                isTimeSelectReady = true;

                lastArrowKey = KeyCode.UpArrow;

                SelectStudyColum();
            }
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (Mode == UIMode.Study)
            {
                CurrentButtonImageColorActive(false);

                isTimeSelectReady = true;

                lastArrowKey = KeyCode.DownArrow;

               SelectStudyColum();
            }
        }

        if ((Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow)) && Mode == UIMode.Study)
        {
            if (lastArrowKey == KeyCode.UpArrow)
            {
                MoveStudyColum(-1);
            }
            else if (lastArrowKey == KeyCode.DownArrow)
            {
                MoveStudyColum(1);
            }

        }
    }
    [SerializeField]
    protected bool isUpSelected = false; //위 화살표 키를 통해 RestUI의 버튼들 조작 가능
    private void UpdateRestMode()
    {
        //if (isRestQuestionUI == false) { return; }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if(Mode == UIMode.Rest)
            {
                isUpSelected = true;

                CurrentButtonImageColorActive(false);
                isTimeSelectReady = true;
                lastArrowKey = KeyCode.UpArrow;
                SelectCloum(true);
            }
          
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (Mode == UIMode.Rest)
            {
                isUpSelected = false;

                CurrentButtonImageColorActive(false);
                isTimeSelectReady = true;
                lastArrowKey = KeyCode.DownArrow;
                SelectCloum(false);
            }
        }
        if ((Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow)) && Mode == UIMode.Rest)
        {
            if (lastArrowKey == KeyCode.UpArrow)
            {
                SelectFinshed(true);
            }
            else if (lastArrowKey == KeyCode.DownArrow)
            {
                SelectFinshed(false);
            }

        }

  

    }
    /// <summary>
    /// 선택지 버튼의 좌,우 방향으로 이동했을 때 처리
    /// </summary>
    private void MoveSelection(int direction)
    {
        // 기존 선택 버튼 효과 해제
        CurrentButtonImageColorActive(false);

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
        CurrentButtonImageColorActive(true);

        SetUI();
    }
    /// <summary>
    /// 버튼 생성의 진입점이며, DialougeSystem이 마지막 대사의 버튼 생성 이벤트 발행을 해줌
    /// </summary>
    /// <param name="choices"></param>
    private void CreateChoiceButtons(List<ChoiceData> choices)
    {
        ReturnChoiceButtons();

        var routine = DailyRoutineManager.Instance.DailyRoutine;

        if (routine == DailyRoutine.Room)
        {
            RebuildRoomChoices(choices);    // Room패턴은 특수 상황임 -> 버튼 1개만 생성
            return;
        }
        for (int i = 0; i < choices.Count; i++)
        {
            CreateButton(choices[i], i);
        }
            
    }
    // Room 패턴 전용: 버튼 1 개만 생성
    private void RebuildRoomChoices(List<ChoiceData> choices)
    {
       // ReturnChoiceButtons();

        string needed = DecideNeededEffectId(currentPlayerId); 

        if (string.IsNullOrEmpty(needed)) return;

        for (int i = 0; i < choices.Count; i++)
        {
            if (choices[i].EffectId == needed)
            {
                CreateButton(choices[i], 0); //실제, 여기서 1개만 버튼이 생성됨
                break; 
            }
        }
    }
    // Room 패턴 전용: 캐릭터에 따라 버튼 판별
    private string DecideNeededEffectId(string playerId)
    {
        // 강민주=car, 윤채은/한민주=bus

        if (playerId == "강민주") return "car";
        if (playerId == "윤채은" || playerId == "한민주") return "bus";
        return null;
    }


    /// <summary>
    /// 버튼 생성 공통 메서드
    /// </summary>
    private void CreateButton(ChoiceData choice, int index)
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

            SetUI();

            CurrentButtonImageColorActive(true);
        }
    }

    void SetUI()
    {      

        ChoiceData choiceData = choiceBtnItemList[currentIndex].GetChoiceData();

        Debug.Log(choiceData.ChoiceId);


        if (choiceData.ChoiceId.Equals("Rest")) //ChoiceId가 Rest이면 - 액셀 참고
        {
            Mode = UIMode.Rest;

            EventBus.Publish(UIManager.OnInGameUI, false);

            EventBus.Publish(UIManager.OnRestQuestions, true);
        }
        else if (choiceData.ChoiceId.Equals("Study"))
        {
            Mode = UIMode.FixedStudy;

            EventBus.Publish(UIManager.OnRestQuestions, false);

            DailyRoutine currentDailyRoutine = DailyRoutineManager.Instance.DailyRoutine;

            if (currentDailyRoutine == DailyRoutine.Room) { return; }
            if (currentDailyRoutine == DailyRoutine.GotoSchool) { return; }
            if (currentDailyRoutine == DailyRoutine.ArriveSchool_Breakfast) { return; }

            Mode = UIMode.Study;

            EventBus.Publish(UIManager.OnInGameUI, true);

        }
        else
        {
            Mode = UIMode.None;

            EventBus.Publish(UIManager.OnInGameUI, false);

            EventBus.Publish(UIManager.OnRestQuestions, false);
        }
    }
    /// <summary>
    /// 현재 버튼의 색상의 명도를 높일지 말지에 대한 메서드
    /// </summary>
    protected void CurrentButtonImageColorActive(bool acitve)
    {
        if(choiceBtnItemList.Count > 0)
        {
            choiceBtnItemList[currentIndex].SetHighlight(acitve);
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
