using System;
using System.Collections.Generic;
using Global.ActionBus;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
namespace RPGCore.Projectile
{
	/// <summary>
	/// <para>Projectile基本功能组件。作为[SR]的组件存在于各个ProjectileBehaviour内部。按需组合</para>
	/// </summary>
	[Serializable]
	public abstract class ProjectileBaseFunctionComponent 
	{
		[ShowInInspector,LabelText("关联Projectile"),FoldoutGroup("运行时")]
		protected ProjectileBehaviour_Runtime _selfRelatedProjectileBehaviourRef;

		[ShowInInspector, LabelText("当前地形高度"), NonSerialized, FoldoutGroup("运行时", true), ReadOnly]
		protected static float CurrentTerrainHeight;

		public static void SetCurrentTerrainHeight(float height)
		{
			CurrentTerrainHeight = height;
		}
		public abstract void ResetOnReturn();
		
		
		public virtual void InitializeComponent(ProjectileBehaviour_Runtime parentProjectileBehaviour)
		{
			_selfRelatedProjectileBehaviourRef = parentProjectileBehaviour;
		}

		public abstract void DeepCopyToRuntimeList(ProjectileBaseFunctionComponent copyFrom ,List<ProjectileBaseFunctionComponent> targetList);
		

		
		
		public virtual void OnFunctionStart(float currentTime)
		{
			
		}
		
		

		public virtual void FixedUpdateTick(float currentTime, int currentFrame, float delta){}



		public virtual void OnProjectileLifetimeEnd()
		{

			
			
			
		}




	}
}