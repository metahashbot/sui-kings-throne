using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RPGCore.Dispatcher
{
    // /// <summary>
    // /// <para>RPGCore中，负责维护与记录所有Dispatcher的管理类</para>
    // /// </summary>
    // public class RPG_DispatcherCore : MonoBehaviour
    // {
    //
    //     private const float DISPATCHER_LENGHT = 10f;
    //     private int _currentDispatcherID = 0;
    //
    //
    //     public static RPG_DispatcherCore Instance;
    //     /// <summary>
    //     /// <para>空的DispatcherPrefab。每次需要生成新的Dispatcher的时候就用这个</para>
    //     /// </summary>
    //     [Required, SerializeField] private GameObject _sf_DispatcherBasePrefab;
    //
    //     [NonSerialized,ShowInInspector,ReadOnly]
    //     private List<RolePlay_BaseDispatcher> _allDispatcherList;
    //     public List<RolePlay_BaseDispatcher> AllDispatcherList => _allDispatcherList;
    //
    //     private void Awake()
    //     {
    //         Instance = this;
    //         _allDispatcherList = new List<RolePlay_BaseDispatcher>();
    //     }
    //
    //     public RolePlay_BaseDispatcher CreateNewDispatcher( Vector3 posInWS, float scale)
    //     {
    //         var targetDispatcher = Instantiate(_sf_DispatcherBasePrefab, transform).GetComponent<RolePlay_BaseDispatcher>();
    //         _allDispatcherList.Add(targetDispatcher);
    //         _currentDispatcherID += 1;
    //         targetDispatcher.transform.position = posInWS;
    //         targetDispatcher.transform.localScale = Vector3.one * scale;
    //         targetDispatcher.Initialize(_currentDispatcherID);
    //
    //
    //
    //         return targetDispatcher;
    //     }
    //
    //
    //
    //
    //     private void OnDestroy()
    //     {
    //         foreach (var dispatcherInfo in _allDispatcherList)
    //         {
    //             Destroy(dispatcherInfo);
    //         }
    //
    //         Instance = null;
    //         _allDispatcherList.Clear();
    //     }
    // }
}