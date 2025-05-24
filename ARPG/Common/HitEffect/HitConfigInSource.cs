using System;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Common.HitEffect
{
	[TypeInfoBox("用于要求 通用受击特效管理 中根据伤害结果来播放相关特效的 数据结构")]
	[Serializable]
	public class HitConfigInSource
	{
		[SerializeField,LabelText("使用默认的？")]
		public bool AllDefault = true;

		[SerializeField,LabelText("将要额外使用的受击特效配置名")]
		public string[] RelatedConfigName;
		
		
		



	}
}