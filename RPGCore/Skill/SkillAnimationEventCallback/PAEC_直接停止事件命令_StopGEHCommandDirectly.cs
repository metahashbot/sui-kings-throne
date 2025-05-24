using System.Collections.Generic;
using ARPG.Character.Base;
using ARPG.Equipment;
using GameplayEvent;
using RPGCore.Buff;
using RPGCore.Skill;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Player
{
	public class PAEC_直接停止事件命令_StopGEHCommandDirectly : BasePlayerAnimationEventCallback
	{
		[SerializeField, LabelText("需要停止的命令ID")]
		 private List<string> _RelatedHandlerID = new List<string>();

		public override BasePlayerAnimationEventCallback ExecuteByBuff(BaseARPGCharacterBehaviour behaviour, BaseRPBuff buff)
		{
			StopDirectly();
			return base.ExecuteByBuff(behaviour, buff);
		}
		public override BasePlayerAnimationEventCallback ExecuteBySkill(
			BaseARPGCharacterBehaviour behaviour,
			BaseRPSkill skillRef,
			bool createNewWhenExist = false)
		{
			StopDirectly();

			return base.ExecuteBySkill(behaviour, skillRef, createNewWhenExist);
		}
		public override BasePlayerAnimationEventCallback ExecuteByWeapon(
			BaseARPGCharacterBehaviour behaviour,
			BaseWeaponHandler weaponHandler,
			bool createNewWhenExist = false)
		{
			StopDirectly();

			return base.ExecuteByWeapon(behaviour, weaponHandler, createNewWhenExist);
		}


		private void StopDirectly()
		{
			foreach (string perID in _RelatedHandlerID)
			{
				if (string.IsNullOrEmpty(perID))
				{
					continue;
				}
				GameplayEventManager.Instance.StopGameplayCommand(perID);
			}
		}
	}
}