using System;
using System.Collections.Generic;
using ARPG.Character;
using ARPG.Character.Config;
using ARPG.Manager;
using DG.Tweening;
using Global;
using Global.ActionBus;
using Global.UI;
using Global.UIBase;
using Global.Utility;
using RPGCore.Buff.ConcreteBuff;
using RPGCore.DataEntry;
using RPGCore.Skill;
using RPGCore.Skill.Config;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ARPG.UI.Panel
{
	/// <summary>
	/// <para>ARPG场景，左下角玩家状态</para>
	/// </summary>
	public class UIP_PlayerStatePanel : UI_UIBasePanel
	{
#region 当前角色

		[LabelText("image_当前角色_底板"), SerializeField, Required, TitleGroup("===Widget===")]
		private Image _image_CurrentCharacterBaseImage;


		[LabelText("image_当前角色_头像"), SerializeField, Required, TitleGroup("===Widget===")]
		private Image _image_CurrentCharacterImage;

		[LabelText("image_当前角色职业图标 "), SerializeField, Required, TitleGroup("===Widget===")]
		private Image _image_CurrentCharacterBattleJobImage;

        [LabelText("text_当前角色等级 "), SerializeField, Required, TitleGroup("===Widget===")]
        private TextMeshProUGUI _text_CurrentCharacterLevel;

        [LabelText("text_当前角色名字 "), SerializeField, Required, TitleGroup("===Widget===")]
		private TextMeshProUGUI _text_CurrentCharacterName;

#region 当前角色数值属性

		[LabelText("image_UP元素图标 "), SerializeField, Required, TitleGroup("===Widget===")]
		private Image _image_Main_UPFrontImage;
		private Tweener _tween_UPFrontImage;


		[LabelText("image_up元素发光"), SerializeField, Required, TitleGroup("===Widget===")]
		private Image _image_Main_UPOuterGlowImage;
		private Tweener _tween_UPOuterGlowImage;


		[LabelText("slider_UP充能填充"), SerializeField, Required, TitleGroup("===Widget===")]
		private Slider _slider_Main_UPSlider;

		// private FloatPresentValue_RPDataEntry _upEntryRef;
		[LabelText("image_UP充能填充图片 "), SerializeField, Required, TitleGroup("===Widget===")]
 		private Image _image_Main_UPSliderFillImage;


		[LabelText("slider_HP条"), SerializeField, Required, TitleGroup("===Widget===")]
		private Slider _slider_HPSlider;
		private Tweener _tween_HPBlueTween;
		[LabelText("slider_HP白色底板"), SerializeField, Required, TitleGroup("===Widget===")]
		private Slider _slider_HPWhiteBaseSlider;
		private Tweener _tween_HPWhiteTween;
		private FloatPresentValue_RPDataEntry _hpEntryRef;
		private Float_RPDataEntry _currentCharacterMaxHPEntryRef;


		[LabelText("slider_SP条"), SerializeField, Required, TitleGroup("===Widget===")]
		private Slider _slider_SPSlider;
		private Tweener _tween_SPBlueTween;
		[LabelText("slider_SP白色底板"), SerializeField, Required, TitleGroup("===Widget===")]
		private Slider _slider_SPWhiteBaseSlider;
		private Tweener _tween_SPWhiteTween;
		private FloatPresentValue_RPDataEntry _spEntryRef;
		private Float_RPDataEntry _currentCharacterMaxSPEntryRef;


        [LabelText("slider_Exp条"), SerializeField, Required, TitleGroup("===Widget===")]
        private Slider _slider_ExpSlider;
        private Tweener _tween_ExpBlueTween;
        [LabelText("slider_Exp白色底板"), SerializeField, Required, TitleGroup("===Widget===")]
        private Slider _slider_ExpWhiteBaseSlider;
        private Tweener _tween_ExpWhiteTween;
        private FloatPresentValue_RPDataEntry _expEntryRef;
        private Float_RPDataEntry _currentCharacterLevelUpExpEntryRef;



        [LabelText("image_血量警示闪烁"), SerializeField, Required, TitleGroup("===Widget===")]
		private Image _image_HPChangeBlinkImage;

		private Tweener _tween_HPBlink;

#endregion

#endregion

#region 候选

		[LabelText("prefab-角色候选 "), SerializeField, Required, TitleGroup("===Prefab===")]
		private GameObject _uirw_CandidateEntry;

		private List<UIRW_CharacterInCandidate> _uirw_CandidateList = new List<UIRW_CharacterInCandidate>();

		[LabelText("layout_角色候选Layout"), SerializeField, Required, TitleGroup("===Widget===")]
		private VerticalLayoutGroup _layout_CandidateLayoutGroup;

#endregion







		private PlayerARPGConcreteCharacterBehaviour _currentActivePlayerBehaviourRef;


		public override void StartInitializeByUIM()
		{
			base.StartInitializeByUIM();
			//在这里写试图获取一次玩家队伍
			//如果暂时没有玩家队伍，就不显示这个面板
			//如果有玩家队伍，就按配置依次索引队伍中每个角色的数据，将其显示在主面板上

			_currentCharacterSkillList = new List<SOConfig_RPSkill>();
			UIRW_PerPlayerSkillEntry.StaticInitialize(this);
			_image_HPChangeBlinkImage.gameObject.SetActive(false);
			
			base.StartInitializeByUIM();
		}

		protected override void BindingEvent()
		{
			base.BindingEvent();
			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_PC_OnCurrentUsingCharacterChanged_当前使用的角色更换,
				_ABC_RefreshPanelState_OnCurrentActiveCharacterChanged,
				100);
			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_UI_RequireSkillIconRefresh_要求技能图标刷新, 
				_ABC_RefreshSkillIcon);
			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_PC_OnTeamMemberChanged_队伍成员更换,
				_ABC_RefreshCurrentCharacterAndRebuildCandidate);
			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_Input_ChangeInputStyle_更换输入风格于同输入方式下, 
				_ABC_RefreshSkillInputText);
		}


		private void _ABC_RefreshPanelState_OnCurrentActiveCharacterChanged(DS_ActionBusArguGroup ds)
		{
			_currentActivePlayerBehaviourRef = ds.ObjectArgu1 as PlayerARPGConcreteCharacterBehaviour;


			if (ds.ObjectArgu2 != null)
			{
				(ds.ObjectArgu2 as PlayerARPGConcreteCharacterBehaviour).GetRelatedActionBus().RemoveAction(
					ActionBus_ActionTypeEnum.L_DataEntry_OnDataEntryValueChanged_数据项的值被改变,
					_ABC_ProcessSliderChange_OnValueChanged);
			}

			_currentActivePlayerBehaviourRef.GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_DataEntry_OnDataEntryValueChanged_数据项的值被改变,
				_ABC_ProcessSliderChange_OnValueChanged);



			_tween_HPBlueTween?.TryComplete();
			_tween_HPWhiteTween?.TryComplete();
			_tween_SPBlueTween?.TryComplete();
			_tween_SPWhiteTween?.TryComplete();
            _tween_ExpBlueTween?.TryComplete();
            _tween_ExpWhiteTween?.TryComplete();


            _hpEntryRef = _currentActivePlayerBehaviourRef.
				GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentHP_当前HP);
			_currentCharacterMaxHPEntryRef = _currentActivePlayerBehaviourRef.
				GetFloatDataEntryByType(RP_DataEntry_EnumType.HPMax_最大HP);
			_spEntryRef = _currentActivePlayerBehaviourRef.
				GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentSP_当前SP);
			_currentCharacterMaxSPEntryRef = _currentActivePlayerBehaviourRef.
				GetFloatDataEntryByType(RP_DataEntry_EnumType.SPMax_最大SP);
            _expEntryRef = _currentActivePlayerBehaviourRef.
				GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentExp);
            _currentCharacterLevelUpExpEntryRef = _currentActivePlayerBehaviourRef.
				GetFloatDataEntryByType(RP_DataEntry_EnumType.LevelUpExp);
            // _upEntryRef =
            // 	_currentActivePlayerBehaviourRef.GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentUP_当前UP);
            var fe_characterResource = GlobalConfigurationAssetHolderHelper.Instance.FE_CharacterResourceInfo;
			//刷新当前的各种数据
			_image_CurrentCharacterImage.sprite = fe_characterResource.GetIcon_Outer(_currentActivePlayerBehaviourRef
				.SelfBehaviourNamedType.GetCharacterNameStringByType());

            _text_CurrentCharacterLevel.text = "Lv." + _currentActivePlayerBehaviourRef.
                GetFloatDataEntryByType(RP_DataEntry_EnumType.CharacterLevel).GetRoundIntValue().ToString();

            _text_CurrentCharacterName.text = GlobalConfigurationAssetHolderHelper.GetCurrentBaseGRS()
				.GetLocalizedStringByTableAndKey(LocalizationTableNameC._LCT_CharacterName,
					_currentActivePlayerBehaviourRef.SelfBehaviourNamedType.GetCharacterNameStringByType());

			//根据当前角色伤害类型选定UP图标和颜色
			var currentDamageType =
				(_currentActivePlayerBehaviourRef.ApplyBuff_GetRelatedBuff(RolePlay_BuffTypeEnum
					.ChangeCommonDamageType_常规伤害类型更改) as Buff_ChangeCommonDamageType).CurrentDamageType;


			_image_Main_UPFrontImage.sprite = BaseUIManager.QuickGetDamageTypeIcon(currentDamageType, true);
			_image_Main_UPSliderFillImage.color = 
				_image_Main_UPOuterGlowImage.color =
				BaseUIManager.QuickGetDamageTypeOuterColor(currentDamageType);


			//TODO:设置角色战斗职业图标

			var currentHPPartial =
				_currentActivePlayerBehaviourRef.GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentHP_当前HP)
					.CurrentValue / _currentCharacterMaxHPEntryRef.CurrentValue;
			_slider_HPSlider.value = currentHPPartial;
			_slider_HPWhiteBaseSlider.value = currentHPPartial;

			var currentSPPartial =
				_currentActivePlayerBehaviourRef.GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentSP_当前SP)
					.CurrentValue / _currentCharacterMaxSPEntryRef.CurrentValue;
			_slider_SPSlider.value = currentSPPartial;
			_slider_SPWhiteBaseSlider.value = currentSPPartial;

            var currentExpPartial =
                _currentActivePlayerBehaviourRef.GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentExp)
                    .CurrentValue / _currentCharacterLevelUpExpEntryRef.CurrentValue;
            _slider_ExpSlider.value = currentExpPartial;
            _slider_ExpWhiteBaseSlider.value = currentExpPartial;

			//刷新技能
			FullyRefreshSkillLayout();
			RefreshCurrentCharacterAndRebuildCandidates();
		}

