using System;
using Global;
using Global.Character;
using Global.UIBase;
using RPGCore.Skill;
using RPGCore.Skill.Config;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
namespace ARPG.UI.Panel.PlayerCharacter
{
	public class UISP_PCF_HoverSkillSubPanel : UI_UIBaseSubPanel
	{
		public RectTransform SelfRectTransform { get; private set; }

		[SerializeField, Required, LabelText("text_技能按键"), TitleGroup("配置")]
		private TextMeshProUGUI _text_SkillInputButton;

		[SerializeField, Required, LabelText("text_技能名字"), TitleGroup("配置")]
		private TextMeshProUGUI _text_SkillName;

		[SerializeField, Required, LabelText("text_技能类型(位移/大)"), TitleGroup("配置")]
		private TextMeshProUGUI _text_SkillSlotType;

		[SerializeField, Required, LabelText("text_技能品质"), TitleGroup("配置")]
		private TextMeshProUGUI _text_SkillQuality;

		[SerializeField, Required, LabelText("text_技能性质"), TitleGroup("配置")]
		private TextMeshProUGUI _text_SkillProperty;

		[SerializeField, Required, LabelText("text_技能CD时长"), TitleGroup("配置")]
		private TextMeshProUGUI _text_SkillCD;

		[SerializeField, Required, LabelText("text_技能消耗"), TitleGroup("配置")]
		private TextMeshProUGUI _text_SkillConsume;

		[SerializeField, Required, LabelText("text_技能详情"), TitleGroup("配置")]
		private TextMeshProUGUI _text_SkillDetail;


		[LabelText("vl_技能强化布局"), SerializeField, Required, TitleGroup("配置")]
		private VerticalLayoutGroup _vl_SkillEnhanceLayout;

		[LabelText("text_具体技能强化文本"), SerializeField, Required, TitleGroup("配置")]
		private TextMeshProUGUI _text_SkillEnhanceDetail;


		[SerializeField, Required, LabelText("text_技能额外描述"), TitleGroup("配置")]
		private TextMeshProUGUI _text_SkillExtraDesc;







		public override void StartInitializeBySP(UI_UIBasePanel parentUIP)
		{
			SelfRectTransform = GetComponent<RectTransform>();
			base.StartInitializeBySP(parentUIP);
		}

		public override void ShowThisSubPanel()
		{
			base.ShowThisSubPanel();
		}

		public override void HideThisSubPanel()
		{
			base.HideThisSubPanel();
		}


		private void ResetContent()
		{
		}



		


		public void RefreshDisplayContent(
			SkillSlotTypeEnum skillSlot, RPSkill_SkillTypeEnum skillType,
			SOConfig_RPSkill config)
		{
			_vl_SkillEnhanceLayout.gameObject.SetActive(false);
			switch (skillSlot)
			{
				case SkillSlotTypeEnum.SlotNormal1_常规槽位1:
					_text_SkillInputButton.text = "[Q]  ";
					break;
				case SkillSlotTypeEnum.SlotNormal2_常规槽位2:
					_text_SkillInputButton.text = "[E]  ";
					break;
				case SkillSlotTypeEnum.DisplacementSkill_位移槽位:
					_text_SkillInputButton.text = "[SHIFT]  ";
					break;
				case SkillSlotTypeEnum.UltraSkill_超杀槽位:
					_text_SkillInputButton.text = "[R]  ";
					break;
				case SkillSlotTypeEnum.ChainSkill_连协槽位:
					_text_SkillInputButton.text = "[入]  ";
					break;
			}

			LocalizedString str_skillName = new LocalizedString(LocalizationTableNameC._LCT_SkillName,
				skillType.ToString());
			_text_SkillName.text = str_skillName.GetLocalizedString();

			switch (skillSlot)
			{
				case SkillSlotTypeEnum.SlotNormal1_常规槽位1:
				case SkillSlotTypeEnum.SlotNormal2_常规槽位2:
					_text_SkillSlotType.text = "普通技能 ";
					break;
				case SkillSlotTypeEnum.DisplacementSkill_位移槽位:
					_text_SkillSlotType.text = "位移技能 ";
					break;
				case SkillSlotTypeEnum.UltraSkill_超杀槽位:
					_text_SkillSlotType.text = "超杀技能 ";
					break;
				case SkillSlotTypeEnum.ChainSkill_连协槽位:
					_text_SkillSlotType.text = "连携技能";
					break;
			}

			_text_SkillQuality.text = " 大师级 ";


			var ts = "";
			// foreach (SkillPropertyTypeEnumFlag perType in config.ContentInSO.PropertyTypeFlags)
			// {
			// 	ts += new LocalizedString(LocalizationTableNameC._LCT_SkillPropertyTag, perType.ToString())
			// 		.GetLocalizedString();
			// }
			_text_SkillProperty.text = ts;
			

			//当config为空的时候，通常表示这是一个普攻，所以不用显示CD
			if (config != null)
			{
				_text_SkillCD.text = $"{config.ConcreteSkillFunction.CoolDownTime.ToString("F0")} 秒";
				_text_SkillConsume.text = $"{config.ConcreteSkillFunction.SPConsume.ToString("F0")} SP";
			}
			_text_SkillDetail.text = new LocalizedString(LocalizationTableNameC._LCT_SkillDetail,
				skillType.ToString()).GetLocalizedString();


			_text_SkillExtraDesc.text = new LocalizedString(LocalizationTableNameC._LCT_SkillExtraDesc,
				skillType.ToString()).GetLocalizedString();


			//查找技能强化
		}


	}
}