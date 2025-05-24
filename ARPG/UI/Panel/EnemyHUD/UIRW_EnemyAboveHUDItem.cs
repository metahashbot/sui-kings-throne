using System;
using System.Collections.Generic;
using ARPG.BattleActivity.Config.EnemySpawnAddon;
using ARPG.Character.Enemy;
using ARPG.Manager;
using DG.Tweening;
using Global;
using Global.ActionBus;
using Global.UIBase;
using RPGCore.Buff;
using RPGCore.Buff.ConcreteBuff;
using RPGCore.Buff.ConcreteBuff.Common;
using RPGCore.Buff.ConcreteBuff.Element.First;
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ARPG.UI.Panel
{
	/// <summary>
	/// <para>显示到每个敌人(或友军)头上的HUD项目。实际上它并不是UI(没有Canvas)，只是管理在UIP之下，所以就这么归类了</para>
	/// <para>包含的功能有：血条、名字、弱点条、(问UIP_HUDPanel要的)BuffIcon</para>
	/// <para>在生成及加载顺序来说，生成并设置AboveHUD一定是【在应用ESA 之前】的</para>
	/// </summary>
	public class UIRW_EnemyAboveHUDItem : UI_UISingleRuntimeWidget
	{

		[Flags]
		public enum EnemyAboveHUDItemFunctionFlagEnum
		{
			IncludeHPSlider_包含HP滑条 = 1,
			IncludeNameTextLabel_包含名字标签 = 1 << 2,
			IncludeWeaknessSlider_包含弱点滑条 = 1 << 3,
			IncludeBuffGroupLayout_包含Buff组布局 = 1 << 4,
			IncludeFullHolderFloating_包含整体的上下浮动效果 = 1 << 5,
			HideHPSliderOnNoHurt_一段时间没有HP变动则隐藏HP = 1 << 6,
		}

		public static readonly EnemyAboveHUDItemFunctionFlagEnum DefaultAboveHUDConfig_NormalEnemy =
			EnemyAboveHUDItemFunctionFlagEnum.IncludeHPSlider_包含HP滑条 |
			EnemyAboveHUDItemFunctionFlagEnum.IncludeBuffGroupLayout_包含Buff组布局 |
			EnemyAboveHUDItemFunctionFlagEnum.HideHPSliderOnNoHurt_一段时间没有HP变动则隐藏HP;

		public static readonly EnemyAboveHUDItemFunctionFlagEnum DefaultAboveHUDConfig_EliteEnemy =
			EnemyAboveHUDItemFunctionFlagEnum.IncludeHPSlider_包含HP滑条 |
			EnemyAboveHUDItemFunctionFlagEnum.IncludeNameTextLabel_包含名字标签 |
			EnemyAboveHUDItemFunctionFlagEnum.IncludeBuffGroupLayout_包含Buff组布局 |
			EnemyAboveHUDItemFunctionFlagEnum.IncludeWeaknessSlider_包含弱点滑条;
		
		public static readonly EnemyAboveHUDItemFunctionFlagEnum DefaultAboveHUDConfig_InClipTypeList =
			EnemyAboveHUDItemFunctionFlagEnum.IncludeNameTextLabel_包含名字标签 |
			EnemyAboveHUDItemFunctionFlagEnum.IncludeFullHolderFloating_包含整体的上下浮动效果;

#region 设置

		protected const float _自身上下浮动时posY移动高度 = 0.2f;
		protected const float _自身上下浮动时单次时长 = 1.5f;

		protected static float _RedSliderTweenDuration = 0.13f;
		protected static float _WhiteSliderTweenDuration = 0.1f;
		protected static float _WhiteSliderTweenDelay = 0.35f;

#endregion


#region 本体

		[ShowInInspector, LabelText("当前的HUD功能状态")]
		public EnemyAboveHUDItemFunctionFlagEnum SelfFunctionFlag { get; private set; }


		private void InitMultipleFunctionFlags(EnemyAboveHUDItemFunctionFlagEnum flags)
		{
			SelfFunctionFlag = flags;
			//traversal flags
			if (flags.HasFlag(EnemyAboveHUDItemFunctionFlagEnum.IncludeHPSlider_包含HP滑条))
			{
				AddSingleFunctionFlag(EnemyAboveHUDItemFunctionFlagEnum.IncludeHPSlider_包含HP滑条);
			}

			if (flags.HasFlag(EnemyAboveHUDItemFunctionFlagEnum.IncludeNameTextLabel_包含名字标签))
			{ 
				AddSingleFunctionFlag(EnemyAboveHUDItemFunctionFlagEnum.IncludeNameTextLabel_包含名字标签);
			}

			if (flags.HasFlag(EnemyAboveHUDItemFunctionFlagEnum.IncludeWeaknessSlider_包含弱点滑条))
			{
				AddSingleFunctionFlag(EnemyAboveHUDItemFunctionFlagEnum.IncludeWeaknessSlider_包含弱点滑条);
			}

			if (flags.HasFlag(EnemyAboveHUDItemFunctionFlagEnum.IncludeBuffGroupLayout_包含Buff组布局))
			{ 
				AddSingleFunctionFlag(EnemyAboveHUDItemFunctionFlagEnum.IncludeBuffGroupLayout_包含Buff组布局);
			}

			if (flags.HasFlag(EnemyAboveHUDItemFunctionFlagEnum.IncludeFullHolderFloating_包含整体的上下浮动效果))
			{ 
				AddSingleFunctionFlag(EnemyAboveHUDItemFunctionFlagEnum.IncludeFullHolderFloating_包含整体的上下浮动效果);
			}
			
		}

		private void AddSingleFunctionFlag(EnemyAboveHUDItemFunctionFlagEnum singleFlag)
		{
			SelfFunctionFlag |= singleFlag;
			//traversal singleFlag
			 
			 
			 
			switch (singleFlag)
			{
				case { } when singleFlag.HasFlag(EnemyAboveHUDItemFunctionFlagEnum.IncludeHPSlider_包含HP滑条):
					_InternalInit_EnableHPSliderFunction();
					break;
				case { } when singleFlag.HasFlag(EnemyAboveHUDItemFunctionFlagEnum.IncludeNameTextLabel_包含名字标签):
					_InternalInit_EnableNameTextLabelFunction();
					break;
				case {} when singleFlag.HasFlag(EnemyAboveHUDItemFunctionFlagEnum.IncludeWeaknessSlider_包含弱点滑条):
					_InternalInit_EnableWeaknessSlider();
					break;
				case {} when singleFlag.HasFlag(EnemyAboveHUDItemFunctionFlagEnum.IncludeBuffGroupLayout_包含Buff组布局):
					_InternalInit_EnableBuffGroupFunction();
					break;
				case {} when singleFlag.HasFlag(EnemyAboveHUDItemFunctionFlagEnum.IncludeFullHolderFloating_包含整体的上下浮动效果):
					_tween_selfPositionYoyo = rect_SuspensionAnchor.DOAnchorPosY(_自身上下浮动时posY移动高度, _自身上下浮动时单次时长)
						.SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
					break;
				case {}  when singleFlag.HasFlag(EnemyAboveHUDItemFunctionFlagEnum.HideHPSliderOnNoHurt_一段时间没有HP变动则隐藏HP):
					break;
			}
		}

		private void RemoveSingleFunctionFlag(EnemyAboveHUDItemFunctionFlagEnum single)
		{
			SelfFunctionFlag &= ~single;

			switch (single)
			{
				case EnemyAboveHUDItemFunctionFlagEnum.IncludeHPSlider_包含HP滑条:
					_InternalInit_DisableHPSliderFunction();
					break;
				case EnemyAboveHUDItemFunctionFlagEnum.IncludeNameTextLabel_包含名字标签:
					_InternalInit_DisableNameTextLabelFunction();
					break;
				case EnemyAboveHUDItemFunctionFlagEnum.IncludeWeaknessSlider_包含弱点滑条:
					_InternalInit_DisableWeaknessSlider();
					break;
				case EnemyAboveHUDItemFunctionFlagEnum.IncludeBuffGroupLayout_包含Buff组布局:
					_InternalInit_DisableBuffGroupFunction();
					break;
				case EnemyAboveHUDItemFunctionFlagEnum.IncludeFullHolderFloating_包含整体的上下浮动效果:
					_tween_selfPositionYoyo?.Kill();
					break;
				case EnemyAboveHUDItemFunctionFlagEnum.HideHPSliderOnNoHurt_一段时间没有HP变动则隐藏HP:
					break;
			}
		}

		[LabelText("rect_自身RectTransform"), SerializeField, Required, TitleGroup("本体")]
		public RectTransform _rect_SelfRectTransform;
		[LabelText("rect_悬浮锚点定位"), SerializeField, Required, TitleGroup("本体")]
		protected RectTransform rect_SuspensionAnchor;


		private UIP_EnemyHUDPanel _parentPanelRef;

		public EnemyARPGCharacterBehaviour RelatedEnemyBehaviourRef { get; private set; }

		private Float_RPDataEntry _maxHPEntry;
		private static GameReferenceService_ARPG _grsRef;
		private static CharacterOnMapManager _comRef;



		/// <summary>
		/// 自己上下浮动的那个动画
		/// </summary>
		private Tweener _tween_selfPositionYoyo;



		/// <summary>
		/// <para>生成初始化的时候，为其注入依赖等。</para>
		/// </summary>
		public void InstantiateInitialize(UIP_EnemyHUDPanel panelRef)
		{
			_parentPanelRef = panelRef;
			_grsRef = GameReferenceService_ARPG.Instance;
			_comRef = SubGameplayLogicManager_ARPG.Instance.CharacterOnMapManagerReference;
			ResetToDefault();
		}


		private void ResetToDefault()
		{
			_rect_SelfRectTransform.localPosition = Vector3.zero;
			slider_WeaknessSlider.gameObject.SetActive(false);
			_rt_HealthSliderHolder.gameObject.SetActive((false));
			text_EnemyNameText.gameObject.SetActive(false);
			layout_BuffLayoutGroup.gameObject.SetActive(false);
			_tween_selfPositionYoyo?.Kill();
			_tween_RedSliderChangeValue?.Kill();
			_tween_WhiteSliderChangeValue?.Kill();
		}
		
		
		
		
		/// <summary>
		/// <para>注入enemy引用依赖。并根据默认的情形设置功能flag开启状态</para>
		/// </summary>
		public void SetRelatedEnemyRefAndInitializeFunction(EnemyARPGCharacterBehaviour behaviour)
		{
			gameObject.SetActive(true);
			RelatedEnemyBehaviourRef = behaviour;
			transform.SetParent(behaviour._HUDHeightMatchGameObject.transform);
			transform.localPosition = Vector3.zero;
			transform.localScale = 0.01f * Vector3.one;
			transform.localRotation = Quaternion.identity;
			ResetToDefault();
			behaviour.GetRelatedActionBus()
				.RegisterAction(ActionBus_ActionTypeEnum.L_ARPGBehaviour_BehaviourDataInvalid_角色行为数据不再有效,
					_ABC_RemoveHUDItem_OnEnemyDataInvalid);
			
			//Boss不应当走到这里，它在UIP的响应那里就应该已经return掉用另外的东西了
			if (behaviour.ApplyDamage_CheckTargetBuff(RolePlay_BuffTypeEnum._EnemyTag_BossEnemy_首领敌人) ==
			    BuffAvailableType.Available_TimeInAndMeetRequirement)
			{
				DBug.LogError( "UIRW_敌人上方HUD ：Boss不应当走到这里，它在UIP的响应那里就应该已经return掉用另外的东西了");
				return;
			}

			//给自己加设置
			if (_comRef._DefaultIgnoreEnemyTypeList.Contains(behaviour.SelfBehaviourNamedType))
			{
				SelfFunctionFlag = DefaultAboveHUDConfig_InClipTypeList;
			}
			else if (behaviour.ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum._EnemyTag_NormalEnemy_普通敌人) ==
			         BuffAvailableType.Available_TimeInAndMeetRequirement)
			{
				SelfFunctionFlag = DefaultAboveHUDConfig_NormalEnemy;
			}
			else if (behaviour.ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum._EnemyTag_EliteEnemy_精英敌人) ==
			         BuffAvailableType.Available_TimeInAndMeetRequirement)
			{
				SelfFunctionFlag = DefaultAboveHUDConfig_EliteEnemy;
			}


			InitMultipleFunctionFlags(SelfFunctionFlag);
			
		}

