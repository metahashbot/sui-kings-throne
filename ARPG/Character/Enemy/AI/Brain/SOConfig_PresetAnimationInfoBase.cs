using System;
using System.Collections.Generic;
using ARPG.Character.Base;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI
{
	[Serializable]
	[CreateAssetMenu(fileName = "(P)_预设的动画信息", menuName = "#SO Assets#/#敌人AI#/Ani_预设的动画信息", order = 161)]
	public class SOConfig_PresetAnimationInfoBase : ScriptableObject
	{
		

		[SerializeReference, LabelText("预设的动画信息")]
		[ListDrawerSettings(ShowPaging = true,
			ShowItemCount = true,
			ShowIndexLabels = true,
			ListElementLabelName = "ConfigName", NumberOfItemsPerPage = 50)]
		public List<AnimationInfoBase> SelfAllPresetAnimationInfoList;
		//关联的编辑期Brain原始SOConfig引用，用来当各种下拉菜单的查找源的
		[HideInInspector, SerializeField]
		public SOConfig_AIBrain RelatedBrainSOConfig;

	}
}