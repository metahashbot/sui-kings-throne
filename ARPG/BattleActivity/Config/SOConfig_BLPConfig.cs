using System;
using RPGCore.UtilityDataStructure;
using UnityEngine;
namespace ARPG.Manager.Config
{
	[Serializable]
	[CreateAssetMenu(fileName = "BLP_预设的BLP内容",
		menuName = "#SO Assets#/#战斗关卡配置#/敌人生成配置/BLP_预设的BLP内容",
		order = 56)]
	public class SOConfig_BLPConfig : ScriptableObject
	{

		[SerializeReference]
		public BaseBuffLogicPassingComponent[] BLPList_Serialize;

	}
}