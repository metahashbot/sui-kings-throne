using System;
using System.Collections.Generic;
using Global.ActionBus;
using RPGCore.Buff.ConcreteBuff.Utility;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Buff.ConcreteBuff.Common
{
	[Serializable]
	[TypeInfoBox("由演出控制的无敌，行为和常规无敌类似。常规的行为在移除的时候不会移除演出的无敌，除非特别指定这个\n" +
	             "它有和强弱霸体类似的机制，本身持续时间没有意义，总是接受各种源的设置，源空了就移除了")]
	public class Buff_InvincibleAllByDirector : Buff_InvincibleAll
	{
		
		protected List<RPDS_InvincibleApplyInfo> _invincibleApplyInfoList = new List<RPDS_InvincibleApplyInfo>();




		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_机制无敌施加信息_DirectorInvincibleApplyInfoBLP blpApply:
					var fi = _invincibleApplyInfoList.FindIndex((info =>
						ReferenceEquals(info.Applier, blpApply.Applier)));
					if (fi == -1)
					{
						RPDS_InvincibleApplyInfo stoicApplyInfo = GenericPool<RPDS_InvincibleApplyInfo>.Get();
						stoicApplyInfo.Applier = blpApply.Applier;
						stoicApplyInfo.RemainingDuration = blpApply.RemainingDuration;
						_invincibleApplyInfoList.Add(stoicApplyInfo);
					}
					else
					{
						var oldInfo = _invincibleApplyInfoList[fi];
						oldInfo.RemainingDuration = blpApply.RemainingDuration;
					}


					break;
				case BLP_机制无敌移除信息_DirectorInvincibleRemoveInfoBLP blpRemove:
					var fi2 = _invincibleApplyInfoList.FindIndex((info =>
						ReferenceEquals(info.Applier, blpRemove.Applier)));
					if (fi2 != -1)
					{
						GenericPool<RPDS_InvincibleApplyInfo>.Release(_invincibleApplyInfoList[fi2]);
						_invincibleApplyInfoList.RemoveAt(fi2);
					}
					else
					{
						// DBug.LogWarning($"在移除霸体时，找不到对应的施加信息，这是不应该的");
					}
					UpdateAvailableTimeByInfoList();

					break;
			}
		}


		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			for (int i = _invincibleApplyInfoList.Count - 1; i >= 0; i--)
			{
				  GenericPool<RPDS_InvincibleApplyInfo>.Release(_invincibleApplyInfoList[i]);
				  _invincibleApplyInfoList.RemoveAt(i);
			}
			_invincibleApplyInfoList.Clear();
			return base.OnBuffPreRemove();
		}


		private void UpdateAvailableTimeByInfoList()
		{
			float maxDuration = -0.01f;

			for (int i = _invincibleApplyInfoList.Count - 1; i >= 0; i--)
			{
				if (Mathf.Approximately(_invincibleApplyInfoList[i].RemainingDuration, -1f))
				{
					maxDuration = -1f;
					ResetDurationAndAvailableTimeAs(-1f, -1f, false);
					return;
				}
				else
				{
					if (_invincibleApplyInfoList[i].RemainingDuration > maxDuration)
					{
						maxDuration = _invincibleApplyInfoList[i].RemainingDuration;
					}
				}
			}

			ResetDurationAndAvailableTimeAs(maxDuration, maxDuration, false);

		}



		public class BLP_机制无敌施加信息_DirectorInvincibleApplyInfoBLP : BaseBuffLogicPassingComponent
		{

			public I_RPLogicCanApplyInvincible Applier;
			public float RemainingDuration = -1f;
			public override void ReleaseOnReturnToPool()
			{
				GenericPool<BLP_机制无敌施加信息_DirectorInvincibleApplyInfoBLP>.Release(this);
			}
		}


		public class BLP_机制无敌移除信息_DirectorInvincibleRemoveInfoBLP : BaseBuffLogicPassingComponent
		{

			public I_RPLogicCanApplyInvincible Applier;
			public override void ReleaseOnReturnToPool()
			{
				GenericPool<BLP_机制无敌移除信息_DirectorInvincibleRemoveInfoBLP>.Release(this);
			}
		}

		
		
	}
	
	
}