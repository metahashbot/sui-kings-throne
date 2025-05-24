using System;
using System.Collections.Generic;
using Global;
using Global.ActionBus;
using RPGCore;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Manager
{
	public class VFXPoolManager : MonoBehaviour
	{
		public static VFXPoolManager Instance;

		public static readonly int PoolUpdateFrequency = 5;


		[ShowInInspector, LabelText("特效对象池信息_常规粒子"), FoldoutGroup("运行时", true)]
		private Dictionary<GameObject, VFXPerTypePool> _selfVFXTypePoolDict;

		[ShowInInspector, LabelText("特效对象池信息_PSPP粒子"), FoldoutGroup("运行时", true)]
		private Dictionary<GameObject, VFX_PSPP_PerTypeInfo> _selfVFXTypePoolDict_PSPP;


		private class VFX_PSPP_PerTypeInfo
		{
			protected static Transform VFXManagerTransformParent;

			public static void InjectParentTransform(Transform parent)
			{
				VFXManagerTransformParent = parent;
			}


			[ShowInInspector, LabelText("原始Prefab Asset")]
			public GameObject OriginalVFXPrefab;

			private List<int> _needToReturn;

			[ShowInInspector, LabelText("当前空闲PS")]
			protected List<VFX_ParticleSystemPlayProxy> _currentFreePSPPList;

			[ShowInInspector, LabelText("当前忙碌PS")]
			protected List<VFX_ParticleSystemPlayProxy> _currentBusyPSPPList;


			public void ClearOnUnload()
			{
				foreach (VFX_ParticleSystemPlayProxy perPS in _currentFreePSPPList)
				{
					GameObject.Destroy(perPS.gameObject);
				}
				foreach (VFX_ParticleSystemPlayProxy perPS in _currentBusyPSPPList)
				{
					GameObject.Destroy(perPS.gameObject);
				}
			}

			public VFX_ParticleSystemPlayProxy CreateNew(bool alsoSetMainAsDisable = true)
			{
				int nameIndex = _currentBusyPSPPList.Count + _currentFreePSPPList.Count;
				var tmpGO = Instantiate(OriginalVFXPrefab, VFXManagerTransformParent);
				tmpGO.transform.localPosition = Vector3.zero;
				tmpGO.name = $"{nameIndex}—{OriginalVFXPrefab.name}";
				if (!tmpGO.TryGetComponent(out VFX_ParticleSystemPlayProxy pspp))
				{
					DBug.LogError($" 特效{OriginalVFXPrefab.name} 被当做PSPP方式从池中获取，但是并没有Get到PSPP组件，这不合理，检查一下");
				}
				ParticleSystem ps = tmpGO.GetComponent<ParticleSystem>();
				pspp.SetMainPS(ps);
				if (alsoSetMainAsDisable)
				{
					ParticleSystem.MainModule mainModule = ps.main;
					mainModule.stopAction = ParticleSystemStopAction.Disable;
				}
				return pspp;
			}

			public VFX_PSPP_PerTypeInfo(GameObject originalPrefab)
			{
				OriginalVFXPrefab = originalPrefab;
				_needToReturn = new List<int>();
				_currentBusyPSPPList = new List<VFX_ParticleSystemPlayProxy>();
				_currentFreePSPPList = new List<VFX_ParticleSystemPlayProxy>();
			}



			public void ReturnToPool(VFX_ParticleSystemPlayProxy pspp)
			{
				if (!pspp.MainParticleSystemRef.transform.IsChildOf(VFXManagerTransformParent))
				{
					pspp.transform.SetParent(VFXManagerTransformParent);
				}
				pspp.MainParticleSystemRef.Clear(true);
				pspp.MainParticleSystemRef.Stop(true);
				pspp.MainParticleSystemRef.gameObject.SetActive(false);
				if (_currentBusyPSPPList.Contains(pspp))
				{
					_currentBusyPSPPList.Remove(pspp);
				}
				if (!_currentFreePSPPList.Contains(pspp))
				{
					_currentFreePSPPList.Add(pspp);
				}
			}

			public VFX_ParticleSystemPlayProxy GetFreePS(bool alsoSetMainAsDisable = true)
			{
				for (int i = _currentFreePSPPList.Count - 1; i >= 0; i--)
				{
					VFX_ParticleSystemPlayProxy tmpPS = _currentFreePSPPList[i];
					if (!tmpPS.gameObject.activeInHierarchy && !tmpPS.gameObject.activeSelf)
					{
						tmpPS.gameObject.SetActive(true);
						_currentFreePSPPList.Remove(tmpPS);
						_currentBusyPSPPList.Add(tmpPS);
						return tmpPS;
					}
				}
				var newPS = CreateNew(alsoSetMainAsDisable);
				_currentBusyPSPPList.Add(newPS);
				return newPS;
			}


			/// <summary>
			/// <para>只要不是处于Hierarchy活跃的，就都视作已经结束的。</para>
			/// </summary>
			public void UpdateTick(float ct, int cf, float delta)
			{
				for (int i = _currentBusyPSPPList.Count - 1; i >= 0; i--)
				{
					_currentBusyPSPPList[i].UpdateTick(ct, cf, delta);
					if (!_currentBusyPSPPList[i].gameObject.activeInHierarchy &&
					    !_currentBusyPSPPList[i].gameObject.activeSelf)
					{
						var tmp = _currentBusyPSPPList[i];
						_currentBusyPSPPList.RemoveAt(i);
						_currentFreePSPPList.Add(tmp);
					}
				}
			}

		}


		private class VFXPerTypePool
		{
			protected static Transform VFXManagerTransformParent;

			public static void InjectParentTransform(Transform parent)
			{
				VFXManagerTransformParent = parent;
			}


			[ShowInInspector, LabelText("原始Prefab Asset")]
			public GameObject OriginalVFXPrefab;

			private List<int> _needToReturn;

			[ShowInInspector, LabelText("当前空闲PS")]
			protected List<ParticleSystem> _currentFreePSList;

			[ShowInInspector, LabelText("当前忙碌PS")]
			protected List<ParticleSystem> _currentBusyPSList;


			public void ClearOnUnload()
			{
				foreach (ParticleSystem perPS in _currentFreePSList)
				{
					GameObject.Destroy(perPS.gameObject);
				}
				foreach (ParticleSystem perPS in _currentBusyPSList)
				{
					GameObject.Destroy(perPS.gameObject);
				}
			}

			public ParticleSystem CreateNew(bool alsoSetMainAsDisable = true)
			{
				int nameIndex = _currentBusyPSList.Count + _currentFreePSList.Count;
#if UNITY_EDITOR
				if (OriginalVFXPrefab == null)
				{
					string errorStr=  $"对象池在生成新特效时，出现了一个原始VFXPrefab为空的情况。当前是这个池的第{nameIndex}个特效，检查一下\n";
					try
					{
						if (_currentBusyPSList.Count > 0)
						{
							errorStr +=
								$"当前忙碌特效有{_currentBusyPSList.Count}个，第0个是{_currentBusyPSList[0].gameObject.name}";
						}
						else if (_currentFreePSList.Count > 0)
						{
							errorStr +=
								$"当前空闲特效有{_currentFreePSList.Count}个，第0个是{_currentFreePSList[0].gameObject.name}";
						}
						else
						{
							errorStr += $"当前没有特效";
						}
					}
					catch (Exception e)
					{
						errorStr += "甚至之前的一个特效都没有，李在赣蛇魔";
					}
					finally
					{
						DBug.LogError(errorStr);

					}
				}
#endif
				var tmpGO = Instantiate(OriginalVFXPrefab, VFXManagerTransformParent);
				tmpGO.transform.localPosition = Vector3.zero;
				tmpGO.name = $"{nameIndex}—{OriginalVFXPrefab.name}";
#if UNITY_EDITOR
				if (tmpGO.TryGetComponent(out VFX_ParticleSystemPlayProxy pspp))
				{
					DBug.LogError($" 特效{OriginalVFXPrefab.name} 包含了PSPP组件，它应当是个手动控制播放的特效。这里是非PSPP池，不应当调用至此，检查一下");
				}
#endif
			
				ParticleSystem ps = tmpGO.GetComponent<ParticleSystem>();

				foreach (ParticleSystem perPS in tmpGO.GetComponentsInChildren<ParticleSystem>(true))
				{
					if (perPS == ps)
					{
						continue;
					}
					else
					{
						if (perPS.main.stopAction == ParticleSystemStopAction.Disable)
						{
							var module = perPS.main;
							module.stopAction = ParticleSystemStopAction.None;
#if UNITY_EDITOR
							Debug.LogError($"【特效】{perPS.name}的停止行为被修改为【None】，请注意检查。其父级为 {ps.gameObject.name}");
#endif
						}
					}
				}
				if (alsoSetMainAsDisable)
				{
					ParticleSystem.MainModule mainModule = ps.main;
					mainModule.stopAction = ParticleSystemStopAction.Disable;
				}
				return ps;
			}

			public VFXPerTypePool(GameObject originalPrefab)
			{
				OriginalVFXPrefab = originalPrefab;
				_needToReturn = new List<int>();
				_currentBusyPSList = new List<ParticleSystem>();
				_currentFreePSList = new List<ParticleSystem>();
			}



			public void ReturnToPool(ParticleSystem ps)
			{
				if (!ps.transform.IsChildOf(VFXManagerTransformParent))
				{
					ps.transform.SetParent(VFXManagerTransformParent);
				}
				ps.Clear(true);
				ps.Stop(true);
				ps.gameObject.SetActive(false);
				if (_currentBusyPSList.Contains(ps))
				{
					_currentBusyPSList.Remove(ps);
				}
				if (!_currentFreePSList.Contains(ps))
				{
					_currentFreePSList.Add(ps);
				}
			}

			public ParticleSystem GetFreePS(bool alsoSetMainAsDisable = true)
			{
				for (int i = _currentFreePSList.Count - 1; i >= 0; i--)
				{
					ParticleSystem tmpPS = _currentFreePSList[i];
					if (!tmpPS.gameObject.activeInHierarchy && !tmpPS.gameObject.activeSelf)
					{
						tmpPS.gameObject.SetActive(true);
						_currentFreePSList.Remove(tmpPS);
						_currentBusyPSList.Add(tmpPS);
						return tmpPS;
					}
				}
				var newPS = CreateNew(alsoSetMainAsDisable);
				_currentBusyPSList.Add(newPS);
				return newPS;
			}


			/// <summary>
			/// <para>只要不是处于Hierarchy活跃的，就都视作已经结束的。</para>
			/// </summary>
			public void UpdateTick(float ct, int cf, float delta)
			{
				for (int i = _currentBusyPSList.Count - 1; i >= 0; i--)
				{
					if (!_currentBusyPSList[i].gameObject.activeInHierarchy &&
					    !_currentBusyPSList[i].gameObject.activeSelf)
					{
						var tmp = _currentBusyPSList[i];
						_currentBusyPSList.RemoveAt(i);
						_currentFreePSList.Add(tmp);
					}
				}
			}
		}




		public void InitializeOnAwake()
		{
			Instance = this;
			_selfVFXTypePoolDict = new Dictionary<GameObject, VFXPerTypePool>();
			_selfVFXTypePoolDict_PSPP = new Dictionary<GameObject, VFX_PSPP_PerTypeInfo>();
			VFXPerTypePool.InjectParentTransform(transform);
			VFX_PSPP_PerTypeInfo.InjectParentTransform(transform);
		}



		public void StartInitialize()
		{
			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_GE_VFXSpawnCameraWithID_要求生成一个VFX于相机,
				_ABC_RegisterVFXTagIDToPrefab);
		}

		public void UpdateTick(float ct, int cf, float delta)
		{            
            foreach (VFXPerTypePool perTypePool in _selfVFXTypePoolDict.Values)
			{
				perTypePool.UpdateTick(ct, cf, delta);
			}
			foreach (VFX_PSPP_PerTypeInfo perTypePool in _selfVFXTypePoolDict_PSPP.Values)
			{
				perTypePool.UpdateTick(ct, cf, delta);
			}
			//
			// for (int i = NonSelfTickVfxInfoList.Count - 1; i >= 0; i--)
			// {
			// 	NonSelfTickVfxInfoList[i].UpdateTick(ct, cf, delta); 
			// 	 if( NonSelfTickVfxInfoList[i].CurrentState == VFX_ParticleSystemPlayProxy.ParticleSystemPlayProxyStateTypeEnum.Inactive_不活跃)
			// 	 {
			// 		 NonSelfTickVfxInfoList.RemoveAt(i);
			// 	 }
			// }
		}

		/// <summary>
		/// <para>从对象池中获取一个空闲的PS。需要传入这个PS特效的原始Prefab</para>
		/// <para>返回获得的PS是GameObject.Active的，但是PS还没有操作，显式地Play()一下就相当于激活了</para>
		/// </summary>
		public ParticleSystem GetParticleSystemRuntimeByPrefab(GameObject prefab, bool alsoSetMainAsDisable = true)
		{
			if (!_selfVFXTypePoolDict.ContainsKey(prefab))
			{
				_selfVFXTypePoolDict.Add(prefab, new VFXPerTypePool(prefab));
			}
			return _selfVFXTypePoolDict[prefab].GetFreePS(alsoSetMainAsDisable);
		}

		public VFX_ParticleSystemPlayProxy GetPSPPRuntimeByPrefab(GameObject prefab, bool alsoSetMainAsDisable = true)
		{
			if (!_selfVFXTypePoolDict_PSPP.ContainsKey(prefab))
			{
				_selfVFXTypePoolDict_PSPP.Add(prefab, new VFX_PSPP_PerTypeInfo(prefab));
			}
			return _selfVFXTypePoolDict_PSPP[prefab].GetFreePS(alsoSetMainAsDisable);
		}



		public void GetRuntimeRefByPrefab(GameObject prefab, out ParticleSystem ps, out VFX_ParticleSystemPlayProxy pspp)
		{
			if (prefab.TryGetComponent(out VFX_ParticleSystemPlayProxy d))
			{
				pspp = GetPSPPRuntimeByPrefab( prefab);
				ps = null;
				return;
			}
			else
			{
				ps = GetParticleSystemRuntimeByPrefab(prefab);
				pspp = null;
				return;
			}
			 
		}


		public void ReturnParticleSystemToPool(GameObject prefab, ParticleSystem ps)
		{
			if (!_selfVFXTypePoolDict.ContainsKey(prefab))
			{
#if UNITY_EDITOR
				DBug.LogError($"在回收VFX特效时，VFX池报告并不存在{prefab.name}的VFX池，这可能是因为你在运行时创建了一个新的VFX，但是没有在VFX池中注册。");
#endif
				return;
			}
			_selfVFXTypePoolDict[prefab].ReturnToPool(ps);
		}


		public void FadeOutParticleSystemToPool(GameObject prefab, ParticleSystem mainPS, float fadeOutDuration)
		{
			if (!_selfVFXTypePoolDict.ContainsKey(prefab))
			{
#if UNITY_EDITOR
	
				 
#endif
			}
			
		}


		public void ReturnParticleSystemToPool(GameObject prefab, VFX_ParticleSystemPlayProxy ps)
		{
			if (!_selfVFXTypePoolDict_PSPP.ContainsKey(prefab))
			{
#if UNITY_EDITOR
				DBug.LogError($"在回收VFX特效时，VFX池报告并不存在{prefab.name}的VFX池，这可能是因为你在运行时创建了一个新的VFX，但是没有在VFX池中注册。");
#endif
				return;
			}
			_selfVFXTypePoolDict_PSPP[prefab].ReturnToPool(ps);
		}



		public static void StopAndResetAnchorToPool(ParticleSystem ps)
		{
			ps.Stop();
			ps.transform.SetParent(Instance.transform);
		}


#region VFX 于 摄像机

		private Dictionary<string, List<GameObject>> _vfxTagIDToPrefabDict = new Dictionary<string, List<GameObject>>();

		private void _ABC_RegisterVFXTagIDToPrefab(DS_ActionBusArguGroup ds)
		{
			var obj = ds.ObjectArgu1 as ParticleSystem;
			var tagID = ds.ObjectArgu2 as string;
			if (!_vfxTagIDToPrefabDict.ContainsKey(tagID))
			{
				_vfxTagIDToPrefabDict.Add(tagID, new List<GameObject>());
			}
			_vfxTagIDToPrefabDict[tagID].Add(obj.gameObject);
		}

		public void ToggleVFXByTagID(string tagID, bool active)
		{
			if (!_vfxTagIDToPrefabDict.ContainsKey(tagID))
			{
				return;
			}
			foreach (var perPrefab in _vfxTagIDToPrefabDict[tagID])
			{
				perPrefab.SetActive(active);
			}
		}

		public bool _cameraVFXsActive = true;
		public void ToggleCameraVFXs()
		{
            _cameraVFXsActive = !_cameraVFXsActive;
            foreach (List<GameObject> perObjList in _vfxTagIDToPrefabDict.Values)
            {
				if (null != perObjList && perObjList.Count > 0)
				{
                    for (int i = 0; i < perObjList.Count; i++)
                    {
						perObjList[i].SetActive(_cameraVFXsActive);
                    }
				}                
            }
        }

#endregion

		public void ClearOnUnload()
		{
			foreach (VFXPerTypePool perTypePool in _selfVFXTypePoolDict.Values)
			{
				perTypePool.ClearOnUnload();
			}

			foreach (VFX_PSPP_PerTypeInfo perTypePool in _selfVFXTypePoolDict_PSPP.Values)
			{
				perTypePool.ClearOnUnload();
			}
		}


	}
}