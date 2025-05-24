using System;
using System.Collections.Generic;
using System.Linq;
using Animancer;
using ARPG.Character.Config;
using ARPG.Common.HitEffect;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Global;
using Global.ActionBus;
using RPGCore;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
namespace ARPG.Character.Base
{
	[TypeInfoBox("每个角色之下用于播放序列帧动画的Helper。区别于Spine")]
	[RequireComponent(typeof(Animator))]
	[RequireComponent(typeof(AnimancerComponent))]
	public class BaseCharacterSheetAnimationHelper : CharacterAnimationHelperBase
	{
#if UNITY_EDITOR
		[OnInspectorGUI]
		private void RedrawConstantly()
		{
			Sirenix.Utilities.Editor.GUIHelper.RequestRepaint();
			if (!Application.isPlaying)
			{
				_CheckChangeTexture();
			}
		}
#endif


#region 内部字段

		public Animator SelfAnimatorRef { get; protected set; }

		public MeshRenderer SelfMeshRendererRef { get; protected set; }

		public AnimancerComponent SelfAnimancerComponent { get; protected set; }

		[SerializeField, LabelText("默认材质"), Required, AssetsOnly]
		protected Material _defaultMaterialOfThisHelper;
		
		
		private List<AnimationClip> _currentAnimancerClips = new List<AnimationClip>();	
		
		
		[SerializeField,LabelText(""),ToggleButtons("开启影子复制Quad","不管")]
		public bool EnableShadowQuad = false;
		
		[SerializeField,LabelText("阴影Quad的y修正")]
		 [ShowIf( "EnableShadowQuad")]
		public float ShadowQuadYOffset = -0.2f;
		

		[SerializeField, LabelText("Quad GO")]
		[ShowIf("EnableShadowQuad")]
		private GameObject _GO_OriginalQuad;

		private GameObject _go_shadowQuad;

		private MeshRenderer _mr_shadowQuad;
		

#endregion


#region 外部引用

		

#endregion
#region 图组


		[SerializeField,LabelText("显式指定使用的图组")]
#if UNITY_EDITOR
		[OnValueChanged(nameof(_Editor_RefreshSpriteGroupRef))]
#endif
		private int _currentSpriteGroupIndex;


#if UNITY_EDITOR
		private void _Editor_RefreshSpriteGroupRef()
		{
			RefreshSpriteGroup();
		}
		
#endif
		public class SpriteGroupIndexContent
		{
			public int TargetSpriteIndex;
		}
		private SpriteGroupIndexContent _selfSpriteGroupIndexContent = new SpriteGroupIndexContent();

		[ShowInInspector, LabelText("当前使用的【图组】索引"), ReadOnly]
		public int CurrentSpriteGroupIndex
		{
			get => _currentSpriteGroupIndex;
			set
			{
				_currentSpriteGroupIndex = value;
				if (Application.isPlaying)
				{
					_selfSpriteGroupIndexContent.TargetSpriteIndex = _currentSpriteGroupIndex;
					var ds = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
						.L_AnimationHelper_OnSpriteSheetIndexChanged_当图片表索引改变);
					ds.ObjectArgu1 = _selfSpriteGroupIndexContent;
					_localActionBusRef?.TriggerActionByType(ds);
					_currentSpriteGroupIndex = _selfSpriteGroupIndexContent.TargetSpriteIndex;
				}
				RefreshSpriteGroup();
			}
		}


		[ShowInInspector, ReadOnly, LabelText("当前使用的【图组】")]
		public SOConfig_PerSheetAnimationGroup CurrentSpriteGroupRef { get; private set; }

		


		[Button("刷新图组")]
		private void RefreshSpriteGroup()
		{
			CurrentSpriteGroupRef = GlobalConfigurationAssetHolderHelper.GetGCAHH().Collection_SheetAnimationConfigGroup
				.GetSheetGroupByIndex(_currentSpriteGroupIndex);
			if (SelfAnimatorRef == null)
			{
				SelfAnimatorRef = GetComponent<Animator>();
			}
			
		}


