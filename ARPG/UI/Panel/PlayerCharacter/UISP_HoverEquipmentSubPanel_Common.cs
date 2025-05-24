using System;
using System.Collections.Generic;
using System.Text;
using ARPG.Equipment;
using Global;
using Global.GlobalConfig;
using Global.Loot;
using Global.UI;
using Global.UIBase;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Serialization;
using UnityEngine.UI;
using static Global.Loot.SOFE_WeaponInfo;
#if UNITY_EDITOR
using static vietlabs.fr2.FR2_SplitView;
#endif
namespace ARPG.UI.Panel.PlayerCharacter
{
	public class UISP_HoverEquipmentSubPanel_Common : UI_UIBaseSubPanel
	{
		private StringBuilder _selfSB = new StringBuilder();

		public RectTransform SelfRectTransform { get; private set; }

		[SerializeField, Required, LabelText("layout-本体整体Layout"), TitleGroup("配置")]
		public VerticalLayoutGroup _layout_MainFull;

		[SerializeField, Required, LabelText("text_装备名字"), TitleGroup("配置")]
		private TextMeshProUGUI _text_EquipmentName;
		[SerializeField, Required, LabelText("text_装备强化标签"), TitleGroup("配置")]
		private TextMeshProUGUI _text_EquipmentStrengthenLabel;

		[SerializeField, Required, LabelText("text_穿戴中文本"), TitleGroup("配置")]
		private TextMeshProUGUI _text_EquippedText;
		
		[LabelText("text-出售价格文本"), SerializeField, Required, TitleGroup("配置")]
		private TextMeshProUGUI _text_SellPriceText;

		[SerializeField, Required, LabelText("text_品质和装备类型"), TitleGroup("配置")]
		private TextMeshProUGUI _text_EquipmentQualityAndTypeLabel;

		[SerializeField, Required, LabelText("text_装备槽位类型"), TitleGroup("配置")]
		private TextMeshProUGUI _text_EquipmentSlotTypeText;
		
		[LabelText("holder_武器元素图标底板 "), SerializeField, Required, TitleGroup("配置")]
		private Image _image_WeaponElementHolder;
		
		[LabelText("img_武器元素图标 "), SerializeField, Required, TitleGroup("配置")]
		private Image _image_WeaponElementIcon;

		[SerializeField, Required, LabelText("text_装备适用职业"), TitleGroup("配置")]
		private TextMeshProUGUI _text_EquipmentApplicableJobText;

		
		[LabelText("text_装备主要效果具体文本 "), SerializeField, Required, TitleGroup("配置")]
		private TextMeshProUGUI _text_EquipmentMainAddonText;
		
		[LabelText("VL_装备附加效果布局 "), SerializeField, Required, TitleGroup("配置")]
		private VerticalLayoutGroup _layout_EquipmentExtraAddon;
		
		[LabelText("text_装备附加效果具体文本 "), SerializeField, Required, TitleGroup("配置")]
		private TextMeshProUGUI _text_EquipmentExtraAddonText;
		[LabelText("text_装备附加效果——强化效果具体文本 "), SerializeField, Required, TitleGroup("配置")]
		private TextMeshProUGUI _text_EquipmentExtraAddon_StrengthenText;
		
		[LabelText("VL_装备Perk布局 "), SerializeField, Required, TitleGroup("配置")]
		private VerticalLayoutGroup _layout_EquipmentPerkLayout;
		
		
		[LabelText("text_装备Perk具体文本 "), SerializeField, Required, TitleGroup("配置")]
		private TextMeshProUGUI _text_EquipmentConcretePerkText;
		
		
		[LabelText("vl_套装属性布局 "), SerializeField, Required, TitleGroup("配置")] 
		private VerticalLayoutGroup _layout_SuitPropertyLayout;
		
		[SerializeField, Required, LabelText("text_套装名称文本"), TitleGroup("配置")]
		private TextMeshProUGUI _text_SuitNameText;

