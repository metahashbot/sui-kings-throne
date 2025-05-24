using System;
using System.Collections.Generic;
using Global;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace ARPG.Character.Config
{
	[Serializable]
	[CreateAssetMenu(fileName = "单个动画组配置", menuName = "#SO Assets#/#角色配置#/#动画#/单个动画组配置", order = 22)]
	public class SOConfig_PerSheetAnimationGroup : ScriptableObject
	{

#if UNITY_EDITOR
		[Button("刷新所有的图组SO至库",ButtonSizes.Large,Icon = SdfIconType.Images),GUIColor(1f,1f,0f)]
		private void _Button_RefreshAllSheetGroup()
		{
			UnityEditor.AssetDatabase.Refresh();
			var gcahh_sheetGroupCollection =
				GlobalConfigurationAssetHolderHelper.GetGCAHH().Collection_SheetAnimationConfigGroup;
			gcahh_sheetGroupCollection.SheetGroupCollection.Clear();
			var path = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_PerSheetAnimationGroup");
			foreach (var perGUID in path)
			{
				var perPath = UnityEditor.AssetDatabase.GUIDToAssetPath(perGUID);
				var perSO = UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_PerSheetAnimationGroup>(perPath);
				gcahh_sheetGroupCollection.SheetGroupCollection.Add(perSO);
			}
			UnityEditor.EditorUtility.SetDirty(gcahh_sheetGroupCollection);
			UnityEditor.AssetDatabase.SaveAssets();
		}

		[HorizontalGroup("1")]
		[Button("打开角色资源Excel", ButtonSizes.Medium, Icon = SdfIconType.FileExcelFill)]
		private void _Button_OpenExcel()
		{
			//open Assets/ExcelData/Resource/通用资源表.xlsx
			UnityEditor.AssetDatabase.OpenAsset(
				UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.DefaultAsset>(
					"Assets/ExcelData/RPG/角色资源索引.xlsx"));
		}
		[HorizontalGroup("1")]
		[Button("进行角色资源转表", ButtonSizes.Medium, Icon = SdfIconType.Circle)]
		private void _button_Convert()
		{
			// get Assets/ExcelData/通用资源0_通用Sprite.asset
			var d = UnityEditor.AssetDatabase.LoadAssetAtPath<SO_Conversion>("Assets/ExcelData/角色_1_1_角色资源.asset");
			d.ProcessConvert();
		}

#endif
		
		[SerializeField, LabelText("对应角色枚举索引")]
		public int RelatedCharacterSheetGroupIndex;

		[SerializeField, LabelText("角色词条")]
		public string CharacterTag;
		

		[Serializable]
		public class PerInfoGroup
		{
			[NonSerialized]
			public AsyncOperationHandle<Texture2D> op;
			public AssetReferenceT<Texture2D> TextureAddress;
			[NonSerialized]
			public AsyncOperationHandle<Texture2D> op_normal;
			public AssetReferenceT<Texture2D> NormalTextureAddress;
			
			[SerializeField, LabelText("对应【图集】索引")]
			public int TargetSheetIndex;
			public Texture2D GetTexture2D()
			{
				if (!op.IsValid())
				{
					op = TextureAddress.LoadAssetAsync<Texture2D>();
					op.WaitForCompletion();
				}
				return op.Result;
			}

			public Texture2D GetNormalTexture2D()
			{
				if (NormalTextureAddress == null)
				{ 
					return null;
				}
				
				if (!op_normal.IsValid())
				{
					if (!NormalTextureAddress.IsValid())
					{
						return null;
					}
					op_normal = NormalTextureAddress.LoadAssetAsync<Texture2D>();
					op_normal.WaitForCompletion();
				}
				return op_normal.Result;
			}
		}


		[SerializeField, LabelText("该角色所有的【图集】信息")]
		public List<PerInfoGroup> PerInfoGroups = new List<PerInfoGroup>();

		public Texture2D GetTexture2D(int index)
		{
#if UNITY_EDITOR
			

#endif
			int find =  PerInfoGroups.FindIndex((group => group.TargetSheetIndex == index));
			if (find == -1)
			{
				DBug.LogError($"没有在【图组】{RelatedCharacterSheetGroupIndex}中找到索引为{index}的图片");
				return GlobalConfigurationAssetHolderHelper.GetGCAHH().Collection_SheetAnimationConfigGroup
					.GetSheetGroupByIndex(0).GetTexture2D(0);
			}

			return PerInfoGroups[find].GetTexture2D();
		}

		public Texture2D GetNormalTexture2D( int index)
		{
			int find =  PerInfoGroups.FindIndex((group => group.TargetSheetIndex == index));
			if (find == -1)
			{
				DBug.LogError($"没有在【图组】{RelatedCharacterSheetGroupIndex}中找到索引为{index}的图片");
				return null;
			}

			return PerInfoGroups[find].GetNormalTexture2D();
		}
		public void PreloadAll()
		{
			if (RelatedCharacterSheetGroupIndex == 0)
			{
				return;
			}
			foreach (PerInfoGroup perInfoGroup in PerInfoGroups)
			{
				if (!perInfoGroup.op.IsValid())
				{

#if UNITY_EDITOR
					if (!perInfoGroup.TextureAddress.editorAsset)
					{
						DBug.LogError($" 【图组】{RelatedCharacterSheetGroupIndex}中的【图集】{perInfoGroup.TargetSheetIndex}没有设置图片");
						return;
					}
#endif
					perInfoGroup.op = perInfoGroup.TextureAddress.LoadAssetAsync<Texture2D>();
					perInfoGroup.op.WaitForCompletion();
				}
			}
		}


	}
}