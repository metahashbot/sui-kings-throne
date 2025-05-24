using System;
using System.Collections.Generic;
using Global;
using Global.Loot;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace ARPG.Equipment
{
	[Serializable]
	[CreateAssetMenu(fileName = "具体武器", menuName = "#SO Assets#/#Preset SO#/具体武器", order = 351)]
	public class SOFE_WeaponEquipmentMatching : ScriptableObject
	{



		[Serializable]
		public class PerTypeInfo
		{
			public string UID;
			public string EquipmentName;
			public string EquipmentTemplateName;
			public string IconPath;
			[NonSerialized]
			public AsyncOperationHandle<Sprite> _OP_EquipmentIcon;
			public Sprite GetEquipmentIcon()
			{
				if (!_OP_EquipmentIcon.IsValid())
				{
					 _OP_EquipmentIcon = Addressables.LoadAssetAsync<Sprite>(IconPath);
					 _OP_EquipmentIcon.WaitForCompletion();
				}
				return _OP_EquipmentIcon.Result;
			}

			public List<IngredientInfoPair> CraftIngredientList =
				new List<IngredientInfoPair>();

			public List<EquipmentRawPerkInfoPair> PerkInfoList = new List<EquipmentRawPerkInfoPair>();

			public List<IngredientInfoPair> SellPrice = new List<IngredientInfoPair>();
			
			public string WeaponUIIdleAnimationAddress;
			[NonSerialized]
			public AsyncOperationHandle<AnimationClip> _op_WeaponUIIdleAnimationHandle;

		}

		[Searchable]
		public List<PerTypeInfo> AllTypeInfoList = new List<PerTypeInfo>();



		/// <summary>
		/// 试图按照装备名字查找，第二列的那个，显示给人看的那个。会查找到第一个匹配的。
		/// <para>会有若干相同模板名的装备条目的，它们之间的区别是品质不同</para>
		/// </summary>
		/// <returns></returns>
		public bool TryGetByEquipmentName(string equipmentName , out PerTypeInfo get)
		{
			for (int i = 0; i < AllTypeInfoList.Count; i++)
			{
				var tInfo = AllTypeInfoList[i];
				if (tInfo.EquipmentName.Trim().Equals(equipmentName.Trim(), StringComparison.OrdinalIgnoreCase))
				{
					get = tInfo;
					return true;
				}
			}
			get = null;
			return false;
		}

        
		public bool TryGetTemplateByUID(string uid ,out PerTypeInfo get)
		{
			var index = AllTypeInfoList.FindIndex((x) =>
				x.UID.Trim().Equals(uid.Trim(), StringComparison.OrdinalIgnoreCase));
			if (index == -1)
			{
				get = null;
				return false;
			}
			
			get = AllTypeInfoList[index];
			return true;
			
		}
		
		
		
		public PerTypeInfo GetTemplateInfoByUID(string uid)
		{
			var index = AllTypeInfoList.FindIndex((x) =>
				x.UID.Trim().Equals(uid.Trim(), StringComparison.OrdinalIgnoreCase));
			if (index == -1)
			{
				throw new IndexOutOfRangeException($"查找武器UID : {uid}时，没有找到对应的武器");
			}
			
			return AllTypeInfoList[index];
		}
	}
}