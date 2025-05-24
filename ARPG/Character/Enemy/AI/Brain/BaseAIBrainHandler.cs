using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using ARPG.Character.Base;
using ARPG.Character.Base.CustomSpineData;
using ARPG.Character.Enemy.AI.BehaviourPattern;
using ARPG.Character.Enemy.AI.Decision;
using ARPG.Character.Enemy.AI.Decision.ConcreteDecision;
using ARPG.Character.Enemy.AI.Decision.DecisionComponent;
using ARPG.Character.Enemy.AI.Listen;
using ARPG.Character.Player.Ally;
using ARPG.Manager;
using GameplayEvent.Handler.StructureUtility;
using Global;
using Global.ActionBus;
using Global.Utility;
using RPGCore;
using RPGCore.Buff;
using RPGCore.Buff.ConcreteBuff.Common;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure.Config;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Pool;
using Object = UnityEngine.Object;
namespace ARPG.Character.Enemy.AI
{
    [Serializable]
    public class BaseAIBrainHandler
    {


#if UNITY_EDITOR
        [OnInspectorGUI]
        private void RedrawConstantly() { Sirenix.Utilities.Editor.GUIHelper.RequestRepaint(); }

        private bool ContainAIDebugLocalFile = false;
        private void _Editor_CheckIfHasAIBrainLocalDebugFile()
        {
                var rawContent = UnityEditor.EditorPrefs.GetString(nameof(Debug_AIBrainLocalDebugFile), string.Empty);
                if (string.IsNullOrEmpty(rawContent))
                {
                    return;
                }
                Debug_AIBrainLocalDebugFile   CurrentAIBrainLocalDebugFile = JsonUtility.FromJson<Debug_AIBrainLocalDebugFile>(rawContent);
                if (!CurrentAIBrainLocalDebugFile.Enabled)
                {
                    return;
                }

                if (!string.Equals(CurrentAIBrainLocalDebugFile.BrainName,
                        SelfRelatedAIBrainConfigRuntimeInstance.ConfigContent.BrainTypeID))
                {
                    return;
                }

                SOConfig_AIDecision newTempDecision = ScriptableObject.CreateInstance<SOConfig_AIDecision>();
                newTempDecision.DecisionHandler = new DH_空动画决策_EmptyAnimationDecision();
                newTempDecision.ConfigContent = new DecisionContentInConfig();
                newTempDecision.ConfigContent.DCCConfigInfo = new DCCConfigInfo();
                newTempDecision.ConfigContent.DCCConfigInfo.CCList_Decision = new List<BaseDecisionCommonComponent>();
                var tmpNewAddToGroup =new DCC_按组加入决策至队列_AddDecisionGroupToQueue();
                tmpNewAddToGroup.DecisionNodes = new List<DCC_按组加入决策至队列_AddDecisionGroupToQueue.NodeInfo>();
                newTempDecision.ConfigContent.DecisionID = "AI本地调试";
                if (!string.IsNullOrEmpty(CurrentAIBrainLocalDebugFile.DID_Name1))
                {
                    var node1 = new DCC_按组加入决策至队列_AddDecisionGroupToQueue.NodeInfo
                    {
                        NodeToDecision = CurrentAIBrainLocalDebugFile.DID_Name1,
                        ChoicePickWeightList = new List<BaseChoicePickWeight_基本选取权重>()
                        {
                            new BaseChoicePickWeight_基本选取权重(100)
                        },
                        Leafs = null
                    };
                    tmpNewAddToGroup.DecisionNodes.Add( node1);
                    if (!string.IsNullOrEmpty(CurrentAIBrainLocalDebugFile.DID_Name2))
                    {
                        node1.Leafs = new List<DCC_按组加入决策至队列_AddDecisionGroupToQueue.NodeInfo>();
                        var node2 = new DCC_按组加入决策至队列_AddDecisionGroupToQueue.NodeInfo
                        {
                            NodeToDecision = CurrentAIBrainLocalDebugFile.DID_Name2,
                            ChoicePickWeightList = new List<BaseChoicePickWeight_基本选取权重>()
                            {
                                new BaseChoicePickWeight_基本选取权重(100)
                            },
                            Leafs = null
                        };
                        node1.Leafs.Add(node2);
                        
                        if (!string.IsNullOrEmpty(CurrentAIBrainLocalDebugFile.DID_Name3))
                        {
                            node2.Leafs = new List<DCC_按组加入决策至队列_AddDecisionGroupToQueue.NodeInfo>();
                            var node3 = new DCC_按组加入决策至队列_AddDecisionGroupToQueue.NodeInfo
                            {
                                NodeToDecision = CurrentAIBrainLocalDebugFile.DID_Name3,
                                ChoicePickWeightList = new List<BaseChoicePickWeight_基本选取权重>()
                                {
                                    new BaseChoicePickWeight_基本选取权重(100)
                                },
                                Leafs = null
                            };
                            node2.Leafs.Add(node3);
                            if (!string.IsNullOrEmpty(CurrentAIBrainLocalDebugFile.DID_Name4))
                            {
                                node3.Leafs = new List<DCC_按组加入决策至队列_AddDecisionGroupToQueue.NodeInfo>();
                                var node4 = new DCC_按组加入决策至队列_AddDecisionGroupToQueue.NodeInfo
                                {
                                    NodeToDecision = CurrentAIBrainLocalDebugFile.DID_Name4,
                                    ChoicePickWeightList = new List<BaseChoicePickWeight_基本选取权重>()
                                    {
                                        new BaseChoicePickWeight_基本选取权重(100)
                                    },
                                    Leafs = null
                                };
                                node3.Leafs.Add(node4);
                                if (!string.IsNullOrEmpty(CurrentAIBrainLocalDebugFile.DID_Name5))
                                {
                                    node4.Leafs = new List<DCC_按组加入决策至队列_AddDecisionGroupToQueue.NodeInfo>();
                                    var node5 = new DCC_按组加入决策至队列_AddDecisionGroupToQueue.NodeInfo
                                    {
                                        NodeToDecision = CurrentAIBrainLocalDebugFile.DID_Name5,
                                        ChoicePickWeightList = new List<BaseChoicePickWeight_基本选取权重>()
                                        {
                                            new BaseChoicePickWeight_基本选取权重(100)
                                        },
                                        Leafs = null
                                    };
                                    node4.Leafs.Add(node5);
                                }
                            }
                        }
                    }
                    
                }

                newTempDecision.ConfigContent.DCCConfigInfo.CCList_Decision.Add(tmpNewAddToGroup);

                newTempDecision.DecisionHandler.InjectRawConfigReference(newTempDecision, newTempDecision);
                

                SelfRelatedAIBrainConfigRuntimeInstance.ConfigContent.InternalPublicDecisionList.Add(newTempDecision);
                
                ContainAIDebugLocalFile = true;

        }

        private IEnumerator Editor_TryDCC()
        {
            yield return null;
            
            if (ContainAIDebugLocalFile)
            { 
                CurrentActiveBehaviourPattern.DecisionList.Add(FindSelfDecisionByString("AI本地调试"));        
                var newDCC_White = new DCC_黑白名单调整决策自主推演可用性_ExceptionToggleDecisionAutoDeduce();
            newDCC_White._toggleToOn = true;
            newDCC_White._relatedDecisionNames = new List<string>();
            newDCC_White._relatedDecisionNames.Add("AI本地调试");
            newDCC_White.EnterComponent(SelfRelatedAIBrainConfigRuntimeInstance);
            }
        }
        
        
        
        
#endif

        [SerializeField, LabelText("【特效】：关联的特效配置文件"), FoldoutGroup("配置",true),ListDrawerSettings(
            ShowFoldout = true,
            DefaultExpandedState = true)]
        [Searchable]
        public List<SOConfig_PresetVFXInfoGroup> AllVFXInfoList_File = new List<SOConfig_PresetVFXInfoGroup>();

