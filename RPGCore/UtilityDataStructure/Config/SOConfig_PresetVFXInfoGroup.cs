using System;
using System.Collections.Generic;
using ARPG.Character.Enemy.AI;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.UtilityDataStructure.Config
{
	[Serializable]
	[CreateAssetMenu(fileName = "(P)预设特效信息组",menuName = "#SO Assets#/#RPG Core#/通用/(P)预设特效信息组",order = 153)]
	public class SOConfig_PresetVFXInfoGroup : ScriptableObject
	{

		[SerializeField, LabelText("关联的特效信息们"),ListDrawerSettings(ShowFoldout = true, ShowIndexLabels = true, ListElementLabelName = "_VFX_InfoID")]
		public List<PerVFXInfo> PerVFXInfoList = new List<PerVFXInfo>();

		//关联的编辑期Brain原始SOConfig引用，用来当各种下拉菜单的查找源的
		[HideInInspector, SerializeField]
		public SOConfig_AIBrain RelatedBrainSOConfig;

	}
}