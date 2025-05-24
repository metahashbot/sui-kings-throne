using System;
using Global.ActionBus;
using RPGCore.Buff.Config;
using Sirenix.OdinInspector;
namespace RPGCore.Buff.BuffComponent
{
	public enum BuffTickIndexTypeEnum
	{
		TickType1_常规时间Tick1 = 1,
		TickType2_常规时间Tick2 = 2,
	}
	
	/// <summary>
	/// <para>RPBuff的一个组件：OverTime组件（HOT/DOT）</para>
	/// </summary>
	[Serializable]
	public class BuffTriggerSource_OvertimeTickComponent  : BaseBuffTriggerSource
	{

		
		[LabelText("Tick时，每多长时间触发一次"),FoldoutGroup("配置",true)]
		public float PerTickInterval = 1f;

		[LabelText("Tick索引，如果有多个Tick来源，则这个作为区分"), FoldoutGroup("配置", true)]
		public BuffTickIndexTypeEnum TickIndex = BuffTickIndexTypeEnum.TickType1_常规时间Tick1;


		[NonSerialized,LabelText("已经Tick多少次了？"),FoldoutGroup("运行时",true)]
		public int AlreadyTickCount = 0;

		[NonSerialized, LabelText("上次Tick的时间"), FoldoutGroup("运行时", true)]
		public float LastTickTime;


		[NonSerialized,LabelText("下一次Tick的时间"), FoldoutGroup("运行时", true)]
		private float _nextTickTime;
		
		
		
		public void FixedUpdateTickByBuffHolder(float currentTime, float delta)
		{
			if (currentTime > _nextTickTime)
			{
				_nextTickTime = currentTime + PerTickInterval; 
				AlreadyTickCount += 1;
				SelfRelatedBuffRef.OnOvertimeTickByTickComponent(this);
			
			}
		
		}
		

	}
}