using System;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.Common
{
	[Serializable]
	public class WARResult_效果之播放特效配置_WeaknessResultSpawnVFXConfig : BaseWeaknessAffectRule , I_WeaknessComponentAsResult
	{
		[SerializeField,LabelText("将要播放的配置ID"), GUIColor(187f / 255f, 1f, 0f)]
		public string SpawnVFXConfigID;

		[NonSerialized,LabelText("关联特效引用"),ShowInInspector,ReadOnly]
		public PerVFXInfo RelatedVFXInfo;

		
		public static WARResult_效果之播放特效配置_WeaknessResultSpawnVFXConfig CreateNew(string spawnVfxConfigID)
		{
			var newRule = new WARResult_效果之播放特效配置_WeaknessResultSpawnVFXConfig();
			newRule.SpawnVFXConfigID = spawnVfxConfigID;
			return newRule;
		}
		
		public override void UnloadAndClearBeforeRemove(Buff_通用敌人弱点_CommonEnemyWeakness relatedBuff)
		{
			if (RelatedVFXInfo != null)
			{
				RelatedVFXInfo.VFX_StopThis(true);
			}
		}
		
		public void RegisterToGroup(Buff_通用敌人弱点_CommonEnemyWeakness.WeaknessInfoGroup group)
		{
			var newCopy = CreateNew( SpawnVFXConfigID);
			newCopy.RelatedBuffRef = group.RelatedBuff;
			newCopy.RelatedGroupRef = group;
			group.AddResultRule(newCopy);
		}
		
		
		public void TriggerWeaknessResult()
		{
			RelatedVFXInfo = RelatedBuffRef._VFX_GetAndSetBeforePlay(SpawnVFXConfigID)._VFX__10_PlayThis(true, true);
		}
	}
}