        [NonSerialized, LabelText("关联的特效信息们 - 运行时所有"),
         FoldoutGroup("运行时", false, VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()")]
        public List<PerVFXInfo> AllVFXInfoList_RuntimeAll;

        protected List<SOConfig_PresetVFXInfoGroup>
            _runtimeVFXInfoGroupSOList = new List<SOConfig_PresetVFXInfoGroup>();

        public float AIBrainStartRunningTime { get; protected set; }


        //清理时销毁内容
        [NonSerialized]
        public List<SOConfig_PresetSideEffects> RuntimeSideEffectList = new List<SOConfig_PresetSideEffects>();
        
        

        public BaseARPGCharacterBehaviour GetCurrentHatredTarget()
        {
            if (!_currentHatredTarget)
            {
                RefreshHatredTarget();
            }
            return _currentHatredTarget;
        }
        [ShowInInspector, LabelText("当前仇恨目标"),
         FoldoutGroup("运行时", false, VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()"), GUIColor(0.85f, 0f, 0f)]
        protected BaseARPGCharacterBehaviour _currentHatredTarget;

// #if UNITY_EDITOR
//         [Button("添加一个【循环异常】"), HorizontalGroup("配置/A"), FoldoutGroup("配置", true)]
//         private void _Button_AddLoopAbnormal()
//         {
//             if (!SelfAllPresetAnimationInfoList.Exists(info => info.ConfigName.Equals("循环异常")))
//             {
//                 SelfAllPresetAnimationInfoList.Add(new SheetAnimationInfo_帧动画配置
//                 {
//                     ConfigName = "循环异常",
//                 });
//             }
//         }
//         [Button("添加一个【不循环异常】"), HorizontalGroup("配置/A"), FoldoutGroup("配置", true)]
//         private void _Button_AddNonLoopAbnormal()
//         {
//             if (!SelfAllPresetAnimationInfoList.Exists(info => info.ConfigName.Equals("不循环异常")))
//             {
//                 SelfAllPresetAnimationInfoList.Add(new SheetAnimationInfo_帧动画配置
//                 {
//                     ConfigName = "不循环异常",
//                 });
//             }
//         }
//
// #endif


        [Space(30)]
        [SerializeField, LabelText("【动画】：关联的动画配置文件"), FoldoutGroup("配置", true),ListDrawerSettings(ShowFoldout = true,DefaultExpandedState = true)]
        public List<SOConfig_PresetAnimationInfoBase> SelfAllPresetAnimationInfoListFile =
            new List<SOConfig_PresetAnimationInfoBase>();

        protected List<SOConfig_PresetAnimationInfoBase> _animationInfoSORuntimeList =
            new List<SOConfig_PresetAnimationInfoBase>();



        [NonSerialized, LabelText("预设动画信息 - 运行时所有"),
         FoldoutGroup("运行时", false, VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()")]
        public List<AnimationInfoBase> SelfAllPresetAnimationInfoList_RuntimeAll;





        public AnimationInfoBase GetRelatedAnimationInfo(string name)
        {
            int findIndex = SelfAllPresetAnimationInfoList_RuntimeAll.FindIndex(x =>
                string.Equals(name, x.ConfigName, StringComparison.OrdinalIgnoreCase));
            if (findIndex == -1)
            {
                DBug.LogError(
                    $"决策查找动画{name}失败，找不到对应的动画信息。来源脑:{SelfRelatedAIBrainConfigRuntimeInstance.name},归属角色:{SelfRelatedARPGCharacterBehaviour.name}");
            }
            return SelfAllPresetAnimationInfoList_RuntimeAll[findIndex];
        }




        #region Static

        protected static CharacterOnMapManager _characterOnMapManagerRef;
        protected static ActivityManager_ARPG Ref;
        protected static GlobalActionBus _globalActionBusRef;

        public static void StaticInitialize()
        {
            _characterOnMapManagerRef = SubGameplayLogicManager_ARPG.Instance.CharacterOnMapManagerReference;
            Ref = SubGameplayLogicManager_ARPG.Instance.ActivityManagerArpgInstance;
            _globalActionBusRef = GlobalActionBus.GetGlobalActionBus();
            ;
        }

        #endregion

        [ShowInInspector, LabelText("当前正活跃的【行为模式】"), FoldoutGroup("运行时", true),
         InlineEditor(InlineEditorObjectFieldModes.Boxed)]
        public SOConfig_AIBehaviourPattern CurrentActiveBehaviourPattern { get; protected set; }


        [ShowInInspector, LabelText("上一个活跃的【行为模式】"), FoldoutGroup("运行时", true),
         InlineEditor(InlineEditorObjectFieldModes.Boxed), ReadOnly]
        public SOConfig_AIBehaviourPattern LastActiveBehaviourPattern { get; protected set; }



        [ShowInInspector, LabelText("当前Brain正在运转？"), FoldoutGroup("运行时", true), NonSerialized]
        public bool CurrentBrainActive = false;

        [ShowInInspector, LabelText("关联的BrainConfig"), FoldoutGroup("运行时", true), NonSerialized]
        public SOConfig_AIBrain SelfRelatedAIBrainConfigRuntimeInstance;


        [ShowInInspector, LabelText("关联的角色Behaviour"), FoldoutGroup("运行时", true), NonSerialized]
        public BaseARPGCharacterBehaviour SelfRelatedARPGCharacterBehaviour;


        [ShowInInspector, NonSerialized, LabelText("关联事件线"), FoldoutGroup("运行时", true)]
        public LocalActionBus SelfLocalActionBusRef;


        /// <summary>
        /// <para>决策队列，实际上是按照列表的数据结构存储的</para>
        /// </summary>
        [ShowInInspector, NonSerialized, LabelText("当前决策队列"), FoldoutGroup("运行时", true)]
        public List<SOConfig_AIDecision> CurrentDecisionQueueList;

        protected bool _queueClearLocked = false;
        
        public void EnableQueueClearLock()
        {
            _queueClearLocked = true;
        }

        public void DisableQueueClearLock()
        {
            _queueClearLocked = false;
        }
        

        [ShowInInspector, NonSerialized, LabelText("上一个决策"), FoldoutGroup("运行时", true)]
        public SOConfig_AIDecision LastDecisionRef;


        [ShowInInspector, LabelText("当前正在运转的决策"), FoldoutGroup("运行时", true)]
        public SOConfig_AIDecision CurrentRunningDecision;


        /// <summary>
        /// <para>当前占用等级</para>
        /// </summary>
        [ShowInInspector, FoldoutGroup("运行时", true), LabelText("当前决策占用等级_仅用于自行推演")]
        public float CurrentRunningDecisionOccupationLevel { get; protected set; }


        /// <summary>
        /// <para>能否自行推演。 当可以推演的时候，总是会在到时间点的情况下试图推演。</para>
        /// <para>有些决策的具体内容会阻塞推演，比如一些没有具体时长的决策，这样的决策通常是会自行解除阻塞 | 或者自行直接处理后续的决策业务 </para>
        /// </summary>
        [ShowInInspector, FoldoutGroup("运行时", true), LabelText("当前能否自行推演"), NonSerialized]
        public bool CurrentBrainAbleToAutoDeduce = true;


        [ShowInInspector, LabelText("下次能够自行推演的时间点"), FoldoutGroup("运行时", true)]
        public float AbleToAutoDeduceTime { get; protected set; }




        /// <summary>
        /// <para>一个决策候选，在进行【自主决策】的时候，会把所有当前 【有可能】 被执行的决策加入候选容器中，然后按照加权随机的方式进行决策</para>
        /// <para>如果是强制执行，或者是决策队列有后续决策，则不会自主决策</para>
        /// </summary>
        public class DecisionCandidate
        {
            public float Weight;

            public SOConfig_AIDecision RelatedDecisionRef;

        }

        public List<DecisionCandidate> SelfDecisionCandidateList;



        /// <summary>
        /// <para>调用源是Behaviour在初始化时，对着运行时Instance调用的，所以还需要传入原始模板</para>
        /// </summary>
        /// <param name="behaviour"></param>
        /// <param name="rawConfig"></param>
        /// <param name="brainRuntimeInstance"></param>
        public virtual void Initialize(
            BaseARPGCharacterBehaviour behaviour,
            SOConfig_AIBrain rawConfig,
            SOConfig_AIBrain brainRuntimeInstance)
        {
            _currentHatredTarget = null;
            

            AIBrainStartRunningTime = BaseGameReferenceService.CurrentFixedTime;
            //构建动画信息。这个是不需要深拷贝的
            SelfAllPresetAnimationInfoList_RuntimeAll = new List<AnimationInfoBase>();
            foreach (SOConfig_PresetAnimationInfoBase perFile in SelfAllPresetAnimationInfoListFile)
            {
                //来自文件的需要把SO重新拷一份
                var newObj = UnityEngine.Object.Instantiate(perFile);
                _animationInfoSORuntimeList.Add(newObj);
                foreach (AnimationInfoBase perInfo in newObj.SelfAllPresetAnimationInfoList)
                {
                    SelfAllPresetAnimationInfoList_RuntimeAll.Add(perInfo);
                }
            }
            // foreach (AnimationInfoBase perInfo in SelfAllPresetAnimationInfoList)
            // {
            //     SelfAllPresetAnimationInfoList_RuntimeAll.Add(perInfo);
            // }
            //构建特效信息，这个是不需要深拷贝的
            AllVFXInfoList_RuntimeAll = new List<PerVFXInfo>();
            foreach (SOConfig_PresetVFXInfoGroup perFile in AllVFXInfoList_File)
            {
                var newOBj = UnityEngine.Object.Instantiate(perFile);
                _runtimeVFXInfoGroupSOList.Add(newOBj);
                foreach (PerVFXInfo perInfo in newOBj.PerVFXInfoList)
                {
                    AllVFXInfoList_RuntimeAll.Add(perInfo);
                }
            }



            if (behaviour is AllyARPGCharacterBehaviour)
            {
                CurrentHatredSearchType = SearchType.SearchEnemy_搜索敌人;
            }
            else if (behaviour is EnemyARPGCharacterBehaviour)
            {
                CurrentHatredSearchType = SearchType.SearchPlayerAndAlly_搜索玩家和玩家友军;
            }
            SelfRelatedARPGCharacterBehaviour = behaviour;
            SelfRelatedAIBrainConfigRuntimeInstance = brainRuntimeInstance;
            SelfRelatedAIBrainConfigRuntimeInstance.RawAIBrainAssetTemplate = rawConfig;


            SelfRelatedAIBrainConfigRuntimeInstance.ConfigContent.InternalPublicDecisionList.Clear();
#if UNITY_EDITOR
            _Editor_CheckIfHasAIBrainLocalDebugFile();
#endif


            SelfLocalActionBusRef = behaviour.GetRelatedActionBus();
            CurrentBrainActive = true;
            LastDecisionRef = null;
            CurrentRunningDecision = null;

            CurrentDecisionQueueList = CollectionPool<List<SOConfig_AIDecision>, SOConfig_AIDecision>.Get();
            CurrentDecisionQueueList.Clear();

            SelfDecisionCandidateList = CollectionPool<List<DecisionCandidate>, DecisionCandidate>.Get();
            SelfDecisionCandidateList.Clear();

            CurrentRunningDecisionOccupationLevel = 0;
            CurrentBrainAbleToAutoDeduce = true;
            AbleToAutoDeduceTime = 0;


            //初始化所有行为模式
            SelfRelatedAIBrainConfigRuntimeInstance.ConfigContent.BehaviourPatternList =
                new List<SOConfig_AIBehaviourPattern>();
            var behaviourPatternList = SelfRelatedAIBrainConfigRuntimeInstance.ConfigContent.BehaviourPatternList;

            foreach (SOConfig_AIBehaviourPattern perPattern in rawConfig.ConfigContent.BehaviourPatternList)
            {
                SOConfig_AIBehaviourPattern newPattern = UnityEngine.Object.Instantiate(perPattern);
                newPattern.RelatedBrainRef = SelfRelatedAIBrainConfigRuntimeInstance;
                newPattern.IsCurrentActivePattern = false;
                behaviourPatternList.Add(newPattern);
                newPattern.DecisionList = new List<SOConfig_AIDecision>();
                foreach (SOConfig_AIDecision perRawDecision in perPattern.DecisionList)
                {
                    if(perRawDecision == null)
                    {
                        continue;
                    }
                    var newDecision = UnityEngine.Object.Instantiate(perRawDecision);             
                    newDecision.DecisionHandler.InjectRawConfigReference(perRawDecision, newDecision);

                    newDecision.DecisionHandler.Initialize(brainRuntimeInstance);
                    newPattern.DecisionList.Add(newDecision);
                }
                if (perPattern.ListenList != null)
                {
                    newPattern.ListenList = new List<SOConfig_AIListen>();
                    foreach (SOConfig_AIListen perRawListen in perPattern.ListenList)
                    {
                        SOConfig_AIListen newListenInstance = UnityEngine.Object.Instantiate(perRawListen);
                        newPattern.ListenList.Add(newListenInstance);
                    }
                }
            }
            foreach (var perDecision in rawConfig.ConfigContent.InternalPublicDecisionList)
            {
                var newDecision = UnityEngine.Object.Instantiate(perDecision);
                newDecision.DecisionHandler.InjectRawConfigReference(perDecision, newDecision);
                SelfRelatedAIBrainConfigRuntimeInstance.ConfigContent.InternalPublicDecisionList.Add(newDecision);
            }

            foreach (var perDecision in SelfRelatedAIBrainConfigRuntimeInstance.ConfigContent.InternalPublicDecisionList)
            {
                perDecision.DecisionHandler.Initialize(brainRuntimeInstance);
            }



            var alwaysListenList = SelfRelatedAIBrainConfigRuntimeInstance.ConfigContent.AlwaysListenList =
                new List<SOConfig_AIListen>();
            foreach (SOConfig_AIListen perRawListen in rawConfig.ConfigContent.AlwaysListenList)
            {
                alwaysListenList.Add(UnityEngine.Object.Instantiate((perRawListen)));
            }



            //深拷贝所有决策

            RegisterAllListen();




            SelfLocalActionBusRef.RegisterAction(
                ActionBus_ActionTypeEnum.L_AIDecision_DecisionRequireBrainAutoDeducePostpone_由决策要求推迟Brain的自动推演时间点,
                _ABC_PostponeAutoDeduce_RequireFromDecision);
            SelfLocalActionBusRef.RegisterAction(
                ActionBus_ActionTypeEnum.L_AIDecision_DecisionRequireBrainBlockAutoDeduce_由决策要求阻塞Brain的自动推演,
                _ABC_BlockAutoDeduce_RequireFromDecision);
            SelfLocalActionBusRef.RegisterAction(
                ActionBus_ActionTypeEnum.L_AIDecision_DecisionRequirePreemptDecision_由决策要求抢占式执行决策,
                _ABC_PreemptDecision_RequireFromDecision);
            SelfLocalActionBusRef.RegisterAction(
                ActionBus_ActionTypeEnum.L_AIDecision_DecisionRequireBrainReleaseAutoDeduce_由决策要求释放Brain的自动推演,
                _ABC_ReleaseAutoDeduce_RequireFromDecision);
            SelfLocalActionBusRef.RegisterAction(
                ActionBus_ActionTypeEnum.L_AIDecision_DecisionRequireAutoDeduce_由决策要求立刻进行自行推演,
                _ABC_TryAutoDeduce_RequireFromDecision);
            SelfLocalActionBusRef.RegisterAction(
                ActionBus_ActionTypeEnum.L_AIDecision_DecisionRequireQueueDecision_由决策要求排队一个决策,
                _ABC_EnqueueDecision_RequireFromDecision);
            SelfLocalActionBusRef.RegisterAction(
                ActionBus_ActionTypeEnum.L_AISideEffect_OnSideEffectRequireAutoDeduce_AI副作用要求自主推演,
                _ABC_TryAutoDeduce_RequireFromSideEffect);
            _globalActionBusRef = GlobalActionBus.GetGlobalActionBus();

            _globalActionBusRef.RegisterAction(ActionBus_ActionTypeEnum.G_AllyBehaviour_OnAllyDieDirect_友军直接死亡没有尸体,
                _ABC_CheckIfNeedRefreshHatredTarget_OnAnyBehaviourDie);
            _globalActionBusRef.RegisterAction(ActionBus_ActionTypeEnum.G_AllyBehaviour_OnAllyDieToCorpse_友军死亡到尸体,
                _ABC_CheckIfNeedRefreshHatredTarget_OnAnyBehaviourDie);
            _globalActionBusRef.RegisterAction(ActionBus_ActionTypeEnum.G_EnemyBehaviour_OnEnemyDieDirect_敌人直接死亡没有尸体,
                _ABC_CheckIfNeedRefreshHatredTarget_OnAnyBehaviourDie);
            _globalActionBusRef.RegisterAction(ActionBus_ActionTypeEnum.G_EnemyBehaviour_OnEnemyDieToCorpse_敌人死亡到尸体,
                _ABC_CheckIfNeedRefreshHatredTarget_OnAnyBehaviourDie);
            _globalActionBusRef.RegisterAction(ActionBus_ActionTypeEnum.G_PC_OnCurrentUsingCharacterChanged_当前使用的角色更换,
                _ABC_CheckIfRefreshHatredTarget_OnCurrentActivePlayerCharacterChanged);

#if UNITY_EDITOR
            behaviour.StartCoroutine(Editor_TryDCC());
#endif

        }



        public virtual void AddNewSingleAIListen(SOConfig_AIListen listen)
        {
            var newListenRuntime = UnityEngine.Object.Instantiate(listen);
            SelfRelatedAIBrainConfigRuntimeInstance.ConfigContent.AlwaysListenList.Add(newListenRuntime);
            newListenRuntime.ListenComponent.InitializeAndProcessRegister(SelfRelatedAIBrainConfigRuntimeInstance);
        }


        /// <summary>
        /// <para>根据运行时深拷贝后的ListenList，来注册相关的各类监听</para>
        /// </summary>
        protected virtual void RegisterAllListen()
        {
            if (SelfRelatedAIBrainConfigRuntimeInstance.ConfigContent.AlwaysListenList == null ||
                SelfRelatedAIBrainConfigRuntimeInstance.ConfigContent.AlwaysListenList.Count == 0)
            {
                return;
            }
            foreach (SOConfig_AIListen perListen in SelfRelatedAIBrainConfigRuntimeInstance.ConfigContent
                .AlwaysListenList)
            {
                if (perListen.ListenComponent == null)
                {
                    continue;
                }
                perListen.ListenComponent.InitializeAndProcessRegister(SelfRelatedAIBrainConfigRuntimeInstance);
            }
        }







        public virtual void _ABC_GeneralCallback_RegisteredByListen(DS_ActionBusArguGroup ds)
        {
            var actionType = ds.ActionType;
        }




        public virtual void UpdateTick(float ct, int cf, float delta)
        {
            for (int i = AllVFXInfoList_RuntimeAll.Count - 1; i >= 0; i--)
            {
                AllVFXInfoList_RuntimeAll[i].UpdateTick(ct, cf, delta);
            }
            if (!CurrentBrainActive)
            {
                return;
            }
        }


        public virtual void FixedUpdateTick(float ct, int cf, float delta)
        {
            if (!CurrentBrainActive)
            {
                return;
            }
            foreach (var perListen in SelfRelatedAIBrainConfigRuntimeInstance.ConfigContent.AlwaysListenList)
            {
                if (perListen.ListenComponent is I_AIListenNeedTick tick)
                {
                    tick.FixedUpdateTick(ct, cf, delta);
                }
            }

            if (CurrentActiveBehaviourPattern.ListenList != null)
            {
                foreach (var perListen in CurrentActiveBehaviourPattern.ListenList)
                {
                    if (perListen.ListenComponent is I_AIListenNeedTick tick)
                    {
                        tick.FixedUpdateTick(ct, cf, delta);
                    }
                }
            }


            if (CurrentRunningDecision != null)
            {
                CurrentRunningDecision.DecisionHandler.FixedUpdateTick(ct, cf, delta);
            }


            if (IfCanAutoDeduce())
            {
                ProcessAutoDeduce();
            }
        }

        #region 自主推演

        /// <summary>
        /// <para>检查能否自我推演</para>
        /// </summary>
        protected virtual bool IfCanAutoDeduce()
        {
            //自行推演被阻塞，返回
            if (!CurrentBrainAbleToAutoDeduce)
            {
                return false;
            }
            //还没到点，返回
            if (BaseGameReferenceService.CurrentFixedTime < AbleToAutoDeduceTime)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// <para>执行自我推演</para>
        /// </summary>
        protected virtual void ProcessAutoDeduce()
        {
            if (!CurrentBrainActive)
            {
                DBug.Log(
                    $" {SelfRelatedARPGCharacterBehaviour.name}的AIBrian{SelfRelatedAIBrainConfigRuntimeInstance.name}" +
                    $"在自主推演的时候，当前Brain已经不活跃了，不再进行自主推演");
            }
            //如果可以进行推演了，那就说明当前正在执行的那个决策已经停了
            //那么此时如果队列里还有东西，那就拿出来就行。
            //队列里还有东西，那就按队列的走，返回
            if (CurrentDecisionQueueList.Count > 0)
            {
                DBug.Log($"{SelfRelatedARPGCharacterBehaviour.name}正在尝试自我推演\n" +
                    $"执行{CurrentDecisionQueueList[0].name}\n");
                CurrentRunningDecision = CurrentDecisionQueueList[0];
                CurrentDecisionQueueList.RemoveAt(0);
                CurrentRunningDecision.DecisionHandler.OnDecisionBeforeStartExecution();
                return;
            }




            //清理所有Candidate
            foreach (DecisionCandidate perCandidate in SelfDecisionCandidateList)
            {
                GenericPool<DecisionCandidate>.Release(perCandidate);
            }
            SelfDecisionCandidateList.Clear();



            //当前阻塞程度


            foreach (SOConfig_AIDecision perDecision in CurrentActiveBehaviourPattern.DecisionList)
            {
                //如果这不是一个可参与自主推演的决策，跳过
                if (!perDecision.ConfigContent.CanAutoDeduce)
                {
                    continue;
                }
                //如果目标决策 在选取时的占用级过低，低于了当前的占用级，则表明它在这种情况下不能被选取，则也跳过
                if (perDecision.ConfigContent.OccupationLevel_Pick < CurrentRunningDecisionOccupationLevel)
                {
                    continue;
                }
                //加入候选
                var newCandidate = GenericPool<DecisionCandidate>.Get();
                newCandidate.RelatedDecisionRef = perDecision;
                float targetWeight = perDecision.DecisionHandler.GetPickWeightAtAutoDeduce();

                //如果目标权重接近0了，则不会将其加入候选，这样无论如何都选不到0权的决策
                if (Mathf.Approximately(targetWeight, 0f))
                {
                    continue;
                }
                newCandidate.Weight = targetWeight;
                SelfDecisionCandidateList.Add(newCandidate);
            }

            //如果是0，这其实并不合理，因为怎么样也能选个Idle啥的，
            if (SelfDecisionCandidateList.Count == 0)
            {
#if UNITY_EDITOR
                DBug.LogError(
                    $"{SelfRelatedARPGCharacterBehaviour.name}的AIBrian{SelfRelatedAIBrainConfigRuntimeInstance.name}" +
                    $"在自主推演的时候，可用决策数量为0，这不合理，检查一下");
#endif
                return;
            }

            DecisionCandidate picked = ShuffleAndGetCandidates();

#if UNITY_EDITOR
            //if (CurrentBrainActive && SelfRelatedARPGCharacterBehaviour != null &&
            //    SelfRelatedARPGCharacterBehaviour.CharacterDataValid)
            //{
            //    string log =
            //        $"{SelfRelatedARPGCharacterBehaviour.name}正在尝试自我推演，从{SelfDecisionCandidateList.Count}个候选中选择了{picked.RelatedDecisionRef.name}，当前可用所有候选是";
            //    foreach (DecisionCandidate perDecision in SelfDecisionCandidateList)
            //    {
            //        if (perDecision.RelatedDecisionRef.ConfigContent.CanAutoDeduce)
            //        {
            //            log += $"{perDecision.RelatedDecisionRef.name}(当前权{perDecision.Weight})； ";
            //        }
            //    }
            //    DBug.Log(log);
            //}
#endif


            AddDecisionToQueue(picked.RelatedDecisionRef, DecisionEnqueueType.AutoDeduce_自主推演);
        }


        protected virtual DecisionCandidate ShuffleAndGetCandidates()
        {
            List<KeyValuePair<int, float>> assistList =
                CollectionPool<List<KeyValuePair<int, float>>, KeyValuePair<int, float>>.Get();
            float totalWeight = 0f;

            foreach (DecisionCandidate thisDecisionCandidate in SelfDecisionCandidateList)
            {
                totalWeight += thisDecisionCandidate.Weight;
            }
            for (int i = 0; i < SelfDecisionCandidateList.Count; i++)
            {
                float r = UnityEngine.Random.Range(0f, totalWeight);
                assistList.Add(new KeyValuePair<int, float>(i, r + SelfDecisionCandidateList[i].Weight));
            }
            assistList.Sort(((p1, p2) =>
            {
                if (p1.Value < p2.Value)
                {
                    return 1;
                }
                else if (p1.Value > p2.Value)
                {
                    return -1;
                }
                else return 0;
            }));
            DecisionCandidate returnItem = SelfDecisionCandidateList[assistList[0].Key];
            assistList.Clear();
            CollectionPool<List<KeyValuePair<int, float>>, KeyValuePair<int, float>>.Release(assistList);
            return returnItem;
        }

        #endregion

        #region 决策执行


        public enum DecisionEnqueueType
        {
            None = 0,
            JustEnqueue_加入排队 = 1,
            FullClearAndEnqueueFirst_清空并置首 = 2,
            AutoDeduce_自主推演 = 3,
            EnqueueToSecond_加入排队但是清空后续 = 4,
            BreakAndEnqueueFirst_打断清空并置首 = 5,
        }


        public virtual void AddDecisionToQueue(string decisionName, DecisionEnqueueType enqueueType)
        {
            AddDecisionToQueue(FindSelfDecisionByString(decisionName), enqueueType);
        }
        private static List<SOConfig_AIDecision> _listToInsert = new List<SOConfig_AIDecision>();

        public virtual void InsertDecisionToQueue(List<string> toInsert)
        {
            if (!CurrentBrainActive)
            {
                DBug.Log(
                    $"AIDebug: {SelfRelatedAIBrainConfigRuntimeInstance.name}由决策【加入队列】，但是当前Brain已经不活跃了，不再进行加入队列");
                return;
            }
            _listToInsert.Clear();
            for (int i = 0; i < toInsert.Count; i++)
            {
                _listToInsert.Add(FindSelfDecisionByString(toInsert[i]));

            }

            CurrentDecisionQueueList.InsertRange(0, _listToInsert);


#if UNITY_EDITOR
            string logS = $"正在向AIBrain{SelfRelatedAIBrainConfigRuntimeInstance.name}【插入】决策们，它们分别是：";

            for (int i = 0; i < toInsert.Count; i++)
            {
                logS += $"{toInsert[i]} 、";

            }
            DBug.Log(logS);

#endif

        }

        /// <summary>
        /// <para>将决策加入队列。</para>
        /// <para>首先总是会触发 L_AIBrain_OnDecisionPickedToQueue_当决策被选取到队列 。</para>
        /// <para>然后根据 DecisionEnqueueType ，是仅仅加入队列 |或| 清空并置首(L_AIDecision_OnDecisionBreak_当决策被打断)</para>
        /// <para>【置首】指的是现在立刻执行，把之前的Current打断，然后把Current设置过来，然后还会清除队列。要执行的Decision不会进到队列</para>
        /// <para>返回的是刚刚被操作的decisionRuntime。如果没加成功就会是null</para>
        /// </summary>
        public virtual SOConfig_AIDecision AddDecisionToQueue(
            SOConfig_AIDecision decisionRuntime,
            DecisionEnqueueType enqueueType,
            bool stillAddOnInactive = false)
        {
            if (!CurrentBrainActive && !stillAddOnInactive)
            {
                DBug.Log(
                    $"AIDebug: {SelfRelatedAIBrainConfigRuntimeInstance.name}由决策{decisionRuntime.name}【加入队列】，但是当前Brain已经不活跃了，不再进行加入队列");
                return null;
            }
            var dsQueue =
                new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_AIBrain_OnDecisionPickedToQueue_当决策被选取到队列);
            dsQueue.ObjectArgu1 = decisionRuntime;
            dsQueue.IntArgu1 = (int)enqueueType;
            SelfLocalActionBusRef.TriggerActionByType(dsQueue);
            decisionRuntime.DecisionHandler.BeforePickedToQueueByBrain();


            switch (enqueueType)
            {
                case DecisionEnqueueType.JustEnqueue_加入排队:
                        CurrentDecisionQueueList.Add(decisionRuntime);
               
                    break;
                case DecisionEnqueueType.EnqueueToSecond_加入排队但是清空后续:
                    if (!_queueClearLocked)
                    {
                        CurrentDecisionQueueList?.Clear();
                    }
#if UNITY_EDITOR
                    else
                    {
                        DBug.Log($" AIDebug: {SelfRelatedAIBrainConfigRuntimeInstance.name}由决策{decisionRuntime.name}【加入排队但是清空后续】，但启用了[队列清除锁]，不会清除队列");
                    }
#endif
                    CurrentDecisionQueueList.Add(decisionRuntime);
                    break;
                case DecisionEnqueueType.FullClearAndEnqueueFirst_清空并置首:
                    if (!_queueClearLocked)
                    {
                        CurrentDecisionQueueList?.Clear();
                    }
#if UNITY_EDITOR
                    else
                {
                    DBug.Log(
                        $" AIDebug: {SelfRelatedAIBrainConfigRuntimeInstance.name}由决策{decisionRuntime.name}【清空并置首】，但启用了[队列清除锁]，不会清除队列");
                }
#endif

                    if (CurrentRunningDecision != null && CurrentRunningDecision.DecisionHandler.DecisionRunningNormal)
                    {
                        CurrentRunningDecision.DecisionHandler.OnDecisionRunningBreak(decisionRuntime);
                    }
                    CurrentRunningDecision = decisionRuntime;
                    CurrentRunningDecision.DecisionHandler.OnDecisionBeforeStartExecution();
                    break;
                case DecisionEnqueueType.AutoDeduce_自主推演:
                    if (CurrentRunningDecision != null && CurrentRunningDecision.DecisionHandler.DecisionRunningNormal)
                    {
                        CurrentRunningDecision.DecisionHandler.OnDecisionNormalComplete();
                    }
                    CurrentRunningDecision = decisionRuntime;
                    CurrentRunningDecision.DecisionHandler.OnDecisionBeforeStartExecution();
                    break;
                case DecisionEnqueueType.BreakAndEnqueueFirst_打断清空并置首:
                    if (CurrentRunningDecision != null && CurrentRunningDecision.DecisionHandler.DecisionRunningNormal)
                    {
                        CurrentRunningDecision.DecisionHandler.OnDecisionRunningBreak(decisionRuntime);
                    }
                    CurrentRunningDecision = decisionRuntime;
                    CurrentRunningDecision.DecisionHandler.OnDecisionBeforeStartExecution();
                    if (!_queueClearLocked)
                    {
                        CurrentDecisionQueueList?.Clear();
                    }
#if UNITY_EDITOR
                    else
                {
                    DBug.Log(
                        $" AIDebug: {SelfRelatedAIBrainConfigRuntimeInstance.name}由决策{decisionRuntime.name}【打断清空并置首】，但启用了[队列清除锁]，不会清除队列");
                }
#endif

                    break;

            }

#if UNITY_EDITOR

            //switch (enqueueType)
            //{
            //    case DecisionEnqueueType.JustEnqueue_加入排队:
            //        DBug.Log(
            //            $"AIDebug: {SelfRelatedAIBrainConfigRuntimeInstance.name}【排队】了决策{decisionRuntime.name}，队列目前还有{CurrentDecisionQueueList.Count}个决策");
            //        break;
            //    case DecisionEnqueueType.FullClearAndEnqueueFirst_清空并置首:
            //        DBug.Log(
            //            $"AIDebug: {SelfRelatedAIBrainConfigRuntimeInstance.name}【插队】了决策{decisionRuntime.name}，清空了队列");
            //        break;
            //    case DecisionEnqueueType.AutoDeduce_自主推演:
            //        DBug.Log(
            //            $"AIDebug: {SelfRelatedAIBrainConfigRuntimeInstance.name}【自主推演】了决策{decisionRuntime.name}，清空了队列");
            //        break;
            //    case DecisionEnqueueType.BreakAndEnqueueFirst_打断清空并置首:
            //        DBug.Log(
            //            $" AIDebug: {SelfRelatedAIBrainConfigRuntimeInstance.name}由决策{decisionRuntime.name}【打断】了，队列没有变化，后续还有{CurrentDecisionQueueList.Count}个决策");
            //        break;
            //}
#endif
            return decisionRuntime;
        }

        #endregion

        #region 决策结束

        /*
		 * 对于决策结束，通常都是来源于决策本身要求的。
		 */

        #endregion
        #region 仇恨

        /// <summary>
        /// <para>内部用于计算仇恨目标的容器。循环使用，所以不要存它的引用</para>
        /// </summary>
        protected static List<HatredTarget> _internalHatredTargetList = new List<HatredTarget>();

        protected struct HatredTarget
        {
            public readonly BaseARPGCharacterBehaviour Target;
            public float HatredValue;
            public HatredTarget(BaseARPGCharacterBehaviour target) : this()
            {
                Target = target;
            }
        }

        protected void _ABC_CheckIfNeedRefreshHatredTarget_OnAnyBehaviourDie(DS_ActionBusArguGroup ds)
        {
            var diedBehaviour = ds.ObjectArgu1 as BaseARPGCharacterBehaviour;
            if (System.Object.ReferenceEquals(diedBehaviour, _currentHatredTarget))
            {
                RefreshHatredTarget();
            }
        }

        protected void _ABC_CheckIfRefreshHatredTarget_OnCurrentActivePlayerCharacterChanged(DS_ActionBusArguGroup ds)
        {
            var lastCharacter = ds.ObjectArgu2 as BaseARPGCharacterBehaviour;
            RefreshHatredTarget();
        }
        public enum SearchType
        {
            SearchEnemy_搜索敌人 = 0, SearchPlayerAndAlly_搜索玩家和玩家友军 = 1, SearchOnlyPlayer_仅搜索玩家 = 2,
        }
        public SearchType CurrentHatredSearchType { get; protected set; }
        /// <summary>
        /// <para>刷新仇恨目标</para>
        /// </summary>
        public void RefreshHatredTarget()
        {
            _internalHatredTargetList.Clear();

            foreach (BaseARPGCharacterBehaviour perBehaviour in _characterOnMapManagerRef
                .CurrentAllActiveARPGCharacterBehaviourCollection)
            {
                if (!perBehaviour.gameObject.activeInHierarchy)
                {
                    continue;
                }
                if (!perBehaviour.CharacterDataValid)
                {
                    continue;
                }
                switch (CurrentHatredSearchType)
                {
                    case SearchType.SearchEnemy_搜索敌人:
                        if (perBehaviour is not EnemyARPGCharacterBehaviour enemy)
                        {
                            continue;
                        }
                        break;
                    case SearchType.SearchPlayerAndAlly_搜索玩家和玩家友军:
                        if (perBehaviour is EnemyARPGCharacterBehaviour)
                        {
                            continue;
                        }
                        break;
                    case SearchType.SearchOnlyPlayer_仅搜索玩家:
                        if (perBehaviour is not PlayerARPGConcreteCharacterBehaviour)
                        {
                            continue;
                        }
                        break;
                }

                float hatredValue = 0f;
                HatredTarget ht = new HatredTarget(perBehaviour);
                if (perBehaviour.ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum.Taunt_嘲讽_CF) ==
                    BuffAvailableType.Available_TimeInAndMeetRequirement)
                {
                    var tauntBuffRef =
                        (perBehaviour.ReceiveBuff_GetTargetBuff(RolePlay_BuffTypeEnum.Taunt_嘲讽_CF) as Buff_Taunt);
                    //距离检查
                    if (Vector3.SqrMagnitude(SelfRelatedARPGCharacterBehaviour.transform.position -
                                             perBehaviour.transform.position) > tauntBuffRef.CurrentTauntRange *
                        tauntBuffRef.CurrentTauntRange)
                    {
                        continue;
                    }
                    else
                    {
                        hatredValue += tauntBuffRef.CurrentTauntToThreatValue;
                    }
                }

                if (perBehaviour.ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum.ThreatReduce_威胁降低_WXJD) ==
                    BuffAvailableType.Available_TimeInAndMeetRequirement)
                {
                    hatredValue -=
                        (perBehaviour.ReceiveBuff_GetTargetBuff(RolePlay_BuffTypeEnum.ThreatReduce_威胁降低_WXJD) as
                            Buff_ThreatReduce).CurrentTauntToThreatValue;
                }
                ht.HatredValue = hatredValue;
                _internalHatredTargetList.Add(ht);
            }
            _internalHatredTargetList.Shuffle();
            _internalHatredTargetList.Sort((x, y) => x.HatredValue.CompareTo(y.HatredValue));


            if (_internalHatredTargetList.Count > 0)
            {
                var newHatredTarget = _internalHatredTargetList[0].Target;
                if (newHatredTarget == _currentHatredTarget)
                {
                    _currentHatredTarget = newHatredTarget;
                }
                else
                {
                    //发生了变动，触发事件
                    var ds_htChanged =
                        new DS_ActionBusArguGroup(
                            ActionBus_ActionTypeEnum.L_AIDecision_OnHatredTargetChanged_当仇恨目标发生了变换);
                    ds_htChanged.ObjectArgu1 = newHatredTarget;
                    ds_htChanged.ObjectArgu2 = _currentHatredTarget;
                    SelfLocalActionBusRef.TriggerActionByType(ds_htChanged);

                    //之前是空的，现在有了，触发新获得的事件
                    if (_currentHatredTarget == null)
                    {
                        var ds_becomeValid = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
                            .L_AIDecision_OnHatredTargetBecomeValid_当仇恨目标变为有效目标);

                        ds_becomeValid.ObjectArgu1 = newHatredTarget;
                        SelfLocalActionBusRef.TriggerActionByType(ds_becomeValid);
                    }
                    _currentHatredTarget = newHatredTarget;
                }
            }
            else
            {
                if (_currentHatredTarget != null)
                {
                    //丢掉了
                    var ds_htLost = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
                        .L_AIDecision_OnHatredTargetBecomeInvalid_当仇恨目标变为无效目标);
                    SelfLocalActionBusRef.TriggerActionByType(ds_htLost);
                }
                _currentHatredTarget = null;
            }
        }

        #endregion


        #region 行为模式

        /// <summary>
        /// <para>选取默认行为模式。通常这是由于敌人刚刚生成的时候，需要根据一些情况来选择一开始的行为模式</para>
        /// <para>如果生成时没有额外指定，这里就会找所有pattern中第一个被标记为Default的</para>
        /// </summary>
        public virtual void PickDefaultBehaviourPattern()
        {
            foreach (SOConfig_AIBehaviourPattern perPattern in SelfRelatedAIBrainConfigRuntimeInstance.ConfigContent
                .BehaviourPatternList)
            {
                if (perPattern.CommonFlag.HasFlag(AIBehaviourPatternCommonFlag
                    .AsDefaultPatternIfNotSpecific_作为默认模式如果未显式指定其他))
                {
                    SwitchBehaviourPattern(perPattern);
                    return;
                }
            }
        }



        public virtual void SwitchBehaviourPattern(string targetPattern)
        {
            var find = FindBehaviourPatternByUID(targetPattern);
            SwitchBehaviourPattern(find);
        }



        public virtual void SwitchBehaviourPattern(AIBehaviourPatternCommonFlag flag)
        {
            var find = SelfRelatedAIBrainConfigRuntimeInstance.ConfigContent.BehaviourPatternList.FindIndex((pattern =>
                pattern.CommonFlag.HasFlag(flag)));
            if (find == -1)
            {
                throw new ArgumentOutOfRangeException(
                    $"查找【行为模式】包含标签:{flag}时失败，来自脑{SelfRelatedARPGCharacterBehaviour.name}");
            }
            SwitchBehaviourPattern(SelfRelatedAIBrainConfigRuntimeInstance.ConfigContent.BehaviourPatternList[find]);
        }







        public virtual void SwitchBehaviourPattern(SOConfig_AIBehaviourPattern targetPattern)
        {
            if (!CurrentBrainActive)
            {
                DBug.Log($"角色{SelfRelatedARPGCharacterBehaviour.name}正在切换行为模式，但是当前Brain已经不活跃了，不再进行切换");
                return;
            }
            if (!targetPattern.IsAvailable)
            {
                DBug.LogError(
                    $"目标行为模式{targetPattern.name}不可用，这不合理，检查一下，来自脑{SelfRelatedARPGCharacterBehaviour.name}\n开发环境依然进行了切换，但这并不合理");
            }

            if (CurrentActiveBehaviourPattern == null)
            {
                DBug.Log($"角色{SelfRelatedARPGCharacterBehaviour.name}正在切换行为模式切换到{targetPattern.name}");
            }
            else
            {
                DBug.Log(
                    $"角色{SelfRelatedARPGCharacterBehaviour.name}正在切换行为模式从{CurrentActiveBehaviourPattern.name}切换到{targetPattern.name}");
            }
            //清理当前正活跃的pattern
            if (CurrentActiveBehaviourPattern != null)
            {
                CurrentActiveBehaviourPattern.IsCurrentActivePattern = false;


                foreach (var perListen in CurrentActiveBehaviourPattern.ListenList)
                {
                    perListen.ListenComponent.UnRegisterListenInActionBus();
                }
                var ds_exitPattern =
                    new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_AIBrain_OnBehaviourPatternExit_当一个行为模式退出);
                ds_exitPattern.ObjectArgu1 = CurrentActiveBehaviourPattern;
                SelfLocalActionBusRef.TriggerActionByType(ds_exitPattern);
            }
            CurrentDecisionQueueList.Clear();



            //处理新的pattern，触发相关事件
            CurrentActiveBehaviourPattern = targetPattern;
            targetPattern.IsCurrentActivePattern = true;
            var ds_enterPattern =
                new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_AIBrain_OnBehaviourPatternEnter_当一个行为模式进入);
            ds_enterPattern.ObjectArgu1 = CurrentActiveBehaviourPattern;
            SelfLocalActionBusRef.TriggerActionByType(ds_enterPattern);

            if (CurrentActiveBehaviourPattern.ListenList != null)
            {
                foreach (SOConfig_AIListen perListen in CurrentActiveBehaviourPattern.ListenList)
                {
                    perListen.ListenComponent.InitializeAndProcessRegister(SelfRelatedAIBrainConfigRuntimeInstance);
                }
            }

            bool breakFlag = false;
            //处理队列并开始推演
            if (targetPattern.PresetDecisionUIDList != null && targetPattern.PresetDecisionUIDList.Count > 0)
            {
                foreach (string perPresetDecision in targetPattern.PresetDecisionUIDList)
                {
                    SOConfig_AIDecision decisionWaitToAdd = FindSelfDecisionByString(perPresetDecision);
                    if (!breakFlag)
                    {
                        breakFlag = true;
                        CurrentRunningDecision?.DecisionHandler.OnDecisionRunningBreak(decisionWaitToAdd);
                    }
                    AddDecisionToQueue(decisionWaitToAdd, DecisionEnqueueType.JustEnqueue_加入排队);
                }
            }
            ProcessAutoDeduce();
        }


