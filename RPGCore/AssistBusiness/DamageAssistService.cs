using System.Collections.Generic;
using Global;
using Global.ActionBus;
using RPGCore.Interface;
using RPGCore.Projectile;
using RPGCore.UtilityDataStructure;
using UnityEngine;
using UnityEngine.Pool;

namespace RPGCore.AssistBusiness
{
    /// <summary>
    /// <para>伤害系统的辅助类。</para>
    /// <para>主要业务：</para>
    /// <para>      伤害的记录</para>
    /// <para>      溅射等复合伤害业务时的辅助</para>
    /// </summary>
    public class DamageAssistService
    {
#region 外部引用

#endregion


        private const float _comboLostDuration = 2.5f;
        private float _currentTime;

        private float _lastComboRecordTime;

        /// <summary>
        /// <para>处理伤害</para>
        /// <para>函数返回前会把使用的DamageApplyInfo释放掉，不要在这个函数结束后使用它了</para>
        /// <para>造成的伤害结果会返回。【一定】须要把它在不使用的时候释放掉。</para>
        /// </summary>
        public RP_DS_DamageApplyResult ApplyDamage(
            RP_DS_DamageApplyInfo damageInfo, int effectStamp = 0)
        {
            effectStamp = effectStamp == 0 ? damageInfo.DamageTimestamp : effectStamp;
            RP_DS_DamageApplyResult damageApplyResult =
                damageInfo.DamageReceiver.ReceiveDamage_ReceiveFromRPDS(damageInfo, effectStamp);
            
            
            

            if (damageApplyResult.ResultLogicalType == RP_DamageResultLogicalType.InvincibleAllSoNothing ||
                damageApplyResult.ResultLogicalType == RP_DamageResultLogicalType.DodgedSoNothing)
            {
            }
            //如果是无敌或者被闪避了，则溅射效果会被吞掉
            //格挡依然是会保留溅射效果的
            else if (damageApplyResult.ResultLogicalType == RP_DamageResultLogicalType.NormalResult ||
                     damageApplyResult.ResultLogicalType == RP_DamageResultLogicalType.ParriedAndOnlyEffect)
            {
                // if (damageInfo.ContainSplash)
                // {
                // }
            }
            if (damageApplyResult.ResultLogicalType != RP_DamageResultLogicalType.InvalidDamage)
            {

            }
            
            
            //230107:跳字和伤害记录都改成了非直接调用而是走一个全局事件 的了，这样解耦
            GlobalActionBus.GetGlobalActionBus().TriggerActionByType(
                ActionBus_ActionTypeEnum.G_DamageAssistService_OnNewDamageEntryGenerated,
                new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_DamageAssistService_OnNewDamageEntryGenerated,
                    damageApplyResult));

            //向GameProcessRecordHelper传递Result，进行伤害记录
            ////TODO Commented 

            // _selfGameProcessRecordHelperImgRef.RecordDamageFromDamageAssistByRPDS(damageApplyResult);

            //TODO Commented 

            //计算连击
            //
            // switch (damageInfo.DamageCaster)
            // {
            //     case BaseProjectileBehaviour baseProjectileBehaviour:
            //         var projectileCaster = baseProjectileBehaviour.GetProjectileCaster();
            //         if (projectileCaster is PlayerBehaviour_InMainGame ||
            //             projectileCaster is Tower_AssassinateDomain_MK1_Behaviour)
            //
            //         {
            //             _currentComboCount += 1;
            //             _selfGameProcessRecordHelperImgRef.SetDamageCombo(_currentComboCount);
            //             _lastComboRecordTime = _currentTime;
            //         }
            //
            //         break;
            //     case PlayerBehaviour_InMainGame playerBehaviourInMainGame:
            //         _currentComboCount += 1;
            //         _selfGameProcessRecordHelperImgRef.SetDamageCombo(_currentComboCount);
            //         _lastComboRecordTime = _currentTime;
            //         break;
            //     case TowerBehaviour_Base towerBehaviourBase:
            //         break;
            // }

            damageInfo.ReleaseBeforeToPool();
            return damageApplyResult;
        }


        public void UpdateTick(float currentTime, int currentFrame, float delta)
        {
            
            
        }
    }
}