		[SerializeField, Required, LabelText("text_套装组件完成度文本"), TitleGroup("配置")]
		private TextMeshProUGUI _text_SuitComponentCompletionText;

		[SerializeField, Required, LabelText("text_套装具体效果文本"), TitleGroup("配置")]
		private TextMeshProUGUI _text_SuitEffectText;



		[SerializeField, Required, LabelText("text_额外描述文本"), TitleGroup("配置")]
		private TextMeshProUGUI _text_DescText;





		public override void StartInitializeBySP(UI_UIBasePanel parentUIP)
		{
			SelfRectTransform = GetComponent<RectTransform>();
			base.StartInitializeBySP(parentUIP);
			;
		}



#region 基本显示内容


		/// <summary>
		/// <para>重置悬停面板内容为默认，全部关闭全部销毁，为新的显示做准备</para>
		/// </summary>
		private void ResetContent()
		{
			//reset all _text_ to ""
			_text_EquipmentName.text = "";
			_text_EquipmentStrengthenLabel.text = "";
			_text_EquippedText.text = "";
			_text_SellPriceText.text = "";

			//关闭所有非必定情况，按需展开
			//包括出售价格、套装属性、套装名称、套装组件完成度、套装具体效果、附加效果、Perk组

			_text_SellPriceText.gameObject.SetActive(false);
			_layout_SuitPropertyLayout.gameObject.SetActive(false);
			_layout_EquipmentExtraAddon.gameObject.SetActive(false);
			_layout_EquipmentPerkLayout.gameObject.SetActive(false);
			_text_EquipmentExtraAddon_StrengthenText.gameObject.SetActive(false);
			_image_WeaponElementHolder.gameObject.SetActive(false);
		}



		public enum EquipmentDetailHoverShowType
		{
			None_未指定= 0,
			WeaponInInventory_仓库中武器 = 11,
			EquipmentInInventory_仓库中普通装备 = 12,
			EquippedWeapon_已装备武器 = 21,
			EquippedEquipment_已装备普通装备 = 22,
			
			WeaponInCraftBlueprint_打造蓝图武器 = 31,
			EquipmentInCraftBlueprint_打造蓝图普通装备 = 32,
			
			WeaponToSell_待出售武器 = 41,
			EquipmentToSell_待出售普通装备 = 42,
			
			
			WeaponToStrengthen_待强化武器 = 51,
			EquipmentToStrengthen_待强化普通装备 = 52,
		}

		[ShowInInspector, LabelText("当前显示类型"), ReadOnly, TitleGroup("===检视===")]
		public EquipmentDetailHoverShowType CurrentShowType { get; private set; }



		/// <summary>
		/// <para>为某个具体的装备填充其名字和</para>
		/// </summary>
		private void Common_FillNameAndDescOfConcreteWeapon(
			SOFE_WeaponInfo.PerTypeInfo info)
		{
			_text_EquipmentName.text = info.Name;
			_text_EquipmentName.color = Color.white;
			_text_DescText.text = GlobalConfigurationAssetHolderHelper.GetCurrentBaseGRS()
				.GetLocalizedStringByTableAndKey(LocalizationTableNameC._LCT_EquipmentExtraDesc,
                    info.Name.ToString());
		}


		/// <summary>
		/// 为存档中的一个武器或装备添加它的前缀
		/// </summary>
		private void Common_FillPrefixDescOfInfoInGCSO(GlobalConfigSO.PlayerEquipmentInfo info)
		{
			var fe_perk = GlobalConfigurationAssetHolderHelper.GetGCAHH().FE_EquipmentPerkPair;
			if(info.EIC_EquipmentPrefixInfo == null || string.IsNullOrEmpty(info.EIC_EquipmentPrefixInfo.PrefixName))
			{
				return;
			}
			string t = GlobalConfigurationAssetHolderHelper.GetCurrentBaseGRS().GetLocalizedStringByTableAndKey(LocalizationTableNameC._LC_PerkText,
				info.EIC_EquipmentPrefixInfo.PrefixName);
			var s = _text_EquipmentName.text;
			_text_EquipmentName.text = $"{t} {s}";
			
		}

