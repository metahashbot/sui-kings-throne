using System;
using RPGCore.DataEntry;
using RPGCore.Interface;
using UnityEngine;

namespace RPGCore.UtilityDataStructure
{
    /// <summary>
    /// <para>在计算的时候，会传入要求的计算倍率。</para>
    /// <para>数值的默认算法为 AV1 * ModifyValue * [ ModifyMultiplier ] </para>
    /// <para>  e.g.Buff的DataEntryEffect默认为 MM = SAG.F1 * CG.F1</para>
    /// </summary>
    public class RP_DS_DataEntryApplyInfo
    {
        public RP_DataEntry_EnumType DataEntryTypeEnum;
        public ModifyEntry_CalculatePosition CalculatePosition;
        public RPDM_DataEntry_ModifyFrom ModifyFrom;
        public float FinalModifyValue;

        public I_RP_DataEntry_ObjectCanReceiveDataEntryEffect Receiver;
        public I_RP_DataEntry_ObjectCanApplyDataEntryEffect Caster;
        public int EffectIDStamp;

        /// <summary>
        /// <para>默认设置，位置FA，值0，stamp-1</para>
        /// </summary>
        public RP_DS_DataEntryApplyInfo _DefaultSet()
        {
            DataEntryTypeEnum = RP_DataEntry_EnumType.None;
            CalculatePosition = ModifyEntry_CalculatePosition.FrontAdd;
            ModifyFrom = RPDM_DataEntry_ModifyFrom.NoEnum_不指定;
            FinalModifyValue = 0f;
            Receiver = null;
            Caster = null;
            EffectIDStamp = -1;
            return this;
        }

        /// <summary>
        /// <para>设置接收方和发起方。这个操作一定要在SetEffect之前</para>
        /// </summary>
        public RP_DS_DataEntryApplyInfo SetCasterAndReceiver(I_RP_DataEntry_ObjectCanApplyDataEntryEffect caster,I_RP_DataEntry_ObjectCanReceiveDataEntryEffect receiver)
        {
            Caster = caster;
            Receiver = receiver;
            return this;
        }

        public RP_DS_DataEntryApplyInfo SetModifyFromEnum(RPDM_DataEntry_ModifyFrom from)
        {
            ModifyFrom = from;
            return this;
        }

        public RP_DS_DataEntryApplyInfo ResetValueAs(float value)
        {
            FinalModifyValue = value;
            return this;
        }

        public RP_DS_DataEntryApplyInfo ResetModifyTargetEnum(RP_DataEntry_EnumType entry)
        {
            DataEntryTypeEnum = entry;
            return this;
        }

        public RP_DS_DataEntryApplyInfo ApplyValueMul(float mul)
        {
            FinalModifyValue *= mul;
            return this;
        }

        public RP_DS_DataEntryApplyInfo SetStamp(int stamp)
        {
            EffectIDStamp = stamp;
            return this;
        }
        /// <summary>
        /// <para>根据一个ConSer的DataEntryEffect来设置细节。</para>
        /// <para>这个操作一定要在设置接收方和发起方之后</para>
        /// <para>经过该操作后，FinalModifyValue将会设置为ConSer中直接表示的数值，且ConSer中的AV1已经计算</para>
        /// <para>    FallbackValue → 上/下界*ModifyValue*AV1 → ModifyValue*AV1</para>
        /// </summary>
        public RP_DS_DataEntryApplyInfo SetEffectByConSer(ConSer_DataEntryEffect conSerDataEntryEffect)
        {
#if UNITY_EDITOR
            if (Caster == null && Receiver == null)
            {
                Debug.LogError("在设置DataEntry的RPDS的时候，发起方和接收方都是空的。这不合理，检查一下调用来源的顺序");
            }
#endif
            if (conSerDataEntryEffect.InitializeValueAsPercent)
            {
                /////////////////////////////
                // 修饰使用的值关联了一个数据项，则计算它
                ///////////////////////////
                RP_DataEntry_Base fromEntry = null;
                //使用来源的数据项
                if (conSerDataEntryEffect.InitializeValueFromOrigin)
                {
                    fromEntry =
                        Caster.ApplyDataEntry_GetRelatedDataEntry(conSerDataEntryEffect.InitializeValueFromEntry);
                    if (fromEntry == null)
                    {
#if UNITY_EDITOR
                        Debug.LogErrorFormat("试图使用发起者来修饰{0}上的DataEntry ：但发起者{1}并不存在{2}这个Entry",
                            Receiver,Caster, conSerDataEntryEffect.DataEntryTypeEnum);
#endif
                        throw new ArgumentOutOfRangeException();
                    }
                }
                //使用接受者的数据项
                else
                {
                    fromEntry = Receiver.ReceiveDataEntry_GetRelatedDataEntry(conSerDataEntryEffect.InitializeValueFromEntry);
                    if (fromEntry == null)
                    {
#if UNITY_EDITOR
                        Debug.LogErrorFormat("试图修饰{0}上的DataEntry ： {1} ，但它并没有这个Entry",
                            Receiver, conSerDataEntryEffect.InitializeValueFromEntry);
#endif
                        throw new ArgumentOutOfRangeException();
                    }
                }

                //得到了目标数据项，然后看是关联当前值还是上界
                if (conSerDataEntryEffect.UpperBoundOrCurrentValue)
                {
                    //上界：
                    if (!fromEntry.ContainUpperBound)
                    {
                        FinalModifyValue = conSerDataEntryEffect.FallbackValue;
                    }
                    else
                    {
                        FinalModifyValue = (fromEntry as Float_RPDataEntry).UpperBoundValue *
                                           conSerDataEntryEffect.ModifyValue * conSerDataEntryEffect.ArgValue1;
                    }
                }
                else
                {
                    FinalModifyValue = (fromEntry as Float_RPDataEntry).GetCurrentValue() *
                                       conSerDataEntryEffect.ModifyValue * conSerDataEntryEffect.ArgValue1;
                }
            }
            else
            {
                FinalModifyValue = 
                    conSerDataEntryEffect.ModifyValue * conSerDataEntryEffect.ArgValue1;
            }

            DataEntryTypeEnum = conSerDataEntryEffect.DataEntryTypeEnum;
            CalculatePosition = conSerDataEntryEffect.ModifyCalculatePosition;



            return this;
        }
    
    
    
    }
}