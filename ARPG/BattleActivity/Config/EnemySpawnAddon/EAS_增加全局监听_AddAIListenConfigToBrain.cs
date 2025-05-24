using System;
using System.Collections.Generic;
using ARPG.Character.Enemy.AI.Listen;
using ARPG.Manager.Config;
using Sirenix.OdinInspector;
using UnityEngine.Pool;
// using UnityEngine.Pool;

namespace ARPG.BattleActivity.Config.EnemySpawnAddon
{
    [Serializable]
    public class EAS_增加全局监听_AddAIListenConfigToBrain : BaseEnemySpawnAddon
    {
        [LabelText("需要增加到其AIBrain全局监听的监听配置")]
        public SOConfig_AIListen ToAddAIListenConfig;


        public static EAS_增加全局监听_AddAIListenConfigToBrain GetFromPool(EAS_增加全局监听_AddAIListenConfigToBrain copy = null)
        {
            // var newEAS = GenericPool<EAS_增加全局监听_AddAIListenConfigToBrain>.Get();
            var newEAS = new EAS_增加全局监听_AddAIListenConfigToBrain();
            if (copy != null)
            {
                newEAS.ToAddAIListenConfig = copy.ToAddAIListenConfig;
            }

            return newEAS;
        } 
    
    
        public override void ResetOnReturn()
        {
            GenericPool<EAS_增加全局监听_AddAIListenConfigToBrain>.Release(this);
            return;
        }

 
    }
}