#endregion

#region 血条

		[LabelText("rt_血条Holder"), SerializeField, Required]
		[TitleGroup("血条/===Widget===")]
		protected RectTransform _rt_HealthSliderHolder;


		[TitleGroup("血条")]
		[LabelText("_srSlider_血条白底Slider"), SerializeField, Required, TitleGroup("血条/===Widget===")]
		protected SpriteRenderer slider_HealthSlider_WhiteBG;

		[LabelText("_srSlider_血条红色slider"), SerializeField, Required, TitleGroup("血条/===Widget===")]
		protected SpriteRenderer slider_HealthSlider_Red;


		/*
		 * 白条总是会试图在若干秒后Tween到当前红条值，每次都会重新设置那个delay
		 */

		/// <summary>
		/// 红条变短的那个Tween
		/// </summary>
		private Tweener _tween_RedSliderChangeValue;
		private Tweener _tween_WhiteSliderChangeValue;

		private float _lastHPChangeTime;



		private void _InternalInit_EnableHPSliderFunction()
		{
			RelatedEnemyBehaviourRef.GetRelatedActionBus().RegisterAction(ActionBus_ActionTypeEnum.L_DataEntry_OnDataEntryValueChanged_数据项的值被改变,
				_ABC_UpdateSlider_OnDataValueChanged);
			
			if(SelfFunctionFlag.HasFlag( EnemyAboveHUDItemFunctionFlagEnum.HideHPSliderOnNoHurt_一段时间没有HP变动则隐藏HP))
			{
				_rt_HealthSliderHolder.gameObject.SetActive(false);
			}
			else
			{
				_rt_HealthSliderHolder.gameObject.SetActive(true);
			}
			slider_HealthSlider_Red.gameObject.SetActive(true);
			slider_HealthSlider_WhiteBG.gameObject.SetActive(true);
			_maxHPEntry = RelatedEnemyBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.HPMax_最大HP);
			slider_HealthSlider_Red.material.SetFloat("_FillAmount", 1f);
			slider_HealthSlider_WhiteBG.material.SetFloat("_FillAmount", 1f);
		

		}

		private void _InternalInit_DisableHPSliderFunction()
		{
			_rt_HealthSliderHolder.gameObject.SetActive(false);
		}
		


		private void _ABC_UpdateSlider_OnDataValueChanged(DS_ActionBusArguGroup ds)
		{
			if (!gameObject.activeInHierarchy)
			{
				return;
			}


			var dataEntry = ds.ObjectArgu1 as FloatPresentValue_RPDataEntry;
			if (dataEntry == null)
			{
				return;
			}


			if (dataEntry.RP_DataEntryType == RP_DataEntry_EnumType.CurrentHP_当前HP)
			{
				var pv = dataEntry;
				var targetValue = pv.CurrentValue;
				var maxValue = _maxHPEntry.CurrentValue;
				var partial = targetValue / maxValue;
				if (SelfFunctionFlag.HasFlag(EnemyAboveHUDItemFunctionFlagEnum.HideHPSliderOnNoHurt_一段时间没有HP变动则隐藏HP))
				{
					_lastHPChangeTime = BaseGameReferenceService.CurrentFixedTime;
					// if (partial > 0.999f)
					// {
					// 	if (_rt_HealthSliderHolder.gameObject.activeInHierarchy)
					// 	{
					// 		_tween_RedSliderChangeValue?.Kill();
					// 		_tween_WhiteSliderChangeValue?.Kill();
					// 		_rt_HealthSliderHolder.gameObject.SetActive(false);
					// 	}
					// 	return;
					// }
					// else
					// {
					// 	if(!_rt_HealthSliderHolder.gameObject.activeInHierarchy)
					// 	{
					// 		_rt_HealthSliderHolder.gameObject.SetActive(true);
					// 	}
					// }
				}
				if (!_rt_HealthSliderHolder.gameObject.activeInHierarchy)
				{
					_rt_HealthSliderHolder.gameObject.SetActive(true);
				}
				
				if (_tween_RedSliderChangeValue == null)
				{
					_tween_RedSliderChangeValue =
						slider_HealthSlider_Red.material.DOFloat(partial, "_FillAmount", _RedSliderTweenDuration);
				}
				//如果上次未完成，就立刻停止，直接重设动画
				else if (!_tween_RedSliderChangeValue.IsComplete())
				{
					_tween_RedSliderChangeValue.Kill();
					_tween_RedSliderChangeValue = slider_HealthSlider_Red.material.DOFloat(partial,
						"_FillAmount",
						_RedSliderTweenDuration);
				}

				//然后设置白条
				if (_tween_WhiteSliderChangeValue == null)
				{
					_tween_WhiteSliderChangeValue = slider_HealthSlider_WhiteBG.material.DOFloat(partial,
						"_FillAmount",
						_WhiteSliderTweenDuration).SetDelay(_WhiteSliderTweenDelay);
				}
				//如果白条也没有完成，那就立刻停止，然后重设
				else if (!_tween_WhiteSliderChangeValue.IsComplete())
				{
					_tween_WhiteSliderChangeValue.Kill();
					_tween_WhiteSliderChangeValue = slider_HealthSlider_WhiteBG.material.DOFloat(partial,
						"_FillAmount",
						_WhiteSliderTweenDuration).SetDelay(_WhiteSliderTweenDelay);
				}
			}
		}


