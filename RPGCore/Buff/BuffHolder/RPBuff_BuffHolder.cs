/*
│	File:RPG_BuffHolder.cs
│	DroyLouo(ccd775@gmail.com)
│	CreateTime:2021-6-17 00:29:47
╰━━━━━━━━━━━━━━━━
*/
//#pragma warning disable CS0162
//#pragma warning disable CS0414

using System;
using System.Collections.Generic;
using ARPG.Character;
using Global;
using Global.ActionBus;
using RPGCore.Buff.BuffComponent;
using RPGCore.Buff.ConcreteBuff;
using RPGCore.Buff.Config;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace RPGCore.Buff.BuffHolder
{
	/// <summary>
	/// <para> RPDM使用的BuffHolder。 </para>
	/// </summary>
	public partial class RPBuff_BuffHolder
	{
		private I_RP_Buff_ObjectCanReceiveBuff _parentRef;

		[ShowInInspector, LabelText("当前运行时Buff容器Dict"), FoldoutGroup("运行时", true),
		 InlineEditor(InlineEditorObjectFieldModes.Boxed)]
		private Dictionary<RolePlay_BuffTypeEnum, SOConfig_RPBuff> _selfActiveBuffCollection;

		public Dictionary<RolePlay_BuffTypeEnum, SOConfig_RPBuff> SelfActiveBuffCollection => _selfActiveBuffCollection;

		private float _currentTime;

		public RPBuff_BuffHolder(I_RP_Buff_ObjectCanReceiveBuff parent)
		{
			_parentRef = parent;
			_selfActiveBuffCollection = new Dictionary<RolePlay_BuffTypeEnum, SOConfig_RPBuff>();
		}

		public void GetCurrentBuffList(List<SOConfig_RPBuff> buffList)
		{
			foreach (var perBuff in _selfActiveBuffCollection.Values)
			{
				buffList.Add(perBuff);
			}
		}


#region 添加
		
		public BaseRPBuff TryApplyBuff_DirectMode(
			RolePlay_BuffTypeEnum type,
			List<BaseBuffLogicPassingComponent> blpc ,
			I_RP_Buff_ObjectCanApplyBuff caster = null)
		{
			//已存在，就刷新一下然后返回
			if (_selfActiveBuffCollection.ContainsKey(type))
			{
				Debug.LogWarning(
					$"{(_parentRef as MonoBehaviour).name}内部初始化过程中出现了重复buff{type}。这可能是不合理的，检查一下初始化buff时所用的模板信息");
				BaseRPBuff existedBuff = GetTargetBuff(type);
				var ds = existedBuff.OnExistBuffRefreshed(caster, blpc);
				_parentRef.ReceiveBuff_GetRelatedActionBus()
					.TriggerActionByType(ActionBus_ActionTypeEnum.L_Buff_OnBuffRefreshed_已存在Buff被刷新, ds);
				return existedBuff;
			}
			//不存在，就new一个，加到容器里，走一个initialize流程，然后返回
			else
			{
				BaseRPBuff newBuffHandler = InstantiateBuffRuntimeInstance(type, caster);
				SOConfig_RPBuff newInstance = newBuffHandler.SelfConfigInstance;
				newInstance.ConcreteBuffFunction.OnBuffInitialized(blpc);

				return newInstance.ConcreteBuffFunction;
			}
		}


		/// <summary>
		/// <para>试图添加一个新Buff。直接添加的模式。通常来源于BuffHolder/DataModel等底层逻辑的内部处理，比如初始化、关联buff预载等。</para>
		/// <para>如果重复的Buff，会报警告并按照Refresh逻辑进行。</para>
		/// <para>如果之前没有的Buff，就是正常的Initialize</para>
		/// </summary>
		public BaseRPBuff TryApplyBuff_DirectMode(
			RolePlay_BuffTypeEnum type,
			I_RP_Buff_ObjectCanApplyBuff caster = null,
			BaseBuffLogicPassingComponent[] blpc = null)
		{
			//已存在，就刷新一下然后返回
			if (_selfActiveBuffCollection.ContainsKey(type))
			{
				Debug.LogWarning(
					$"{(_parentRef as MonoBehaviour).name}内部初始化过程中出现了重复buff{type}。这可能是不合理的，检查一下初始化buff时所用的模板信息");
				BaseRPBuff existedBuff = GetTargetBuff(type);
				var ds = existedBuff.OnExistBuffRefreshed(caster, blpc);
				_parentRef.ReceiveBuff_GetRelatedActionBus()
					.TriggerActionByType(ActionBus_ActionTypeEnum.L_Buff_OnBuffRefreshed_已存在Buff被刷新, ds);
				return existedBuff;
			}
			//不存在，就new一个，加到容器里，走一个initialize流程，然后返回
			else
			{
				BaseRPBuff newBuffHandler = InstantiateBuffRuntimeInstance(type, caster);
				SOConfig_RPBuff newInstance = newBuffHandler.SelfConfigInstance;
				newInstance.ConcreteBuffFunction.OnBuffInitialized(blpc);

				return newInstance.ConcreteBuffFunction;
			}
		}

		/// <summary>
		/// <para>生成Buff的运行时实例（配置和具体业务类）</para>
		/// <para>随后广播 PreApply。因为有些时候试图添加的这个Buff可能会因为其他业务的影响，而并不能实际添加上</para>
		/// <para>这时候需要中间层业务 RP_DS_BuffApplyResult </para>
		/// </summary>
		private BaseRPBuff InstantiateBuffRuntimeInstance(
			RolePlay_BuffTypeEnum type,
			I_RP_Buff_ObjectCanApplyBuff caster)
		{
			//找到模板
			SOConfig_RPBuff targetBuffSOConfigRaw =
				GlobalConfigurationAssetHolderHelper.Instance.Collection_RPBuff.GetRPBuffByTypeAndLevel(type);
			if (caster == null)
			{
				caster = _parentRef as I_RP_Buff_ObjectCanApplyBuff;
			}

			//生成一个运行时实例
			SOConfig_RPBuff newInstance = Object.Instantiate(targetBuffSOConfigRaw);
			BaseRPBuff newBuffBaseRef = newInstance.ConcreteBuffFunction;
			newInstance.ConcreteBuffFunction.Init(this, newInstance, targetBuffSOConfigRaw, _parentRef, caster);
			_selfActiveBuffCollection.Add(type, newInstance);


			return newBuffBaseRef;
		}


		public (BuffApplyResultEnum, BaseRPBuff) TryApplyBuff(
			RolePlay_BuffTypeEnum buffType,
			I_RP_Buff_ObjectCanApplyBuff caster,
			I_RP_Buff_ObjectCanReceiveBuff receiver,
			List<BaseBuffLogicPassingComponent> logicPassingComponents)
		{
			var check = CheckTargetBuff(buffType);
			//如果已经存在，那就刷新一下
			if (check != BuffAvailableType.NotExist)
			{
				BaseRPBuff targetExistBuff = GetTargetBuff(buffType);

				var ds = targetExistBuff.OnExistBuffRefreshed(caster, logicPassingComponents);
				return (BuffApplyResultEnum.AlreadyExistsAndRefresh, targetExistBuff);
			}
			else
			{
				bool forceApply = false;

				if (logicPassingComponents != null && logicPassingComponents.Count > 0)
				{
					//检查是否需要强制添加
					foreach (var buffLogicPassingComponent in logicPassingComponents)
					{
						if (buffLogicPassingComponent is BuffLogicPassing_ForceApply forceApplyComponent)
						{
							forceApply = forceApplyComponent.ForceApply;
							break;
						}
					}
				}

				//如果有强制添加，则直接走DirectMode，不会检查，然后return。
				//23.10.13修改：ApplyResult为Block现在只会由 _Internal_IfBuffCanApplyCheck_WhenTryToApplyOtherBuff 的调用汇报出来，DirectMode之后真的就是Direct了，只管添加不管是否能添加
				if (!forceApply)
				{
					//从对象池中拿一个 BuffApplyResult
					RP_DS_BuffApplyResult baResult = GenericPool<RP_DS_BuffApplyResult>.Get();
					baResult.Reset();
					baResult.Caster = caster;
					baResult.Receiver = receiver;

					DS_ActionBusArguGroup ds =
						new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_Buff_OnBuffPreAdd_一个Buff将要被添加);
					ds.IntArgu1 = (int)buffType;
					ds.ObjectArgu1 = logicPassingComponents;
					ds.ObjectArgu2 = baResult;
					_parentRef.ReceiveBuff_GetRelatedActionBus()
						.TriggerActionByType(ActionBus_ActionTypeEnum.L_Buff_OnBuffPreAdd_一个Buff将要被添加, ds);

					if (baResult.BlockByOtherBuff)
					{
						var applyFailed =
							new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
								.L_Buff_OnBlockedAtPreAdd_在PreAdd期被阻挡而没有添加成功);
						applyFailed.IntArgu1 = (int)buffType;
						DBug.Log($"Buff添加被阻挡，类型为{buffType}");
						receiver.ReceiveBuff_GetRelatedActionBus()?.TriggerActionByType(applyFailed);
						GenericPool<RP_DS_BuffApplyResult>.Release(baResult);
						return (BuffApplyResultEnum.BlockedSoFailed, null);
					}
					GenericPool<RP_DS_BuffApplyResult>.Release(baResult);

				}

				var applyResult = TryApplyBuff_DirectMode(buffType, logicPassingComponents, caster);



				return (BuffApplyResultEnum.Success, applyResult);
			}
		}

		private static List<BaseBuffLogicPassingComponent> tmpList = new List<BaseBuffLogicPassingComponent>();
		
		
		public (BuffApplyResultEnum, BaseRPBuff) TryApplyBuff(
			RolePlay_BuffTypeEnum buffType,
			I_RP_Buff_ObjectCanApplyBuff caster,
			I_RP_Buff_ObjectCanReceiveBuff receiver,
			params BaseBuffLogicPassingComponent[] logicPassingComponents)
		{
			tmpList.Clear();
			if (logicPassingComponents != null && logicPassingComponents.Length > 0)
			{
				foreach (BaseBuffLogicPassingComponent perBLP in logicPassingComponents)
				{
					tmpList.Add(perBLP);
				}
			}
			return TryApplyBuff(buffType, caster, receiver, tmpList);
		}

		/// <summary>
		/// <para>试图添加一个新buff。来自外部的调用。外部：其他Behaviour/其他Dispatcher/世界统一/无源的</para>
		/// <para>区别于来自内部的添加</para>
		/// <para>这个有可能失败</para>
		/// <para>流程为：如果已经存在，则刷新并返回；不存在，则如果是强行添加，则等效DirectMode。不是，则检查是否能够添加，能则等效DirectMode，不能则返回失败。</para>
		/// </summary>
		public (BuffApplyResultEnum, BaseRPBuff) TryApplyBuff(
			ConSer_BuffApplyInfo buffApplyInfo,
			I_RP_Buff_ObjectCanApplyBuff caster,
			I_RP_Buff_ObjectCanReceiveBuff receiver)
		{
			return TryApplyBuff(buffApplyInfo.BuffType,
				caster,
				receiver,
				buffApplyInfo.GetFullBLPList());
		}

		/// <summary>
		/// <para>根据配置时的信息来初始化buff。如果要求覆写CalculationGroup，则进行覆写</para>
		/// </summary>
		/// <param name="configGroup"></param>
		public void AddBuffFromInitializeConfig(SOConfig_BuffConfigGroup configGroup)
		{
			AddBuffFromInitializeConfig(configGroup.InitConfigList);
		}

		public void AddBuffFromInitializeConfig(List<Buff_InitConfigEntry> configList)
		{
			foreach (Buff_InitConfigEntry initializeConfig in configList)
			{
				BaseRPBuff t = TryApplyBuff_DirectMode(initializeConfig.Type);
			}
		}


		/// <summary>
		/// <para>试图获得目标Buff。不会检查等级。检查等级的事情应当是CheckTargetBuff进行的</para>
		/// </summary>
		public BaseRPBuff GetTargetBuff(RolePlay_BuffTypeEnum type, bool createWhenNotExist = false)
		{
			if (_selfActiveBuffCollection.ContainsKey(type))
			{
				return _selfActiveBuffCollection[type].ConcreteBuffFunction;
			}
			else
			{
				if (createWhenNotExist)
				{
					DBug.LogWarning($"{(_parentRef as MonoBehaviour).name}在获取Buff{type}时，并没有获取到，则此时创建了一个");
					return TryApplyBuff_DirectMode(type);
				}
				else
				{
					DBug.LogError($"{(_parentRef as MonoBehaviour).name}在获取Buff{type}时，并没有获取到，而且没有要求创建新的");
					return null;
				}
			}
		}

