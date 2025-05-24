using System;
using System.Collections.Generic;
using ARPG.Character.Perk;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Inheritance
{
	/// <summary>
	/// <para>一个继承源</para>
	/// </summary>
	[Serializable]
	[CreateAssetMenu(fileName = "_库名字_继承Perk信息库", menuName = "#SO Assets#/#角色配置#/继承/继承库", order = 21)]
	public class SOConfig_PerInheritanceSource : SerializedScriptableObject
	{
#if UNITY_EDITOR
		[Button("刷新：检索所有继承配置文件，筛选出指定类型"), PropertyOrder(-100)]
		public void Refresh()
		{
			if (RelatedInfoList == null)
			{
				RelatedInfoList = new List<PerkInternalInfoPair>();
			}
			UnityEditor.AssetDatabase.Refresh();
			RelatedInfoList.Clear();
			var contents = Resources.FindObjectsOfTypeAll<SOConfig_PerInheritanceConfig>();
			foreach (var perSO in contents)
			{
				if (perSO.InheritanceComponentCollection.Exists((component =>
					component.GetType() == InheritanceSourceType)))
				{
					var findIndex = RelatedInfoList.FindIndex((pair => pair.RelatedType == perSO.PerkType));
					if (findIndex == -1)
					{
						RelatedInfoList.Add(new PerkInternalInfoPair
						{
							RelatedType = perSO.PerkType,
							RelatedComponents = new List<BaseInheritanceComponent>()
						});
						findIndex = RelatedInfoList.Count - 1;
					}
					var targetInfo = RelatedInfoList[findIndex];
					BaseInheritanceComponent targetComponentRef = perSO.InheritanceComponentCollection.Find((component =>
						component.GetType() == InheritanceSourceType));
					targetInfo.RelatedComponents.Add(targetComponentRef);

				}
			}
		}
		
#endif
		
		[LabelText("需要检查的继承源类型")]
		[SerializeField]
		public Type InheritanceSourceType;



		[InfoBox("改这里同样也会改本来的那个配置")]
		[LabelText("所有拥有这个继承源类型组件的继承项目配置。")]
		[Searchable]
		[SerializeField]
		
		public List<PerkInternalInfoPair> RelatedInfoList;




		public class PerkInternalInfoPair
		{
			public RP_PerkTypeEnum_CharacterConfig RelatedType;
			public List<BaseInheritanceComponent> RelatedComponents;

		}


	}
}