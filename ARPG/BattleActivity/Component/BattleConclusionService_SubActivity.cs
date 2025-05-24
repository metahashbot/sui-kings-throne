using System;
using Global.ActionBus;
using Global;
using Sirenix.OdinInspector;
using ARPG.Character.Enemy;
using RPGCore.UtilityDataStructure;
using ARPG.Character;
namespace ARPG.Manager.Component
{
    [TypeInfoBox("掉落物管理")]
    [Serializable]
    public class BattleConclusionService_SubActivity : BaseSubActivityService
    {
        [LabelText("消耗时间(s)")]
        public float CostTime;
        [LabelText("击杀敌人计数")]
        public int KillCount;
        [LabelText("累计收到伤害")]
        public float TakenDamage;

        /// <summary>
        /// 系统初始化，绑定事件
        /// </summary>
        public void AwakeInitialize()
        {
            GlobalActionBus.GetGlobalActionBus().RegisterAction(
                ActionBus_ActionTypeEnum.G_EnemyBehaviour_OnEnemyDieToCorpse_敌人死亡到尸体,
                OnEnemyDie);
        }

        public void LateLoadInitialize()
        {
            CostTime = 0f;
            KillCount = 0;
            TakenDamage = 0f;

            var characterList = SubGameplayLogicManager_ARPG.Instance
                .PlayerCharacterBehaviourControllerReference.CurrentAllCharacterBehaviourList;
            foreach (var character in characterList)
            {
                character.GetRelatedActionBus().RegisterAction(
                    ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_DamageTakenOnHP_对接收方伤害打到了HP上,
                    OnTakenDamage);
            }
        }

        public override void UpdateTick(float ct, int cf, float delta)
        {
            CostTime += delta;
        }

        private void OnEnemyDie(DS_ActionBusArguGroup ds)
        {
            if(ds.ObjectArgu1 is EnemyARPGCharacterBehaviour enemy)
            {
                // 从配置表生成的敌人没有关联Config
                if (enemy.RelatedSpawnConfigInstance == null)
                {
                    KillCount++;
                    return;
                }
                // 从SOConfig生成的需要检查是否是真的敌人
                else if (!enemy.RelatedSpawnConfigInstance.EnemySpawnConfigTypeHandler.IfNotCountInLevelClear)
                {
                    KillCount++;
                }
            }
        }

        private void OnTakenDamage(DS_ActionBusArguGroup ds)
        {
            if(ds.ObjectArgu1 is RP_DS_DamageApplyResult damage)
            {
                if (damage.Receiver is PlayerARPGConcreteCharacterBehaviour)
                {
                    TakenDamage += damage.DamageResult_TakenOnHP;
                }
            }
        }

        void OnDestroy()
        {
            GlobalActionBus.GetGlobalActionBus().RemoveAction(
                ActionBus_ActionTypeEnum.G_EnemyBehaviour_OnEnemyDieToCorpse_敌人死亡到尸体,
                OnEnemyDie);

            GlobalActionBus.GetGlobalActionBus().RemoveAction(
                ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_DamageTakenOnHP_对接收方伤害打到了HP上,
                OnTakenDamage);
        }
    }
}