using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct Speaker //화자의 정보를 저장하는 Speaker 구조체 정의
{
    public SpriteRenderer spriteRender; //캐릭터 이미지
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
}

public class DialougeSystem : MonoBehaviour
{
    [SerializeField]
    DailyDialougeExcel DailyDialougeDB;


    [SerializeField]
    private Speaker[] Speakers; //대화에 참여하는 캐릭터들의 UI 배열

    [SerializeField]
    private DialogData[] Dialogs; //현재 분기의 대사 목록 배열

    [SerializeField]
    private bool isAutoStart = true; //자동 시작여부
    private bool isFirst = true; //최초 1회만 호출하기 위한 변수
    private int currentDialogIndex = -1; //현재 대사 순번
    private int currentSpeakerIndex = 0; //현재 말을 하는 화자(Speaker)의 speakers 배열 순번

    private float typingSpeed = 0.1f; //텍스트 타이핑 효과 재생속도
    private bool isTypingEffect = false; //텍스트 타이핑 효과를 재생중인지

    private string LocalStateName = "WakeUp";
    private void Awake()
    {
        int index = 0;

        for(int i = 0; i < DailyDialougeDB.DailySelect.Count; ++i)
        {
            if (DailyDialougeDB.DailySelect[i].StateName == LocalStateName)
            {
                Dialogs[index].name = DailyDialougeDB.DailySelect[i].StateName;
                Dialogs[index].dialogue = DailyDialougeDB.DailySelect[i].PlayerDialogue;
                index++;
            }
        }

        SetUp();
    }

    /// <summary>
    /// 현재 대사 분기의 첫 대사 출력전에 초기화하는 메서드 
    /// - 모든 대화 관련 게임 오브젝트 비활성화 및
    /// 캐릭터 이미지를 보이도록 설정
    /// </summary>
    private void SetUp() 
    {
        for(int i = 0; i< Speakers.Length; i++)
        {
            SetActiveObjects(Speakers[i], false); //대화 관련 게임 오브젝트 비활성화

            Speakers[i].spriteRender.gameObject.SetActive(true); //캐릭터 이미지는 보이도록
        }
    }

    /// <summary>
    /// 대사 분기를 진행하고, 분기가 종료되었을 때 true를 반환하는 메서드
    /// </summary>
    public bool UpdateDialog()
    {
       if(isFirst == true) //대사 분기가 시작될 때, 1회만 호출
        {
            SetUp(); //초기화. 

            if(isAutoStart) //자동 재생(isAutoStart가 true)로 되어 있으면 
            {
                SetNextDialogue(); //첫번째 대사를 자동 재생
            }

            isFirst = false;

        }

       if(Input.GetMouseButtonDown(0))
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
                for (int i = 0; i < Speakers.Length; i++)
                {
                    SetActiveObjects(Speakers[i], false); //대화 관련 게임 오브젝트 비활성화

                    Speakers[i].spriteRender.gameObject.SetActive(false); //캐릭터 이미지는 보이지 않게
                }
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

        //현재 화자의 대사 텍스트 설정
        //Speakers[currentSpeakerIndex].textDialouge.text = Dialogs[currentDialogIndex].dialogue; //주석: 이 코드는 대화 한번에 출력이므로, 타이핑 효과 내기 위함임

        StartCoroutine(OnTypingText());


    }

    /// <summary>
    /// 화자의 대화 관련 오브젝트 활성화 및 비활성화 하는 메서드 , 화자가 대화를 하고 있나 없나에 따라 알파값 설정
    /// </summary>
    /// 
    private void SetActiveObjects(Speaker speaker, bool isActive)
    {
        speaker.imageDialog.gameObject.SetActive(isActive);
        speaker.textName.gameObject.SetActive(isActive);
        speaker.textDialouge.gameObject.SetActive(isActive);

        speaker.objectArrow.SetActive(false); //화살표는 대사 종료되었을 때만 활성화하기 때문에 항상 false;

        //캐릭터 알파 값 변경
        Color color = speaker.spriteRender.color;
        color.a = isActive == true ? 1 : 0.2f;
        speaker.spriteRender.color = color;
    }


    IEnumerator OnTypingText()
    {
        int index = 0;
        isTypingEffect = true;

        //텍스트를 한글자씩 타이핑 치듯 재생
        while (index <= Dialogs[currentDialogIndex].dialogue.Length)
        {
            if (!isTypingEffect) yield break; // 클릭 시 종료

            Speakers[currentSpeakerIndex].textDialouge.text = Dialogs[currentDialogIndex].dialogue.Substring(0, index);

            index++;

            yield return new WaitForSeconds(typingSpeed);
        }

        isTypingEffect = false;

        //대사 완료시 커서 활성화
        Speakers[currentSpeakerIndex].objectArrow.SetActive(true);
    }
}
