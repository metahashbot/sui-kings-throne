using System;
using ARPG.Character.Base;
using ARPG.Equipment;
using GameplayEvent;
using RPGCore.Skill;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Player
{
	[TypeInfoBox("会对蓄力进行一些数值上的缩放。并不兼容所有GEH，通常只兼容一些和镜头演出有关的GEH\n" +
	             "当前兼容：\n" +
	             "屏幕振动、")]
	[Serializable]
	public class PAEC_响应蓄力的事件触发_LaunchEventAffectByCharge : PAEC_生成游戏事件_SpawnGameplayEvent
	{


		[Header("===蓄力===")]
		[SerializeField, LabelText("√:受前摇蓄力影响 | 口:受施法蓄力影响")]
		public bool AffectByPrepare = true;

		[SerializeField, LabelText("乘数1 ： 曲线Y：蓄满的乘数")]
		public float MulAtFullCharge_1 = 3f;

		[SerializeField, LabelText("乘数2 ： 曲线：乘数随蓄满的曲线")]
		public AnimationCurve SizeMultiplyCurve_1 = AnimationCurve.Linear(0f, 0.33f, 1f, 1f);


		[SerializeField, LabelText("乘数2 ： 曲线Y：蓄满的乘数")]
		public float MulAtFullCharge_2 = 3f;

		[SerializeField, LabelText("乘数2 ： 曲线：乘数随蓄满的曲线")]
		public AnimationCurve SizeMultiplyCurve_2 = AnimationCurve.Linear(0f, 0.33f, 1f, 1f);
		
		
		public override BasePlayerAnimationEventCallback ExecuteByWeapon(
			BaseARPGCharacterBehaviour behaviour,
			BaseWeaponHandler weaponHandler,
			bool createNewWhenExist = false)
		{
			return base.ExecuteByWeapon(behaviour, weaponHandler, createNewWhenExist);
		}




		public override string GetElementNameInList()
		{
			return $"{GetBaseCustomName()} 响应蓄力的事件触发";
		}





	}
}