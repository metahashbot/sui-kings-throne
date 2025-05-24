using System;
using System.Collections.Generic;
using Global.Utility;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Projectile
{
	[TypeInfoBox("按速度诱导")]
	[Serializable]
	public class PFC_诱导_TrackingObject : ProjectileBaseFunctionComponent
	{
		[ShowInInspector, LabelText("正在诱导的对象"),NonSerialized,ReadOnly,FoldoutGroup("运行时",true)]
		public GameObject TrackingTarget;

		[ShowInInspector, LabelText("上一次诱导对象的位置"), NonSerialized, ReadOnly, FoldoutGroup("运行时", true)]
		public Vector3 LastTrackingAvailablePosition;


		[SerializeField, LabelText("诱导丢失时寻找新的诱导对象吗")]
		public bool AutoReTrackingOnLostTarget = false;

		[SerializeField, LabelText("诱导最大偏转角速度(角度/秒)")]
		public float MaxTrackingAngleSpeed = 120f;
		

		public override void InitializeComponent(ProjectileBehaviour_Runtime parentProjectileBehaviour)
		{
			base.InitializeComponent(parentProjectileBehaviour);
			
		}
		
		
		
		public void SetTrackingTarget(GameObject target)
		{
			
			TrackingTarget = target;

			if (target != null)
			{
				LastTrackingAvailablePosition = target.transform.position;
			}
			
		}

		public override void FixedUpdateTick(float currentTime, int currentFrame, float delta)
		{
			Vector3 currentTrackPosition = LastTrackingAvailablePosition;
			bool ifNeedChangeDirection = false;
			if (TrackingTarget != null)
			{
				ifNeedChangeDirection = true;
				currentTrackPosition = TrackingTarget.transform.position;
			}
			else
			{
				ifNeedChangeDirection = false;

				//TODO 如果需要重追踪，在这里进行
			}

			if (ifNeedChangeDirection)
			{

				var projectileTransformRef = _selfRelatedProjectileBehaviourRef.RelatedGORef.transform;

				Vector3 currentLocalDirection = _selfRelatedProjectileBehaviourRef.DesiredMoveDirection;
				var trackPY0 = currentTrackPosition;
				trackPY0.y = 0f;
				var currentPY0 = projectileTransformRef.position;
				currentPY0.y = 0f;
				Vector3 targetLocalDirection = (trackPY0 - currentPY0).normalized;
				float diffAngle = Vector3.SignedAngle(currentLocalDirection, targetLocalDirection, Vector3.up);
				float diffABS = Mathf.Abs(diffAngle);
				float actuallyCanRotate = Mathf.Clamp(diffABS, 0f, MaxTrackingAngleSpeed * delta);
				//SingedAngle的值是左手定则，即如果起始方向顺着左手握住的方向转到目的方向，则是正值，否则是负值。始终在-180~180之间。比如右到前的SA就是-90;
				if (diffAngle < 0f)
				{
					//小于0，则需要逆时针旋转
					currentLocalDirection = MathExtend.Vector3RotateOnXOZ(currentLocalDirection, actuallyCanRotate);
				}
				else
				{
					//大于0 则需要顺时针旋转，转值是负的
					currentLocalDirection = MathExtend.Vector3RotateOnXOZ(currentLocalDirection, -actuallyCanRotate);
				}
				
				_selfRelatedProjectileBehaviourRef.DesiredMoveDirection = currentLocalDirection;
				_selfRelatedProjectileBehaviourRef.RelatedGORef.transform.forward = currentLocalDirection;
			}


		}

		public static PFC_诱导_TrackingObject GetFromPool(PFC_诱导_TrackingObject pfc = null)
		{
			var tmpNew = GenericPool<PFC_诱导_TrackingObject>.Get();
			if (pfc != null)
			{
				tmpNew.MaxTrackingAngleSpeed = pfc.MaxTrackingAngleSpeed;
				tmpNew.AutoReTrackingOnLostTarget = pfc.AutoReTrackingOnLostTarget;
				tmpNew.TrackingTarget = pfc.TrackingTarget;
				tmpNew.LastTrackingAvailablePosition = pfc.LastTrackingAvailablePosition;
			}
			return tmpNew;
		}
		public override void ResetOnReturn()
		{
			GenericPool<PFC_诱导_TrackingObject>.Release(this);
		}
		public override void DeepCopyToRuntimeList(ProjectileBaseFunctionComponent copyFrom, List<ProjectileBaseFunctionComponent> targetList)
		{
			PFC_诱导_TrackingObject tmpNew = GetFromPool(copyFrom as PFC_诱导_TrackingObject);
			targetList.Add(tmpNew); 

		}
	}
}