#endregion

#region 名字

		


		[LabelText("text_文本部分")]
		public TextMeshPro text_EnemyNameText;



		private void _InternalInit_EnableNameTextLabelFunction()
		{

			text_EnemyNameText.gameObject.SetActive(true);

			text_EnemyNameText.text = _grsRef.GetLocalizedStringByTableAndKey(LocalizationTableNameC._LCT_CharacterName,
				RelatedEnemyBehaviourRef.SelfBehaviourNamedType.ToString());
			text_EnemyNameText.fontSize = _parentPanelRef._defaultNameTextFontSize;
			text_EnemyNameText.color = Color.white;
		}

		private void _InternalInit_DisableNameTextLabelFunction()
		{
			text_EnemyNameText.gameObject.SetActive(false);
		}
		
		
		public void ShowTextAs(ESA_调整HUD内容_ModifySelfHUD contentESA)
		{
			AddSingleFunctionFlag(EnemyAboveHUDItemFunctionFlagEnum.IncludeNameTextLabel_包含名字标签);
			if (contentESA._EnableText)
			{
				if (contentESA._EnableNameFloatAnimation)
				{
					AddSingleFunctionFlag(EnemyAboveHUDItemFunctionFlagEnum.IncludeFullHolderFloating_包含整体的上下浮动效果);
				}
				if(!contentESA._UseDefaultName)
				{
					text_EnemyNameText.text =
						_grsRef.GetLocalizedStringByTableAndKey(LocalizationTableNameC._LCT_CharacterName,
							contentESA._TextKey);
				}
				else
				{
					text_EnemyNameText.text =
						_grsRef.GetLocalizedStringByTableAndKey(LocalizationTableNameC._LCT_CharacterName,
							RelatedEnemyBehaviourRef.SelfBehaviourNamedType.ToString());
				}
			
				text_EnemyNameText.fontSize = contentESA._TextSize;
				text_EnemyNameText.color = contentESA._TextColor;
			}
		}

		public void HideText()
		{
			RemoveSingleFunctionFlag(EnemyAboveHUDItemFunctionFlagEnum.IncludeNameTextLabel_包含名字标签);
			text_EnemyNameText.gameObject.SetActive(false);
		}

		// private void RefreshTextContent()
		// {
		// 	//临时方案：
		// 	//检查三个buff，有的话显示内容，响应buff刷新生成
		// 	if (!RelatedEnemyBehaviourRef.CharacterDataValid)
		// 	{
		// 		text_EnemyNameText.gameObject.SetActive(false);
		// 		return;
		// 	}
		// 	if (!text_EnemyNameText.gameObject.activeSelf)
		// 	{
		// 		if (RelatedEnemyBehaviourRef.ApplyDamage_CheckTargetBuff(RolePlay_BuffTypeEnum.Enemy_CommonWild_基本狂化) !=
		// 		    BuffAvailableType.NotExist ||
		// 		    RelatedEnemyBehaviourRef.ApplyDamage_CheckTargetBuff(RolePlay_BuffTypeEnum
		// 			    .Enemy_SingleBattleWild_单体战斗狂化) != BuffAvailableType.NotExist)
		// 		{
		// 			text_EnemyNameText.text = "[狂化·勇士级]";
		// 			text_EnemyNameText.fontSize = 285f;
		// 			text_EnemyNameText.gameObject.SetActive(true);
		// 		}
		// 		else if (RelatedEnemyBehaviourRef.ApplyDamage_CheckTargetBuff(RolePlay_BuffTypeEnum
		// 			         .Enemy_CommonShieldBuilding_基本盾构) != BuffAvailableType.NotExist ||
		// 		         RelatedEnemyBehaviourRef.ApplyDamage_CheckTargetBuff(RolePlay_BuffTypeEnum
		// 			         .Enemy_ShockWaveShieldBuilding_冲击波盾构) != BuffAvailableType.NotExist)
		// 		{
		// 			text_EnemyNameText.text = "[盾构·勇士级]";
		// 			text_EnemyNameText.fontSize = 285f;
		// 			text_EnemyNameText.gameObject.SetActive(true);
		// 		}
		// 		else if (RelatedEnemyBehaviourRef.ApplyDamage_CheckTargetBuff(RolePlay_BuffTypeEnum
		// 			.Enemy_CommonSplit_通用分裂) != BuffAvailableType.NotExist)
		// 		{
		// 			text_EnemyNameText.text = "[分裂·勇士级]";
		// 			text_EnemyNameText.fontSize = 285f;
		// 			text_EnemyNameText.gameObject.SetActive(true);
		// 		}
		// 	}
		// }


