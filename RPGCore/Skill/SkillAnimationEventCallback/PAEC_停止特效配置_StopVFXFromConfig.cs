using System;
using ARPG.Character.Base;
using ARPG.Equipment;
using RPGCore;
using RPGCore.Skill;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Player
{
	
	[Serializable]
	public class PAEC_停止特效配置_StopVFXFromConfig : BasePlayerAnimationEventCallback
	{
		[SerializeField, LabelText("需要停止的特效配置"), GUIColor(187f / 255f, 1f, 0f)]
		public string _vfx_Stop;

		[SerializeField, LabelText("立刻停止？")]
		public bool _stopImmediate = false;


		public override BasePlayerAnimationEventCallback ExecuteBySkill(
			BaseARPGCharacterBehaviour behaviour,
			BaseRPSkill skillRef,
			bool createNewWhenExist = false)
		{
			PerVFXInfo targetVFXInfo = null;
			//targetVFXInfo = skillRef._VFX_JustGet(_vfx_Stop)?.VFX_StopThis(_stopImmediate);
			targetVFXInfo = skillRef?._VFX_StopAllSameUid(_vfx_Stop, _stopImmediate);
			return this;
		}
		public override BasePlayerAnimationEventCallback ExecuteByWeapon(
			BaseARPGCharacterBehaviour behaviour,
			BaseWeaponHandler weaponHandler,
			bool createNewWhenExist = false)
		{
			
			PerVFXInfo targetVFXInfo = null;
			//argetVFXInfo = weaponHandler._VFX_PS_JustGet(_vfx_Stop)?.VFX_StopThis(_stopImmediate);
			targetVFXInfo = weaponHandler._VFX_PS_StopAllSameUid(_vfx_Stop, _stopImmediate);
			return this;
		}



		public override string GetElementNameInList()
		{
			return $"{GetBaseCustomName()} {(_stopImmediate ? "立刻": "" )} 停止特效：_{_vfx_Stop}_";
		}
	}
}