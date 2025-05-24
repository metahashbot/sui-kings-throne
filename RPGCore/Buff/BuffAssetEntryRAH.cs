using System.Collections.Generic;
using Global;
using Global.AssetLoad;
using Global.RuntimeAssetHolder;
using RPGCore.Buff.Config;
using UnityEngine;
namespace RPGCore.Buff
{
	public class BuffAssetEntryRAH : RuntimeAssetHolder_Base
	{
		private SOCollection_RPBuffAssetEntry _buffAssetEntryCollection;
		public class PerBuffTypeContent
		{
			public RolePlay_BuffTypeEnum BuffType;
			public RPBuffAssetEntry LoadedEntry;
		}

		public Dictionary<RolePlay_BuffTypeEnum, PerBuffTypeContent> AllLoadedBuffAssetEntryDict
			= new Dictionary<RolePlay_BuffTypeEnum, PerBuffTypeContent>();


		public RPBuffAssetEntry GetTargetBuffAssetEntry(RolePlay_BuffTypeEnum type)
		{
			if (!AllLoadedBuffAssetEntryDict.ContainsKey(type))
			{
				Debug.LogWarning($"Buff{type}在要求RAH返回对应Entry时，RAH并没有对应项目，将要在运行时加载，这不太好，检查一下预加载逻辑");
				LoadTargetTypeBuffAssetEntry(type);
			}

			return AllLoadedBuffAssetEntryDict[type].LoadedEntry;
		}
		
		public void LoadTargetTypeBuffAssetEntry(RolePlay_BuffTypeEnum type)
		{
			if (AllLoadedBuffAssetEntryDict.ContainsKey(type))
			{
				return;
			}
			RPBuffConfigContentInSO_LoadAssetEntry target = _buffAssetEntryCollection.GetAssetEntryInfoByType(type).EntryContent;
			if (target != null)
			{
				PerBuffTypeContent tmpNewContent = new PerBuffTypeContent();
				AllLoadedBuffAssetEntryDict.Add(type, tmpNewContent);
				tmpNewContent.LoadedEntry.LoadEntryByAssetEntryConfigSO(target);
			}
			else
			{
				Debug.LogError($"在加载Buff{type}的时候，被要求加载关联的AssetEntry了，但是并没有查找到相关的AssetEntry");
			}
		}

		public override void PrepareCollection_InitializeOnStart(CollectionContainRAHUtility lutBase,
			GeneralGameAssetLoadPhaseEnum phaseFlag = GeneralGameAssetLoadPhaseEnum.None)
		{

			_buffAssetEntryCollection = lutBase as SOCollection_RPBuffAssetEntry;
			
		}
		public override void UnloadAndClear()
		{
			foreach (var perContent in AllLoadedBuffAssetEntryDict.Values)
			{
				perContent.LoadedEntry.UnloadAndClear();
			}
			AllLoadedBuffAssetEntryDict.Clear();
		}
	}
}