using System;
using System.Collections.Generic;
using System.Linq;
using RPGCore;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Common.HitEffect
{
	[Serializable]
	[CreateAssetMenu(fileName = "受击特效配置组", menuName = "#SO Assets#/#战斗关卡配置#/受击特效配置组", order = 56)]
	public class SOConfig_HitVFXGroupConfig : ScriptableObject
	{


// #if UNITY_EDITOR
// 		[Button("重置一下")]
// 		public void Button_RESET()
// 		{
// 			// find all soconfig ai decision by asset database
//  			var soConfigs = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_HitVFXGroupConfig");
// 		    foreach (var perGUID in soConfigs)
// 		    {
// 			    var perPath = UnityEditor.AssetDatabase.GUIDToAssetPath(perGUID);
// 			    SOConfig_HitVFXGroupConfig perSO =
// 				    UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_HitVFXGroupConfig>(perPath);
//
// 			    foreach (PerDamageVFXInfoPair perPair in perSO.AllDamageVFXInfoList)
// 			    {
// 				    perPair.VFXInfoConfig = new PerVFXInfo();
//
// 				    switch (perPair.VFXTransformContent)
// 				    {
// 					    case VFXTransform_Bone vfxTransformBone:
// 						    if (vfxTransformBone.BoneConfigNames.Length > 1)
// 						    {
// 							    perPair.VFXInfoConfig.ContainMultipleConfig = true;
// 							    perPair.VFXInfoConfig.CustomMultipleConfigNameLists = new List<VFXAnchorInfoPair>();
// 							    foreach (string perString in vfxTransformBone.BoneConfigNames)
// 							    {
// 								    perPair.VFXInfoConfig.CustomMultipleConfigNameLists.Add(new VFXAnchorInfoPair
// 								    {
// 									    AnchorType = PerVFXInfo.PresetAnchorPosTypeEnum.AnchorCustom_挂点到自定义配置名,
// 									    AnchorName = perString
// 								    });
// 							    }
// 						    }
// 						    else
// 						    {
// 							    perPair.VFXInfoConfig.PresetAnchorHolderIndex =
// 								    PerVFXInfo.PresetAnchorPosTypeEnum.AnchorCustom_挂点到自定义配置名;
// 							    perPair.VFXInfoConfig.CustomConfigName = vfxTransformBone.BoneConfigNames[0];
// 						    }
//
// 						    break;
// 					    case VFXTransform_BonePosition vfxTransformBonePosition:
// 						    if (vfxTransformBonePosition.BoneConfigNames.Length > 1)
// 						    {
// 							    perPair.VFXInfoConfig.ContainMultipleConfig = true;
// 							    perPair.VFXInfoConfig.CustomMultipleConfigNameLists = new List<VFXAnchorInfoPair>();
// 							    foreach (string perString in vfxTransformBonePosition.BoneConfigNames)
// 							    {
// 								    perPair.VFXInfoConfig.CustomMultipleConfigNameLists.Add(new VFXAnchorInfoPair
// 								    {
// 									    AnchorType = PerVFXInfo.PresetAnchorPosTypeEnum.PosOnlyCustom_位置同步到自定义配置名,
// 									    AnchorName = perString
// 								    });
// 							    }
// 						    }
// 						    else
// 						    {
// 							    perPair.VFXInfoConfig.PresetAnchorHolderIndex =
// 								    PerVFXInfo.PresetAnchorPosTypeEnum.PosOnlyCustom_位置同步到自定义配置名;
// 							    perPair.VFXInfoConfig.CustomConfigName = vfxTransformBonePosition.BoneConfigNames[0];
// 						    }
// 						    break;
// 					    case VFXTransform_JustScaleAnchor vfxTransformJustScaleAnchor:
// 						    perPair.VFXInfoConfig.PresetAnchorHolderIndex =
// 							    PerVFXInfo.PresetAnchorPosTypeEnum.AnchorOnlyScale_挂点到仅缩放;
// 						    break;
// 					    case VFXTransform_JustScalePosition vfxTransformJustScalePosition:
// 						    perPair.VFXInfoConfig.PresetAnchorHolderIndex =
// 							    PerVFXInfo.PresetAnchorPosTypeEnum.PosOnlyPos_位置同步到仅自身;
// 						    break;
// 				    }
//
// 				    if (perPair.RelatedVFXPrefabArray.Length > 1)
// 				    {
// 					    perPair.VFXInfoConfig.ContainRandomPrefab = true;
// 					    perPair.VFXInfoConfig.RandomArray = new GameObject[perPair.RelatedVFXPrefabArray.Length];
// 					    Array.Copy( perPair.RelatedVFXPrefabArray, perPair.VFXInfoConfig.RandomArray, perPair.RelatedVFXPrefabArray.Length);
// 					    
// 				    }
// 				    else
// 				    {
// 					    perPair.VFXInfoConfig.ContainRandomPrefab = false;
// 					    if (perPair.RelatedVFXPrefabArray != null && perPair.RelatedVFXPrefabArray.Length > 0)
// 					    {
// 						    perPair.VFXInfoConfig.Prefab = perPair.RelatedVFXPrefabArray[0];
// 					    }
// 				    }
// 			    }
//
// 			    UnityEditor.EditorUtility.SetDirty(perSO);
// 		    }
// 		    UnityEditor.AssetDatabase.SaveAssets();
// 		}
// #endif

		[LabelText("这套配置的名字"),SerializeField]
		public string ConfigName;



		[LabelText("具体的信息组们"),SerializeField]
		public List<PerDamageVFXInfoPair> AllDamageVFXInfoList;


		


// #if UNITY_EDITOR
//
// 		[Button("填充一下")]
// 		private void _button_FillBase()
// 		{
// 			var damages = (DamageTypeEnum[]) Enum.GetValues(typeof(DamageTypeEnum));
// 		
// 			List<DamageTypeEnum> types = damages.ToList();
// 			types.Remove(DamageTypeEnum.None);
// 		
// 			List<RolePlay_BuffTypeEnum> buffs = new List<RolePlay_BuffTypeEnum>
// 			{
// 				RolePlay_BuffTypeEnum._EnemyTag_NormalEnemy_普通敌人, RolePlay_BuffTypeEnum._EnemyTag_EliteEnemy_精英敌人,
// 				RolePlay_BuffTypeEnum._EnemyTag_BossEnemy_首领敌人
// 			};
// 			
// 			//foreach two list and create each PerDamageVFXInfoPair
// 			AllDamageVFXInfoList = new List<PerDamageVFXInfoPair>();
// 			foreach (var type in types)
// 			{
// 				foreach (var buff in buffs)
// 				{
// 					var tmpNewInfo = new PerDamageVFXInfoPair();
// 					string nameEnemy = null;
// 					if (buff == RolePlay_BuffTypeEnum._EnemyTag_NormalEnemy_普通敌人)
// 					{
// 						nameEnemy = "小怪";
// 					}
// 					else if (buff == RolePlay_BuffTypeEnum._EnemyTag_EliteEnemy_精英敌人)
// 					{
// 						nameEnemy = "精英";
// 					}
// 					else if (buff == RolePlay_BuffTypeEnum._EnemyTag_BossEnemy_首领敌人)
// 					{
// 						nameEnemy = "Boss";
// 					}
// 					nameEnemy += type.ToString().Split("_")[1];
// 					// tmpNewInfo.ConfigName = nameEnemy + "受击";
// 					tmpNewInfo.VFXInfoContent = new VFXInfo_CommonHit
// 					{
// 						DamageType = type,
// 						RelatedBuff = buff
// 					};
// 					tmpNewInfo.VFXSpawnSource = new VFXSpawn_Priority()
// 					{
// 						Priority =  5,
// 					};
// 					tmpNewInfo.VFXInfoConfig = new PerVFXInfo();
// 					tmpNewInfo.VFXInfoConfig.PresetAnchorHolderIndex =
// 						PerVFXInfo.PresetAnchorPosTypeEnum.AnchorCustom_挂点到自定义配置名;
// 					tmpNewInfo.VFXInfoConfig.CustomConfigName = "胸部";
// 					AllDamageVFXInfoList.Add(tmpNewInfo);
// 				}
// 			}
// 			//save
// 			UnityEditor.EditorUtility.SetDirty(this);
// 			UnityEditor.AssetDatabase.SaveAssets();
// 		}
// #endif


	}
}