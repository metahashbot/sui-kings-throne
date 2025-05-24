using System;
using System.Collections.Generic;
using ARPG.Manager;
using DG.Tweening;
using Global.ActionBus;
using RPGCore.Projectile.Layout;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Projectile
{
	[Serializable]
	public class PFC_投射物将于预警后实际生成_ActiveProjectileAfterWarningVFX : ProjectileBaseFunctionComponent
	{
		[SerializeField, LabelText("使用的预警VFX Prefab")]
		public GameObject WarnVFXPrefab;


		[NonSerialized]
		public VFX_ParticleSystemPlayProxy RuntimeVFXInstanceRef;
		
		/// <summary>
		/// 当前正在播放预警特效？
		/// </summary>
		[NonSerialized]
		public bool CurrentPlayingWarnVFX;
		
		
		[SerializeField, LabelText("预警VFX的持续时间")]
		[InfoBox("！需要这个特效的持续时间为1秒！")]
		public float WarnVFXDuration = 1f;

		[NonSerialized]
		public float RemainingWarnVFXTime;
		
		[SerializeField, LabelText("预警VFX的尺寸额外乘数")]
		public float WarnVFXSizeMultiplier = 1f;
		
		[SerializeField, LabelText("预警VFX的高度额外加数")] 
		public float WarnVFXHeightAddition = 0.5f;
		
		[SerializeField,LabelText("预警时是否暂停投射物")]
		public bool PauseProjectileWhenWarn = true;
		
		 [SerializeField,LabelText("预警是否跟随投射物运动")]
		 [HideIf("PauseProjectileWhenWarn")]
		public bool WarnVFXFollowProjectile = false;
		
		
		public static PFC_投射物将于预警后实际生成_ActiveProjectileAfterWarningVFX GetFromPool(PFC_投射物将于预警后实际生成_ActiveProjectileAfterWarningVFX copy = null)
		{
			PFC_投射物将于预警后实际生成_ActiveProjectileAfterWarningVFX tmpNew = GenericPool<PFC_投射物将于预警后实际生成_ActiveProjectileAfterWarningVFX>.Get();
			if (copy != null)
			{
				tmpNew.WarnVFXPrefab = copy.WarnVFXPrefab;
				tmpNew.WarnVFXDuration = copy.WarnVFXDuration;
				tmpNew.WarnVFXSizeMultiplier = copy.WarnVFXSizeMultiplier;
				tmpNew.WarnVFXHeightAddition = copy.WarnVFXHeightAddition;
				tmpNew.PauseProjectileWhenWarn = copy.PauseProjectileWhenWarn;
				tmpNew.WarnVFXFollowProjectile = copy.WarnVFXFollowProjectile;
			}
			return tmpNew;
		}




		public override void OnFunctionStart(float currentTime)
		{
			base.OnFunctionStart(currentTime);

			if (!WarnVFXPrefab)
			{
				throw new ArgumentException($"来自投射物{_selfRelatedProjectileBehaviourRef.RelatedGORef.name}拥有预警后生成的组件，" +
				                            $"（版面:{_selfRelatedProjectileBehaviourRef.SelfLayoutConfigReference.name}）。" +
				                            $"但是并没有 VFX Prefab 被赋值。");
			}
#if UNITY_EDITOR
			var ps = WarnVFXPrefab.GetComponent<ParticleSystem>();
			if (ps.main.stopAction != ParticleSystemStopAction.Disable)
			{
				DBug.LogError($"来自投射物{_selfRelatedProjectileBehaviourRef.RelatedGORef.name}拥有预警后生成的组件，" +
				              $"（版面:{_selfRelatedProjectileBehaviourRef.SelfLayoutConfigReference.name}）。" +
				              $"但是 VFX Prefab 的 Stop Action 不是 Disable。");
			}
#endif
			_selfRelatedProjectileBehaviourRef.StartLifetime += WarnVFXDuration;
			var vfxPool = VFXPoolManager.Instance;
			RuntimeVFXInstanceRef = vfxPool.GetPSPPRuntimeByPrefab(WarnVFXPrefab);
			RuntimeVFXInstanceRef.ExtraDelayDuration = WarnVFXDuration;
			
			 
			
			//ss = 1 , time = 1.5s, ss = 1.5 / 1 = 
			// var simulationSpeed = 1f / WarnVFXDuration;
			//
			// var main = RuntimeVFXInstanceRef.main;
			// main.simulationSpeed = simulationSpeed;
			//
			//
			
			RuntimeVFXInstanceRef.transform.position =
				_selfRelatedProjectileBehaviourRef.RelatedGORef.transform.position + Vector3.up * WarnVFXHeightAddition;
			RuntimeVFXInstanceRef.transform.localScale = _selfRelatedProjectileBehaviourRef.StartLocalSize *
			                                             WarnVFXSizeMultiplier * Vector3.one;

			RuntimeVFXInstanceRef.Play();
			RuntimeVFXInstanceRef.SimulationSpeed = 1f / WarnVFXDuration;

			CurrentPlayingWarnVFX = true;
			RemainingWarnVFXTime = WarnVFXDuration;
			if (PauseProjectileWhenWarn)
			{
				_selfRelatedProjectileBehaviourRef.PauseProjectile();
				_selfRelatedProjectileBehaviourRef.RelatedGORef.SetActive(false);
			}
			else
			{
				_selfRelatedProjectileBehaviourRef.RelatedGORef.SetActive(false);
			}
		}

		

		public override void FixedUpdateTick(float currentTime, int currentFrame, float delta)
		{
			base.FixedUpdateTick(currentTime, currentFrame, delta);
			if (CurrentPlayingWarnVFX)
			{
				RemainingWarnVFXTime -= delta;
				if (RemainingWarnVFXTime <= 0)
				{
					CurrentPlayingWarnVFX = false;
					if (PauseProjectileWhenWarn)
					{
						_selfRelatedProjectileBehaviourRef.UnpauseProjectile();
					}
					
					_selfRelatedProjectileBehaviourRef.RelatedGORef.SetActive(true);
				}
				if (WarnVFXFollowProjectile)
				{
					RuntimeVFXInstanceRef.transform.position =
						_selfRelatedProjectileBehaviourRef.RelatedGORef.transform.position +
						Vector3.up * WarnVFXHeightAddition;
				}
			}
		}

		public override void ResetOnReturn()
		{

			RuntimeVFXInstanceRef = null;
			GenericPool<PFC_投射物将于预警后实际生成_ActiveProjectileAfterWarningVFX>.Release(this);
			 
			
		}
		public override void DeepCopyToRuntimeList(ProjectileBaseFunctionComponent copyFrom, List<ProjectileBaseFunctionComponent> targetList)
		{
			
			targetList.Add(GetFromPool(copyFrom as PFC_投射物将于预警后实际生成_ActiveProjectileAfterWarningVFX));
			 
			
			
			
		}
	}
}