#endregion

#region 删除


		/// <summary>
		/// <para>试图移除一个Buff。</para>
		/// <para>如果没有传入operationLevel，则会无视等级试图移除（仍然可能会被其他Buff阻挡）</para>
		/// <para>如果传入了operationLevel，则不会移除比传入值更高的等级的buff</para>
		/// <para>如果是forceMode，则会强制移除</para>
		/// <para>！会将目标从容器中移除！不要在遍历容器的时候用</para>
		/// </summary>
		public BuffRemoveResultEnum TryRemoveBuff(RolePlay_BuffTypeEnum type, bool asRemove = false)
		{
			if (!_selfActiveBuffCollection.ContainsKey(type))
			{
				return BuffRemoveResultEnum.NotExist;
			}

			if (!asRemove)
			{
				DBug.LogFormat($"正在试图移除buff，其目标类型为{_selfActiveBuffCollection[type].ConcreteBuffFunction.SelfBuffType}");
			}
			SOConfig_RPBuff buffConfig = _selfActiveBuffCollection[type];

			BaseRPBuff targetBuff = buffConfig.ConcreteBuffFunction;
			ClearCertainBuffContent(targetBuff);
			targetBuff.OnBuffPreRemove();
			_selfActiveBuffCollection.Remove(type);
			Object.Destroy(buffConfig);
			return BuffRemoveResultEnum.RemoveSuccess;
		}

		/// <summary>
		/// 清除某个Buff的运行时内容，比如各种监听，和在BuffHolder中的注册。但还没有从总容器中移除
		/// </summary>
		private void ClearCertainBuffContent(BaseRPBuff targetBuff)
		{
			//移除可能存在的OT注册
			for (int i = _Internal_OverTimeRegisterList.Count - 1; i >= 0; i--)
			{
				InternalOverTimeRegisterInfo tmp = _Internal_OverTimeRegisterList[i];
				if (tmp.RelatedBuff == targetBuff)
				{
					_Internal_OverTimeRegisterList.RemoveAt(i);
				}
			}

			//移除可能存在的ABC注册
			for (int i = _Internal_CallbackRegisterList.Count - 1; i >= 0; i--)
			{
				InternalCallbackRegisterInfo tmp = _Internal_CallbackRegisterList[i];
				if (tmp.RelatedBuff == targetBuff)
				{
					if (tmp.IsGlobal)
					{
						GlobalActionBus.GetGlobalActionBus()
							.RemoveAction(tmp.Type, targetBuff.GeneralActionBusCallback);
					}
					else
					{
						_parentRef.ReceiveBuff_GetRelatedActionBus().RemoveAction(tmp.Type,
							targetBuff.GeneralActionBusCallback);
					}

					_Internal_CallbackRegisterList.RemoveAt(i);
				}
			}
		}


		public void ClearBuffHolder()
		{
			foreach (SOConfig_RPBuff perBuff in _selfActiveBuffCollection.Values)
			{
				ClearCertainBuffContent(perBuff.ConcreteBuffFunction);
				perBuff.ConcreteBuffFunction.OnBuffPreRemove();
			}


			_selfActiveBuffCollection.Clear();
		}

