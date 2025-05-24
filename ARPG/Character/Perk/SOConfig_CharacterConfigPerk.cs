using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Perk
{
	[TypeInfoBox("记录一个Perk如何生效，生效的具体细节功能的SO配置文件")]
	[Serializable]
	[CreateAssetMenu(fileName = "单项Perk配置", menuName = "#SO Assets#/#角色配置#/单项Perk配置", order = 21)]
	public class SOConfig_CharacterConfigPerk : ScriptableObject
	{
		/// <summary>
		/// <para>perk的类型是enum固定的，不会变多</para>
		/// </summary>
		public RP_PerkTypeEnum_CharacterConfig Type;

		// [SerializeReference, LabelText("Perk的生效条件们")]
		// public List<PerkAvailableRequirementComponent> AvailableRequirementCollection;
		//
		// [SerializeReference, LabelText("Perk的作用效果们")]
		// public List<BasePerkEffectComponent> PerkEffectComponentCollection;
		
	}
}