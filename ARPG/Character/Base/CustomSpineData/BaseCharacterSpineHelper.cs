using System;
using System.Collections.Generic;
using Global.ActionBus;
using RPGCore;
using RPGCore.Interface;
using Sirenix.OdinInspector;
using UnityEngine;
using WorldMapScene.Character;
namespace ARPG.Character.Base.CustomSpineData
{
	
	// [TypeInfoBox("使用Spine的角色动画Helper")]
	// public class BaseCharacterSpineHelper : CharacterAnimationHelperBase
	// {

// 		[NonSerialized,ShowInInspector,ReadOnly]
// 		public SpineAnimationInfo_Spine动画配置 CurrentSpineAnimationInfo;
//
// 		protected static readonly int _sp_Opacity = Shader.PropertyToID("_Opacity");
//
// 		[SpineSkin, SerializeField, LabelText("初始皮肤_服饰"), FoldoutGroup("配置/Spine", true)]
// 		protected string _sf_InitSpineSkin_Cloth;
// 		[SerializeField, LabelText("包含皮肤吗"), FoldoutGroup("配置/Spine", true)]
// 		protected bool ContainSkin;
// 		[SpineSkin, SerializeField, LabelText("初始皮肤_武器"), FoldoutGroup("配置/Spine", true), ShowIf(nameof(ContainSkin))]
// 		protected string _sf_InitSpineSkin_Weapon;
//
//
// 		[SpineAnimation, SerializeField, LabelText("Idle动画"), FoldoutGroup("配置/Spine", true)]
// 		public string IdleAnimation;
//
//
// 		[ShowInInspector, ReadOnly, LabelText("当前Spine皮肤组合"), FoldoutGroup("运行时", true)]
// 		protected Skin _currentSkinCombine;
//
//
//
//
//
// 		[ShowInInspector, LabelText("Spine - 动画"), FoldoutGroup("运行时", true)]
// 		public SkeletonAnimation SelfSkeleton;
//
// 		[ShowInInspector, LabelText("Spine - Data"), FoldoutGroup("运行时", true)]
// 		public SkeletonData SelfSkeletonData;
//
// 		[ShowInInspector, ReadOnly, LabelText("Spine - 动画状态"), FoldoutGroup("运行时", true)]
// 		public Spine.AnimationState SelfAnimationState;
//
//
// 		public override void InstantiateOnInitialize(LocalActionBus localActionBus, RolePlay_ArtHelperBase artH)
// 		{
// 			base.InstantiateOnInitialize(localActionBus, artH);
//
// 			SelfSkeleton = GetComponent<SkeletonAnimation>();
//
// 			if (SelfSkeleton != null)
// 			{
// 				SelfAnimationState = SelfSkeleton.AnimationState;
// 				SelfSkeletonData = SelfSkeleton.Skeleton.Data;
// 				if (ContainSkin)
// 				{
// 					_currentSkinCombine = new Skin("InitDefault");
// 					_currentSkinCombine.AddSkin(SelfSkeletonData.FindSkin(_sf_InitSpineSkin_Cloth));
// 					_currentSkinCombine.AddSkin(SelfSkeletonData.FindSkin(_sf_InitSpineSkin_Weapon));
// 					SelfSkeleton.Skeleton.SetSkin(_currentSkinCombine);
// 				}
// 				SelfSkeleton.Skeleton.SetSlotsToSetupPose();
// 				SelfAnimationState.Data.DefaultMix = 0f;
//
// 				_relatedMR = GetComponent<MeshRenderer>();
//
// 			}
// 			else
// 			{
// 				DBug.LogWarning($"{this.name}没有Spine组件，无法初始化Spine皮肤组合");
// 			}
// 		}
// 		public override void UpdateTick(float ct, int cf, float delta)
// 		{
// 			
// 		}
// 		protected override void SetAnimationLogicSpeedMul(float oriMul, float newMul)
// 		{
// 			TrackEntry current = SelfAnimationState.GetCurrent(0);
// 			float oriSpeed = current.TimeScale / oriMul;
// 			//用当前的实际速度除掉更换之前的乘数，就是原始的动画的速度
// 			//那么新的速度就是原始速度再乘 乘数
// 			float newSpeed = oriSpeed * newMul;
// 			current.TimeScale = newSpeed;
// 		}
// 		public override void SetMainTint(Color color)
// 		{
// 			
// 			
// 			
// 		}
//
// #if UNITY_EDITOR
// 		[InfoBox("！！！！！需要进行的手动操作！！！！！！！\n" +
// 		         "！双击一下底下的 Transparent Mode ，让它保持开启！\n" +
// 		         "双击打开 Lighting 中的 UseTraditionalLightBlend 和 EnableAdditionalLights ,两个强度设成1\n" +
// 		         "关闭深度写入！渲染顺序设置为3000")]
// 		[Button("【调整材质】设置材质的shader到 RealToon。")]
// 		private void _button_SetShader()
// 		{
// 			var mr = GetComponent<MeshRenderer>();
// 			var sharedMaterial = mr.sharedMaterial;
// 			var realToonShader = UnityEngine.AddressableAssets.Addressables
// 				.LoadAssetAsync<Shader>("RealToon_DefaultShader.shader").WaitForCompletion();
//
// 			sharedMaterial.shader = realToonShader;
// 			
// 			//TODO  贴图调整为  Alpha is transparency + point filter
// 			//get texture asset
// 			var texture = sharedMaterial.GetTexture("_MainTex");
// 			var path =UnityEditor.AssetDatabase.GetAssetPath(texture);
// 			var textureAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture>(path);
// 			UnityEditor.TextureImporter importer =
// 				UnityEditor.AssetImporter.GetAtPath(path) as UnityEditor.TextureImporter;
// 			importer.alphaIsTransparency = true;
// 			importer.filterMode = FilterMode.Point;
// 			importer.textureCompression = UnityEditor.TextureImporterCompression.Uncompressed;
// 			importer.SaveAndReimport();
// 			UnityEditor.AssetDatabase.SaveAssets();
// 			
// 			var path_Mat = UnityEditor.AssetDatabase.GetAssetPath(sharedMaterial);
// 			var matAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path_Mat);
//
//
// 			//Culling调整为双面
// 			matAsset.SetInt("_Culling", 0);
// 			
// 			//开启透明模式
// 			matAsset.SetFloat("_TRANSMODE", 1f);
// 			matAsset.EnableKeyword("N_F_TRANS_ON");
// 			//主色调整为1
// 			matAsset.SetFloat("_MaiColPo", 1f);
// 			//关闭Outline
// 			matAsset.SetFloat("_N_F_O", 0.0f);
// 			
// 			//关闭自阴影
// 			matAsset.SetFloat("_N_F_SS", 0.0f);
// 			
// 			//关闭Cutout
// 			matAsset.DisableKeyword("N_F_CO_ON");
// 			
// 			
//
// 			//接受环境光照
// 			matAsset.SetFloat("_RELG", 1);
// 			matAsset.EnableKeyword("N_F_USETLB_ON");
// 			matAsset.SetFloat("_UseTLB", 1f);
// 			matAsset.EnableKeyword("N_F_EAL_ON");
// 			matAsset.SetFloat("_N_F_EAL", 1f);
// 			matAsset.SetFloat("_DirectionalLightIntensity", 1f);
// 			matAsset.SetFloat("_PointSpotlightIntensity", 1f);
// 			
// 			//开启自发光
// 			matAsset.SetFloat("_N_F_SL", 1f);
// 			matAsset.SetColor("_SelfLitColor",Color.white);
// 			matAsset.SetFloat("_SelfLitIntensity", 0.5f);
// 			matAsset.SetFloat("_SelfLitPower", 1f);
// 			
// 			
// 			
//
// 			//关闭深度写入
// 			matAsset.SetInt("_ZWrite",0);
//
// 			matAsset.renderQueue = 3000;
// 			matAsset.enableInstancing = true;
//
//
//
//
// 			UnityEditor.EditorUtility.SetDirty(matAsset);
// 			UnityEditor.AssetDatabase.SaveAssets();
//
// 			
// 			
// 		}
//
// 		// [Button("设置材质的Shader 到 SkeletonLit 。一般的小怪就用这个")]
// 		// private void _Button_SetShaderToSkeletonLit()
// 		// {
// 		// 	var mr = GetComponent<MeshRenderer>();
// 		// 	var sharedMaterial = mr.sharedMaterial;
// 		// 	var skeletonLitShader = UnityEngine.AddressableAssets.Addressables
// 		// 		.LoadAssetAsync<Shader>("Spine-SkeletonLit-URP.shader").WaitForCompletion();
// 		// 	sharedMaterial.shader = skeletonLitShader;
// 		//
// 		// 	sharedMaterial.SetInt("_StraightAlphaInput", 1);
// 		// 	sharedMaterial.SetInt("_ReceiveShadows", 1);
// 		// 	sharedMaterial.SetInt("_DoubleSidedLighting", 1);
// 		//
// 		// 	sharedMaterial.SetFloat("_StencilComp", 5);
// 		// 	sharedMaterial.enableInstancing = true;
// 		//
// 		//
// 		//
// 		// 	UnityEditor.EditorUtility.SetDirty(sharedMaterial);
// 		// 	UnityEditor.AssetDatabase.SaveAssets();
// 		// }
// #endif

	// }
}