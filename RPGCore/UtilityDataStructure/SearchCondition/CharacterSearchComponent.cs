using System.Collections.Generic;
using ARPG.Character.Base;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.UtilityDataStructure
{
	public class CharacterSearchComponent 
	{

		[SerializeReference, LabelText("必须要全都满足的条件")]
		public List<BaseCharacterSearchOption> SearchOptions_AllMatch = new List<BaseCharacterSearchOption>();


		[SerializeReference, LabelText("不能包含的条件")]
		public List<BaseCharacterSearchOption> SearchOptions_NotContain = new List<BaseCharacterSearchOption>();

		[SerializeReference, LabelText("必须要满足一个的条件")]
		public List<BaseCharacterSearchOption> SearchOptions_OneMatch = new List<BaseCharacterSearchOption>();


	}
}