		/// <summary>
		/// <para>图组中图片的索引</para>
		/// </summary>
		[OnValueChanged(nameof(ChangeTexture))]
		public int _SheetIndexInCurrentGroup_ = 0;
		private int __SheetIndexInCurrentGroup__Last = -1;



		
		

#endregion



#region 材质效果


		private Tweener _tween_HitColorTween;
		private Tweener _tween_EmissionTween;

		private static readonly int _sp_MainTexture = Shader.PropertyToID("_MainTexture");
		private static readonly int _sp_NormalTexture = Shader.PropertyToID("_NormalTex");
		private static readonly int _sp__FlipbookKeyword = Shader.PropertyToID("_Flipbook");
		private static readonly int _sp__Cell = Shader.PropertyToID("_Cell");
		private static readonly int _sp__SpriteIndex = Shader.PropertyToID("_SheetIndex");
		private static readonly int _SP_HitEffectColorTint = Shader.PropertyToID("_HitEffectColorTint");
		private static readonly int _SP__HitEffectOpacity = Shader.PropertyToID("_HitEffectOpacity");


#endregion


		[OnInspectorInit]
		private void OnInspectorInit()
		{
			if (!SelfAnimatorRef)
			{
				SelfAnimatorRef = GetComponentInChildren<Animator>();
			}
			if (!SelfAnimancerComponent)
			{
				SelfAnimancerComponent = GetComponentInChildren<AnimancerComponent>();
			}
			if (!SelfMeshRendererRef)
			{
				SelfMeshRendererRef = GetComponentInChildren<MeshRenderer>();
			}
			if (_defaultMaterialOfThisHelper == null)
			{
				_defaultMaterialOfThisHelper = GlobalConfigurationAssetHolderHelper.GetGCAHH()
					._material_DefaultCharacterQuadAnimationMaterial;

			}
			if (SelfMeshRendererRef != null)
			{
				SelfMeshRendererRef.sharedMaterial = _defaultMaterialOfThisHelper;
				_SheetIndexInCurrentGroup_ = 0;
				ChangeTexture();
			}
		}
		