        public SOConfig_AIBehaviourPattern FindBehaviourPatternByUID(string targetID)
        {
            int findIndex = SelfRelatedAIBrainConfigRuntimeInstance.ConfigContent.BehaviourPatternList.FindIndex(
                (pattern => pattern.BehaviourPatternTypeID.Equals(targetID, StringComparison.OrdinalIgnoreCase)));
            if (findIndex == -1)
            {
                DBug.LogError($"查找【行为模式】:{targetID}时失败，来自脑{SelfRelatedARPGCharacterBehaviour.name}");
                return null;
            }
            return SelfRelatedAIBrainConfigRuntimeInstance.ConfigContent.BehaviourPatternList[findIndex];
        }

        #endregion








        #region 基本回调们

        #region 来自决策执行时的要求们

        /// <summary>
        /// <para>正常的推迟</para>
        /// </summary>
        protected virtual void _ABC_PostponeAutoDeduce_RequireFromDecision(DS_ActionBusArguGroup ds)
        {
            var fromDecision = ds.ObjectArgu1 as SOConfig_AIDecision;
            var deduceTime = ds.FloatArgu1.Value;
            AbleToAutoDeduceTime = BaseGameReferenceService.CurrentFixedTime + deduceTime;
        }

        protected virtual void _ABC_BlockAutoDeduce_RequireFromDecision(DS_ActionBusArguGroup ds)
        {
            var fromDecision = ds.ObjectArgu1 as SOConfig_AIDecision;
            CurrentBrainAbleToAutoDeduce = false;
        }

