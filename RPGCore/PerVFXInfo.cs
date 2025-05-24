using System;
using System.Collections;
using System.Collections.Generic;
using ARPG.Character.Enemy.AI;
using ARPG.Manager;
using DG.Tweening;
using Global;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
namespace RPGCore
{


	[Serializable]
	public struct VFXAnchorInfoPair
	{
		public PerVFXInfo.PresetAnchorPosTypeEnum AnchorType;
		[ShowIf("@(this.AnchorType == PerVFXInfo.PresetAnchorPosTypeEnum.AnchorCustom_挂点到自定义配置名 || " +
		        "this.AnchorType == PerVFXInfo.PresetAnchorPosTypeEnum.PosOnlyCustom_位置同步到自定义配置名)"),
		 GUIColor(206f / 255f, 177f / 255f, 227f / 255f)]
		public string AnchorName;
	}

	/// <summary>
	/// <para>用于Buff、技能、决策的特效配置。这种配置除了特效本身之外，还包含了与gameplay有关的各类细节配置，比如挂点、尺寸、旋转等</para>
	/// <para>这种特效可以选择是否池化。他们匹配的也是prefab。</para>
	/// </summary>
	[Serializable]
	public class PerVFXInfo
	{

#region 静态

		public static PerVFXInfo GetDeepCopy(PerVFXInfo copyFrom)
		{
			if (copyFrom == null)
			{
				return null;
			}

			PerVFXInfo copyTo = new PerVFXInfo();

			// Copy all the value type properties
			copyTo._VFX_InfoID = copyFrom._VFX_InfoID;
			copyTo.IsSingletonVFX = copyFrom.IsSingletonVFX;
			copyTo.DisableAfterSingletonStop = copyFrom.DisableAfterSingletonStop;
			copyTo.NeedAlphaFadeOut = copyFrom.NeedAlphaFadeOut;
			copyTo.AlphaFadeOutDuration = copyFrom.AlphaFadeOutDuration;
			copyTo.ContainPresetTransformOffset = copyFrom.ContainPresetTransformOffset;
			copyTo.ContainPresetAnchorHolder = copyFrom.ContainPresetAnchorHolder;
			copyTo.ContainMultipleConfig = copyFrom.ContainMultipleConfig;
			copyTo.PresetAnchorHolderIndex = copyFrom.PresetAnchorHolderIndex;
			copyTo.CustomConfigName = copyFrom.CustomConfigName;
			copyTo.ContainVFXScaleOperation = copyFrom.ContainVFXScaleOperation;
			copyTo.ScaleUseAbsoluteScale = copyFrom.ScaleUseAbsoluteScale;
			copyTo.AbsoluteScale = copyFrom.AbsoluteScale;
			copyTo.ScaleNotIncludeCharacterScale = copyFrom.ScaleNotIncludeCharacterScale;
			copyTo.ScaleNotIncludeOriginalArtScale = copyFrom.ScaleNotIncludeOriginalArtScale;
			copyTo.ScaleIncludeArtHelperVisualRadius = copyFrom.ScaleIncludeArtHelperVisualRadius;
			copyTo.PresetLocalScaleMul = copyFrom.PresetLocalScaleMul;
			copyTo.ContainPresetRotationOffset = copyFrom.ContainPresetRotationOffset;
			copyTo.RotationAsOverride = copyFrom.RotationAsOverride;
			copyTo.RotationInWorld = copyFrom.RotationInWorld;
			copyTo.PresetRotationEuler = copyFrom.PresetRotationEuler;
			copyTo.ContainPresetPositionOffset = copyFrom.ContainPresetPositionOffset;
			copyTo.PositionOffsetOnWorld = copyFrom.PositionOffsetOnWorld;
			copyTo.OffsetLength = copyFrom.OffsetLength;

			// Copy all the reference type properties
			// For GameObject, we just assign the reference because Unity does not support cloning GameObjects
			copyTo.Prefab = copyFrom.Prefab;

			// For lists, we need to create a new list and add a copy of each item in the original list
			if (copyFrom.RelatedDamageTypes != null)
			{
				copyTo.RelatedDamageTypes = new List<DamageTypeEnum>(copyFrom.RelatedDamageTypes);
			}

			if (copyFrom.CustomMultipleConfigNameLists != null)
			{
				copyTo.CustomMultipleConfigNameLists =
					new List<VFXAnchorInfoPair>(copyFrom.CustomMultipleConfigNameLists);
			}

			// Return the deep copied object
			return copyTo;
		}

		public delegate DamageTypeEnum GetDamageTypeDelegate();