#endregion


#region 弱点


		[LabelText("slider_弱点条Slider"), SerializeField, Required, TitleGroup("===Widget===")]
		protected SpriteRenderer slider_WeaknessSlider;


		private float _sliderTargetValue;


		private Tweener _tween_weaknessSlider;





		/// <summary>
		/// <para>计量条中【常驻】的那个</para>
		/// </summary>
		private Buff_通用敌人弱点_CommonEnemyWeakness.WeaknessInfoGroup _residentWeaknessInfoGroupRef;



		private void _InternalInit_EnableWeaknessSlider()
		{
			
			//如果包含弱点Slider，则会额外增加Buff被添加的监听，用来检查什么时候弱点这个Buff被加入。因为设置AboveHUD是先于生成时设置弱点附加buff的，所以需要这样来获取依赖引用
			var lab = RelatedEnemyBehaviourRef.GetRelatedActionBus();
			lab.RegisterAction(ActionBus_ActionTypeEnum.L_Buff_OnBuffInitialized_一个Buff被确定初始化了,
				_ABC_TryGetWeaknessBuffRef_OnBuffInitialized);
			

		}


		private void _ABC_TryGetWeaknessBuffRef_OnBuffInitialized(DS_ActionBusArguGroup ds)
		{
			if ((RolePlay_BuffTypeEnum)ds.IntArgu1.Value != RolePlay_BuffTypeEnum.CommonEnemyWeakness_通用敌人弱点)
			{
				return;
			}
			var buffRef = ds.ObjectArgu1 as Buff_通用敌人弱点_CommonEnemyWeakness;
			var mainGet = 
			buffRef.GetMainWeaknessInfoGroup();
			if (mainGet == null)
			{
				return;
			}
			_residentWeaknessInfoGroupRef = mainGet;


			slider_WeaknessSlider.gameObject.SetActive(true);
			slider_WeaknessSlider.material.SetFloat("_FillAmount", 0f);

			
			
			
		}
		

		private void _InternalInit_DisableWeaknessSlider()
		{
			slider_WeaknessSlider.gameObject.SetActive(false);
		}

		
		
