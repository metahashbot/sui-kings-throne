using RPGCore.Buff;
using RPGCore.DataEntry;

namespace RPGCore.Interface
{
    /// <summary>
    /// <para>表明一个RPObject可以施加DataEntry效果</para>
    /// <para> RP Behaviour 、TowerBehaviour(Aura) , Projectile, RP Dispatcher</para>
    /// </summary>
    public interface I_RP_DataEntry_ObjectCanApplyDataEntryEffect
    {
        public abstract RP_DataEntry_Base ApplyDataEntry_GetRelatedDataEntry(RP_DataEntry_EnumType type);
        public abstract BuffAvailableType ApplyDataEntry_CheckTargetBuff(RolePlay_BuffTypeEnum type);
        public abstract BaseRPBuff ApplyDataEntry_GetTargetBuff(RolePlay_BuffTypeEnum type);
    }
}