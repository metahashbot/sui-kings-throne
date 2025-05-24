
using Global.ActionBus;
using RPGCore.Buff;
using RPGCore.Buff.BuffHolder;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;

namespace RPGCore.Dispatcher
{
    // public partial class RolePlay_BaseDispatcher
    // {
    //     private RPBuff_BuffHolder _selfBuffHolderInstance;
    //     public RPBuff_BuffHolder SelfBuffHolderInstance => _selfBuffHolderInstance;
    //
    //
    //     public BuffApplyResultEnum ReceiveBuff_TryApplyBuff(RP_DS_BuffApplyInfo rpDSBuffApplyInfo,
    //         I_RP_Buff_ObjectCanApplyBuff caster, int effectIDStamp)
    //     {
    //         return _selfBuffHolderInstance.TryApplyBuff_FromExternal(rpDSBuffApplyInfo, caster, this);
    //     }
    //
    //     public BuffRemoveResultEnum ReceiveBuff_RemoveTargetBuff(RolePlay_BuffTypeEnum materialType, int operationLevel)
    //     {
    //         return _selfBuffHolderInstance.TryRemoveBuff(materialType, operationLevel);
    //     }
    //     public LocalActionBus ReceiveBuff_GetRelatedActionBus()
    //     {
    //         return _selfActionBusInstance;
    //     }
    //
    //     public BuffAvailableType ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum materialType)
    //     {
    //         return _selfBuffHolderInstance.CheckTargetBuff(materialType);
    //     }
    //
    //     public RPBuff_Runtime ReceiveBuff_GetTargetBuff(RolePlay_BuffTypeEnum materialType)
    //     {
    //         return _selfBuffHolderInstance.GetTargetBuff(materialType);
    //     }
    //
    //     public RP_DataEntry_Base ReceiveBuff_GetDataEntry(RP_DataEntry_EnumType entryType)
    //     {
    //         return _selfDatabaseInstance.GetTargetDataEntry(entryType);
    //     }
    //
    //     public string ApplyBuff_GetRelatedCasterName()
    //     {
    //         return name;
    //     }
    //
    //     public RP_DataEntry_Base ApplyBuff_GetRelatedDataEntry(RP_DataEntry_EnumType materialType)
    //     {
    //         return _selfDatabaseInstance.GetTargetDataEntry(materialType);
    //     }
    //
    //     public RPBuff_Runtime ApplyBuff_GetRelatedBuff(RolePlay_BuffTypeEnum buffType)
    //     {
    //         return _selfBuffHolderInstance.GetTargetBuff(buffType);
    //     }
    //
    //
    //
    // }
}