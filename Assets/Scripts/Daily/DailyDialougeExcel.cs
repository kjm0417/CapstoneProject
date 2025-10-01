using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExcelAsset]
public class DailyDialougeExcel : ScriptableObject
{
	public List<DailyDialougeData> Story; // 스토리 진행을 위한
	public List<DailyDialougeData> Transition; // 다음 패턴 전환을 위한
}
