using System;
using System.Collections.Generic;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RPGCore.Skill.Config.Effector
{
    [Serializable]
    public class ConSer_RPSkillEffector_EffectorOnSelector :RPSkill_EffectorBase
    {
        [LabelText("Selector会选到自己吗?(OnSelf的Effector无视此配置)")]
        public bool AreaEffectOnSelf = false;
    
        public bool ContainDamageEffector;
        [LabelText("拥有Damage作用器"),ShowIf(nameof(ContainDamageEffector))] 
        public List<ConSer_DamageApplyInfo> DamageApplyInfo;
    
    
    
    
        [LabelText("拥有数据项目作用器，根据 来源/目标 的 值/(某个数据项的)百分比 影响某个数据项")]
        public bool ContainDataEntryEffector = false;
    
        [ShowIf(nameof(ContainDataEntryEffector))] 
        public List<ConSer_DataEntryEffect> DataEntryEffectorList;

        [LabelText("拥有buff效果的作用器")] 
        public bool ContainBuffEffector = false;
        [ShowIf(nameof(ContainBuffEffector))] 
        public List<ConSer_BuffApplyInfo> BuffEffectorList;

        [LabelText("拥有强制位移效果的作用器")]
        public bool ContainForceDisplacementEffector = false;

        [ShowIf(nameof(ContainForceDisplacementEffector))]
        public List<RP_DS_ForceDisplacementEffector> ForceDisplacementEffector;

        [LabelText("是否拥有对Dispatcher作用的效果")]
        public bool ContainDispatcherEffector;

        [ShowIf(nameof(ContainDispatcherEffector))]
        public List<ConSer_BuffApplyInfo> DispatcherBuffEffectorList;


    }
}
namespace RPGCore.Skill.Config.Effector
{
}