#endregion

#region 查找

		/// <summary>
		/// <para>检查目标buff。</para>
		/// </summary>
		public BuffAvailableType CheckTargetBuff(RolePlay_BuffTypeEnum type)
		{
			if (!_selfActiveBuffCollection.ContainsKey(type))
			{
				return BuffAvailableType.NotExist;
			}

			return _selfActiveBuffCollection[type].ConcreteBuffFunction.GetBuffCurrentAvailableType();
		}

		/// <summary>
		/// 检查该BuffHolder是否包含任意一个buff，它拥有所传入的FunctionFlag中的任意一个的标签。
		/// <para>常用于：检查有没有能造成 “禁用普通移动”效果的buff等等</para>
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public bool ReceiveBuff_CheckExistValidBuffWithTag(RP_BuffInternalFunctionFlagTypeEnum type)
		{
			switch (type)
			{
				case { } when type.HasFlag(RP_BuffInternalFunctionFlagTypeEnum.DisableCommonMovement_禁用普通移动):
					foreach (var perBuff in _selfActiveBuffCollection.Values)	
					{
						if (perBuff.ConcreteBuffFunction.GetBuffCurrentAvailableType() ==
						    BuffAvailableType.Available_TimeInAndMeetRequirement &&
						    perBuff.ConcreteBuffFunction.InternalFunctionFlagType.HasFlag(
							    RP_BuffInternalFunctionFlagTypeEnum.DisableCommonMovement_禁用普通移动))
						{
							return true;
						}
					}
					break;
				case { } when type.HasFlag(RP_BuffInternalFunctionFlagTypeEnum.BlockByStrongStoic_被强霸体屏蔽):
					foreach (var perBuff in _selfActiveBuffCollection.Values)	
					{
						if (perBuff.ConcreteBuffFunction.GetBuffCurrentAvailableType() ==
						    BuffAvailableType.Available_TimeInAndMeetRequirement &&
						    perBuff.ConcreteBuffFunction.InternalFunctionFlagType.HasFlag(
							    RP_BuffInternalFunctionFlagTypeEnum.BlockByStrongStoic_被强霸体屏蔽))
						{
							return true;
						}
					}
					break;
				case { } when type.HasFlag(RP_BuffInternalFunctionFlagTypeEnum.BlockByWeakStoic_被弱霸体屏蔽):
					foreach (var perBuff in _selfActiveBuffCollection.Values)	
					{
						if (perBuff.ConcreteBuffFunction.GetBuffCurrentAvailableType() ==
						    BuffAvailableType.Available_TimeInAndMeetRequirement &&
						    perBuff.ConcreteBuffFunction.InternalFunctionFlagType.HasFlag(
							    RP_BuffInternalFunctionFlagTypeEnum.BlockByWeakStoic_被弱霸体屏蔽))
						{
							return true;
						}
					}
					break;
				case { } when type.HasFlag(RP_BuffInternalFunctionFlagTypeEnum.ResistByWeakStoic_被弱霸体抵抗):
					foreach (var perBuff in _selfActiveBuffCollection.Values)	
					{
						if (perBuff.ConcreteBuffFunction.GetBuffCurrentAvailableType() ==
						    BuffAvailableType.Available_TimeInAndMeetRequirement &&
						    perBuff.ConcreteBuffFunction.InternalFunctionFlagType.HasFlag(
							    RP_BuffInternalFunctionFlagTypeEnum.ResistByWeakStoic_被弱霸体抵抗))
						{
							return true;
						}
					}
					break;
					 
			}
			return false;
		}

