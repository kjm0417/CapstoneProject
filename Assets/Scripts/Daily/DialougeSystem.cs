using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

[System.Serializable]
public class ChoiceData
{
    public string ChoiceId;
    public string Text;     //버튼에 보여줄 텍스트
    public string EffectId; //버튼에 맞는 연출 ID
    public string NextPattern; //다음 일상 패턴 저장
}
[System.Serializable]
public struct Speaker //화자의 정보를 저장하는 Speaker 구조체 정의
{
    public Image PlayerImage; //캐릭터 이미지
    public Image imageDialog; //대화창 Image UI
    public TextMeshProUGUI textName; //화자 이름
    public TextMeshProUGUI textDialouge; //대화 텍스트
    public GameObject objectArrow; //대사가 완료 되었을 때 출력되는 커서 오브젝트
}

[System.Serializable]
public struct DialogData //화자 이름과 대사 정보를 저장하는 DialogData 구조체 정의
{
    public int speakerIndex; //이름과 대사를 출력할 현재 DialogSystem의 speakers 배열 순번
    public string name; //캐릭터 이름
    [TextArea(3, 5)] //3열 5행까지 
    public string dialogue; //대사
                            // public string choices; //선택 대사
    public List<ChoiceData> choices; // 자식 테이블에서 가져온 선택지들
    public int speakerImagePos; //플레이어 이미지 UI 위치 : 0 - 왼쪽 , 1 - 중간 , 2 - 오른쪽

}

public class DialougeSystem : SingleTonGeneric<DialougeSystem> //일상적인 대화 또는 스토리가 진행되는 시스템
{
    public static string OnLastDialougeSelected = "OnLastDialougeSelect";

    public static string OnNotNextDialoue = "OnNotNextDialouge"; //대사 종료 방지 이벤트

    [SerializeField]
    DailyDialougeExcel DailyDialougeDB;

    [SerializeField]
    private Speaker[] Speakers; //대화에 참여하는 캐릭터들의 UI 배열

    [SerializeField]
    private DialogData[] Dialogs; //현재 분기의 대사 목록 배열

    [SerializeField]
    private bool isAutoStart = true; //자동 시작여부
    private bool isFirst = true; //최초 1회만 호출하기 위한 변수
    [SerializeField]
    private int currentDialogIndex = -1; //현재 대사 순번
    [SerializeField]
    private int currentSpeakerIndex = 0; //현재 말을 하는 화자(Speaker)의 speakers 배열 순번

    [SerializeField]
    private float typingSpeed = 0.1f; //텍스트 타이핑 효과 재생속도
    private bool isTypingEffect = false; //텍스트 타이핑 효과를 재생중인지

    [SerializeField] private Transform leftAnchor;
    [SerializeField] private Transform centerAnchor;
    [SerializeField] private Transform rightAnchor;

    private WaitForSeconds typingYield; //추가

    private void Awake()
    {
        RefreshTypingYield(); //추가
    }


    private void OnEnable()
    {
        EventBus.Subscribe(DailySelectButtonManager.OnReturnButtonSelected, SetUpUI);
        EventBus.Subscribe<bool>(OnLastDialougeSelected, ShowLastDialogueInfo);
        EventBus.Subscribe<bool>(OnNotNextDialoue, OnFlag);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe(DailySelectButtonManager.OnReturnButtonSelected, SetUpUI);
        EventBus.UnSubscribe<bool>(OnLastDialougeSelected, ShowLastDialogueInfo);
        EventBus.UnSubscribe<bool>(OnNotNextDialoue, OnFlag);
    }

    private void OnFlag(bool isActive)
    {
        isSelectDialougeLast = isActive;
    }

    /// <summary>
    /// 선택 대사의 액셀 데이터들을 받아와서 해당 선택 대사에 맞는 대화를 진행할 수 있게 데이터 값을 저장
    /// </summary>
    public void InitSelectDialouge(DialogData[] dialogs , bool isAuto )
    {
       
        Dialogs = dialogs;
        isAutoStart = isAuto;

        currentDialogIndex = -1;
        currentSpeakerIndex = 0;
        isFirst = true;
        isTypingEffect = false;

        //로그 테스트용
        for(int i = 0; i < Dialogs.Length; i++)
        {
            Debug.Log($"대사 순번 : {Dialogs[i].speakerIndex} \n 화자 이름 :  {Dialogs[i].name}\n 대사 내용 : {Dialogs[i].dialogue}\n  선택 대사 : {Dialogs[i].choices}\n");
        }

        if(isAutoStart && Dialogs.Length > 0)
        {
            StartCoroutine(DialogueFlow());
        }
    }