		/// <summary>
		/// <para>为某个具体的装备填充其名字和描述文本</para>
		/// </summary>
		private void Common_FillNameAndDescOfConcreteNonWeaponEquipment(
            SOFE_ArmorInfo.PerTypeInfo info)
		{
			_text_EquipmentName.text = info.Name;
			_text_EquipmentName.color = Color.white;
			_text_DescText.text = GlobalConfigurationAssetHolderHelper.GetCurrentBaseGRS()
				.GetLocalizedStringByTableAndKey(LocalizationTableNameC._LCT_EquipmentExtraDesc,
                info.Name.ToString());
		}


		private void Common_SetNameColor(EquipmentBaseTemplateConfig info)
		{
            switch (info.QualityType)
			{
				case EquipmentQualityTypeEnum.None_未指定:
				case EquipmentQualityTypeEnum.White_白色:
					_text_EquipmentName.color = Color.white;
					break;
				case EquipmentQualityTypeEnum.Green_绿色:
					_text_EquipmentName.color = Color.green;
					break;
				case EquipmentQualityTypeEnum.Blue_蓝色:
					_text_EquipmentName.color = new Color(1f/255f,174f/255f,228f/255f);
					break;
				case EquipmentQualityTypeEnum.Purple_紫色:
					_text_EquipmentName.color = new Color(234f/255f, 74f/255f,  255f/255f);
					break;
				case EquipmentQualityTypeEnum.Gold_金色:
					_text_EquipmentName.color = new Color(252f / 255f, 157f / 255f, 0f);
					break;
				case EquipmentQualityTypeEnum.Red_红色:
					_text_EquipmentName.color = new Color(254f / 255f, 107f / 255f, 29f / 255f);
					break;
				case EquipmentQualityTypeEnum.Pink_粉色:
					_text_EquipmentName.color = new Color(202f / 255f, 124f / 255f, 183f / 255f);
					break;
			}
			
		}
		
		

		private void Common_FillContentFromNonWeaponEquipment(SOFE_ArmorInfo.PerTypeInfo info)
		{
			_text_EquipmentQualityAndTypeLabel.text =
				$"{(GlobalConfigurationAssetHolderHelper.GetCurrentBaseGRS().GetLocalizedStringByTableAndKey(LocalizationTableNameC._LCT_EquipmentCommonName, info.QualityType.ToString()))}"
				+ $" {GlobalConfigurationAssetHolderHelper.GetCurrentBaseGRS().GetLocalizedStringByTableAndKey(LocalizationTableNameC._LCT_EquipmentCommonName, info.SlotType.ToString())}"
				;
			_text_EquipmentSlotTypeText.gameObject.SetActive(true);
            _text_EquipmentSlotTypeText.gameObject.SetActive(false);
            _text_EquipmentSlotTypeText.text = GlobalConfigurationAssetHolderHelper.GetCurrentBaseGRS()
				.GetLocalizedStringByTableAndKey(LocalizationTableNameC._LCT_EquipmentCommonName,
				info.SlotType.ToString());
            var tt = info.MatchingJobType;
            string ss = "";
            foreach (var perjob in tt)
            {
                ss += GlobalConfigurationAssetHolderHelper.GetCurrentBaseGRS()
                    .GetLocalizedStringByTableAndKey(LocalizationTableNameC._LCT_CharacterBattleJobName, perjob.ToString());
            }
            _text_EquipmentApplicableJobText.text = ss;

            string text_MainProperty = "";
			//主要效果
			if (info.HPMax > float.Epsilon)
			{
				text_MainProperty += $"最大生命值 +{info.HPMax}\n";
			}

			if (info.SPMax > float.Epsilon)
			{
				text_MainProperty += $"最大SP +{info.SPMax}\n";
			}

			if (info.AttackPower > float.Epsilon)
			{
				text_MainProperty += $"攻击力 +{info.AttackPower}\n";
			}

			if (info.Defense > float.Epsilon)
			{
				text_MainProperty += $"防御力 +{info.Defense}\n";
			}

			if (info.AttackSpeed > float.Epsilon)
			{
				text_MainProperty += $"攻击速度 +{info.AttackSpeed}\n";
			}

			if (info.SkillAccelerate > float.Epsilon)
			{
				text_MainProperty += $"技能加速 +{info.SkillAccelerate}\n";
			}

			if (info.Toughness > float.Epsilon)
			{
				text_MainProperty += $"韧性 +{info.Toughness}\n";
			}


			if (info.CriticalRate > float.Epsilon)
			{
				text_MainProperty += $"暴击率 +{info.CriticalRate}%\n";
			}

			if (info.CriticalBonus > float.Epsilon)
			{
				text_MainProperty += $"爆伤加成 +{info.CriticalBonus * 100f}%\n";
			}

			if (info.Accuracy > float.Epsilon)
			{
				text_MainProperty += $"命中率 +{info.Accuracy}%\n";
			}

			if (info.Dodge > float.Epsilon)
			{
				text_MainProperty += $"闪避率 +{info.Dodge}%\n";
			}

			if (info.MoveSpeed > float.Epsilon)
			{
				text_MainProperty += $"移动速度 +{info.MoveSpeed}\n";
			}
			_text_EquipmentMainAddonText.text = text_MainProperty;
		}


