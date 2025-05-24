using System;
namespace ARPG.Manager.Config
{
	[Serializable]
	public class SECT_简单生成_SimpleSpawnEnemyConfigType : BaseSpawnEnemyConfigTypeHandler
	{


		public override void FixedUpdateTick(float ct, int cf, float delta)
		{
			CheckIfSpawnHandlerFinished();
			base.FixedUpdateTick(ct, cf, delta);
		}
		protected override void CheckIfSpawnHandlerFinished()
		{
			if (_relatedSingleSpawnInfoListRef.Count == 0)
			{
				RelatedSpawnConfigRuntimeRef.ConfigFinished = true;
			}
		}
	}
}