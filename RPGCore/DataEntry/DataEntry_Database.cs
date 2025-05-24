using System.Collections.Generic;
using Global.ActionBus;
using RPGCore.InitConfig;
using RPGCore.Interface;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

namespace RPGCore.DataEntry
{
    public class DataEntry_Database
    {
        protected I_RP_Database_ObjectContainDataEntryDatabase _selfRelatedRPObjectReference;
        public I_RP_Database_ObjectContainDataEntryDatabase SelfRelatedRPObjectReference => _selfRelatedRPObjectReference;

        /// <summary>
        /// <para>自身的所有数据项</para>
        /// <para>存的是抽象基类。遍历的时候使用as判断类型即可</para>
        /// </summary>
        [ShowInInspector,ReadOnly]
        protected Dictionary<RP_DataEntry_EnumType, RP_DataEntry_Base> _selfDataEntryDictionary;

#if UNITY_EDITOR
        public Dictionary<RP_DataEntry_EnumType, RP_DataEntry_Base> _e_SelfDataEntryDictionary => _selfDataEntryDictionary;
#endif

        public void FillDataEntryCache(Dictionary<RP_DataEntry_EnumType, float> cacheDict)
        {
            foreach (var entryBase in _selfDataEntryDictionary.Values)
            {
                if (entryBase is Float_RPDataEntry floatEntry)
                {
                    cacheDict.Add(floatEntry.RP_DataEntryType, floatEntry.CurrentValue);
                }
            }
        }


        public DataEntry_Database(I_RP_Database_ObjectContainDataEntryDatabase relatedReference)
        {
            _selfRelatedRPObjectReference = relatedReference;
            _selfDataEntryDictionary =
                CollectionPool<Dictionary<RP_DataEntry_EnumType, RP_DataEntry_Base>,
                    KeyValuePair<RP_DataEntry_EnumType, RP_DataEntry_Base>>.Get();
            _selfDataEntryDictionary.Clear();
            
        }

        public RP_DataEntry_Base GetTargetDataEntry(RP_DataEntry_EnumType type,bool allowNotExist = false)
        {
            if (_selfDataEntryDictionary.ContainsKey(type))
            {
                return _selfDataEntryDictionary[type];
                // throw new ArgumentOutOfRangeException("在" + _selfRelatedRPObjectReference + "内并没有找到类型" + materialType +
                //                                       "的数据Entry");
            }
            if (!allowNotExist)
            {
                Debug.LogError(
                    $"在{_selfRelatedRPObjectReference.GetObjectName_ObjectContainDataEntryDatabase()}对象上查找基类Entry{type}时失败，返回空");
            }
            return null;
        }

        public FloatPresentValue_RPDataEntry GetFloatPresentValueEntryByType(RP_DataEntry_EnumType type)
        {
            if (_selfDataEntryDictionary.ContainsKey(type))
            {
#if UNITY_EDITOR
                if (_selfDataEntryDictionary[type] is not FloatPresentValue_RPDataEntry)
                {
                    DBug.LogErrorFormat("试图在{0}的DataEntry中访问类型{1}的项目，是以FloatPresentValue方式获取的，但对象并不是FloatPresentValue",
                        _selfRelatedRPObjectReference,
                        type);
                }
#endif
                return _selfDataEntryDictionary[type] as FloatPresentValue_RPDataEntry;
            }
            else
            {
#if UNITY_EDITOR
                DBug.LogWarningFormat("试图在{0}的DataEntry查找项目{1}，但它并没有这个项目，并且没有要求创建。检查一下数据配置模板",
                    _selfRelatedRPObjectReference.ToString(),
                    type);
#endif
                return null;
            }
        }


        public Float_RPDataEntry GetFloatDataEntryByType(RP_DataEntry_EnumType type ,bool allowNotExist = false)
        {
            if (_selfDataEntryDictionary.ContainsKey(type))
            {
#if UNITY_EDITOR
                if (_selfDataEntryDictionary[type] is not Float_RPDataEntry)
                {
                    DBug.LogErrorFormat("试图在{0}的DataEntry中访问类型{1}的项目，是以Float方式获取的，但对象并不是floatEntry",
                        _selfRelatedRPObjectReference,
                        type);
                }
#endif

#if UNITY_EDITOR
                RP_DataEntry_Base tmp = _selfDataEntryDictionary[type];
                if (tmp is FloatPresentValue_RPDataEntry)
                {
                    DBug.LogWarning($"数据类型{type}实际上是PresentValue，但此处按照常规DataEntry获取了，这可能会导致一些逻辑错误");
                }
#endif
                return _selfDataEntryDictionary[type] as Float_RPDataEntry;
            }
            else
            {
                if (!allowNotExist)
                {
#if UNITY_EDITOR
                    DBug.LogWarningFormat("试图在{0}的DataEntry查找项目{1}，但它并没有这个项目，并且没有要求创建。检查一下数据配置模板",
                        _selfRelatedRPObjectReference.ToString(),
                        type);
#endif
                }
                
                return null;
                
            }
        }


