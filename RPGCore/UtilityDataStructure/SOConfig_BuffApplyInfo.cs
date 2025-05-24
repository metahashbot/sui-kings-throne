using System;
using System.Collections.Generic;
using UnityEngine;
namespace RPGCore.UtilityDataStructure
{
	[Serializable]
	[CreateAssetMenu(fileName = "预设的Buff施加信息", menuName = "#SO Assets#/#RPG Core#/预设的Buff施加信息", order = 139)]
	public class SOConfig_BuffApplyInfo : ScriptableObject
	{
		public List<ConSer_BuffApplyInfo> BuffEffectList = new List<ConSer_BuffApplyInfo>();
	}
}