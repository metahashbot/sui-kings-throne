using System.Collections.Generic;
using ARPG.Character.Enemy;
using ARPG.Manager;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Global;
using Global.ActionBus;
using Global.UIBase;
using RPGCore.Buff;
using RPGCore.Buff.ConcreteBuff.Common;
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace ARPG.UI.Panel.EnemyTopHUD
{
	public class UIRW_PerEliteTopHUD_EnemyTopHUDPanel : UI_UISingleRuntimeWidget
	{
		public enum EliteHUDPosition
		{
			BossTOP_Boss最上方 = 0,
			LeftLeft_最左侧 = 1,
			LeftMiddle_左中间 = 2,
			RightMiddle_右中间 = 3,
			RightRight_最右侧 = 4, }




		protected static float _RedSliderTweenDuration = 0.13f;
		protected static float _WhiteSliderTweenDuration = 0.1f;
		protected static float _WhiteSliderTweenDelay = 0.35f;

		/// <summary>
		/// <para>当前活跃吗||正在被使用</para>
		/// </summary>
		public bool CurrentUIRWActive { get; private set; }

		[SerializeField, LabelText("自己的HUD位置")]
		public EliteHUDPosition SelfHUDPosition;


		[LabelText("image_头像Icon"), SerializeField, Required, TitleGroup("===Widget===")]
		private UnityEngine.UI.Image image_EnemyIcon;

		[LabelText("text_名字文本"), SerializeField, Required, TitleGroup("===Widget===")]
		private TextMeshProUGUI text_nameText;


		[LabelText("slider_血条白底Slider"), SerializeField, Required, TitleGroup("===Widget===")]
		private Slider slider_HealthSlider_WhiteBG;

		[LabelText("slider_血条红色slider"), SerializeField, Required, TitleGroup("===Widget===")]
		private Slider slider_HealthSlider_Red;

		[LabelText("slider_弱点条Slider"), SerializeField, Required, TitleGroup("===Widget===")]
		private Slider slider_WeaknessSlider;

		private float _sliderTargetValue;


		private Tweener _tween_weaknessSlider;


		[LabelText("HL_Buff图标布局"), SerializeField, Required, TitleGroup("===Widget===")]
		private HorizontalLayoutGroup layout_BuffHUD;



		List<UIRW_EnemyBuffHUDEntry_EnemyTopHUD> _list_SelfBuffIconUIRWList =
			new List<UIRW_EnemyBuffHUDEntry_EnemyTopHUD>();




		public UIP_EnemyTopHUDPanel ParentRef { get; private set; }

		public EnemyARPGCharacterBehaviour RelatedEnemyBehaviourRef { get; private set; }


		private Float_RPDataEntry _maxHPEntry;

		/// <summary>
		/// <para>计量条中【常驻】的那个</para>
		/// </summary>
		private Buff_通用敌人弱点_CommonEnemyWeakness.WeaknessInfoGroup _residentWeaknessInfoGroupRef;







		/// <summary>
		/// <para>由UIP传入一个指定的敌人角色。需要数据是有效的，因为会向内添加很多监听</para>
		/// </summary>
		public void RefreshRelatedBehaviour(EnemyARPGCharacterBehaviour behaviour, UIP_EnemyTopHUDPanel parent)
		{
			// if (!behaviour.CharacterDataValid)
			// {
			//     DBug.LogError($"敌人血条： {nameof(behaviour)} 数据无效，这不应当出现，因为在刷新角色前通常");
			//     return;
			// }


			CurrentUIRWActive = true;
			ParentRef = parent;
			gameObject.SetActive(true);
			RelatedEnemyBehaviourRef = behaviour;

			var lab = behaviour.GetRelatedActionBus();
			lab.RegisterAction(ActionBus_ActionTypeEnum.L_DataEntry_OnDataEntryValueChanged_数据项的值被改变,
				_ABC_UpdateSlider);
			lab.RegisterAction(ActionBus_ActionTypeEnum.L_ARPGBehaviour_BehaviourDataInvalid_角色行为数据不再有效,
				_ABC_ClearRelated);
			lab.RegisterAction(ActionBus_ActionTypeEnum.L_Weakness_NewWeaknessGroupAdd_新的弱点组被添加,
				_ABC_CheckIfShowResidentWeaknessSlider);

			GlobalActionBus.GetGlobalActionBus()
				.RegisterAction(ActionBus_ActionTypeEnum.G_UI_RequireBuffDisplayContent_要求Buff显示内容,
					_CheckIfNeedAddBuff);



			GlobalActionBus.GetGlobalActionBus()
				.RegisterAction(ActionBus_ActionTypeEnum.G_UI_BuffDisplayContentClear_清理Buff显示内容, _CheckIfRemoveBuff);




			_maxHPEntry = behaviour.GetFloatDataEntryByType(RP_DataEntry_EnumType.HPMax_最大HP);
			slider_HealthSlider_Red.value = 1f;
			slider_HealthSlider_WhiteBG.value = 1f;


			//检查它的弱点buff
			var weakBuffCR = behaviour.ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum.CommonEnemyWeakness_通用敌人弱点);
			if (weakBuffCR != BuffAvailableType.NotExist)
			{
				var weaknessRef =
					behaviour.ReceiveBuff_GetTargetBuff(RolePlay_BuffTypeEnum.CommonEnemyWeakness_通用敌人弱点) as
						Buff_通用敌人弱点_CommonEnemyWeakness;
				_residentWeaknessInfoGroupRef = weaknessRef.GetMainWeaknessInfoGroup();
				if (_residentWeaknessInfoGroupRef != null)
				{
					ActivateResidentWeaknessSlider();
				}
				else
				{
					DeactivateResidentWeaknessSlider();
				}
			}
			//没有弱点
			else
			{
				slider_WeaknessSlider.gameObject.SetActive(false);
			}




			var resourceEntry = GlobalConfigurationAssetHolderHelper.GetGCAHH().FE_CharacterResourceInfo
				.GetConfigByType(behaviour.SelfBehaviourNamedType);

			text_nameText.text = GameReferenceService_ARPG.Instance.GetLocalizedStringByTableAndKey(
				LocalizationTableNameC._LCT_CharacterName,
				RelatedEnemyBehaviourRef.SelfBehaviourNamedType.ToString());
			image_EnemyIcon.sprite = resourceEntry.GetIcon_Inner();
		}

#region 血条变动

		/*
		 * 白条总是会试图在若干秒后Tween到当前红条值，每次都会重新设置那个delay
		 */

		/// <summary>
		/// 红条变短的那个Tween
		/// </summary>
		private Tweener _tween_RedSliderChangeValue;
		private Tweener _tween_WhiteSliderChangeValue;





		private void _ABC_UpdateSlider(DS_ActionBusArguGroup ds)
		{
			if (!CurrentUIRWActive)
			{
				return;
			}
			var dataEntry = ds.ObjectArgu1 as Float_RPDataEntry;
			if (dataEntry == null)
			{
				return;
			}


			if (dataEntry.RP_DataEntryType == RP_DataEntry_EnumType.CurrentHP_当前HP)
			{
				var pv = dataEntry as FloatPresentValue_RPDataEntry;
				var targetValue = pv.CurrentValue;
				var maxValue = _maxHPEntry.CurrentValue;
				var partial = targetValue / maxValue;
				if (_tween_RedSliderChangeValue == null)
				{
					_tween_RedSliderChangeValue = slider_HealthSlider_Red.DOValue(partial, _RedSliderTweenDuration);
				}
				//如果上次未完成，就立刻停止，直接重设动画
				else if (!_tween_RedSliderChangeValue.IsComplete())
				{
					_tween_RedSliderChangeValue.Kill();
					_tween_RedSliderChangeValue = slider_HealthSlider_Red.DOValue(partial, _RedSliderTweenDuration);
				}

				//然后设置白条
				if (_tween_WhiteSliderChangeValue == null)
				{
					_tween_WhiteSliderChangeValue = slider_HealthSlider_WhiteBG
						.DOValue(partial, _WhiteSliderTweenDuration).SetDelay(_WhiteSliderTweenDelay);
				}
				//如果白条也没有完成，那就立刻停止，然后重设
				else if (!_tween_WhiteSliderChangeValue.IsComplete())
				{
					_tween_WhiteSliderChangeValue.Kill();
					_tween_WhiteSliderChangeValue = slider_HealthSlider_WhiteBG
						.DOValue(partial, _WhiteSliderTweenDuration).SetDelay(_WhiteSliderTweenDelay);
				}
			}
		}

#endregion


#region Buff条

		private void _CheckIfNeedAddBuff(DS_ActionBusArguGroup ds)
		{
			I_BuffContentMayDisplayOnUI I_DisplayRef = ds.ObjectArgu1 as I_BuffContentMayDisplayOnUI;
			var content = ds.ObjectArgu2 as ConcreteBuffDisplayOnUIInfo;
			if (content.BuffUIType == BuffUITypeEnum.EnemyShowBuff_敌人需要显示的Buff)
			{
				var newObj = Instantiate(ParentRef.prefab_EnemyBuffIcon, layout_BuffHUD.transform);
				var newUIRW = newObj.GetComponent<UIRW_EnemyBuffHUDEntry_EnemyTopHUD>();
				_list_SelfBuffIconUIRWList.Add(newUIRW);
				newUIRW.SetTargetBuff(I_DisplayRef);
			}
		}

		private void _CheckIfRemoveBuff(DS_ActionBusArguGroup ds)
		{
			I_BuffContentMayDisplayOnUI I_DisplayRef = ds.ObjectArgu1 as I_BuffContentMayDisplayOnUI;
			var content = ds.ObjectArgu2 as ConcreteBuffDisplayOnUIInfo;
			var findI = _list_SelfBuffIconUIRWList.FindIndex((hud => hud.SelfTargetBuffInterfaceRef == I_DisplayRef));
			if (findI == -1)
			{
				return;
			}
			var targetUIRW = _list_SelfBuffIconUIRWList[findI];
			Destroy(targetUIRW.gameObject);

			_list_SelfBuffIconUIRWList.RemoveAt(findI);
		}

#endregion

#region 弱点条

		private void _ABC_CheckIfShowResidentWeaknessSlider(DS_ActionBusArguGroup ds)
		{
			var wGroup = ds.GetObj1AsT<Buff_通用敌人弱点_CommonEnemyWeakness.WeaknessInfoGroup>();
			if (wGroup!=null)
			{
				if (wGroup.AsResidentWeaknessGroup)
				{
					_residentWeaknessInfoGroupRef = wGroup;
					ActivateResidentWeaknessSlider();
				}
			}
		}


		/// <summary>
		/// <para>激活作为【常驻计量】 的计量条</para>
		/// </summary>
		private void ActivateResidentWeaknessSlider()
		{
			slider_WeaknessSlider.gameObject.SetActive(true);
			slider_WeaknessSlider.value = 1f - _residentWeaknessInfoGroupRef.CurrentAmount /
			                              _residentWeaknessInfoGroupRef.TriggerAmount;
		}


		private void DeactivateResidentWeaknessSlider()
		{
			slider_WeaknessSlider.gameObject.SetActive(false);
		}

		

#endregion

		public void UpdateTick(float ct, int cf, float delta)
		{
			if (!CurrentUIRWActive)
			{
				return;
			}


			
			if (_residentWeaknessInfoGroupRef != null)
			{
				var cw = _residentWeaknessInfoGroupRef.CurrentAmount;
				var maxW = _residentWeaknessInfoGroupRef.TriggerAmount;
				var partial = 1f -  cw / maxW;
				//不相同，则试图刷这个动画
				if (!Mathf.Approximately(_sliderTargetValue, partial))
				{
					if (_tween_weaknessSlider != null)
					{
						_tween_weaknessSlider.Kill();
					}
					_sliderTargetValue = partial;
					_tween_weaknessSlider = slider_WeaknessSlider.DOValue(_sliderTargetValue, 0.05f);
				}
			}
		}


#region 清理

		private void _ABC_ClearRelated(DS_ActionBusArguGroup ds)
		{
			CurrentUIRWActive = false;
			gameObject.SetActive(false);
			RelatedEnemyBehaviourRef = null;
			_residentWeaknessInfoGroupRef = null;
			_maxHPEntry = null;
			foreach (var perUIRW in _list_SelfBuffIconUIRWList)
			{
				Destroy(perUIRW.gameObject);
			}
			_list_SelfBuffIconUIRWList.Clear();
			var lab = RelatedEnemyBehaviourRef?.GetRelatedActionBus();
			lab?.RemoveAction(ActionBus_ActionTypeEnum.L_DataEntry_OnDataEntryValueChanged_数据项的值被改变,
				_ABC_UpdateSlider);
			lab?.RemoveAction(ActionBus_ActionTypeEnum.L_ARPGBehaviour_BehaviourDataInvalid_角色行为数据不再有效,
				_ABC_ClearRelated);
			lab?.RemoveAction(ActionBus_ActionTypeEnum.L_Weakness_NewWeaknessGroupAdd_新的弱点组被添加,
				_ABC_CheckIfShowResidentWeaknessSlider);
		}

#endregion








	}
}