#region 数据项变动

		/// <summary>
		/// <para>当HP、SP、UP值发生变动时，刷新它们</para>
		/// </summary>
		/// <param name="ds"></param>
		private void _ABC_ProcessSliderChange_OnValueChanged(DS_ActionBusArguGroup ds)
		{
			if (ds.ObjectArgu1 is not FloatPresentValue_RPDataEntry fp)
			{
				return;
			}

			switch (fp.RP_DataEntryType)
			{
				case RP_DataEntry_EnumType.CurrentHP_当前HP:
					ProcessHPChange(ds.FloatArgu2.Value, ds.FloatArgu1.Value);
					break;
				case RP_DataEntry_EnumType.CurrentSP_当前SP:
					ProcessSPChange(ds.FloatArgu2.Value, ds.FloatArgu1.Value);
					break;
				case RP_DataEntry_EnumType.CurrentExp:
					ProcessExpChange(ds.FloatArgu2.Value, ds.FloatArgu1.Value);
					break;
			}
		}


		private void ProcessHPChange(float oriValue, float newValue)
		{
			var maxValue = _currentCharacterMaxHPEntryRef.CurrentValue;
			//原始值小于新值，说明是加血。
			if (oriValue < newValue)
			{
			}
			//扣血
			else
			{
				//如果HP变少了，则检查是否需要处理那个提示闪烁

				if ((_hpEntryRef.CurrentValue / _currentCharacterMaxHPEntryRef.CurrentValue) < 0.2f)
				{
					if (_tween_HPBlink == null || _tween_HPBlink.IsComplete())
					{
						return;
					}
					else
					{
						var tColor = new Color(1f, 1f, 1f, 0f);
						_image_HPChangeBlinkImage.gameObject.SetActive(true);
						_image_HPChangeBlinkImage.color = tColor;
						_tween_HPBlink = _image_HPChangeBlinkImage.DOFade(1f, 0.5f).SetLoops(-1, LoopType.Yoyo);
					}
				}
				else
				{
					if (_tween_HPBlink!=null && !_tween_HPBlink.IsComplete())
					{
						_tween_HPBlink.Kill();
					}
					_image_HPChangeBlinkImage.gameObject.SetActive(true);
					var tColor = new Color(1f, 1f, 1f, 0.3f);
					_image_HPChangeBlinkImage.color = tColor;
					_tween_HPBlink = _image_HPChangeBlinkImage.DOFade(1f, 0.3f).SetLoops(2,LoopType.Yoyo).OnComplete((() =>
						_image_HPChangeBlinkImage.gameObject.SetActive(false)));
					
				}
			}

            _tween_HPBlueTween?.TryComplete();
            _tween_HPBlueTween = _slider_HPSlider.DOValue(newValue / maxValue, 0.2f);

			_tween_HPWhiteTween?.Kill();
			_tween_HPWhiteTween = _slider_HPWhiteBaseSlider.DOValue(newValue / maxValue, 0.2f).SetDelay(0.2f);
		}



		private void ProcessSPChange(float oriValue, float newValue)
		{
			var maxValue = _currentCharacterMaxSPEntryRef.CurrentValue;
			//原始值小于新值，说明是SP。
			if (oriValue < newValue)
			{
			}
			//扣SP
			else
			{
				//如果SP变少了，则检查是否需要处理那个提示闪烁
			}

            _tween_SPBlueTween?.TryComplete();
            _tween_SPBlueTween = _slider_SPSlider.DOValue(newValue / maxValue, 0.2f);

			_tween_SPWhiteTween?.Kill();
			_tween_SPWhiteTween = _slider_SPWhiteBaseSlider.DOValue(newValue / maxValue, 0.2f).SetDelay(0.2f);
		}

		/// <summary>
		/// <para>检查当前Exp的值</para>
		/// </summary>
		private void ProcessExpChange(float oriValue, float newValue)
		{ 
            _text_CurrentCharacterLevel.text = "Lv." + _currentActivePlayerBehaviourRef.
                GetFloatDataEntryByType(RP_DataEntry_EnumType.CharacterLevel).GetRoundIntValue().ToString();

            var maxValue = _currentCharacterLevelUpExpEntryRef.CurrentValue;

            _tween_ExpBlueTween?.TryComplete();
            _tween_ExpBlueTween = _slider_ExpSlider.DOValue(newValue / maxValue, 0.2f);

            _tween_ExpWhiteTween?.Kill();
            _tween_ExpWhiteTween = _slider_ExpWhiteBaseSlider.DOValue(newValue / maxValue, 0.2f).SetDelay(0.2f);
        }

