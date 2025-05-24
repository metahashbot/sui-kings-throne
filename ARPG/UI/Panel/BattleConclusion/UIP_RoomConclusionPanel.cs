using ARPG.Common;
using ARPG.Manager;
using Global;
using Global.ActionBus;
using Global.UIBase;
using Sirenix.OdinInspector;
using UnityEngine;
using static ARPG.Manager.Config.SOFE_MissionConfigInfo;
namespace ARPG.UI.Panel.BattleConclusion
{

    public class UIP_RoomConclusionPanel : UI_UIBasePanel
    {
        [LabelText("UI_强化1"), SerializeField, TitleGroup("===Widget==="), Required]
        protected UIRW_PerEnhanceBuff UI_Enhance1;
        [LabelText("UI_强化2"), SerializeField, TitleGroup("===Widget==="), Required]
        protected UIRW_PerEnhanceBuff UI_Enhance2;
        [LabelText("UI_强化3"), SerializeField, TitleGroup("===Widget==="), Required]
        protected UIRW_PerEnhanceBuff UI_Enhance3;

        public override void StartInitializeByUIM()
        {
            base.StartInitializeByUIM();

            int missionUID = EditorProxy_ARPGEditor.Instance.AreaLogicHolderPrefab
                .GetComponent<EditorProxy_AreaLogicHolder>().MissionUID;
            if (GCAHHExtend.IfMissionConfigContain(missionUID))
            {
                UI_Enhance1.Initialize(ClosePanel);
                UI_Enhance2.Initialize(ClosePanel);
                UI_Enhance3.Initialize(ClosePanel);

                GlobalActionBus.GetGlobalActionBus().RegisterAction(
                    ActionBus_ActionTypeEnum.G_NB_TriggerAreaManager_OnTransmissionToNewArea_当传送到新区域,
                    OnTransmissionToNewArea);

                GlobalActionBus.GetGlobalActionBus().RegisterAction(
                    ActionBus_ActionTypeEnum.G_Activity_ARPG_ClearAreaEnemy_活动ARPG_清除区域内的敌人,
                    OnClearAreaEnemy);
            }
        }

        private bool skipRoomConclusion = false;
        private void OnTransmissionToNewArea(DS_ActionBusArguGroup ds)
        {
            if (ds.ObjectArgu1 is string areaUID)
            {
                var templateID = ds.IntArgu1.Value;
                var roomConfig = GCAHHExtend.GetRoomTemplateConfigInfo(templateID);
                // 跳过补给房间和Boss房间的房间结算
                if (roomConfig.Type == RoomTemplateEnumType.SupplyRoom ||
                   roomConfig.Type == RoomTemplateEnumType.BossRoom)
                {
                    skipRoomConclusion = true;
                }
                else
                {
                    skipRoomConclusion = false;
                }
            }
        }

        private void OnClearAreaEnemy(DS_ActionBusArguGroup ds)
        {
            if (skipRoomConclusion)
            {
                return;
            }
            base.ShowThisPanel();
            var enhanceBuffList = GCAHHExtend.GetEnhanceBuffInfoList();
            int index1 = Random.Range(0, enhanceBuffList.Count - 2);
            int index2 = Random.Range(index1 + 1, enhanceBuffList.Count - 1);
            int index3 = Random.Range(index2 + 1, enhanceBuffList.Count);
            var info1 = enhanceBuffList[index1];
            UI_Enhance1.SetContent(info1.Name, info1.IconSprite, (RolePlay_BuffTypeEnum)info1.UID);
            var info2 = enhanceBuffList[index2];
            UI_Enhance2.SetContent(info2.Name, info2.IconSprite, (RolePlay_BuffTypeEnum)info2.UID);
            var info3 = enhanceBuffList[index3];
            UI_Enhance3.SetContent(info3.Name, info3.IconSprite, (RolePlay_BuffTypeEnum)info3.UID);

            var ds_pause = new DS_ActionBusArguGroup(
                ActionBus_ActionTypeEnum.G_Global_RequirePauseGame_要求暂停游戏);
            ds_pause.ObjectArgu1 = this;
            GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_pause);
        }

        private void ClosePanel()
        {
            base.HideThisPanel();

            var ds_unpause = new DS_ActionBusArguGroup(
                ActionBus_ActionTypeEnum.G_Global_RequireResumeGame_要求解除暂停);
            ds_unpause.ObjectArgu1 = this;
            GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_unpause);
        }
    }
}