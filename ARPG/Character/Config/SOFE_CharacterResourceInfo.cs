using System;
using System.Collections.Generic;
using Global;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace ARPG.Character.Config
{
	[CreateAssetMenu(fileName = "(S)角色资源信息", menuName = "#SO Assets#/#ARPG#/#角色#/SOFE_角色资源信息", order = 511)]
	[Serializable]
	public class SOFE_CharacterResourceInfo : BaseSOFromExcel
	{



		[Serializable]
		public class PerTypeInfo
		{
			[NonSerialized]
			public bool IsLoaded;
			public string CharacterName;


			public string A_ARPGCharacterPrefab;
			[NonSerialized]
			public AsyncOperationHandle<GameObject> _OP_ARPGCharacterPrefabHandle;

			public GameObject GetA_ARPGCharacterPrefab()
			{
				if (string.IsNullOrEmpty(A_ARPGCharacterPrefab))
				{
					DBug.LogError( $"角色{CharacterName}没有配置A_ARPGCharacterPrefab");
					return null;
				}
				if (!_OP_ARPGCharacterPrefabHandle.IsValid())
				{
					_OP_ARPGCharacterPrefabHandle = Addressables.LoadAssetAsync<GameObject>(A_ARPGCharacterPrefab);
					_OP_ARPGCharacterPrefabHandle.WaitForCompletion();
				}
				return _OP_ARPGCharacterPrefabHandle.Result;
			}

			public int CharacterIndex;



			public string A_RegionMapPrefab;
			[NonSerialized]
			public AsyncOperationHandle<GameObject> _OP_A_RegionMapPrefabHandle;


			public GameObject GetA_RegionMapPrefab()
			{
				if (string.IsNullOrEmpty(A_RegionMapPrefab))
				{
					DBug.LogError( $"角色{CharacterName}没有配置A_RegionMapPrefab");
					return null;
				}
				if (!_OP_A_RegionMapPrefabHandle.IsValid() )
				{
					_OP_A_RegionMapPrefabHandle = Addressables.LoadAssetAsync<GameObject>(A_RegionMapPrefab);
					_OP_A_RegionMapPrefabHandle.WaitForCompletion();
				}
				return _OP_A_RegionMapPrefabHandle.Result;
			}

			public string IdleAnimationOnUI;
			[NonSerialized]
			public AsyncOperationHandle<GameObject> _OP_IdleAnimationOnUIHandle;


			public GameObject GetIdleAnimationOnUI()
			{
				if (string.IsNullOrEmpty(IdleAnimationOnUI))
				{
					DBug.LogError( $"角色{CharacterName}没有配置IdleAnimationOnUI");
					return null;
				}
				if (!_OP_IdleAnimationOnUIHandle.IsValid())
				{
					_OP_IdleAnimationOnUIHandle = Addressables.LoadAssetAsync<GameObject>(IdleAnimationOnUI);
					_OP_IdleAnimationOnUIHandle.WaitForCompletion();
				}
				return _OP_IdleAnimationOnUIHandle.Result;
			}



			public string Icon_Inner;
			[NonSerialized]
			public AsyncOperationHandle<Sprite> _OP_Icon_InnerHandle;

			public Sprite GetIcon_Inner()
			{
				if (!_OP_Icon_InnerHandle.IsValid())
				{
					_OP_Icon_InnerHandle = Addressables.LoadAssetAsync<Sprite>(Icon_Inner);
					_OP_Icon_InnerHandle.WaitForCompletion();
				}
				return _OP_Icon_InnerHandle.Result;
			}
			public string Icon_Outer;
			[NonSerialized]
			public AsyncOperationHandle<Sprite> _OP_Icon_OuterHandle;

			 

			public Sprite GetIcon_Outer()
			{
				if (!_OP_Icon_OuterHandle.IsValid())
				{
					_OP_Icon_OuterHandle = Addressables.LoadAssetAsync<Sprite>(Icon_Outer);
					_OP_Icon_OuterHandle.WaitForCompletion();
				}
				return _OP_Icon_OuterHandle.Result;
			}
			public string Icon_Dialogue;
			[NonSerialized]
			public AsyncOperationHandle<Sprite> _OP_Icon_DialogueHandle;
			
			public Sprite GetIcon_Dialogue()
			{
				if (!_OP_Icon_DialogueHandle.IsValid())
				{
					_OP_Icon_DialogueHandle = Addressables.LoadAssetAsync<Sprite>(Icon_Dialogue);
					_OP_Icon_DialogueHandle.WaitForCompletion();
				}
				return _OP_Icon_DialogueHandle.Result;
			}

			
			
			public string Portrait_Half;
			[NonSerialized]
			public AsyncOperationHandle<Sprite> _OP_Portrait_HalfHandle;

			public Sprite GetPortrait_Half()
			{
				if (!_OP_Portrait_HalfHandle.IsValid())
				{
					_OP_Portrait_HalfHandle = Addressables.LoadAssetAsync<Sprite>(Portrait_Half);
					_OP_Portrait_HalfHandle.WaitForCompletion();
				}
				return _OP_Portrait_HalfHandle.Result;
			}
			public string Portrait_Pixel;
			[NonSerialized]
			public AsyncOperationHandle<Sprite> _OP_Portrait_PixelHandle;

			public Sprite GetPortrait_Pixel()
			{
				if (!_OP_Portrait_PixelHandle.IsValid())
				{
					_OP_Portrait_PixelHandle = Addressables.LoadAssetAsync<Sprite>(Portrait_Pixel);
					_OP_Portrait_PixelHandle.WaitForCompletion();
				}
				return _OP_Portrait_PixelHandle.Result;
			}





		}

		/// <summary>
		///  key是角色名字(Name,中文的那个)
		/// </summary>
		public Dictionary<string, PerTypeInfo> AllInfoDict = new Dictionary<string, PerTypeInfo>();

		public PerTypeInfo GetConfigByName(string cName)
		{
			 var info = AllInfoDict[cName];
			LoadTargetConfig(info);
			return info;
		}

		public PerTypeInfo GetConfigByType(CharacterNamedTypeEnum type)
		{
			var t = type.GetCharacterNameStringByType();
			if (string.IsNullOrEmpty(t))
			{
				return null;
			}
			var info = AllInfoDict[t];
			LoadTargetConfig(info);

			return info;
		}

		public PerTypeInfo GetConfigByType(int type)
		{
			 var t = ((CharacterNamedTypeEnum)type).GetCharacterNameStringByType();
			var info = AllInfoDict[t];
			LoadTargetConfig(info);
			return info;
		}



		public void PreloadOnStartLoadByGRS()
		{
			foreach (var item in AllInfoDict.Values)
			{
				item.IsLoaded = false;
			}
		}



		public GameObject GetIdleAnimationOnUI(string cName)
		{
			PerTypeInfo config = AllInfoDict[cName];
			LoadTargetConfig(config);
			return config._OP_IdleAnimationOnUIHandle.Result;
		}

		public Sprite GetIcon_Inner(string cName)
		{
			PerTypeInfo config = AllInfoDict[cName];
			LoadTargetConfig(config);
			return config._OP_Icon_InnerHandle.Result;
		}

		public Sprite GetIcon_Outer(string cName)
		{
			PerTypeInfo config = AllInfoDict[cName];
			LoadTargetConfig(config);
			return config._OP_Icon_OuterHandle.Result;
		}

		public Sprite GetIcon_Dialogue(string cName)
		{
			PerTypeInfo config = AllInfoDict[cName];
			LoadTargetConfig(config);
			if(config._OP_Icon_DialogueHandle.IsValid())
			{
				return config._OP_Icon_DialogueHandle.Result;
			}
			return null;
		}

		public Sprite GetPortrait_Half(string cName)
		{
			PerTypeInfo config = AllInfoDict[cName];
			LoadTargetConfig(config);
			return config._OP_Portrait_HalfHandle.Result;
		}

		public Sprite GetPortrait_Pixel(string cName)
		{
			PerTypeInfo config = AllInfoDict[cName];
			LoadTargetConfig(config);
			return config._OP_Portrait_PixelHandle.Result;
		}

		public GameObject GetA_ARPGCharacterPrefab(string cName)
		{
			PerTypeInfo config = AllInfoDict[cName];
			LoadTargetConfig(config);
			return config._OP_ARPGCharacterPrefabHandle.Result;
		}

		public GameObject GetA_RegionMapPrefab(string cName)
		{
			PerTypeInfo config = AllInfoDict[cName];
			LoadTargetConfig(config);
			return config._OP_A_RegionMapPrefabHandle.Result;
		}

		public GameObject GetA_RegionMapPrefab(int ind)
		{
			
			foreach (PerTypeInfo perInfo in AllInfoDict.Values)
			{
				if (perInfo.CharacterIndex == ind)
				{
					return perInfo.GetA_RegionMapPrefab();
				}
			}
			DBug.LogError($"没有能够使用数字：{ind} 找到对应的非战斗NPC Prefab");
			return null;
		}

		
		





		public void PreloadTarget(string cName)
		{
			PerTypeInfo config = AllInfoDict[cName];
			LoadTargetConfig(config);
		}


		private void LoadTargetConfig(PerTypeInfo config)
		{
			if (config.IsLoaded)
			{
				return;
			}
			config.IsLoaded = true;

			if (config.A_ARPGCharacterPrefab != null && config.A_ARPGCharacterPrefab.Length > 1)
			{
				if (!config._OP_A_RegionMapPrefabHandle.IsValid())
				{
					config._OP_ARPGCharacterPrefabHandle =
						Addressables.LoadAssetAsync<GameObject>(config.A_ARPGCharacterPrefab);
					config._OP_ARPGCharacterPrefabHandle.WaitForCompletion();
				}
            }

            if (config.IdleAnimationOnUI != null && config.IdleAnimationOnUI.Length > 1)
            {
                if (!config._OP_IdleAnimationOnUIHandle.IsValid())
                {
                    config._OP_IdleAnimationOnUIHandle =
                        Addressables.LoadAssetAsync<GameObject>(config.IdleAnimationOnUI);
                    config._OP_IdleAnimationOnUIHandle.WaitForCompletion();
                }
            }

            if (config.A_RegionMapPrefab != null && config.A_RegionMapPrefab.Length > 1)
			{
				if (!config._OP_A_RegionMapPrefabHandle.IsValid())
				{
					config._OP_A_RegionMapPrefabHandle =
						Addressables.LoadAssetAsync<GameObject>(config.A_RegionMapPrefab);
					config._OP_A_RegionMapPrefabHandle.WaitForCompletion();
				}
			}

			if (config.Icon_Inner != null && config.Icon_Inner.Length > 1)
			{
				if (!config._OP_Icon_InnerHandle.IsValid())
				{
					config._OP_Icon_InnerHandle = Addressables.LoadAssetAsync<Sprite>(config.Icon_Inner);
					config._OP_Icon_InnerHandle.WaitForCompletion();
				}
			}

			if (config.Icon_Outer != null && config.Icon_Outer.Length > 1)
			{
				if (!config._OP_Icon_OuterHandle.IsValid())
				{
					config._OP_Icon_OuterHandle = Addressables.LoadAssetAsync<Sprite>(config.Icon_Outer);
					config._OP_Icon_OuterHandle.WaitForCompletion();
				}
			}
			
			if (config.Icon_Dialogue != null && config.Icon_Dialogue.Length > 1)
			{
				if (!config._OP_Icon_DialogueHandle.IsValid())
				{
					config._OP_Icon_DialogueHandle = Addressables.LoadAssetAsync<Sprite>(config.Icon_Dialogue);
					config._OP_Icon_DialogueHandle.WaitForCompletion();
				}
			}

			if (config.Portrait_Half != null && config.Portrait_Half.Length > 1)
			{
				if (!config._OP_Portrait_HalfHandle.IsValid())
				{
					config._OP_Portrait_HalfHandle = Addressables.LoadAssetAsync<Sprite>(config.Portrait_Half);
					config._OP_Portrait_HalfHandle.WaitForCompletion();
				}
			}

			if (config.Portrait_Pixel != null && config.Portrait_Pixel.Length > 1)
			{
				if (!config._OP_Portrait_PixelHandle.IsValid())
				{
					config._OP_Portrait_PixelHandle = Addressables.LoadAssetAsync<Sprite>(config.Portrait_Pixel);
					config._OP_Portrait_PixelHandle.WaitForCompletion();
				}
			}
		}

	}
}