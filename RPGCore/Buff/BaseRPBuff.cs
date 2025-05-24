using System;
using System.Collections.Generic;
using System.Linq;
using ARPG.Character.Base;
using ARPG.Character.Player;
using ARPG.Manager;
using ARPG.UI.Panel;
using GameplayEvent;
using GameplayEvent.SO;
using Global;
using Global.ActionBus;
using RPGCore.AssistBusiness;
using RPGCore.Buff.BuffComponent;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.ConcreteBuff;
using RPGCore.Buff.ConcreteBuff.Common;
using RPGCore.Buff.Config;
using RPGCore.Buff.Requirement;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using RPGCore.UtilityDataStructure.BuffLogicPassingComponent;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace RPGCore.Buff
{
	/// <summary>
	/// <para>这是运行时BuffFunction的基类。</para>
	/// <para>它会作为配置时的 SerializeReference 的基类，用来储存每个Buff不同的功能</para>
	/// <para>运行时也用的是它，由于ScriptableObject的Instantiate，运行时使用的都是新的实例</para>
	/// </summary>
	[Serializable]
	public abstract class BaseRPBuff : I_BuffContentMayDisplayOnUI
	{






#region 配置：逻辑

		/// <summary>
		/// <para>NamedType。</para>
		/// <para>同样的BuffHandler可以被用在不同的BuffType之上</para>
		/// </summary>
		[SerializeField, LabelText("Buff枚举类型")]
		[PropertyOrder(-100)]
		public RolePlay_BuffTypeEnum SelfBuffType;

		[SerializeField,LabelText("Buff内部逻辑标签们")]
		[PropertyOrder(-100)]
		public RP_BuffInternalFunctionFlagTypeEnum InternalFunctionFlagType;

		[SerializeField, LabelText("Buff于配置的初始持续时长")]
		[PropertyOrder(-100)]
		protected float _buffDuration = -1;


		
		
		public float BuffInitDuration => _buffDuration;

		[SerializeField, LabelText("Buff于配置的初始生效时长")]
		[PropertyOrder(-100)]
		protected float _buffAvailableTime = -1;

		public float BuffInitAvailableTime => _buffAvailableTime;
		[SerializeField, LabelText("刷新时对有效&持续时长的操作")]
		[PropertyOrder(-100)]
		protected RP_BuffTimeRefreshEffectTypeEnum _timeRefreshEffect = RP_BuffTimeRefreshEffectTypeEnum.None_不操作;



#endregion

#region 配置：非逻辑

		[FoldoutGroup("表现配置")]
		[SerializeField, LabelText("VFX配置"),
		 ListDrawerSettings(ShowFoldout = true, ShowIndexLabels = true, ListElementLabelName = "_VFX_InfoID")]
		[PropertyOrder(-20)]
		public List<PerVFXInfo> AllVFXInfoList;


		[FoldoutGroup("表现配置")]
		[LabelText("显示于UI的信息")]
		[SerializeField]
		[PropertyOrder(-20)]
		public ConcreteBuffDisplayOnUIInfo DisplayInfo;

#region 时间点的事件

		[LabelText("Buff被加入时触发的事件们")] [SerializeField]
		[PropertyOrder(-10)]
		[FoldoutGroup("事件&预设参数")]
		protected List<SOConfig_PrefabEventConfig> OnBuffAddedEventList;

		[LabelText("Buff被加入时触发的PAC")] [SerializeField]
		[PropertyOrder(-10)]
		[FoldoutGroup("事件&预设参数")]
		protected PACConfigInfo _pacConfig_OnBuffAddAsNew;
		
		[SerializeField]
		[LabelText("Buff被移除时触发的事件们")] [PropertyOrder(-10)]
		[FoldoutGroup("事件&预设参数")]
		protected List<SOConfig_PrefabEventConfig> OnBuffRemovedEventList;

		[LabelText("Buff被移除时触发的PAC")] [SerializeField]
		[PropertyOrder(-10)]
		[FoldoutGroup("事件&预设参数")]
		protected PACConfigInfo _pacConfig_OnBuffPreRemove;

		[LabelText("Buff被刷新时触发的事件们")]
		[SerializeField] [PropertyOrder(-10)]
		[FoldoutGroup("事件&预设参数")]
		protected List<SOConfig_PrefabEventConfig> OnBuffRefreshedEventList;


		[LabelText("Buff刷新时触发的PAC")] [SerializeField]
		[PropertyOrder(-10)]
		[FoldoutGroup("事件&预设参数")]
		protected PACConfigInfo _pacConfig_OnExistBuffRefresh;



#endregion

#endregion



		[SerializeReference, LabelText("预设BLP参数")]
		[FoldoutGroup("事件&预设参数")] [PropertyOrder(-10)]
		public List<BaseBuffLogicPassingComponent> BuffLogicPassingComponentsOnInit;

#region 静态引用

		protected static CharacterOnMapManager _characterOnMapManagerRef;
		protected static DamageAssistService _damageAssistServiceRef;
		public static void StaticInitialize()
		{
			_characterOnMapManagerRef = SubGameplayLogicManager_ARPG.Instance.CharacterOnMapManagerReference;
			_damageAssistServiceRef = SubGameplayLogicManager_ARPG.Instance.DamageAssistServiceInstance;
		}

#endregion

#region 运行时显示

		/// <summary>
		/// <para>就是自己。</para>
		/// <para>对于.asset模板本体来说，那这个就是.asset文件的引用。它是SerializeField的，这是因为对于.asset模板来说它总是需要一开始就有自己的引用</para>
		/// </summary>
		[ShowInInspector, LabelText("配置的运行时实例"),
		 FoldoutGroup("运行时", false, VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()"), SerializeField, PropertyOrder(999)]
		[Header("==========↑↑↑==========")]
		public SOConfig_RPBuff SelfConfigInstance;





		/// <summary>
		/// <para>Buff从属于的RPBehaviour</para>
		/// </summary>
		[ShowInInspector, LabelText("Buff所属的RPBehaviour"),
		 FoldoutGroup("运行时", false, VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()"), PropertyOrder(999)]
		public I_RP_Buff_ObjectCanReceiveBuff Parent_SelfBelongToObject { get; protected set; }

		/// <summary>
		/// 这个buff从哪来的？
		/// </summary>
		[ShowInInspector, LabelText("Buff来源"),
		 FoldoutGroup("运行时", false, VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()"), PropertyOrder(999)]
		public I_RP_Buff_ObjectCanApplyBuff Source_SelfReceiveFromObjectRef { get; private set; }


		/// <summary>
		/// <para>是否为【常驻存在】holder的Buff？</para>
		/// <para>区别于 ResidentAvailable，常驻存在 只是说这个buff不会从BuffHolder中移除，但它可能并不始终有效！</para>
		/// </summary>
		[ShowInInspector, LabelText("是否为【常驻存在】holder的Buff？"),
		 FoldoutGroup("运行时", false, VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()"), PropertyOrder(999)]
		public bool IsPermanentExistBuff { get; private set; }

		/// <summary>
		/// <para>是否为【始终有效】的buff</para>
		/// <para>区别于 PermanentExist，始终有效 只是说这个buff在其存在的期间始终报告TimeIn，
		/// 但它依然有可能被其他buff阻挡而无效 或 因为存在时间耗尽而被移除</para>
		/// </summary>
		[ShowInInspector, LabelText("是否为【始终有效】的buff"),
		 FoldoutGroup("运行时", false, VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()"), PropertyOrder(999)]
		public bool IsResidentAvailableBuff { get; private set; }


		/// <summary>
		/// <para>Buff从被加入到现在经过了多久</para>
		/// </summary>
		[ShowInInspector, LabelText("Buff从被加入到现在经过了多久"),
		 FoldoutGroup("运行时", false, VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()"), PropertyOrder(999)]
		public float BuffElapsedTime { get; private set; }

		/// <summary>
		/// <para>Buff的持续时间：它还会持续存在多久？Permanent的Buff这个始终是-1</para>
		/// </summary>
		[ShowInInspector, LabelText("Buff的持续时间：它还会持续存在多久？"),
		 FoldoutGroup("运行时", false, VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()"), PropertyOrder(999)]
		public float BuffRemainingExistDuration { get; private set; }

		/// <summary>
		/// <para>Buff的剩余有效时间：它还能有效多久？</para>
		/// <para>区分 ExistTime 和 RemainingTime。一个Buff可以仅【存在】但并【不有效】。</para>
		/// </summary>
		[ShowInInspector, LabelText("Buff的剩余有效时间：它还能有效多久？"),
		 FoldoutGroup("运行时", false, VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()"), PropertyOrder(999)]
		public float BuffRemainingAvailableTime { get; private set; }

		/// <summary>
		/// <para>用来记录这是不是buff有效时间刚刚被耗尽的那个flag</para>
		/// </summary>
		protected bool _buffRemainingTimeUseUpRecord = false;

#endregion


#region 基类功能

		public virtual BuffAvailableType GetBuffCurrentAvailableType()
		{
			BuffAvailableType result = BuffAvailableType.Available_TimeInAndMeetRequirement;
			if (MarkAsRemoved)
			{
                // 雷暴等buff创建时就标记为MarkAsRemoved，一帧内有多次该类型伤害就会触发此问题
				// 需要新机制来解决
                DBug.LogWarning($"不应该获取到一个被标记为移除的buff: {this.UI_GetBuffContent_NameText()}");
				return BuffAvailableType.None;
			}


			//如果是始终有效的buff，则判断是否有额外条件需要满足
			if (IsResidentAvailableBuff)
			{
				result = BuffAvailableType.Available_TimeInAndMeetRequirement;
			}
			else
			{
				if (BuffRemainingAvailableTime < 0.01f)
				{
					result = BuffAvailableType.Timeout_AvailableTimeOut;
				}
			}

			return result;
		}


		/// <summary>
		/// <para>如果设置为-1，则会把自己设置为一个始终有效的buff</para>
		/// <para>为0则相当于直接要求无效</para>
		/// <para>默认情况下，如果设置值比当前值小，则不会重置；设置值比当前值大，则重置为较大的</para>
		/// </summary>
		public virtual void ResetAvailableTimeAs(float availableTime)
		{
			var targetAB = Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus();
			if (Mathf.Approximately(availableTime, -1f))
			{
				IsResidentAvailableBuff = true;
				BuffRemainingAvailableTime = -1f;
			}
			else
			{
				IsResidentAvailableBuff = false;
				_buffRemainingTimeUseUpRecord = false;
				if (BuffRemainingAvailableTime < availableTime)
				{
					if (this is I_MayResistByToughness resistByToughness)
					{
						availableTime = resistByToughness.GetFinalValueResistByToughness(availableTime);
					}
					BuffRemainingAvailableTime = availableTime;
				}
			}

			if (targetAB != null)
			{
				DS_ActionBusArguGroup ds =
					new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_Buff_OnChangeBuffAvailableTime);
				ds.IntArgu1 = (int)SelfBuffType;
				ds.FloatArgu1 = availableTime;
				ds.ObjectArgu1 = this;
				targetAB.TriggerActionByType(ActionBus_ActionTypeEnum.L_Buff_OnChangeBuffAvailableTime, ds);
			}
		}

		/// <summary>
		/// <para>重设Buff存在时间。</para>
		/// <para>默认的Buff存在时间都是-1，即加上了就始终存在，有没有效是通过AvailableTime来判定的。</para>
		/// </summary>
		public virtual void ResetExistDurationAs(float existDuration)
		{
			LocalActionBus targetAB = Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus();

			if (Mathf.Approximately(existDuration, -1f))
			{
				IsPermanentExistBuff = true;
				BuffRemainingExistDuration = -1f;
			}
			else
			{
				IsPermanentExistBuff = false;
				if (BuffRemainingExistDuration < existDuration)
				{
					// if (this is I_MayResistByToughness resistByToughness)
					// {
					// 	existDuration = resistByToughness.GetFinalValueResistByToughness(existDuration);
					// }
					BuffRemainingExistDuration = existDuration;
				}
			}

			if (targetAB != null)
			{
				DS_ActionBusArguGroup ds =
					new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_Buff_OnChangeBuffAvailableTime);
				ds.IntArgu1 = (int)SelfBuffType;
				ds.FloatArgu1 = existDuration;
				ds.ObjectArgu1 = this;
				targetAB.TriggerActionByType(ActionBus_ActionTypeEnum.L_Buff_OnChangeBuffExistDuration, ds);
			}
		}


		/// <summary>
		/// <para>快速地 (不小于地)重设持续时长和有效时长</para>
		/// </summary>
		protected void ResetDurationAndAvailableTimeAs(float duration, float availableTime, bool notLess = true)
		{
			if (notLess)
			{
				if (BuffRemainingAvailableTime < availableTime)
				{
					ResetAvailableTimeAs(availableTime);
				}
				if (BuffRemainingExistDuration < duration)
				{
					ResetExistDurationAs(duration);
				}
			}
			else
			{
				ResetExistDurationAs(duration);
				ResetAvailableTimeAs(availableTime);
			}
		}

#endregion


#region 初始化部分

		/// <summary>
		/// <para>基类初始化。包含的内容有：</para>
		/// <para>注入传入的依赖引用、深拷贝参数组。</para>
		/// </summary>
		public virtual void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			SelfConfigInstance = configRuntimeInstance;
			SelfConfigInstance.OriginalBuffConfigTemplate = configRawTemplate;
			Parent_SelfBelongToObject = parent;
			Source_SelfReceiveFromObjectRef = selfReceiveFrom;



			_buffRemainingTimeUseUpRecord = false;

			if (Mathf.Approximately(BuffInitDuration, -1f) || Mathf.Approximately(BuffInitDuration, 0f))
			{
				IsPermanentExistBuff = true;
				BuffRemainingExistDuration = -1f;
			}
			else
			{
				IsPermanentExistBuff = false;
				BuffRemainingExistDuration = BuffInitDuration;
			}

			if (Mathf.Approximately(BuffInitAvailableTime, -1f) || Mathf.Approximately(BuffInitAvailableTime,0f))
			{
				IsResidentAvailableBuff = true;
				BuffRemainingAvailableTime = -1f;
			}
			else
			{
				IsResidentAvailableBuff = false;
				BuffRemainingAvailableTime = BuffInitAvailableTime;
			}

			//处理本地化内容和UI显示内容

			if (BuffLogicPassingComponentsOnInit != null && BuffLogicPassingComponentsOnInit.Count > 0)
			{
				foreach (BaseBuffLogicPassingComponent perBLP in BuffLogicPassingComponentsOnInit)
				{
					ProcessPerBLP(perBLP);
				}
			}

			DisplayInfo._selfBuffNameLS = GameReferenceService_ARPG.Instance.GetLocalizedStringByTableAndKey_ReturnLS(
				LocalizationTableNameC._LTName_BuffName,
				string.IsNullOrEmpty(DisplayInfo.NameKey) ? SelfBuffType.ToString() : DisplayInfo.NameKey);

			DisplayInfo._selfBuffDisplayContentLS =
				GameReferenceService_ARPG.Instance.GetLocalizedStringByTableAndKey_ReturnLS(
					LocalizationTableNameC._LCT_BuffContent,
					string.IsNullOrEmpty(DisplayInfo.NameKey) ? SelfBuffType.ToString() : DisplayInfo.NameKey);

			DisplayInfo._selfBuffDescContentLS =
				GameReferenceService_ARPG.Instance.GetLocalizedStringByTableAndKey_ReturnLS(
					LocalizationTableNameC._LCT_BuffDesc,
					string.IsNullOrEmpty(DisplayInfo.NameKey) ? SelfBuffType.ToString() : DisplayInfo.NameKey);


			switch (DisplayInfo.BuffUIType)
			{
				case BuffUITypeEnum.NeutralBuff_个人中性Buff:
				case BuffUITypeEnum.SelfPositiveBuff_个人正面Buff:
				case BuffUITypeEnum.SelfNegativeBuff_个人负面Buff:
				case BuffUITypeEnum.EnemyShowBuff_敌人需要显示的Buff:
				case BuffUITypeEnum.EnemyOnlyBroadcast_敌人需要广播的Buff:

					DisplayInfo.IconSprite = GlobalConfigurationAssetHolderHelper.GetGCAHH().FE_BuffAndPerkResource
						.GetSpriteByUID(string.IsNullOrEmpty(DisplayInfo.IconKey) ? SelfBuffType.ToString()
							: DisplayInfo.IconKey);
					_Internal_RequireBuffDisplayContent();
					break;
			}
		}

		protected virtual void _Internal_RequireBuffDisplayContent()
		{
			if (MarkAsRemoved)
			{
				return;
			}
			var ds_show = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_UI_RequireBuffDisplayContent_要求Buff显示内容);
			ds_show.ObjectArgu1 = this;
			ds_show.ObjectArgu2 = DisplayInfo;
			GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_show);
		}

		protected virtual void _Internal_RequireRemoveBuffDisplayContent()
		{
			
		}

#endregion


		~BaseRPBuff()
		{
		}


#region Buff具体功能，虚&抽象

		/// <summary>
		/// <para>buff需要有OT组件才会随着时间不断触发效果。</para>
		/// <para>Buff需要有Callback组件才会由于某些事件而不断触发效果</para>
		/// <para>没有 OT组件 或 Callback组件 的Buff只会在OnInitialized的时候触发一次自身的效果</para>
		/// </summary>
		public virtual void FromHolder_FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			BuffElapsedTime += delta;

			//算出啥时候开始闪烁特效

			// var targetBuff = SelfConfigInstance.BuffContentInConfigSO;
			// float targetTime = targetBuff.BuffDuration * targetBuff.BuffUIBlinkThreshold / 100f;
			// if (BuffRemainingExistDuration > 0 && BuffRemainingExistDuration < targetTime &&
			//     targetBuff.ShiningTrigger &&
			//     targetBuff.BuffUIType != RPBuffConfigContentInSO.BuffUITypeEnum.NoUIBuff_不显示到UI中的Buff)
			// {
			// 	targetBuff.ShiningTrigger = false;
			// 	DS_ActionBusArguGroup ds =
			// 		new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_Buff_OnBuffExistShining_Buff即将到期UI闪烁);
			// 	ds.ObjectArgu1 = SelfConfigInstance;
			// 	Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().TriggerActionByType(
			// 		ActionBus_ActionTypeEnum.L_Buff_OnBuffExistShining_Buff即将到期UI闪烁, ds);
			// }

			if (!IsPermanentExistBuff)
			{
				BuffRemainingExistDuration -= delta;

				//时间组件全部无效了
				if (BuffRemainingExistDuration < 0f)
				{
					MarkAsRemoved = true;
					return;
				}
			}

			if (!IsResidentAvailableBuff)
			{
				BuffRemainingAvailableTime -= delta;
				BuffRemainingAvailableTime = Mathf.Clamp(BuffRemainingAvailableTime, 0f, float.MaxValue);
				if (IsPermanentExistBuff)
				{
					if (BuffRemainingAvailableTime < float.Epsilon && !_buffRemainingTimeUseUpRecord)
					{
						_buffRemainingTimeUseUpRecord = true;

						_Internal_OnAvailableTimeUseUp();
					
					}
				}

				if (BuffRemainingAvailableTime < float.Epsilon)
				{
					return;
				}
			}
		}
		
		public void FromHolder_UpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			for (int i = 0; i < AllVFXInfoList.Count; i++)
			{
				AllVFXInfoList[i].UpdateTick(currentTime, currentFrameCount, delta);
			}
		}


		protected virtual void _Internal_OnAvailableTimeUseUp()
		{
			DS_ActionBusArguGroup ds = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_Buff_OnAvailableTimeUseUp);
			ds.IntArgu1 = (int)SelfBuffType;
			ds.ObjectArgu1 = this;
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().TriggerActionByType(
				ActionBus_ActionTypeEnum.L_Buff_OnAvailableTimeUseUp,
				ds);
		}

		/// <summary>
		/// <para>清理操作，基类实现仅Destroy自己Config的运行时实例。基类会在OnPreRemove的时候调用</para>
		/// </summary>
		protected virtual void ClearAndUnload()
		{
			VFX_GeneralClear(true);
			Object.Destroy(SelfConfigInstance);
		}

		protected virtual bool _Internal_BuffAvailableCheck_BlockByOther()
		{
			return false;
		}



#region 回调点

		public virtual void GeneralActionBusCallback(DS_ActionBusArguGroup ds)
		{
		}

		private static List<BaseBuffLogicPassingComponent> blpList = new List<BaseBuffLogicPassingComponent>();
		public virtual DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			if (_pacConfig_OnBuffPreRemove != null)
			{
				_pacConfig_OnBuffPreRemove.BuildRuntimePAEC();
			}
			if (_pacConfig_OnBuffAddAsNew != null)
			{
				_pacConfig_OnBuffAddAsNew.BuildRuntimePAEC();
			}
			if (_pacConfig_OnExistBuffRefresh != null)
			{
				_pacConfig_OnExistBuffRefresh.BuildRuntimePAEC();
			}
			
			
			
			DS_ActionBusArguGroup ds =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_Buff_OnBuffInitialized_一个Buff被确定初始化了);
			ds.IntArgu1 = (int)SelfBuffType;
			ds.ObjectArgu1 = this;
			ds.ObjectArgu2 = blps;
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().TriggerActionByType(ds);
			if (blps != null && blps.Count > 0)
			{
				for (int i = blps.Count - 1; i >= 0; i--)
				{
					var perBLP = blps[i];
					ProcessPerBLP(perBLP);
				}
			}
			if (OnBuffAddedEventList != null && OnBuffAddedEventList.Count > 0)
			{
				foreach (var perEvent in OnBuffAddedEventList)
				{
					GameplayEventManager.Instance.StartGameplayEvent(perEvent);
				}
			}
			if (_pacConfig_OnBuffAddAsNew != null)
			{
				_pacConfig_OnBuffAddAsNew.JsutExecuteAllEffectByBuff(
					Parent_SelfBelongToObject as BaseARPGCharacterBehaviour,
					this);
			}
			return ds;
		}

		public DS_ActionBusArguGroup OnBuffInitialized(BaseBuffLogicPassingComponent[] blps = null)
		{
			blpList.Clear();
			if (blps != null && blps.Length > 0)
			{
				foreach (BaseBuffLogicPassingComponent perBLP in blps)
				{
					blpList.Add(perBLP);
				}
			}
			return OnBuffInitialized(blpList);
		}




		private static List<BaseBuffLogicPassingComponent> _tmpBLPList = new List<BaseBuffLogicPassingComponent>();
		public virtual DS_ActionBusArguGroup OnExistBuffRefreshed(
			I_RP_Buff_ObjectCanApplyBuff caster,
			List<BaseBuffLogicPassingComponent> blps)
		{
			
			DS_ActionBusArguGroup ds =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_Buff_OnBuffRefreshed_已存在Buff被刷新);
			ds.IntArgu1 = (int)SelfBuffType;
			ds.ObjectArgu1 = this;
			ds.ObjectArgu2 = caster;
			ds.ObjectArguStr = blps;
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus()
				.TriggerActionByType(ActionBus_ActionTypeEnum.L_Buff_OnBuffRefreshed_已存在Buff被刷新, ds);
			if (blps != null && blps.Count > 0)
			{
				for (int i = 0; i < blps.Count; i++)
				{
					var perBLP = blps[i];
					ProcessPerBLP(perBLP);
				}
			}
			if (_pacConfig_OnExistBuffRefresh != null)
			{
				_pacConfig_OnExistBuffRefresh.JsutExecuteAllEffectByBuff(
					Parent_SelfBelongToObject as BaseARPGCharacterBehaviour,
					this);
			}

			switch (_timeRefreshEffect)
			{
				case RP_BuffTimeRefreshEffectTypeEnum.None_不操作:
					break;
				case RP_BuffTimeRefreshEffectTypeEnum.ResetAsConfig_重设为配置时长:
					ResetDurationAndAvailableTimeAs(BuffInitDuration, BuffRemainingExistDuration, true);
					break;
				case RP_BuffTimeRefreshEffectTypeEnum.AddByConfig_按配置增加时长:
					ResetDurationAndAvailableTimeAs(BuffRemainingExistDuration + BuffInitDuration,
						BuffRemainingAvailableTime + BuffInitAvailableTime,
						true);
					break;
			}

			if (OnBuffRefreshedEventList != null && OnBuffRefreshedEventList.Count > 0)
			{
				foreach (var perEvent in OnBuffRefreshedEventList)
				{
					GameplayEventManager.Instance.StartGameplayEvent(perEvent);
				}
			}
			return ds;
		}

		public DS_ActionBusArguGroup OnExistBuffRefreshed(I_RP_Buff_ObjectCanApplyBuff caster)
		{
			_tmpBLPList.Clear();
			return OnExistBuffRefreshed(caster, _tmpBLPList);
		}
		public DS_ActionBusArguGroup OnExistBuffRefreshed(
			I_RP_Buff_ObjectCanApplyBuff caster,
			BaseBuffLogicPassingComponent[] blps)
		{
			_tmpBLPList.Clear();
			if (blps != null && blps.Length > 0)
			{
				foreach (var perBLP in blps)
				{
					_tmpBLPList.Add(perBLP);
				}
			}
			return OnExistBuffRefreshed(caster, _tmpBLPList);
		}




		protected virtual void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			switch (blp)
			{
				case BLP_设置持续和有效时间_SetDurationAndTime blp_SetDurationAndTime:
					if (blp_SetDurationAndTime.ContainDurationSet)
					{
						if (blp_SetDurationAndTime.IsDurationReset)
						{
							ResetExistDurationAs(blp_SetDurationAndTime.IsDurationNotLessThan
								? Math.Max(BuffRemainingExistDuration, blp_SetDurationAndTime.DurationResetValue)
								: blp_SetDurationAndTime.DurationResetValue);
							_buffDuration = blp_SetDurationAndTime.DurationResetValue;
						}
						else
						{
							ResetExistDurationAs(blp_SetDurationAndTime.IsDurationModifyByMultiply
								? BuffRemainingExistDuration * blp_SetDurationAndTime.DurationModifyValue
								: BuffRemainingExistDuration + blp_SetDurationAndTime.DurationModifyValue);
						}
					}
					if (blp_SetDurationAndTime.ContainAvailableTimeSet)
					{
						if (blp_SetDurationAndTime.IsAvailableTimeReset)
						{
							ResetAvailableTimeAs(blp_SetDurationAndTime.IsAvailableTimeNotLessThan
								? Math.Max(BuffRemainingAvailableTime, blp_SetDurationAndTime.AvailableTimeResetValue)
								: blp_SetDurationAndTime.AvailableTimeResetValue);
							_buffAvailableTime = blp_SetDurationAndTime.AvailableTimeResetValue;
						}
						else
						{
							ResetAvailableTimeAs(blp_SetDurationAndTime.IsAvailableTimeModifyByMultiply
								? BuffRemainingAvailableTime * blp_SetDurationAndTime.AvailableTimeModifyValue
								: BuffRemainingAvailableTime + blp_SetDurationAndTime.AvailableTimeModifyValue);
						}
					}
					break;
			}
		}

		/// <summary>
		/// <para>Buff在真的从BuffHolder的容器中移除前的那一刻会触发的|</para>
		/// <para>注意，基类里面已经完整触发了事件并Destroy了对应的Object，如果需要更改这个操作，则需要完全重写而不使用base.()</para>
		/// </summary>
		/// <returns></returns>
		public virtual DS_ActionBusArguGroup OnBuffPreRemove()
		{
			if (DisplayInfo != null && DisplayInfo.BuffUIType != BuffUITypeEnum.NoUIBuff_不显示到UI中的Buff)
			{
				var ds_hide =
					new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_UI_BuffDisplayContentClear_清理Buff显示内容);
				ds_hide.ObjectArgu1 = this;
				ds_hide.ObjectArgu2 = DisplayInfo;
				GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_hide);
			}

			if (OnBuffRemovedEventList != null && OnBuffRemovedEventList.Count > 0)
			{
				foreach (var perEvent in OnBuffRemovedEventList)
				{
					GameplayEventManager.Instance.StartGameplayEvent(perEvent);
				}
			}
			if (_pacConfig_OnBuffPreRemove != null)
			{
				_pacConfig_OnBuffPreRemove.JsutExecuteAllEffectByBuff(
					Parent_SelfBelongToObject as BaseARPGCharacterBehaviour,
					this);
			}


			DS_ActionBusArguGroup ds = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_Buff_OnBuffRemoved);
			ds.IntArgu1 = (int)SelfBuffType;
			ds.ObjectArgu1 = this;
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus()
				.TriggerActionByType(ds);
			VFX_GeneralClear();
			ClearAndUnload();
			return ds;
		}


		public virtual DS_ActionBusArguGroup OnOvertimeTickByTickComponent(
			BuffTriggerSource_OvertimeTickComponent source)
		{
			DS_ActionBusArguGroup ds =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_Buff_OnBuffTickOverTime_Buff的OT组件运转一次);
			ds.IntArgu1 = (int)SelfBuffType;
			ds.ObjectArgu1 = this;
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus()
				.TriggerActionByType(ActionBus_ActionTypeEnum.L_Buff_OnBuffTickOverTime_Buff的OT组件运转一次, ds);
			return ds;
		}

#endregion

#endregion

		/// <summary>
		/// <para>标记为移除。常用于自己移除自己的情况，会在下一次FUT的时候进行具体的移除操作，同时已经被标记为移除的Buff会被视作无效。</para>
		/// <para>移除其他buff的时候直接调用Remove相关方法即可</para>
		/// </summary>
		public bool MarkAsRemoved { get; protected set; }

#region 特效

		public PerVFXInfo _VFX_GetAndSetBeforePlay(
			string uid,
			bool needApplyTransformOffset = true,
			bool withDamageTypeVariant = false,
			PerVFXInfo.GetDamageTypeDelegate getDamageType = null,
			string from = null)
		{
			PerVFXInfo selfVFXInfo = PerVFXInfo._VFX_GetByUID(AllVFXInfoList,
				uid,
				withDamageTypeVariant,
				getDamageType,
				from);
			if (selfVFXInfo == null)
			{
				return null;
			}
			return selfVFXInfo._VFX_GetPSHandle( needApplyTransformOffset,
				Parent_SelfBelongToObject.GetRelatedVFXContainer());
		}



		public PerVFXInfo _VFX_JustGet(string uid)
		{
			PerVFXInfo selfVFXInfo = PerVFXInfo._VFX_GetByUID(AllVFXInfoList, uid, false);
			return selfVFXInfo;
		}

		public PerVFXInfo _VFX_GetAndSetBeforePlay(
			PerVFXInfo info,
			bool needApplyTransformOffset = true)
		{
			return info._VFX_GetPSHandle( needApplyTransformOffset, Parent_SelfBelongToObject.GetRelatedVFXContainer());
		}

		public virtual void VFX_GeneralClear(bool immediate = false)
		{
			PerVFXInfo.VFX_GeneralClear(AllVFXInfoList, immediate);
		}

		
		

		/// <summary>
		/// <para>获取当前表现出来的伤害类型</para>
		/// </summary>
		/// <returns></returns>
		protected virtual DamageTypeEnum GetCurrentDamageType()
		{
			if (Parent_SelfBelongToObject.ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum
				.ChangeCommonDamageType_常规伤害类型更改) == BuffAvailableType.NotExist)
			{
				return DamageTypeEnum.NoType_无属性;
			}
			else
			{
				var targetType =
					Parent_SelfBelongToObject.ReceiveBuff_GetTargetBuff(RolePlay_BuffTypeEnum
						.ChangeCommonDamageType_常规伤害类型更改) as Buff_ChangeCommonDamageType;
				return targetType.CurrentDamageType;
			}
		}

#endregion


#region 文本

		// /// <summary>
		// /// <para>将这个Buff的基本效果填入传入的string。这是简短版，常用于perk词条，相当于Buff的名字 ，就好像是   赏金 LV.4 这样</para>
		// /// <para>如果本地化里面有这个Buff类型的名字，</para>
		// /// </summary>
		// public virtual void FillEffectToString_Summary(ref string s, int level)
		// {
		// 	var d = new LocalizedString(LocalizationTableNameC._LCT_RegionMapBaseText,
		// 		SelfConfigInstance.BuffContentInConfigSO.BuffType.ToString());
		// 	if(!d.IsEmpty)
		// 	{
		// 		s += d.GetLocalizedString() + " LV." + level;
		// 	}
		// 	else
		// 	{
		// 		s += SelfConfigInstance.BuffContentInConfigSO.BuffType + " LV." + level;
		// 	}

		// }

#endregion
		public ConcreteBuffDisplayOnUIInfo RelatedBuffDisplayOnUIInfo => DisplayInfo;
		private UIRW_PerPlayerBuffEntry _selfUIRW_BuffEntryRef;
		public UIRW_PerPlayerBuffEntry SelfUIRW_BuffEntryRef
		{
			get => _selfUIRW_BuffEntryRef;
			set => _selfUIRW_BuffEntryRef = value;
		}
		public virtual string UI_GetBuffContent_RemainingTimeText()
		{
			if (Mathf.Approximately(BuffRemainingAvailableTime, -1f))
			{
				return "";
			}
			float rTime = BuffRemainingAvailableTime;
			if (rTime > 60f)
			{
				return $"{(int)(rTime / 60f):00}:{(int)(rTime % 60f):00}";
			}
			else
			{
				return $"0:{rTime:00}";
			}
		}
		public virtual string UI_GetBuffContent_NameText()
		{
			return DisplayInfo._selfBuffDisplayContentLS.GetLocalizedString();
		}
		public virtual int? UI_GetBuffContent_Stack()
		{
			return null;
		}
		public bool RelatedBehaviourDataValid()
		{
			if (Parent_SelfBelongToObject == null)
			{
				return false;
			}
			if (!Parent_SelfBelongToObject.CurrentDataValid())
			{
				return false;
			}
			return true;
		}



		public virtual float GetRemainingPartial()
		{
			return BuffRemainingAvailableTime / BuffInitAvailableTime;
		}
		public bool IfNeedBlink()
		{
			if (Mathf.Approximately(BuffRemainingAvailableTime, -1f))
			{
				return false;
			}
			return BuffRemainingAvailableTime <  6f;
		}




// #if UNITY_EDITOR
//
// 		[Button("进行一个转换")]
// 		public void Button_ConvertAll()
// 		{
// 			//search all SOConfig_RPBuff by assetdatabase
// 			string[] allAssetPath = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_RPBuff");
// 			foreach (string perGUID in allAssetPath)
// 			{
// 				var perPath = UnityEditor.AssetDatabase.GUIDToAssetPath(perGUID);
// 				var perSO = UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_RPBuff>(perPath);
// 				var t = perSO.BuffContentInConfigSO.BuffType;
// 				perSO.ConcreteBuffFunction.SelfBuffType = t;
// 				UnityEditor.EditorUtility.SetDirty(perSO);
//
// 			}
//
// 			UnityEditor.AssetDatabase.SaveAssets();
//
// 		}
// 		[Button("进行一个检查")]
// 		public void Button_CheckAll()
// 		{
// 			//search all SOConfig_RPBuff by assetdatabase
// 			string[] allAssetPath = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_RPBuff");
// 			foreach (string perGUID in allAssetPath)
// 			{
// 				var perPath = UnityEditor.AssetDatabase.GUIDToAssetPath(perGUID);
// 				var perSO = UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_RPBuff>(perPath);
// 				if (perSO.BuffContentInConfigSO.BuffTriggerSources != null &&
// 				    perSO.BuffContentInConfigSO.BuffTriggerSources.Count > 0)
// 				{
// 					Debug.Log($" {perSO.name} 有触发源");
// 				}
//
// 			}
//
// 			UnityEditor.AssetDatabase.SaveAssets();
//
// 		}
// #endif






	}

	/// <summary>
	/// <para>表明当前Buff的有效状态的枚举</para>
	/// </summary>
	public enum BuffAvailableType
	{
		None = 0,

		/// <summary>
		/// <para>时间有效且符合要求</para>
		/// </summary>
		Available_TimeInAndMeetRequirement = 10,

		/// <summary>
		/// 时间有效但不满足额外条件
		/// </summary>
		TimeInButNotMeetOtherRequirement = 11,

		/// <summary>
		/// 超时——超出有效时间了
		/// </summary>
		Timeout_AvailableTimeOut = 21,
		NotExist = 22,
		
	}
}