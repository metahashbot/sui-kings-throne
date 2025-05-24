using System;
using RPGCore.Buff;

namespace RPGCore.UtilityDataStructure
{
    /// <summary>
    /// <para>在Damage内附属的Buff效果。区别于本身的DataEntry效果。</para>
    /// <para>附属于Damage的表示这个效果可能会被闪避</para>
    /// </summary>
    [Serializable]
    public class ConSer_BuffEffectInsideDamage
    {
        public RolePlay_BuffTypeEnum TargetBuff;
        public int TargetLevel;
        public ConSer_BuffApplyInfo CalculationArgumentGroup;



    }
}