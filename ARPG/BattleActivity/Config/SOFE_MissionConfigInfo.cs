using System;
using System.Collections.Generic;
using ARPG.Config.BattleLevelConfig;
using GameplayEvent.Handler;
using GameplayEvent.SO;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
namespace ARPG.Manager.Config
{
	/// <summary>
	/// 这个文件记录了一个关卡是怎么配置的，包括房间的数量，每个房间使用的模板和权重等信息
	/// </summary>
	[Serializable]
	[CreateAssetMenu(fileName = "SOFE_关卡配置", menuName = "#SO Assets#/SOFE/SOFE_关卡配置")]
	public class SOFE_MissionConfigInfo : ScriptableObject
	{
		[LabelText("关卡列表")]
		public List<MissionConfigInfo> MissionConfigList = new();

		/// <summary>
		/// 一个关卡的房间数量是确定的，每个房间有单独配置
		/// </summary>
		[Serializable]
		public sealed class MissionConfigInfo
		{
			[LabelText("关卡UID"), Searchable]
			public int UID;
            [LabelText("房间数量")]
            public int RoomCount;
            [LabelText("房间列表")]
            public List<RoomConfigInfo> RoomConfigList = new();
		}

		/// <summary>
		/// 一个房间配置包含多个可能的房间模板，并不是一个确定的房间
		/// </summary>
		[Serializable]
		public class RoomConfigInfo
        {
            [LabelText("房间编号"), Searchable]
            public int Index;
            [LabelText("房间数量")]
            public int ExitNumber;
			[LabelText("房间模板列表")]
			public List<RoomTemplateInfo> RoomTemplateList = new();
        }

		/// <summary>
		/// 这个类仅应在MissionConfig内使用，表示房间配置使用了哪个模板
		/// 不记录模板具体配置
		/// </summary>
		[Serializable]
		public class RoomTemplateInfo
        {
            [LabelText("模板UID"), Searchable]
            public int UID;
            [LabelText("模板名称")]
            public string Name;
            [LabelText("模板权重")]
            public float Weight;
		}

        [LabelText("房间模板列表")]
        public List<RoomTemplateConfigInfo> RoomTemplateList = new();

        public enum RoomTemplateEnumType
        {
            None,
            EnterRoom,
            NormalBattleRoom,
            EliteBattleRoom,
            SupplyRoom,
            BossRoom
        }

        /// <summary>
        /// 一个房间模板内可以包含多次刷怪
        /// </summary>
        [Serializable]
        public class RoomTemplateConfigInfo
        {
            [LabelText("模板UID"), Searchable]
            public int UID;
            [LabelText("模板名称")]
            public string Name;
            [LabelText("房间类型")]
            public RoomTemplateEnumType Type = RoomTemplateEnumType.None;
            // 对应区域配置的区域ID字段
            [LabelText("区域ID")]
            public string AreaID;
            [LabelText("波次列表")]
            public List<WaveConfigInfo> WaveConfigList = new();
        }

        [Serializable]
        public class WaveConfigInfo
        {
            [LabelText("波次序号"), Searchable]
            public int Index;
            [LabelText("刷怪列表")]
            public List<EnemyConfigInfo> EnemyConfigList = new();
        }

        /// <summary>
        /// 每次刷怪的详细信息
        /// </summary>
        [Serializable]
        public class EnemyConfigInfo
        {
            [LabelText("敌人UID"), Searchable]
            public int UID;
            [LabelText("敌人名称")]
            public string Name;
            [LabelText("敌人数量")]
            public int EnemyNumber;
            // 对应区域配置的生成点字段
            [LabelText("出生地点")]
            public string SpawnPoint;
            [LabelText("额外强化")]
            public float ExtraStrength;
        }

        public RoomTemplateConfigInfo GetRoomTemplateConfigInfo(int tUID)
        {
            var roomTemplate = RoomTemplateList.Find(t => t.UID == tUID);
            return roomTemplate;
        }

        public int GetFirstRoomTemplateUID(int mUID)
        {
            var missionConfig = MissionConfigList.Find(m => m.UID == mUID);
            var roomConfig = missionConfig?.RoomConfigList.Find(r => r.Index == 0);
            var res = roomConfig?.RoomTemplateList[0].UID;
            return res ?? -1;
        }

        public List<RoomTemplateConfigInfo> GetExitRoomTemplateList(int UID, int index)
        {
            List<RoomTemplateConfigInfo> res = new();
            var missionConfig = MissionConfigList.Find(m => m.UID == UID);
            if (missionConfig == null)
            {
                Debug.LogError($"关卡{UID}无配置");
                return res;
            }
            var currentRoomConfig = missionConfig.RoomConfigList.Find(r => r.Index == index);
            var nextRoomConfig = missionConfig.RoomConfigList.Find(r => r.Index == index + 1);
            if (currentRoomConfig == null)
            {
                Debug.LogError($"序号{index}的房间无配置");
                return res;
            }
            if (nextRoomConfig == null)
            {
                Debug.LogError($"序号{index + 1}的房间无配置");
                return res;
            }
            int templateCount = currentRoomConfig.ExitNumber;

            var supplyRoom = RoomTemplateList.Find(t => t.Type == RoomTemplateEnumType.SupplyRoom
                && nextRoomConfig.RoomTemplateList.Find(r => r.UID == t.UID) != null);
            if (supplyRoom != null)
            {
                res.Add(supplyRoom);
                templateCount--;
            }

            float totalWeight = 0;
            List<RoomTemplateInfo> tmpList = new();
            foreach(var t in nextRoomConfig.RoomTemplateList)
            {
                if (t.UID == supplyRoom?.UID)
                {
                    continue;
                }
                totalWeight += t.Weight;
                tmpList.Add(t);
            }

            for (int i = 0; i < templateCount; i++)
            {
                float rand = UnityEngine.Random.Range(0f, totalWeight);
                float currentWeight = 0f;
                RoomTemplateInfo randInfo = null;
                foreach(var t in tmpList)
                {
                    currentWeight += t.Weight;
                    if (rand < currentWeight)
                    {
                        randInfo = t;
                        break;
                    }
                }
                var randTemplate = RoomTemplateList.Find(t => t.UID == randInfo.UID);
                if (randTemplate != null)
                {
                    totalWeight -= randInfo.Weight;
                    tmpList.Remove(randInfo);
                    res.Add(randTemplate);
                }
            }

            return res;
        }
    }
}