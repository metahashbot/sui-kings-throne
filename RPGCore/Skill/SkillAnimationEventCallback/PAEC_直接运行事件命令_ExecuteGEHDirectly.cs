using System;
using System.Collections.Generic;
using ARPG.Character.Base;
using ARPG.Equipment;
using GameplayEvent;
using RPGCore.Buff;
using RPGCore.Skill;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Player
{
	[Serializable]
	public class PAEC_直接运行事件命令_ExecuteGEHDirectly : BasePlayerAnimationEventCallback
	{
		[SerializeReference,LabelText("事件命令们")]
		public List<BaseGameplayEventHandler> RelatedHandlers = new List<BaseGameplayEventHandler>();


		public override BasePlayerAnimationEventCallback ExecuteByBuff(BaseARPGCharacterBehaviour behaviour, BaseRPBuff buff)
		{
			ExecuteDirectly(behaviour);
			return base.ExecuteByBuff(behaviour, buff);
		}
		public override BasePlayerAnimationEventCallback ExecuteBySkill(
			BaseARPGCharacterBehaviour behaviour,
			BaseRPSkill skillRef,
			bool createNewWhenExist = false)
		{
			ExecuteDirectly(behaviour);
			return base.ExecuteBySkill(behaviour, skillRef, createNewWhenExist);
		}
		public override BasePlayerAnimationEventCallback ExecuteByWeapon(
			BaseARPGCharacterBehaviour behaviour,
			BaseWeaponHandler weaponHandler,
			bool createNewWhenExist = false)
		{
			ExecuteDirectly(behaviour);
			return base.ExecuteByWeapon(behaviour, weaponHandler, createNewWhenExist);
		}

		private void ExecuteDirectly(BaseARPGCharacterBehaviour behaviour)
		{
			var ds_ge = new DS_GameplayEventArguGroup();
			foreach (BaseGameplayEventHandler perH in RelatedHandlers)
			{
				GameplayEventManager.Instance.ExecuteGEHCommandDirectly(perH, ds_ge);
			}
		}

	}
}