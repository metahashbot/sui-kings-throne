
using RPGCore.Buff;
using RPGCore.DataEntry;

namespace RPGCore.Interface
{
    /// <summary>
    /// <para>表明一个RPObject可以施加Buff效果</para>
    /// <para> RPBehaviour / RPDispatcher / TowerBehaviour</para>
    /// </summary>
    public interface I_RP_Buff_ObjectCanApplyBuff
    {

        public abstract string ApplyBuff_GetRelatedCasterName();
        public abstract RP_DataEntry_Base ApplyBuff_GetRelatedDataEntry(RP_DataEntry_EnumType type, bool allowNotExist=  false);
        public abstract BaseRPBuff ApplyBuff_GetRelatedBuff(RolePlay_BuffTypeEnum buffType);
    }
}