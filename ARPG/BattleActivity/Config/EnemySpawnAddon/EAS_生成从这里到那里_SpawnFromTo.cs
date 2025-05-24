using System;
using System.Collections.Generic;
using UnityEngine.Pool;
// using UnityEngine.Pool;
namespace ARPG.Manager.Config
{
	[Serializable]
	public class EAS_生成从这里到那里_SpawnFromTo : BaseEnemySpawnAddon
	{
		public override void ResetOnReturn()
		{
			GenericPool<EAS_生成从这里到那里_SpawnFromTo>.Release(this);
			return;
		}
	}
}