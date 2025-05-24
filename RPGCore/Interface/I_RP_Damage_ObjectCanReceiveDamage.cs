using ARPG.Character.Base;
using Global.ActionBus;
using RPGCore.Buff;
using RPGCore.DataEntry;
using RPGCore.UtilityDataStructure;
using UnityEngine;

namespace RPGCore.Interface
{
    /// <summary>
    /// <para>表明一个RPObject可以接受来自其他RPObject的Damage</para>
    /// <para>RPBehaviour</para>
    /// <para>其实Dispatcher也可以，但其实只是接受Damage里面包含的BuffEffect。</para>
    /// <para>      这些业务是由DamageAssistService处理的，并不由Dispatcher自身直接实现</para>
    /// </summary>
    public interface I_RP_Damage_ObjectCanReceiveDamage
    {
        public abstract RP_DS_DamageApplyResult ReceiveDamage_ReceiveFromRPDS(RP_DS_DamageApplyInfo applyInfo,int effectIDStamp);

        public abstract Float_RPDataEntry ReceiveDamage_GetRelatedDataEntry(RP_DataEntry_EnumType dataEntryType);
        public abstract FloatPresentValue_RPDataEntry ReceiveDamage_GetRelatedPresentValue(RP_DataEntry_EnumType type);

        public abstract BuffAvailableType ReceiveDamage_CheckTargetBuff(RolePlay_BuffTypeEnum type);
        public abstract BaseRPBuff ReceiveDamage_GetTargetBuff(RolePlay_BuffTypeEnum type);
        /// <summary>
        /// <para>获取当前伤害接受者的位置。如果需要进行额外修正，则覆写该方法。默认直接提供transform.position</para>
        /// </summary>
        public abstract Vector3 ReceiveDamage_GetCurrentReceiverPosition();
        public abstract Vector3 ReceiveDamage_GetCurrentReceiverFaceDirection();
        public abstract bool ReceiveDamage_IfDataValid();


        public abstract BaseARPGArtHelper GetRelatedArtHelper();

        public virtual I_RP_ContainVFXContainer GetRelatedVFXContainer()
        {
            return GetRelatedArtHelper() as I_RP_ContainVFXContainer;
        }

        public LocalActionBus GetRelatedActionBus();

    }
}