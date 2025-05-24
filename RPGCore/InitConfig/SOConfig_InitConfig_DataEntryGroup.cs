using System;
using RPGCore.DataEntry;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.InitConfig
{
	[TypeInfoBox("一个用于存放 DataEntry 初始化信息的SO。仅仅存数据，并不关心自己到底是谁的数据。所以这个数据可以被共用")]
	[Serializable]
	[CreateAssetMenu(fileName = "DataEntry Config Group",
		menuName = "#SO Assets#/#RPG Core#/#初始化配置#/数据项 配置组",
		order = 142)]
	public class SOConfig_InitConfig_DataEntryGroup : ScriptableObject
	{
		public ConSer_DataEntryInitGroup ConfigCollection;
	}
}