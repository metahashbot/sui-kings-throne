using System;
using System.Collections.Generic;
using Global.ActionBus;
using Global;
using Sirenix.OdinInspector;
using static UnityEngine.Object;
using Global.AreaOnMap;
using Global.AreaOnMap.EditorProxy;
using System.Linq;
using ARPG.Common;
using Unity.Mathematics;
using System.Diagnostics;
using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using AmplifyShaderEditor;
#endif
namespace ARPG.Manager.Component
{
	[TypeInfoBox("掉落物管理")]
	[Serializable]
	public class RoomController_SubActivity : BaseSubActivityService
	{
        public Dictionary<string, EP_TransmissionEntrance> AllTransmissionEntrance = new();
        public Dictionary<string, List<EP_TransmissionExit>> AllTransmissionExit = new();
        public Dictionary<string, EP_RecoveryPoint> AllRecoveryPoint = new();
        public int CurrentRoomIndex;

        /// <summary>
        /// 系统初始化，绑定事件
        /// </summary>
        public void AwakeInitialize()
        {
            foreach (var per in FindObjectsOfType<EP_TransmissionEntrance>(true))
            {
                AllTransmissionEntrance.Add(per.RelatedAreaUID, per);
            }
            foreach (var per in FindObjectsOfType<EP_TransmissionExit>(true))
            {
                if (!AllTransmissionExit.ContainsKey(per.RelatedAreaUID))
                {
                    AllTransmissionExit.Add(per.RelatedAreaUID, new List<EP_TransmissionExit>());
                }
                AllTransmissionExit[per.RelatedAreaUID].Add(per);
            }
            foreach (var per in FindObjectsOfType<EP_RecoveryPoint>(true))
            {
                AllRecoveryPoint.Add(per.RelatedAreaUID, per);
            }

            GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_NB_TriggerAreaManager_OnTransmissionToNewArea_当传送到新区域,
                OnTransmissionToNewArea);

            GlobalActionBus.GetGlobalActionBus().RegisterAction(
                ActionBus_ActionTypeEnum.G_Activity_ARPG_ClearAreaEnemy_活动ARPG_清除区域内的敌人,
                OnClearAreaEnemy);
        }

        public void LateLoadInitialize()
        {
            CurrentRoomIndex = 0;

            int missionUID = EditorProxy_ARPGEditor.Instance.AreaLogicHolderPrefab
                .GetComponent<EditorProxy_AreaLogicHolder>().MissionUID;
            var templateUID = GCAHHExtend.GetFirstRoomTemplateUID(missionUID);
            if (templateUID < 0)
            {
                return;
            }
            var templateConfig = GCAHHExtend.GetRoomTemplateConfigInfo(templateUID);
            ActivityManagerRef.StartCoroutine(LateTransmission(templateConfig.AreaID, templateConfig.UID));
        }

        private IEnumerator LateTransmission(string areaID, int targetTemplateID)
        {
            yield return new WaitForSeconds(1.0f);

            var ds_transmission = new DS_ActionBusArguGroup(
                ActionBus_ActionTypeEnum.G_NB_TriggerAreaManager_OnTransmissionToNewArea_当传送到新区域);
            ds_transmission.ObjectArgu1 = areaID;
            ds_transmission.ObjectArgu2 = null;
            ds_transmission.IntArgu1 = targetTemplateID;
            GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_transmission);
        }

        /// <summary>
        /// 传送到新房间时关闭传送点和恢复点
        /// </summary>
        private void OnTransmissionToNewArea(DS_ActionBusArguGroup ds)
        {
            //关闭旧房间的恢复点
            if (ds.ObjectArgu2 is string oldArea)
            {
                if (AllRecoveryPoint.ContainsKey(oldArea))
                {
                    AllRecoveryPoint[oldArea].gameObject.SetActive(false);
                }
            }
            //先关后开，即使新旧是同一房间也没问题
            if (ds.ObjectArgu1 is string areaUID)
            {
                //如果当前房间是补给房，打开恢复点并直接设置好传送出口
                var templateID = ds.IntArgu1.Value;
                var roomConfig = GCAHHExtend.GetRoomTemplateConfigInfo(templateID);
                UnityEngine.Debug.Log($"进入房间，房间模板{templateID}");
                if (roomConfig.Type == Config.SOFE_MissionConfigInfo.RoomTemplateEnumType.SupplyRoom)
                {
                    if (AllRecoveryPoint.ContainsKey(areaUID))
                    {
                        AllRecoveryPoint[areaUID].gameObject.SetActive(true);
                        AllRecoveryPoint[areaUID].hasTakenEffect = false;
                    }
                }
                //关闭新房间出口
                if (AllTransmissionExit.ContainsKey(areaUID))
                {
                    foreach (var e in AllTransmissionExit[areaUID])
                    {
                        e.gameObject.SetActive(false);
                    }
                }
            }
        }

        /// <summary>
        /// 区域内敌人清理完毕时设置出口传送点参数和下一个房间的参数
        /// </summary>
        private void OnClearAreaEnemy(DS_ActionBusArguGroup ds)
        {
            if (ds.ObjectArguStr is string areaUID)
            {
                SetTransmissionExit(areaUID);
            }
        }

        private void SetTransmissionExit(string areaUID)
        {
            if (AllTransmissionExit.ContainsKey(areaUID))
            {
                int missionUID = EditorProxy_ARPGEditor.Instance.AreaLogicHolderPrefab
                    .GetComponent<EditorProxy_AreaLogicHolder>().MissionUID;
                var exitRoomTemplateList = GCAHHExtend.GetExitRoomTemplateList(missionUID, CurrentRoomIndex);

                int n = math.min(AllTransmissionExit[areaUID].Count, exitRoomTemplateList.Count);
                for (int i = 0; i < n; i++)
                {
                    var exit = AllTransmissionExit[areaUID][i];
                    var template = exitRoomTemplateList[i];
                    exit.gameObject.SetActive(true);
                    exit.TargetAreaUID = template.AreaID;
                    exit.TargetTemplateUID = template.UID;
                    exit.SupplyIcon.SetActive(false);
                    exit.NormalBattleIcon.SetActive(false);
                    exit.EliteBattelIcon.SetActive(false);
                    exit.BossIcon.SetActive(false);
                    switch (template.Type)
                    {
                        case Config.SOFE_MissionConfigInfo.RoomTemplateEnumType.NormalBattleRoom:
                            exit.NormalBattleIcon.SetActive(true);
                            break;
                        case Config.SOFE_MissionConfigInfo.RoomTemplateEnumType.EliteBattleRoom:
                            exit.EliteBattelIcon.SetActive(true);
                            break;
                        case Config.SOFE_MissionConfigInfo.RoomTemplateEnumType.BossRoom:
                            exit.BossIcon.SetActive(true);
                            break;
                        case Config.SOFE_MissionConfigInfo.RoomTemplateEnumType.SupplyRoom:
                            exit.SupplyIcon.SetActive(true);
                            break;
                    }
                    //UnityEngine.Debug.Log($"设置出口{i}，房间模板{template.UID}");
                }

                CurrentRoomIndex++;
            }
        }
    }
}