#endregion

#region Buff们


		[LabelText("layout_Buff布局"), SerializeField, Required, TitleGroup("===Widget===")]
		protected LayoutGroup layout_BuffLayoutGroup;

		private List<UIRW_SingleBuffIconOnEnemyHUD> _list_selfRelatedBuffIconUIRW =
			new List<UIRW_SingleBuffIconOnEnemyHUD>();





		private void _InternalInit_EnableBuffGroupFunction()
		{

			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_UI_RequireBuffDisplayContent_要求Buff显示内容,
				_ABC_ProcessBuffLayoutGroup_OnBuffDisplayBroadcast);
			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_UI_BuffDisplayContentClear_清理Buff显示内容,
				_ABC_ProcessBuffRemoveInLayoutGroup_OnBuffDisplayClearBroadcast);

			layout_BuffLayoutGroup.gameObject.SetActive(true);


		}

		private void _InternalInit_DisableBuffGroupFunction()
		{
			
			
			
			
		}
		
		/// <summary>
		/// <para>当Buff在全局上广播要求显示时，在此处进行处理</para>
		/// </summary>
		private void _ABC_ProcessBuffLayoutGroup_OnBuffDisplayBroadcast(DS_ActionBusArguGroup ds)
		{
			I_BuffContentMayDisplayOnUI I_DisplayRef = ds.ObjectArgu1 as I_BuffContentMayDisplayOnUI;
			if (!I_DisplayRef.RelatedBehaviourDataValid())
			{
				return;
			}
			
			
			switch (I_DisplayRef)
			{
				case BaseRPBuff baseRPBuff:
					if (!ReferenceEquals(baseRPBuff.Parent_SelfBelongToObject, RelatedEnemyBehaviourRef))
					{
						return;
					}
					if (_list_selfRelatedBuffIconUIRW.Exists((hud =>
						hud.SelfTargetBuffInterfaceRef is BaseRPBuff buff &&
						buff.SelfBuffType == baseRPBuff.SelfBuffType)))
					{
						return;
					}
					//跳过一级元素标签， 它们被元素标签组处理了，不在这里显示
					if (baseRPBuff is FirstElementTagBuff)
					{
						return;
					}
					break;
				case PerBuffTimedStack perStack:
					if (!ReferenceEquals((perStack.RelatedBuffRef as BaseRPBuff).Parent_SelfBelongToObject,
						RelatedEnemyBehaviourRef))
					{
						return;
					}
					break;
			}
			
			var content = ds.ObjectArgu2 as ConcreteBuffDisplayOnUIInfo;
			if (content.BuffUIType == BuffUITypeEnum.EnemyShowBuff_敌人需要显示的Buff)
			{
				if (_list_selfRelatedBuffIconUIRW.Exists((hud => hud.SelfTargetBuffInterfaceRef == I_DisplayRef)))
				{
					return;
				}
				var newUIRW = _parentPanelRef.GetFreeBuffIcon();
				newUIRW.transform.SetParent(layout_BuffLayoutGroup.transform);
				newUIRW.transform.localPosition = Vector3.zero;
				newUIRW.transform.localRotation = Quaternion.identity;
				newUIRW.transform.localScale = Vector3.one;
				_list_selfRelatedBuffIconUIRW.Add(newUIRW);
				newUIRW.SetTargetBuff(this, I_DisplayRef);
			}
		}

		private void _ABC_ProcessBuffRemoveInLayoutGroup_OnBuffDisplayClearBroadcast(DS_ActionBusArguGroup ds)
		{
			I_BuffContentMayDisplayOnUI I_DisplayRef = ds.ObjectArgu1 as I_BuffContentMayDisplayOnUI;
			var content = ds.ObjectArgu2 as ConcreteBuffDisplayOnUIInfo;
			int findI = -1;
			switch (I_DisplayRef)
			{
				case BaseRPBuff baseRPBuff:
					findI = _list_selfRelatedBuffIconUIRW.FindIndex((hud =>
						hud.SelfTargetBuffInterfaceRef is BaseRPBuff buff &&
						buff.SelfBuffType == baseRPBuff.SelfBuffType));
					break;
				case PerBuffTimedStack perStack:
					findI = _list_selfRelatedBuffIconUIRW.FindIndex((hud =>
						hud.SelfTargetBuffInterfaceRef is PerBuffTimedStack stack &&
						(stack.RelatedBuffRef as BaseRPBuff).SelfBuffType ==
						(perStack.RelatedBuffRef as BaseRPBuff).SelfBuffType));
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(I_DisplayRef));
			}
			if (findI == -1)
			{
				return;
			}
			UIRW_SingleBuffIconOnEnemyHUD targetUIRW = _list_selfRelatedBuffIconUIRW[findI];


			_parentPanelRef.ReturnBuffIconUIRW(targetUIRW);
			_list_selfRelatedBuffIconUIRW.RemoveAt(findI);
		}


