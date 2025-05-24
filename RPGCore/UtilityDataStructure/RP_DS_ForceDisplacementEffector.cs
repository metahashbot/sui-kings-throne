using System;
using UnityEngine;

namespace RPGCore.UtilityDataStructure
{
    [Serializable]
    public class RP_DS_ForceDisplacementEffector
    {
        [Range(-5, 5), Header("正数推，负数拉")]
        public int Force = 1;
    }
}