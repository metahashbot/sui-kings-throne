using System;
using System.Collections.Generic;
using ARPG.Character;
using ARPG.Character.Base;
using ARPG.Character.Base.CustomSpineData;
using ARPG.Character.Config;
using ARPG.Character.Player;
using ARPG.Equipment;
using ARPG.Manager;
using Global;
using Global.ActionBus;
using Global.AssetLoad;
using Global.Character;
using Global.GlobalConfig;
using Global.Loot;
using Global.RuntimeRecord;
using Global.UI;
using Global.UIBase;
using RPGCore.Buff.ConcreteBuff;
using RPGCore.DataEntry;
using RPGCore.Skill;
using RPGCore.Skill.Config;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace ARPG.UI.Panel.PlayerCharacter
{
    public class UISP_PCF_CurrentCharacterStatePanel : UI_UIBaseSubPanel
    {
        private UIP_PlayerCharacterFullPanel _playerCharacterFullPanelRef;



#region 当前装备

        private UnityAction<GlobalConfigSO.PlayerEquipmentInfo> _callback_click;

        private UnityAction<GlobalConfigSO.PlayerEquipmentInfo, BaseEventData> _callback_HoverEnter;

        private UnityAction _callback_hoverExit;

        public void InjectCallback_CurrentWeapon(
            UnityAction<GlobalConfigSO.PlayerEquipmentInfo> click,
            UnityAction<GlobalConfigSO.PlayerEquipmentInfo, BaseEventData> hoverEnter,
            UnityAction hoverExit)
        {
            _callback_click = click;
            _callback_HoverEnter = hoverEnter;
            _callback_hoverExit = hoverExit;
        }


        [SerializeField,LabelText("当前所有已装备的槽位UIRW 列表"),TitleGroup("===Widget===")]
        private List<UIRW_EquipmentEquippedEntry_PlayerCharacterFullPanel> CurrentAllEquipmentUIRWList =
            new List<UIRW_EquipmentEquippedEntry_PlayerCharacterFullPanel>();



        public void RefreshAllEquipment()
        {
            foreach (UIRW_EquipmentEquippedEntry_PlayerCharacterFullPanel perUIRW in CurrentAllEquipmentUIRWList)
            {
                perUIRW.RefreshRelatedEquipmentInfo();
            }
        }




#region 卡牌


        /// <summary>
        /// <para>在ARPG环境下是打不开的</para>
        /// </summary>
        [LabelText("button-打开卡牌按钮"), SerializeField, Required, TitleGroup("===Widget===")]
        private Button _button_OpenCardGroupButton;
        
        
        [LabelText("text-当前战斗力文本"), SerializeField, Required, TitleGroup("===Widget===")]
        private TextMeshProUGUI _text_CurrentBattlePowerText;


        /// <summary>
        /// <para>刷新当前战斗力</para>
        /// </summary>
        public void RefreshCurrentBattleLevel()
        {
            
        }
        
        
        
        

#endregion

#endregion



#region 中间部分的数值




        [SerializeField, Required, LabelText("text_攻击力数值"), TitleGroup("===Widget===/属性")]
        private TextMeshProUGUI _text_AttackText;
        [SerializeField, Required, LabelText("text_防御力数值"), TitleGroup("===Widget===/属性")]
        private TextMeshProUGUI _text_DefenseText;
        [SerializeField, Required, LabelText("text_韧性数值"), TitleGroup("===Widget===/属性")]
        private TextMeshProUGUI _text_ToughnessText;
        [SerializeField, Required, LabelText("text_攻击速度数值"), TitleGroup("===Widget===/属性")]
        private TextMeshProUGUI _text_AttackSpeedText;
        [SerializeField, Required, LabelText("text_技能急速数值"), TitleGroup("===Widget===/属性")]
        private TextMeshProUGUI _text_SkillSpeedText;
        [SerializeField, Required, LabelText("text_暴击率数值"), TitleGroup("===Widget===/属性")]
        private TextMeshProUGUI _text_CriticalRateText;
        [SerializeField, Required, LabelText("text_暴击伤害数值"), TitleGroup("===Widget===/属性")]
        private TextMeshProUGUI _text_CriticalDamageText;
        [SerializeField, Required, LabelText("text_命中加成数值"), TitleGroup("===Widget===/属性")]
        private TextMeshProUGUI _text_AccuracyText;
        [SerializeField, Required, LabelText("text_闪避加成数值"), TitleGroup("===Widget===/属性")]
        private TextMeshProUGUI _text_DodgeText;


#endregion



#region 下方的技能

        
        
        [SerializeField,LabelText("六个技能槽位") ,Required,TitleGroup("===Widget===/技能")]
        private List<UIFW_SkillEntryToHover_Common> _currentSkillUIFWList = new List<UIFW_SkillEntryToHover_Common>();



        private UnityAction<int, SkillSlotTypeEnum, RPSkill_SkillTypeEnum, BaseEventData> _callback_HoverEnterSkill;
        private UnityAction _callback_hoverExitSkill;




        public void InjectCallback_Skill(
            UnityAction<int, SkillSlotTypeEnum, RPSkill_SkillTypeEnum, BaseEventData> hoverEnter,
            UnityAction hoverExit)
        {
            _callback_HoverEnterSkill = hoverEnter;
            _callback_hoverExitSkill = hoverExit;
        }
        
        

        private void RefreshCurrentPlayerSkill()
        {
            foreach (var perUIFW in _currentSkillUIFWList) 
            {
                perUIFW.InstantiateInitialize(_playerCharacterFullPanelRef.CurrentSelectCharacterID,_callback_HoverEnterSkill, _callback_hoverExitSkill );
            }
            
        }
        


#endregion



#region 中间角色动画



        [SerializeField,Required,LabelText("holder_角色待机动画的Holder"),TitleGroup("===Widget===/动画")] 
        private Transform _holder_CharacterIdleAnimation;
        
        /// <summary>
        /// <para>生成的动画</para>
        /// </summary>
        private GameObject _go_CharacterIdleAnimation;


        private void RefreshCurrentCharacterIdleAnimation()
        {
            if (_go_CharacterIdleAnimation != null)
            {
                UnityEngine.Object.Destroy(_go_CharacterIdleAnimation);
                _go_CharacterIdleAnimation = null;
            }

            var idleAnimationPrefab = GlobalConfigurationAssetHolderHelper.Instance
                .FE_CharacterResourceInfo.GetConfigByType(_playerCharacterFullPanelRef.CurrentSelectCharacterID)
                .GetIdleAnimationOnUI();
            
            var newGO = UnityEngine.Object.Instantiate(idleAnimationPrefab, _holder_CharacterIdleAnimation);
            _go_CharacterIdleAnimation = newGO;

        }

        

#endregion


#region 卡牌

        private void _Button_OpenCardInfo()
        {
            var ds_not = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_UI_RequireNotification_要求广播一个通知);
            ds_not.ObjectArguStr = "~卡牌功能暂未实装~";
            GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_not);
        }

        

