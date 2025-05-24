using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGCore.Buff.ConcreteBuff.Common
{
    [Serializable]
    public class WARResult_效果之传递Buff参数_ParseBLPResult : BaseWeaknessAffectRule, I_WeaknessComponentAsResult
    {
        [SerializeField]
        public List<ConSer_BuffApplyInfo> BuffApplyInfo;

        [SerializeField, LabelText("不存在Buff的时候不传递参数")]
        public bool NotParseWhenNotExist = true;




        public void RegisterToGroup(Buff_通用敌人弱点_CommonEnemyWeakness.WeaknessInfoGroup group)
        {
            var newCopy = CreateNew(this);
            newCopy.RelatedBuffRef = group.RelatedBuff;
            newCopy.RelatedGroupRef = group;
            group.AddResultRule(newCopy);

        }

        public static WARResult_效果之传递Buff参数_ParseBLPResult CreateNew(WARResult_效果之传递Buff参数_ParseBLPResult copyFrom)
        {
            var newRule = new WARResult_效果之传递Buff参数_ParseBLPResult();
            newRule.BuffApplyInfo = copyFrom.BuffApplyInfo;
            newRule.NotParseWhenNotExist = copyFrom.NotParseWhenNotExist;
            return newRule;
            
        }
        public void TriggerWeaknessResult()
        {
            for (int i = 0; i < BuffApplyInfo.Count; i++)
            {
                ConSer_BuffApplyInfo currentApplyInfo = BuffApplyInfo[i];
                var checkR = RelatedBuffRef.Parent_SelfBelongToObject.ReceiveBuff_CheckTargetBuff(currentApplyInfo.BuffType);
                if(checkR == BuffAvailableType.NotExist && NotParseWhenNotExist)
                {
                    continue;
                }
                RelatedBuffRef.Parent_SelfBelongToObject.ReceiveBuff_TryApplyBuff(currentApplyInfo.BuffType, RelatedBuffRef.Parent_SelfBelongToObject as I_RP_Buff_ObjectCanApplyBuff
                    , RelatedBuffRef.Parent_SelfBelongToObject, currentApplyInfo.GetFullBLPList()) ;

            }


        }

        public override void UnloadAndClearBeforeRemove(Buff_通用敌人弱点_CommonEnemyWeakness relatedBuff)
        {

        }
    }
}