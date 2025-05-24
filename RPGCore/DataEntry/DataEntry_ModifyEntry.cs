/*
│	File:RPG_DataEntryEffector.cs
│	DroyLouo(ccd775@gmail.com)
│	CreateTime:2021-6-16 22:24:27
╰━━━━━━━━━━━━━━━━
*/
//#pragma warning disable CS0162
//#pragma warning disable CS0414

using RPGCore.Buff;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

namespace RPGCore.DataEntry
{
    public static class DataEntryUtility
    {
    }
    // /// <summary>
    // /// <para>整数类型的记录项</para>
    // /// </summary>
    // public struct Integer_RPDM_DE_ModifyEntry
    // {
    //
    //     public int Modify;
    //
    //     public RPDM_DataEntry_ModifyFrom ModifyFrom;
    //     public int ModifyFromID;
    //
    //     public Integer_RPDM_DE_ModifyEntry(int modify, RPDM_DataEntry_ModifyFrom modifyFrom = RPDM_DataEntry_ModifyFrom.NoEnum_不指定, int modifyFromID = 0)
    //     {
    //         Modify = modify;
    //         ModifyFrom = modifyFrom;
    //         if (modifyFromID == 0)
    //         {
    //             ModifyFromID = (int) modifyFrom;
    //         }
    //         else
    //         {
    //             ModifyFromID = modifyFromID;
    //         }
    //     }
    // }
    /// <summary>
    /// <para>浮点类型的记录项</para>
    /// </summary>
    public class Float_ModifyEntry_RPDataEntry
    {
        [ShowInInspector]
        public float ModifyValue = 0f;
        public RPDM_DataEntry_ModifyFrom ModifyFrom = RPDM_DataEntry_ModifyFrom.NoEnum_不指定;
        public object ModifyFromObjectRef;
        public ModifyEntry_CalculatePosition _CalculatePosition;
        public float ModifyTimeStamp;
        public int ModifyVersion;
        public RP_DataEntry_Base ModifyOnEntryRef;




        public static Float_ModifyEntry_RPDataEntry GetNewFromPool(float modifyValue,
            RPDM_DataEntry_ModifyFrom modifyFrom = RPDM_DataEntry_ModifyFrom.NoEnum_不指定,
            ModifyEntry_CalculatePosition calculatePosition = ModifyEntry_CalculatePosition.Original,
            object modifyFromObject = null, int version = -1
          )
        {
            Float_ModifyEntry_RPDataEntry newEntry = GenericPool<Float_ModifyEntry_RPDataEntry>.Get();
            newEntry.ModifyValue = modifyValue;
            newEntry.ModifyFrom = modifyFrom;
            newEntry.ModifyFromObjectRef = modifyFromObject;
            newEntry._CalculatePosition = calculatePosition;
            newEntry.ModifyTimeStamp = Time.time;
            newEntry.ModifyVersion = version;
            return newEntry;
        }

        public override string ToString()
        {
            return
                $"修饰值:{ModifyValue}，位置于:{_CalculatePosition},来源枚举:{ModifyFrom},来源对象:{ModifyFromObjectRef},时间戳:{ModifyTimeStamp},版本:{ModifyVersion}";
        }



        public void ReleaseToPool()
        {
            GenericPool<Float_ModifyEntry_RPDataEntry>.Release(this);
        }
    }

    /// <summary>
    /// <para>乘区是百分比，即目标位置上的值为100时表示额外增加了100%</para>
    /// </summary>
    public enum ModifyEntry_CalculatePosition
    {
        Original = 1,
        FrontAdd = 100,
        /// <summary>
        /// 乘算区是百分数算法，当作为修饰值时，写10表示在乘区+0.1，最后会乘1.1
        /// </summary>
        FrontMul = 200,
        RearAdd = 300,
        /// <summary>
        /// 乘算区是百分数算法，当作为修饰值时，写10表示在乘区+0.1，最后会乘1.1
        /// </summary>
        RearMul = 400,
    }


    public enum ModifyEntry_CPSimple
    {
        Original_覆盖原始 = 1,
        Add_加法 = 10,
    }

    /// <summary>
    /// <para>数据项的修改是什么原因？</para>
    /// </summary>
    public enum RPDM_DataEntry_ModifyFrom
    {
        /// <summary>
        /// 该来源不在Enum中列出，只能通过ID来查找
        /// </summary>
        NoEnum_不指定 = 0,
    
    
    
        Initialize_FromReset_被重置 = 1,
        
    
        /// <summary>初始化——使用模板数据加载。0~99之间的都是各种各样的Initialize</summary>
        Initialize_LoadWithTemplate_加载 = 10,
    
        FromEquipment_来自装备 = 31,
        
        FromEvent_来自事件  = 41,
    
    
        FromDamage_伤害流程 = 111,
        
        FromBuff_Buff = 201,
    
        FromSkill_技能 = 202,
    
        FromDropItem_来自掉落物 = 215,

        FromDodge = 6003,
    
        FromDataEntryEffect = 10000,

    
        FromDebug = 9999999,

    }
}