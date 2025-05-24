using System;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.DropItem
{
	[Serializable]
	public abstract class DropComponent_DropCondition
	{

		public virtual void InitializeOnLoad()
		{
			
		}
		
		
		public virtual void InitializeOnDrop(){}
	}
	[Serializable]
	public class MayDropOnEnemyWithBuffKilled : DropComponent_DropCondition
	{
		[SerializeField, LabelText("需要拥有的所有buff们")]
		public RolePlay_BuffTypeEnum[] NeedHaveAllBuffArray;

		[SerializeField, LabelText("√ 按照概率掉落 |  口  按照计数掉落")]
		public bool DropAsChange = true;

		[LabelText("掉率"), SerializeField, Range(0, 100), SuffixLabel("%")]
		public float DropChance = 25f;

		[LabelText("掉落计数"), SerializeField,HideIf(nameof(DropAsChange))]
		public int DropInterval = 5;
		[HideIf(nameof(DropAsChange)),NonSerialized]
		public int RemainingInterval;

		public override void InitializeOnLoad()
		{
			RemainingInterval = DropInterval;
		}
	}



	/// <summary>
	/// <para>当指定ActionBus上有要求掉落的事件触发时</para>
	/// </summary>
	[Serializable]
	public class DropWithDropRequirementActionBusTriggered : DropComponent_DropCondition
	{
		[LabelText("监听如下事件字符串参数"),SerializeField]
		public string[] ListenEventArgus;
		
		
		


	}
	
	
	
}