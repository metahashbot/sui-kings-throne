using RPGCore.Buff;
using RPGCore.DataEntry;
using RPGCore.UtilityDataStructure;

namespace RPGCore.Interface
{
    /// <summary>
    /// <para>表明一个RPObject可以接受DataEntry的效果</para>
    /// <para>RPBehaviour</para>
    /// </summary>
    public interface I_RP_DataEntry_ObjectCanReceiveDataEntryEffect
    {
        public abstract void ReceiveDataEntry_ReceiveFromRPDS(RP_DS_DataEntryApplyInfo rpds);

        public abstract RP_DataEntry_Base ReceiveDataEntry_GetRelatedDataEntry(RP_DataEntry_EnumType type);

        public abstract BuffAvailableType ReceiveDataEntry_CheckTargetBuff(RolePlay_BuffTypeEnum type);
        public abstract BaseRPBuff ReceiveDataEntry_GetTargetBuff(RolePlay_BuffTypeEnum type);

    }
}