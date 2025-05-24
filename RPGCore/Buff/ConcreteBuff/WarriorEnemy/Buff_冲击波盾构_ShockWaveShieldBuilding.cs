using System;
using System.Collections.Generic;
using ARPG.Manager;
using Global;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Buff.ConcreteBuff.WarriorEnemy
{
	[Serializable]
	public class Buff_冲击波盾构_ShockWaveShieldBuilding : Buff_BaseShieldBuilding
	{
		[SerializeField, LabelText("充能时间"), FoldoutGroup("配置", true), TitleGroup("配置/数值"),]
		public float ChargeTime = 10f;

		[SerializeField, LabelText("警示时间"), FoldoutGroup("配置", true), TitleGroup("配置/数值")]
		public float WarningTime = 2f;

		[SerializeField, LabelText("充能时特效"), FoldoutGroup("配置", true), TitleGroup("配置/VFX"),
		 GUIColor(187f / 255f, 1f, 0f)]
		protected string _vfx_charge;

		protected bool charge_flag = true;
		protected PerVFXInfo _vfxInfo_Charge;
		
		[SerializeField, LabelText("警示时特效"), FoldoutGroup("配置", true), TitleGroup("配置/VFX"),
		 GUIColor(187f / 255f, 1f, 0f)]
		protected string _vfx_warning;

		protected PerVFXInfo _vfxInfo_Warning;


		[SerializeField, LabelText("释放时特效"), FoldoutGroup("配置", true), TitleGroup("配置/VFX"),
		 GUIColor(187f / 255f, 1f, 0f)]
		protected string _vfx_release;

		protected PerVFXInfo _vfxInfo_Release;






		[NonSerialized, LabelText("下次更换状态的时间"), FoldoutGroup("运行时"), TitleGroup("运行时/数值"), ShowInInspector, ReadOnly]
		[ListDrawerSettings(ShowFoldout = true)]
		protected List<BuffApplyInfo_Runtime> _buffApplyInfoList = new List<BuffApplyInfo_Runtime>();







		/// <summary>
		/// 下次更换状态的时间
		/// </summary>
		[NonSerialized, LabelText("下次更换状态的时间"), FoldoutGroup("运行时"), TitleGroup("运行时/数值"), ShowInInspector,
		 ReadOnly]
		protected float _nextChangeStateTime;


		public enum ShieldBuildingStateTypeEnum
		{
			NotAvailable_不可用 = 0, Charging_充能中 = 1, Warning_预警中 = 2,
		}

		[NonSerialized, LabelText("当前状态"), FoldoutGroup("运行时", true), TitleGroup("运行时/数值"), ShowInInspector, ReadOnly]
		protected ShieldBuildingStateTypeEnum _currentBuffState = ShieldBuildingStateTypeEnum.NotAvailable_不可用;

		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			
			_buffApplyInfoList.Clear();
		}


		protected override void EnableAbility()
		{
			base.EnableAbility();
			_currentBuffState = ShieldBuildingStateTypeEnum.Charging_充能中;
			_currentBuffState = ShieldBuildingStateTypeEnum.Charging_充能中;
			_nextChangeStateTime = BaseGameReferenceService.CurrentFixedTime + ChargeTime;
		}
		protected override void DisableAbility()
		{
			base.DisableAbility();
			_vfxInfo_Warning?.VFX_StopThis(true);
            _vfxInfo_Charge?.VFX_StopThis(true);
			_currentBuffState = ShieldBuildingStateTypeEnum.NotAvailable_不可用;
		}


		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			var ds_init = base.OnBuffInitialized(blps);
			foreach (BaseBuffLogicPassingComponent perBLP in blps)
			{
				switch (perBLP)
				{
					case BLP_冲击波盾构构建内容_ShockWaveShieldBuildingContent blp冲击波盾构构建内容ShockWaveShieldBuildingContent:

						ChargeTime = blp冲击波盾构构建内容ShockWaveShieldBuildingContent.ChargeTime;
						WarningTime = blp冲击波盾构构建内容ShockWaveShieldBuildingContent.WarningTime;
						//直接new进去数据就好了
						
						foreach (var perBAI in blp冲击波盾构构建内容ShockWaveShieldBuildingContent._buffApplyInfoList)
						{
							_buffApplyInfoList.Add(new BuffApplyInfo_Runtime
							{
								BuffType = perBAI.BuffType,
								BuffLogicPassingComponents = perBAI.GetFullBLPList()
							});
						}
						break;
				}
			}
			
			return ds_init;
		}

		public override void FromHolder_FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.FromHolder_FixedUpdateTick(currentTime, currentFrameCount, delta);


			if (!CurrentAbilityAvailable)
			{
				return;
			}

			switch (_currentBuffState)
			{
				case ShieldBuildingStateTypeEnum.NotAvailable_不可用:
					return;
				case ShieldBuildingStateTypeEnum.Charging_充能中:

					if (charge_flag)
					{
						_vfxInfo_Charge = _VFX_GetAndSetBeforePlay(_vfx_charge);
						float mulSimSpeed = 10 / ChargeTime;
						_vfxInfo_Charge?._VFX__6_SetSimulationSpeed(mulSimSpeed)?._VFX__10_PlayThis();
						
						charge_flag = false;
					}
					
					
					if (currentTime > _nextChangeStateTime)
					{
						_currentBuffState = ShieldBuildingStateTypeEnum.Warning_预警中;
						_vfxInfo_Charge.VFX_StopThis(true);
						charge_flag = true;
						_nextChangeStateTime = currentTime + WarningTime;
					    _vfxInfo_Warning =	_VFX_GetAndSetBeforePlay(_vfx_warning)?._VFX__10_PlayThis();
					}

					break;
				case ShieldBuildingStateTypeEnum.Warning_预警中:
					if (currentTime > _nextChangeStateTime)
					{
						_currentBuffState = ShieldBuildingStateTypeEnum.Charging_充能中;
						_VFX_GetAndSetBeforePlay(_vfx_release)?._VFX__10_PlayThis();
                        if (_vfx_warning != null)
						{
							_vfxInfo_Warning.VFX_StopThis(true);
						}
						ProcessShockWaveBuffEffect();
						_nextChangeStateTime = currentTime + ChargeTime;
					}
					break;
			}
		}






		private void ProcessShockWaveBuffEffect()
		{
			//获取当前活跃的玩家角色
			var currentPlayer = SubGameplayLogicManager_ARPG.Instance.PlayerCharacterBehaviourControllerReference
				.CurrentControllingBehaviour;

			
			//施加buff
			foreach (var perApply in _buffApplyInfoList)
			{
				currentPlayer.ReceiveBuff_TryApplyBuff(perApply.BuffType,
					Parent_SelfBelongToObject as I_RP_Buff_ObjectCanApplyBuff,
					currentPlayer,
					perApply.BuffLogicPassingComponents);
			}
		}








		[Serializable]
		public class BLP_冲击波盾构构建内容_ShockWaveShieldBuildingContent : BLP_盾构基本构建内容_BaseShieldBuildingContent
		{


			[SerializeField, LabelText("充能时间"), FoldoutGroup("配置", true), TitleGroup("配置/数值"),]
			public float ChargeTime = 10f;

			[SerializeField, LabelText("警示时间"), FoldoutGroup("配置", true), TitleGroup("配置/数值")]
			public float WarningTime = 2f;


			
			[InfoBox("记得填UID！\n常用：`:BuffLogicPassing_通用数据项修饰_GeneralDataEntryModify\n" +
			         "`:BLP_易伤传递参数_FragileOverallBLP")]
			[SerializeField, LabelText("将要施加的Buff信息"), FoldoutGroup("配置", true), TitleGroup("配置/数值"),
			 ListDrawerSettings(ShowFoldout = true, AddCopiesLastElement = true)]
			public List<ConSer_BuffApplyInfo> _buffApplyInfoList;



			public override void ReleaseOnReturnToPool()
			{
				GenericPool<BLP_冲击波盾构构建内容_ShockWaveShieldBuildingContent>.Release(this);
			}

		}




	}
}