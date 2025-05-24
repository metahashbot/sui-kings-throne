using ARPG.Manager;
using RPGCore.UtilityDataStructure;
using UnityEngine;
using UnityEngine.VFX;
namespace RPGCore.Buff.ConcreteBuff.Element
{
	public interface I_BuffContainLoopVFX
	{



		protected string vfxUID { get; set; }
		protected PerVFXInfo relatedVFXInfo { get; set; }
		
		protected bool stopImmediate { get; set; }


		
		
		
		public void SpawnVFX()
		{
			if (relatedVFXInfo != null)
			{
				relatedVFXInfo.VFX_StopThis(stopImmediate);
			}
			relatedVFXInfo = (this as BaseRPBuff)._VFX_GetAndSetBeforePlay(vfxUID, true)?._VFX__10_PlayThis();
		}

		public void StopVFX()
		{
			relatedVFXInfo?.VFX_StopThis(stopImmediate);

		}


		
		
	}
}