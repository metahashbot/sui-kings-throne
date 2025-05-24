using System;
using System.Collections.Generic;
using Global;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RPGCore.Buff.Config
{
	[Serializable]
	[CreateAssetMenu(fileName = "RPBuff Collection", menuName = "#SO Assets#/#RPG Core#/[集合]SO的Buff配置", order = -190)]
	public class SOCollection_RPBuff : SOCollectionBase
	{
#if UNITY_EDITOR
		[Button("刷新：将库内容刷新成与文件夹内容一致的样子"), PropertyOrder(2)]
		public override void Refresh()
		{
			UnityEditor.AssetDatabase.Refresh();
			BuffCollection.Clear();
			var path = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_RPBuff");
			foreach (var perGUID in path)
			{
				var perPath = UnityEditor.AssetDatabase.GUIDToAssetPath(perGUID);
				SOConfig_RPBuff perSO = UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_RPBuff>(perPath);
				// perSO.InitWhenCreate();

				if (perSO.ConcreteBuffFunction != null)
				{
					BuffCollection.Add(perSO);

				}
			}
			UnityEditor.EditorUtility.SetDirty(this);
			UnityEditor.AssetDatabase.SaveAssets();
		}

		[Header(("==============================="))]
#endif


		[Searchable]
		public List<SOConfig_RPBuff> BuffCollection;



		/// <summary>
		/// <para>如果等级是-1，则会返回查找到的第一个。否则只会返回目标等级的那个</para>
		/// </summary>
		/// <returns></returns>
		public SOConfig_RPBuff GetRPBuffByTypeAndLevel(RolePlay_BuffTypeEnum buffEnum)
		{
			for (int i = 0; i < BuffCollection.Count; i++)
			{
#if UNITY_EDITOR
				if (BuffCollection[i].ConcreteBuffFunction == null)
				{
					DBug.LogError($" {BuffCollection[i].name} 的ConcreteBuffFunction为空，配置有些问题，把它重新配或者直接删掉");
				}
#endif
				if (BuffCollection[i].ConcreteBuffFunction.SelfBuffType == buffEnum)
				{
					return BuffCollection[i];
				}
			}

#if UNITY_EDITOR
			Debug.LogError("没有在运行时RPBuffCollection中查找到" + buffEnum + "的配置文件。检查SOCollection是否刷新，或者可能并不需要指定等级");
#endif

			return null;
		}
	}
}