		public override void InstantiateOnInitialize(LocalActionBus localActionBus, RolePlay_ArtHelperBase artH)
		{
			_SheetIndexInCurrentGroup_ = 0;
			__SheetIndexInCurrentGroup__Last = -1;
			base.InstantiateOnInitialize(localActionBus, artH);

			CurrentSpriteGroupIndex = artH.RelatedCharacterSheetGroupIndex;
			CurrentSpriteGroupRef = GlobalConfigurationAssetHolderHelper.GetGCAHH().Collection_SheetAnimationConfigGroup
				.GetSheetGroupByIndex(CurrentSpriteGroupIndex);
		
			CurrentSpriteGroupRef.PreloadAll();

			
			SelfAnimatorRef = GetComponentInChildren<Animator>();
			SelfAnimatorRef.runtimeAnimatorController = null;
			SelfAnimancerComponent = GetComponentInChildren<AnimancerComponent>();
			SelfAnimancerComponent.Animator = SelfAnimatorRef;
			SelfMeshRendererRef = GetComponentInChildren<MeshRenderer>();
			// SelfMeshRendererRef.sharedMaterial = UnityEngine.Object.Instantiate(_defaultMaterialOfThisHelper);
			SelfMeshRendererRef.material = UnityEngine.Object.Instantiate(_defaultMaterialOfThisHelper);
			var currentMat = SelfMeshRendererRef.material;
			currentMat.SetFloat("_Flipbook", 1f);
			currentMat.SetTextureOffset("_MainTexture", Vector2.zero);
			currentMat.SetTextureScale("_MainTexture", Vector2.one);
			currentMat.SetFloat("_HitEffectActive", 0f);
			currentMat.SetFloat(mp_outlineWidth, 0f);
			currentMat.SetFloat("_Smoothness", 0f);
			
			currentMat.SetFloat("_AlphaClip", 0.1f);
			localActionBus?.RegisterAction(ActionBus_ActionTypeEnum.L_Damage_RequiredHitColorEffect_要求命中颜色效果,
				_ABC_ReceiveHitColorEffect);
			localActionBus?.RegisterAction(ActionBus_ActionTypeEnum.L_AnimationHelper_OnOutlineTaskRequired_当要求新的描边任务,
				_ABC_ReceiveOutlineTaskInfo);
			localActionBus?.RegisterAction(ActionBus_ActionTypeEnum.L_AnimationHelper_OnRemoveOutlineTask_当要求移除描边任务,
				_ABC_TryRemoveOutlineTaskInfo);

			if (EnableShadowQuad)
			{
				_go_shadowQuad = UnityEngine.Object.Instantiate(_GO_OriginalQuad, transform);
				Vector3 localPosition = _GO_OriginalQuad.transform.localPosition;
				localPosition.y += ShadowQuadYOffset;
				_go_shadowQuad.transform.localPosition = localPosition;
				_go_shadowQuad.transform.localScale = _GO_OriginalQuad.transform.localScale;
				_go_shadowQuad.transform.localRotation = _GO_OriginalQuad.transform.localRotation;
				SelfMeshRendererRef.shadowCastingMode = ShadowCastingMode.Off;
				_mr_shadowQuad = _go_shadowQuad.GetComponent<MeshRenderer>();
				_mr_shadowQuad.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
			}
		}
		private List<Material> ml = new List<Material>();
		public override void UpdateTick(float ct, int cf, float delta)
		{
			_CheckChangeTexture();
			_UpdateTick_OutlineTaskTick(ct, cf, delta);
			if (Application.isPlaying && EnableShadowQuad)
			{
				MaterialPropertyBlock block = new MaterialPropertyBlock();
				SelfMeshRendererRef.GetPropertyBlock(block);
				// var block = SelfMeshRendererRef.GetPropertyBlock( "_Cell");
				var getCell = block.GetFloat("_Cell");
				_mr_shadowQuad.material.SetFloat(_sp__Cell, getCell);
				 var getIndex = block.GetFloat("_SheetIndex");
				 _mr_shadowQuad.material.SetFloat(_sp__SpriteIndex, getIndex);

			}
		}

		public override void FixedUpdateTick(float ct, int cf, float delta)
		{
		}

		protected override void SetAnimationLogicSpeedMul(float oriMul, float newMul)
		{
			float oriSpeed = SelfAnimancerComponent.Playable.Speed / oriMul;
			//用当前的实际速度除掉更换之前的乘数，就是原始的动画的速度
			//那么新的速度就是原始速度再乘 乘数
			float newSpeed = oriSpeed * newMul;
			SelfAnimancerComponent.Playable.Speed = newSpeed;
		}



#region 换图组