#endregion
        public override void StartInitializeBySP(UI_UIBasePanel parentUIP)
        {
            base.StartInitializeBySP(parentUIP);
            _playerCharacterFullPanelRef = parentUIP as UIP_PlayerCharacterFullPanel;
            
            
            
            

            GlobalActionBus.GetGlobalActionBus().RegisterAction(
                ActionBus_ActionTypeEnum.G_CharacterEquippedEquipmentChanged_角色装备发生变化,
                _ABC_RefreshContent_OnCharacterEquipmentChanged,
                20);



            foreach (UIRW_EquipmentEquippedEntry_PlayerCharacterFullPanel perUIRW in CurrentAllEquipmentUIRWList)
            {
                perUIRW.InstantiateInitialize(_playerCharacterFullPanelRef,
                    _callback_click,
                    _callback_HoverEnter,
                    _callback_hoverExit);
            }


            _button_OpenCardGroupButton.onClick.AddListener(_Button_OpenCardInfo);
        }


        public override void ShowThisSubPanel()
        {
            base.ShowThisSubPanel();
            RefreshContent();
        }

        /// <summary>
        /// <para>当 当前角色的已穿戴装备发生变化时，刷新每个槽位和面板。顺序是20</para>
        /// </summary>
        /// <param name="ds"></param>
        private void _ABC_RefreshContent_OnCharacterEquipmentChanged(DS_ActionBusArguGroup ds)
        {
            if (_playerCharacterFullPanelRef.CurrentSelectCharacterID != ds.IntArgu1.Value)
            {
                return;
            }
            if (!PanelActive)
            {
                return;
            }
            RefreshContent();
        }
        
        
        
        public void RefreshContent()
        {
            RefreshAllEquipment();
            RefreshCurrentPlayerSkill();

            RefreshCurrentCharacterIdleAnimation();


            float attack = 0f, defense = 0f, toughness = 0f, attackSpeed = 0f, skillSpeed = 0f, criticalRate = 0f,
                criticalDamage = 0f, accuracy = 0f, dodge = 0f;

            switch (GlobalConfigurationAssetHolderHelper.GetCurrentLoadPhase())
            {
                case GeneralGameAssetLoadPhaseEnum.WorldMap:
                case GeneralGameAssetLoadPhaseEnum.RegionMap:
                    var characterRecord = GlobalConfigurationAssetHolderHelper.Instance.RuntimeRecordHelperCharacter
                        .GetPlayerInfo(_playerCharacterFullPanelRef.CurrentSelectCharacterID);
                    attack = characterRecord.AttackPower;
                    defense = characterRecord.Defense;
                    toughness = characterRecord.Toughness;
                    attackSpeed = characterRecord.AttackSpeed;
                    skillSpeed = characterRecord.SkillAccelerate;
                    criticalRate = characterRecord.CriticalRate;
                    criticalDamage = characterRecord.CriticalBonus;
                    accuracy = characterRecord.Accuracy;
                    dodge = characterRecord.Dodge;
                    
                    break;
                case GeneralGameAssetLoadPhaseEnum.ARPG:
                    var currentBehaviour = SubGameplayLogicManager_ARPG.Instance.PlayerCharacterBehaviourControllerReference
                        .GetBehaviourRef(_playerCharacterFullPanelRef.CurrentSelectCharacterID);
                    attack = currentBehaviour.GetFloatDataEntryByType(RP_DataEntry_EnumType.AttackPower_攻击力).CurrentValue;
                    defense = currentBehaviour.GetFloatDataEntryByType(RP_DataEntry_EnumType.Defense_防御力).CurrentValue;
                    toughness = currentBehaviour.GetFloatDataEntryByType(RP_DataEntry_EnumType.Toughness_韧性).CurrentValue;
                    attackSpeed = currentBehaviour.GetFloatDataEntryByType(RP_DataEntry_EnumType.AttackSpeed_攻击速度).CurrentValue;
                    skillSpeed = currentBehaviour.GetFloatDataEntryByType(RP_DataEntry_EnumType.SkillAccelerate_技能加速).CurrentValue;
                    criticalRate = currentBehaviour.GetFloatDataEntryByType(RP_DataEntry_EnumType.CriticalRate_暴击率).CurrentValue;
                    criticalDamage = currentBehaviour.GetFloatDataEntryByType(RP_DataEntry_EnumType.CriticalBonus_暴击伤害).CurrentValue;
                    accuracy = currentBehaviour.GetFloatDataEntryByType(RP_DataEntry_EnumType.Accuracy_命中率).CurrentValue;
                    dodge = currentBehaviour.GetFloatDataEntryByType(RP_DataEntry_EnumType.DodgeRate_闪避率).CurrentValue;
                    break;
            }
            
            _text_AttackText.text = ((int)attack).ToString();
            _text_DefenseText.text = ((int)defense).ToString();
            _text_ToughnessText.text = ((int)toughness).ToString();
            
            _text_AttackSpeedText.text = (attackSpeed * 100f).ToString("F0") + "%";

            _text_SkillSpeedText.text = skillSpeed.ToString("F0") + "%";
            _text_CriticalRateText.text = ((int)criticalRate).ToString() +"%";
            _text_CriticalDamageText.text = (150f + criticalDamage *100f).ToString("F0")+"%" ;
            _text_AccuracyText.text = ((int)accuracy).ToString()+"%";
            _text_DodgeText.text = ((int)dodge).ToString() +"%";



            //技能部分
            
            //TODO：此处不应当直接找PlayerController，下面的Spine也是。也应当是来自某个中间层提供的信息
            
            GCSO_PerCharacterInfo currentCharacterInfo =
                GlobalConfigSO.RuntimeContent().AllCharacterInfoCollection
                    .Find((info => info.CharacterID == _playerCharacterFullPanelRef.CurrentSelectCharacterID));
            var cic_skillInfo = currentCharacterInfo.GetComponent<CIC_PlayerSkillInfo>();

            if (cic_skillInfo != null)
            {
                var runtimeWeapon = GlobalConfigSO.RuntimeContent().CurrentEquipmentInfoList.
                    Find((info => info.EquippedWithCharacter == currentCharacterInfo.CharacterID && info.IfIsEquipmentIsWeapon()));
                var weaponInfo = GCAHHExtend.GetEquipmentRawInfo(runtimeWeapon.EquipmentUID) as SOFE_WeaponInfo.PerTypeInfo;
                var damageType = weaponInfo.DamageType;

                // foreach (CIC_PlayerSkillInfo.PlayerSkillInfoEntry skillEntry in cic_skillInfo.ObtainedSkillList)
                // {
                //
                //     Sprite ts = null;
                //     switch (damageType)
                //     {
                //         case DamageTypeEnum.YuanNengGuang_源能光:
                //             ts = _sprite_SkillBaseSprite_Guang;
                //             break;
                //         case DamageTypeEnum.YuanNengDian_源能电:
                //             ts = _sprite_SkillBaseSprite_Dian;
                //             break;
                //         case DamageTypeEnum.AoNengShui_奥能水:
                //             ts = _sprite_SkillBaseSprite_Shui;
                //             break;
                //         case DamageTypeEnum.AoNengHuo_奥能火:
                //             ts = _sprite_SkillBaseSprite_Huo;
                //             break;
                //         case DamageTypeEnum.YouNengXingHong_幽能猩红:
                //             ts = _sprite_SkillBaseSprite_XingHong;
                //             break;
                //     }
                //     SOConfig_RPSkill skillTemplate = GlobalConfigurationAssetHolderHelper.Instance
                //         .Collection_SkillConfig.GetRPSkillConfigByTypeAndLevel(skillEntry.SkillType);
                //     switch (skillEntry.SkillSlot)
                //     {
                //         case SkillSlotTypeEnum.SlotNormal1_常规槽位1:
                //             _image_normalSkillIcon1.gameObject.SetActive(true);
                //             _image_NormalSkill1Border.sprite = ts;
                //             _image_normalSkillIcon1.sprite = skillTemplate.ConcreteSkillFunction.GetCurrentSprite();
                //             break;
                //         case SkillSlotTypeEnum.SlotNormal2_常规槽位2:
                //             _image_normalSkillIcon2.gameObject.SetActive(true);
                //             _image_NormalSKill2Border.sprite = ts;
                //             _image_normalSkillIcon2.sprite = skillTemplate.ConcreteSkillFunction.GetCurrentSprite();
                //             break;
                //         case SkillSlotTypeEnum.DisplacementSkill_位移槽位:
                //             _image_moveSkillIcon.gameObject.SetActive(true);
                //             _image_MoveSKillBorder.sprite = ts;
                //             _image_moveSkillIcon.sprite = skillTemplate.ConcreteSkillFunction.GetCurrentSprite();
                //             break;
                //         case SkillSlotTypeEnum.UltraSkill_超杀槽位:
                //             _image_UpSKillBorder.sprite = ts;
                //             _image_upSkillIcon.gameObject.SetActive(true);
                //             _image_upSkillIcon.sprite = skillTemplate.ConcreteSkillFunction.GetCurrentSprite();
                //             break;
                //     }
                // }


            }




#region Spine部分

            // AnimationClip animationClip = GlobalConfigurationAssetHolderHelper.Instance.RuntimeRecordHelperCharacter
            //     .GetCurrentWeaponTemplateConfig(_playerCharacterFullPanelRef.CurrentSelectCharacterID).WeaponTemplateRef
            //     .WeaponFunction.WeaponIdleOnUIAnimationClip;
            // _animation_CharacterIdleAnimation.clip = animationClip; 
            // _animation_CharacterIdleAnimation.Play();
            
            
            //
            // var currentAnimationHelper = currentBehaviour.GetRelatedArtHelper().CharacterAnimationHelperObjectsDict[0];
            // switch (currentAnimationHelper)
            // {
            //     case BaseCharacterSheetAnimationHelper baseCharacterSheetAnimationHelper:
            //         break;
            //     case BaseCharacterSpineHelper baseCharacterSpineHelper:
            //         _sg_CharacterSpine.skeletonDataAsset = baseCharacterSpineHelper.SelfSkeleton.SkeletonDataAsset;
            //         _sg_CharacterSpine.Initialize(true);
            //
            //
            //         // _sg_CharacterSpine.transform.localScale =
            //             // currentBehaviour.GetRelatedArtHelper()._HUDSpineScale * Vector3.one;
            //         _sg_CharacterSpine.AnimationState.SetAnimation(0, baseCharacterSpineHelper.IdleAnimation, true);
            //         break;
            //     default:
            //         throw new ArgumentOutOfRangeException(nameof(currentAnimationHelper));
            // }



#endregion
            
            
            
            
            
            
        }


#region 全局监听

        

#endregion



    }
}
