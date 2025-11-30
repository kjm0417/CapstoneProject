using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExcelAsset]
public class DailyDialougeExcel : ScriptableObject
{
	public List<DailyDialougeData> Story; // 스토리 진행 부모 테이블
	public List<DailyDialougeData> StoryEffect; // 스토리 진행 자식 테이블
	public List<DailyDialougeData> Transition; // 일상 패턴 전환 부모 테이블 
	public List<DailyDialougeData> TransitionEffect; // 일상 패턴 전환 자식 테이블

	public List<DailyDialougeData> InteractObject; // 상호작용 가능한 오브젝트 테이블
	public List<DailyDialougeData> InteractSelectDialouge; // 상호작용 가능한 오브젝트 다이얼로그 테이블
}
