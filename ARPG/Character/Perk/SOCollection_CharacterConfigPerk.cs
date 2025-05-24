using System;
using System.Collections.Generic;
using Global;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Perk
{
	[Serializable]
	[CreateAssetMenu(fileName = "(S)Perk_角色配置_SO库", menuName = "#SO Assets#/#角色配置#/(S)Perk_角色配置_SO库", order = 21)]
	public class SOCollection_CharacterConfigPerk : SOCollectionBase
	{

#if UNITY_EDITOR
		[Button("刷新：刷新项目中的相关配置到这里来"), PropertyOrder(2)]
		public override void Refresh()
		{
			UnityEditor.AssetDatabase.Refresh();
			Collection.Clear();
			var path = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_CharacterConfigPerk");
			foreach (var perGUID in path)
			{
				var perPath = UnityEditor.AssetDatabase.GUIDToAssetPath(perGUID);
				var perSO = UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_CharacterConfigPerk>(perPath);
				Collection.Add(perSO);
			}
			UnityEditor.EditorUtility.SetDirty(this);
			UnityEditor.AssetDatabase.SaveAssets();
		}

		[Header(("==============================="))]
#endif
		public List<SOConfig_CharacterConfigPerk> Collection;
		public SOConfig_CharacterConfigPerk GetRelatedPerkConfigByType(RP_PerkTypeEnum_CharacterConfig type)
		{
			return Collection.Find((perk => perk.Type == type));
		}
		
	}
}