using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.Pool;
namespace RPGCore.Projectile
{
	public class PFC_命中后触发事件_SpawnGameplayEventOnHit : ProjectileBaseFunctionComponent
	{
		[LabelText("将要触发的事件UID们"), GUIColor(255f / 255f, 248f / 255f, 10f / 255f),ListDrawerSettings(ShowFoldout = true)]
		public List<string> GameplayEventNameList = new List<string>();
		


		public static PFC_命中后触发事件_SpawnGameplayEventOnHit GetFromPool(PFC_命中后触发事件_SpawnGameplayEventOnHit copyFrom)
		{
			var newInstance = GenericPool<PFC_命中后触发事件_SpawnGameplayEventOnHit>.Get();
			newInstance.GameplayEventNameList = copyFrom.GameplayEventNameList;
			return newInstance; 
			
		}
		public override void ResetOnReturn()
		{
			GenericPool<PFC_命中后触发事件_SpawnGameplayEventOnHit>.Release(this);
		}
		public override void DeepCopyToRuntimeList(ProjectileBaseFunctionComponent copyFrom, List<ProjectileBaseFunctionComponent> targetList)
		{
			
			var tmpNew = GetFromPool(copyFrom as PFC_命中后触发事件_SpawnGameplayEventOnHit);
			targetList.Add(tmpNew);
		}
	}
}