    IEnumerator DialogueFlow()
    {
        // UpdateDialog가 true 반환될 때까지 반복
        while (!UpdateDialog())
        {
            yield return null;
        }
        Debug.Log("대화 종료됨");
    }

    /// <summary>
    /// 현재 대사 분기의 첫 대사 출력전에 초기화하는 메서드 
    /// - 모든 대화 관련 게임 오브젝트 비활성화 및
    /// 캐릭터 이미지를 보이도록 설정
    /// </summary>
    private void SetUpUI()
    {
        for (int i = 0; i < Speakers.Length; i++)
        {
            SetActiveObjects(Speakers[i], false); //대화 관련 게임 오브젝트 비활성화

            //Speakers[i].PlayerImage.gameObject.SetActive(true); //캐릭터 이미지는 보이도록
        }

        isSelectDialougeLast = false;
    }

    /// <summary>
    /// 대사 분기를 진행하고, 분기가 종료되었을 때 true를 반환하는 메서드
    /// </summary>
    public bool UpdateDialog()
    {
       if(isFirst == true) //대사 분기가 시작될 때, 1회만 호출
        {
            SetUpUI(); //초기화. 

            if(isAutoStart) //자동 재생(isAutoStart가 true)로 되어 있으면 
            {
                SetNextDialogue(); //첫번째 대사를 자동 재생
            }

            isFirst = false;

       }

       if( (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && !isSelectDialougeLast)
        {
            if(isTypingEffect) //텍스트 타이핑효과를 재생중일 때 , 마우스 왼 클릭 시, 효과 종료
            {
                isTypingEffect = false;

                //타이핑 효과를 중지하고, 현재 대사 전체를 출력
                StopCoroutine(OnTypingText());

                Speakers[currentSpeakerIndex].textDialouge.text = Dialogs[currentDialogIndex].dialogue;

                //대사가 완료되었을 때 출력되는 커서 활성화
                Speakers[currentSpeakerIndex].objectArrow.SetActive(true);

                return false;
            }
            if(Dialogs.Length > currentDialogIndex + 1) // 대사가 남아 있을 때
            {
                SetNextDialogue();
            }
            else //대사가 더 이상 없을 경우 모든 오브젝트를 비활성화하고 true 반환
            {
                SetUpUI();

                return true;
            }
        }

        return false;
    }
    /// <summary>
    /// 이전 화자의 대화 관련 오브젝트 비활성화 하고, 현재 화자 순번,이름,텍스트 설정하여 다음 대사를 진행하게 하는 메서드
    /// </summary>
    private void SetNextDialogue()
    {

        //이전 화자 대화 관련 오브젝트 비활성화
        SetActiveObjects(Speakers[currentSpeakerIndex], false);

        //다음 대사 진행
        currentDialogIndex++;

        //현재 화자 순번 설정
        currentSpeakerIndex = Dialogs[currentDialogIndex].speakerIndex; //

        //현재 화자의 대화 관련 오브젝트 활성화
        SetActiveObjects(Speakers[currentSpeakerIndex], true);

        //현재 화자 이름 텍스트 설정
        Speakers[currentSpeakerIndex].textName.text = Dialogs[currentDialogIndex].name;

        ApplySpeakerPosition(Speakers[currentSpeakerIndex], Dialogs[currentDialogIndex].speakerImagePos);

        //현재 화자의 대사 텍스트 설정
        //Speakers[currentSpeakerIndex].textDialouge.text = Dialogs[currentDialogIndex].dialogue; //주석: 이 코드는 대화 한번에 출력이므로, 타이핑 효과 내기 위함임

        RefreshTypingYield(); //추가

        StartCoroutine(OnTypingText());


    }
                                                                
    /// <summary>
    /// 화자의 대화 관련 오브젝트 활성화 및 비활성화 하는 메서드 , 화자가 대화를 하고 있나 없나에 따라 알파값 설정
    /// </summary>
    private void SetActiveObjects(Speaker speaker, bool isActive)
    {

        speaker.imageDialog.gameObject.SetActive(isActive);
        speaker.textName.gameObject.SetActive(isActive);
        speaker.textDialouge.gameObject.SetActive(isActive);

        speaker.objectArrow.SetActive(false); //화살표는 대사 종료되었을 때만 활성화하기 때문에 항상 false;

        //캐릭터 이미지 UI 알파 값 변경 - 잠깐 보류
       // Color color = speaker.PlayerImage.color;
        //color.a = isActive == true ? 1 : 0f;
       // speaker.PlayerImage.color = color;

    }
    /// <summary>
    /// 화자의 캐릭터 UI 위치 설정하는 메서드
    /// </summary>
    private void ApplySpeakerPosition(Speaker speaker, int ImagePos)
    {
        switch (ImagePos)
        {
            case 0: speaker.PlayerImage.transform.position = leftAnchor.position; break;
            case 1: speaker.PlayerImage.transform.position = centerAnchor.position; break;
            case 2: speaker.PlayerImage.transform.position = rightAnchor.position; break;
        }
    }

    public bool isSelectDialougeLast; //마지막 선택 대사일 때

    IEnumerator OnTypingText()
    {
        if (currentDialogIndex == Dialogs.Length - 1)
        {

            if (Dialogs[currentDialogIndex].choices != null && Dialogs[currentDialogIndex].choices.Count > 0)
            {
                isSelectDialougeLast = true; //대화 종료 방지            

                EventBus.Publish(DailySelectButtonManager.OnCreateButtonSelected, Dialogs[currentDialogIndex].choices);
            }
        }

        int index = 0;
        isTypingEffect = true;

        //텍스트를 한글자씩 타이핑 치듯 재생
        while (index <= Dialogs[currentDialogIndex].dialogue.Length)
        {
            if (!isTypingEffect) yield break; // 클릭 시 종료

            Speakers[currentSpeakerIndex].textDialouge.text = Dialogs[currentDialogIndex].dialogue.Substring(0, index);

            index++;

            yield return typingYield; 
        }

        isTypingEffect = false;

        //대사 완료시 커서 활성화
        Speakers[currentSpeakerIndex].objectArrow.SetActive(true);



    }
    /// <summary>
    /// typingYield를 typingSpeed 값에 맞게 새로 생성하는 함수를 정의한다.
    /// </summary>
    private void RefreshTypingYield() 
    {
        typingYield = new WaitForSeconds(typingSpeed);  
    }

    /// <summary>
    /// 마지막 대사를 그대로 UI에 다시 표시하는 메서드
    /// </summary>
    public void ShowLastDialogueInfo(bool isActive)
    {
        // 안전 검사
        if (Dialogs == null || Dialogs.Length == 0) return;

        // 마지막 대사 데이터 가져오기
        DialogData lastDialog = Dialogs[Dialogs.Length - 1];

        // 해당 화자의 인덱스
        int lastSpeakerIndex = lastDialog.speakerIndex;

        // 모든 화자 UI 비활성화 후 마지막 화자만 활성화
        for (int i = 0; i < Speakers.Length; i++)
            SetActiveObjects(Speakers[i], false);

        SetActiveObjects(Speakers[lastSpeakerIndex], true);

        // 이름, 대사, 캐릭터 이미지 모두 마지막 대사 정보로 갱신
        Speakers[lastSpeakerIndex].textName.text = lastDialog.name;
        Speakers[lastSpeakerIndex].textDialouge.text = lastDialog.dialogue;
        ApplySpeakerPosition(Speakers[lastSpeakerIndex], lastDialog.speakerImagePos);

        // 커서 표시 (대화 완료 상태처럼)
        Speakers[lastSpeakerIndex].objectArrow.SetActive(true);

        EventBus.Publish(DailySelectButtonManager.OnCreateButtonSelected, Dialogs[lastSpeakerIndex].choices);
    }


}
