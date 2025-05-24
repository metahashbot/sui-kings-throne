using DG.Tweening;
using RPGCore.UtilityDataStructure;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.Element
{
	public interface I_BuffContainOnetimeVFXOnInitializeOrRefreshed
	{


		protected string vfxUID { get; set; }


		public void SpawnVFX()
		{
			(this as BaseRPBuff)._VFX_GetAndSetBeforePlay(vfxUID, true)._VFX__10_PlayThis();
		}

	}
}