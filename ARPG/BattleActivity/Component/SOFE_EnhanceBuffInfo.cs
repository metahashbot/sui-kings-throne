using System;
using System.Collections.Generic;
using UnityEngine;
namespace Global.Loot
{
    [Serializable]
    [CreateAssetMenu(fileName = "SOFE_强化Buff", menuName = "#SO Assets#/SOFE/SOFE_强化Buff", order = 61)]
    public class SOFE_EnhanceBuffInfo : ScriptableObject
    {
        [Serializable]
        public class EnhanceBuffInfo
        {
            public int UID;
            public string Name;
            public Sprite IconSprite;
        }

        public List<EnhanceBuffInfo> EnhanceBuffInfoList = new ();

        public EnhanceBuffInfo GetEnhanceBuffInfo(int uid)
        {
            return EnhanceBuffInfoList.Find((x) => x.UID == uid);
        }
    }
}