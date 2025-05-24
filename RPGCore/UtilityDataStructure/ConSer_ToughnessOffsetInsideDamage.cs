using System;
using Sirenix.OdinInspector;

namespace RPGCore.UtilityDataStructure
{
    [Serializable]
    [TypeInfoBox("在Damage内部的，关于削韧效果的额外修正")]
    public class ConSer_ToughnessOffsetInsideDamage
    {
        public float AdditionalAddon = 0;
        public float AdditionalMultiplier = 1f;
    }
}
