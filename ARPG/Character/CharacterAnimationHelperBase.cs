using System;
using ARPG.Character.Base;
using ARPG.Character.Base.CustomSpineData;
using Global.ActionBus;
using RPGCore;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
namespace ARPG.Character
{
	
	[TypeInfoBox("基类。帧动画或Spine版的动画都继承自此")]
	public abstract class CharacterAnimationHelperBase : MonoBehaviour
	{

#region 配置部分


		[SerializeField, LabelText("Helper索引"), TitleGroup("配置")]
		public int SelfHelperIndex;

		[SerializeField, LabelText("初始激活吗"), TitleGroup("配置")]
		public bool InitActive = false;

#endregion


#region 运行时信息

		/// <summary>
		/// <para>当前活跃的动画信息</para>
		/// </summary>
		[ShowInInspector, FoldoutGroup("运行时")]
		[NonSerialized]
		public AnimationInfoBase CurrentActiveAnimationInfo;


		[ShowInInspector, FoldoutGroup("运行时")]
		protected float _animationLogicSpeedMul = 1f;



		[ShowInInspector, FoldoutGroup("运行时")]
		public bool CurrentPlayingOffsetClip { get; protected set; }
		protected LocalActionBus _localActionBusRef;
		protected MeshRenderer _relatedMR;
		public RolePlay_ArtHelperBase SelfRelatedArtHelperRef { get; protected set; }


#endregion

#region 调速相关

		/// <summary>
		/// <para>动画逻辑速度乘数。常用于游戏性上的动画调速，与动画配置上的速度无关，会共同构成速度的乘数</para>
		/// </summary>
		public float AnimationLogicSpeedMul
		{
			get => _animationLogicSpeedMul;
			set
			{
				SetAnimationLogicSpeedMul(_animationLogicSpeedMul, value);
				_animationLogicSpeedMul = value;
			}
		}

		protected abstract void SetAnimationLogicSpeedMul(float oriMul, float newMul);


		

#endregion

		public virtual void InstantiateOnInitialize(LocalActionBus localActionBus, RolePlay_ArtHelperBase artH)
		{
			SelfRelatedArtHelperRef = artH;
			_localActionBusRef = localActionBus;
			gameObject.SetActive(InitActive);
			CurrentActiveAnimationInfo = null;





		}
#region Tick

		public abstract void UpdateTick(float ct, int cf, float delta);
		public virtual void FixedUpdateTick(float ct, int cf, float delta) { }

		

#endregion


#region 对外设置材质的功能

		public abstract void SetMainTint(Color color);


#endregion

	}
}