using System;
using ARPG.Manager;
using DG.Tweening;
using Global;
using Global.ActionBus;
using Global.Character;
using Global.UI;
using Global.UIBase;
using RPGCore.Buff;
using RPGCore.Buff.ConcreteBuff;
using RPGCore.Skill;
using RPGCore.Skill.Config;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ARPG.UI.Panel
{
	public class UIRW_PerPlayerSkillEntry : UI_UISingleRuntimeWidget
	{

		[SerializeField, LabelText("当前技能的槽位"), Required]
		private SkillSlotTypeEnum _selfSkillSlotType;

		public SkillSlotTypeEnum SelfSkillSlotType => _selfSkillSlotType;

		[SerializeField, Required, LabelText("Image_技能外框"), FoldoutGroup("配置", true)]
		private Image _image_SkillBorderImage;
		[SerializeField, Required, LabelText("技能图标"), FoldoutGroup("配置", true)]
		private Image _image_SkillIconImage;

		[SerializeField, Required, LabelText("Image_CD覆盖"), FoldoutGroup("配置", true)]
		private Image _image_CDOverlayImage;


		[SerializeField, Required, LabelText("text_技能按键文本"), FoldoutGroup("配置", true)]
		private TextMeshProUGUI _text_InputTextLabel;


		[ShowInInspector, LabelText("关联的技能运行时实例"), FoldoutGroup("运行时", true)]
		private SOConfig_RPSkill _selfRelatedSkillConfigRuntime;


		private static UIP_PlayerStatePanel _playerSkillPanelRef;
		private static UIManager_ARPG _managerArpgRef;
		public static void StaticInitialize(UIP_PlayerStatePanel psp)
		{
			_managerArpgRef = UIManager_ARPG.Instance;

			_playerSkillPanelRef = psp;
		}

		public void InitializeOnInstantiate(SOConfig_RPSkill skill)
		{
			if (skill == null)
			{
				gameObject.SetActive(false);
				return;
			}
			gameObject.SetActive(true);

			_image_CDOverlayImage.gameObject.SetActive(false);
			// _image_CDOutGlowImage.gameObject.SetActive(false);




			_selfRelatedSkillConfigRuntime = skill;
			_image_SkillIconImage.sprite = skill.ConcreteSkillFunction.GetCurrentSprite();

			RefreshSkillIcon();

			_image_CDOverlayImage.gameObject.SetActive(false);
			UpdateInputText();
			// _image_CDOutGlowImage.gameObject.SetActive(false);
		}


		public void RefreshSkillIcon()
		{
			// string content = "";
			// switch (_selfRelatedSkillConfigRuntime.ConcreteSkillFunction.SkillSlot)
			// {
			// 	case SkillSlotTypeEnum.DisplacementSkill_位移槽位:
			// 		content = "SHIFT";
			// 		break;
			//
			// 	case SkillSlotTypeEnum.SlotNormal1_常规槽位1:
			// 		content = "Q";
			// 		break;
			//
			// 	case SkillSlotTypeEnum.SlotNormal2_常规槽位2:
			// 		content = "E";
			// 		break;
			//
			// 	case SkillSlotTypeEnum.UltraSkill_超杀槽位:
			// 		content = "R";
			// 		break;
			// }
			// _text_InputTextLabel.text = content;



			if (_selfRelatedSkillConfigRuntime == null || _selfRelatedSkillConfigRuntime.ConcreteSkillFunction == null)
			{
				return;
			}
			Buff_ChangeCommonDamageType tt =
				_selfRelatedSkillConfigRuntime.ConcreteSkillFunction.RelatedRPSkillCaster.ReleaseSkill_GetRelatedBuff(
					RolePlay_BuffTypeEnum.ChangeCommonDamageType_常规伤害类型更改) as Buff_ChangeCommonDamageType;
			_image_SkillBorderImage.sprite = BaseUIManager.QuickGetSkillBorder(tt.CurrentDamageType);
			_image_SkillIconImage.sprite = _selfRelatedSkillConfigRuntime.ConcreteSkillFunction.GetCurrentSprite();
		}





		/// <summary>
		/// <para>刷新输入上的文本。常见于初始化时，以及更换了输入设备、输入风格时</para>
		/// </summary>
		public void UpdateInputText()
		{
			bool pureKeyboard = GlobalConfigurationAssetHolderHelper.GetGCAHH().GlobalConfigSO_Runtime.Content
				.UsePureKeyboard;
			
			
			switch (_selfRelatedSkillConfigRuntime.ConcreteSkillFunction.SkillSlot)	
			{
				case SkillSlotTypeEnum.SlotNormal1_常规槽位1:
					_text_InputTextLabel.text = pureKeyboard ? "K" : "Q";
					break;
				case SkillSlotTypeEnum.SlotNormal2_常规槽位2:
					_text_InputTextLabel.text = pureKeyboard ? "L" : "E";
					 
					break;
				case SkillSlotTypeEnum.DisplacementSkill_位移槽位:
					_text_InputTextLabel.text = "SPACE";
					break;
				case SkillSlotTypeEnum.UltraSkill_超杀槽位:
					_text_InputTextLabel.text = pureKeyboard ? "O" : "R";
					break;
			}
			
			
		}

		/// <summary>
		/// <para>之前在计CD吗</para>
		/// </summary>
		private bool _lastCountingCD = false;

		public void UpdateTick(float ct, int cf, float delta)
		{
			if (!gameObject.activeSelf)
			{
				return;
			}

			float remainingCDPartial = _selfRelatedSkillConfigRuntime.ConcreteSkillFunction.RemainingCoolDownDuration /
			                           _selfRelatedSkillConfigRuntime.ConcreteSkillFunction.CurrentCoolDownDuration;


			if (remainingCDPartial <= 0f)
			{
				if (_lastCountingCD)
				{
					_lastCountingCD = false;
					// _image_CDOutGlowImage.gameObject.SetActive(true);
					// _image_CDOutGlowImage.color = Color.white;
					// _image_CDOutGlowImage.DOColor(Color.clear, 0.85f)
					// 	.OnComplete((() => _image_CDOverlayImage.gameObject.SetActive(false)));


					var _ds_generalVFX = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
						.G_UI_General_RequireGeneralInteractVFX_UI要求通用交互VFX);
					_ds_generalVFX.ObjectArgu1 = transform;
					_ds_generalVFX.IntArgu2 = 1;
					GlobalActionBus.GetGlobalActionBus().TriggerActionByType(_ds_generalVFX);
					_image_SkillIconImage.color = Color.white;
				}
				_image_SkillIconImage.color = Color.white;

				SkillReadyTypeEnum state = _selfRelatedSkillConfigRuntime.ConcreteSkillFunction.GetSkillReadyType();
				if (state != SkillReadyTypeEnum.Ready)
				{
					_image_SkillIconImage.color = Color.gray;

					remainingCDPartial = 1f;
				}
			}
			else if (remainingCDPartial > 0.01f)
			{
				_lastCountingCD = true;
				_image_SkillIconImage.color = Color.gray;
				if (!_image_CDOverlayImage.isActiveAndEnabled)
				{
					_image_CDOverlayImage.gameObject.SetActive(true);
				}
			}

			_image_CDOverlayImage.fillAmount = remainingCDPartial;

			//
			// if (remainingCDPartial > 0.01f)
			// {
			// 	_selfSkillReadyImage.SetActive(false);
			// }
			// else
			// {
			// 	_selfSkillReadyImage.SetActive(true);
			// }
			//
			// _selfSkillCDCoverImage.fillAmount = remainingCDPartial;
		}
	}
}