        protected virtual void _ABC_PreemptDecision_RequireFromDecision(DS_ActionBusArguGroup ds)
        {
            var fromDecision = ds.ObjectArgu1 as SOConfig_AIDecision;
            var newDecision = ds.ObjectArgu2 as SOConfig_AIDecision;
            AddDecisionToQueue(newDecision, DecisionEnqueueType.FullClearAndEnqueueFirst_清空并置首);
        }

        protected virtual void _ABC_ReleaseAutoDeduce_RequireFromDecision(DS_ActionBusArguGroup ds)
        {
            var fromDecision = ds.ObjectArgu1 as SOConfig_AIDecision;
            CurrentBrainAbleToAutoDeduce = true;
        }

        protected virtual void _ABC_TryAutoDeduce_RequireFromDecision(DS_ActionBusArguGroup ds)
        {
            int type = ds.IntArgu1.Value;
            if (type == 1 || IfCanAutoDeduce())
            {
                ProcessAutoDeduce();
            }
        }

        protected virtual void _ABC_TryAutoDeduce_RequireFromSideEffect(DS_ActionBusArguGroup ds)
        {
            int type = ds.IntArgu1.Value;
            CurrentRunningDecision?.DecisionHandler.OnDecisionRunningBreak(CurrentRunningDecision);
            if (type == 1 || IfCanAutoDeduce())
            {
                ProcessAutoDeduce();
            }
        }

        
        
        
        protected virtual void _ABC_EnqueueDecision_RequireFromDecision(DS_ActionBusArguGroup ds)
        {
            var fromDecision = ds.ObjectArgu1 as SOConfig_AIDecision;
            var newDecision = ds.ObjectArgu2 as SOConfig_AIDecision;
            AddDecisionToQueue(newDecision, DecisionEnqueueType.JustEnqueue_加入排队);
        }