		private void Common_FillContentFromWeapon(
			SOFE_WeaponInfo.PerTypeInfo info)
		{
			_text_EquipmentQualityAndTypeLabel.text =
				$"{(GlobalConfigurationAssetHolderHelper.GetCurrentBaseGRS().GetLocalizedStringByTableAndKey(LocalizationTableNameC._LCT_EquipmentCommonName, info.QualityType.ToString()))}" +
				$" {GlobalConfigurationAssetHolderHelper.GetCurrentBaseGRS().GetLocalizedStringByTableAndKey(LocalizationTableNameC._LCT_EquipmentCommonName, info.WeaponType.ToString())}";
			_text_EquipmentSlotTypeText.gameObject.SetActive(false);
			// _text_EquipmentSlotTypeText.text = new LocalizedString(LocalizationTableNameC._LCT_EquipmentCommonName,
			// 	info.SlotType.ToString()).GetLocalizedString();
			_image_WeaponElementHolder.gameObject.SetActive(true);
			_image_WeaponElementIcon.sprite = BaseUIManager.QuickGetDamageTypeIcon(info.DamageType);
			var tt =  info.MatchingJobType;
			string ss = "";
			foreach (var perjob in tt)
			{
				ss += GlobalConfigurationAssetHolderHelper.GetCurrentBaseGRS()
					.GetLocalizedStringByTableAndKey(LocalizationTableNameC._LCT_CharacterBattleJobName, perjob.ToString());
			}
			_text_EquipmentApplicableJobText.text = ss;

			string text_MainProperty = "";
			//主要效果
			if (info.HPMax > float.Epsilon)
			{
				text_MainProperty += $"最大生命值 +{info.HPMax}\n";
			}
			 
			if (info.SPMax > float.Epsilon)
			{
				text_MainProperty += $"最大SP +{info.SPMax}\n";
			}
			
			if (info.AttackPower > float.Epsilon)
			{
				text_MainProperty += $"攻击力 +{info.AttackPower}\n";
			}
			
			if (info.Defense > float.Epsilon)
			{
				text_MainProperty += $"防御力 +{info.Defense}\n";
			}
			
			if (info.AttackSpeed > float.Epsilon)
			{
				text_MainProperty += $"攻击速度 +{info.AttackSpeed}\n";
			}
			
			if (info.SkillAccelerate > float.Epsilon)
			{
				text_MainProperty += $"技能加速 +{info.SkillAccelerate}\n";
			}
			
			if (info.Toughness > float.Epsilon)
			{
				text_MainProperty += $"韧性 +{info.Toughness}\n";
			}
			 
			
			if (info.CriticalRate > float.Epsilon)
			{
				text_MainProperty += $"暴击率 +{info.CriticalRate}%\n";
			}
			
			if (info.CriticalBonus > float.Epsilon)
			{
				text_MainProperty += $"爆伤加成 +{info.CriticalBonus * 100f}%\n";
			}
			
			if (info.Accuracy > float.Epsilon)
			{
				text_MainProperty += $"命中率 +{info.Accuracy}%\n";
			}
			
			if (info.Dodge > float.Epsilon)
			{
				text_MainProperty += $"闪避率 +{info.Dodge}%\n";
			}
			
			if (info.MoveSpeed > float.Epsilon)
			{
				text_MainProperty += $"移动速度 +{info.MoveSpeed}\n";
			}
			_text_EquipmentMainAddonText.text = text_MainProperty;
			
			
		}


