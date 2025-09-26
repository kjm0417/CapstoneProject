using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExcelAsset]
public class DailyDialougeExcel : ScriptableObject
{
	public List<DailyDialougeData> DailySelect; // Replace 'EntityType' to an actual type that is serializable.
}
