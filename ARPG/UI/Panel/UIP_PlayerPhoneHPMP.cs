using System.Collections.Generic;
using ARPG.Character;
using Global;
using Global.ActionBus;
using Global.UIBase;
using RPGCore.DataEntry;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace ARPG.UI.Panel
{
	public class UIP_PlayerPhoneHPMP : UI_UIBasePanel
	{
		[SerializeField, Required, LabelText("HP - 滑条Holder"), FoldoutGroup("配置", true)]
		private GameObject _holder_HPSliderHolder;

		[SerializeField, Required, LabelText("HP - 滑条"), FoldoutGroup("配置", true)]
		private Slider _slider_HPSlider;

		[SerializeField, Required, LabelText("HP - 文本"), FoldoutGroup("配置", true)]
		private TextMeshProUGUI _text_HPText;

		[SerializeField, Required, LabelText("HP高光"), FoldoutGroup("配置", true)]
		private GameObject HPHighLight;

		[SerializeField, Required, LabelText("SP - 滑条Holder"), FoldoutGroup("配置", true)]
		private GameObject _holder_SPSliderHolder;

		[SerializeField, Required, LabelText("SP - 滑条"), FoldoutGroup("配置", true)]
		private Slider _slider_SPSlider;

		[SerializeField, Required, LabelText("SP高光"), FoldoutGroup("配置", true)]
		private GameObject SPHighLight;

		[SerializeField, Required, LabelText("Text-角色名字"), FoldoutGroup("配置", true)]
		private TextMeshProUGUI _text_CharacterName;

		[SerializeField, Required, LabelText("Image _ 主要角色的UP的UP覆盖"), FoldoutGroup("配置", true)]
		private Image _image_Main_UPOverImage;

		[SerializeField, Required, LabelText("超能图标1"), FoldoutGroup("配置", true)]
		private Image UPOverlayImage1;

		[SerializeField, Required, LabelText("超能图标2"), FoldoutGroup("配置", true)]
		private Image UPOverlayImage2;

		[SerializeField, Required, LabelText("超能满了发光"), FoldoutGroup("配置", true)]
		private GameObject UPFullLight;

		[SerializeField, Required, LabelText("受击后血量变红"), FoldoutGroup("配置", true)]
		private GameObject HitTurnsRed;

		[SerializeField, Required, LabelText("受击后血量变红的时间渐变"), FoldoutGroup("配置", true)]
		private float HitTurnsRedTime = 2f;

		private PlayerARPGConcreteCharacterBehaviour _currentActivePlayerBehaviourRef;

		private Float_RPDataEntry _hpEntryRef;
		private Float_RPDataEntry _maxHPEntryRef;
		private Float_RPDataEntry _spEntryRef;
		private Float_RPDataEntry _maxSPEntryRef;
		// private Float_RPDataEntry _upEntryRef;

		protected override void BindingEvent()
		{
			base.BindingEvent();
			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_PC_OnCurrentUsingCharacterChanged_当前使用的角色更换,
				_ABC_SpawnCharacter_OnCurrentActiveCharacterChanged);
		}


		private void _ABC_SpawnCharacter_OnCurrentActiveCharacterChanged(DS_ActionBusArguGroup ds)
		{
			_currentActivePlayerBehaviourRef = ds.ObjectArgu1 as PlayerARPGConcreteCharacterBehaviour;

			var relatedAsset = GlobalConfigurationAssetHolderHelper.GetGCAHH().FE_ARPGCharacterInitConfig
				.GetConfigByBehaviourRef(_currentActivePlayerBehaviourRef);

			_text_CharacterName.text = new LocalizedString(LocalizationTableNameC._LCT_CharacterName, relatedAsset.Name)
				.GetLocalizedString();

			// //移除老角色本地事件线监听，防止重复绑定，然后绑定新角色本地事件线监听
			// if (ds.ObjectArgu2 != null)
			// {
			// 	(ds.ObjectArgu2 as PlayerARPGConcreteCharacterBehaviour).GetRelatedActionBus().RemoveAction(
			// 		ActionBus_ActionTypeEnum.L_DamageOnDamageTakenOnHP_伤害打到了HP上, _ABC_SelfHPHurt);
			// }
			//
			//
			// _currentActivePlayerBehaviourRef.GetRelatedActionBus().RegisterAction(
			// 	ActionBus_ActionTypeEnum.L_DamageOnDamageTakenOnHP_伤害打到了HP上, _ABC_SelfHPHurt);


			//切出角色，同步一下HP SP UP的数据
			_hpEntryRef =
				_currentActivePlayerBehaviourRef.GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentHP_当前HP);
			_maxHPEntryRef = _currentActivePlayerBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.HPMax_最大HP);
			_spEntryRef =
				_currentActivePlayerBehaviourRef.GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentSP_当前SP);

			_maxSPEntryRef = _currentActivePlayerBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.SPMax_最大SP);

			// _upEntryRef =
			// 	_currentActivePlayerBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.CurrentUP_当前UP);
		}

		private void _ABC_SelfHPHurt(DS_ActionBusArguGroup ds)
		{
			if ((ds.ObjectArgu1 as RP_DS_DamageApplyResult).Receiver == _currentActivePlayerBehaviourRef)
			{
				HitTurnsRed.SetActive(true);
			}
		}


		public override void UpdateTick(float currentTime, int currentFrameCount, float deltaTime)
		{
			if (_currentActivePlayerBehaviourRef == null)
			{
				return;
			}

			if (HitTurnsRed.activeSelf)
			{
				Image targetImage = HitTurnsRed.GetComponent<Image>();
				Color targetColor = targetImage.color;
				if (targetImage.color.a <= 0)
				{
					targetColor.a = 1;
					targetImage.color = targetColor;
					HitTurnsRed.SetActive(false);
				}
				else
				{
					targetColor.a -= deltaTime / HitTurnsRedTime;
					targetImage.color = targetColor;
				}
			}

			base.UpdateTick(currentTime, currentFrameCount, deltaTime);
			_slider_HPSlider.value = _hpEntryRef.CurrentValue / _maxHPEntryRef.CurrentValue;
			_text_HPText.text = $"{_hpEntryRef.CurrentValue:F0}/{_maxHPEntryRef.CurrentValue:F0}";
			if (_slider_HPSlider.value < 0.95f)
			{
				HPHighLight.SetActive(true);
				Vector3 targetPositionHP = HPHighLight.transform.localPosition;
				RectTransform targetHPRect = _holder_HPSliderHolder.GetComponent<RectTransform>();
				targetPositionHP.x = _slider_HPSlider.value * targetHPRect.rect.width - targetHPRect.rect.width / 2f;
				HPHighLight.transform.localPosition = targetPositionHP;
				if (_slider_HPSlider.value < 0.05f)
				{
					HPHighLight.SetActive(false);
				}
			}

			_slider_SPSlider.value = _spEntryRef.CurrentValue / _maxSPEntryRef.CurrentValue;
			// _text_SPText.text = $"{_spEntryRef.CurrentValue:F0}/{_maxSPEntryRef.CurrentValue:F0}";

			if (_slider_SPSlider.value < 0.95f)
			{
				SPHighLight.SetActive(true);
				Vector3 targetPositionSP = SPHighLight.transform.localPosition;
				RectTransform targetSPRect = _holder_SPSliderHolder.GetComponent<RectTransform>();
				targetPositionSP.x = _slider_SPSlider.value * targetSPRect.rect.width - targetSPRect.rect.width / 2f;
				SPHighLight.transform.localPosition = targetPositionSP;
				if (_slider_SPSlider.value < 0.05f)
				{
					SPHighLight.SetActive(false);
				}
			}

			// _image_Main_UPOverImage.fillAmount = _upEntryRef.CurrentValue / 100f;
			//
			// UPOverlayImage1.fillAmount = 1 - _upEntryRef.CurrentValue / 100f;
			//
			// Color tempColor1 = UPOverlayImage1.color;
			// tempColor1.a = 1 - _upEntryRef.CurrentValue / 100f;
			// UPOverlayImage1.color = tempColor1;
			//
			// UPOverlayImage2.fillAmount = _upEntryRef.CurrentValue / 100f;
			//
			// Color tempColor2 = UPOverlayImage1.color;
			// tempColor2.a = _upEntryRef.CurrentValue / 100f;
			// UPOverlayImage2.color = tempColor2;
			//
			// if (_upEntryRef.CurrentValue / 100f > 0.9999f)
			// {
			// 	UPFullLight.SetActive(true);
			// }
			// else
			// {
			// 	UPFullLight.SetActive(false);
			// }
		}
	}
}