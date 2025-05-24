using ARPG.Character;
using ARPG.Character.Config;
using ARPG.Character.Player;
using Global;
using Global.ActionBus;
using Global.Character;
using Global.UI;
using Global.UIBase;
using RPGCore.Buff.ConcreteBuff;
using RPGCore.Buff.ConcreteBuff.Utility;
using RPGCore.DataEntry;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using UnityEngine.UI;
namespace ARPG.UI.Panel
{
	public class UIRW_CharacterInCandidate : UI_UISingleRuntimeWidget
	{
		[SerializeField, LabelText("Button_更换角色"), Required]
		private Button _button_ChangeCharacter;

		[SerializeField, LabelText("Image_角色头像"), Required]
		private Image _image_CharacterHead;

		[SerializeField, LabelText("Text_角色名字"), Required]
		private TextMeshProUGUI _text_CharacterName;
		
		[SerializeField, LabelText("Text_角色候选位置"), Required]
		private TextMeshProUGUI _text_CharacterCandidatePosition;

		[SerializeField, LabelText("Slider_HP滑条"), Required]
		private Slider _slider_HPSlider;
		
		[SerializeField,LabelText("Image_超能图标"),Required]
		private Image _image_SuperPowerIcon;

		[SerializeField, LabelText("Image_遮罩 _可用性"), Required]
		private Image _image_Mask_SwitchCD;
		
		
		
		[ShowInInspector,LabelText("关联角色")]
		public PlayerARPGConcreteCharacterBehaviour RelatedBehaviour { get; protected set; }
		
		/// <summary>
		/// 当前正在操控的那个角色
		/// </summary>
		public PlayerARPGConcreteCharacterBehaviour CurrentControllingBehaviour { get; private set; }

		private Buff_PlayerSwitchCharacterBlock _buff_PlayerSwitchCharacterBlockRef;



		public void Initialize(PlayerARPGConcreteCharacterBehaviour player, PlayerARPGConcreteCharacterBehaviour active, int index)
		{
			RelatedBehaviour = player;
			CurrentControllingBehaviour = active;

			var relatedAssetEntry = GlobalConfigurationAssetHolderHelper.GetGCAHH().FE_CharacterResourceInfo
				.GetConfigByType(player.SelfBehaviourNamedType);

			_image_CharacterHead.sprite = relatedAssetEntry.GetIcon_Inner();
			_text_CharacterName.text = GlobalConfigurationAssetHolderHelper.GetCurrentBaseGRS()
				.GetLocalizedStringByTableAndKey(LocalizationTableNameC._LCT_CharacterName,
					relatedAssetEntry.CharacterName);
			_text_CharacterCandidatePosition.text = index.ToString();
			
			var _entry_HP = player.GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentHP_当前HP);
			var _entry_MaxHP = player.GetFloatDataEntryByType(RP_DataEntry_EnumType.HPMax_最大HP);
			_slider_HPSlider.value = _entry_HP.CurrentValue / _entry_MaxHP.CurrentValue;


			_image_SuperPowerIcon.sprite = BaseUIManager.QuickGetDamageTypeIcon(
				(RelatedBehaviour.ReceiveBuff_GetTargetBuff(RolePlay_BuffTypeEnum.ChangeCommonDamageType_常规伤害类型更改) as
					Buff_ChangeCommonDamageType).CurrentDamageType);
			_buff_PlayerSwitchCharacterBlockRef = player.ReceiveBuff_GetTargetBuff(RolePlay_BuffTypeEnum.ARPG_PlayerSwitchCharacterBlock_玩家更换角色阻塞) as
					Buff_PlayerSwitchCharacterBlock;

			//如果已经不再active，那就是已经退场了
			if (RelatedBehaviour.CurrentState == PlayerARPGConcreteCharacterBehaviour.PlayerCharacterStateTypeEnum.ExitBattle_已退场)
			{
				_image_Mask_SwitchCD.fillAmount = 1f;
				_lastCounting = false;
			}
			else
			{
				_lastCounting = true;
			}
			
		}


		private bool _lastCounting;

		public void UpdateTick(float ct, int cf, float delta)
		{
			if (_lastCounting)
			{
				float fa = _buff_PlayerSwitchCharacterBlockRef.BuffRemainingAvailableTime /
				           _buff_PlayerSwitchCharacterBlockRef.CurrentSingleBlockDuration;

				if (fa <= 0f)
				{
					_lastCounting = false;
					var ds_vfxGeneral = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
						.G_UI_General_RequireGeneralInteractVFX_UI要求通用交互VFX);
					ds_vfxGeneral.ObjectArgu1 = transform;
					ds_vfxGeneral.IntArgu2 = 1;
					GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_vfxGeneral);
				}
				else
				{
					_image_Mask_SwitchCD.fillAmount = fa;
				}

			}
		}


	}
}