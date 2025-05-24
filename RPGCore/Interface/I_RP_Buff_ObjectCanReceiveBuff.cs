using System.Collections.Generic;
using Global.ActionBus;
using RPGCore.Buff;
using RPGCore.Buff.BuffHolder;
using RPGCore.DataEntry;
using RPGCore.UtilityDataStructure;
using UnityEngine;

namespace RPGCore.Interface
{
    /// <summary>
    ///  <para> 表明这是一个可以接受Buff的RPObject ：RPBehaviour、RPDispatcher、TowerBehaviour</para>
    /// </summary>
    public interface I_RP_Buff_ObjectCanReceiveBuff
    {
        /// <summary> 
        /// <para>运行时的ReceiveBuff。</para>
        /// </summary>
        public abstract BuffApplyResultEnum ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum buffType, 
            I_RP_Buff_ObjectCanApplyBuff caster, I_RP_Buff_ObjectCanReceiveBuff receiver,
            params BaseBuffLogicPassingComponent[] logicPassingComponents);
        public abstract BuffApplyResultEnum ReceiveBuff_TryApplyBuff(
            RolePlay_BuffTypeEnum buffType,
            I_RP_Buff_ObjectCanApplyBuff caster,
            I_RP_Buff_ObjectCanReceiveBuff receiver,
            List<BaseBuffLogicPassingComponent> logicPassingComponents);

        public abstract bool CurrentDataValid();
        public abstract Vector3 GetBuffReceiverPosition();
        
        public abstract RolePlay_ArtHelperBase ReceiveBuff_GetRelatedArtHelper();
        public abstract BuffRemoveResultEnum ReceiveBuff_RemoveTargetBuff(RolePlay_BuffTypeEnum type);

        public abstract LocalActionBus ReceiveBuff_GetRelatedActionBus();

        public abstract BuffAvailableType ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum type);
        public abstract BaseRPBuff ReceiveBuff_GetTargetBuff(RolePlay_BuffTypeEnum type,bool createWhenNotExist = false);
        public abstract bool ReceiveBuff_CheckExistValidBuffWithTag(RP_BuffInternalFunctionFlagTypeEnum type);

        public abstract Float_RPDataEntry ReceiveBuff_GetFloatDataEntry(
            RP_DataEntry_EnumType entryType,
            bool allowNotExist = false);
        public abstract FloatPresentValue_RPDataEntry ReceiveBuff_GetFloatPresentValue(RP_DataEntry_EnumType type);

        public virtual I_RP_ContainVFXContainer GetRelatedVFXContainer()
        {
            return ReceiveBuff_GetRelatedArtHelper() as I_RP_ContainVFXContainer;
        }


    }
}


