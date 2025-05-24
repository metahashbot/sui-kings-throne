using System;
using System.Collections.Generic;
using RPGCore.Buff;
using RPGCore.Buff.BuffHolder;
using RPGCore.Interface;
using RPGCore.Projectile;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

namespace RPGCore.UtilityDataStructure
{

    public struct DamageVFXOverrideInfo
    {
        
        public string OverrideDamageConfigName;

        /// <summary>
        /// <para>True为替代原本的通用受击特效；False为在原本特效额外增加</para>
        /// </summary>
        public bool OverrideOrAdditive;
    }



    /// <summary>
    /// <para>独立算子。简单加区和乘区。结果就是先加再乘</para>
    /// </summary>
    [Serializable]
    public struct CalculationPair_SimpleTwo
    {
        public float AddonPart;
        /// <summary>
        /// <para>这是一个带小数点的数，写成2会让结果翻倍</para>
        /// </summary>
        public float MultiplyPart;


        public static CalculationPair_SimpleTwo Default()
        {
            return new CalculationPair_SimpleTwo()
            {
                AddonPart = 0,
                MultiplyPart = 1f,
            };
        }
        public float GetResult(float origin)
        { 
            return (origin + AddonPart) * MultiplyPart;
        }
    }

    
    /// <summary>
    /// <para>一个伤害的结果。</para>
    /// </summary>
    public class RP_DS_DamageApplyResult
    {
        public I_RP_Damage_ObjectCanReceiveDamage Receiver;
        public I_RP_Damage_ObjectCanApplyDamage Caster;
        /// <summary>
        /// <para>关联的投射物运行时实例，有可能是空的</para>
        /// </summary>
        public ProjectileBehaviour_Runtime RelatedProjectileRuntimeRef;

        public int DamageTimestamp;
        
        public DamageProcessStepOption ProcessOption;
        
        //伤害逻辑类型——正常|被闪避|被无敌等等
        public RP_DamageResultLogicalType ResultLogicalType;
        
        /// <summary>
        /// 伤害的世界坐标
        /// 24.3修改：原来的伤害位置是投射物和受击方命中线段的中点，但实际上并不太应该和投射物有关。改为了在受击方圆周上，根据其碰撞信息计算的 投射物在其圆周上最近点
        /// </summary>
        public Nullable<UnityEngine.Vector3> DamageWorldPosition;
        //伤害的类型——
        public DamageTypeEnum DamageType;

        public DamageFromTypeFlag DamageFromTypeFlags;

        /// <summary>
        /// <para>原始的伤害数值，用于闪避|暴击计算之前</para>
        /// </summary>
        public float DamageRawValueFromDamageApplyInfo;

        public CalculationPair_SimpleTwo CP_Heal;
        public float HealAmount;
        
        
        
        public CalculationPair_SimpleTwo CP_DodgeRate;
        public CalculationPair_SimpleTwo CP_Accuracy;

        /// <summary>
        /// <para>A算区用的那个算子。这是一个加算区。</para>
        /// </summary>
        public CalculationPair_SimpleTwo CP_APart_Defense;
        /// <summary>
        /// 穿甲部分
        /// </summary>
        public CalculationPair_SimpleTwo CP_APart_DefenseIgnore;
        public float DamageAmount_APart;


        public CalculationPair_SimpleTwo CP_SkillPower;
        public float DamageAmount_SkillPowerPart;


        /// <summary>
        /// <para>B算区的用的算子，这是一个乘算区。即结果会直接再乘到之前的计算结果上</para>
        /// </summary>
        public CalculationPair_SimpleTwo CP_DamageAmount_BPart;
        public float DamageAmount_ExtraDamagePart;


        /// <summary>
        /// <para>暴击率算子是一个百分比。即如果此算子的结果为 40，则相当于暴击率额外增加了40%</para>
        /// </summary>
        public CalculationPair_SimpleTwo CP_CriticalRate;
        /// <summary>
        /// 暴击算子将会作为乘数直接运算。即，如果此算子的结果为 0.4，则相当于暴击伤害增加了40%
        /// </summary>
        public CalculationPair_SimpleTwo CP_CriticalBonus;
        public bool IsDamageCauseCritical;
        public float DamageAmount_CriticalPart;
        
        
        



        /// <summary>
        /// <para>D算区的用的算子，这是一个乘算区。即结果会直接再乘到之前的计算结果上</para>
        /// </summary>
        public CalculationPair_SimpleTwo CP_DamageAmount_DPart;
        public float DamageAmount_FinalBonusPart;

        /// <summary>
        /// <para>伤害是否保底</para>
        /// </summary>
        public bool DamageGuarantee;
        

        public float DamageAmount_AfterShield;
        public float DamageResult_TakenOnShield;


        /// <summary>
        /// 伤害是背击的吗？为空表示无法判断（比如并不是投射物造成的伤害），true表示背击
        /// </summary>
        public Nullable<bool> DamageIsFromBack;
        
        public float DamageResult_TakenOnHP;
        
        
        