		/// <summary>
		/// 检查是否需要更换图组中的图片
		/// </summary>
		private void _CheckChangeTexture()
		{
			if (!Mathf.Approximately(_SheetIndexInCurrentGroup_, __SheetIndexInCurrentGroup__Last))
			{
				if (Application.isPlaying)
				{
					_selfSpriteGroupIndexContent.TargetSpriteIndex = _SheetIndexInCurrentGroup_;
					var ds = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
						.L_AnimationHelper_OnSpriteSheetIndexChanged_当图片表索引改变);
					ds.ObjectArgu1 = _selfSpriteGroupIndexContent;
					_localActionBusRef?.TriggerActionByType(ds);
					_SheetIndexInCurrentGroup_ = _selfSpriteGroupIndexContent.TargetSpriteIndex;
				}
				ChangeTexture();
				__SheetIndexInCurrentGroup__Last = _SheetIndexInCurrentGroup_;
			}
		}

		private void LateUpdate()
		{
			_CheckChangeTexture();
		}




		private void ChangeTexture()
		{
			if (CurrentSpriteGroupRef == null)
			{
				CurrentSpriteGroupRef = GlobalConfigurationAssetHolderHelper.GetGCAHH()
					.Collection_SheetAnimationConfigGroup.GetSheetGroupByIndex(CurrentSpriteGroupIndex);
			}
			if (!SelfMeshRendererRef)
			{
				SelfMeshRendererRef = GetComponentInChildren<MeshRenderer>();
			}
#if UNITY_EDITOR
			if (SelfMeshRendererRef.sharedMaterial == null || !SelfMeshRendererRef.sharedMaterial.shader.name.Contains("Quad"))
			{
				SelfMeshRendererRef.sharedMaterial = _defaultMaterialOfThisHelper;
				SelfMeshRendererRef.sharedMaterial.SetFloat("_Flipbook", 1f);
			}
#endif


			var targetTexture = CurrentSpriteGroupRef.GetTexture2D(_SheetIndexInCurrentGroup_);

			SelfMeshRendererRef.material.SetTexture(_sp_MainTexture, targetTexture);
			if (Application.isPlaying && EnableShadowQuad)
			{ 
				_mr_shadowQuad.material.SetTexture(_sp_MainTexture, targetTexture);
			}

			var normalTexture = CurrentSpriteGroupRef.GetNormalTexture2D(_SheetIndexInCurrentGroup_);
			if (normalTexture != null)
			{
				SelfMeshRendererRef.material.SetTexture(_sp_NormalTexture, normalTexture);
				SelfMeshRendererRef.material.SetFloat("_NormalStrength", 1f);
			}
			else
			{
				SelfMeshRendererRef.material.SetFloat("_NormalStrength", 0f);
				SelfMeshRendererRef.material.SetTexture(_sp_NormalTexture, null);
			}
		}