        public FloatPresentValue_RPDataEntry InitializeFloatPresentValueEntry(
            RP_DataEntry_EnumType type,
            Float_RPDataEntry maxEntryRef)
        {
            if (_selfDataEntryDictionary.ContainsKey(type))
            {
                (_selfDataEntryDictionary[type] as FloatPresentValue_RPDataEntry).ResetDataToValue(maxEntryRef.CurrentValue);

                return _selfDataEntryDictionary[type] as FloatPresentValue_RPDataEntry;
            }
            else
            {
                FloatPresentValue_RPDataEntry newEntry =
                    FloatPresentValue_RPDataEntry.GetFromPool(type, this, maxEntryRef);

                Float_ModifyEntry_RPDataEntry newModify = Float_ModifyEntry_RPDataEntry.GetNewFromPool(maxEntryRef.CurrentValue,
                    RPDM_DataEntry_ModifyFrom.Initialize_LoadWithTemplate_加载,
                    ModifyEntry_CalculatePosition.Original);
                
                _selfDataEntryDictionary.Add(type, newEntry);
                newEntry.AddDataEntryModifier(newModify);
                DS_ActionBusArguGroup ds =
                    new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_DataEntry_OnDataEntryInitialized_数据项被初始化);
                ds.ObjectArgu1 = newEntry;
                ds.ObjectArgu2 = _selfRelatedRPObjectReference;
                _selfRelatedRPObjectReference.GetRelatedActionBus()
                    .TriggerActionByType(ActionBus_ActionTypeEnum.L_DataEntry_OnDataEntryInitialized_数据项被初始化, ds);
                return newEntry;
            }
        }

        /// <summary>
        /// <para>按照类型和初始值直接初始化一个新的FloatDataEntry</para>
        /// </summary>
        public Float_RPDataEntry InitializeFloatDataEntry(RP_DataEntry_EnumType typeEnum, float initValue)
        {
            if (_selfDataEntryDictionary.ContainsKey(typeEnum))
            {
                (_selfDataEntryDictionary[typeEnum] as Float_RPDataEntry).ReplaceOriginalValue(initValue);

                return _selfDataEntryDictionary[typeEnum] as Float_RPDataEntry;
            }
            else
            {
                Float_RPDataEntry newEntry = Float_RPDataEntry.GetFromPool(typeEnum, this);
                
                var newModify = Float_ModifyEntry_RPDataEntry.GetNewFromPool(initValue,
                    RPDM_DataEntry_ModifyFrom.Initialize_LoadWithTemplate_加载,
                    ModifyEntry_CalculatePosition.Original);
                _selfDataEntryDictionary.Add(typeEnum, newEntry);
                newEntry.AddDataEntryModifier(newModify);
                DS_ActionBusArguGroup ds =
                    new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_DataEntry_OnDataEntryInitialized_数据项被初始化);
                ds.ObjectArgu1 = newEntry;
                ds.ObjectArgu2 = _selfRelatedRPObjectReference;
                _selfRelatedRPObjectReference.GetRelatedActionBus()
                    .TriggerActionByType(ActionBus_ActionTypeEnum.L_DataEntry_OnDataEntryInitialized_数据项被初始化, ds);
                return newEntry;
            }
        }

        public Float_RPDataEntry InitializeFloatDataEntry(ConSer_DataEntryInitializeConfig config)
        {
            var typeEnum = config.EntryType;
            Float_RPDataEntry newEntry = Float_RPDataEntry.GetFromPool(typeEnum,this); 
            if (_selfDataEntryDictionary.ContainsKey(typeEnum))
            {
                _selfDataEntryDictionary.Remove(typeEnum);
            }
            _selfDataEntryDictionary.Add(typeEnum, newEntry);

            if (config.ContainLowerBound)
            {
                newEntry.SetLowerBound(config.BaseLowerBound);
            }
            if (config.ContainUpperBound)
            {
                newEntry.SetUpperBound(config.BaseUpperBound);
            }

            var newModify = Float_ModifyEntry_RPDataEntry.GetNewFromPool(config.BaseValue,
                RPDM_DataEntry_ModifyFrom.Initialize_LoadWithTemplate_加载,
                ModifyEntry_CalculatePosition.Original);
            newEntry.AddDataEntryModifier(newModify);
            DS_ActionBusArguGroup ds =
                new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_DataEntry_OnDataEntryInitialized_数据项被初始化);
            ds.ObjectArgu1 = newEntry;
            ds.ObjectArgu2 = _selfRelatedRPObjectReference;
            _selfRelatedRPObjectReference.GetRelatedActionBus()
                .TriggerActionByType(ActionBus_ActionTypeEnum.L_DataEntry_OnDataEntryInitialized_数据项被初始化, ds);
            return newEntry;
        }

        public void ClearBeforeDestroy()
        {

            foreach (RP_DataEntry_Base perEntry in _selfDataEntryDictionary.Values)
            {
                perEntry.ClearBeforeDestroy();
            }
            _selfDataEntryDictionary.Clear();
            CollectionPool<Dictionary<RP_DataEntry_EnumType, RP_DataEntry_Base>,
                KeyValuePair<RP_DataEntry_EnumType, RP_DataEntry_Base>>.Release(_selfDataEntryDictionary);
            _selfDataEntryDictionary = null;

        }
        
        
        
        // public void InitializeFromInitConfig(ConSer_DataEntryInitGroup config)
        // {
        //     foreach (ConSer_DataEntryInitEntry_Float initEntry in config.InitDataEntryList)
        //     {
        //         InitializeFromInitConfig(initEntry);
        //     }
        // }

        // public void InitializeFromInitConfig(List<SOConfig_InitConfig_DataEntryGroup> listConfig)
        // {
        //     foreach (var initGroup in listConfig)
        //     {
        //         InitializeFromInitConfig(initGroup.ConfigCollection);
        //     }
        // }
    }
}