using System;
using System.Collections.Generic;
using ARPG.Manager.Config;
using Global;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Pool;
namespace ARPG.BattleActivity.Config.EnemySpawnAddon
{
	[Serializable]
	public class ESA_调整HUD内容_ModifySelfHUD : BaseEnemySpawnAddon
	{

#if UNITY_EDITOR
		[Button("打开本地化表——角色名字", ButtonSizes.Large, Icon = SdfIconType.Translate), PropertyOrder(-100),]
		public void OpenEditor()
		{
			var table = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Localization.LocalizationTableCollection>(
				"Assets/Localization/StringTables/CharacterName_角色名字.asset");
			UnityEditor.Localization.UI.LocalizationTablesWindow.ShowWindow(table);
		}
		[Header("====================")]
#endif

		
		
		[SerializeField, LabelText("√将文本调整为开")]
		public bool _EnableText = true;

		[SerializeField,LabelText("√:不使用角色枚举名字 | 口:使用默认角色枚举名"),ShowIf("@this._EnableText")]
		public bool _UseDefaultName = true;
		[SerializeField, LabelText("文本Key_对应角色名字表"), ShowIf("@(this._UseDefaultName && this._EnableText)")]
		public string _TextKey;

		[SerializeField, LabelText("文本尺寸"), ShowIf(nameof(_EnableText))]
		public float _TextSize = 285;

		[SerializeField,LabelText("文本颜色") ,ShowIf(nameof(_EnableText))]
		public Color _TextColor = new Color(1, 1, 1, 1);
		 
		
		[SerializeField, LabelText("开启名字浮动动画效果？")]
		public bool _EnableNameFloatAnimation = true;
		


		public override void ResetOnReturn()
		{
			GenericPool<ESA_调整HUD内容_ModifySelfHUD>.Release(this);
		}
	}
}