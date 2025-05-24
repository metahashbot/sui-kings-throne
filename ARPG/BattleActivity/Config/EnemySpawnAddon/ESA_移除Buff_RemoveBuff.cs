using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.Pool;
// using UnityEngine.Pool;
namespace ARPG.Manager.Config
{
	[Serializable]
	public class ESA_移除Buff_RemoveBuff : BaseEnemySpawnAddon
	{

		[Serializable]
		public class PerRemoveInfo
		{
			public RolePlay_BuffTypeEnum BuffID;
		}

		[LabelText("需要移除的Buff信息们")]
		public List<PerRemoveInfo> AddBuffInfoList = new List<PerRemoveInfo>();

		public override void ResetOnReturn()
		{
			GenericPool<ESA_移除Buff_RemoveBuff>.Release(this);
			return;
		}
	}
}