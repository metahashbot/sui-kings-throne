using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.Pool;
namespace RPGCore.Projectile
{
	[Serializable]
	public class PFC_激活时触发事件_SpawnGameplayEventOnActive : ProjectileBaseFunctionComponent
	{


		[LabelText("将要触发的事件UID们"), GUIColor(255f / 255f, 248f / 255f, 10f / 255f),
		 ListDrawerSettings(ShowFoldout = true)]
		public List<string> GameplayEventNameList = new List<string>();


		public override void ResetOnReturn()
		{
			GenericPool<PFC_激活时触发事件_SpawnGameplayEventOnActive>.Release(this);
		}
		public override void DeepCopyToRuntimeList(ProjectileBaseFunctionComponent copyFrom, List<ProjectileBaseFunctionComponent> targetList)
		{
			var tmpNew = GetFromPool(copyFrom as PFC_激活时触发事件_SpawnGameplayEventOnActive);
			targetList.Add(tmpNew);
		}
		
		
		public static PFC_激活时触发事件_SpawnGameplayEventOnActive GetFromPool(PFC_激活时触发事件_SpawnGameplayEventOnActive copyFrom)
		{
			var newInstance = GenericPool<PFC_激活时触发事件_SpawnGameplayEventOnActive>.Get();
			newInstance.GameplayEventNameList = copyFrom.GameplayEventNameList;
			return newInstance; 
			
		}
	}
}