        #endregion

        #endregion

        #region 清理

        /// <summary>
        /// <para>关联角色Behaviour被销毁前的清理工作</para>
        /// </summary>
        public virtual void ClearBeforeDestroy()
        {
            VFX_GeneralClear(true);

            CurrentDecisionQueueList.Clear();
            CollectionPool<List<SOConfig_AIDecision>, SOConfig_AIDecision>.Release(CurrentDecisionQueueList);
            CurrentDecisionQueueList = null;
            SelfDecisionCandidateList.Clear();
            CollectionPool<List<DecisionCandidate>, DecisionCandidate>.Release(SelfDecisionCandidateList);
            SelfDecisionCandidateList = null;


            if (CurrentActiveBehaviourPattern.ListenList != null)
            {
                foreach (SOConfig_AIListen perListen in CurrentActiveBehaviourPattern.ListenList)
                {
                    perListen.ListenComponent.UnRegisterListenInActionBus();
                    Object.Destroy(perListen);
                }
            }
            foreach (var perBehaviourPattern in SelfRelatedAIBrainConfigRuntimeInstance.ConfigContent
                .BehaviourPatternList)
            {
                perBehaviourPattern.ListenList.Clear();

                foreach (SOConfig_AIDecision perDecisionRuntime in perBehaviourPattern.DecisionList)
                {
                    perDecisionRuntime.DecisionHandler.ClearOnUnload();
                    Object.Destroy(perDecisionRuntime);
                }
                perBehaviourPattern.DecisionList.Clear();
            }
            SelfRelatedAIBrainConfigRuntimeInstance.ConfigContent.BehaviourPatternList.Clear();

            foreach (SOConfig_PresetAnimationInfoBase perConfig in _animationInfoSORuntimeList)
            {
                UnityEngine.Object.Destroy(perConfig);
            }
            _animationInfoSORuntimeList.Clear();

            foreach (var perObj in _runtimeVFXInfoGroupSOList)
            {
                UnityEngine.Object.Destroy(perObj);
            }
            _runtimeVFXInfoGroupSOList.Clear();


            for (int i = 0; i < RuntimeSideEffectList.Count; i++)
            {
                RuntimeSideEffectList[i].DCCConfigInfo.ClearOnUnload();
                Object.Destroy(RuntimeSideEffectList[i]);
            }

            foreach (var perListenRuntime in SelfRelatedAIBrainConfigRuntimeInstance.ConfigContent.AlwaysListenList)
            {
                perListenRuntime.ListenComponent.UnRegisterListenInActionBus();
                Object.Destroy(perListenRuntime);
            }

            SelfRelatedAIBrainConfigRuntimeInstance.ConfigContent.AlwaysListenList.Clear();
            CollectionPool<List<SOConfig_AIListen>, SOConfig_AIListen>.Release(SelfRelatedAIBrainConfigRuntimeInstance
                .ConfigContent.AlwaysListenList);

            _globalActionBusRef.RemoveAction(ActionBus_ActionTypeEnum.G_AllyBehaviour_OnAllyDieDirect_友军直接死亡没有尸体,
                _ABC_CheckIfNeedRefreshHatredTarget_OnAnyBehaviourDie);
            _globalActionBusRef.RemoveAction(ActionBus_ActionTypeEnum.G_AllyBehaviour_OnAllyDieToCorpse_友军死亡到尸体,
                _ABC_CheckIfNeedRefreshHatredTarget_OnAnyBehaviourDie);
            _globalActionBusRef.RemoveAction(ActionBus_ActionTypeEnum.G_EnemyBehaviour_OnEnemyDieDirect_敌人直接死亡没有尸体,
                _ABC_CheckIfNeedRefreshHatredTarget_OnAnyBehaviourDie);
            _globalActionBusRef.RemoveAction(ActionBus_ActionTypeEnum.G_EnemyBehaviour_OnEnemyDieToCorpse_敌人死亡到尸体,
                _ABC_CheckIfNeedRefreshHatredTarget_OnAnyBehaviourDie);
            _globalActionBusRef.RemoveAction(ActionBus_ActionTypeEnum.G_PC_OnCurrentUsingCharacterChanged_当前使用的角色更换,
                _ABC_CheckIfRefreshHatredTarget_OnCurrentActivePlayerCharacterChanged);
        }

