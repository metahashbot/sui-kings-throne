using System;
using System.Collections.Generic;
using ARPG.Character.Base;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Projectile.Layout
{
	
	[Serializable ]
	[CreateAssetMenu(fileName = "Projectile Layout(投射物布局)", menuName = "#SO Assets#/#投射物#/一个布局", order = 174)]
	public class SOConfig_ProjectileLayout : ScriptableObject
	{
#if UNITY_EDITOR

		// [Button("测试_算作玩家生成")]
		// private void _TestShoot()
		// {
		// 	if (!Application.isPlaying)
		// 	{
		// 		//检查当前是否是 战斗场景_测试 场景
		// 		
		// 		
		// 		
		// 		if (UnityEditor.EditorUtility.DisplayDialog("需要运行", "需要在【战斗场景_测试】中并且处于运行状态才能进行测试！", "转到目标场景并运行", "取消"))
		// 		{
		// 			
		// 		}
		// 	}
		//
		// 	FindObjectOfType<ProjectileLayoutManager>().Debug_SpawnProjectileLayout(this);
		// }

#endif

		[SerializeField, LabelText("投射物布局配置的SO部分"), PropertyOrder(10)]
		public ProjectileLayoutContentInSO LayoutContentInSO;
	






		[SerializeReference, LabelText("具体的Layout Handler功能"), PropertyOrder(30)]
		public BaseLayoutHandler LayoutHandlerFunction = new BaseLayoutHandler();


		
		[NonSerialized,ShowInInspector,LabelText("原始的SOAsset模板"),PropertyOrder(50)]
		public SOConfig_ProjectileLayout OriginalSOAssetTemplate;




		public Func<Vector3> GetLayoutSpawnPositionFunc()
		{
			return GetLayoutSpawnPosition;
		}

		public Vector3 GetLayoutSpawnPosition()
		{
			return LayoutHandlerFunction.OverrideSpawnFromPosition ?? LayoutHandlerFunction.GetCasterPosition();
		}


#if UNITY_EDITOR
		[Button]
		private void _Button_()
		{
			List<string> used = new List<string>();
			var path = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_ProjectileLayout");
			foreach (var perGUID in path)
			{
				var perPath = UnityEditor.AssetDatabase.GUIDToAssetPath(perGUID);
				SOConfig_ProjectileLayout perSO = UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_ProjectileLayout>(perPath);
				// perSO.InitWhenCreate();

				for (int i = 0; i < perSO.LayoutContentInSO.RelatedProjectileTypeDisplayNameList.Count; i++)
				{
					if( used.Contains(perSO.LayoutContentInSO.RelatedProjectileTypeDisplayNameList[i]))
					{
					}
					else
					{
						used.Add(perSO.LayoutContentInSO.RelatedProjectileTypeDisplayNameList[i]);
					}
				}
				
				
			}

			for (int i = 0; i < used.Count; i++)
			{
				Debug.Log(used[i]);
			}
		}
#endif
#if UNITY_EDITOR
		// [Button("转移碰撞数据")]
		// private void _ConvertAllCollisionInfo()
		// {
		// 	//load all prefab contains BaseARPGCharacterBehaviour
		// 	var guids = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_ProjectileLayout");
		// 	foreach (var guid in guids)
		// 	{
		// 		 string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
		// 		 var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_ProjectileLayout>(path);
		// 	
		// 		 var b = prefab.LayoutContentInSO;
		// 		 
		// 		 
		// 		 
		// 		 
		// 		 b.CollisionInfo.CollisionInfoList = new List<SingleCollisionInfo>();
		// 		 b.CollisionInfo.CollisionInfoList.Add(new SingleCollisionInfo
		// 		 {
		// 			 ColliderType = b.CollisionInfo.ColliderType,
		// 			 Radius = b.CollisionInfo.Radius,
		// 			 InclineRatio = b.CollisionInfo.InclineRatio,
		// 			 Length = b.CollisionInfo.Length,
		// 			 OffsetPos = b.CollisionInfo.OffsetPos
		// 		 });
		// 		 UnityEditor.EditorUtility.SetDirty(prefab);
		// 	
		// 	}
		//
		//
		// 	//load all prefab contains BaseARPGCharacterBehaviour
		// 	var guids2 = UnityEditor.AssetDatabase.FindAssets("t:Prefab");
		// 	foreach (var guid in guids2)
		// 	{
		// 		string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
		// 		var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
		// 		var b = prefab.GetComponent<BaseARPGCharacterBehaviour>();
		// 		if (b == null)
		// 		{
		// 			continue;
		// 		}
		//
		// 		b.SelfCollisionInfo.CollisionInfoList = new List<SingleCollisionInfo>();
		// 		b.SelfCollisionInfo.CollisionInfoList.Add(new SingleCollisionInfo
		// 		{
		// 			ColliderType = b.SelfCollisionInfo.ColliderType,
		// 			Radius = b.SelfCollisionInfo.Radius,
		// 			InclineRatio = b.SelfCollisionInfo.InclineRatio,
		// 			Length = b.SelfCollisionInfo.Length,
		// 			OffsetPos = b.SelfCollisionInfo.OffsetPos
		// 		});
		// 		if (b.ContainMultiCollisionInfo)
		// 		{
		// 			foreach (var perInfo in b.MultiCollisionInfoList)
		// 			{
		// 				perInfo.CollisionInfoList = new List<SingleCollisionInfo>();
		// 				perInfo.CollisionInfoList.Add(new SingleCollisionInfo
		// 				{
		// 					ColliderType = perInfo.ColliderType,
		// 					Radius = perInfo.Radius,
		// 					InclineRatio = perInfo.InclineRatio,
		// 					Length = perInfo.Length,
		// 					OffsetPos = perInfo.OffsetPos
		// 				});
		// 			}
		// 		}
		// 		UnityEditor.EditorUtility.SetDirty(b);
		// 		UnityEditor.EditorUtility.SetDirty(prefab);
		// 		UnityEditor.AssetDatabase.SaveAssetIfDirty(prefab);
		// 	}
		// 	
		// 	
		// }
#endif
	}
}