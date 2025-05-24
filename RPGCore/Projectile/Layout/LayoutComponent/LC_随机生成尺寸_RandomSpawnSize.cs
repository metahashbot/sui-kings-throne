using System;
using Global.ActionBus;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Projectile.Layout.LayoutComponent
{
	[Serializable]
	public class LC_随机生成尺寸_RandomSpawnSize : BaseProjectileLayoutComponent
	{
		 
		[LabelText("√:随机结果作为【乘数】 | 口:随机结果作为【覆盖】")]
		public bool UseAsMultiplier = true;
		
		[LabelText("    尺寸范围")]
		public Vector2 SizeRange = new Vector2(0.5f, 1.5f);


		public override void InitializeBeforeStart(SOConfig_ProjectileLayout config, float currentTime, int currentFrame)
		{
			base.InitializeBeforeStart(config, currentTime, currentFrame);
			config.LayoutHandlerFunction.SelfActionBusReference.RegisterAction(
				ActionBus_ActionTypeEnum.L_PLC_Spawn_OneProjectileSetScale_一个投射物生成设置了尺寸,
				_ABC_ProcessSpawnSize_OnOneSpawnOperationSetSize);
		}




		private void _ABC_ProcessSpawnSize_OnOneSpawnOperationSetSize(DS_ActionBusArguGroup ds)
		{
			if (!CheckIfSameLayoutConfigParent(ds.ObjectArgu1 as BaseProjectileLayoutComponent))
			{
				return;
			}
			LayoutSpawnInfo_OneSpawn oneSpawnInfo = ds.ObjectArgu2 as LayoutSpawnInfo_OneSpawn;
			oneSpawnInfo.SpawnSize = UseAsMultiplier
				? oneSpawnInfo.SpawnSize * UnityEngine.Random.Range(SizeRange.x, SizeRange.y)
				: UnityEngine.Random.Range(SizeRange.x, SizeRange.y);
		}


		public override void ClearAndUnload(SOConfig_ProjectileLayout parentLayoutRef)
		{
			base.ClearAndUnload(parentLayoutRef);
			
		}

	}
}