#endregion

#region 一些对外设置Transform

		public void SetUIRWHeight(float heightAt)
		{
			_rect_SelfRectTransform.anchoredPosition =
				new Vector2(_rect_SelfRectTransform.anchoredPosition.x, heightAt);
			_tween_selfPositionYoyo?.Kill();
			rect_SuspensionAnchor.anchoredPosition = Vector2.zero;
			_tween_selfPositionYoyo = rect_SuspensionAnchor.DOAnchorPosY(_自身上下浮动时posY移动高度, _自身上下浮动时单次时长)
				.SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
		}

#endregion






		public void UpdateTick(float ct, int cf, float delta)
		{
			if (!gameObject.activeInHierarchy)
			{
				return;
			}

			if (cf % 5 == 0)
			{
				foreach (UIRW_SingleBuffIconOnEnemyHUD perIcon in _list_selfRelatedBuffIconUIRW)
				{
					perIcon.RefreshRemainingPartial();
				}
			}

			if (SelfFunctionFlag.HasFlag(EnemyAboveHUDItemFunctionFlagEnum.HideHPSliderOnNoHurt_一段时间没有HP变动则隐藏HP) &&
			    ct > (_lastHPChangeTime + _parentPanelRef._waitTimeToHideHPBar) )
			{
				_rt_HealthSliderHolder.gameObject.SetActive(false);
			}


			if (_residentWeaknessInfoGroupRef != null)
			{
				var cw = _residentWeaknessInfoGroupRef.CurrentAmount;
				var maxW = _residentWeaknessInfoGroupRef.TriggerAmount;
				var partial = cw / maxW;
				//不相同，则试图刷这个动画
				if (!Mathf.Approximately(_sliderTargetValue, partial))
				{
					if (_tween_weaknessSlider != null)
					{
						_tween_weaknessSlider.Kill();
					}
					_sliderTargetValue = partial;
					_tween_weaknessSlider = slider_WeaknessSlider.material.DOFloat(partial, "_FillAmount", 0.05f);
				}
			}
			
			
		}

