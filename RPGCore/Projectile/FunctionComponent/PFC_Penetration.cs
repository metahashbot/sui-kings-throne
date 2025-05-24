using System;
using System.Collections.Generic;
using Global.ActionBus;
using RPGCore.Interface;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Projectile
{
	public class PFC_Penetration : ProjectileBaseFunctionComponent
	{

		[ShowInInspector, LabelText("当前剩余穿透量小于0则耗尽，将要销毁"), NonSerialized, ReadOnly, FoldoutGroup("运行时")]
		public float CurrentRemainingPenetrationAmount = 100;

		[SerializeField, LabelText("初始穿透剩余量"), ShowInInspector]
		public float InitPenetration = 100f;

		[SerializeField, LabelText("已穿透的会被反复命中吗")]
		public bool WillHitAlreadyPenetrate = false;

		[SerializeField, LabelText("    重复命中:多少帧后可以重复命中")]
		[ShowIf(nameof(WillHitAlreadyPenetrate))]
		public int PenetrationRepeatFrameInterval = 10;

		[SerializeField, LabelText("每次穿透的衰减量"), ShowInInspector]
		public float PerPenetrationReduce = 0;


		/// <summary>
		/// <para>tuple2的int是发生命中的逻辑帧数，tuple3是关联投射物的时间戳</para>
		/// </summary>
		[NonSerialized, LabelText("已穿透列表"), ReadOnly, FoldoutGroup("运行时")]
		private List<(I_RP_Projectile_ObjectCanReceiveProjectileCollision, int,int)> PenetratedList = new List<(
			I_RP_Projectile_ObjectCanReceiveProjectileCollision, int,int)>();
		
		public void AddPenetrateInfo( I_RP_Projectile_ObjectCanReceiveProjectileCollision penetratedObject, int frame, int id)
		{
			PenetratedList.Add((penetratedObject, frame,id));
		}

		public bool CheckExistIgnore(I_RP_Projectile_ObjectCanReceiveProjectileCollision source)
		{
			foreach (var perTuple in PenetratedList)
			{
				if (perTuple.Item1 == source)
				{
					return true;
				}
			}
			return false;
			
		}

		public static PFC_Penetration GetFromPool(PFC_Penetration copy = null)
		{
			var tmpNew = GenericPool<PFC_Penetration>.Get();
			if (copy != null)
			{
				tmpNew.PerPenetrationReduce = copy.PerPenetrationReduce;
				tmpNew.InitPenetration = copy.InitPenetration;
				tmpNew.WillHitAlreadyPenetrate = copy.WillHitAlreadyPenetrate;
				tmpNew.PenetrationRepeatFrameInterval = copy.PenetrationRepeatFrameInterval;
				tmpNew.PenetratedList =
					CollectionPool<List<(I_RP_Projectile_ObjectCanReceiveProjectileCollision, int,int)>, (
						I_RP_Projectile_ObjectCanReceiveProjectileCollision, int ,int)>.Get();
				tmpNew.PenetratedList.Clear();
			}
			return tmpNew;
		}


		public override void FixedUpdateTick(float currentTime, int currentFrame, float delta)
		{
			base.FixedUpdateTick(currentTime, currentFrame, delta);
			if (!WillHitAlreadyPenetrate)
			{
				return;
			}

			//可以重复穿透的，则超出帧数的就移除掉，不会再存在于已穿透列表了。
			for (int i = _selfRelatedProjectileBehaviourRef.SelfPenetrationComponent.PenetratedList.Count - 1;
				i >= 0;
				i--)
			{
				var perTuple = _selfRelatedProjectileBehaviourRef.SelfPenetrationComponent.PenetratedList[i];
				if (currentFrame > (perTuple.Item2 + PenetrationRepeatFrameInterval))
				{
					(I_RP_Projectile_ObjectCanReceiveProjectileCollision, int, int) toRemove = _selfRelatedProjectileBehaviourRef.SelfPenetrationComponent.PenetratedList[i];
					toRemove.Item1.RemoveProjectileStamp(toRemove.Item3);
					_selfRelatedProjectileBehaviourRef.SelfPenetrationComponent.PenetratedList.RemoveAt(i);
				}
			}
		}

		public override void ResetOnReturn()
		{
			PenetratedList.Clear();
			CollectionPool<List<(I_RP_Projectile_ObjectCanReceiveProjectileCollision, int,int)>, (I_RP_Projectile_ObjectCanReceiveProjectileCollision
				, int,int)>.Release(PenetratedList);
			GenericPool<PFC_Penetration>.Release(this);
		}
		public override void DeepCopyToRuntimeList(
			ProjectileBaseFunctionComponent copyFrom,
			List<ProjectileBaseFunctionComponent> targetList)
		{
			targetList.Add(GetFromPool(copyFrom as PFC_Penetration));
		}
		public override void OnFunctionStart(float currentTime)
		{
			CurrentRemainingPenetrationAmount = InitPenetration;
		}



	}
}