		/// <summary>
		/// 填充具体的二级属性
		/// </summary>
		private void Common_FillContentOfSecondProperty(GlobalConfigSO.PlayerEquipmentInfo equipped)
		{
			if (equipped.EIC_EquipmentSecondPropertyInfo == null)
			{
				return;
			}

			_selfSB.Clear();

			if (equipped.EIC_EquipmentSecondPropertyInfo.M_StrengthAddon > float.Epsilon)
			{
				_selfSB.Append($"力量 +{equipped.EIC_EquipmentSecondPropertyInfo.M_StrengthAddon}\n");
			}
			
			if (equipped.EIC_EquipmentSecondPropertyInfo.M_IntelligenceAddon > float.Epsilon)
			{
				_selfSB.Append($"智力 +{equipped.EIC_EquipmentSecondPropertyInfo.M_IntelligenceAddon}\n");
			}
			
			if (equipped.EIC_EquipmentSecondPropertyInfo.M_DexterityAddon > float.Epsilon)
			{
				_selfSB.Append($"敏捷 +{equipped.EIC_EquipmentSecondPropertyInfo.M_DexterityAddon}\n");
			}
			
			if (equipped.EIC_EquipmentSecondPropertyInfo.M_VitalityAddon > float.Epsilon)
			{
				_selfSB.Append($"体质 +{equipped.EIC_EquipmentSecondPropertyInfo.M_VitalityAddon}\n");
			}
			 
			if (equipped.EIC_EquipmentSecondPropertyInfo.M_SpiritAddon > float.Epsilon)
			{
				_selfSB.Append($"精神 +{equipped.EIC_EquipmentSecondPropertyInfo.M_SpiritAddon}\n");
			}

			_layout_EquipmentExtraAddon.gameObject.SetActive(true);
			_text_EquipmentExtraAddonText.text = _selfSB.ToString();
		}


		/// <summary>
		///<para>获取某件装备上现在有的perk</para>
		/// </summary>
		private void Common_FillContentOfObtainedPerk(GlobalConfigSO.PlayerEquipmentInfo e)
		{
			if (e.EIC_EquipmentBuffInfo == null || e.EIC_EquipmentBuffInfo.ConcretePerkInfos == null ||
			    e.EIC_EquipmentBuffInfo.ConcretePerkInfos.Count == 0)
			{
				return;
			}
			var fe_perk = GlobalConfigurationAssetHolderHelper.GetGCAHH().FE_EquipmentPerkPair;
			string ss = "";
			e.EIC_EquipmentBuffInfo.ConcretePerkInfos.GetPerkTextContent_Full(ref ss);
			
			if (!string.IsNullOrEmpty(ss))
			{
				_layout_EquipmentPerkLayout.gameObject.SetActive(true);
				_text_EquipmentConcretePerkText.text = ss;
			}
		}


