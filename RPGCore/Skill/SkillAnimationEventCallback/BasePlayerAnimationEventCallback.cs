using System;
using ARPG.Character.Base;
using ARPG.Equipment;
using RPGCore;
using RPGCore.Buff;
using RPGCore.Skill;
using Sirenix.OdinInspector;
namespace ARPG.Character.Player
{
	public enum PlayerAnimationEventPresetEnumType
	{
		Start_开始 = 1, Complete_结束 = 2, Custom_自定义 = 3
	}



	[Serializable]
	public abstract class BasePlayerAnimationEventCallback
	{
		public static PlayerCharacterBehaviourController _pcbRef;
		[LabelText("需要动画匹配")]
		public bool RequireAnimationMatching = true;
		
		
		[LabelText("关联动画配置名"), GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		[ShowIf( "@this.RequireAnimationMatching")]
		public string _AN_RelatedAnimationConfigName;

		[LabelText("事件触发预设,[结束]仅会在第一次结束时触发")]
		[ShowIf("@this.RequireAnimationMatching")]
		public PlayerAnimationEventPresetEnumType AnimationEventPreset = PlayerAnimationEventPresetEnumType.Start_开始;

		[LabelText("自定义事件名"), ShowIf("@(this.AnimationEventPreset == PlayerAnimationEventPresetEnumType.Custom_自定义 && this.RequireAnimationMatching)")]
		public string CustomEventString;



		public virtual BasePlayerAnimationEventCallback ExecuteByBuff(BaseARPGCharacterBehaviour behaviour,BaseRPBuff buff )
		{
			return this;
		}

		public virtual BasePlayerAnimationEventCallback ExecuteBySkill(
			BaseARPGCharacterBehaviour behaviour,
			BaseRPSkill skillRef,
			bool createNewWhenExist = false)
		{
			return this;
		}
		public virtual BasePlayerAnimationEventCallback ExecuteByWeapon(
			BaseARPGCharacterBehaviour behaviour,
			BaseWeaponHandler weaponHandler,
			bool createNewWhenExist = false)
		{
			
			return this;
		}




		public virtual string GetElementNameInList()
		{
			return GetType().Name;
		}
		public string GetBaseCustomName()
		{
			if (RequireAnimationMatching)
			{
				if (AnimationEventPreset == PlayerAnimationEventPresetEnumType.Custom_自定义)
				{
					return $"于动画[{_AN_RelatedAnimationConfigName}] 的 [自定义事件] [{CustomEventString}]时  ";
				}
				else
				{
					return $"于动画 [{_AN_RelatedAnimationConfigName}] 的 [{AnimationEventPreset}] 时， ";
				}
			}
			else
			{
				return "直接 ";
			}
		}
		
	}
}