#region 文本显示

#endregion

#region 清理

		private void _ABC_RemoveHUDItem_OnEnemyDataInvalid(DS_ActionBusArguGroup ds)
		{
			var targetBehaviour = ds.ObjectArgu1 as EnemyARPGCharacterBehaviour;

			ClearOnReturn();

			// var targetWeaknessIndex =
			// 	_list_currentActiveWeaknessBarUIRWList.FindIndex((item =>
			// 		item.RelatedEnemyBehaviourRef == targetBehaviour));
			// if (targetWeaknessIndex != -1)
			// {
			// 	var targetWeakness = _list_currentActiveWeaknessBarUIRWList[targetWeaknessIndex];
			//
			// 	_list_currentActiveWeaknessBarUIRWList.RemoveAt(targetWeaknessIndex);
			// 	Destroy(targetWeakness.gameObject);
			// }
		}


		public void ClearOnReturn()
		{
			RelatedEnemyBehaviourRef.GetRelatedActionBus()
				.RemoveAction(ActionBus_ActionTypeEnum.L_DataEntry_OnDataEntryValueChanged_数据项的值被改变, _ABC_UpdateSlider_OnDataValueChanged);
			RelatedEnemyBehaviourRef.GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_Buff_OnBuffInitialized_一个Buff被确定初始化了,
				_ABC_TryGetWeaknessBuffRef_OnBuffInitialized,
				true);

			RelatedEnemyBehaviourRef.GetRelatedActionBus()
				.RemoveAction(ActionBus_ActionTypeEnum.L_ARPGBehaviour_BehaviourDataInvalid_角色行为数据不再有效,
					_ABC_RemoveHUDItem_OnEnemyDataInvalid);
			
			
			
			var gab = GlobalActionBus.GetGlobalActionBus();
			gab.RemoveAction(ActionBus_ActionTypeEnum.G_UI_RequireBuffDisplayContent_要求Buff显示内容,
				_ABC_ProcessBuffLayoutGroup_OnBuffDisplayBroadcast,
				true);
			gab.RemoveAction(ActionBus_ActionTypeEnum.G_UI_BuffDisplayContentClear_清理Buff显示内容,
				_ABC_ProcessBuffRemoveInLayoutGroup_OnBuffDisplayClearBroadcast,
				true);

			
			gameObject.SetActive(false);
			RelatedEnemyBehaviourRef = null;
			_residentWeaknessInfoGroupRef = null;
			_maxHPEntry = null;
			
			if (_list_selfRelatedBuffIconUIRW != null)
			{
				for (int i = _list_selfRelatedBuffIconUIRW.Count - 1; i >= 0; i--)
				{
					_parentPanelRef.ReturnBuffIconUIRW(_list_selfRelatedBuffIconUIRW[i]);
				}
				_list_selfRelatedBuffIconUIRW.Clear();
			}

			_parentPanelRef.ReturnAboveHUDItem(this);
		}

#endregion





	}
}