        #endregion

        #region 各种查找

        /// <summary>
        /// <para>获取当前正在执行决策的UID(小写）</para>
        /// </summary>
        /// <returns></returns>
        public virtual string GetCurrentDecisionUIDInLower()
        {
            return CurrentRunningDecision.ConfigContent.DecisionID.ToLowerInvariant();
        }


        /// <summary>
        /// <para>试图在当前活动的行为模式中找到首个指定类型的决策</para>
        /// </summary>
        public virtual bool TryGetDecisionRuntimeInstanceByDecisionHandlerInsideCurrentBehaviourPattern<T>(
            out T decisionHandler) where T : BaseDecisionHandler
        {
            if (CurrentActiveBehaviourPattern == null || CurrentActiveBehaviourPattern.DecisionList == null)
            {
                decisionHandler = null;
                return false;
            }
            foreach (var perDecision in CurrentActiveBehaviourPattern.DecisionList)
            {
                if (perDecision.DecisionHandler is T handler)
                {
                    decisionHandler = handler;
                    return true;
                }
            }
            decisionHandler = null;
            return false;
        }

        /// <summary>
        /// <para>按照名字查找决策。可选是否允许公用决策列表？是否允许不存在</para>
        /// </summary>
        /// <param name="str"></param>
        /// <param name="allowInternalPublicDecisionList"></param>
        /// <param name="allowNotExist"></param>
        /// <returns></returns>
        public virtual SOConfig_AIDecision FindSelfDecisionByString(
            string str,
            bool allowInternalPublicDecisionList = true,
            bool allowNotExist = false)
        {
            Debug.Log($"{SelfRelatedAIBrainConfigRuntimeInstance.name}正在查找\n" +
                      $"决策:{str}\n");

            foreach (SOConfig_AIDecision perDecision in CurrentActiveBehaviourPattern.DecisionList)
            {
                if (perDecision.ConfigContent.DecisionID.Equals(str, StringComparison.OrdinalIgnoreCase))
                {
                    return perDecision;
                }
            }

            if (allowInternalPublicDecisionList)
            {
                var f = SelfRelatedAIBrainConfigRuntimeInstance.ConfigContent.InternalPublicDecisionList.FindIndex(
                    (decision => decision.ConfigContent.DecisionID.Equals(str, StringComparison.OrdinalIgnoreCase)));
                if (f != -1)
                {
                    return SelfRelatedAIBrainConfigRuntimeInstance.ConfigContent.InternalPublicDecisionList[f];
                }
            }
#if UNITY_EDITOR
            if (!allowNotExist)
            {
                foreach (var perPattern in SelfRelatedAIBrainConfigRuntimeInstance.ConfigContent.BehaviourPatternList)
                {
                    if (perPattern.DecisionList.Exists((decision =>
                        decision.ConfigContent.DecisionID.Equals(str, StringComparison.OrdinalIgnoreCase))))
                    {
                        DBug.LogError($"在AI脑{SelfRelatedAIBrainConfigRuntimeInstance.name}中查找决策:{str}时，" +
                                      $"没有在当前活跃的行为模式:{CurrentActiveBehaviourPattern.BehaviourPatternTypeID}中找到，" +
                                      $"但是在其他行为模式:{perPattern.BehaviourPatternTypeID}中找到了，这不合理。");
                        return null;
                    }
                }
            }

#endif
            if (!allowNotExist)
            {
                DBug.LogError($"！！！Brain{SelfRelatedAIBrainConfigRuntimeInstance.name}在查找决策【{str}】时未找到，返回空。");

            }
            return null;
        }

