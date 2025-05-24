using System;
using ARPG.Manager;
using Global;
using Global.ActionBus;
using Global.AssetLoad;
using Global.RuntimeRecord;
using Global.UIBase;
using RPGCore.DataEntry;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
namespace ARPG.UI.Panel.PlayerCharacter
{
    public class UISP_PCF_CharacterDetailInfoSubPanel : UI_UIBaseSubPanel
    {
        [SerializeField, Required, LabelText("Text_力量数值"),TitleGroup("配置")]
        private TextMeshProUGUI _text_MStrengthValueText;

        [SerializeField, Required, LabelText("Text_敏捷数值"), TitleGroup("配置")]
        private TextMeshProUGUI _text_MDexterityValueText;
        [SerializeField, Required, LabelText("Text_体质数值"), TitleGroup("配置")]
        private TextMeshProUGUI _text_MVitalityValueText;
        [SerializeField, Required, LabelText("Text_精神数值"), TitleGroup("配置")]
        private TextMeshProUGUI _text_MSpiritValueText;
        [SerializeField, Required, LabelText("Text_智慧数值"), TitleGroup("配置")]
        private TextMeshProUGUI _text_MIntellectValueText;
        [SerializeField, Required, LabelText("Text_魅力数值"), TitleGroup("配置")]
        private TextMeshProUGUI _text_MCharmValueText;
        [SerializeField, Required, LabelText("Text_背景故事文本"), TitleGroup("配置")]
        private TextMeshProUGUI _text_StoryText;


        private const string _LT_CharacterStoryTable = "CharacterStory_角色背景故事";


        private UIP_PlayerCharacterFullPanel _playerCharacterFullPanelRef;

        public override void StartInitializeBySP(UI_UIBasePanel parentUIP)
        {
            base.StartInitializeBySP(parentUIP);
            _playerCharacterFullPanelRef = parentUIP as UIP_PlayerCharacterFullPanel;
        

        }


        public override void ShowThisSubPanel()
        {
            base.ShowThisSubPanel();
            
            
            RefreshContent();
            GlobalActionBus.GetGlobalActionBus().RegisterAction(
                ActionBus_ActionTypeEnum.G_CharacterEquippedEquipmentChanged_角色装备发生变化,
                _ABC_RefreshContent_OnCharacterEquipmentChanged,
                20);
        }

        private void _ABC_RefreshContent_OnCharacterEquipmentChanged(DS_ActionBusArguGroup ds)
        {
            RefreshContent();
        }





        public void RefreshContent()
        {
            float _dex = 0f, _str=0f, _vit=0f, _spi=0f, _int=0f, _cha=0f;
            switch (GlobalConfigurationAssetHolderHelper.GetCurrentLoadPhase())
            {
                case GeneralGameAssetLoadPhaseEnum.WorldMap:
                case GeneralGameAssetLoadPhaseEnum.RegionMap:
                    var record = GlobalConfigurationAssetHolderHelper.GetGCAHH().RuntimeRecordHelperCharacter.GetCharacterRecord( _playerCharacterFullPanelRef.CurrentSelectCharacterID);
                    _dex = record.M_Dexterity;
                    _str = record.M_Strength;
                    _vit = record.M_Vitality;
                    _spi = record.M_Spirit;
                    _int = record.M_Intelligence;
                    _cha = record.M_Charming;
                    break;
                case GeneralGameAssetLoadPhaseEnum.ARPG:
                    var behaviour = SubGameplayLogicManager_ARPG.Instance.PlayerCharacterBehaviourControllerReference.GetBehaviourRef( _playerCharacterFullPanelRef.CurrentSelectCharacterID);
                    _dex = behaviour.GetFloatDataEntryByType(RP_DataEntry_EnumType.M_Dexterity_主敏捷).CurrentValue;
                    _str = behaviour.GetFloatDataEntryByType(RP_DataEntry_EnumType.M_Strength_主力量).CurrentValue;
                    _vit = behaviour.GetFloatDataEntryByType(RP_DataEntry_EnumType.M_Vitality_主体质).CurrentValue;
                    _spi = behaviour.GetFloatDataEntryByType(RP_DataEntry_EnumType.M_Spirit_主精神).CurrentValue;
                    _int = behaviour.GetFloatDataEntryByType(RP_DataEntry_EnumType.M_Intellect_主智力).CurrentValue;
                    _cha = behaviour.GetFloatDataEntryByType(RP_DataEntry_EnumType.M_Charm_主魅力).CurrentValue;
                    break;
            }
            
            
            
            _text_MDexterityValueText.text = ((int)_dex).ToString();
            _text_MStrengthValueText.text = ((int)_str).ToString();
            _text_MVitalityValueText.text = ((int)_vit).ToString();
            _text_MSpiritValueText.text = ((int)_spi).ToString();
            _text_MIntellectValueText.text = ((int) _int).ToString();
            _text_MCharmValueText.text = ((int)_cha).ToString();

            _text_StoryText.text = GlobalConfigurationAssetHolderHelper.GetCurrentBaseGRS()
                .GetLocalizedStringByTableAndKey(_LT_CharacterStoryTable,
                    _playerCharacterFullPanelRef.CurrentSelectCharacterID.ToString());
        }

        


    }
}
