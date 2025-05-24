using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine.Pool;
namespace RPGCore.Buff.ConcreteBuff.Utility
{
	/// <summary>
	/// <para>用于施加霸体时，标记来源的数据结构</para>
	/// <para>因为会出现某些施加源施加时并不是按照时间施加的，它们不知道具体要持续多长时间，所以施加进来的时候相当于按标记施加，标记移除就不算时间了</para>
	/// </summary>
	public class RPDS_StoicApplyInfo
	{
		[ShowInInspector]
		public I_RPLogicCanApplyStoic Applier;
		[ShowInInspector]
		public float RemainingDuration;

	}
	


	public class BLP_霸体施加信息_StoicApplyInfoBLP : BaseBuffLogicPassingComponent
	{

		public I_RPLogicCanApplyStoic Applier;
		public float RemainingDuration;
		public override void ReleaseOnReturnToPool()
		{
			GenericPool<BLP_霸体施加信息_StoicApplyInfoBLP>.Release(this);
		}
	}


	public class BLP_霸体移除信息_StoicRemoveInfoBLP : BaseBuffLogicPassingComponent
	{

		public I_RPLogicCanApplyStoic Applier;
		public override void ReleaseOnReturnToPool()
		{ 
			GenericPool<BLP_霸体移除信息_StoicRemoveInfoBLP>.Release(this);
		}
	}
	
}