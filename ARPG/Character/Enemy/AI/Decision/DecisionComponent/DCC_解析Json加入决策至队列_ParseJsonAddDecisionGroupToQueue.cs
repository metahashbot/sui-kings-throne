using System;
using System.Collections.Generic;
using Global.Utility;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{
    [Serializable]
    public class DCC_解析Json加入决策至队列_ParseJsonAddDecisionGroupToQueue : BaseDecisionCommonComponent
    {
        private enum AddToQueueType
        {
            JustEnqueue_加入排队到后方 = 1, FullClearAndEnqueue_排到首位且清空队列 = 2, BreakAndEnqueue_打断并置首 = 3,
        }

        [SerializeField, LabelText("加入队列的方式"),]
        private AddToQueueType _enqueueType = AddToQueueType.JustEnqueue_加入排队到后方;

        [SerializeField, LabelText("Json数据"), GUIColor(255f / 255f, 223f / 255f, 239f / 255f)]
        public string Json;

        [Serializable]
        public class Decision
        {
            public float Weight;
            public string[] DecisionNodes;
        }

        [Serializable]
        public class DecisionGroup
        {
            public Decision[] Decisions;
        }

        private static List<string> finalList = new List<string>();
        private static List<float> weightList = new List<float>();
        public override void EnterComponent(SOConfig_AIBrain relatedBrain)
        {

            finalList.Clear();

            if (string.IsNullOrEmpty(Json))
            {
                DBug.LogError("决策组为空");
                return;
            }

            var decision = UnityEngine.JsonUtility.FromJson<DecisionGroup>(Json);

            //确保每个节点上一定都能拿出一个决策来
            var currentPointer = decision.Decisions[0];
            //首个节点就是 在DecisionNodes 上选

            weightList.Clear();
            for (int i = 0; i < decision.Decisions.Length; i++)
            {
                weightList.Add(decision.Decisions[i].Weight);
            }

            var indexFirst = ShuffleUtility.ShuffleWeightListAndGetIndex(weightList);
            if (indexFirst == -1)
            {
                DBug.LogError("决策组为空");
                return;
            }
            currentPointer = decision.Decisions[indexFirst];

            for (int i = 0; i < currentPointer.DecisionNodes.Length; i++)
            {
                finalList.Add(currentPointer.DecisionNodes[i]);
            }

            DBug.Log($"AIDebug:{relatedBrain.name}决策组结果，其中决策数量为{finalList.Count}个,分别为 {string.Join(",", finalList)}");
            switch (_enqueueType)
            {
                case AddToQueueType.JustEnqueue_加入排队到后方:
                    for (int i = 0; i < finalList.Count; i++)
                    {
                        relatedBrain.BrainHandlerFunction.AddDecisionToQueue(finalList[i],
                            BaseAIBrainHandler.DecisionEnqueueType.JustEnqueue_加入排队);
                    }
                    break;
                case AddToQueueType.FullClearAndEnqueue_排到首位且清空队列:
                    for (int i = 0; i < finalList.Count; i++)
                    {
                        if (i == 0)
                        {
                            relatedBrain.BrainHandlerFunction.AddDecisionToQueue(finalList[i],
                                BaseAIBrainHandler.DecisionEnqueueType.EnqueueToSecond_加入排队但是清空后续);
                        }
                        else
                        {
                            relatedBrain.BrainHandlerFunction.AddDecisionToQueue(finalList[i],
                                BaseAIBrainHandler.DecisionEnqueueType.JustEnqueue_加入排队);
                        }
                    }
                    break;
                case AddToQueueType.BreakAndEnqueue_打断并置首:
                    for (int i = 0; i < finalList.Count; i++)
                    {
                        if (i == 0)
                        {
                            relatedBrain.BrainHandlerFunction.AddDecisionToQueue(finalList[i],
                                BaseAIBrainHandler.DecisionEnqueueType.FullClearAndEnqueueFirst_清空并置首);
                        }
                        else
                        {
                            relatedBrain.BrainHandlerFunction.AddDecisionToQueue(finalList[i],
                                BaseAIBrainHandler.DecisionEnqueueType.JustEnqueue_加入排队);
                        }
                    }
                    break;
            }
        }
    }
}