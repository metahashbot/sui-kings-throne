using System;
using ARPG.Character.Base;
using ARPG.Common;
using ARPG.Equipment;
using RPGCore.Skill;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Player
{
	[Serializable]
	public class PAEC_刷新吸附状态_RefreshAbsorbState : BasePlayerAnimationEventCallback
	{
		[SerializeField,LabelText("刷新为该状态")]
		public PlayerAutoAbsorbConfig RefreshTo;



		public override BasePlayerAnimationEventCallback ExecuteBySkill(
			BaseARPGCharacterBehaviour behaviour,
			BaseRPSkill skillRef,
			bool createNewWhenExist = false)
		{
			return base.ExecuteBySkill(behaviour, skillRef, createNewWhenExist);
		}
		public override BasePlayerAnimationEventCallback ExecuteByWeapon(
			BaseARPGCharacterBehaviour behaviour,
			BaseWeaponHandler weaponHandler,
			bool createNewWhenExist = false)
		{
			var aec = base.ExecuteByWeapon(behaviour, weaponHandler, createNewWhenExist);
			weaponHandler.CurrentAutoAbsorbInfo.ReceiveNewConfig(RefreshTo);
			return aec;
		}

		public override string GetElementNameInList()
		{
			return $"{GetBaseCustomName()} 刷新吸附状态为：_{RefreshTo?.ToggleTo}_";
		}
	}
}