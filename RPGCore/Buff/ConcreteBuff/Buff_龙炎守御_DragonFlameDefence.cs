using ARPG.Character;
using Global.ActionBus;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using System;
using System.Collections.Generic;
using ARPG.Manager;
using Global;
using Global.Audio;
using RPGCore.Projectile;
using RPGCore.Projectile.Layout;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

namespace RPGCore.Buff.ConcreteBuff
{
    public class Buff_龙炎守御_DragonFlameDefence : BaseRPBuff
    {

        [SerializeField, LabelText("初始化时层数")]
        [TitleGroup("===具体配置===")]
        private int _initStackCount = 5;
        
        [SerializeField,LabelText("每层提供的免伤百分比")]
        [TitleGroup("===具体配置===")]
        private float _defenceMultiplier = 0.12f;
         
        [ShowInInspector,LabelText("当前层数"), FoldoutGroup("运行时")]
         public int CurrentStackCount { get; private set; }


        [SerializeField,LabelText("音频任务_龙卷风的")]
        private Config_二动态位置音频任务_TwoDynamicPositionTask _audioTaskConfig;






        [SerializeField, LabelText("layout_炎卷版面_1")]
        [TitleGroup("===具体配置===")]
        private List<SOConfig_ProjectileLayout> _fireStormLayoutConfigList = new List<SOConfig_ProjectileLayout>();

        private List<SOConfig_ProjectileLayout> _runtimeLayoutList = new List<SOConfig_ProjectileLayout>();

        
        //
        // [SerializeField,LabelText("刷新炎卷的时间间隔")]
        // [TitleGroup("===具体配置===")]
        // private float _fireStormRefreshInterval = 20f;
        //
        // [SerializeField, LabelText("炎卷生成距离玩家")]
        // [TitleGroup("===具体配置===")]
        // private float _fireStormSpawnDistance = 15f;



        private class PerStormInfo
        {
            public PerVFXInfo RelatedInfo;
        }
        
        

        public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
        {
            var ds = base.OnBuffInitialized(blps);
            CurrentStackCount = _initStackCount;

            Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RegisterAction(
                ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeRearMul_对接收方将要进行后乘最终算区计算,
                _ABC_ProcessFireDamageReduce_OnDamageFrontAddCalculate);
            for (int i = 0; i < _fireStormLayoutConfigList.Count; i++)
            {
                _runtimeLayoutList.Add(_fireStormLayoutConfigList[i].SpawnLayout(
                   Parent_SelfBelongToObject as I_RP_Projectile_ObjectCanReleaseProjectile,
                   false));
            }
            GlobalActionBus.GetGlobalActionBus().RegisterAction(
                ActionBus_ActionTypeEnum.G_Audio_AudioPlayWithBroadcast_音频播放并广播,
                _ABC_AddAudioTask_OnAudioPlay);

            return ds;
        }

        private void _ABC_AddAudioTask_OnAudioPlay(DS_ActionBusArguGroup ds)
        {
            var audioID = ds.ObjectArguStr as string;
            if (!string.Equals(audioID, _audioTaskConfig.AudioBroadcastID))
            {
                return;
            }
            if (ds.ObjectArgu2 is ProjectileBehaviour_Runtime pb)
            {
                if (_runtimeLayoutList.Exists((layout =>
                    layout.LayoutHandlerFunction.CheckIfProjectileIsFromThisLayoutHandler(pb))))
                {
                    GeneralAudioManager.Instance.AddNewAudioTask_DynamicPositionAndTransform(
                        ds.GetObj1AsT<AudioSource>(),
                        SubGameplayLogicManager_ARPG.Instance.PlayerCharacterBehaviourControllerReference
                            .GetCurrentActivePlayerPositionFunc(),
                        pb.RelatedGORef.transform,
                        _audioTaskConfig.VolumeOverDistanceAbsoluteVolumeMul,
                        audioID);
                }
            }
        }
        
        private void _ABC_ProcessFireDamageReduce_OnDamageFrontAddCalculate(DS_ActionBusArguGroup ds)
        {
            var dar = ds.GetObj1AsT<RP_DS_DamageApplyResult>();
            dar.CP_DamageAmount_DPart.MultiplyPart -= CurrentStackCount * (_defenceMultiplier / 100f);
        }

        
        
        public override void FromHolder_FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
        {
            base.FromHolder_FixedUpdateTick(currentTime, currentFrameCount, delta);
        }

        protected override void ClearAndUnload()
        {
            GeneralAudioManager.Instance.StopAudioTaskByTaskID(_audioTaskConfig.AudioBroadcastID);
            base.ClearAndUnload();
            Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
                ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeRearMul_对接收方将要进行后乘最终算区计算,
                _ABC_ProcessFireDamageReduce_OnDamageFrontAddCalculate);
        }


        protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
        {
            base.ProcessPerBLP(blp);
            switch (blp)
            {
                case BLP_龙炎守御层数修饰信息_ModifyValueOnFlameDenfense blp_modifyFlame:
                    CurrentStackCount += blp_modifyFlame.ModifyValue;
                    CurrentStackCount = Math.Clamp(CurrentStackCount, 0, 10);
                    break;
            }
        }

        [Serializable]
        public class BLP_龙炎守御层数修饰信息_ModifyValueOnFlameDenfense : BaseBuffLogicPassingComponent
        {
            [SerializeField]
            public int ModifyValue = -1;

            public override void ReleaseOnReturnToPool()
            {
                GenericPool<BLP_龙炎守御层数修饰信息_ModifyValueOnFlameDenfense>.Release(this);
            }
        }
    }
}