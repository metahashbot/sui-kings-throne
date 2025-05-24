using System;
using System.Collections.Generic;
using ARPG.Character.Enemy.AI.Decision.DecisionComponent;
using ARPG.Character.Player;
using ARPG.Character.Player.Ally;
using ARPG.Manager;
using ARPG.Manager.Component;
using Global.ActionBus;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Listen
{
	[Serializable]
	[TypeInfoBox("距离监听——与仇恨目标的距离监听")]
	public class AIListen_PlayerDistanceListen : BaseAIListenComponent, I_AIListenNeedTick
	{

		[NonSerialized, ShowInInspector, LabelText("当前监听有效吗？"), FoldoutGroup("运行时", true)]
		public bool ListenValid;
#region 静态部分

		protected static CharacterOnMapManager _characterOnMapManagerRef;

#endregion

[SerializeField, LabelText("监听需要玩家也处于同一个区域"), FoldoutGroup("配置", true)]
public bool RequirePlayerInsideSameArea = true;

private static AreaInfo_SubActivityService _areaInfoActivityServiceRef;
private string _selfRelatedSpawnAreaID;
private bool _needPlayerCheck = false;

		[SerializeField, LabelText("√按秒数tick；口：按帧数Tick"), FoldoutGroup("配置", true)]
		public bool TickBySecond = true;

		[SerializeField, LabelText("Tick间隔帧数"), HideIf(nameof(TickBySecond)), FoldoutGroup("配置", true)]
		public int TickIntervalFrame = 5;

		[SerializeField, LabelText("Tick间隔秒数"), ShowIf(nameof(TickBySecond)), FoldoutGroup("配置", true)]
		public float TickIntervalTime = 0.2f;

		[ShowInInspector, LabelText("下次Tick时间点"), ShowIf(nameof(TickBySecond)), FoldoutGroup("运行时", true)]
		public float NextTickTime;
		[ShowInInspector, LabelText("下次Tick 帧数点"), HideIf(nameof(TickBySecond)), FoldoutGroup("运行时", true)]
		public int NextTickFrame;
		
		
		
#region 范围1

		[SerializeField, LabelText("距离1初始[进入]值"), FoldoutGroup("配置/距离1")]
		public float Range1InitValue_Enter = 2f;
		
		[NonSerialized]
		public float Range1CurrentEnterValue;
		
		[SerializeField, LabelText("初始状态触发吗"), FoldoutGroup("配置/距离1")]
		public bool Range1InitTrigger = false;
		//
		// [SerializeReference, LabelText("进入距离1：触发的副作用组件们【运行时只读】"), FoldoutGroup("配置/距离1")]
		// public List<BaseDecisionCommonComponent> Enter1_CommonComponents;

		[SerializeField, LabelText("【进入】距离1"), FoldoutGroup("配置/距离1")]
		 
		public DCCConfigInfo Enter1_DCCConfig;
		
		// [SerializeReference, LabelText("离开距离1：触发的副作用组件们【运行时只读】"), FoldoutGroup("配置/距离1")]
		// public List<BaseDecisionCommonComponent> Exit1_CommonComponents;
		
		[SerializeField, LabelText("【离开】距离1"), FoldoutGroup("配置/距离1")]
		public DCCConfigInfo Exit1_DCCConfig;
		

#endregion

#region 范围2

		[Header("【范围2】")] [GUIColor(205f/255f, 201f / 255f, 87f / 255f)]
		[SerializeField, LabelText("包含范围2的监听吗"), FoldoutGroup("配置/范围2", true)]
		public bool ContainRange2;

		[SerializeField, LabelText("距离2初始[进入]值"), FoldoutGroup("配置/范围2", true), ShowIf(nameof(ContainRange2))]
		public float Range2InitEnterValue = 4f;
		
		[NonSerialized]
		public float Range2CurrentEnterValue;
		
		[SerializeField, LabelText("初始状态触发吗"), FoldoutGroup("配置/范围2", true), ShowIf(nameof(ContainRange2))]
		public bool Range2InitTrigger = false;

		// [SerializeReference, LabelText("进入距离2：触发的副作用组件们【运行时只读】"), FoldoutGroup("配置/范围2", true),
		//  ShowIf(nameof(ContainRange2))]
		// public List<BaseDecisionCommonComponent> Enter2_CommonComponents;

		[SerializeField, LabelText("【进入】距离2"), FoldoutGroup("配置/范围2", true), ShowIf(nameof(ContainRange2))]
		public DCCConfigInfo DCCConfig_Enter2;
		
		// [SerializeReference, LabelText("离开距离2：触发的副作用组件们【运行时只读】"), FoldoutGroup("配置/范围2", true),
		//  ShowIf(nameof(ContainRange2))]
		// public List<BaseDecisionCommonComponent> Exit2_CommonComponents;
		
		[SerializeField, LabelText("【离开】距离2"), FoldoutGroup("配置/范围2", true), ShowIf(nameof(ContainRange2))]
		public DCCConfigInfo DCCConfig_Exit2;

#endregion

#region 范围3

		[SerializeField, LabelText("包含范围3的监听吗"), FoldoutGroup("配置/范围3", true)]
		public bool ContainRange3;

		[SerializeField, LabelText("距离3初始[进入]值"), FoldoutGroup("配置/范围3", true), ShowIf(nameof(ContainRange3))]
		public float Range3InitEnterValue = 4f;
		
		[NonSerialized]
		public float Range3CurrentEnterValue;

		[SerializeField, LabelText("初始状态触发吗"), FoldoutGroup("配置/范围3", true), ShowIf(nameof(ContainRange3))]
		public bool Range3InitTrigger = false;

		// [SerializeReference, LabelText("进入距离3：触发的副作用组件们【运行时只读】"), FoldoutGroup("配置", true),
		//  ShowIf(nameof(ContainRange3))]
		// public List<BaseDecisionCommonComponent> Enter3_CommonComponents;
		
		[SerializeField, LabelText("【进入】距离3"), FoldoutGroup("配置/范围3", true), ShowIf(nameof(ContainRange3))]
		public DCCConfigInfo DCCConfig_Enter3;

		// [SerializeReference, LabelText("离开距离3：触发的副作用组件们【运行时只读】"), FoldoutGroup("配置", true),
		//  ShowIf(nameof(ContainRange3))]
		// public List<BaseDecisionCommonComponent> Exit3_CommonComponents;
		
		[SerializeField, LabelText("【离开】距离3"), FoldoutGroup("配置/范围3", true), ShowIf(nameof(ContainRange3))]
		public DCCConfigInfo DCCConfig_Exit3;
#endregion


#region 范围4

		[SerializeField, LabelText("包含范围4的监听吗"), FoldoutGroup("配置/范围4", true)]
		public bool ContainRange4;

		[SerializeField, LabelText("距离4初始[进入]值"), FoldoutGroup("配置/范围4", true), ShowIf(nameof(ContainRange4))]
		public float Range4InitEnterValue = 6f;


		[NonSerialized]
		public float Range4CurrentEnterValue;

		[SerializeField, LabelText("初始状态触发吗"), FoldoutGroup("配置/范围4", true), ShowIf(nameof(ContainRange4))]
		public bool Range4InitTrigger = false;

		// [SerializeReference, LabelText("进入距离4：触发的副作用组件们【运行时只读】"), FoldoutGroup("配置", true),
		//  ShowIf(nameof(ContainRange4))]
		// public List<BaseDecisionCommonComponent> Enter4_CommonComponents;
		
		[SerializeField, LabelText("【进入】距离4"), FoldoutGroup("配置/范围4", true), ShowIf(nameof(ContainRange4))]
		public DCCConfigInfo DCCConfig_Enter4;

		// [SerializeReference, LabelText("离开距离4：触发的副作用组件们【运行时只读】"), FoldoutGroup("配置", true),
		//  ShowIf(nameof(ContainRange4))]
		// public List<BaseDecisionCommonComponent> Exit4_CommonComponents;
		
		[SerializeField, LabelText("【离开】距离4"), FoldoutGroup("配置/范围4", true), ShowIf(nameof(ContainRange4))]
		public DCCConfigInfo DCCConfig_Exit4;
#endregion

#region 范围5

		[SerializeField, LabelText("包含范围5的监听吗"), FoldoutGroup("配置/范围5", true)]
		public bool ContainRange5;
		[SerializeField, LabelText("距离5初始[进入]值"), FoldoutGroup("配置/范围5", true), ShowIf(nameof(ContainRange5))]
		public float Range5InitEnterValue = 8f;
		
		[NonSerialized]
		public float Range5CurrentEnterValue;
		[SerializeField, LabelText("初始状态触发吗"), FoldoutGroup("配置/范围5", true), ShowIf(nameof(ContainRange5))]
		public bool Range5InitTrigger = false;
		// [SerializeReference, LabelText("进入距离5：触发的副作用组件们【运行时只读】"), FoldoutGroup("配置", true),
		//  ShowIf(nameof(ContainRange5))]
		// public List<BaseDecisionCommonComponent> Enter5_CommonComponents;
		
		[SerializeField, LabelText("【进入】距离5"), FoldoutGroup("配置/范围5", true), ShowIf(nameof(ContainRange5))]
		public DCCConfigInfo DCCConfig_Enter5;
		// [SerializeReference, LabelText("离开距离5：触发的副作用组件们【运行时只读】"), FoldoutGroup("配置", true),
		//  ShowIf(nameof(ContainRange5))]
		// public List<BaseDecisionCommonComponent> Exit5_CommonComponents;
		
		[SerializeField, LabelText("【离开】距离5"), FoldoutGroup("配置/范围5", true), ShowIf(nameof(ContainRange5))]
		public DCCConfigInfo DCCConfig_Exit5;
#endregion

#region 范围6

		[SerializeField, LabelText("包含范围6的监听吗"), FoldoutGroup("配置/范围6", true)]
		public bool ContainRange6;
		[SerializeField, LabelText("距离6初始[进入]值"), FoldoutGroup("配置/范围6", true), ShowIf(nameof(ContainRange6))]
		public float Range6InitEnterValue = 10f;
		[NonSerialized]
		public float Range6CurrentEnterValue;
		[SerializeField, LabelText("初始状态触发吗"), FoldoutGroup("配置/范围6", true), ShowIf(nameof(ContainRange6))]
		public bool Range6InitTrigger = false;
		// [SerializeReference, LabelText("进入距离6：触发的副作用组件们【运行时只读】"), FoldoutGroup("配置", true),
		//  ShowIf(nameof(ContainRange6))]
		// public List<BaseDecisionCommonComponent> Enter6_CommonComponents;
		//  
		[SerializeField, LabelText("【进入】距离6"), FoldoutGroup("配置/范围6", true), ShowIf(nameof(ContainRange6))]
		public DCCConfigInfo DCCConfig_Enter6;
		// [SerializeReference, LabelText("离开距离6：触发的副作用组件们【运行时只读】"), FoldoutGroup("配置", true),
		//  ShowIf(nameof(ContainRange6))]
		// public List<BaseDecisionCommonComponent> Exit6_CommonComponents;

		[SerializeField, LabelText("【离开】距离6"), FoldoutGroup("配置/范围6", true), ShowIf(nameof(ContainRange6))]
		public DCCConfigInfo DCCConfig_Exit6;
		 

#endregion
		
		
		

		[ShowInInspector, NonSerialized, LabelText("上次检测的距离Sqr"), FoldoutGroup("运行时", true)]
		private float _lastCheckDistanceSqr = 999999f;





		private LocalActionBus _labRef;
		public override void InitializeAndProcessRegister(SOConfig_AIBrain brainRef)
		{
			base.InitializeAndProcessRegister(brainRef);
			_characterOnMapManagerRef = SubGameplayLogicManager_ARPG.Instance.CharacterOnMapManagerReference;
			ListenValid = true;

			NextTickFrame = 0;
			NextTickTime = 0;

			_areaInfoActivityServiceRef = SubGameplayLogicManager_ARPG.Instance.ActivityManagerArpgInstance
				.AreaInfoSubActivityServiceComponentRef;
			

			if (RequirePlayerInsideSameArea && brainRef.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour is EnemyARPGCharacterBehaviour enemy)
			{
				_needPlayerCheck = true;	
				_selfRelatedSpawnAreaID = enemy.RelatedAreaID;
			}




			Enter1_DCCConfig.ProcessRuntimeBuild();
			Exit1_DCCConfig.ProcessRuntimeBuild();
			;
			if (ContainRange2)
			{
				DCCConfig_Enter2.ProcessRuntimeBuild();
				DCCConfig_Exit2.ProcessRuntimeBuild();
			}
			if (ContainRange3)
			{
				DCCConfig_Enter3.ProcessRuntimeBuild();
				DCCConfig_Exit3.ProcessRuntimeBuild();
			}
			if (ContainRange4)
			{
				DCCConfig_Enter4.ProcessRuntimeBuild();
				DCCConfig_Exit4.ProcessRuntimeBuild();
			}
			if (ContainRange5)
			{
				DCCConfig_Enter5.ProcessRuntimeBuild();
				DCCConfig_Exit5.ProcessRuntimeBuild();
			}
			if (ContainRange6)
			{
				DCCConfig_Enter6.ProcessRuntimeBuild();
				DCCConfig_Exit6.ProcessRuntimeBuild();
			}
			

			Range1CurrentEnterValue = Range1InitValue_Enter;
			Range2CurrentEnterValue = Range2InitEnterValue;
			Range3CurrentEnterValue = Range3InitEnterValue;
			Range4CurrentEnterValue = Range4InitEnterValue;
			Range5CurrentEnterValue = Range5InitEnterValue;
			Range6CurrentEnterValue = Range6InitEnterValue;


			_labRef = brainRef.BrainHandlerFunction.SelfLocalActionBusRef;

			_labRef.RegisterAction(ActionBus_ActionTypeEnum.L_AIListen_OnPlayerDistanceEnterRange1_AI监听_玩家进入了距离1,
				RelatedAIBrainRuntimeInstance.BrainHandlerFunction._ABC_GeneralCallback_RegisteredByListen);
			_labRef.RegisterAction(ActionBus_ActionTypeEnum.L_AIListen_OnPlayerDistanceExitRange1_AI监听_玩家离开了距离1,
				RelatedAIBrainRuntimeInstance.BrainHandlerFunction._ABC_GeneralCallback_RegisteredByListen);
			if (ContainRange2)
			{
				_labRef.RegisterAction(ActionBus_ActionTypeEnum.L_AIListen_OnPlayerDistanceEnterRange2_AI监听_玩家进入了距离2,
					RelatedAIBrainRuntimeInstance.BrainHandlerFunction._ABC_GeneralCallback_RegisteredByListen);
				_labRef.RegisterAction(ActionBus_ActionTypeEnum.L_AIListen_OnPlayerDistanceExitRange2_AI监听_玩家离开了距离2,
					RelatedAIBrainRuntimeInstance.BrainHandlerFunction._ABC_GeneralCallback_RegisteredByListen);
			}
			if (ContainRange3)
			{
				_labRef.RegisterAction(ActionBus_ActionTypeEnum.L_AIListen_OnPlayerDistanceEnterRange3_AI监听_玩家进入了距离3,
					RelatedAIBrainRuntimeInstance.BrainHandlerFunction._ABC_GeneralCallback_RegisteredByListen);
				_labRef.RegisterAction(ActionBus_ActionTypeEnum.L_AIListen_OnPlayerDistanceExitRange3_AI监听_玩家离开了距离3,
					RelatedAIBrainRuntimeInstance.BrainHandlerFunction._ABC_GeneralCallback_RegisteredByListen);
				
			}
			if (ContainRange4)
			{
				_labRef.RegisterAction(ActionBus_ActionTypeEnum.L_AIListen_OnPlayerDistanceEnterRange4_AI监听_玩家进入了距离4,
					RelatedAIBrainRuntimeInstance.BrainHandlerFunction._ABC_GeneralCallback_RegisteredByListen);
				_labRef.RegisterAction(ActionBus_ActionTypeEnum.L_AIListen_OnPlayerDistanceExitRange4_AI监听_玩家离开了距离4,
					RelatedAIBrainRuntimeInstance.BrainHandlerFunction._ABC_GeneralCallback_RegisteredByListen);
			}
			if (ContainRange5)
			{
				_labRef.RegisterAction(ActionBus_ActionTypeEnum.L_AIListen_OnPlayerDistanceEnterRange5_AI监听_玩家进入了距离5,
					RelatedAIBrainRuntimeInstance.BrainHandlerFunction._ABC_GeneralCallback_RegisteredByListen);
				_labRef.RegisterAction(ActionBus_ActionTypeEnum.L_AIListen_OnPlayerDistanceExitRange5_AI监听_玩家离开了距离5,
					RelatedAIBrainRuntimeInstance.BrainHandlerFunction._ABC_GeneralCallback_RegisteredByListen);
			}
			if (ContainRange6)
			{
				_labRef.RegisterAction(ActionBus_ActionTypeEnum.L_AIListen_OnPlayerDistanceEnterRange6_AI监听_玩家进入了距离6,
					RelatedAIBrainRuntimeInstance.BrainHandlerFunction._ABC_GeneralCallback_RegisteredByListen);
				_labRef.RegisterAction(ActionBus_ActionTypeEnum.L_AIListen_OnPlayerDistanceExitRange6_AI监听_玩家离开了距离6,
					RelatedAIBrainRuntimeInstance.BrainHandlerFunction._ABC_GeneralCallback_RegisteredByListen);
			}




			_lastCheckDistanceSqr = GetCurrentDistanceSqr();



			if (Range1InitTrigger)
			{
				if (_lastCheckDistanceSqr < Range1CurrentEnterValue * Range1CurrentEnterValue)
				{
					OnPlayerEnterRange1();
				}
				else
				{
					OnPlayerExitRange1();
				}
			}
			if (ContainRange2 && Range2InitTrigger)
			{
				if (_lastCheckDistanceSqr < Range2CurrentEnterValue * Range2CurrentEnterValue)
				{
					OnPlayerEnterRange2();
				}
				else
				{
					OnPlayerExitRange2();
				}
			}
			if (ContainRange3 && Range3InitTrigger)
			{
				if (_lastCheckDistanceSqr < Range3CurrentEnterValue * Range3CurrentEnterValue)
				{
					OnPlayerEnterRange3();
				}
				else
				{
					OnPlayerExitRange3();
				}
			}
			if (ContainRange4 && Range4InitTrigger)
			{
				if (_lastCheckDistanceSqr < Range4CurrentEnterValue * Range4CurrentEnterValue)
				{
					OnPlayerEnterRange4();
				}
				else
				{
					OnPlayerExitRange4();
				}
			}
			if (ContainRange5 && Range5InitTrigger)
			{
				if (_lastCheckDistanceSqr < Range5CurrentEnterValue * Range5CurrentEnterValue)
				{
					OnPlayerEnterRange5();
				}
				else
				{
					OnPlayerExitRange5();
				}
			}
			if (ContainRange6 && Range6InitTrigger)
			{
				if (_lastCheckDistanceSqr < Range6CurrentEnterValue * Range6CurrentEnterValue)
				{
					OnPlayerEnterRange6();
				}
				else
				{
					OnPlayerExitRange6();
				}
			}
			AddNextTime();
		}




		protected virtual void OnPlayerEnterRange1()
		{
			if (_needPlayerCheck &&  RequirePlayerInsideSameArea)
			{
				var check = _areaInfoActivityServiceRef.CheckIfPlayerInsideArea(_selfRelatedSpawnAreaID);
				if (check == null || check.Value == false)
				{
					return;
				}
			}
			
			DBug.Log($"AI调试：玩家距离监听——【进入】【距离1】，来自AIBrian{RelatedAIBrainRuntimeInstance.name},归属于{RelatedAIBrainRuntimeInstance.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.name}," +
			         $"包含{Enter1_DCCConfig.CommonComponents_RuntimeAll.Count}个作用组件");
			var ds_enter1 =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
					.L_AIListen_OnPlayerDistanceEnterRange1_AI监听_玩家进入了距离1);
			if(Enter1_DCCConfig.CommonComponents_RuntimeAll !=null)
			{
				//倒序遍历
				for (int i = Enter1_DCCConfig.CommonComponents_RuntimeAll.Count - 1; i >= 0; i--)
				{
					var component = Enter1_DCCConfig.CommonComponents_RuntimeAll[i];
					component.EnterComponent(RelatedAIBrainRuntimeInstance);
				}
				
			}
		

			_labRef.TriggerActionByType(ds_enter1);
		}

		protected virtual void OnPlayerExitRange1()
		{	if (_needPlayerCheck &&  RequirePlayerInsideSameArea)
			{
				var check = _areaInfoActivityServiceRef.CheckIfPlayerInsideArea(_selfRelatedSpawnAreaID);
				if (check == null || check.Value == false)
				{
					return;
				}
			}
			DBug.Log(
				$"AI调试：玩家距离监听——【离开】【距离1】，来自AIBrian{RelatedAIBrainRuntimeInstance.name},归属于{RelatedAIBrainRuntimeInstance.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.name}," +
				$"包含{Exit1_DCCConfig.CommonComponents_RuntimeAll.Count}个作用组件");
			var ds_exit1 =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_AIListen_OnPlayerDistanceExitRange1_AI监听_玩家离开了距离1);
			if (Exit1_DCCConfig.CommonComponents_RuntimeAll != null)
			{
				  //倒序遍历
				for (int i = Exit1_DCCConfig.CommonComponents_RuntimeAll.Count - 1; i >= 0; i--)
				{
					var component = Exit1_DCCConfig.CommonComponents_RuntimeAll[i];
					component.EnterComponent(RelatedAIBrainRuntimeInstance);
				}
				
			}
			_labRef.TriggerActionByType(ds_exit1);
		}


		protected virtual void OnPlayerEnterRange2()
		{	if (_needPlayerCheck &&  RequirePlayerInsideSameArea)
			{
				var check = _areaInfoActivityServiceRef.CheckIfPlayerInsideArea(_selfRelatedSpawnAreaID);
				if (check == null || check.Value == false)
				{
					return;
				}
			}
			DBug.Log(
				$"AI调试：玩家距离监听——【进入】【距离2】，来自AIBrian{RelatedAIBrainRuntimeInstance.name},归属于{RelatedAIBrainRuntimeInstance.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.name}," +
				$"包含{DCCConfig_Enter2.CommonComponents_RuntimeAll.Count}个作用组件");
			var ds_enter2 =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
					.L_AIListen_OnPlayerDistanceEnterRange2_AI监听_玩家进入了距离2);
			if (DCCConfig_Enter2.CommonComponents_RuntimeAll != null)
			{
				for (int i = DCCConfig_Enter2.CommonComponents_RuntimeAll.Count - 1; i >= 0; i--)
				{
					var component = DCCConfig_Enter2.CommonComponents_RuntimeAll[i];
					component.EnterComponent(RelatedAIBrainRuntimeInstance);
				}
			}
			_labRef.TriggerActionByType(ds_enter2);
		}


		protected virtual void OnPlayerExitRange2()
		{	if (_needPlayerCheck &&  RequirePlayerInsideSameArea)
			{
				var check = _areaInfoActivityServiceRef.CheckIfPlayerInsideArea(_selfRelatedSpawnAreaID);
				if (check == null || check.Value == false)
				{
					return;
				}
			}
			DBug.Log(
				$"AI调试：玩家距离监听——【离开】【距离2】，来自AIBrian{RelatedAIBrainRuntimeInstance.name},归属于{RelatedAIBrainRuntimeInstance.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.name}," +
				$"包含{DCCConfig_Exit2.CommonComponents_RuntimeAll.Count}个作用组件");
			var ds_exit2 =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_AIListen_OnPlayerDistanceExitRange2_AI监听_玩家离开了距离2);
			if (DCCConfig_Exit2.CommonComponents_RuntimeAll != null)
			{
				//倒序遍历
				for (int i = DCCConfig_Exit2.CommonComponents_RuntimeAll.Count - 1; i >= 0; i--)
				{
					var component = DCCConfig_Exit2.CommonComponents_RuntimeAll[i];
					component.EnterComponent(RelatedAIBrainRuntimeInstance);
				}
				
			}
			_labRef.TriggerActionByType(ds_exit2);
		}


		protected virtual void OnPlayerEnterRange3()
		{	if (_needPlayerCheck &&  RequirePlayerInsideSameArea)
			{
				var check = _areaInfoActivityServiceRef.CheckIfPlayerInsideArea(_selfRelatedSpawnAreaID);
				if (check == null || check.Value == false)
				{
					return;
				}
			}
			DBug.Log(
				$"AI调试：玩家距离监听——【进入】【距离3】，来自AIBrian{RelatedAIBrainRuntimeInstance.name},归属于{RelatedAIBrainRuntimeInstance.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.name}," +
				$"包含{DCCConfig_Enter3.CommonComponents_RuntimeAll.Count}个作用组件");
			var ds_enter3 =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
					.L_AIListen_OnPlayerDistanceEnterRange3_AI监听_玩家进入了距离3);
			if (DCCConfig_Enter3.CommonComponents_RuntimeAll != null)
			{
				//倒序遍历
				for (int i = DCCConfig_Enter3.CommonComponents_RuntimeAll.Count - 1; i >= 0; i--)
				{
					var component = DCCConfig_Enter3.CommonComponents_RuntimeAll[i];
					component.EnterComponent(RelatedAIBrainRuntimeInstance);
				}
				
			}
			_labRef.TriggerActionByType(ds_enter3);
		}


		protected virtual void OnPlayerExitRange3()
		{	if (_needPlayerCheck &&  RequirePlayerInsideSameArea)
			{
				var check = _areaInfoActivityServiceRef.CheckIfPlayerInsideArea(_selfRelatedSpawnAreaID);
				if (check == null || check.Value == false)
				{
					return;
				}
			}
			DBug.Log(
				$"AI调试：玩家距离监听——【离开】【距离3】，来自AIBrian{RelatedAIBrainRuntimeInstance.name},归属于{RelatedAIBrainRuntimeInstance.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.name}," +
				$"包含{DCCConfig_Exit3.CommonComponents_RuntimeAll.Count}个副作用组件");
			var ds_exit3 =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_AIListen_OnPlayerDistanceExitRange3_AI监听_玩家离开了距离3);
			if (DCCConfig_Exit3.CommonComponents_RuntimeAll != null)
			{
				//倒序遍历 
				for (int i = DCCConfig_Exit3.CommonComponents_RuntimeAll.Count - 1; i >= 0; i--)
				{
					var component = DCCConfig_Exit3.CommonComponents_RuntimeAll[i];
					component.EnterComponent(RelatedAIBrainRuntimeInstance);
				}
			}
			_labRef.TriggerActionByType(ds_exit3);
		}









		protected virtual void OnPlayerEnterRange4()
		{	if (_needPlayerCheck &&  RequirePlayerInsideSameArea)
			{
				var check = _areaInfoActivityServiceRef.CheckIfPlayerInsideArea(_selfRelatedSpawnAreaID);
				if (check == null || check.Value == false)
				{
					return;
				}
			}
			DBug.Log(
				$"AI调试：玩家距离监听——【进入】【距离4】，来自AIBrian{RelatedAIBrainRuntimeInstance.name},归属于{RelatedAIBrainRuntimeInstance.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.name}," +
				$"包含{DCCConfig_Enter4.CommonComponents_RuntimeAll.Count}个副作用组件");
			var ds_enter4 =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
					.L_AIListen_OnPlayerDistanceEnterRange4_AI监听_玩家进入了距离4);
			if (DCCConfig_Enter4.CommonComponents_RuntimeAll != null)
			{

				//倒序遍历 
				for (int i = DCCConfig_Enter4.CommonComponents_RuntimeAll.Count - 1; i >= 0; i--)
				{
					var component = DCCConfig_Enter4.CommonComponents_RuntimeAll[i];
					component.EnterComponent(RelatedAIBrainRuntimeInstance);
				}
				  
			}
			_labRef.TriggerActionByType(ds_enter4);
		}


		protected virtual void OnPlayerExitRange4()
		{	if (_needPlayerCheck &&  RequirePlayerInsideSameArea)
			{
				var check = _areaInfoActivityServiceRef.CheckIfPlayerInsideArea(_selfRelatedSpawnAreaID);
				if (check == null || check.Value == false)
				{
					return;
				}
			}
			DBug.Log(
				$"AI调试：玩家距离监听——【离开】【距离4】，来自AIBrian{RelatedAIBrainRuntimeInstance.name},归属于{RelatedAIBrainRuntimeInstance.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.name}," +
				$"包含{DCCConfig_Exit4.CommonComponents_RuntimeAll.Count}个副作用组件");
			var ds_exit4 =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_AIListen_OnPlayerDistanceExitRange4_AI监听_玩家离开了距离4);
			if (DCCConfig_Exit4.CommonComponents_RuntimeAll != null)
			{
				//倒序遍历 
			 				for (int i = DCCConfig_Exit4.CommonComponents_RuntimeAll.Count - 1; i >= 0; i--)
				{
					var component = DCCConfig_Exit4.CommonComponents_RuntimeAll[i];
					component.EnterComponent(RelatedAIBrainRuntimeInstance);
				}
			}
			_labRef.TriggerActionByType(ds_exit4);
		}







		protected virtual void OnPlayerEnterRange5()
		{	if (_needPlayerCheck &&  RequirePlayerInsideSameArea)
			{
				var check = _areaInfoActivityServiceRef.CheckIfPlayerInsideArea(_selfRelatedSpawnAreaID);
				if (check == null || check.Value == false)
				{
					return;
				}
			}
			DBug.Log(
				$"AI调试：玩家距离监听——【进入】【距离5】，来自AIBrian{RelatedAIBrainRuntimeInstance.name},归属于{RelatedAIBrainRuntimeInstance.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.name}," +
				$"包含{DCCConfig_Enter5.CommonComponents_RuntimeAll.Count}个副作用组件");
			var ds_enter5 =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
					.L_AIListen_OnPlayerDistanceEnterRange5_AI监听_玩家进入了距离5);
			if (DCCConfig_Enter5.CommonComponents_RuntimeAll != null)
			{
				foreach (var component in DCCConfig_Enter5.CommonComponents_RuntimeAll)
				{
					component.EnterComponent(RelatedAIBrainRuntimeInstance);
				}
			}
			_labRef.TriggerActionByType(ds_enter5);
		}


		protected virtual void OnPlayerExitRange5()
		{	if (_needPlayerCheck &&  RequirePlayerInsideSameArea)
			{
				var check = _areaInfoActivityServiceRef.CheckIfPlayerInsideArea(_selfRelatedSpawnAreaID);
				if (check == null || check.Value == false)
				{
					return;
				}
			}
			DBug.Log(
				$"AI调试：玩家距离监听——【离开】【距离5】，来自AIBrian{RelatedAIBrainRuntimeInstance.name},归属于{RelatedAIBrainRuntimeInstance.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.name}," +
				$"包含{DCCConfig_Exit5.CommonComponents_RuntimeAll.Count}个副作用组件");
			var ds_exit5 =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_AIListen_OnPlayerDistanceExitRange5_AI监听_玩家离开了距离5);
			if (DCCConfig_Exit5.CommonComponents_RuntimeAll != null)
			{
				foreach (var component in DCCConfig_Exit5.CommonComponents_RuntimeAll)
				{
					component.EnterComponent(RelatedAIBrainRuntimeInstance);
				}
			}
			_labRef.TriggerActionByType(ds_exit5);
		}







		protected virtual void OnPlayerEnterRange6()
		{	if (_needPlayerCheck &&  RequirePlayerInsideSameArea)
			{
				var check = _areaInfoActivityServiceRef.CheckIfPlayerInsideArea(_selfRelatedSpawnAreaID);
				if (check == null || check.Value == false)
				{
					return;
				}
			}
			DBug.Log(
				$"AI调试：玩家距离监听——【进入】【距离6】，来自AIBrian{RelatedAIBrainRuntimeInstance.name},归属于{RelatedAIBrainRuntimeInstance.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.name}," +
				$"包含{DCCConfig_Enter6.CommonComponents_RuntimeAll.Count}个副作用组件");
			var ds_enter6 =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
					.L_AIListen_OnPlayerDistanceEnterRange6_AI监听_玩家进入了距离6);
			if (DCCConfig_Enter6.CommonComponents_RuntimeAll != null)
			{
				foreach (var component in DCCConfig_Enter6.CommonComponents_RuntimeAll)
				{
					component.EnterComponent(RelatedAIBrainRuntimeInstance);
				}
			}
			_labRef.TriggerActionByType(ds_enter6);
		}


		protected virtual void OnPlayerExitRange6()
		{	if (_needPlayerCheck &&  RequirePlayerInsideSameArea)
			{
				var check = _areaInfoActivityServiceRef.CheckIfPlayerInsideArea(_selfRelatedSpawnAreaID);
				if (check == null || check.Value == false)
				{
					return;
				}
			}
			DBug.Log(
				$"AI调试：玩家距离监听——【离开】【距离6】，来自AIBrian{RelatedAIBrainRuntimeInstance.name},归属于{RelatedAIBrainRuntimeInstance.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.name}," +
				$"包含{DCCConfig_Exit6.CommonComponents_RuntimeAll.Count}个副作用组件");
			var ds_exit6 =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_AIListen_OnPlayerDistanceExitRange6_AI监听_玩家离开了距离6);
			if (DCCConfig_Exit6.CommonComponents_RuntimeAll != null)
			{
				foreach (var component in DCCConfig_Exit6.CommonComponents_RuntimeAll)
				{
					component.EnterComponent(RelatedAIBrainRuntimeInstance);
				}
			}
			_labRef.TriggerActionByType(ds_exit6);
		}

		

		protected virtual void AddNextTime()
		{
			if (TickBySecond)
			{
				NextTickTime = _currentTime + TickIntervalTime;
			}
			else
			{
				NextTickFrame = _currentFrame + TickIntervalFrame;
			}
		}

		/// <summary>
		/// <para>获取  当前AIBrain持有者 对于  当前活跃玩家角色 的距离平方。</para>
		/// <para>如果当前没有活跃玩家角色，则视作距离无穷大</para>
		/// </summary>
		/// <returns></returns>
		protected float GetCurrentDistanceSqr()
		{
			var selfPos = RelatedAIBrainRuntimeInstance.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour
				.transform.position;
			selfPos.y = 0f;

			var currentHatredTarget = RelatedAIBrainRuntimeInstance.BrainHandlerFunction.GetCurrentHatredTarget();
			float disSqr = float.MaxValue;

			//当前没有活跃的玩家，算作玩家脱离了所有范围
			if (!currentHatredTarget)
			{
			}
			else
			{
				Vector3 playerPos = currentHatredTarget.transform.position;
				playerPos.y = 0f;
				disSqr = Vector3.SqrMagnitude(selfPos - playerPos);
			}

			return disSqr;
		}

		public void FixedUpdateTick(float ct, int cf, float delta)
		{
			if (!ListenValid)
			{
				return;
			}


			if ((TickBySecond && ct > NextTickTime) || (!TickBySecond && cf > NextTickFrame))
			{
				ProcessDistanceCheck();



				AddNextTime();
			}
		}



		protected virtual void ProcessDistanceCheck()
		{
			float currentDistanceSqr = GetCurrentDistanceSqr();

			//Range  1：
			//想要触发 “进入范围“，则需 上次距离 > 范围 && 这次距离 < 范围
			if (_lastCheckDistanceSqr > Range1CurrentEnterValue * Range1CurrentEnterValue &&
			    currentDistanceSqr < Range1CurrentEnterValue * Range1CurrentEnterValue)
			{
				OnPlayerEnterRange1();
			}
			//想要触发 “离开范围”，需要 上次距离 < 范围 && 这次距离 > 范围
			else if (_lastCheckDistanceSqr < Range1CurrentEnterValue * Range1CurrentEnterValue &&
			         currentDistanceSqr > Range1CurrentEnterValue * Range1CurrentEnterValue)
			{
				OnPlayerExitRange1();
			}

			if (ContainRange2)
			{
				if (_lastCheckDistanceSqr > Range2CurrentEnterValue * Range2CurrentEnterValue &&
				    currentDistanceSqr < Range2CurrentEnterValue * Range2CurrentEnterValue)
				{
					OnPlayerEnterRange2();
				}
				//想要触发 “离开范围”，需要 上次距离 < 范围 && 这次距离 > 范围
				else if (_lastCheckDistanceSqr < Range2CurrentEnterValue * Range2CurrentEnterValue &&
				         currentDistanceSqr > Range2CurrentEnterValue * Range2CurrentEnterValue)
				{
					OnPlayerExitRange2();
				}
			}


			if (ContainRange3)
			{
				if (_lastCheckDistanceSqr > Range3CurrentEnterValue * Range3CurrentEnterValue &&
				    currentDistanceSqr < Range3CurrentEnterValue * Range3CurrentEnterValue)
				{
					OnPlayerEnterRange3();
				}
				//想要触发 “离开范围”，需要 上次距离 < 范围 && 这次距离 > 范围
				else if (_lastCheckDistanceSqr < Range3CurrentEnterValue * Range3CurrentEnterValue &&
				         currentDistanceSqr > Range3CurrentEnterValue * Range3CurrentEnterValue)
				{
					OnPlayerExitRange3();
				}
			}


			if (ContainRange4)
			{
				if (_lastCheckDistanceSqr > Range4CurrentEnterValue * Range4CurrentEnterValue &&
				    currentDistanceSqr < Range4CurrentEnterValue * Range4CurrentEnterValue)
				{
					OnPlayerEnterRange4();
				}
				//想要触发 “离开范围”，需要 上次距离 < 范围 && 这次距离 > 范围
				else if (_lastCheckDistanceSqr < Range4CurrentEnterValue * Range4CurrentEnterValue &&
				         currentDistanceSqr > Range4CurrentEnterValue * Range4CurrentEnterValue)
				{
					OnPlayerExitRange4();
				}
			}

			if (ContainRange5)
			{
				if (_lastCheckDistanceSqr > Range5CurrentEnterValue * Range5CurrentEnterValue &&
				    currentDistanceSqr < Range5CurrentEnterValue * Range5CurrentEnterValue)
				{
					OnPlayerEnterRange5();
				}
				//想要触发 “离开范围”，需要 上次距离 < 范围 && 这次距离 > 范围
				else if (_lastCheckDistanceSqr < Range5CurrentEnterValue * Range5CurrentEnterValue &&
				         currentDistanceSqr > Range5CurrentEnterValue * Range5CurrentEnterValue)
				{
					OnPlayerExitRange5();
				}
			}

			if (ContainRange6)
			{
				if (_lastCheckDistanceSqr > Range6CurrentEnterValue * Range6CurrentEnterValue &&
				    currentDistanceSqr < Range6CurrentEnterValue * Range6CurrentEnterValue)
				{
					OnPlayerEnterRange6();
				}
				//想要触发 “离开范围”，需要 上次距离 < 范围 && 这次距离 > 范围
				else if (_lastCheckDistanceSqr < Range6CurrentEnterValue * Range6CurrentEnterValue &&
				         currentDistanceSqr > Range6CurrentEnterValue * Range6CurrentEnterValue)
				{
					OnPlayerExitRange6();
				}
			}



			_lastCheckDistanceSqr = currentDistanceSqr;
		}

		public override void UnRegisterListenInActionBus()
		{
			base.UnRegisterListenInActionBus();
			_labRef.RemoveAction(ActionBus_ActionTypeEnum.L_AIListen_OnPlayerDistanceEnterRange1_AI监听_玩家进入了距离1,
				RelatedAIBrainRuntimeInstance.BrainHandlerFunction._ABC_GeneralCallback_RegisteredByListen);
			_labRef.RemoveAction(ActionBus_ActionTypeEnum.L_AIListen_OnPlayerDistanceExitRange1_AI监听_玩家离开了距离1,
				RelatedAIBrainRuntimeInstance.BrainHandlerFunction._ABC_GeneralCallback_RegisteredByListen);
			Enter1_DCCConfig.ClearOnUnload();
			Exit1_DCCConfig.ClearOnUnload();
			if (ContainRange2)
			{
				_labRef.RemoveAction(ActionBus_ActionTypeEnum.L_AIListen_OnPlayerDistanceEnterRange2_AI监听_玩家进入了距离2,
					RelatedAIBrainRuntimeInstance.BrainHandlerFunction._ABC_GeneralCallback_RegisteredByListen);
				_labRef.RemoveAction(ActionBus_ActionTypeEnum.L_AIListen_OnPlayerDistanceExitRange2_AI监听_玩家离开了距离2,
					RelatedAIBrainRuntimeInstance.BrainHandlerFunction._ABC_GeneralCallback_RegisteredByListen);
				DCCConfig_Enter2.ClearOnUnload();
				DCCConfig_Exit2.ClearOnUnload();
			}
			 
			if (ContainRange3)
			{
				_labRef.RemoveAction(ActionBus_ActionTypeEnum.L_AIListen_OnPlayerDistanceEnterRange3_AI监听_玩家进入了距离3,
					RelatedAIBrainRuntimeInstance.BrainHandlerFunction._ABC_GeneralCallback_RegisteredByListen);
				_labRef.RemoveAction(ActionBus_ActionTypeEnum.L_AIListen_OnPlayerDistanceExitRange3_AI监听_玩家离开了距离3,
					RelatedAIBrainRuntimeInstance.BrainHandlerFunction._ABC_GeneralCallback_RegisteredByListen);
				DCCConfig_Enter3.ClearOnUnload();
				DCCConfig_Exit3.ClearOnUnload();
			}
			
			if (ContainRange4)
			{
				_labRef.RemoveAction(ActionBus_ActionTypeEnum.L_AIListen_OnPlayerDistanceEnterRange4_AI监听_玩家进入了距离4,
					RelatedAIBrainRuntimeInstance.BrainHandlerFunction._ABC_GeneralCallback_RegisteredByListen);
				_labRef.RemoveAction(ActionBus_ActionTypeEnum.L_AIListen_OnPlayerDistanceExitRange4_AI监听_玩家离开了距离4,
					RelatedAIBrainRuntimeInstance.BrainHandlerFunction._ABC_GeneralCallback_RegisteredByListen);
				DCCConfig_Enter4.ClearOnUnload();
				DCCConfig_Exit4.ClearOnUnload();
			}
			 
			if (ContainRange5)
			{
				_labRef.RemoveAction(ActionBus_ActionTypeEnum.L_AIListen_OnPlayerDistanceEnterRange5_AI监听_玩家进入了距离5,
					RelatedAIBrainRuntimeInstance.BrainHandlerFunction._ABC_GeneralCallback_RegisteredByListen);
				_labRef.RemoveAction(ActionBus_ActionTypeEnum.L_AIListen_OnPlayerDistanceExitRange5_AI监听_玩家离开了距离5,
					RelatedAIBrainRuntimeInstance.BrainHandlerFunction._ABC_GeneralCallback_RegisteredByListen);
				DCCConfig_Enter5.ClearOnUnload();
				DCCConfig_Exit5.ClearOnUnload();
			}
			 
			if (ContainRange6)
			{
				_labRef.RemoveAction(ActionBus_ActionTypeEnum.L_AIListen_OnPlayerDistanceEnterRange6_AI监听_玩家进入了距离6,
					RelatedAIBrainRuntimeInstance.BrainHandlerFunction._ABC_GeneralCallback_RegisteredByListen);
				_labRef.RemoveAction(ActionBus_ActionTypeEnum.L_AIListen_OnPlayerDistanceExitRange6_AI监听_玩家离开了距离6,
					RelatedAIBrainRuntimeInstance.BrainHandlerFunction._ABC_GeneralCallback_RegisteredByListen);
				DCCConfig_Enter6.ClearOnUnload();
				DCCConfig_Exit6.ClearOnUnload();
			}
			
			 
		}
	}
}