using ARPG.Character.Base;
using ARPG.Equipment;
using GameplayEvent;
using GameplayEvent.SO;
using RPGCore;
using RPGCore.Buff;
using RPGCore.Skill;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Player
{
	public class PAEC_生成游戏事件_SpawnGameplayEvent : BasePlayerAnimationEventCallback
	{
		[SerializeField,LabelText("需要触发的事件",SdfIconType.CalendarEvent)]
		[InlineEditor(InlineEditorObjectFieldModes.Boxed),]
		public SOConfig_PrefabEventConfig RelatedConfig;

		public override BasePlayerAnimationEventCallback ExecuteBySkill(
			BaseARPGCharacterBehaviour behaviour,
			BaseRPSkill skillRef,
			bool createNewWhenExist = false)
		{
			if(RelatedConfig)
			{
				GameplayEventManager.Instance.StartGameplayEvent(RelatedConfig);
			}
			return this;
		}
		public override BasePlayerAnimationEventCallback ExecuteByWeapon(
			BaseARPGCharacterBehaviour behaviour,
			BaseWeaponHandler weaponHandler,
			bool createNewWhenExist = false)
		{
			if (RelatedConfig)
			{
				GameplayEventManager.Instance.StartGameplayEvent(RelatedConfig);
			}
			return this;
		}

		public override BasePlayerAnimationEventCallback ExecuteByBuff(BaseARPGCharacterBehaviour behaviour, BaseRPBuff buff)
		{
			if (RelatedConfig)
			{
				GameplayEventManager.Instance.StartGameplayEvent(RelatedConfig);
			}
			return this;
		}


		public override string GetElementNameInList()
		{
			return $"{GetBaseCustomName()} 生成游戏事件：_{RelatedConfig?.name}_";
		}
	}
}