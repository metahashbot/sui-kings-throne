using System;
using System.Collections.Generic;
using ARPG.Character.Perk;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Inheritance
{
	[Serializable]
	[CreateAssetMenu(fileName = "一个Perk的集成信息",menuName = "#SO Assets#/#角色配置#/继承/单项Perk的继承信息",order = 21)]
	public class SOConfig_PerInheritanceConfig : ScriptableObject
	{
		public RP_PerkTypeEnum_CharacterConfig PerkType;

		[LabelText("基本权重")]
		public float BaseWeight = 1f;
		
		[SerializeReference,LabelText("组件库")]
		public List<BaseInheritanceComponent> InheritanceComponentCollection;
		
		
		
		public float GetInheritanceWeight()
		{
			float result = BaseWeight;
			float original = BaseWeight;
			float frontAdd = 0;
			float frontMul = 1f;
			float rearAdd = 0;
			float rearMul = 1f;
			float finalMul = 1f;
			
			
			foreach (var component in InheritanceComponentCollection)
			{
				
			}
			return result;
		}
		
		

		
	}
}