		private void Common_FillCurrentStrengthen(GlobalConfigSO.PlayerEquipmentInfo e)
		{
			if (e.ItemStrengthenLevel > 0)
			{
				_text_EquipmentStrengthenLabel.gameObject.SetActive(true);
				_text_EquipmentStrengthenLabel.text = $"+ {e.ItemStrengthenLevel}";
			}
		}




        #endregion

        #region 具体显示内容



        /// <summary>
        /// <para> 显示为仓库内武器</para>
        /// </summary>
        public void ShowOnType_WeaponInInventory(GlobalConfigSO.PlayerEquipmentInfo info)
		{
			ResetContent();
			CurrentShowType = EquipmentDetailHoverShowType.WeaponInInventory_仓库中武器;
            var rawInfo = GCAHHExtend.GetEquipmentRawInfo(info.EquipmentUID) as SOFE_WeaponInfo.PerTypeInfo;
            Common_FillContentFromWeapon(rawInfo);
			Common_FillNameAndDescOfConcreteWeapon(rawInfo);
			Common_SetNameColor(rawInfo);
			Common_FillContentOfSecondProperty(info);
			Common_FillContentOfObtainedPerk(info);
			Common_FillCurrentStrengthen(info);
			Common_FillPrefixDescOfInfoInGCSO(info);
			ShowThisSubPanel();
		}
		
		
		/// <summary>
		///  <para>显示为 仓库内装备</para>
		/// </summary>
		public void ShowOnType_EquipmentInInventory(GlobalConfigSO.PlayerEquipmentInfo info)
		{
			ResetContent();
			CurrentShowType = EquipmentDetailHoverShowType.EquipmentInInventory_仓库中普通装备;
            var rawInfo = GCAHHExtend.GetEquipmentRawInfo(info.EquipmentUID) as SOFE_ArmorInfo.PerTypeInfo;
            Common_FillContentFromNonWeaponEquipment(rawInfo);
			Common_FillNameAndDescOfConcreteNonWeaponEquipment(rawInfo);
			Common_SetNameColor(rawInfo);
			Common_FillContentOfSecondProperty(info);
			Common_FillContentOfObtainedPerk(info);
			Common_FillCurrentStrengthen(info);
			Common_FillPrefixDescOfInfoInGCSO(info);
			ShowThisSubPanel();
		}


		/// <summary>
		/// <para>将要出售的仓库中装备</para>
		/// </summary>
		public void ShowOnType_EquipmentInInventoryToSell(GlobalConfigSO.PlayerEquipmentInfo concreteEquipment)
		{
			//ShowOnType_EquipmentInInventory(concreteEquipment);
			//var templateGet = GlobalConfigurationAssetHolderHelper.GetGCAHH().FE_ConcreteNonWeaponEquipment.GetTemplateInfoByUID(concreteEquipment.EquipmentUID);
			//string s = "";
			//templateGet.SellPrice.PriceToString(ref s);
			//_text_SellPriceText.text = $"出售价格 {s}";
			//Common_FillCurrentStrengthen(concreteEquipment);
			//Common_SetNameColor(concreteEquipment);
			//Common_FillPrefixDescOfInfoInGCSO(concreteEquipment);
			//ShowThisSubPanel();
		}
		
		public void ShowOnType_WeaponInInventoryToSell(GlobalConfigSO.PlayerEquipmentInfo concreteWeapon)
		{
			//ShowOnType_WeaponInInventory(concreteWeapon);
			//var templateGet = GlobalConfigurationAssetHolderHelper.GetGCAHH().FE_ConcreteWeapon.GetTemplateInfoByUID(concreteWeapon.EquipmentUID);
			//string s = "";
			//templateGet.SellPrice.PriceToString(ref s);
			//_text_SellPriceText.text = $"出售价格 {s}";
			//Common_FillCurrentStrengthen(concreteWeapon);
			//Common_SetNameColor(concreteWeapon);
			//Common_FillPrefixDescOfInfoInGCSO(concreteWeapon);
			//ShowThisSubPanel();
		}



