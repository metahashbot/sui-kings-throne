using System;
using System.Collections.Generic;
using ARPG.Config.BattleLevelConfig;
using GameplayEvent.Handler;
using GameplayEvent.SO;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
namespace ARPG.Manager.Config
{
	[Serializable]
	[CreateAssetMenu(fileName = "敌人生成配置", menuName = "#SO Assets#/#战斗关卡配置#/敌人生成配置", order = 56)]
	public class SOConfig_SpawnEnemyConfig : ScriptableObject
	{
		[NonSerialized, ReadOnly, LabelText("原始模板")]
		public SOConfig_SpawnEnemyConfig OriginalTemplateRef;

		[SerializeField, LabelText("这个配置的ID")]
		public string ConfigID;



		/// <summary>
		/// <para>对于简单生成，将会在队列被清空时置true</para>
		/// <para>对于计时生成，将会在当前时间大于设置的结束事件后置true</para>
		/// <para>对于计数补充，将会在点数完成 并且 没有等待生成 时置True</para>
		/// <para>当触发匹配的GEH_停止生成时，无条件置true</para>
		/// </summary>
		[NonSerialized, ReadOnly, LabelText("配置已完成？"), ShowInInspector]
		public bool ConfigFinished = false;

		[NonSerialized, ReadOnly, LabelText("该配置已经广播过清除？"), ShowInInspector]
		public bool ClearBroadcast = false;


		[Serializable]
		public sealed class PerEnemyTypeInfo
		{
			[LabelText("敌人角色类型"), Searchable]
			public CharacterNamedTypeEnum Type;
			[LabelText("生成数量"), SuffixLabel("个"), HorizontalGroup("A"), GUIColor(128f / 255f, 168f / 255f, 255f / 255f)]
			public int SpawnCount = 1;
			
			[LabelText("角色个体尺寸乘数")]
			public float CharacterSelfVariantScaleMultiplier = 1f;
			
			[LabelText("是否覆写生成等级"), HorizontalGroup("B")]
			public bool IsOverridingLevel = false;
			[ShowIf(nameof(IsOverridingLevel))]
			[LabelText("敌人等级，会自动退化查找"), SuffixLabel("级"), HorizontalGroup("B")]
			public int OverrideLevelAt = 1;
			[LabelText("延迟时间"), SuffixLabel("秒")]
			public float DelayTime;

			public EnemySpawnPoint_PresetTypeEnum SpawnPresetType = EnemySpawnPoint_PresetTypeEnum.Default_默认通用;
			[InfoBox("如果不是[默认通用]但又没填，则也相当于在这个类型的生成点上随机选")]
			[LabelText("特定生成点ID"),
			 ShowIf(
				 ("@(this.SpawnPresetType != EnemySpawnPoint_PresetTypeEnum.Default_默认通用 &&this.SpawnPresetType != EnemySpawnPoint_PresetTypeEnum.NonRepeatCollection_不重复的集合)"))]
			public string SpecificPointID;


			[LabelText("生成点集合"),
			 ShowIf("@this.SpawnPresetType == EnemySpawnPoint_PresetTypeEnum.NonRepeatCollection_不重复的集合")]
			public List<string> SpecificPointIDList;
			[LabelText("    随机的还是按顺序的？"),
			 ShowIf("@this.SpawnPresetType == EnemySpawnPoint_PresetTypeEnum.NonRepeatCollection_不重复的集合")]
			public bool RandomOrOrder = false;
			[LabelText("    √:已满时跳过 | 口:已满时随机")]
			[ShowIf("@this.SpawnPresetType == EnemySpawnPoint_PresetTypeEnum.NonRepeatCollection_不重复的集合")]
			public bool SkipWhenFull = false;

			[NonSerialized] public int CollectionIndex = 0;
			
			

			[LabelText("包含额外附加项")]
			public bool ContainAddons;
			[SerializeReference, LabelText("直属配置==额外附加项内容们"), ShowIf(nameof(ContainAddons))]
			[FormerlySerializedAs("AddonList")]
			private List<BaseEnemySpawnAddon> AddonList_Serialize = new List<BaseEnemySpawnAddon>();

			[SerializeField, LabelText("文件配置==额外附加项内容们"), ShowIf(nameof(ContainAddons)),
			 InlineEditor(InlineEditorObjectFieldModes.Boxed), ListDrawerSettings(ShowFoldout = true)]
			private List<SOConfig_EnemySpawnAddonPresetConfig> AddonList_File =
				new List<SOConfig_EnemySpawnAddonPresetConfig>();


			/// <summary>
			/// <para>非序列化且未初始化，则如果没有构建好的话一定是null的，</para>
			/// </summary>
			[NonSerialized, LabelText("具体运行时——额外附加项内容们")]
			private List<BaseEnemySpawnAddon> AddonList_RuntimeFinal;
			public List<BaseEnemySpawnAddon> GetRuntimeFinalAddonList()
			{
				AddonList_RuntimeFinal = new List<BaseEnemySpawnAddon>();
				AddonList_RuntimeFinal.AddRange(AddonList_Serialize);
				foreach (var perAddonPresetConfig in AddonList_File)
				{
					var newConfig = UnityEngine.Object.Instantiate(perAddonPresetConfig);
					foreach (var perAddon in newConfig.AddonList_Serialize)
					{
						AddonList_RuntimeFinal.Add(perAddon);
					}
				}
				return AddonList_RuntimeFinal;
			}