#endregion


		public void SetOverrideAnimatorController(AnimatorOverrideController aoc)
		{
			SelfAnimatorRef.runtimeAnimatorController = aoc;
		}

		public void SetAlphaOverride(float alpha)
		{
			SelfMeshRendererRef.material.SetFloat("_AlphaOverride", alpha);
		}


		/// <summary>
		/// <para>holdOrRest:0 为相同动画hold，不同动画从头开始播，1为需要reset，2为需要hold(不同动画则维持当前的播放进度，如果是相同动画会无事发生）</para>
		/// </summary>
		public void PlayWithSpeed(ClipTransition clip, float speed = 1f, int holdOrRest = 0)
		{
			if (!gameObject.activeSelf)
			{
				return;
			}
			if (!SelfAnimancerComponent.gameObject.activeInHierarchy)
			{
				return;
			}
			SelfAnimancerComponent.Playable.Speed = speed * AnimationLogicSpeedMul;
			AnimancerState currentClipState = SelfAnimancerComponent.States.Current;
			switch (holdOrRest)
			{
				case 0:
					float n = 0f;
					if (currentClipState != null && currentClipState.Clip.name.Equals(clip.Name))
					{
						return;
					}
					else
					{
						SelfAnimancerComponent.Play(clip);
						SelfAnimancerComponent.States.Current.NormalizedTime = 0f;
					}
					break;
				case 1:
					//需要reset
					SelfAnimancerComponent.Play(clip);
					SelfAnimancerComponent.States.Current.NormalizedTime = 0f;
					break;
				case 2:
					//需要hold
					if (SelfAnimancerComponent.IsPlaying(clip))
					{
						return;
					}
					float nor = 0f;
					if (currentClipState != null)
					{
						nor = currentClipState.NormalizedTime;
						nor %= 1f;
					}
					SelfAnimancerComponent.Play(clip);
					SelfAnimancerComponent.States.Current.NormalizedTime = nor;
					break;
			}	
		}

		protected void _ABC_ReceiveHitColorEffect(DS_ActionBusArguGroup ds)
		{
			var hitColorConfig = ds.ObjectArgu1 as HitColorEffectInfo;
			PerHitColorInfoPair configInfo = ds.ObjectArguStr as PerHitColorInfoPair;
			
			// if (SelfRelatedArtHelperRef && SelfRelatedArtHelperRef.HitColorConfigOverride != null)
			// {
			// 	int findIndex = SelfRelatedArtHelperRef.HitColorConfigOverride.FindIndex((helper =>
			// 		helper.SingleInfoUID.Equals(uid, StringComparison.OrdinalIgnoreCase)));
			// 	if (findIndex != -1)
			// 	{
			// 		hitColorConfig = SelfRelatedArtHelperRef.HitColorConfigOverride[findIndex]
			// 			.HitColorEffectInfoContent;
			// 	}
			// }
			
			if (_tween_HitColorTween != null && !_tween_HitColorTween.IsComplete())
			{
				_tween_HitColorTween.Complete();
			}
			if (_tween_EmissionTween != null && !_tween_EmissionTween.IsComplete())
			{
				_tween_EmissionTween.Complete();
			}
			

			SelfMeshRendererRef.material.SetFloat("_HitEffectActive", 1f);
			_tween_HitColorTween = SelfMeshRendererRef.material
				.DOColor(hitColorConfig.TargetColor, _SP_HitEffectColorTint, hitColorConfig.Duration)
				.SetEase(hitColorConfig.EaseType).SetLoops(2, LoopType.Yoyo)
				.OnComplete(_Callback_DisableShaderOfHitEffect);
			SelfMeshRendererRef.material.SetFloat(_SP__HitEffectOpacity, hitColorConfig.TargetAlpha);
			_tween_EmissionTween = SelfMeshRendererRef.material
				.DOFloat(0f, "_EmissionMultiplier", hitColorConfig.Duration).SetEase(hitColorConfig.EaseType)
				.SetLoops(2, LoopType.Yoyo);
		}

		private void _Callback_DisableShaderOfHitEffect()
		{
			if (SelfMeshRendererRef == null)
			{
				return;
			}
			SelfMeshRendererRef?.material.SetFloat("_HitEffectActive", 0f);
		}
#region 对外设置材质的功能

		/// <summary>
		/// <para>设置主纹理染色</para>
		/// </summary>
		public override void SetMainTint(Color color)
		{
			SelfMeshRendererRef.material.SetColor("_BaseTint", color);
		}
		// public override void SetOutlineColor(Color color)
		// {
		// 	_selfMeshRendererRef.material.SetColor("_OutlineColor", color);
		// }
		// public override void SetOutlineWidth(float width)
		// {
		// 	if (Mathf.Approximately(width, 0f))
		// 	{
		// 		_selfMeshRendererRef.material.SetColor("_OutlineColor", Color.black);
		// 	}
		// 	_selfMeshRendererRef.material.SetFloat("_OutlineWidth", width);
		// }

