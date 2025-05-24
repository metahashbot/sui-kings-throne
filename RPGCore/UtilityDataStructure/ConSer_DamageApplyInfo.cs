using System;
using System.Collections.Generic;
using ARPG.Manager.Config;
using RPGCore.Interface;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

namespace RPGCore.UtilityDataStructure
{
    [Serializable]
    public class ConSer_ForceMovementInDamageApplyInfo
    {
    }

    [Serializable]
    public class ConSer_ForceStiffnessInDamageApplyInfo
    {
    }

    [Serializable]
    public class ConSer_ApplyBuffInDamageApplyInfo
    {
        [LabelText("允许触发的逻辑类型"),EnumToggleButtons]
        public RP_DamageResultLogicalType TriggerLogicalType = RP_DamageResultLogicalType.NormalResult;

        [LabelText("将要施加的Buff类型")]
        public RolePlay_BuffTypeEnum ApplyBuff;

        [LabelText("包含时长覆写吗")]
        public bool ContainDurationOverride;

        [LabelText("重设【存在时长】为"), ShowIf(nameof(ContainDurationOverride))]
        public float ResetExistDurationTo = -1f;
        [LabelText("重设【有效时长】为"), ShowIf(nameof(ContainDurationOverride))]
        public float ResetValidDurationTo = 5f;

    }
    /// <summary>
    /// <para>编辑时可序列化的DamageApplyInfo。区别于RPDS_DamageApplyInfo</para>
    /// <para>常见于各种配置中。实际在运行的时候，使用的是以此构建的运行时RPDS_DamageApplyInfo</para>
    /// <para>当然运行时也会有不使用序列化配置进行的DamageApplyInfo构建，那就与这个无关了</para>
    /// </summary>
    [Serializable]
    public class ConSer_DamageApplyInfo
    {
        [SerializeField]
        [LabelText("伤害类型")]
        public DamageTypeEnum DamageType = DamageTypeEnum.NoType_无属性;

        [SerializeField]
        [LabelText("伤害参与的步骤")]
        public DamageProcessStepOption ProcessOption = DamageProcessStepOption.DefaultDPSO_NormalAttack();
        
        
        

        [SerializeField]
        [LabelText("伤害显式标记来源")]
        public DamageFromTypeFlag DamageFromFlag = DamageFromTypeFlag.None_未指定;

        [SerializeField]
        [LabelText("基础伤害/如果有关联DE则为基础加值")]
        public float DamageTryTakenBase = 1;

        [SerializeField]
        [LabelText("数据项关联"),ToggleButtons("关联角色数值","纯数值")]
        public bool DamageTakenRelatedDataEntry;

        [SerializeField]
        [LabelText("    关联数据项，结果为相加"),ShowIf(nameof(DamageTakenRelatedDataEntry))]
        public List<ConSer_DataEntryRelationConfig> RelatedDataEntryInfos;

        [SerializeField]    
        [LabelText("包含Buff效果吗？")]
        public bool ContainBuffEffect;

        [SerializeField]
        [ShowIf(nameof(ContainBuffEffect)),LabelText("  需要施加的Buff信息=直属")]
        public ConSer_BuffApplyInfo[] BuffEffectArray;


        [SerializeField]
        [ShowIf(nameof(ContainBuffEffect)), LabelText("  需要施加的Buff信息=文件")]
        [InfoBox("新建文件在#SO Assets#/#RPG Core#/预设的Buff施加信息")]
        public SOConfig_BuffApplyInfo[] BuffEffectArray_File;



        public void ReleaseOnReturnToPool()
        {
            
        }
        

    }
}