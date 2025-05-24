using Global.ActionBus;
using RPGCore.Buff;
using RPGCore.DataEntry;
using UnityEngine;

namespace RPGCore.Interface
{
    /// <summary>
    /// <para>表明一个RPObject可以施加伤害</para>
    /// <para> RPBehaviour / RP Dispatcher / TowerBehaviour / Projectile</para>
    /// </summary>
    public interface I_RP_Damage_ObjectCanApplyDamage
    {
        public abstract string ApplyDamage_GetRelatedCasterName();
        public abstract Float_RPDataEntry ApplyDamage_GetRelatedDataEntry(RP_DataEntry_EnumType type, bool allowNotExist = false);
        public abstract FloatPresentValue_RPDataEntry ApplyDamage_GetRelatedPresentValue(RP_DataEntry_EnumType type);
        public abstract BuffAvailableType ApplyDamage_CheckTargetBuff(RolePlay_BuffTypeEnum type);
        public abstract BaseRPBuff ApplyDamage_GetTargetBuff(RolePlay_BuffTypeEnum type);
        public abstract LocalActionBus ApplyDamage_GetLocalActionBus();

        public abstract bool CasterDataEntryValid();
        public abstract Vector3? GetDamageCasterPosition();

    }
}