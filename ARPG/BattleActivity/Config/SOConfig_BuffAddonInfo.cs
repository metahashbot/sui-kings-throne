using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Manager.Config
{
	[Serializable]
	[CreateAssetMenu(fileName = "BAI_预设的Buff附加内容",
		menuName = "#SO Assets#/#战斗关卡配置#/敌人生成配置/BAI_预设的Buff附加内容",
		order = 56)]

	public class SOConfig_BuffAddonInfo : ScriptableObject
	{
		[SerializeField,LabelText("附加项内容")]
		public List<ESA_附加Buff_AddBuff.PerAddonInfo> AddonInfoList = new List<ESA_附加Buff_AddBuff.PerAddonInfo>();
	}
}