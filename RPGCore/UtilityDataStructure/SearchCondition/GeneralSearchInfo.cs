using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.UtilityDataStructure
{
	/// <summary>
	/// <para>通用搜索信息</para>
	/// </summary>
	[Serializable]
	public class GeneralSearchInfo
	{

		[LabelText("包含【角色】搜索组件吗")]
		public bool ContainCharacterSearchInfo;
		
		[LabelText("具体的角色搜索条件")]
		public CharacterSearchComponent CharacterSearchComponent;
		
		
		
		
		
        
        
        
	}
}