        /// <summary>
        /// <para> 显示为 装备着的武器</para>
        /// </summary>
        public void ShowOnType_EquippedWeapon(GlobalConfigSO.PlayerEquipmentInfo info)
		{
            ResetContent();
            CurrentShowType = EquipmentDetailHoverShowType.EquippedWeapon_已装备武器;
			_text_EquippedText.text = "穿戴中";
			var rawInfo = GCAHHExtend.GetEquipmentRawInfo(info.EquipmentUID) as SOFE_WeaponInfo.PerTypeInfo;
            Common_FillContentFromWeapon(rawInfo);
            Common_FillNameAndDescOfConcreteWeapon(rawInfo);
            Common_SetNameColor(rawInfo);
            Common_FillContentOfSecondProperty(info);
            Common_FillContentOfObtainedPerk(info);
            Common_FillCurrentStrengthen(info);
            Common_FillPrefixDescOfInfoInGCSO(info);
            ShowThisSubPanel();
        }



		/// <summary>
		/// <para> 显示为 装备着的普通装备</para>
		/// </summary>
		public void ShowOnType_EquippedEquipment(GlobalConfigSO.PlayerEquipmentInfo info)
		{
			ResetContent();
			CurrentShowType = EquipmentDetailHoverShowType.EquippedEquipment_已装备普通装备;
            _text_EquippedText.text = "穿戴中";
            var rawInfo = GCAHHExtend.GetEquipmentRawInfo(info.EquipmentUID) as SOFE_ArmorInfo.PerTypeInfo;
			Common_FillContentFromNonWeaponEquipment(rawInfo);
			Common_FillNameAndDescOfConcreteNonWeaponEquipment(rawInfo);
			Common_SetNameColor(rawInfo);
			Common_FillContentOfSecondProperty(info);
			Common_FillContentOfObtainedPerk(info);
			Common_FillCurrentStrengthen(info);
			Common_FillPrefixDescOfInfoInGCSO(info);
			ShowThisSubPanel();
		}
		
		
		
		/// <summary>
		/// <para>蓝图那个是随便拿的，因为也不需要显示属性</para>
		/// </summary>
		/// <param name="typeByName"></param>
		public void ShowOnType_WeaponInCraftBlueprint(SOFE_WeaponInfo.PerTypeInfo info)
		{
			ResetContent();

            CurrentShowType = EquipmentDetailHoverShowType.WeaponInCraftBlueprint_打造蓝图武器;
            _text_EquipmentName.text = info.Name;
            _text_EquipmentQualityAndTypeLabel.text = $"随机品质" +
                $" {GlobalConfigurationAssetHolderHelper.GetCurrentBaseGRS().GetLocalizedStringByTableAndKey(LocalizationTableNameC._LCT_EquipmentCommonName, info.WeaponType.ToString())}";
            _text_EquipmentSlotTypeText.gameObject.SetActive(false);
            _image_WeaponElementHolder.gameObject.SetActive(true);
            _image_WeaponElementIcon.sprite = BaseUIManager.QuickGetDamageTypeIcon(info.DamageType);
            var tt = info.MatchingJobType;
            string ss = "";
			foreach (var perjob in tt)
			{
				ss += GlobalConfigurationAssetHolderHelper.GetCurrentBaseGRS()
					.GetLocalizedStringByTableAndKey(LocalizationTableNameC._LCT_CharacterBattleJobName, perjob.ToString());
			}
            _text_EquipmentApplicableJobText.text = ss;

            ShowThisSubPanel();
		}