#endregion
#region 描边！

		private static readonly int mp_outlineColor = Shader.PropertyToID("_OutlineColor");
		private static readonly int mp_outlineWidth = Shader.PropertyToID("_OutlineWidth");
		private static readonly int mp_outlineEnable = Shader.PropertyToID("_EnableOutline");

		private List<SheetOutlineTaskInfo> _outlineRuntimeTaskList = new List<SheetOutlineTaskInfo>();


		private SheetOutlineTaskInfo _currentFirstOutlineTask;

		private Tweener _currentOutlineTaskTween;

		/// <summary>
		/// <para>对描边任务进行处理</para>
		/// </summary>
		/// <param name="ds"></param>
		private void _ABC_ReceiveOutlineTaskInfo(DS_ActionBusArguGroup ds)
		{
			var sheetOutlineInfoTemplate = ds.GetObj1AsT<SheetOutlineTaskInfo>();
			float taskDuration = ds.FloatArgu1.Value;
			var TaskName = ds.ObjectArguStr as string;
			//如果已经存在，则重设时长
			for (int i = _outlineRuntimeTaskList.Count - 1; i >= 0; i--)
			{
				if (string.Equals(TaskName, _outlineRuntimeTaskList[i].TaskName, StringComparison.OrdinalIgnoreCase))
				{
					_outlineRuntimeTaskList[i].RemainingTime = taskDuration;
					return;
				}
			}

			//不存在，那搞个新的
			//进行一个DeepCopyFromPool
			SheetOutlineTaskInfo newInfo = SheetOutlineTaskInfo.DeepCopyFromPool(sheetOutlineInfoTemplate);
			 
			_outlineRuntimeTaskList.Add(newInfo);
			newInfo.TaskName = TaskName;
			newInfo.RemainingTime = taskDuration;
			_OutlineTask_StartFirst();
		}

		private void _ABC_TryRemoveOutlineTaskInfo(DS_ActionBusArguGroup ds)
		{
			var taskName = ds.GetObj1AsT<string>();
			for (int i = _outlineRuntimeTaskList.Count - 1; i >= 0; i--)
			{
				if (string.Equals(taskName, _outlineRuntimeTaskList[i].TaskName, StringComparison.OrdinalIgnoreCase))
				{
					var toRemove = _outlineRuntimeTaskList[i];
					_outlineRuntimeTaskList.RemoveAt(i);
					toRemove.KillTween();
					if (ReferenceEquals(_currentFirstOutlineTask, toRemove))
					{
						_OutlineTask_StartFirst();
					}
					UnityEngine.Pool.GenericPool<SheetOutlineTaskInfo>.Release(toRemove);
					return;
				}
			}
			if (_outlineRuntimeTaskList.Count == 0)
			{
				MeshRenderer selfMeshRendererRef = SelfMeshRendererRef;
				selfMeshRendererRef.material.SetColor(mp_outlineColor, Color.white);
				selfMeshRendererRef.material.SetFloat(mp_outlineWidth, 0f);
				
			}
		}

		private void _OutlineTask_StartFirst()
		{
			MeshRenderer selfMeshRendererRef = SelfMeshRendererRef;
			if (_outlineRuntimeTaskList.Count == 0)
			{
				selfMeshRendererRef.material.SetColor(mp_outlineColor, Color.white);
				selfMeshRendererRef.material.SetFloat(mp_outlineWidth, 0f);
				selfMeshRendererRef.material.SetFloat(mp_outlineEnable, 0f);
			}
			else
			{
				var first = _outlineRuntimeTaskList.OrderByDescending(item => item.Priority).FirstOrDefault();
				if (!ReferenceEquals(first, _currentFirstOutlineTask))
				{
					_currentOutlineTaskTween?.Kill();
				}
				if (first != null)
				{
					_currentFirstOutlineTask = first;
					_currentFirstOutlineTask.StartTween(ref selfMeshRendererRef);
				}
			}
		}

		private void _UpdateTick_OutlineTaskTick(float ct, int cf, float delta)
		{
			for (int i = _outlineRuntimeTaskList.Count - 1; i >= 0; i--)
			{
				var item = _outlineRuntimeTaskList[i];
				item.RemainingTime -= delta;
				if (item.RemainingTime <= 0f)
				{
					item.KillTween();
					_outlineRuntimeTaskList.RemoveAt(i);
					if (ReferenceEquals(item, _currentFirstOutlineTask))
					{
						_OutlineTask_StartFirst();
					}
					UnityEngine.Pool.GenericPool<SheetOutlineTaskInfo>.Release(item);
				}
			}
		}

#endregion


		T GetComponentInParentRecursively<T>(UnityEngine.Component c) where T : MonoBehaviour
		{
			if (c == null)
			{
				return null;
			}
			T comp = c.GetComponent<T>();
			if (comp != null)
			{
				return comp as T;
			}
			return GetComponentInParentRecursively<T>(c.transform.parent);
		}


	}
}