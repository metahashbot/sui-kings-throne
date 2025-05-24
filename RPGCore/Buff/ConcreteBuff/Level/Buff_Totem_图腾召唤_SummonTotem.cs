using System;
using System.Collections.Generic;
using ARPG.Character;
using ARPG.Character.Enemy;
using GameplayEvent;
using GameplayEvent.SO;
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
    public class Buff_Totem_图腾召唤_SummonTotem : BaseRPBuff
    {
        [LabelText("需要触发的事件们")]
        private List<SOConfig_PrefabEventConfig> _triggerGameplayEventList = new List<SOConfig_PrefabEventConfig>();


        [LabelText("触发的间隔时长"), SuffixLabel("秒")]
        private float _triggerInterval;

        private float _nextTriggerTime;

        private string _relatedAreaID;
        
        [SerializeField, LabelText("释放时特效"), FoldoutGroup("配置", true), TitleGroup("配置/VFX"),
         GUIColor(187f / 255f, 1f, 0f)]
        protected string _vfx_release;

        public override void Init(
            RPBuff_BuffHolder buffHolderRef,
            SOConfig_RPBuff configRuntimeInstance,
            SOConfig_RPBuff configRawTemplate,
            I_RP_Buff_ObjectCanReceiveBuff parent,
            I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
        {
            base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
             _nextTriggerTime = BaseGameReferenceService.CurrentFixedTime;
            parent.ReceiveBuff_GetRelatedActionBus().RegisterAction(
                ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeTakeToHP_对接收方将要对HP进行伤害计算,
                _ABC_DamageTruncate,
                999);
        }

        protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
        {
            base.ProcessPerBLP(blp);
            switch (blp)
            {
                case BLP_召唤图腾的配置信息_SummonTotemConfig summonTotemConfig:
                    foreach (var perEvent in summonTotemConfig._triggerGameplayEventList)
                    {
                        _triggerGameplayEventList.Add(perEvent);
                    }

                    _triggerInterval = summonTotemConfig._triggerInterval;
                    _nextTriggerTime = BaseGameReferenceService.CurrentFixedTime + _triggerInterval;
                    _relatedAreaID = summonTotemConfig.RelatedAreaID;
                    break;
            }
        }


        public override void FromHolder_FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
        {
            base.FromHolder_FixedUpdateTick(currentTime, currentFrameCount, delta);


            if (GetBuffCurrentAvailableType() != BuffAvailableType.Available_TimeInAndMeetRequirement)
            {
                return;
            }

            if (currentTime <= _nextTriggerTime)
            {
                return;
            }
            _nextTriggerTime = currentTime + _triggerInterval;
            int sameAreaEnemyCount = 0;
            foreach (var perBehaviour in _characterOnMapManagerRef.CurrentAllActiveARPGCharacterBehaviourCollection)
            {
                if (perBehaviour is not EnemyARPGCharacterBehaviour enemy)
                {
                    continue;
                }

                if (enemy.RelatedSpawnConfigInstance.RelatedSpawnEnemyGameplayEventHandlerRef.AreaUID.Equals(
                        _relatedAreaID, StringComparison.OrdinalIgnoreCase))
                {
                    sameAreaEnemyCount += 1;
                    break;
                }
            }

            if (sameAreaEnemyCount == 0)
            {
                return;
            }

       
            var ds_event = new DS_GameplayEventArguGroup();
            ds_event.ObjectArgu1 = Parent_SelfBelongToObject;
            ds_event.ObjectArguStr = _relatedAreaID;
            foreach (SOConfig_PrefabEventConfig perPrefab in _triggerGameplayEventList)
            {
                GameplayEventManager.Instance.StartGameplayEvent(perPrefab, ds_event);
            }
            _VFX_GetAndSetBeforePlay(_vfx_release)?._VFX__10_PlayThis();
            
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
                    player.ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum.Level_速度反召唤图腾_SpeedBonusAntiSummon);
                if (checkR == BuffAvailableType.NotExist)
                {
                    return;
                }

                var buff =
                    player.ApplyBuff_GetRelatedBuff(RolePlay_BuffTypeEnum.Level_速度反召唤图腾_SpeedBonusAntiSummon) as
                        Buff_AntiTotem_速度反召唤图腾_SpeedBonus;
                dar.DamageAmount_AfterShield = buff.CurrentStackCount;
            }
        }

        public override DS_ActionBusArguGroup OnBuffPreRemove()
        {
            Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
                ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeTakeToHP_对接收方将要对HP进行伤害计算,
                _ABC_DamageTruncate);
            return base.OnBuffPreRemove();
        }


        [Serializable]
        public class BLP_召唤图腾的配置信息_SummonTotemConfig : BaseBuffLogicPassingComponent
        {
            [SerializeField, LabelText("需要触发的事件们 ")]
            public List<SOConfig_PrefabEventConfig> _triggerGameplayEventList = new List<SOConfig_PrefabEventConfig>();

            [SerializeField, LabelText("触发的间隔时长"), SuffixLabel("秒")]
            public float _triggerInterval;

            [SerializeField, LabelText("生成归属区域ID")]
            public string RelatedAreaID;

            public override void ReleaseOnReturnToPool()
            {
                GenericPool<BLP_召唤图腾的配置信息_SummonTotemConfig>.Release(this);
            }
        }
    }
}