		public static void VFX_StopAll(List<PerVFXInfo> infoList)
		{
			foreach (PerVFXInfo perInfo in infoList)
			{
				if (perInfo.CurrentActiveRuntimePSPlayProxyRef == null)
				{
				}
				else
				{
					//独占的，那就直接置为Destroy或者直接就地Destroy
					if (perInfo.IsSingletonVFX)
					{
						perInfo.VFX_StopThis();
					}
					//非独占的，
					else
					{
						if (perInfo.CurrentActiveRuntimePSPlayProxyRefList != null &&
						    perInfo.CurrentActiveRuntimePSPlayProxyRefList.Count > 0)
						{
							foreach (var perProxy in perInfo.CurrentActiveRuntimePSPlayProxyRefList)
							{
								perProxy.StopImmediately();
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// 清理所有的VFX → 销毁或者退回它们。
		/// <para></para>
		/// </summary>
		public static void VFX_GeneralClear(List<PerVFXInfo> infoList, bool immediateStop = false)
		{
			foreach (PerVFXInfo perInfo in infoList)
			{
				//俩都是空的，那就压根没播放过，直接跳过
				if (perInfo.CurrentActiveRuntimePSPlayProxyRef == null && perInfo.CurrentMainPSRef == null)
				{
					continue;
				}

				if (perInfo.IsSingletonVFX)
				{
					perInfo.VFX_StopThis(immediateStop);
					
					
				}
			}
		}




		public static PerVFXInfo _VFX_GetByUID(
			List<PerVFXInfo> allVFXInfoList,
			string uid,
			bool containDamageVariant,
			GetDamageTypeDelegate getDamageType = null,
			string from = "未传入")
		{
			if(allVFXInfoList.Count == 0)
			{
				return null;
			}
			if (!containDamageVariant)
			{
				var index = allVFXInfoList.FindIndex((info =>
					string.Equals(uid, info._VFX_InfoID, StringComparison.OrdinalIgnoreCase)));
				if (index == -1)
				{
					DBug.LogError($"{from}内部查找了一个VFX{uid}，但是没有找到，将会返回空");
					return null;
				}
				else
				{
					int findIndex = allVFXInfoList.FindIndex((info =>
						string.Equals(uid, info._VFX_InfoID, StringComparison.OrdinalIgnoreCase)));
					if (findIndex == -1)
					{
						return null;
					}
					else
					{
						var t = allVFXInfoList[findIndex];
						if (t.Prefab == null)
						{
							DBug.LogError($"VFXInfo {t._VFX_InfoID} 没有指定Prefab，无法获取PSHandle");
							return null;
						}
						
						return t;
					}
				}
			}
			else
			{
				if (getDamageType == null)
				{
					DBug.LogError($"{from}在查找VFX信息ID时，要求了伤害变体，但是并没有传入获取变体所用的委托。这不合理");
					return null;
				}
				var findIndex = allVFXInfoList.FindIndex((info =>
					string.Equals(uid, info._VFX_InfoID, StringComparison.OrdinalIgnoreCase) &&
					info.RelatedDamageTypes.Contains(getDamageType.Invoke())));
				if (findIndex == -1)
				{
					findIndex = allVFXInfoList.FindIndex((info =>
						string.Equals(uid, info._VFX_InfoID, StringComparison.OrdinalIgnoreCase) &&
						info.RelatedDamageTypes.Contains(DamageTypeEnum.NoType_无属性)));
				}

				if (findIndex == -1)
				{
					DBug.LogWarning($"{from}内部查找了一个VFX{uid}，它指定了伤害类型变种，但是配置里面里连个无属性的都没有");
					return null;
				}
				else
				{
					var t = allVFXInfoList[findIndex];
					if (t.Prefab == null)
					{
						DBug.LogError($"VFXInfo {t._VFX_InfoID} 没有指定Prefab，无法获取PSHandle");
						return null;
					}
					return t;
				}
			}
		}


		public static PerVFXInfo _VFX_GetAndSetBeforePlay(
			List<PerVFXInfo> allVFXInfoList,
			string uid,
			I_RP_ContainVFXContainer container,
			bool containDamageVariant,
			GetDamageTypeDelegate getDamageType = null,
			string from = "未传入")
		{
			return _VFX_GetByUID(allVFXInfoList, uid, containDamageVariant, getDamageType, from)._VFX_GetPSHandle(true,
				container);
		}


		public bool IsValid()
		{
			if (Prefab == null)
			{
				return false;
			}
			if (RelatedDamageTypes == null || RelatedDamageTypes.Count < 1)
			{
				return false;
			}
			return true;
		}

#endregion


		/*
		 * 唯一的实例，那它在被使用的时候必然不会额外产生更多的实例，如果之前的实例还在活跃，那就会关闭它然后重播。
		 * 实例当然也可以不唯一，这种时候它可以走通用VFX池，也可以不走就单纯播后不理(StopAction为Destroy)
		 *
		 */

		[SerializeField]
		[LabelText("特效的UID", SdfIconType.InfoCircle), GUIColor(187f / 255f, 1f, 0f)]
		public string _VFX_InfoID;


		[SerializeField, AssetsOnly, LabelText("关联的特效Prefab")]
		public GameObject Prefab;

		[SerializeField, LabelText("实例唯一？")]
		public bool IsSingletonVFX = true;

		[SerializeField, LabelText("√:使用完成后关闭 | 口:使用完成后销毁")] [ShowIf(nameof(IsSingletonVFX))]
		public bool DisableAfterSingletonStop = true;


		[LabelText("匹配的伤害类型们")]
		public List<DamageTypeEnum> RelatedDamageTypes = new List<DamageTypeEnum>()
		{
			DamageTypeEnum.NoType_无属性
		};



		[SerializeField, LabelText("  停止时需要[透明度淡出]")] [ShowIf(nameof(IsSingletonVFX))]
		public bool NeedAlphaFadeOut = false;

		[SerializeField, LabelText("    透明度淡出时长")] [ShowIf("@(this.IsSingletonVFX && this.NeedAlphaFadeOut)")]
		public float AlphaFadeOutDuration = 0.5f;



		[SerializeField]
		[LabelText("包含预配置的形变修正")]
		public bool ContainPresetTransformOffset = true;
		[SerializeField]
		[FoldoutGroup("【形变配置】",VisibleIf =  "@this.ContainPresetTransformOffset")]
		[LabelText("包含预配置的挂点或位置点"), ShowIf(nameof(ContainPresetTransformOffset))]
		public bool ContainPresetAnchorHolder = true;

		public enum PresetAnchorPosTypeEnum
		{
			None_无 = 0,
			AnchorOnlyPos_挂点到仅自身 = 1,
			AnchorOnlyScale_挂点到仅缩放 = 2,
			AnchorScaleRotation_挂点到缩放和旋转 = 3,
			AnchorSRF_挂点到缩放旋转翻转 = 4,
			AnchorCustom_挂点到自定义配置名 = 5,
			PosOnlyPos_位置同步到仅自身 = 6,
			PosOnlyCustom_位置同步到自定义配置名 = 7,
			PosSRF_位置同步到旋转缩放翻转 = 8, }


		[SerializeField]
		[LabelText("        配置可能出现多个？")]
		[ShowIf("@(this.ContainPresetAnchorHolder)")]
		[FoldoutGroup("【形变配置】")]
		public bool ContainMultipleConfig = false;

		[InfoBox("上面的CustomConfigName只用于单一的配置。在某些情况下，比如某些特效对于某些敌人有专属的挂点，这时候就使用下面这种配置方式。\n" +
		         "在[多个配置]的情况下，会从上往下开始查找，查找到任意一个符合的配置，就会在最先查找到的配置上播放，而不会继续匹配后续的配置了")]
		[LabelText("       多配置-会选取第一个查找到的"), GUIColor(206f / 255f, 177f / 255f, 227f / 255f)]
		[ShowIf("@(this.ContainMultipleConfig && this.ContainPresetAnchorHolder)")]
		[SerializeField]
		[FoldoutGroup("【形变配置】")]
		public List<VFXAnchorInfoPair> CustomMultipleConfigNameLists;



		[SerializeField]
		[ShowIf("@(this.ContainPresetAnchorHolder && !this.ContainMultipleConfig)")]
		[LabelText("    预配置的挂点或位置点")]
		[FoldoutGroup("【形变配置】")]
		public PresetAnchorPosTypeEnum PresetAnchorHolderIndex = PresetAnchorPosTypeEnum.PosOnlyPos_位置同步到仅自身;



		[SerializeField]
		[FoldoutGroup("【形变配置】")]
		[LabelText("    自定义配置名"), GUIColor(206f / 255f, 177f / 255f, 227f / 255f)]
		[ShowIf("@((this.PresetAnchorHolderIndex == PresetAnchorPosTypeEnum.AnchorCustom_挂点到自定义配置名 || " +
		        "this.PresetAnchorHolderIndex == PresetAnchorPosTypeEnum.PosOnlyCustom_位置同步到自定义配置名)&& !this.ContainMultipleConfig && this.ContainPresetAnchorHolder)")]
		public string CustomConfigName;



		[FoldoutGroup("【形变配置】")]
		[FormerlySerializedAs("ContainPresetScaleOffset")] [SerializeField]
		[LabelText("包含尺寸上的额外修正？"), ShowIf(nameof(ContainPresetTransformOffset))]
		public bool ContainVFXScaleOperation = true;
		
		[FoldoutGroup("【形变配置】")]
		[LabelText("    √:使用绝对特效尺寸 | 口:计算层级尺寸")]
		[ShowIf(nameof(ContainVFXScaleOperation))] [SerializeField]
		public bool ScaleUseAbsoluteScale = false;

		[FoldoutGroup("【形变配置】")]
		[LabelText("    绝对特效世界尺寸")] [ShowIf("@(this.ContainVFXScaleOperation && this.ScaleUseAbsoluteScale)")]
		[SerializeField]
		public float AbsoluteScale = 1f;

		[FoldoutGroup("【形变配置】")]
		[LabelText("    Anchor时剔除角色层级尺寸？")]
		[ShowIf("@(this.ContainVFXScaleOperation && !this.ScaleUseAbsoluteScale)")] [SerializeField]
		public bool ScaleNotIncludeCharacterScale = false;

		[FoldoutGroup("【形变配置】")]
		[LabelText("    计算开始时 → √:从1开始计算 | 口:从prefab尺寸开始计算")]
		[ShowIf("@(this.ContainVFXScaleOperation && !this.ScaleUseAbsoluteScale)")] [SerializeField]
		public bool ScaleNotIncludeOriginalArtScale = false;

		[FoldoutGroup("【形变配置】")]
		[LabelText("    √:乘算关联角色ArtHelper上所记录的视觉半径 | 口:不乘算它")]
		[ShowIf("@(this.ContainVFXScaleOperation && !this.ScaleUseAbsoluteScale)")] [SerializeField]
		public bool ScaleIncludeArtHelperVisualRadius = true;

		[FoldoutGroup("【形变配置】")]
		[LabelText("    额外乘算的尺寸")] [ShowIf("@(this.ContainVFXScaleOperation && !this.ScaleUseAbsoluteScale)")]
		[SerializeField]
		public float PresetLocalScaleMul = 1f;




		[FoldoutGroup("【形变配置】")]
		[LabelText("包含预配置的旋转修正"), ShowIf(nameof(ContainPresetTransformOffset))] [SerializeField]
		public bool ContainPresetRotationOffset = false;

		[FoldoutGroup("【形变配置】")]
		[LabelText("    √ 覆盖旋转 ； 口 增加旋转"),
		 ShowIf("@(this.ContainPresetTransformOffset && this.ContainPresetRotationOffset)")] [SerializeField]
		public bool RotationAsOverride = true;

		[FoldoutGroup("【形变配置】")]
		[LabelText("    √ 修改世界旋转  ；  口 修改本地旋转"),
		 ShowIf("@(this.ContainPresetTransformOffset && this.ContainPresetRotationOffset)")] [SerializeField]
		public bool RotationInWorld = true;

		[FoldoutGroup("【形变配置】")]
		[LabelText("    预配置的旋转欧拉：ZXY顺序"),
		 ShowIf("@(this.ContainPresetTransformOffset && this.ContainPresetRotationOffset)")] [SerializeField]
		public Vector3 PresetRotationEuler;



		[FoldoutGroup("【形变配置】")]
		[LabelText("包含预配置的位置修正"), ShowIf(nameof(ContainPresetTransformOffset))] [SerializeField]
		public bool ContainPresetPositionOffset;

		[FoldoutGroup("【形变配置】")]
		[LabelText("    √ 在世界上修正 ： 口 在本地修正"),
		 ShowIf("@(this.ContainPresetTransformOffset && this.ContainPresetPositionOffset)")] [SerializeField]
		public bool PositionOffsetOnWorld = true;


		[FoldoutGroup("【形变配置】")]
		[LabelText("    在三个轴各修正多少：X右 | Y上 | Z前"),
		 ShowIf("@(this.ContainPresetTransformOffset && this.ContainPresetPositionOffset)")] [SerializeField]
		public Vector3 OffsetLength;



		private (PresetAnchorPosTypeEnum, string) _getAnchorOrPosToDataCache;


		/// <summary>
		/// 是否需要手动模拟？为null则还没有指定，它会在首次获得时判断；true则需要进行Tick，并Simulate()；false则不用管它
		/// </summary>
		public Nullable<bool> NeedManualSimulate { get; private set; }

		[NonSerialized]
		protected ParticleSystem CurrentMainPSRef;

		public ParticleSystem GetCurrentMainPSRef => CurrentMainPSRef;

		[NonSerialized]
		protected List<ParticleSystem> CurrentMainPSList;


		[NonSerialized]
		protected VFX_ParticleSystemPlayProxy CurrentActiveRuntimePSPlayProxyRef;

		public VFX_ParticleSystemPlayProxy GetCurrentActiveRuntimePSPlayProxyRef => CurrentActiveRuntimePSPlayProxyRef;

		[NonSerialized]
		protected List<VFX_ParticleSystemPlayProxy> CurrentActiveRuntimePSPlayProxyRefList;



		[NonSerialized]
		protected I_RP_ContainVFXContainer _selfRelatedContainerRef;


#region 对外获取

		public PerVFXInfo GetRawHandler(
			BaseAIBrainHandler handler,
			string uid,
			I_RP_ContainVFXContainer container = null,
			bool withDamageTypeVariant = true,
			PerVFXInfo.GetDamageTypeDelegate getDamageType = null,
			string from = null)
		{
			return PerVFXInfo._VFX_GetByUID(handler.AllVFXInfoList_RuntimeAll,
				uid,
				withDamageTypeVariant,
				getDamageType,
				from);
		}

		public PerVFXInfo GetPresetHandler(
			BaseAIBrainHandler handler,
			string uid,
			bool needApplyTransformOffset = true,
			I_RP_ContainVFXContainer container = null,
			bool withDamageTypeVariant = true,
			PerVFXInfo.GetDamageTypeDelegate getDamageType = null,
			string from = null)
		{
			PerVFXInfo selfVFXInfo = PerVFXInfo._VFX_GetByUID(handler.AllVFXInfoList_RuntimeAll,
				uid,
				withDamageTypeVariant,
				getDamageType,
				from);
			return selfVFXInfo._VFX_GetPSHandle(needApplyTransformOffset, container);
		}

#endregion


#region 获取 & 播放

		private void CheckIfManualSimulate()
		{
			if (NeedManualSimulate != null)
			{
				return;
			}
#if UNITY_EDITOR
			if (!Prefab)
			{
				DBug.LogError($"VFXInfo {_VFX_InfoID} 没有指定Prefab");
				return;
			}
			if (!Prefab.TryGetComponent(out ParticleSystem ps))
			{
				DBug.LogError($"VFXInfo {_VFX_InfoID} 的Prefab没有粒子系统，这是不允许的");
				return;
			}
#endif
			if (Prefab.TryGetComponent(out VFX_ParticleSystemPlayProxy pspp))
			{
				NeedManualSimulate = true;
			}
			else
			{
				NeedManualSimulate = false;
			}
		}

		/// <summary>
		/// <para>获取当前的PS，根据是否为单例、是否使用通用池，返回一个合理的ParticleSystem。可能会是新的，也可能是池里的</para>
		/// <para>needNewIfExist 对于非唯一的实例，如果当前ref还活跃着，是否需要重新生成一份？如果不需要新的，那就是重播当前的</para>
		/// </summary>
		public PerVFXInfo _VFX_GetPSHandle(
			bool needApplyTransformOffset = true,
			I_RP_ContainVFXContainer container = null)
		{
			CheckIfManualSimulate();
			if (NeedManualSimulate == null)
			{
				return this;
			}
			//是唯一的，则通常来说它的StopAction会是Disable来循环使用。

			if (IsSingletonVFX)
			{
				if (NeedManualSimulate.Value)
				{
					if (CurrentActiveRuntimePSPlayProxyRef == null)
					{
						var newObj = UnityEngine.Object.Instantiate(Prefab);
						CurrentActiveRuntimePSPlayProxyRef = newObj.GetComponent<VFX_ParticleSystemPlayProxy>();
						CurrentActiveRuntimePSPlayProxyRef.SetMainPS(CurrentActiveRuntimePSPlayProxyRef
							.GetComponent<ParticleSystem>());
						if (DisableAfterSingletonStop)
						{
							var mainModule = CurrentActiveRuntimePSPlayProxyRef.MainParticleSystemRef.main;
							mainModule.stopAction = ParticleSystemStopAction.Disable;
						}
						else
						{
							var mainModule = CurrentActiveRuntimePSPlayProxyRef.MainParticleSystemRef.main;
							mainModule.stopAction = ParticleSystemStopAction.Destroy;
						}
					}
				}
				else
				{
					if (CurrentMainPSRef == null)
					{
						CurrentMainPSRef = UnityEngine.Object.Instantiate(Prefab).GetComponent<ParticleSystem>();
						if (DisableAfterSingletonStop)
						{
							var mainModule = CurrentMainPSRef.main;
							mainModule.stopAction = ParticleSystemStopAction.Disable;
						}
						else
						{
							var mainModule = CurrentMainPSRef.main;
							mainModule.stopAction = ParticleSystemStopAction.Destroy;
						}
						foreach (ParticleSystem perPS in CurrentMainPSRef.GetComponentsInChildren<ParticleSystem>(true))
						{
							if (perPS == CurrentMainPSRef)
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
									Debug.LogError($"【特效】{perPS.name}的停止行为被修改为【None】，请注意检查。其父级为{CurrentMainPSRef.gameObject.name}");
#endif
								}
							}
						}
					}
					
				}
			}
			//不是唯一的
			else
			{
				if (NeedManualSimulate.Value)
				{
					if (CurrentActiveRuntimePSPlayProxyRefList == null)
					{
						CurrentActiveRuntimePSPlayProxyRefList = new List<VFX_ParticleSystemPlayProxy>();
					}
					CurrentActiveRuntimePSPlayProxyRef = VFXPoolManager.Instance.GetPSPPRuntimeByPrefab(Prefab);
					CurrentActiveRuntimePSPlayProxyRefList.Add(CurrentActiveRuntimePSPlayProxyRef);
				}
				else
				{
					if (CurrentMainPSList == null)
					{
						CurrentMainPSList = new List<ParticleSystem>();
					}
					CurrentMainPSRef = VFXPoolManager.Instance.GetParticleSystemRuntimeByPrefab(Prefab, true);
					CurrentMainPSList.Add(CurrentMainPSRef);
				}
			}

			_VFX_0_ResetTransform();
			if (needApplyTransformOffset)
			{
				_VFX_1_ApplyPresetTransform(container);
			}
			return this;
		}


		private void Replay()
		{
			if (NeedManualSimulate.Value)
			{
				CurrentActiveRuntimePSPlayProxyRef.Replay();
			}
			else
			{
				CurrentMainPSRef.Clear();
				CurrentMainPSRef.Play();
			}
		}

#endregion



		/// <summary>
		/// <para>重置形变。包括把自己重新放到VFXPool，重置 TRS，重置透明度</para>
		/// </summary>
		/// <returns></returns>
		public PerVFXInfo _VFX_0_ResetTransform()
		{
			var trans = NeedManualSimulate.Value ? CurrentActiveRuntimePSPlayProxyRef.transform
				: CurrentMainPSRef.transform;
			trans.SetParent(VFXPoolManager.Instance.transform);
			trans.localPosition = Vector3.zero;
			trans.localRotation = Quaternion.identity;
			trans.localScale = Vector3.one;
			if (IsSingletonVFX && NeedAlphaFadeOut)
			{
				var _selfPSRendererArray = trans.GetComponentsInChildren<ParticleSystemRenderer>();
				if (_selfPSRendererArray != null && _selfPSRendererArray.Length > 0)
				{
					foreach (ParticleSystemRenderer perSR in _selfPSRendererArray)
					{
						perSR.material.SetFloat("_AlphaFinalMul", 1f);
					}
				}
			}
			return this;
		}



		private void GetAnchorOrPosToData(
			PresetAnchorPosTypeEnum type,
			string config,
			I_RP_ContainVFXContainer container,
			out (Transform, float) getOnAnchor,
			out (Vector3?, float) getOnPos)
		{
			getOnAnchor = (null, 1f);
			getOnPos = (null, 1f);
			switch (type)
			{
				case PresetAnchorPosTypeEnum.AnchorOnlyPos_挂点到仅自身:
					getOnAnchor = container.GetVFXHolderTransformAndRegisterVFX(
						ConSer_VFXHolderInfo._VFXAnchorName_OnlySelf,
						CurrentActiveRuntimePSPlayProxyRef);
					if (getOnAnchor.Item1)
					{
						Transform tran_aop = NeedManualSimulate.Value ? CurrentActiveRuntimePSPlayProxyRef.transform
							: CurrentMainPSRef.transform;

						tran_aop.transform.SetParent(getOnAnchor.Item1, false);
					}
					break;
				case PresetAnchorPosTypeEnum.AnchorOnlyScale_挂点到仅缩放:
					getOnAnchor = container.GetVFXHolderTransformAndRegisterVFX(
						ConSer_VFXHolderInfo._VFXAnchorName_OnlyScale,
						CurrentActiveRuntimePSPlayProxyRef);
					if (getOnAnchor.Item1)
					{
						var tran_aos = NeedManualSimulate.Value ? CurrentActiveRuntimePSPlayProxyRef.transform
							: CurrentMainPSRef.transform;
						tran_aos.transform.SetParent(getOnAnchor.Item1, false);
					}
					break;
				case PresetAnchorPosTypeEnum.AnchorScaleRotation_挂点到缩放和旋转:
					getOnAnchor = container.GetVFXHolderTransformAndRegisterVFX(
						ConSer_VFXHolderInfo._VFXAnchorName_ScaleAndRotate,
						CurrentActiveRuntimePSPlayProxyRef);
					if (getOnAnchor.Item1)
					{
						var tran_asr = NeedManualSimulate.Value ? CurrentActiveRuntimePSPlayProxyRef.transform
							: CurrentMainPSRef.transform;

						tran_asr.transform.SetParent(getOnAnchor.Item1, false);
					}
					break;
				case PresetAnchorPosTypeEnum.AnchorSRF_挂点到缩放旋转翻转:
					getOnAnchor = container.GetVFXHolderTransformAndRegisterVFX(ConSer_VFXHolderInfo._VFXAnchorName_SRF,
						CurrentActiveRuntimePSPlayProxyRef);
					if (getOnAnchor.Item1)
					{
						var tran_asrf = NeedManualSimulate.Value ? CurrentActiveRuntimePSPlayProxyRef.transform
							: CurrentMainPSRef.transform;
						tran_asrf.transform.SetParent(getOnAnchor.Item1, false);
					}
					break;
				case PresetAnchorPosTypeEnum.AnchorCustom_挂点到自定义配置名:
					getOnAnchor =
						container.GetVFXHolderTransformAndRegisterVFX(config, CurrentActiveRuntimePSPlayProxyRef);
					if (getOnAnchor.Item1)
					{
						var tran_ac = NeedManualSimulate.Value ? CurrentActiveRuntimePSPlayProxyRef.transform
							: CurrentMainPSRef.transform;
						tran_ac.transform.SetParent(getOnAnchor.Item1, false);
					}
					break;
				case PresetAnchorPosTypeEnum.PosOnlyPos_位置同步到仅自身:
					getOnPos = container.GetVFXHolderGlobalPosition(ConSer_VFXHolderInfo._VFXAnchorName_OnlySelf);
					if (getOnPos.Item1.HasValue)
					{
						var tran_pop = NeedManualSimulate.Value ? CurrentActiveRuntimePSPlayProxyRef.transform
							: CurrentMainPSRef.transform;
						tran_pop.transform.position = getOnPos.Item1.Value;
					}
					break;
				case PresetAnchorPosTypeEnum.PosOnlyCustom_位置同步到自定义配置名:
					getOnPos = container.GetVFXHolderGlobalPosition(config);
					if (getOnPos.Item1.HasValue)
					{
						var tran_popc = NeedManualSimulate.Value ? CurrentActiveRuntimePSPlayProxyRef.transform
							: CurrentMainPSRef.transform;
						tran_popc.transform.position = getOnPos.Item1.Value;
					}
					break;
				case PresetAnchorPosTypeEnum.PosSRF_位置同步到旋转缩放翻转:
					var getOnAnchorSRF = container.GetVFXHolderTransformAndRegisterVFX(
						ConSer_VFXHolderInfo._VFXAnchorName_SRF,
						CurrentActiveRuntimePSPlayProxyRef,
						true);
					var tran_psrf = NeedManualSimulate.Value ? CurrentActiveRuntimePSPlayProxyRef.transform
						: CurrentMainPSRef.transform;
					tran_psrf.SetParent(getOnAnchorSRF.Item1, false);
					tran_psrf.localPosition = Vector3.zero;
					tran_psrf.localRotation = Quaternion.identity;
					tran_psrf.SetParent(VFXPoolManager.Instance.transform);

					break;
			}
		}



		private void GetAnchorOrPosToDataByFullConfig(
			I_RP_ContainVFXContainer container,
			out (Transform, float) getOnAnchor,
			out (Vector3?, float) getOnPos)
		{
			getOnAnchor = (null, 1f);
			getOnPos = (Vector3.zero, 1f);
			if (!ContainMultipleConfig)
			{
				GetAnchorOrPosToData(PresetAnchorHolderIndex,
					CustomConfigName,
					container,
					out getOnAnchor,
					out getOnPos);
			}
			else
			{
				if (CustomMultipleConfigNameLists == null || CustomMultipleConfigNameLists.Count == 0)
				{
					DBug.LogError("出现了一个VFXInfo，它的配置是多个配置，但是没有任何一个配置被填写。这不合理");
					return;
				}
				else
				{
					foreach (var perConfig in CustomMultipleConfigNameLists)
					{
						GetAnchorOrPosToData(perConfig.AnchorType,
							perConfig.AnchorName,
							container,
							out getOnAnchor,
							out getOnPos);
						if (getOnAnchor.Item1 || getOnPos.Item1.HasValue)
						{
							_getAnchorOrPosToDataCache = (perConfig.AnchorType, perConfig.AnchorName);
							return;
						}
					}
					DBug.LogError(" 出现特效配置在播放的时候，使用了复合配置点，但是没有任何一个配置点被查找到，这不合理。从栈上检查一下。");
				}
			}
		}


		/// <summary>
		/// <para>为该VFXInfo设置预配置的形变信息们。顺序为 挂点 、缩放 、 旋转 、 位置</para>
		/// <para></para>
		/// </summary>
		public PerVFXInfo _VFX_1_ApplyPresetTransform(I_RP_ContainVFXContainer container)
		{
			Transform trans = NeedManualSimulate.Value ? CurrentActiveRuntimePSPlayProxyRef.transform
				: CurrentMainPSRef.transform;
			_selfRelatedContainerRef = container;
			float scaleFromAnchor = 1f;
			if (ContainPresetAnchorHolder)
			{
				GetAnchorOrPosToDataByFullConfig(container,
					out (Transform, float) getOnAnchor,
					out (Vector3?, float) getOnPos);
			}
			if (ContainVFXScaleOperation)
			{
				bool flipped = trans.lossyScale.x < 0f;
				if (ScaleUseAbsoluteScale)
				{
					var worldScale = trans.lossyScale;
					Vector3 prefabLocalScale = Prefab.transform.localScale;
					Vector3 convertLocalScale = new Vector3(prefabLocalScale.x / worldScale.x,
						prefabLocalScale.y / worldScale.y,
						prefabLocalScale.z / worldScale.z);
					trans.localScale = convertLocalScale * AbsoluteScale;
				}
				else
				{
					float finalScale = ScaleNotIncludeOriginalArtScale ? 1f : Prefab.transform.localScale.x;
					if (ScaleNotIncludeCharacterScale)
					{
						if (_getAnchorOrPosToDataCache.Item1 != PresetAnchorPosTypeEnum.PosOnlyCustom_位置同步到自定义配置名 &&
						    _getAnchorOrPosToDataCache.Item1 != PresetAnchorPosTypeEnum.PosOnlyPos_位置同步到仅自身 &&
						    _getAnchorOrPosToDataCache.Item1 != PresetAnchorPosTypeEnum.PosSRF_位置同步到旋转缩放翻转)
						{
							float scaleInCharacter = container.GetSelfCharacterHierarchyScale();
							finalScale *= scaleInCharacter;
						}
					}
					if (ScaleIncludeArtHelperVisualRadius)
					{
						float scaleInArtHelper = container.GetSelfVFXScaleRadius();
						finalScale *= scaleInArtHelper;
					}
					finalScale *= PresetLocalScaleMul;
					trans.localScale = trans.localScale * finalScale;
				}
			}



			if (ContainPresetRotationOffset)
			{
				if (RotationAsOverride)
				{
					if (RotationInWorld)
					{
						trans.rotation = Quaternion.Euler(PresetRotationEuler);
					}
					else
					{
						trans.localRotation = Quaternion.Euler(PresetRotationEuler);
					}
				}
				else
				{
					if (RotationInWorld)
					{
						Vector3 currentEuler = trans.rotation.eulerAngles;
						currentEuler += PresetRotationEuler;
						trans.rotation = Quaternion.Euler(currentEuler);
					}
					else
					{
						Vector3 currentEuler = trans.localRotation.eulerAngles;
						currentEuler += PresetRotationEuler;
						trans.localRotation = Quaternion.Euler(currentEuler);
					}
				}
			}

			if (ContainPresetPositionOffset)
			{
				Vector3 offsetPos = BaseGameReferenceService.CurrentBattleLogicRightDirection * OffsetLength.x +
				                    Vector3.up * OffsetLength.y +
				                    BaseGameReferenceService.CurrentBattleLogicalForwardDirection * OffsetLength.z;
				if (PositionOffsetOnWorld)
				{
					trans.position += offsetPos;
				}
				else
				{
					trans.localPosition += offsetPos;
				}
			}
			return this;
		}

		public PerVFXInfo _VFX_2_SetPositionToGlobalPosition(Vector3 position)
		{
			var trans = NeedManualSimulate.Value ? CurrentActiveRuntimePSPlayProxyRef.transform
				: CurrentMainPSRef.transform;
			if (trans == null)
			{
				return this;
			}
			trans.position = position;
			return this;
		}




		/// <summary>
		/// <para>按照 前侧 方向指定 自己的世界旋转</para>
		/// </summary>
		public PerVFXInfo _VFX__3_SetDirectionOnForwardOnGlobalY0(Vector3 forward)
		{
			forward.y = 0f;
			forward.Normalize();
			var rot = Quaternion.LookRotation(forward, Vector3.up);
			var trans = NeedManualSimulate.Value ? CurrentActiveRuntimePSPlayProxyRef.transform
				: CurrentMainPSRef.transform;
			trans.rotation = rot;
			return this;
		}

		public PerVFXInfo _VFX__3_SetDirectionOnForwardOnGlobalAll(Vector3 forward)
		{
			forward.Normalize();
			var rot = Quaternion.LookRotation(forward, Vector3.up);
			var trans = NeedManualSimulate.Value ? CurrentActiveRuntimePSPlayProxyRef.transform
				: CurrentMainPSRef.transform;
			trans.rotation = rot;
			return this;
		}


		public PerVFXInfo _VFX__3_SetDirectionOnRightOnGlobalY0(Vector3 forward)
		{
			forward.y = 0f;
			forward.Normalize();
			var newDir = Vector3.Cross(forward, Vector3.down);
			var rot = Quaternion.LookRotation(newDir, Vector3.up);
			var trans = NeedManualSimulate.Value ? CurrentActiveRuntimePSPlayProxyRef.transform
				: CurrentMainPSRef.transform;
			trans.rotation = rot;
			return this;
		}

		public PerVFXInfo _VFX__3_SetDirectionOnRightOnLocalY0(Vector3 forward)
		{
			forward.y = 0f;
			forward.Normalize();
			var newDir = Vector3.Cross(forward, Vector3.down);
			var rot = Quaternion.LookRotation(newDir, Vector3.up);
			var trans = NeedManualSimulate.Value ? CurrentActiveRuntimePSPlayProxyRef.transform
				: CurrentMainPSRef.transform;
			trans.localRotation = rot;
			return this;
		}

		/// <summary>
		/// <para>按照 前侧 方向指定 自己的本地旋转</para>
		/// </summary>
		public PerVFXInfo _VFX__3_SetDirectionOnForwardOnLocal(Vector3 forward)
		{
			forward.y = 0f;
			forward.Normalize();
			var rot = Quaternion.LookRotation(forward, Vector3.up);
			var trans = NeedManualSimulate.Value ? CurrentActiveRuntimePSPlayProxyRef.transform
				: CurrentMainPSRef.transform;
			trans.localRotation = rot;
			return this;
		}






		/// <summary>
		/// <para>重新设置本地尺寸</para>
		/// </summary>
		/// <param name="scale"></param>
		/// <returns></returns>
		public PerVFXInfo VFX_4_SetLocalScale(float scale)
		{
			var trans = NeedManualSimulate.Value ? CurrentActiveRuntimePSPlayProxyRef.transform
				: CurrentMainPSRef.transform;
			trans.localScale = scale * Vector3.one;
			return this;
		}

		public PerVFXInfo VFX_4_AddScale(float scale, bool ignoreLossy)
		{
			//如果无视lossy，则表明就直接加到localScale上就行
			var tran = NeedManualSimulate.Value ? CurrentActiveRuntimePSPlayProxyRef.transform
				: CurrentMainPSRef.transform;
			if (ignoreLossy)
			{
				var currentScale = tran.localScale;
				currentScale.x += scale;
				currentScale.y += scale;
				currentScale.z += scale;
				tran.localScale = currentScale;
			}
			else
			{
				var mulInHierarchy = tran.lossyScale.magnitude / tran.localScale.magnitude;
				var add = scale / mulInHierarchy;
				var currentScale = tran.localScale;
				currentScale.x += add;
				currentScale.y += add;
				currentScale.z += add;
				tran.localScale = currentScale;
			}
			return this;
		}

		public PerVFXInfo VFX_4_ExtendScale(float scale, bool ignoreLossy)
		{
			var tran = NeedManualSimulate.Value ? CurrentActiveRuntimePSPlayProxyRef.transform
				: CurrentMainPSRef.transform;
			//如果无视lossy，则表明就直接加到localScale上就行
			if (ignoreLossy)
			{
				var currentScale = tran.localScale;
				currentScale.x *= scale;
				currentScale.y *= scale;
				currentScale.z *= scale;
				tran.localScale = currentScale;
			}
			else
			{
				var mulInHierarchy = tran.lossyScale.magnitude / tran.localScale.magnitude;
				var add = scale / mulInHierarchy;
				var currentScale = tran.localScale;
				currentScale.x *= add;
				currentScale.y *= add;
				currentScale.z *= add;
				tran.localScale = currentScale;
			}
			return this;
		}


		/// <summary>
		/// <para>修正位置，根据 世界前 来修正自己的本地位置</para>
		/// </summary>
		public PerVFXInfo _VFX__5_OffsetLocalPositionByWorldForward(float OffsetLocalToWorld)
		{
			Vector3 worldForward = BaseGameReferenceService.CurrentBattleLogicalForwardDirection;
			Vector3 offset = worldForward * OffsetLocalToWorld;


			var trans = NeedManualSimulate.Value ? CurrentActiveRuntimePSPlayProxyRef.transform
				: CurrentMainPSRef.transform;
			trans.localPosition += offset;
			return this;
		}
		/// <summary>
		/// <para>修正位置，根据 世界上 来修正自己的本地位置</para>
		/// </summary>
		public PerVFXInfo _VFX__5_OffsetLocalPositionByWorldUp(float OffsetLocalToWorld)
		{
			Vector3 worldUp = Vector3.up;
			Vector3 offset = worldUp * OffsetLocalToWorld;

			var trans = NeedManualSimulate.Value ? CurrentActiveRuntimePSPlayProxyRef.transform
				: CurrentMainPSRef.transform;
			trans.localPosition += offset;
			return this;
		}

		/// <summary>
		/// <para>修正位置，将本地位置直接设置过去</para>
		/// </summary>
		public PerVFXInfo _VFX__5_SetLocalPosition(Vector3 offset)
		{
			var trans = NeedManualSimulate.Value ? CurrentActiveRuntimePSPlayProxyRef.transform
				: CurrentMainPSRef.transform;
			trans.localPosition = offset;
			return this;
		}


		/// <summary>
		/// <para>修正位置，将自己的本地坐标额外加上这个</para>
		/// </summary>
		public PerVFXInfo _VFX__5_AddLocalPosition(Vector3 offset)
		{
			var trans = NeedManualSimulate.Value ? CurrentActiveRuntimePSPlayProxyRef.transform
				: CurrentMainPSRef.transform;
			trans.localPosition += offset;
			return this;
		}


		/// <summary>
		/// <para>递归地设置粒子的所有播放速度</para>
		/// </summary>
		/// <param name="speed"></param>
		/// <returns></returns>
		public PerVFXInfo _VFX__6_SetSimulationSpeed(float speed = 1f)
		{
			if (NeedManualSimulate.Value)
			{
				CurrentActiveRuntimePSPlayProxyRef.SimulationSpeed = speed;
			}
			else
			{
				DBug.LogError($" VFXInfo {_VFX_InfoID} 想要设置模拟速度。那这个特效应当是PSPP方式手动模拟，但现在被当成默认粒子了，这是不合理的");
			}
			return this;
		}


		public PerVFXInfo _VFX__10_PlayThis(bool forceReplay = true, bool relocated = true)
		{
			if (relocated)
			{
				if (_selfRelatedContainerRef == null)
				{
					DBug.LogError($"{_VFX_InfoID}在播放时要求重新定位，但是在此之前没有传入过Container，这是不合理的。");
					return this;
				}
				_VFX_0_ResetTransform();
				_VFX_1_ApplyPresetTransform(_selfRelatedContainerRef);
			}
			if (forceReplay)
			{
				Replay();
			}
			if (_coroutine != null)
			{
				GlobalConfigurationAssetHolderHelper.GetCurrentBaseGRS().StopCoroutine(_coroutine);
				_coroutine = null;
			}

			if (NeedManualSimulate.Value)
			{
				CurrentActiveRuntimePSPlayProxyRef.gameObject.SetActive(true);
				CurrentActiveRuntimePSPlayProxyRef.Play();
			}
			else
			{
				CurrentMainPSRef.gameObject.SetActive(true);
				CurrentMainPSRef.Play();
			}

			return this;
		}

		/// <summary>
		/// <para>停止这个特效</para>
		/// </summary>
		public PerVFXInfo VFX_StopThis(bool immediateStop = false)
		{
			if (NeedManualSimulate == null)
			{
				return this;
			}
			if (NeedManualSimulate.Value)
			{
				if (CurrentActiveRuntimePSPlayProxyRef == null)
				{
					return this;
				}
			}
			else
			{
				if (CurrentMainPSRef == null)
				{
					return this;
				}
			}
			if (NeedAlphaFadeOut && !immediateStop && CurrentMainPSRef != null && CurrentMainPSRef.gameObject.activeInHierarchy)
			{
				// if (_selfPSRendererArray == null)
				// {
				// 	_selfPSRendererArray = NeedManualSimulate.Value
				// 		? CurrentActiveRuntimePSPlayProxyRef.GetComponentsInChildren<ParticleSystemRenderer>()
				// 		: CurrentMainPSRef.GetComponentsInChildren<ParticleSystemRenderer>();
				// 	DBug.LogWarning($"VFXInfo {_VFX_InfoID} 在进行透明度淡出时，没有预先设置好的PS数组，进行了运行时获取，这不太正确");
				// }
				foreach (var perR in CurrentMainPSRef.GetComponentsInChildren<ParticleSystemRenderer>(true))
				{
					perR.material.DOFloat(0f, "_AlphaFinalMul", AlphaFadeOutDuration);
				}
				if (_coroutine != null)
				{
					GlobalConfigurationAssetHolderHelper.GetCurrentBaseGRS().StopCoroutine(_coroutine);
					_coroutine = null;
				}
				_coroutine = GlobalConfigurationAssetHolderHelper.GetCurrentBaseGRS()
					.StartCoroutine(StopVFXCoroutine(AlphaFadeOutDuration));

			}
			else
			{
				if (immediateStop)
				{
					if (NeedManualSimulate.Value)
					{
						CurrentActiveRuntimePSPlayProxyRef?.StopImmediately();
					}
					else
					{
						CurrentMainPSRef.Clear();
						CurrentMainPSRef.gameObject.SetActive(false);
						CurrentMainPSRef.Stop(true);
					}
				}
				else
				{
					if (NeedManualSimulate.Value)
					{
						CurrentActiveRuntimePSPlayProxyRef?.StopEmitNewParticle();
					}
					else
					{
						CurrentMainPSRef.Stop(true);
					}
				}
			}
			return this;
		}


		private Coroutine _coroutine;

		private IEnumerator StopVFXCoroutine(float duration)
		{
			yield return new WaitForSeconds(duration);
			if (NeedManualSimulate.Value)
			{
				CurrentActiveRuntimePSPlayProxyRef?.StopImmediately();
			}
			else
			{
				CurrentMainPSRef?.Stop(true);
				CurrentMainPSRef.gameObject.SetActive(false);
			}
		}


		public void UpdateTick(float ct, int cf, float delta)
		{
			if (NeedManualSimulate == null)
			{
				return;
			}
			if (!NeedManualSimulate.Value)
			{
				return;
			}
			if (CurrentActiveRuntimePSPlayProxyRef)
			{
				CurrentActiveRuntimePSPlayProxyRef.UpdateTick(ct, cf, delta);
			}
		}




	}
}