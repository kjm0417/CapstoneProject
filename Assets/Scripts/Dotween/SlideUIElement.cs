using System;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class SlideUIElement : MonoBehaviour
{
    public RectTransform target; //움직일 UI
    public Vector2 showPosition; // 이동할 위치 = 최종 목적지 
    [HideInInspector] public Vector2 InitPostion; //초기 위치 

    public InGameCalendar.SlideDirection Direction; //이동 방향
    
}
