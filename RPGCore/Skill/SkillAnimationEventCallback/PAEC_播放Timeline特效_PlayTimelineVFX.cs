using System;
using System.Collections.Generic;
using ARPG.Character.Base;
using ARPG.Equipment;
using RPGCore.Skill;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Timeline;
namespace ARPG.Character.Player
{
	[Serializable]
	public class PAEC_播放Timeline特效_PlayTimelineVFX : BasePlayerAnimationEventCallback
	{
		[SerializeField, LabelText("需要播放的Timeline"),]
		public TimelineAsset RelatedTimeline;

		[SerializeField, LabelText("匹配当前伤害类型？")]
		public bool MatchDamageType = false;

		[SerializeField, LabelText("匹配的伤害类型们")]
		[ShowIf("MatchDamageType")]
		public List<DamageTypeEnum> DamageTypeList = new List<DamageTypeEnum>();
		
		[SerializeField,LabelText("√:使用瞄准方向 | 口:只使用左右方向")]
		public bool UseAimDirection = true;


		public override BasePlayerAnimationEventCallback ExecuteBySkill(
			BaseARPGCharacterBehaviour behaviour,
			BaseRPSkill skillRef,
			bool createNewWhenExist = false)
		{
			return this;
		}
		public override BasePlayerAnimationEventCallback ExecuteByWeapon(
			BaseARPGCharacterBehaviour behaviour,
			BaseWeaponHandler weaponHandler,
			bool createNewWhenExist = false)
		{
			return this;
		}


	}
}