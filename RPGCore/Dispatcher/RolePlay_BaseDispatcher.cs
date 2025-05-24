using Global.ActionBus;
using RPGCore.Buff;
using RPGCore.Buff.BuffHolder;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using UnityEngine;

namespace RPGCore.Dispatcher
{
    // public partial class RolePlay_BaseDispatcher : MonoBehaviour, 
    //     I_RP_Buff_ObjectCanReceiveBuff, 
    //     I_RP_Buff_ObjectCanApplyBuff,
    //     I_RP_Damage_ObjectCanApplyDamage,
    //     I_RP_Damage_ObjectCanReceiveDamage,
    //     I_RP_DataEntry_ObjectCanApplyDataEntryEffect,
    //     I_RP_Database_ObjectContainDataEntryDatabase
    // {
    //
    //     private LocalActionBus _selfActionBusInstance;
    //
    //     public LocalActionBus GetActionBus()
    //     {
//             return _selfActionBusInstance;
//         }
//         /// <summary>
//         /// <para>Dispatcher作用效果的时间戳。一个RPBehaviour或RPDispatcher在同一个时间戳内只会受到一次效果</para>
//         /// </summary>
//         private int _dispatcherEffectIDStamp;
//
//         /// <summary>
//         /// <para>Dispatcher作用效果的时间戳。一个RPBehaviour或RPDispatcher在同一个时间戳内只会受到一次效果</para>
//         /// </summary>
//         public int DispatcherEffectIDStamp => _dispatcherEffectIDStamp;
//
//
//         private const float EFFECT_ID_STAMP_UPDATE_FREQUENCY = 0.5f;
//
//         /// <summary>
//         /// <para>上次更新效果时间戳的时间点，默认都是0.5s更新一次</para>
//         /// </summary>
//         private float _lastUpdateEffectIDStampTime;
//
//         private float _remainingTime;
//         public float RemainingTime => _remainingTime;
//
//
//         private SphereCollider _selfSphereCollider;
//
//
//         private DataEntry_Database _selfDatabaseInstance;
//     
//
//         private void Awake()
//         {
//             _selfActionBusInstance = new LocalActionBus();
//             _selfDatabaseInstance = new DataEntry_Database(this);
//             _selfSphereCollider = GetComponent<SphereCollider>();
//             _selfBuffHolderInstance = new RPBuff_BuffHolder(this);
//
// #if UNITY_EDITOR
//             gameObject.AddComponent<RPDispatcherInspector>();
// #endif
//         }
//
//         public void Initialize(int dispatcherID)
//         {
//         
//         
//         
//         
//         }
//
//         public void FixedUpdateTick(float currentTime, int currentFrame, float delta)
//         {
//             _selfBuffHolderInstance.FixedUpdateTick(currentTime, currentFrame, delta);
//         
//             if (currentTime - _lastUpdateEffectIDStampTime > EFFECT_ID_STAMP_UPDATE_FREQUENCY)
//             {
//                 _dispatcherEffectIDStamp += 1;
//             }
//
//             _remainingTime -= delta;
//             if (_remainingTime < 0f)
//             {
//                 RemoveThisDispatcher();
//             }
//         }
//
//         /// <summary>
//         /// <para>移除这个Dispatcher</para>
//         /// </summary>
//         public void RemoveThisDispatcher()
//         {
//             //从Roster中移除自己
//             RPG_DispatcherCore.Instance.AllDispatcherList.Remove(this);
//             _selfBuffHolderInstance.ClearBuffHolder();
//       
//
//             Destroy(gameObject);
//         }
//
//
//         public string ApplyDamage_GetRelatedCasterName()
//         {
//             return name;
//         }
//
//         public virtual RP_DataEntry_Base ApplyDamage_GetRelatedDataEntry(RP_DataEntry_EnumType materialType)
//         {
//             return _selfDatabaseInstance.GetTargetDataEntry(materialType);
//         }
//
//         public virtual BuffAvailableType ApplyDamage_CheckTargetBuff(RolePlay_BuffTypeEnum materialType)
//         {
//             return _selfBuffHolderInstance.CheckTargetBuff(materialType);
//         }
//
//         public virtual RPBuff_Runtime ApplyDamage_GetTargetBuff(RolePlay_BuffTypeEnum materialType)
//         {
//             return _selfBuffHolderInstance.GetTargetBuff(materialType);
//         }
//
//         public virtual RP_DataEntry_Base ApplyDataEntry_GetRelatedDataEntry(RP_DataEntry_EnumType materialType)
//         {
//             return _selfDatabaseInstance.GetTargetDataEntry(materialType);
//         }
//
//         public BuffAvailableType ApplyDataEntry_CheckTargetBuff(RolePlay_BuffTypeEnum materialType)
//         {
//             return _selfBuffHolderInstance.CheckTargetBuff(materialType);
//         }
//
//         public virtual RPBuff_Runtime ApplyDataEntry_GetTargetBuff(RolePlay_BuffTypeEnum materialType)
//         {
//             return _selfBuffHolderInstance.GetTargetBuff(materialType);
//         }
//
//         public virtual RP_DS_DamageApplyResult ReceiveDamage_ReceiveFromRPDS(RP_DS_DamageApplyInfo applyInfo, int effectIDStamp)
//         {
//             return new RP_DS_DamageApplyResult();
//         }
//
//         public Vector3 ReceiveDamage_GetCurrentReceiverPosition()
//         {
//             return transform.position;
//         }
//
//         public virtual RP_DataEntry_Base ReceiveDamage_GetRelatedDataEntry(RP_DataEntry_EnumType dataEntryType)
//         {
//             return _selfDatabaseInstance.GetTargetDataEntry(dataEntryType);
//         }
//
//         public virtual BuffAvailableType ReceiveDamage_CheckTargetBuff(RolePlay_BuffTypeEnum materialType)
//         {
//             return _selfBuffHolderInstance.CheckTargetBuff(materialType);
//         }
//
//         public  virtual RPBuff_Runtime ReceiveDamage_GetTargetBuff(RolePlay_BuffTypeEnum materialType)
//         {
//             return _selfBuffHolderInstance.GetTargetBuff(materialType);
//         }
//
//         public virtual Vector2 ReceiveDamage_GetCurrentReceiverDirection()
//         {
//             return Vector2.zero;
//         }
//         public string GetObjectName_ObjectContainDataEntryDatabase()
//         {
//
//             return name;
//         }
//         public LocalActionBus GetRelatedActionBus()
//         {
//             return _selfActionBusInstance;
//         }
    // }
}