using ARPG.Manager;
using Global.UIBase;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace ARPG.UI.Panel.BattleConclusion
{

    public class UIRW_PerEnhanceBuff : UI_UISingleRuntimeWidget
    {
        [LabelText("Text_强化"), SerializeField, TitleGroup("===Widget==="), Required]
        protected TextMeshProUGUI Text_Enhance;
        [LabelText("Image_强化"), SerializeField, TitleGroup("===Widget==="), Required]
        protected Image Image_Enhance;
        [LabelText("Button_强化"), SerializeField, TitleGroup("===Widget==="), Required]
        protected Button Button_Enhance;
        protected RolePlay_BuffTypeEnum BuffType;

        public void Initialize(UnityAction callback)
        {
            Button_Enhance.onClick.AddListener(ApplyEnhanceBuff);
            Button_Enhance.onClick.AddListener(callback);
        }

        public void SetContent(string buffName, Sprite buffIcon, RolePlay_BuffTypeEnum type)
        {
            Text_Enhance.text = buffName;
            Image_Enhance.sprite = buffIcon;
            this.BuffType = type;
        }

        private void ApplyEnhanceBuff()
        {
            var allCharacters = SubGameplayLogicManager_ARPG.Instance
                .PlayerCharacterBehaviourControllerReference.CurrentAllCharacterBehaviourList;
            foreach (var c in allCharacters)
            {
                c.ReceiveBuff_TryApplyBuff(BuffType, null, null);
            }
        }
    }
}