using System;
using System.Collections;
using System.Collections.Generic;
using GameplayEvent;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{
    [Serializable]
    public class DCC_停止游戏性事件_StopGameplayEvent : BaseDecisionCommonComponent
    {
        
        
        
        [SerializeField,LabelText("将要停止事件的UID"), GUIColor(255f / 255f, 248f / 255f, 10f / 255f)]
        public string EventUID;
        public override void EnterComponent(SOConfig_AIBrain relatedBrain)
        {
            GameplayEventManager.Instance.StopGameplayEvent(EventUID);
        }

        public override string GetElementNameInList()
        {
            return $"{GetBaseCustomName()} 停止游戏性事件  UID：_{EventUID}_";
        }
    }
}
