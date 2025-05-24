using System;
using System.Collections.Generic;
using ARPG.Character;
using ARPG.Manager;
using Global;
using Global.ActionBus;
using RPGCore.Buff.ConcreteBuff.Utility;
using RPGCore.DataEntry;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.DropItem
{
	[Serializable]
	public abstract class BaseDropItemPickedEffect
	{[NonSerialized]
		public PerDropItemConfig RelatedDropItemConfig;
		public abstract void OnPickup(PlayerARPGConcreteCharacterBehaviour player, PerDropItemConfig config);
	}

	[Serializable]
	public class DIPE_向玩家施加Buff_ApplyBuffOnPlayer : BaseDropItemPickedEffect
	{
		[SerializeField]
		public List<ConSer_BuffApplyInfo> ApplyInfos = new List<ConSer_BuffApplyInfo>();


		[SerializeField,LabelText("对全队生效？")]
		public bool ApplyOnWholeTeam = false;

		
		public override void OnPickup(PlayerARPGConcreteCharacterBehaviour player, PerDropItemConfig config)
		{
			if (ApplyOnWholeTeam)
			{
				foreach (PlayerARPGConcreteCharacterBehaviour perPlayer in SubGameplayLogicManager_ARPG.Instance.PlayerCharacterBehaviourControllerReference.CurrentAllCharacterBehaviourList)
				{
					foreach (ConSer_BuffApplyInfo perApply in ApplyInfos)
					{
						perPlayer.ReceiveBuff_TryApplyBuff(perApply.BuffType,
							perPlayer,
							perPlayer,
							perApply.GetFullBLPList());
					}
				}
			}
			else
			{
				foreach (ConSer_BuffApplyInfo perApply in ApplyInfos)
				{
					player.ReceiveBuff_TryApplyBuff(perApply.BuffType,
						player,
						player,
						perApply.GetFullBLPList());
				}
			}
		}
	}

	[Serializable]
	public class RestoreHPByCount : BaseDropItemPickedEffect
	{
		[LabelText("按数值恢复这么多血量"), SerializeField]
		public float RestoreHPCount = 10f;




		public override void OnPickup(PlayerARPGConcreteCharacterBehaviour player, PerDropItemConfig config)
		{
			var dai = RP_DS_DamageApplyInfo.BuildDamageApplyInfoFromRuntime(player,
				player,
				DamageTypeEnum.Heal_治疗,
				RestoreHPCount);
			dai.StepOption = DamageProcessStepOption.HealDPS();
			dai.DropItemInfoRef = config;
			dai.DamageWorldPosition = player.transform.position;
			dai.DamageTakenBase = RestoreHPCount;
			SubGameplayLogicManager_ARPG.Instance.DamageAssistServiceInstance.ApplyDamage(dai);
		}
	}


	[Serializable]
	public class RestoreHPByPartial : BaseDropItemPickedEffect
	{
		[LabelText("按百分比恢复这么多血量"), SerializeField, SuffixLabel("%")]
		public float RestoreHPPartial = 10f;

		public override void OnPickup(PlayerARPGConcreteCharacterBehaviour player, PerDropItemConfig config)
		{
			var chp = player.GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentHP_当前HP);
			var hpMax = player.ReleaseSkill_GetRelatedFloatDataEntry(RP_DataEntry_EnumType.HPMax_最大HP);
			float restoreAmount = hpMax.CurrentValue * (RestoreHPPartial / 100f);

			var dai = RP_DS_DamageApplyInfo.BuildDamageApplyInfoFromRuntime(player,
				player,
				DamageTypeEnum.Heal_治疗,
				restoreAmount);
			dai.StepOption = DamageProcessStepOption.HealDPS();
			dai.DropItemInfoRef = config;
			dai.DamageWorldPosition = player.transform.position;
			dai.DamageTakenBase = restoreAmount;
			SubGameplayLogicManager_ARPG.Instance.DamageAssistServiceInstance.ApplyDamage(dai);
		}
	}

	[Serializable]
	public class RestoreSPByCount : BaseDropItemPickedEffect
	{
		[LabelText("按数值恢复这么多SP"), SerializeField]
		public float RestoreSPCount = 10f;

		[LabelText("跳字？"), SerializeField]
		public bool Popup = true;

		public override void OnPickup(PlayerARPGConcreteCharacterBehaviour player, PerDropItemConfig config)
		{
			FloatPresentValue_RPDataEntry spEntry =
				player.GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentSP_当前SP) as FloatPresentValue_RPDataEntry;
			spEntry.AddDataEntryModifier(Float_ModifyEntry_RPDataEntry.GetNewFromPool(RestoreSPCount,
				RPDM_DataEntry_ModifyFrom.FromDropItem_来自掉落物,
				ModifyEntry_CalculatePosition.FrontAdd));
			if (Popup)
			{
				var ds_pop = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_DamageHint_PopupSPEntry_SP条目跳字);
				ds_pop.ObjectArgu1 = player;
				ds_pop.FloatArgu1 = RestoreSPCount;
				GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_pop);
			}
			
		}
	}

	[Serializable]
	public class RestoreSPByPartial : BaseDropItemPickedEffect
	{
		[LabelText("按百分比恢复这么多SP"), SerializeField, SuffixLabel("%")]
		public float RestoreSPPartial = 10f;

		[LabelText("跳字？"), SerializeField]
		public bool Popup = true;

		public override void OnPickup(PlayerARPGConcreteCharacterBehaviour player, PerDropItemConfig config)
		{
			var csp = player.GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentSP_当前SP);
			var maxSP = player.GetFloatDataEntryByType(RP_DataEntry_EnumType.SPMax_最大SP);
			float pValue = maxSP.CurrentValue * (RestoreSPPartial / 100f);
			csp.AddDataEntryModifier(Float_ModifyEntry_RPDataEntry.GetNewFromPool(pValue,
				RPDM_DataEntry_ModifyFrom.FromDropItem_来自掉落物,
				ModifyEntry_CalculatePosition.FrontAdd));
			if (Popup)
			{
				var ds_pop = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_DamageHint_PopupSPEntry_SP条目跳字);
				ds_pop.ObjectArgu1 = player;
				ds_pop.FloatArgu1 = pValue;
				GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_pop);
			}
		} 
	}
	
	[Serializable]
	public class RestoreUPByCount : BaseDropItemPickedEffect
	{
		[LabelText("恢复超能这么多秒的CD"), SerializeField]
		public float RestoreUPCount = 10f;

		public override void OnPickup(PlayerARPGConcreteCharacterBehaviour player, PerDropItemConfig config)
		{
			var baseBuff =
				player.ReceiveBuff_GetTargetBuff(RolePlay_BuffTypeEnum.ARPG_PlayerUltraPowerUtility_玩家基本UP功能) as
					Buff_PlayerUltraPowerUtility;
			baseBuff.RestoreUltraPowerCDByPartial(RestoreUPCount);
		}
	}
	
	[Serializable]
	public class RestoreUPByPartial : BaseDropItemPickedEffect
	{
		[LabelText("按百分比恢复这么多超能的CD"), SerializeField, SuffixLabel("%")]
		public float RestoreUPPartial = 10f;

		public override void OnPickup(PlayerARPGConcreteCharacterBehaviour player, PerDropItemConfig config)
		{
			var baseBuff =
				player.ReceiveBuff_GetTargetBuff(RolePlay_BuffTypeEnum.ARPG_PlayerUltraPowerUtility_玩家基本UP功能) as
					Buff_PlayerUltraPowerUtility;
			baseBuff.RestoreUltraPowerCDByPartial(RestoreUPPartial / 100f);
			
		}
	}

	[Serializable]
	public class SpawnVFX_拾取时生成特效 : BaseDropItemPickedEffect
	{

		[LabelText("生成特效预制件")]
		public GameObject prefab_VFX;

		[LabelText("额外位置修正(+)"), SerializeField]
		public Vector3 ExtraPosOffset;

		[LabelText("尺寸乘算"), SerializeField]
		public float ExtraScaleMultiply = 1f;
		
		
		
		
		public override void OnPickup(PlayerARPGConcreteCharacterBehaviour player, PerDropItemConfig config)
		{
			if (prefab_VFX)
			{
				var ps = VFXPoolManager.Instance.GetParticleSystemRuntimeByPrefab(prefab_VFX);
				var t = player.GetRelatedVFXContainer()
					.GetVFXHolderTransformAndRegisterVFX(ConSer_VFXHolderInfo._VFXAnchorName_OnlyScale, ps, true);
				ps.transform.SetParent(t.Item1);
				ps.transform.localPosition = Vector3.zero;
				ps.transform.localPosition += ExtraPosOffset;
				ps.transform.localScale *= (ExtraScaleMultiply * t.Item2);
			}
		}
	}
	
	
}