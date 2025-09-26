using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueManager : SingleTonGeneric<DialogueManager>
{
    public static string OnDialougeSelected = "OnDialougeSelectEvent";

    //[SerializeField]
    //DailyDialougeExcel DailyDialougeDB;

    [SerializeField]
    private DialougeSystem dialogSystem01;
    [SerializeField]
    private DialougeSystem dialogSystem02;
    [SerializeField]
    TextMeshProUGUI textCountDown;

    private IEnumerator Start()
    {
        textCountDown.gameObject.SetActive(false);

        //첫번째 대사 분기 시작
        yield return new WaitUntil(() => dialogSystem01.UpdateDialog());

        //대사 분기 사이에 원하는 행동을 추가 할 수 있다. 여기서 캐릭터를 움직이거나 아이템을 획득하는 등의..
        //현재는 5-4-3-2-1 카운트 다운 실행
        textCountDown.gameObject.SetActive(true);
        int count = 5;
        while(count > 0)
        {
            textCountDown.text = count.ToString();
            count--;

            yield return new WaitForSeconds(1);
        }
        textCountDown.gameObject.SetActive(false);

        yield return new WaitUntil(() => dialogSystem02.UpdateDialog());

        textCountDown.gameObject.SetActive(true);

        textCountDown.text = "The End";

        yield return new WaitForSeconds(2);

        UnityEditor.EditorApplication.ExitPlaymode(); //플레이모드 종료
    }

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<IDailyRoutine>(OnDialougeSelected, OnShowSelectUI);
    }

    private void OnDisable()
    {
        EventBus.Subscribe<IDailyRoutine>(OnDialougeSelected ,OnShowSelectUI);
    }

    private void OnShowSelectUI(IDailyRoutine routine)
    {

    }
}