        public float PopupDamageNumber;
        
        
        //有可能造成死亡。因为常规敌人的死亡记录的时候是不会造成死亡的。
        public bool MayCauseDeath;
        //造成过伤。过伤了的伤害在转向死亡时候会无视“已记录死亡”而直接死亡。
        public bool CauseOverloadDamageEffect;
        //斩杀效果。斩杀效果必定过伤
        public bool CauseEliminateEffect;
        
        public Nullable<DamageVFXOverrideInfo> VFXOverrideInfo;

        public bool CauseBreak;
        
        //包含Buff效果
        public bool ContainBuffEffect;

        public List<(RolePlay_BuffTypeEnum, BuffApplyResultEnum)> ApplyResult;

        /// <summary>
        /// 额外信息，通常用于掉落物
        /// </summary>
        public object DropItemInfoRef;
        

        //reset all content
        public void Reset()
        {

            Receiver = null;
            Caster = null;
            RelatedProjectileRuntimeRef = null;
            DamageTimestamp = 0;
            ResultLogicalType = RP_DamageResultLogicalType.None;
            DamageWorldPosition = null;
            DamageType = DamageTypeEnum.None;
            DamageFromTypeFlags = DamageFromTypeFlag.None_未指定;
            HealAmount = 0f;
            CP_Heal = CalculationPair_SimpleTwo.Default();
            DamageRawValueFromDamageApplyInfo = 0;
            CP_DodgeRate = CalculationPair_SimpleTwo.Default();
            CP_Accuracy = CalculationPair_SimpleTwo.Default();
            CP_APart_Defense =  CalculationPair_SimpleTwo.Default();
            DamageAmount_APart = 0;
            CP_SkillPower = CalculationPair_SimpleTwo.Default();
            DamageAmount_SkillPowerPart = 0;
            CP_DamageAmount_BPart = CalculationPair_SimpleTwo.Default();
            DamageAmount_ExtraDamagePart = 0;
            CP_CriticalRate = CalculationPair_SimpleTwo.Default();
            CP_CriticalBonus = CalculationPair_SimpleTwo.Default();
            IsDamageCauseCritical = false;
            DamageAmount_CriticalPart = 0;
            CP_DamageAmount_DPart = CalculationPair_SimpleTwo.Default();
            DamageAmount_FinalBonusPart = 0;
            
            
            DamageAmount_AfterShield = 0;
            DamageResult_TakenOnShield = 0;
            DamageResult_TakenOnHP = 0;

            PopupDamageNumber = 0;
            MayCauseDeath = false;
            CauseOverloadDamageEffect = false;
            CauseEliminateEffect = false;
            VFXOverrideInfo = null;
            CauseBreak = false;
            ContainBuffEffect = false;
            if (ApplyResult != null)
            {
                ApplyResult.Clear();
            }
            ApplyResult = null;
            
            DropItemInfoRef = null;
        }



        public void AddApplyResultInfo(RolePlay_BuffTypeEnum type, BuffApplyResultEnum result)
        {
            if (ApplyResult == null)
            {
                ApplyResult =
                    CollectionPool<List<(RolePlay_BuffTypeEnum, BuffApplyResultEnum)>, (RolePlay_BuffTypeEnum,
                        BuffApplyResultEnum)>.Get();
            }
            ApplyResult.Add((type, result));
        }

        public void ReleaseToPool()
        {
            GenericPool<RP_DS_DamageApplyResult>.Release(this);
        }
        
    }

    /// <summary>
    /// <para>治疗的计算逻辑类型。用来区分是常规的治疗/无效</para>
    /// </summary>
    [Flags]
    public enum RP_HealApplyResultLogicalType
    {
        None = 0,
        /// <summary>
        /// <para>常规治疗结果</para>
        /// </summary>
        NormalHealResult = 1,
        /// <summary>
        /// <para>无效的治疗，</para>
        /// </summary>
        InvalidHeal = 1 << 4,
    }
    
    
    /// <summary>
    /// <para>伤害的计算逻辑类型。用来区分是常规的伤害/无敌/闪避/格挡的伤害</para>
    /// </summary>
    [Flags]
    public enum RP_DamageResultLogicalType
    {
        None = 0,
        /// <summary>
        /// <para>常规伤害</para>
        /// </summary>
        NormalResult = 1,
        /// <summary>
        /// <para>由于有Invincible_All，所以无事发生</para>
        /// </summary>
        InvincibleAllSoNothing = 1<<1,
        /// <summary>
        /// <para>被闪避了，无事发生</para>
        /// </summary>
        DodgedSoNothing = 1<<2,
        /// <summary>
        /// <para>被格挡了，伤害为0但结算攻击特效</para>
        /// </summary>
        ParriedAndOnlyEffect = 1<<3, 
        
        /// <summary>
        /// <para>无效的伤害，比如友军伤害、重复的伤害</para>
        /// </summary>
        InvalidDamage  = 1<<4,
        
        /// <summary>
        /// <para>被当做治疗了</para>
        /// </summary>
        ActAsHeal = 1<<5,
        
    
    }
}