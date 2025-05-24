using System;
using System.Collections.Generic;
using UnityEngine;
namespace RPGCore.UtilityDataStructure.BuffLogicPassingComponent
{

	[Serializable]
	public class GeneralBuffModifyInfo
	{
		public RolePlay_BuffTypeEnum BuffType;
		
		[SerializeReference]
		public List<BaseBuffLogicPassingComponent> BuffLogicPassingComponents;

	}
}