#endregion

#region 转移

		public void ProcessBuffTransfer(PlayerARPGConcreteCharacterBehaviour transferTo)
		{
			Dictionary<RolePlay_BuffTypeEnum, I_BuffTransferWithinPlayer> needToTransferDict =
				CollectionPool<Dictionary<RolePlay_BuffTypeEnum, I_BuffTransferWithinPlayer>,
					KeyValuePair<RolePlay_BuffTypeEnum, I_BuffTransferWithinPlayer>>.Get();
			needToTransferDict.Clear();
			foreach (SOConfig_RPBuff perBuff in _selfActiveBuffCollection.Values)
			{
				if (perBuff.ConcreteBuffFunction is I_BuffTransferWithinPlayer transfer)
				{
					needToTransferDict.Add(perBuff.ConcreteBuffFunction.SelfBuffType, transfer);
				}
			}
			foreach (KeyValuePair<RolePlay_BuffTypeEnum,I_BuffTransferWithinPlayer> perKVP in needToTransferDict)
			{
				_selfActiveBuffCollection.Remove(perKVP.Key);
				transferTo.ReceiveBuffTransfer( perKVP.Value);
			}
		}

		public void ReceiveTransferFromOtherPlayer(I_BuffTransferWithinPlayer transferFrom, PlayerARPGConcreteCharacterBehaviour newPlayer)
		{
			var baseRPBuff = transferFrom as BaseRPBuff;
			_selfActiveBuffCollection.Add(baseRPBuff.SelfBuffType, baseRPBuff.SelfConfigInstance);
			transferFrom.ProcessTransfer(transferFrom, newPlayer);
		}