#endregion
		/// <summary>
		/// <para>刷新当前的角色，并且重建角色候选</para>
		/// </summary>
		/// <param name="ds"></param>
		private void RefreshCurrentCharacterAndRebuildCandidates()
		{
			var pcRef = SubGameplayLogicManager_ARPG.Instance.PlayerCharacterBehaviourControllerReference;
			var currentControlling = pcRef.CurrentControllingBehaviour;

			if (pcRef.CurrentControllingBehaviour == null)
			{
				return;
			}
			foreach (UIRW_CharacterInCandidate perUIRW in _uirw_CandidateList)
			{
				Destroy(perUIRW.gameObject);
			}
			_uirw_CandidateList.Clear();

			foreach (PlayerARPGConcreteCharacterBehaviour pc in pcRef.CurrentAllCharacterBehaviourList)
			{
				//主要角色已经刷新过了，不需要在这刷新
				if (pc == pcRef.CurrentControllingBehaviour)
				{
					continue;
				}
				UIRW_CharacterInCandidate newUIRW =
					Instantiate(_uirw_CandidateEntry, _layout_CandidateLayoutGroup.transform)
						.GetComponent<UIRW_CharacterInCandidate>();
				newUIRW.gameObject.SetActive(true);
				_uirw_CandidateList.Add(newUIRW);
				newUIRW.Initialize(pc, currentControlling, pc.CurrentPlayerSlot);
			}
		}



		private void _ABC_RefreshCurrentCharacterAndRebuildCandidate(DS_ActionBusArguGroup ds)
		{
			RefreshCurrentCharacterAndRebuildCandidates();	
		}




