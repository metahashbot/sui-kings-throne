using System;
using Cinemachine;
using GameplayEvent.Handler.PresentationUtility_演出功能;
using Global;
using Sirenix.OdinInspector;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
namespace ARPG.Manager
{
	public abstract class BaseMainCameraBehaviour : MonoBehaviour
	{

		[FoldoutGroup("配置", true)]
		[SerializeField, Required]
		public CinemachineVirtualCamera VirtualCameraRef;

		[FoldoutGroup("配置", true)]
		[SerializeField, Required]
		public UnityEngine.Camera MainCamera;
		[FoldoutGroup("配置", true)]
		[SerializeField, Required]
		public UnityEngine.Camera UICamera;


		[FoldoutGroup("配置", true)]
		[SerializeField, Required]
		public Transform CameraTargetTransform;



		public float CurrentScrollSpeed { get; protected set; } = 1f;

		public bool CurrentAvailableScroll { get; protected set; } = false;
		
		public Vector2 CurrentCameraDistanceLimit { get; protected set; } = new Vector2(11f, 20f);
		
		public bool CurrentContainDepthOfField { get; protected set; } = true;
		 
		public Vector2 CurrentDepthOfFieldRange { get; protected set; } = new Vector2(1.2f, 1.5f);
		
		public bool CurrentContainTrackedOffsetY { get; protected set; } = true;
		
		public Vector2 CurrentTrackedOffsetYRange { get; protected set; } = new Vector2(1f, 0.94f);


		public void ReceiveScrollCameraConfig(GEH_设置视距缩放参数_SetScrollCameraDistanceConfig config)
		{
			CurrentScrollSpeed = config.ScrollSpeed;
			CurrentAvailableScroll = config.EnableScroll;
			CurrentCameraDistanceLimit = config.CameraDistanceLimit;
			CurrentContainDepthOfField = config.ContainDepthOfField;
			CurrentDepthOfFieldRange = config.DepthOfFieldRange;
			CurrentContainTrackedOffsetY = config.ContainTrackedOffsetY;
			CurrentTrackedOffsetYRange = config.TrackedOffsetYRange;
		}
		
		
		
		


		public float CurrentItemPitchAngle { get; protected set; } = 20;
		public float CurrentItemYawAngle { get; protected set; } = 0f;

		public float CurrentCharacterPitchAngle { get; protected set; } = 20;
		public float CurrentCharacterYawAngle { get; protected set; } = 0;

		public void SetCurrentPitchInfo(float itemPitch, float characterPitch)
		{
			
			CurrentItemPitchAngle = itemPitch;
			CurrentCharacterPitchAngle = characterPitch;
			
		}
		
		//set yaw
		public void SetCurrentYawInfo(float itemYaw, float characterYaw)
		{
			
			CurrentItemYawAngle = itemYaw;
			CurrentCharacterYawAngle = characterYaw;
			
		}

		

		[HideInInspector]
		public CinemachineFramingTransposer SelfFramingTransposerRef;

		[HideInInspector]
		public CinemachineCollider SelfCinemachineColliderRef;



		[NonSerialized]
		public DepthOfField _dofRef;
		
		
		public virtual void AwakeInitialize()
		{
			BaseInstance = this;
		}

		public static BaseMainCameraBehaviour BaseInstance;
		public static float CurrentPlayerToCameraDistance
		{
			get { return BaseInstance.SelfFramingTransposerRef.m_CameraDistance; }
		}

		public virtual void StartInitialize()
		{

			foreach (UnityEngine.Camera perCamera in FindObjectsOfType<UnityEngine.Camera>())
			{
				if (perCamera != MainCamera && perCamera != UICamera)
				{
					Destroy(perCamera.gameObject);
				}
			}
			if (BaseEditorProxy_FullProxy.SelfGlobalVolume.profile.TryGet(out DepthOfField dof))
			{
				_dofRef = dof;
			}
			else
			{
				_dofRef = null;
			}

		}

		protected virtual void _IC_ProcessScroll(InputAction.CallbackContext context)
		{
			BaseEditorProxy_FullProxy.SelfGlobalVolume.profile.TryGet(out DepthOfField dof);
			if (dof == null)
			{
				return;
			}
			_dofRef = dof;
			if (!CurrentAvailableScroll)
			{
				return;
			}
			float scrollValue = context.ReadValue<float>();
			float newDistance = SelfFramingTransposerRef.m_CameraDistance - scrollValue * CurrentScrollSpeed;
			newDistance = Mathf.Clamp(newDistance, CurrentCameraDistanceLimit.x, CurrentCameraDistanceLimit.y);
			SelfFramingTransposerRef.m_CameraDistance = newDistance;
			if (CurrentContainDepthOfField)
			{
				float newFocalLength = Mathf.Lerp(CurrentDepthOfFieldRange.x, CurrentDepthOfFieldRange.y,
					(newDistance - CurrentCameraDistanceLimit.x) / (CurrentCameraDistanceLimit.y - CurrentCameraDistanceLimit.x));
				_dofRef.focusDistance.value = newFocalLength;
			}
			if (CurrentContainTrackedOffsetY)
			{
				float newOffsetY = Mathf.Lerp(CurrentTrackedOffsetYRange.x, CurrentTrackedOffsetYRange.y,
					(newDistance - CurrentCameraDistanceLimit.x) / (CurrentCameraDistanceLimit.y - CurrentCameraDistanceLimit.x));
				SelfFramingTransposerRef.m_TrackedObjectOffset.y = newOffsetY;
			}
			
		}
		public virtual void LateLoadInitialize()
		{
			
		}

		public virtual void UpdateTick(float ct, int cf, float delta)
		{

		}

		public virtual void FixedUpdateTick(float ct, int cf, float delta)
		{

		}


		public virtual void LateUpdateTick(float ct, int cf, float delta)
		{

		}
		/// <summary>
		/// <para>俯仰角</para>
		/// <para>EulerX，用来调整面对摄像机的角度的</para>
		/// </summary>
		public virtual void SetRotation_EulerX(float rotX)
		{
			var rotation = VirtualCameraRef.transform.rotation;
			var newRot = Quaternion.Euler(rotX, rotation.eulerAngles.y, rotation.eulerAngles.z);
			VirtualCameraRef.transform.rotation = newRot;
		}



		/// <summary>
		/// <para>偏转角</para>
		/// <para>EulerY，用来调整在场景中的旋转角度</para>
		/// </summary>
		/// <param name="rotY"></param>
		public void SetRotation_EulerY(float rotY)
		{
			var currentRotation = VirtualCameraRef.transform.rotation.eulerAngles;
			var newRot = Quaternion.Euler(currentRotation.x, rotY, currentRotation.z);
			VirtualCameraRef.transform.rotation = newRot;
		}

		public void ModifyDamping(float value)
		{
			SelfFramingTransposerRef.m_XDamping = value;
			SelfFramingTransposerRef.m_YDamping = value;
			SelfFramingTransposerRef.m_ZDamping = value;
		}

		/// <summary>
		/// <para>获取当前摄像机注视到地形的那个位置</para>
		/// </summary>
		/// <returns></returns>
		public abstract Vector3 GetCurrentFocusOnTerrainPosition();

		protected virtual void OnDestroy()
		{
			
		}

	}
}