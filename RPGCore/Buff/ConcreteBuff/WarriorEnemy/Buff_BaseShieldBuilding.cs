using System;
using System.Collections.Generic;
using Global;
using Global.ActionBus;
using RPGCore.Buff.ConcreteBuff.Element;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Buff.ConcreteBuff.WarriorEnemy
{
	[Serializable]
	[TypeInfoBox("基本的勇士机制——盾构")]
	public class Buff_BaseShieldBuilding : BaseRPBuff 
	{
		[SerializeField, LabelText("常驻特效"), FoldoutGroup("配置", true), TitleGroup("配置/VFX"),
		 GUIColor(187f / 255f, 1f, 0f)]
		protected string _Vfx_ShieldLoop;
		
		protected PerVFXInfo _vfxInfo_ShieldLoop;
		
		
		[SerializeField, LabelText("能力被破坏时特效"), FoldoutGroup("配置", true), TitleGroup("配置/VFX"),
		 GUIColor(187f / 255f, 1f, 0f)]
		protected string _vfx_AbilityBreak;
		
		protected PerVFXInfo _vfxInfo_AbilityBreak;
		
		[SerializeField,LabelText("盾本体:免伤比例"),FoldoutGroup("配置",true),TitleGroup("配置/数值"),SuffixLabel("%")]
		protected float _shieldDamageReduceRate = 50f;
		
		
		
		
		[SerializeField,LabelText("重新恢复效果的时长") ,FoldoutGroup("配置",true),TitleGroup("配置/数值"),SuffixLabel("秒")]
		protected float _recoveryDuration = 10f;
		protected float _willRecoverTime;
        
        
        
        
        
        
        
        
		[NonSerialized,LabelText("当前能力可用吗")]
		public bool CurrentAbilityAvailable = true;
		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_Buff_OnBuffInitialized_一个Buff被确定初始化了,
				_ABC_DisableAbility_OnCrush);
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeRearMul_对接收方将要进行后乘最终算区计算,
				_ABC_ProcessDamageResistanceOnShield);
			
			EnableAbility();

			return base.OnBuffInitialized(blps);
		}


		protected virtual void _ABC_ProcessDamageResistanceOnShield(DS_ActionBusArguGroup ds)
		{
			if (!CurrentAbilityAvailable)
			{
				return;
			}
			var dar = ds.GetObj1AsT<RP_DS_DamageApplyResult>();
			dar.CP_DamageAmount_DPart.MultiplyPart -= _shieldDamageReduceRate / 100f;
		}





		/// <summary>
		/// <para>当崩解发生，禁用这项能力</para>
		/// </summary>
		protected virtual void _ABC_DisableAbility_OnCrush(DS_ActionBusArguGroup ds)
		{
			var targetBuffType = (RolePlay_BuffTypeEnum)ds.IntArgu1.Value;
			if (targetBuffType == RolePlay_BuffTypeEnum.ElementSecondCrush_BengJie_二级崩解)
			{
				DisableAbility();
			}
		}

		

		protected virtual void EnableAbility()
		{
			CurrentAbilityAvailable = true;
			_vfxInfo_ShieldLoop = _VFX_GetAndSetBeforePlay(_Vfx_ShieldLoop,true)._VFX__10_PlayThis();

		}


		/// <summary>
		/// <para>具体的禁用能力，通常包括停止bool以不再tick，并播放相关特效</para>
		/// </summary>
		protected virtual void DisableAbility()
		{
			if (CurrentAbilityAvailable)
			{
				_willRecoverTime = BaseGameReferenceService.CurrentFixedTime + _recoveryDuration;
                CurrentAbilityAvailable = false;
                _vfxInfo_ShieldLoop.VFX_StopThis(true);
                _vfxInfo_AbilityBreak = _VFX_GetAndSetBeforePlay(_vfx_AbilityBreak)._VFX__10_PlayThis();
			}
		}


		public override void FromHolder_FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			if (!CurrentAbilityAvailable && currentTime > _willRecoverTime)
			{
				EnableAbility();
			}
			base.FromHolder_FixedUpdateTick(currentTime, currentFrameCount, delta);
		}


		protected override void ClearAndUnload()
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_Buff_OnBuffInitialized_一个Buff被确定初始化了,
				_ABC_DisableAbility_OnCrush);
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeRearMul_对接收方将要进行后乘最终算区计算,
				_ABC_ProcessDamageResistanceOnShield);

			base.ClearAndUnload();
		}



		[Serializable]
		public class BLP_盾构基本构建内容_BaseShieldBuildingContent : BaseBuffLogicPassingComponent
		{
			
			[SerializeField, LabelText("盾本体:免伤比例"), FoldoutGroup("配置", true), TitleGroup("配置/数值"), SuffixLabel("%")]
			public float _shieldDamageReduceRate = 50f;

			public override void ReleaseOnReturnToPool()
			{
				GenericPool<BLP_盾构基本构建内容_BaseShieldBuildingContent>.Release(this);
			}
		}
	}
	
	
}