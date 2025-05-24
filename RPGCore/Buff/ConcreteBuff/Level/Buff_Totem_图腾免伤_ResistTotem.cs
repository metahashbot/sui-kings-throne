using System;
using System.Collections.Generic;
using ARPG.Character;
using ARPG.Character.Base;
using ARPG.Character.Enemy;
using Global;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

namespace RPGCore.Buff.ConcreteBuff.Level
{
    [Serializable]
    public class Buff_Totem_图腾免伤_ResistTotem : BaseRPBuff
    {
        
        [SerializeField, LabelText("每层免伤百分比"), SuffixLabel("%"), TitleGroup("===数值===")]
        private float _resistBonusPerStack = 10f;

        [SerializeField, LabelText("不受影响的敌人TypeID"), TitleGroup("===数值===")]
        private List<CharacterNamedTypeEnum> _ignoreList;
        
        protected string AreaUid;
        
        public override void Init(
            RPBuff_BuffHolder buffHolderRef,
            SOConfig_RPBuff configRuntimeInstance,
            SOConfig_RPBuff configRawTemplate,
            I_RP_Buff_ObjectCanReceiveBuff parent,
            I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
        {
            base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
            foreach (BaseARPGCharacterBehaviour perBehaviour in _characterOnMapManagerRef
                         .CurrentAllActiveARPGCharacterBehaviourCollection)
            {
                if (perBehaviour is not EnemyARPGCharacterBehaviour enemy)
                {
                    continue;
                }

                _Internal_ApplyToEnemy(enemy);
            }

            GlobalActionBus.GetGlobalActionBus()
                .RegisterAction(ActionBus_ActionTypeEnum.G_CharacterOnMap_SpawnNewEnemy_生成了一个新敌人,
                    _ABC_TryApplyBuffToNewEnemy);

            parent.ReceiveBuff_GetRelatedActionBus().RegisterAction(
                ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeTakeToHP_对接收方将要对HP进行伤害计算,
                _ABC_DamageTruncate, 999);
        }

        protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
        {
            base.ProcessPerBLP(blp);
            switch (blp)
            {
                case BLP_免伤图腾的配置信息_ResistTotemConfig blp_resist:
                    _resistBonusPerStack = blp_resist.ResistBonusPerStack;
                    AreaUid = blp_resist.AreaUid;
                    break;
            }
        }


        /// <summary>
        /// <para>图腾的伤害截断效果</para>
        /// </summary>
        /// <param name="ds"></param>
        private void _ABC_DamageTruncate(DS_ActionBusArguGroup ds)
        {
            var dar = ds.GetObj1AsT<RP_DS_DamageApplyResult>();

            dar.DamageAmount_AfterShield = 0f;
            if (dar.Caster is not PlayerARPGConcreteCharacterBehaviour player)
            {
                return;
            }
            else
            {
                //查查玩家成分
                var checkR =
                    player.ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum.Level_防御反免伤图腾_DefenseBonusAntiResist);
                if (checkR == BuffAvailableType.NotExist)
                {
                    return;
                }

                var buff =
                    player.ApplyBuff_GetRelatedBuff(RolePlay_BuffTypeEnum.Level_防御反免伤图腾_DefenseBonusAntiResist) as
                        Buff_AntiTotem_防御反免伤图腾_DefenseBonus;
                dar.DamageAmount_AfterShield = buff.CurrentStackCount;
            }
        }

        private void _ABC_TryApplyBuffToNewEnemy(DS_ActionBusArguGroup ds)
        {
            var enemyRef = ds.GetObj1AsT<EnemyARPGCharacterBehaviour>();

            _Internal_ApplyToEnemy(enemyRef);
        }


        private void _Internal_ApplyToEnemy(EnemyARPGCharacterBehaviour behaviour)
        {
            if (!behaviour.RelatedSpawnConfigInstance.RelatedSpawnEnemyGameplayEventHandlerRef.AreaUID.Equals(AreaUid))
            {
                return;
            }

            if (_ignoreList.Contains(behaviour.SelfBehaviourNamedType))
            {
                return;
            }

            var blp = GenericPool<Buff_AffectByTotem_Resist_受图腾影响免伤.BLP_来自图腾的免伤信息_ResistFromTotem>.Get();
            blp.PerStackResistPercent = _resistBonusPerStack;
            blp.ParentBuffRef = this;
            behaviour.ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.Level_来自图腾的免伤_ResistFromTotem,
                Parent_SelfBelongToObject as I_RP_Buff_ObjectCanApplyBuff,
                behaviour,
                blp);
            GenericPool<Buff_AffectByTotem_Resist_受图腾影响免伤.BLP_来自图腾的免伤信息_ResistFromTotem>.Release(blp);
        }


        public override DS_ActionBusArguGroup OnBuffPreRemove()
        {
            var ds = base.OnBuffPreRemove();
            var allEnemy = _characterOnMapManagerRef.CurrentAllActiveARPGCharacterBehaviourCollection;
            foreach (BaseARPGCharacterBehaviour perBehaviour in allEnemy)
            {
                if (perBehaviour is not EnemyARPGCharacterBehaviour perEnemy)
                {
                    continue;
                }

                var checkR =
                    perBehaviour.ApplyDamage_CheckTargetBuff(RolePlay_BuffTypeEnum.Level_来自图腾的免伤_ResistFromTotem);
                if (checkR != BuffAvailableType.NotExist)
                {
                    var bb =
                        perBehaviour.ApplyBuff_GetRelatedBuff(RolePlay_BuffTypeEnum.Level_来自图腾的免伤_ResistFromTotem) as
                            Buff_AffectByTotem_Resist_受图腾影响免伤;
                    bb.TryRemoveFromTotem(this);
                }
            }

            GlobalActionBus.GetGlobalActionBus()
                .RemoveAction(ActionBus_ActionTypeEnum.G_CharacterOnMap_SpawnNewEnemy_生成了一个新敌人,
                    _ABC_TryApplyBuffToNewEnemy);
            Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
                ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeTakeToHP_对接收方将要对HP进行伤害计算,
                _ABC_DamageTruncate);
            return ds;
        }


        [Serializable]
        public class BLP_免伤图腾的配置信息_ResistTotemConfig : BaseBuffLogicPassingComponent
        {
            [LabelText("每层免伤百分比"), SuffixLabel("%")]
            public float ResistBonusPerStack;
            
            [LabelText("区域UID")]
            public string AreaUid;
            
            public override void ReleaseOnReturnToPool()
            {
                GenericPool<BLP_免伤图腾的配置信息_ResistTotemConfig>.Release(this);
            }
        }
    }
}