        #endregion



        public PerVFXInfo _VFX_GetAndSetBeforePlay(
            string uid,
            bool needApplyTransformOffset = true,
            I_RP_ContainVFXContainer container = null,
            bool withDamageTypeVariant = false,
            PerVFXInfo.GetDamageTypeDelegate getDamageType = null,
            string from = null)
        {
            PerVFXInfo selfVFXInfo = PerVFXInfo._VFX_GetByUID(AllVFXInfoList_RuntimeAll,
                uid,
                withDamageTypeVariant,
                getDamageType,
                from);
            if (selfVFXInfo == null)
            { 
                 DBug.LogError( $" {SelfRelatedARPGCharacterBehaviour.name}的AIBrian{SelfRelatedAIBrainConfigRuntimeInstance.name}" +
                    $"在获取VFX:{uid}时，未找到，返回空。");
                
            }
            return selfVFXInfo._VFX_GetPSHandle( needApplyTransformOffset, container);
        }



        public PerVFXInfo _VFX_JustGet(string uid)
        {
            PerVFXInfo selfVFXInfo = PerVFXInfo._VFX_GetByUID(AllVFXInfoList_RuntimeAll, uid, false);
            return selfVFXInfo;
        }

        public PerVFXInfo _VFX_GetAndSetBeforePlay(
            PerVFXInfo info,
            bool needApplyTransformOffset = true)
        {
            return info._VFX_GetPSHandle(
                needApplyTransformOffset,
                SelfRelatedARPGCharacterBehaviour.GetRelatedVFXContainer());
        }

        protected virtual void VFX_GeneralClear(bool immediate = false)
        {
            PerVFXInfo.VFX_GeneralClear(AllVFXInfoList_RuntimeAll, immediate);
        }
        
        
    }
}