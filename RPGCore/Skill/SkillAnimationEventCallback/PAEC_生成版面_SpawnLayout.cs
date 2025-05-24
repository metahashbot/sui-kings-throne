using System;
using ARPG.Character.Base;
using ARPG.Equipment;
using RPGCore;
using RPGCore.Projectile.Layout;
using RPGCore.Skill;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Player
{
	[Serializable]
	public class PAEC_生成版面_SpawnLayout : BasePlayerAnimationEventCallback
	{

		[SerializeField, LabelText("需要生成版面"), InlineEditor(InlineEditorObjectFieldModes.Boxed)]
		public SOConfig_ProjectileLayout RelatedConfig;

		[SerializeField, LabelText("需要统一时间戳吗")]
		public bool NeedUniformTimeStamp = false;
		
		[SerializeField,LabelText("不接受伤害类型变动？")]
		public bool NotAcceptDamageTypeChange = false;


		[NonSerialized]
		public SOConfig_ProjectileLayout _RuntimeLayoutRef;

		public override BasePlayerAnimationEventCallback ExecuteBySkill(
			BaseARPGCharacterBehaviour behaviour,
			BaseRPSkill skillRef,
			bool createNewWhenExist = false)
		{
			if (RelatedConfig)
			{
				RelatedConfig.SpawnLayout(behaviour);
			}
			return this;
		}
		public override BasePlayerAnimationEventCallback ExecuteByWeapon(
			BaseARPGCharacterBehaviour behaviour,
			BaseWeaponHandler weaponHandler,
			bool createNewWhenExist = false)
		{
			return this;
		}

		public override string GetElementNameInList()
		{ 
			return $"{GetBaseCustomName()} 生成版面：_{RelatedConfig?.name}_";
		}
	}
}