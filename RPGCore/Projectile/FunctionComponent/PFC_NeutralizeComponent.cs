using System;
using System.Collections.Generic;
using Global.ActionBus;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Projectile
{
	/// <summary>
	/// <para>这个Projectile将能够产生抵消效果</para>
	/// </summary>
	[Serializable]
	public class PFC_NeutralizeComponent : ProjectileBaseFunctionComponent
	{

		
		
		[Header("抵消等级")]
		public float NeutralizeLevel = 1;


		[Header("抵消消耗HP比率。每抵消1点其他子弹的HP自己消耗多少HP")]
		public float NeutralizeConsumeRatio;


		public static PFC_NeutralizeComponent GetFromPool(PFC_NeutralizeComponent pfc=  null)
		{
			var tmpNew = GenericPool<PFC_NeutralizeComponent>.Get();
			if (pfc != null)
			{
				tmpNew.NeutralizeLevel = pfc.NeutralizeLevel;
				tmpNew.NeutralizeConsumeRatio = pfc.NeutralizeConsumeRatio;
			}
			
			return tmpNew;
		}
		public override void ResetOnReturn()
		{
			GenericPool<PFC_NeutralizeComponent>.Release(this);
		}
		public override void DeepCopyToRuntimeList(ProjectileBaseFunctionComponent copyFrom, List<ProjectileBaseFunctionComponent> targetList)
		{
			var tmpNew = GetFromPool(copyFrom as PFC_NeutralizeComponent);
			targetList.Add(tmpNew);
		}
		public override void FixedUpdateTick(float currentTime, int currentFrame, float delta)
		{
			
		}
		
	}
}