using System;
using System.Collections.Generic;
using System.Linq;
using ARPG.Character;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using Global;
using Global.GlobalConfig;
using RPGCore;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
namespace ARPG.Equipment
{
	[Serializable]
	public class RecordRuntime_RPGEquipmentInfo
	{
		public GlobalConfigSO.PlayerEquipmentInfo RawInfoInGCSORef { get; private set; }

		public EIC_装备基础RPG数值_EquipmentGeneralPropertyInfo EquipmentEffect_BaseProperty { get; private set; }
		public EquipmentBaseTemplateConfig EBTConfigRef { get; private set; }



		/// <summary>
		/// <para>ARPG环境限定：向角色施加一级和二级属性效果</para>
		/// <para>由于ARPG环境下不能换装备，所以不用卸下</para>
		/// </summary>
		public void ApplyEffectToBehaviour_BasePropertyAndSecondProperty(
			PlayerARPGConcreteCharacterBehaviour behaviourRef)
		{
			EquipmentEffect_BaseProperty.ApplyEffectToBehaviour(behaviourRef);
			if (RawInfoInGCSORef.EIC_EquipmentSecondPropertyInfo != null)
			{
				RawInfoInGCSORef.EIC_EquipmentSecondPropertyInfo.ApplyEffectToBehaviour(behaviourRef);
			}
		}


		public void Init(GlobalConfigSO.PlayerEquipmentInfo infoRaw)
		{
			RawInfoInGCSORef = infoRaw;
			var info = GCAHHExtend.GetEquipmentRawInfo(infoRaw.EquipmentUID);
            EquipmentEffect_BaseProperty = new EIC_装备基础RPG数值_EquipmentGeneralPropertyInfo();
            EquipmentEffect_BaseProperty.CopyDataFromBaseConfig(info);
            EBTConfigRef = info;
        }
	}
}