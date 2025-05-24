using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGCore.UtilityDataStructure
{
    public struct RP_DS_DamageDirectionInfo
    {
        public RP_DS_DamageDirection_PerLevel[] PerLevelInfoArray;

        public RP_DS_DamageDirectionInfo(ConSer_DamageDirectionInfo conSer)
        {
            PerLevelInfoArray = new RP_DS_DamageDirection_PerLevel[conSer.PerLevelInfoList.Count];
            for (int i = 0; i < conSer.PerLevelInfoList.Count; i++)
            {
                PerLevelInfoArray[i] = new RP_DS_DamageDirection_PerLevel(conSer.PerLevelInfoList[i]);
            }
        }
    }


    public struct RP_DS_DamageDirection_PerLevel
    {
        public Vector2 TriggerRange;
        public float FloatArgu1;
        public float FloatArgu2;


        public RP_DS_DamageDirection_PerLevel(ConSer_DamageDirection_PerLevelInfo conSer)
        {
            TriggerRange = conSer.TriggerRange;
            FloatArgu1 = conSer.FloatArgu1;
            FloatArgu2 = conSer.FloatArgu2;
        }
    }

    [Serializable]
    public class ConSer_DamageDirectionInfo
    {
        public List<ConSer_DamageDirection_PerLevelInfo> PerLevelInfoList;
    }

    [Serializable]
    public class ConSer_DamageDirection_PerLevelInfo
    {
        public Vector2 TriggerRange;
        public float FloatArgu1 = 1f;
        public float FloatArgu2 = 1f;
    }
}