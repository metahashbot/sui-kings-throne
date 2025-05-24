using System;
using Global;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

namespace RPGCore.Buff.Config
{
	[Serializable]
	[CreateAssetMenu(fileName = "_待命名_一个buff配置", menuName = "#SO Assets#/#RPG Core#/Buff Config", order = 141)]
	public class SOConfig_RPBuff : ScriptableObject
	{

#if UNITY_EDITOR
		[Button("把SO文件的名字改成Type的名字"), GUIColor(1f, 1f, 0f)]
		private void Rename()
		{
			//rename so file name to RolePlay_BuffTypeEnum
			var soPath = UnityEditor.AssetDatabase.GetAssetPath(this);
			if (ConcreteBuffFunction == null)
			{
				DBug.LogError( $"扫描buff配置时，{name}还没有自己的Handler。要么把这个配置文件删了，要么给它配一个具体的BuffHandler");
				return;
			}
			var soName = ConcreteBuffFunction.SelfBuffType.ToString();
			UnityEditor.AssetDatabase.RenameAsset(soPath, soName);
		}

		[Button("刷新一下Buff的SO库"), GUIColor(1f, 1f, 0f)]
		private void RefreshCollection()
		{
			//call SOCollection_RPBuff Refresh
			var gcahh = Addressables.LoadAssetAsync<GameObject>("GCAHH").WaitForCompletion()
				.GetComponent<GlobalConfigurationAssetHolderHelper>();
			gcahh.Collection_RPBuff.Refresh();
		}

		[Button("打开以编辑↓这个script↓"), PropertyOrder(20), GUIColor(1f, 1f, 0f)]
		private void _BUTTON_OpenScript()
		{
			if (ConcreteBuffFunction == null)
			{
				return;
			}

			//find concrete class name
			var concreteClassName = ConcreteBuffFunction.GetType().Name;
			//find all .cs file by assetdatabse
			var allScriptFiles = UnityEditor.AssetDatabase.FindAssets("t:Script");
			foreach (var scriptFile in allScriptFiles)
			{
				var scriptFilePath = UnityEditor.AssetDatabase.GUIDToAssetPath(scriptFile);
				if (scriptFilePath.Contains(concreteClassName))
				{
					UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(scriptFilePath, 0);
					break;
				}
			}
		}
#endif


		[SerializeReference, LabelText("具体的Buff功能"), PropertyOrder(30)]
		public BaseRPBuff ConcreteBuffFunction = new Buff_通用功能Buff_GeneralFunctionBuff();
		
		[FoldoutGroup("运行时", false, VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()")]
		[NonSerialized,ShowInInspector,LabelText("原始Buff配置模板"),PropertyOrder(40)]
		public SOConfig_RPBuff OriginalBuffConfigTemplate;
	}
}