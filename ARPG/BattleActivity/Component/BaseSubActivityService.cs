using System;
namespace ARPG.Manager.Component
{
	[Serializable]
	public abstract class BaseSubActivityService
	{
		public static ActivityManager_ARPG ActivityManagerRef;

		public static void StaticInitialize(ActivityManager_ARPG manager) { ActivityManagerRef = manager; }
		
		

		public virtual void UpdateTick(float ct, int cf, float delta)
		{
			
		}

		public virtual void FixedUpdateTick(float ct, int cf, float delta)
		{
			
		}
	}
}