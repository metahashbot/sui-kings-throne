using System;
using Global.ActionBus;
using UnityEngine;
namespace RPGCore.Buff.BuffComponent
{

	[Serializable]
	public abstract class BaseBuffFunctionInfoComponent
	{
		
	}
	public enum BuffTriggerFromTypeEnum
	{
		None =0,
		FromActionBusCallback = 1,
		FromOvertimeTick = 2,
	}


	public struct BuffTriggerArgument
	{
		public BuffTriggerFromTypeEnum FromType;
		public DS_ActionBusArguGroup DSActionBusArguGroup;
		public float TickTime;
		public int TickIndex;
	}
}