		/// <summary>
		/// <para>蓝图那个是随便拿的，因为也不需要显示属性</para>
		/// </summary>
		/// <param name="typeByName"></param>
		public void ShowOnType_NonWeaponEquipmentInCraftBlueprint(SOFE_ArmorInfo.PerTypeInfo info)
		{
			ResetContent();

            CurrentShowType = EquipmentDetailHoverShowType.EquipmentInCraftBlueprint_打造蓝图普通装备;
            _text_EquipmentName.text = info.Name;
            _text_EquipmentQualityAndTypeLabel.text = $"随机品质" +
                $" {GlobalConfigurationAssetHolderHelper.GetCurrentBaseGRS().GetLocalizedStringByTableAndKey(LocalizationTableNameC._LCT_EquipmentCommonName, info.SlotType.ToString())}";
            _text_EquipmentSlotTypeText.gameObject.SetActive(true);
            _text_EquipmentSlotTypeText.text = GlobalConfigurationAssetHolderHelper.GetCurrentBaseGRS()
                .GetLocalizedStringByTableAndKey(LocalizationTableNameC._LCT_EquipmentCommonName,
                info.SlotType.ToString());
            _image_WeaponElementHolder.gameObject.SetActive(false);
            var tt = info.MatchingJobType;
            string ss = "";
            foreach (var perjob in tt)
            {
                ss += GlobalConfigurationAssetHolderHelper.GetCurrentBaseGRS()
                    .GetLocalizedStringByTableAndKey(LocalizationTableNameC._LCT_CharacterBattleJobName, perjob.ToString());
            }
            _text_EquipmentApplicableJobText.text = ss;

            ShowThisSubPanel();
		}






		public void ShowOnType_WeaponToStrengthen(GlobalConfigSO.PlayerEquipmentInfo equipmentInfo)
		{

			ShowOnType_WeaponInInventory(equipmentInfo);
			CurrentShowType = EquipmentDetailHoverShowType.WeaponToStrengthen_待强化武器;

			var currentLevelContent = GlobalConfigurationAssetHolderHelper.GetGCAHH().FE_EquipmentStrengthenInfo
				.GetWeaponInfoByLevel(equipmentInfo.ItemStrengthenLevel);
			_text_EquipmentExtraAddon_StrengthenText.gameObject.SetActive(true);
			float from = currentLevelContent == null ? 0f : currentLevelContent.AttackPowerBonus;
			var nextLevelCotnent = GlobalConfigurationAssetHolderHelper.GetGCAHH().FE_EquipmentStrengthenInfo
				.GetWeaponInfoByLevel(equipmentInfo.ItemStrengthenLevel + 1);
			float to = nextLevelCotnent == null ? 0f : nextLevelCotnent.AttackPowerBonus;
			_text_EquipmentExtraAddon_StrengthenText.text = $"攻击力: +{from} → 攻击力: +{to}";
			


		}

		public void ShowOnType_EquipmentToStrengthen(GlobalConfigSO.PlayerEquipmentInfo equipmentInfo)
		{

			ShowOnType_EquipmentInInventory(equipmentInfo);
			CurrentShowType = EquipmentDetailHoverShowType.EquipmentToStrengthen_待强化普通装备;

			var currentLevelContent = GlobalConfigurationAssetHolderHelper.GetGCAHH().FE_EquipmentStrengthenInfo
				.GetNonWeaponEquipmentInfoByLevel(equipmentInfo.ItemStrengthenLevel);
			_text_EquipmentExtraAddon_StrengthenText.gameObject.SetActive(true);
			float from = currentLevelContent == null ? 0f : currentLevelContent.DefensePowerBonus;
			var nextLevelCotnent = GlobalConfigurationAssetHolderHelper.GetGCAHH().FE_EquipmentStrengthenInfo
				.GetNonWeaponEquipmentInfoByLevel(equipmentInfo.ItemStrengthenLevel + 1);
			float to = nextLevelCotnent == null ? 0f : nextLevelCotnent.DefensePowerBonus;
			_text_EquipmentExtraAddon_StrengthenText.text = $"防御力: +{from} → 防御力: +{to}";
			
		}




#endregion
		
		
		

	}
}