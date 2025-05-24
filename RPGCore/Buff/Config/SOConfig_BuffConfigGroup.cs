using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGCore.Buff.Config
{
    [Serializable]
    [CreateAssetMenu(fileName = "Buff Config Group", menuName = "#SO Assets#/#RPG Core#/Buff Config Group", order = 0)]
    public class  SOConfig_BuffConfigGroup : ScriptableObject
    {
        public List<Buff_InitConfigEntry> InitConfigList;

    }
}