using System;
using UnityEngine;
namespace RPGCore.Buff.Config
{
	[Serializable]
	[CreateAssetMenu(fileName = "Buff Config_Load Asset Entry",
		menuName = "#SO Assets#/#RPG Core#/Buff Config_Load Asset Entry", order = 0)]
	public class SOConfig_BuffLoadAssetEntry : ScriptableObject
	{
		public RolePlay_BuffTypeEnum RelatedBuffType;
		public RPBuffConfigContentInSO_LoadAssetEntry EntryContent;
	}
}