#endregion

		private List<SOConfig_RPBuff> _tickList = new List<SOConfig_RPBuff>();

		public void FixedUpdateTick(float currentTime, int currentFrameCount, float deltaTime)
		{
			if (!_parentRef.CurrentDataValid())
			{
				return;
			}
			_currentTime = currentTime;
	
			for (int i = _Internal_OverTimeRegisterList.Count - 1; i >= 0; i--)
			{
				InternalOverTimeRegisterInfo perOT = _Internal_OverTimeRegisterList[i];
				perOT.OTSource.FixedUpdateTickByBuffHolder(currentTime, deltaTime);
			}

			_tickList.Clear();
			foreach (var buff in _selfActiveBuffCollection.Values)
			{
				_tickList.Add(buff);
			}
			for (int i = _tickList.Count - 1; i >= 0; i--)
			{
				_tickList[i].ConcreteBuffFunction.FromHolder_FixedUpdateTick(currentTime, currentFrameCount, deltaTime);
				if (_tickList[i].ConcreteBuffFunction.MarkAsRemoved)
				{
					TryRemoveBuff(_tickList[i].ConcreteBuffFunction.SelfBuffType);
					_tickList.RemoveAt(i);
				}
			}
		}

		
		public void UpdateTick( float currentTime, int currentFrameCount, float deltaTime)
		{
			if (!_parentRef.CurrentDataValid())
			{
				return;
			}
			foreach (SOConfig_RPBuff perBuff in _selfActiveBuffCollection.Values)
			{
				perBuff.ConcreteBuffFunction.FromHolder_UpdateTick(currentTime, currentFrameCount, deltaTime);
			}
		}
		

		[ShowInInspector, LabelText("将会经由ABC触发的Buff功能组件列表")]
		private List<InternalCallbackRegisterInfo> _Internal_CallbackRegisterList =
			new List<InternalCallbackRegisterInfo>();

		[ShowInInspector, LabelText("将会经由OT触发的Buff功能组件列表")]
		private List<InternalOverTimeRegisterInfo> _Internal_OverTimeRegisterList =
			new List<InternalOverTimeRegisterInfo>();


		private class InternalCallbackRegisterInfo
		{
			[LabelText("Buff类型")] public ActionBus_ActionTypeEnum Type;
			[LabelText("全局的吗")] public bool IsGlobal;
			[LabelText("关联Buff")] public BaseRPBuff RelatedBuff;
			[LabelText("那个ABC组件")] public BuffTriggerSource_ActionBusCallbackComponent ABCSource;
			[LabelText("优先级")] public int Order;
		}

		private class InternalOverTimeRegisterInfo
		{
			[LabelText("关联Buff")] public BaseRPBuff RelatedBuff;
			[LabelText("那个OT组件")] public BuffTriggerSource_OvertimeTickComponent OTSource;
			public float NextTickValidTime;
		}



	}


	public enum BuffApplyResultEnum
	{
		None = 0,

		/// <summary>
		/// 添加成功
		/// </summary>
		Success = 11,

		/// <summary>
		/// <para>已经存在，并且刷新了现有的Buff</para>
		/// </summary>
		AlreadyExistsAndRefresh = 12,

		/// <summary>
		/// 被阻挡了，没有添加成功
		/// </summary>
		BlockedSoFailed = 21,
	}

	public enum BuffRemoveResultEnum
	{
		None = 0,

		/// <summary>
		/// <para>并不存在目标buff</para>
		/// </summary>
		NotExist = 1,

		/// <summary>
		/// <para>移除成功</para>
		/// </summary>
		RemoveSuccess = 11,

		/// <summary>
		/// <para>移除失败，由于等级原因</para>
		/// </summary>
		FailByLevel = 21,

		/// <summary>
		/// <para>移除失败，由于其他buff的阻挡</para>
		/// </summary>
		FailByOtherBuff = 22,
	}
}