#region 技能

		[SerializeField, LabelText("当前记录的玩家技能项容器"), TitleGroup("===Widget===")]
		private List<UIRW_PerPlayerSkillEntry> _currentAllPlayerSkillEntryList;


		private RPSkill_SkillHolder _currentActivePlayerSkillHolderRef;
		private List<SOConfig_RPSkill> _currentCharacterSkillList = new List<SOConfig_RPSkill>();



		private void _ABC_RefreshSkillIcon(DS_ActionBusArguGroup ds)
		{
			foreach (UIRW_PerPlayerSkillEntry perUIRW in _currentAllPlayerSkillEntryList)
			{
				perUIRW.RefreshSkillIcon();
			}
		}


		private void _ABC_RefreshSkillInputText(DS_ActionBusArguGroup ds)
		{
			foreach (var perUIRW in _currentAllPlayerSkillEntryList)	
			{
				perUIRW.UpdateInputText();
				
				 
			}
		}


		/// <summary>
		/// <para>完整刷新技能布局</para>
		/// </summary>
		private void FullyRefreshSkillLayout()
		{
			_currentCharacterSkillList.Clear();
			_currentActivePlayerSkillHolderRef = _currentActivePlayerBehaviourRef.GetRelatedSkillHolder();
			_currentActivePlayerSkillHolderRef.GetCurrentSkillList(_currentCharacterSkillList);

			//向当前UIRWList刷新内容
			foreach (var perUIRW in _currentAllPlayerSkillEntryList)
			{
				var tt = _currentCharacterSkillList.FindIndex((skill =>
					skill.ConcreteSkillFunction.SkillSlot == perUIRW.SelfSkillSlotType));
				if (tt == -1)
				{
					perUIRW.InitializeOnInstantiate(null);
				}
				else
				{
					perUIRW.InitializeOnInstantiate(_currentCharacterSkillList[tt]);
				}
			}
		}


		public override void UpdateTick(float currentTime, int currentFrameCount, float deltaTime)
		{
			base.UpdateTick(currentTime, currentFrameCount, deltaTime);
			if (_currentAllPlayerSkillEntryList != null && _currentAllPlayerSkillEntryList.Count > 0)
			{
				foreach (UIRW_PerPlayerSkillEntry perEntry in _currentAllPlayerSkillEntryList)
				{
					perEntry.UpdateTick(currentTime, currentFrameCount, deltaTime);
				}
			}
			foreach (UIRW_CharacterInCandidate perUIRW in _uirw_CandidateList)
			{
				perUIRW.UpdateTick( currentTime,  currentFrameCount,  deltaTime);
			}
		}

#endregion
	}
}