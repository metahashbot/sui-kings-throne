using System;
using RPGCore.Buff.ConcreteBuff.Utility;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using RPGCore.UtilityDataStructure.BuffLogicPassingComponent;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Buff.ConcreteBuff.Common
{
	[Serializable]
	public class WARResult_效果之施加眩晕_WeaknessResultOnDizzy : BaseWeaknessAffectRule, I_WeaknessComponentAsResult
	{

		[SerializeField, LabelText("将要施加的眩晕时长"), TitleGroup("===配置===")]
		public float DizzyDuration = 3f;
		[SerializeField, LabelText("眩晕Buff特效不使用默认特效"), TitleGroup("===配置===")]
		public bool DizzyBuffNotUseDefaultVFXID = false;
		[SerializeField, LabelText("眩晕Buff使用的其他特效ID"), ShowIf(nameof(DizzyBuffNotUseDefaultVFXID)),
		 TitleGroup("===配置===")]
		public string SpecificDizzyVFXID;
		 
		
		[SerializeField,LabelText("眩晕后再次开启")]
		[TitleGroup("===配置===")]
		 public bool ReOpenAfterDizzy = false;

		public static WARResult_效果之施加眩晕_WeaknessResultOnDizzy CreateNew(
			WARResult_效果之施加眩晕_WeaknessResultOnDizzy copyFrom)
		{
			var newRule = new WARResult_效果之施加眩晕_WeaknessResultOnDizzy();
			newRule.DizzyDuration = copyFrom.DizzyDuration;
			newRule.DizzyBuffNotUseDefaultVFXID = copyFrom.DizzyBuffNotUseDefaultVFXID;
			newRule.SpecificDizzyVFXID = copyFrom.SpecificDizzyVFXID;
			newRule.ReOpenAfterDizzy = copyFrom.ReOpenAfterDizzy;

			return newRule;
		}

		public void RegisterToGroup(Buff_通用敌人弱点_CommonEnemyWeakness.WeaknessInfoGroup group)
		{
			var newCopy = CreateNew(this);
			newCopy.RelatedBuffRef =group. RelatedBuff;
			newCopy.RelatedGroupRef = group;
			 
			group.AddResultRule(newCopy);
		}

		public override void UnloadAndClearBeforeRemove(Buff_通用敌人弱点_CommonEnemyWeakness relatedBuff)
		{
		}


		public void TriggerWeaknessResult()
		{
			Buff_眩晕状态_DizzyState.BLP_可指定特效的眩晕参数_DizzyUseSpecificVFXID blp_vfx = null;
			blp_vfx = GenericPool<Buff_眩晕状态_DizzyState.BLP_可指定特效的眩晕参数_DizzyUseSpecificVFXID>.Get();

			if (DizzyBuffNotUseDefaultVFXID)
			{
				blp_vfx.ContainNonDefaultVFX = true;
				blp_vfx.NewUID = SpecificDizzyVFXID;
			}
			blp_vfx.DizzyDuration = DizzyDuration;


			Buff_眩晕状态_DizzyState.BLP_眩晕后恢复常驻弱点_ReOpenWeaknessAfterDizzy blp_reopen = null;
			blp_reopen = GenericPool<Buff_眩晕状态_DizzyState.BLP_眩晕后恢复常驻弱点_ReOpenWeaknessAfterDizzy>.Get();
			
			RelatedBuffRef.Parent_SelfBelongToObject.ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.Dizzy_眩晕,
				RelatedBuffRef.Parent_SelfBelongToObject as I_RP_Buff_ObjectCanApplyBuff,
				RelatedBuffRef.Parent_SelfBelongToObject as I_RP_Buff_ObjectCanReceiveBuff,
				blp_vfx,
				blp_reopen);


			GenericPool<Buff_眩晕状态_DizzyState.BLP_可指定特效的眩晕参数_DizzyUseSpecificVFXID>.Release(blp_vfx);
			GenericPool<Buff_眩晕状态_DizzyState.BLP_眩晕后恢复常驻弱点_ReOpenWeaknessAfterDizzy>.Release(blp_reopen);
			 

		}
	}
}