			[LabelText("将要触发的事件们_每个个体生成(分裂不计)"), GUIColor(255f / 255f, 248f / 255f, 10f / 255f),
			 InlineEditor(InlineEditorObjectFieldModes.Boxed), ListDrawerSettings(ShowFoldout = true),SerializeField,]
			public List<SOConfig_PrefabEventConfig> EventConfigList_Single = new List<SOConfig_PrefabEventConfig>();



			[LabelText("将要触发的事件们_每次生成(分裂不计)"), GUIColor(255f / 255f, 248f / 255f, 10f / 255f),
			 InlineEditor(InlineEditorObjectFieldModes.Boxed), ListDrawerSettings(ShowFoldout = true),SerializeField]
			public List<SOConfig_PrefabEventConfig> EventConfigList_Once = new List<SOConfig_PrefabEventConfig>();

		}



		/// <summary>
		/// <para>敌人生成配置的类型</para>
		/// </summary>
		public enum SpawnEnemyConfigTypeEnum
		{
			None_未指定 = 0, JustSpawn_简单生成 = 1, CountAndSupply_技术并补充 = 2,
		}

		[SerializeReference, LabelText("生成逻辑Handler")]
		public BaseSpawnEnemyConfigTypeHandler EnemySpawnConfigTypeHandler;


		[SerializeField, LabelText("正在覆写等级")]
		public bool IsOverridingLevel = false;


#if UNITY_EDITOR


		private void _Button_ChangeAllLevelTo1()
		{
			_Button_ChangeLevelOn(1);
		}


		[Button("修改等级", ButtonSizes.Large)]
		private void _Button_ChangeLevelOn(int at)
		{
			foreach (var perConfig in EnemySpawnConfigTypeHandler.Collection_SerializeConfig)
			{
				perConfig.OverrideLevelAt = at;
			}

			foreach (SOConfig_PerEnemyTypeSpawnConfig perConfigFile in
				EnemySpawnConfigTypeHandler.Collection_FileConfig)
			{
				foreach (var perConfig in perConfigFile.EnemyTypeInfoList)
				{
					perConfig.OverrideLevelAt = at;
				}
				UnityEditor.EditorUtility.SetDirty(perConfigFile);
			}

			UnityEditor.EditorUtility.SetDirty(this);
			UnityEditor.AssetDatabase.SaveAssets();
		}


#endif



		[NonSerialized, ReadOnly, ShowInInspector, LabelText("发起生成的事件Handler")]
		public GEH_生成敌人_SpawnEnemy RelatedSpawnEnemyGameplayEventHandlerRef;

		public void InitializeOnInstantiate(SOConfig_SpawnEnemyConfig rawTemplate , string relatedAreaID)
		{
			OriginalTemplateRef = rawTemplate;
			ConfigFinished = false;
			EnemySpawnConfigTypeHandler.InitializeOnInstantiate(this, rawTemplate, relatedAreaID);
		}






		public void ClearBeforeDestroy()
		{
			EnemySpawnConfigTypeHandler.ClearBeforeDestroy();
		}
#if UNITY_EDITOR
		// [Button("转换所有")]
		// private void ConvertAll()
		// {
		// 	//get all SOConfig_SpawnEnemyConfig from assetdatabase
		//           var allSOConfig_SpawnEnemyConfig = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_SpawnEnemyConfig");
		//           foreach (var configGUID in allSOConfig_SpawnEnemyConfig)
		// 	{
		// 		var configPath = UnityEditor.AssetDatabase.GUIDToAssetPath(configGUID);
		// 		SOConfig_SpawnEnemyConfig config = UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_SpawnEnemyConfig>(configPath);
		// 		if (config == null)
		// 		{
		// 			Debug.LogError($"无法加载配置文件：{configPath}");
		// 			continue;
		// 		}
		// 		SECT_简单生成_SimpleSpawnEnemyConfigType newHandler = new SECT_简单生成_SimpleSpawnEnemyConfigType();
		// 		newHandler.Collection = new List<PerEnemyTypeInfo>(config.Collection);
		// 		for (int i = 0; i < config.Collection.Count; i++)
		// 		{
		// 			PerEnemyTypeInfo tmp = config.Collection[i];
		// 			newHandler.Collection[i].AddonList = new List<BaseEnemySpawnAddon>(tmp.AddonList);
		// 		}
		// 		config.EnemySpawnConfigTypeHandler = newHandler;
		// 		UnityEditor.EditorUtility.SetDirty(config);
		// 	}
		//           UnityEditor.AssetDatabase.SaveAssets();
		//           
		// }
#endif

	}
}