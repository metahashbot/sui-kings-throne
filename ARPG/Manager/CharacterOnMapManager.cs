using System;
using System.Collections.Generic;
using System.Reflection;
using ARPG.Character;
using ARPG.Character.Base;
using ARPG.Character.Config;
using ARPG.Character.Enemy;
using ARPG.Character.Enemy.AI;
using ARPG.Character.Enemy.AI.Decision;
using ARPG.Character.Enemy.AI.Listen;
using ARPG.Character.Player;
using ARPG.Character.Player.Ally;
using ARPG.Config;
using ARPG.Manager.Component;
using ARPG.Manager.Config;
using Global;
using Global.ActionBus;
using Global.AreaOnMap.AreaFunctionHandler;
using Global.AreaOnMap.EditorProxy;
using Global.Character;
using Global.GlobalConfig;
using Global.Utility;
using RPGCore;
using RPGCore.Buff;
using RPGCore.DataEntry;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Pool;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using WorldMapScene.Character.NPC;
using WorldMapScene.Character;
using WorldMapScene.RegionMap;
using Object = UnityEngine.Object;
using WorldMapScene.Player;

namespace ARPG.Manager
{
	/// <summary>
	/// <para>ARPG场景的角色管理器。</para>
	/// </summary>
	public class CharacterOnMapManager : MonoBehaviour
	{
#region 外部引用

		private SubGameplayLogicManager_ARPG _glmRef;

		private PlayerCharacterBehaviourController _playerCharacterBehaviourControllerRef;


		[ShowInInspector, LabelText("当前活跃着的所有ARPG角色容器")]
		public List<BaseARPGCharacterBehaviour> CurrentAllActiveARPGCharacterBehaviourCollection;


		/// <summary>
		/// <para>用于执行各种查找的时候的通用容器</para>
		/// <para>！只能立刻使用！因为查询操作始终都使用的是这个容器</para>
		/// <para>如果需要缓存查询结果，则需要调用者自备容器</para>
		/// </summary>
		private List<EnemyARPGCharacterBehaviour> _internalQueryEnemyList;


		/// <summary>
		/// <para>用于执行各种查找的时候的通用容器</para>
		/// <para>！只能立刻使用！因为查询操作始终都使用的是这个容器</para>
		/// <para>如果需要缓存查询结果，则需要调用者自备容器</para>
		/// </summary>
		private List<BaseARPGCharacterBehaviour> _internalQueryCharacterList;

        [SerializeField, LabelText("prefab_空的NPC队伍模板"), TitleGroup("===运行时===")]
        private GameObject _prefab_npcTeamTemplate;
        [SerializeField, LabelText("prefab-头顶文字模板"), TitleGroup("配置")]
        public GameObject _prefab_headTextTemplate;

        [ShowInInspector, LabelText("记录当前的所有NPC组"), NonSerialized]
        [FoldoutGroup("运行时", true)]
        public List<BaseCharacterGroup_NonBattle> AllNPCGroupOnMapList;
        #endregion



        [ShowInInspector, LabelText("当前角色尺寸"), FoldoutGroup("运行时", true)]
		public float CurrentCharacterScaleInScene { get; private set; } = 1f;
        public float CurrentNPCCharacterScaleInScene { get; private set; } = 1f;




        public void SetCharacterScaleScale(float scale)
		{
			CurrentCharacterScaleInScene = scale;
			foreach (BaseARPGCharacterBehaviour character in CurrentAllActiveARPGCharacterBehaviourCollection)
			{
				character.GetRelatedArtHelper()
					.SetCharacterLocalScale(character.SelfCharacterVariantScale * CurrentCharacterScaleInScene);
			}
		}

        public void SetNPCGroupScale(float scale)
        {
            CurrentNPCCharacterScaleInScene = scale;
			if (null != AllNPCGroupOnMapList)
			{
                for (int i = 0; i < AllNPCGroupOnMapList.Count; i++)
                {
					AllNPCGroupOnMapList[i].SetAllCharacterScale(CurrentNPCCharacterScaleInScene);
                }
            }            
        }

        private float _currentTime;

		private int _nameCounter;


		protected Vector3 _currentCharacterRotationEuler = new Vector3(30f, 0f, 0f);
		/// <summary>
		/// <para>不会被当做有效对象的默认列表。比如各种机制物</para>
		/// </summary>
		[LabelText("默认搜索时忽略的列表。")]
		[SerializeField]
		public List<CharacterNamedTypeEnum> _DefaultIgnoreEnemyTypeList = new List<CharacterNamedTypeEnum>()
		{
			CharacterNamedTypeEnum.Utility_VoidEntity_空实体, CharacterNamedTypeEnum.AttackCircle_攻击反回血圈,
			CharacterNamedTypeEnum.DefenseCircle_防御反免伤圈, CharacterNamedTypeEnum.VelocityCircle_速度反召唤圈
		};



		public Vector3 CurrentCharacterRotationEuler => _currentCharacterRotationEuler;
		public void SetCurrentCharacterRotationEuler(Vector3 euler)
		{
			_currentCharacterRotationEuler = euler;
			Quaternion q = Quaternion.Euler(euler);
			foreach (BaseARPGCharacterBehaviour perBehaviour in CurrentAllActiveARPGCharacterBehaviourCollection)
			{
				perBehaviour.GetSelfRolePlayArtHelper().SetCharacterRotation_Quaternion(q);
			}
		}


#region 自发光信息

		private DS_CharacterEmissionInfo _currentEnemyEmissionInfo;

		public void SetCurrentEmissionInfo(DS_CharacterEmissionInfo emission)
		{
			_currentEnemyEmissionInfo = emission;
			foreach (BaseARPGCharacterBehaviour perBehaviour in CurrentAllActiveARPGCharacterBehaviourCollection)
			{
				perBehaviour.GetRelatedArtHelper().SetEmission_All(emission);
			}
		}

#endregion

#region 描边任务预设

		 [SerializeField,LabelText("描边任务预设们")]
		private List<SheetOutlinePresetTask> _presetOutlinePresetTaskList = new List<SheetOutlinePresetTask>();

		public SheetOutlineTaskInfo GetOutlinePresetTaskByPresetID(string presetID)
		{
			return _presetOutlinePresetTaskList.Find((info => info.TaskPresetID.Equals(presetID))).TaskInfo;
		}

#endregion

#region 过伤击杀特效

		[SerializeField, LabelText("电"), TitleGroup("===过伤VFX==="), AssetsOnly]
		public GameObject OverloadDamageVFXPrefab_Electric_Dian;


		[SerializeField, LabelText("光"), TitleGroup("===过伤VFX==="), AssetsOnly]
		public GameObject OverloadDamageVFXPrefab_Light_Guang;
		  
		[SerializeField, LabelText("火"), TitleGroup("===过伤VFX==="), AssetsOnly]
		public GameObject OverloadDamageVFXPrefab_Fire_Huo;
		 
		
		[SerializeField, LabelText("水"), TitleGroup("===过伤VFX==="), AssetsOnly]
		public GameObject OverloadDamageVFXPrefab_Water_Shui;
		
		[SerializeField, LabelText("风"), TitleGroup("===过伤VFX==="), AssetsOnly]
		public GameObject OverloadDamageVFXPrefab_Wind_Feng;
		
		[SerializeField, LabelText("土"), TitleGroup("===过伤VFX==="), AssetsOnly]
		public GameObject OverloadDamageVFXPrefab_Earth_Tu;
		
		[SerializeField, LabelText("猩红"), TitleGroup("===过伤VFX==="), AssetsOnly]
		public GameObject OverloadDamageVFXPrefab_Crimson_Xinghong;
		
		[SerializeField, LabelText("灵"), TitleGroup("===过伤VFX==="), AssetsOnly]
		public GameObject OverloadDamageVFXPrefab_Spirit_Ling;
		

#endregion




		public class NPCCorpseInfo
		{
			[ShowInInspector, LabelText("变成尸体的时间")]
			public float BeginTime;

			[ShowInInspector, LabelText("尸体还会存在的时间")]
			public float RemainingExistDuration;


			[ShowInInspector, LabelText("关联的NPC Behaviour引用")]
			public BaseARPGCharacterBehaviour RelatedBehaviourRef;

			/// <summary>
			/// <para>死亡之后由于不再处于ActiveBehaviour之中了，但是动画依然要刷新，所以在Corpse容器中会只tickArtHelper</para>
			/// </summary>
			[ShowInInspector, LabelText("关联的ArtHelper引用")]
			public BaseARPGArtHelper RelatedArtHelperRef;

			//reset
			public void Reset()
			{
				BeginTime = 0;
				RemainingExistDuration = 0;
				RelatedBehaviourRef = null;
			}
		}


		private List<NPCCorpseInfo> _currentCorpseInfoCollection;


		public void AwakeInitialize(SubGameplayLogicManager_ARPG glm)
		{
			_glmRef = glm;
			CurrentAllActiveARPGCharacterBehaviourCollection = new List<BaseARPGCharacterBehaviour>();
			_internalQueryEnemyList = new List<EnemyARPGCharacterBehaviour>();
			_currentCorpseInfoCollection = new List<NPCCorpseInfo>();
			_internalQueryCharacterList = new List<BaseARPGCharacterBehaviour>();
            AllNPCGroupOnMapList = new List<BaseCharacterGroup_NonBattle>();
        }


		public void StartInitialize()
		{
			var gab = GlobalActionBus.GetGlobalActionBus();
			BaseARPGCharacterBehaviour.StaticInitialize(_glmRef);

			_playerCharacterBehaviourControllerRef = _glmRef.PlayerCharacterBehaviourControllerReference;
			gab.RegisterAction(ActionBus_ActionTypeEnum.G_EnemyBehaviour_OnEnemyDieToCorpse_敌人死亡到尸体,
				_ABC_OnNPCBehaviourDeathToCorpse,
				300);
			gab.RegisterAction(ActionBus_ActionTypeEnum.G_AllyBehaviour_OnAllyDieToCorpse_友军死亡到尸体,
				_ABC_OnNPCBehaviourDeathToCorpse,
				300);
			gab.RegisterAction(ActionBus_ActionTypeEnum.G_EnemyBehaviour_OnEnemyDieDirect_敌人直接死亡没有尸体,
				_ABC_OnNPCBehaviourDieDirect,
				300);
			gab.RegisterAction(ActionBus_ActionTypeEnum.G_AllyBehaviour_OnAllyDieDirect_友军直接死亡没有尸体,
				_ABC_OnNPCBehaviourDieDirect,
				300);
			gab.RegisterAction(ActionBus_ActionTypeEnum.G_CharacterOnMap_SpawnNewEnemy_生成了一个新敌人,
				_ABC_TryEnableRenderObjects);
			gab.RegisterAction(ActionBus_ActionTypeEnum.G_EnemyBehaviour_OnEnemyDieToCorpse_敌人死亡到尸体,
				_ABC_TryDisableRenderObjects);
			_nameCounter = 0;
		}

		public void LateInitialize()
		{
			_Debug_InitializePresetDebugEnemy();
			BaseAIBrainHandler.StaticInitialize();
			BaseDecisionHandler.StaticInitialize();
			;
		}

#region 生成

		public AllyARPGCharacterBehaviour SpawnNewAllyByTypeAndPosition(
			CharacterNamedTypeEnum characterType,
			Vector3 position,
			int level = 1)
		{
			var lut = GlobalConfigurationAssetHolderHelper.Instance.FE_ARPGCharacterInitConfig;
			(SOFE_ARPGCharacterInitConfig.PerConfigEntryInSOFE,
				SOFE_ARPGCharacterInitRPGEntry_BaseRPG.ARPGCharacterEntry,
				SOFE_CharacterResourceInfo.PerTypeInfo) fullConfig = characterType.GetFullThreeInfoByType(level);
			GameObject enemyPrefab = fullConfig.Item3.GetA_ARPGCharacterPrefab();

			GameObject newObj = Object.Instantiate(enemyPrefab, transform);
			newObj.transform.position = position;
			var allyBehaviour = newObj.GetComponent<AllyARPGCharacterBehaviour>();
			if (allyBehaviour == null)
			{
				Debug.LogError($"正在按照[友军]生成角色{characterType}，但是并没有GetComponent到AllyBehaviour，" +
				               $"可能它就不是一个友军，或者它的prefab上没有AddComponent");
				return null;
			}
			allyBehaviour.InitializeOnInstantiate();
			SOFE_ARPGCharacterInitConfig.PerConfigEntryInSOFE config = fullConfig.Item1;

			allyBehaviour.InitializeByConfig(fullConfig.Item1, fullConfig.Item2, fullConfig.Item3, position);

			allyBehaviour.RefillPositionRecord(position,0f);
			AllyARPGArtHelper artHelper = allyBehaviour.GetSelfRolePlayArtHelper() as AllyARPGArtHelper;
			artHelper.SetCharacterRotation_Quaternion(Quaternion.Euler(_currentCharacterRotationEuler));
			artHelper.SetEmissionInfo(_currentEnemyEmissionInfo);
			CurrentAllActiveARPGCharacterBehaviourCollection.Add(allyBehaviour);
			var ds = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_CharacterOnMap_SpawnNewAlly_生成了一个新友军);
			ds.ObjectArgu1 = allyBehaviour;
			GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds);
			return allyBehaviour;
		}



		/// <summary>
		/// <para>运行时生成敌人。常见于分身|召唤等情况。</para>
		/// <para>   [清怪]处理？清怪的逻辑是会扫整个COM的，只需要新生成的敌人身上的 RelatedConfigInstance 里面的ID和区域ID匹配就能维持逻辑正常</para>
		/// </summary>
		public EnemyARPGCharacterBehaviour SpawnNewEnemyBySingleConfigEntryInRuntime(
			EnemyARPGCharacterBehaviour originalBehaviourRef,
			EnemySpawnService_SubActivityService.SingleSpawnInfo singleInfo,
			Vector3 position, float scale = 1f,
			bool appendSplit = false)
		{
			var lut = GlobalConfigurationAssetHolderHelper.Instance.FE_ARPGCharacterInitConfig;

			int atLevel = singleInfo.RelatedConfigInstance.EnemySpawnConfigTypeHandler.EnemyLevelInSpawnConfigHandler;

			if (singleInfo.RawSpawnInfo.IsOverridingLevel)
			{
				atLevel = singleInfo.RawSpawnInfo.OverrideLevelAt;
			}
			(SOFE_ARPGCharacterInitConfig.PerConfigEntryInSOFE,
				SOFE_ARPGCharacterInitRPGEntry_BaseRPG.ARPGCharacterEntry,
				SOFE_CharacterResourceInfo.PerTypeInfo) fullConfig =
					singleInfo.RawSpawnInfo.Type.GetFullThreeInfoByType(atLevel);




			GameObject enemyPrefab = fullConfig.Item3.GetA_ARPGCharacterPrefab();
			GameObject newObj = Object.Instantiate(enemyPrefab, transform);
			_nameCounter += 1;
			newObj.name = $"{originalBehaviourRef.name}_Split_{_nameCounter}";
			newObj.transform.position = position;
			EnemyARPGCharacterBehaviour enemyBehaviour = newObj.GetComponent<EnemyARPGCharacterBehaviour>();
			enemyBehaviour.InitializeOnInstantiate();
			enemyBehaviour.SetCharacterVariantScale(scale);
			
			enemyBehaviour.SetSpawnConfigRef(singleInfo.RelatedConfigInstance, singleInfo, null);
			enemyBehaviour.RefillPositionRecord(position,0f);


			enemyBehaviour.InitializeByConfig(fullConfig.Item1, fullConfig.Item2, fullConfig.Item3, position);
			var artHelper = enemyBehaviour.GetSelfRolePlayArtHelper() as EnemyARPGArtHelper;
			artHelper.SetRotation_EulerXY(_currentCharacterRotationEuler.x, _currentCharacterRotationEuler.y);

			if (_currentEnemyEmissionInfo != null)
			{
				artHelper.SetEmissionInfo(_currentEnemyEmissionInfo);
			}

			if (appendSplit)
			{
				enemyBehaviour.ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum._EnemyTag_BySplit_分裂产生的敌人,
					enemyBehaviour,
					enemyBehaviour);
			}

			CurrentAllActiveARPGCharacterBehaviourCollection.Add(enemyBehaviour);
            var ds_spawn = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_CharacterOnMap_SpawnNewEnemy_生成了一个新敌人)
            {
                ObjectArgu1 = enemyBehaviour
            };
            GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_spawn);
			return enemyBehaviour;
		}


        /// <summary>
        /// <para>生成一个单个的敌人</para>
        /// </summary>
        public EnemyARPGCharacterBehaviour SpawnNewEnemy(
            CharacterNamedTypeEnum enemyType,
            Vector3 position,
            float characterScaleMul = 1.0f)
        {
            var fullConfig = enemyType.GetFullThreeInfoByType();

            GameObject enemyPrefab = fullConfig.Item3.GetA_ARPGCharacterPrefab();
            GameObject newObj = Object.Instantiate(enemyPrefab, transform);
            newObj.transform.position = position;
            newObj.name = $"{newObj.name}_{++_nameCounter}";
            EnemyARPGCharacterBehaviour enemyBehaviour = newObj.GetComponent<EnemyARPGCharacterBehaviour>();
            enemyBehaviour.InitializeOnInstantiate();
            enemyBehaviour.SetCharacterVariantScale(characterScaleMul);
            enemyBehaviour.RefillPositionRecord(position, 0f);
            enemyBehaviour.InitializeByConfig(fullConfig.Item1, fullConfig.Item2, fullConfig.Item3, position);
            
			var artHelper = enemyBehaviour.GetSelfRolePlayArtHelper() as EnemyARPGArtHelper;
            artHelper.SetRotation_EulerXY(_currentCharacterRotationEuler.x, _currentCharacterRotationEuler.y);
            if (_currentEnemyEmissionInfo != null)
            {
                artHelper.SetEmissionInfo(_currentEnemyEmissionInfo);
            }

            CurrentAllActiveARPGCharacterBehaviourCollection.Add(enemyBehaviour);

            var ds_spawn = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_CharacterOnMap_SpawnNewEnemy_生成了一个新敌人)
            {
                ObjectArgu1 = enemyBehaviour
            };
            GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_spawn);
            return enemyBehaviour;
        }




        /// <summary>
        /// <para>生成一个单个的敌人</para>
        /// </summary>
        public EnemyARPGCharacterBehaviour SpawnNewEnemyBySingleConfigEntry(
			EnemySpawnService_SubActivityService.SingleSpawnInfo singleInfo,
			Vector3 position,
			EnemySpawnPositionRuntimeInfo spawnPositionInfo)
		{
			var lut = GlobalConfigurationAssetHolderHelper.Instance.FE_ARPGCharacterInitConfig;

			int atLevel = singleInfo.RelatedConfigInstance.EnemySpawnConfigTypeHandler.EnemyLevelInSpawnConfigHandler;
			if (singleInfo.RawSpawnInfo.IsOverridingLevel)
			{
				atLevel = singleInfo.RawSpawnInfo.OverrideLevelAt;
			}
			var fullConfig = singleInfo.RawSpawnInfo.Type.GetFullThreeInfoByType(atLevel);

			GameObject enemyPrefab = fullConfig.Item3.GetA_ARPGCharacterPrefab();
			GameObject newObj = Object.Instantiate(enemyPrefab, transform);
			newObj.transform.position = position;
			_nameCounter += 1;
			newObj.name = $"{newObj.name}_{_nameCounter}";
			EnemyARPGCharacterBehaviour enemyBehaviour = newObj.GetComponent<EnemyARPGCharacterBehaviour>();
			enemyBehaviour.InitializeOnInstantiate();
			enemyBehaviour.SetCharacterVariantScale(singleInfo.CharacterSelfVariantScaleMul);
			enemyBehaviour.SetSpawnConfigRef(singleInfo.RelatedConfigInstance, singleInfo, spawnPositionInfo);
			enemyBehaviour.RefillPositionRecord(position, 0f);
			enemyBehaviour.InitializeByConfig(fullConfig.Item1, fullConfig.Item2, fullConfig.Item3, position);
			var artHelper = enemyBehaviour.GetSelfRolePlayArtHelper() as EnemyARPGArtHelper;
			artHelper.SetRotation_EulerXY(_currentCharacterRotationEuler.x, _currentCharacterRotationEuler.y);

			if (_currentEnemyEmissionInfo != null)
			{
				artHelper.SetEmissionInfo(_currentEnemyEmissionInfo);
			}

			CurrentAllActiveARPGCharacterBehaviourCollection.Add(enemyBehaviour);
            var ds_spawn = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_CharacterOnMap_SpawnNewEnemy_生成了一个新敌人)
            {
                ObjectArgu1 = enemyBehaviour
            };
            GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_spawn);
            return enemyBehaviour;
		}




		public void _Debug_InitializePresetDebugEnemy()
		{
			//在这个时候找到的Enemy那一定是测试用的直接摆好的
			var presetDebugEnemy = FindObjectsOfType<EnemyARPGCharacterBehaviour>();
			foreach (var enemy in presetDebugEnemy)
			{
				enemy.InitializeOnInstantiate();
                var ds_spawn = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_CharacterOnMap_SpawnNewEnemy_生成了一个新敌人)
                {
                    ObjectArgu1 = enemy
                };
                GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_spawn);
                var fullConfig = enemy.SelfBehaviourNamedType.GetFullThreeInfoByType(1);

				enemy.InitializeByConfig(fullConfig.Item1, fullConfig.Item2, fullConfig.Item3, new Vector3());


				CurrentAllActiveARPGCharacterBehaviourCollection.Add(enemy);
			}
		}


		/// <summary>
		/// <para>LateLoad时序中，将玩家生成在地图上的合适位置</para>
		/// </summary>
		public void SpawnPlayerCharacterOnMapOnLateInit()
		{
			//SOFE_CharacterInit
			SOFE_ARPGCharacterInitConfig sofe_characterInitConfig =
				GlobalConfigurationAssetHolderHelper.Instance.FE_ARPGCharacterInitConfig;

			var currentIndex = GlobalConfigSO.RuntimeContent().CurrentActiveTeamIndex;

			//先找SpawnPoint
			var playerSpawnPoint = _glmRef.PlayerSpawnRegisterRef;
			//找到当前的队伍信息
			GlobalConfigSO.GCSO_PlayerTeamInfo teamInfo = GlobalConfigurationAssetHolderHelper.Instance
				.GlobalConfigSO_Runtime.Content.CurrentPlayerTeamList.Find((info => info.TeamIndex == currentIndex));

			//获取存档里的玩家角色信息
			List<GCSO_PerCharacterInfo> allCharacterInfoInGCSO = GlobalConfigurationAssetHolderHelper.Instance
				.GlobalConfigSO_Runtime.Content.AllCharacterInfoCollection;
			//从RRH中找到当前的玩家角色们
			var characterRRH = GlobalConfigurationAssetHolderHelper.Instance.RuntimeRecordHelperCharacter;

			var lightSuppress = GlobalConfigurationAssetHolderHelper.Instance.MiscSetting_Runtime.SettingContent
				.GetCurrentLevelDamageBonusPercent();

			//逐个生成角色。队伍中的第一个是队长，作为活跃的角色，其他就绪


			var lut_CharacterPrefab = GlobalConfigurationAssetHolderHelper.Instance.FE_ARPGCharacterInitConfig;
			Vector3 offsetLength = Vector3.forward;

			for (int i = 0; i < teamInfo.CharacterList.Count; i++)
			{
				CharacterNamedTypeEnum currentCharacterType = teamInfo.CharacterList[i];
				//从存档里查当前等级之类的，然后获取实际的具体将要使用的数据项
				GCSO_PerCharacterInfo characterRawInfo =
					allCharacterInfoInGCSO.Find(info => info.CharacterID == (int)currentCharacterType);


				//先是GO本身
				var fullConfig = currentCharacterType.GetFullThreeInfoByType();

				var prefab = fullConfig.Item3.GetA_ARPGCharacterPrefab();
				var newGO = Instantiate(prefab, _glmRef.PlayerCharacterBehaviourControllerReference.transform);
				var newBehaviour = newGO.GetComponent<PlayerARPGConcreteCharacterBehaviour>();
				newBehaviour.InitializeOnInstantiate();

				Vector3? aligned = _glmRef.GetAlignedTerrainPosition(playerSpawnPoint.transform.position);
				if (aligned == null)
				{
					DBug.LogError($"玩家角色{currentCharacterType}生成时，没有找到合适的地面位置，将使用原始位置");
					aligned = playerSpawnPoint.transform.position;
				}

				newBehaviour.InitializeByConfig(fullConfig.Item1, fullConfig.Item2, fullConfig.Item3, aligned.Value);




				_playerCharacterBehaviourControllerRef.RegisterNewCharacterBehaviour(newBehaviour);

				newBehaviour.transform.position = (aligned.Value);
				newBehaviour.RefillPositionRecord(aligned.Value, 0f);



				CurrentAllActiveARPGCharacterBehaviourCollection.Add(newBehaviour);
				GlobalActionBus.GetGlobalActionBus().TriggerActionByType(
					ActionBus_ActionTypeEnum.G_CharacterOnMap_SpawnNewPlayerCharacter_生成了一个新的玩家角色,
					new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
							.G_CharacterOnMap_SpawnNewPlayerCharacter_生成了一个新的玩家角色,
						prefab)); //i==0，是队长，初始激活，其他人按就绪
				newBehaviour.GetSelfRolePlayArtHelper().SetEmission_All(_currentEnemyEmissionInfo);
				if (i == 0)
				{
					newBehaviour.MakeCharacterStateTransition(PlayerARPGConcreteCharacterBehaviour
						.PlayerCharacterTransitionTypeEnum.SpawnAsUsing_生成时直接使用);
					_playerCharacterBehaviourControllerRef.CurrentControllingBehaviour = newBehaviour;
				}
				else
				{
					newBehaviour.MakeCharacterStateTransition(PlayerARPGConcreteCharacterBehaviour
						.PlayerCharacterTransitionTypeEnum.SpawnAsReady_生成时就绪);
				}
				newBehaviour.SetCharacterSlot(i + 1);
				if (lightSuppress > 0f)
				{
					newBehaviour.ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.LightSuppress_光等压制,
						newBehaviour,
						newBehaviour);
				}
			}
			GlobalActionBus.GetGlobalActionBus()
				.TriggerActionByType(ActionBus_ActionTypeEnum.G_PC_OnTeamMemberChanged_队伍成员更换);
		}
        
        public CharacterGroup_NPCTeam AddNewNPCTeam(Vector3 position, EP_BaseArea relatedAreaRef, params CharacterNamedTypeEnum[] names)
        {            
            var newGroup = UnityEngine.Object.Instantiate(_prefab_npcTeamTemplate, transform);
            var newTeam = newGroup.GetComponent<CharacterGroup_NPCTeam>();
            newTeam.transform.position = Vector3.zero;
            newTeam.InstantiateInitialize(null);
            //newTeam.SetAllCharacterScale(CurrentCharacterSize);

            newTeam.SetRelatedAreaRef(relatedAreaRef);

            newTeam.AddCharacter_NPCTeam(position, names);


            foreach (BaseCharacterBehaviour_NonBattle perGroup in newTeam.AllCharacterInThisGroup)
            {
                perGroup.ArtHelperRef.SetRotation_EulerX(_currentCharacterRotationEuler.x);				
            }

            AllNPCGroupOnMapList.Add(newTeam);
            return newTeam;
        }

        private List<CharacterGroup_NPCTeam> _internalNPCTeamSearchList = new List<CharacterGroup_NPCTeam>();
        public List<CharacterGroup_NPCTeam> FindAllNPCTeamBySearchCondition(
            NPCTeamSearchInfoInGEH info,
            DS_GameplayEventArguGroup dseg)
        {
            _internalNPCTeamSearchList.Clear();
            foreach (BaseCharacterGroup_NonBattle group in AllNPCGroupOnMapList)
            {
                if (group is not CharacterGroup_NPCTeam npcTeam)
                {
                    continue;
                }
                //如果需要使用自动配置，则dseg的obj1应当为那个baseArea
                if (info.UseAutoSelfAreaNameID)
                {
                    if (dseg.ObjectArgu1 is not EP_BaseArea areaRef)
                    {
                        DBug.LogError($"试图在搜索NPC时使用 【自动AreaID】，但是使用的DSEG的OBJ1不包含BaseArea引用。李在赣神魔");
                        return _internalNPCTeamSearchList;
                    }
                    if (!areaRef.AreaUID.Equals(npcTeam.BelongToAreaRef.AreaUID))
                    {
                        continue;
                    }
                }
                //没有使用自动配置，那就是指定ID
                else
                {
                    //如果不为空才需要匹配指定区域ID
                    if (!string.IsNullOrEmpty(info.SpecificAreaID))
                    {
                        if (!npcTeam.BelongToAreaRef.AreaUID.Equals(info.SpecificAreaID))
                        {
                            continue;
                        }
                    }
                }
                if (info.OnlyLeader)
                {
                    if (!npcTeam.IfNPCIsLeader(info.NPCNameID))
                    {
                        continue;
                    }
                }
                else
                {
                    if (!npcTeam.IfContainThisNameInGroup(info.NPCNameID))
                    {
                        continue;
                    }
                }
                _internalNPCTeamSearchList.Add(npcTeam);
            }
            return _internalNPCTeamSearchList;
        }
        // /// <summary>
        // /// <para>在指定位置生成按枚举的角色，以指定等级</para>
        // /// <para>会自动对齐到地形</para>
        // /// </summary>
        // public void SpawnCharacterOnPosition(CharacterNamedTypeEnum type, Vector3 position, int spawnAsLevel = 1)
        // {
        // 	if (_glmRef == null)
        // 	{
        // 		_glmRef = FindObjectOfType<SubGameplayLogicManager_ARPG>();
        // 	}
        //
        // 	GlobalConfigurationAssetHolderHelper gcahhRef;
        // 	if (!Application.isPlaying)
        // 	{
        // 		gcahhRef = Addressables.LoadAssetAsync<GameObject>("GCAHH").WaitForCompletion()
        // 			.GetComponent<GlobalConfigurationAssetHolderHelper>();
        // 	}
        // 	else
        // 	{
        // 		gcahhRef = GlobalConfigurationAssetHolderHelper.Instance;
        // 	}
        //
        // 	var prefab = gcahhRef.LUT_ARPGCharacterPrefabCollection.GetTargetPrefabByType(type);
        //
        // 	int typeInt = (int)type;
        //
        // 	Vector3 validPosition = _glmRef.GetAlignedTerrainPosition(position);
        // 	//小于五十万的是玩家，但玩家是在另一个地方生成的， 所以这种情况通常是不对的
        // 	if (typeInt < 500000)
        // 	{
        // 		// var newPlayer = Instantiate(prefab, validPosition, Quaternion.identity);
        // 		// var playerBehaviour = newPlayer.GetComponent<PlayerARPGConcreteCharacterBehaviour>();
        // 		//
        // 		// playerBehaviour.InitializeOnInstantiate();
        // 		//
        // 		//
        // 		//
        // 		// GlobalActionBus.GetGlobalActionBus().TriggerActionByType(
        // 		// 	ActionBus_ActionTypeEnum.G_CharacterOnMap_SpawnNewPlayerCharacter_生成了一个新的玩家角色,
        // 		// 	new DS_ActionBusArguGroup(
        // 		// 		ActionBus_ActionTypeEnum.G_CharacterOnMap_SpawnNewPlayerCharacter_生成了一个新的玩家角色,
        // 		// 		prefab));
        // 	}
        // 	else
        // 	{
        // 	}
        // }
        //
        //
        // public void SpawnCharacterOnPosition(CharacterNamedTypeEnum type, Vector2 positionXZ, int spawnAsLevel = 1)
        // {
        // 	SpawnCharacterOnPosition(type, new Vector3(positionXZ.x, 0f, positionXZ.y), spawnAsLevel);
        // }

        #endregion


        #region Tick

        public void UpdateTick(float ct, int cf, float delta)
		{
			_currentTime = ct;
			for (int i = CurrentAllActiveARPGCharacterBehaviourCollection.Count - 1; i >= 0; i--)
			{
				CurrentAllActiveARPGCharacterBehaviourCollection[i].UpdateTick(ct, cf, delta);
			}
			//死亡之后由于不再处于ActiveBehaviour之中了，但是动画依然要刷新，所以在Corpse容器中会只tickArtHelper
			foreach (NPCCorpseInfo perCorpse in _currentCorpseInfoCollection)
			{
				perCorpse.RelatedArtHelperRef.UpdateTick(ct, cf, delta);
			}


			for (int i = _currentCorpseInfoCollection.Count - 1; i >= 0; i--)
			{
				NPCCorpseInfo tmpCorpseInfo = _currentCorpseInfoCollection[i];
				tmpCorpseInfo.RemainingExistDuration -= delta;
				if (tmpCorpseInfo.RemainingExistDuration < 0f)
				{
					if (tmpCorpseInfo.RelatedBehaviourRef == null)
					{
						DBug.LogWarning($"尸体消失时，第{i}个尸体信息对应的behaviour是空的，检查一下。");
						continue;
					}
					OnNPCCorpseVanish(tmpCorpseInfo);
					BaseARPGCharacterBehaviour behaviourRef = tmpCorpseInfo.RelatedBehaviourRef;
					behaviourRef.ClearBeforeDestroy();
					UnityEngine.Pool.GenericPool<NPCCorpseInfo>.Release(tmpCorpseInfo);
					_currentCorpseInfoCollection.RemoveAt(i);

					DS_ActionBusArguGroup ds_destroy = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
						.G_CharacterOnMap_OnCharacterBehaviourPreDestroyed_当一个角色行为将要被销毁了);
					ds_destroy.ObjectArgu1 = behaviourRef;
					GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_destroy);
					Destroy(behaviourRef.gameObject);
				}
			}
            for (int i = AllNPCGroupOnMapList.Count - 1; i >= 0; i--)
            {
                var perGroup = AllNPCGroupOnMapList[i];
                if (perGroup is not CharacterGroup_PlayerTeam)
                {
                    perGroup.UpdateTick(ct, cf, delta);
                }
            }
        }

		public void FixedUpdateTick(float ct, int cf, float delta)
		{
			BaseAIListenComponent.FixedUpdateTime(ct, cf);

			for (int i = CurrentAllActiveARPGCharacterBehaviourCollection.Count - 1; i >= 0; i--)
			{
				var tmp = CurrentAllActiveARPGCharacterBehaviourCollection[i];
				if (tmp is PlayerARPGConcreteCharacterBehaviour)
				{
					continue;
				}
				tmp.FixedUpdateTick(ct, cf, delta);
			}
            //for (int i = AllNPCGroupOnMapList.Count - 1; i >= 0; i--)
            //{
            //    var perGroup = AllNPCGroupOnMapList[i];
            //    if (perGroup is not CharacterGroup_PlayerTeam)
            //    {
            //        perGroup.FixedUpdateTick(ct, cf, delta);
            //    }
            //}
        }

		public void GetRPBehaviourCollisionInfo(List<CollisionCheckInfo_RPBehaviourFull> result)
		{
			result.Clear();
			for (int i = CurrentAllActiveARPGCharacterBehaviourCollection.Count - 1; i >= 0; i--)
			{
				BaseARPGCharacterBehaviour behaviour = CurrentAllActiveARPGCharacterBehaviourCollection[i];
				if (!behaviour.gameObject.activeInHierarchy)
				{
					continue;
				}
				behaviour.FillCollisionInfoToList( ref result);
			}
		}

#endregion

#region 查询

		public int GetCharacterCountBySpecificType(CharacterNamedTypeEnum type)
		{
			int result = 0;
			foreach (BaseARPGCharacterBehaviour perBehaviour in CurrentAllActiveARPGCharacterBehaviourCollection)
			{
				if (perBehaviour.SelfBehaviourNamedType == type && perBehaviour.CharacterDataValid)
				{
					result++;
				}
			}

			return result;
		}

		/// <summary>
		/// <para>获取指定ID的角色 Behaviour引用。常用于玩家那边查询。如果是敌人，则会返回找到的第一个</para>
		/// </summary>
		public BaseARPGCharacterBehaviour GetCharacterByCharacterID(int id)
		{
			foreach (BaseARPGCharacterBehaviour perB in CurrentAllActiveARPGCharacterBehaviourCollection)
			{
				if ((int)perB.SelfBehaviourNamedType == id)
				{
					return perB;
				}
			}

			return null;
		}


		/// <summary>
		/// <para>获取当前所有的敌人列表。这是默认获取方式，会剔除GameObject不活跃的、不是Enemy的、数据已无效的</para>
		/// </summary>
		/// <param name="collection"></param>
		/// <returns></returns>
		public List<EnemyARPGCharacterBehaviour> GetAllEnemy(List<EnemyARPGCharacterBehaviour> collection = null)
		{
			List<EnemyARPGCharacterBehaviour> resultCollection =
				collection == null ? _internalQueryEnemyList : collection;
			resultCollection.Clear();
			foreach (BaseARPGCharacterBehaviour character in CurrentAllActiveARPGCharacterBehaviourCollection)
			{
				if (!character.gameObject.activeInHierarchy)
				{
					continue;
				}
				if (character is not EnemyARPGCharacterBehaviour enemy)
				{
					continue;
				}
				if (!character.CharacterDataValid)
				{
					continue;
				}
				resultCollection.Add(enemy);
			}

			return resultCollection;
		}

		/// <summary>
		/// <para>查询一定范围内的所有敌人，只需要在范围内就会塞进去</para>
		/// <para>注意使用的容器，如果没有传入调用者自备的容器，则会使用COM内的通用容器，这个容器只能立刻使用的，因为它是一个内部会循环使用的容器</para>
		/// </summary>
		public List<EnemyARPGCharacterBehaviour> GetEnemyListInRange(
			Vector3 fromPos,
			float maxDistance,
			bool withoutInvincible = true,
			List<EnemyARPGCharacterBehaviour> collection = null)
		{
			List<EnemyARPGCharacterBehaviour> resultCollection =
				collection == null ? _internalQueryEnemyList : collection;

			resultCollection.Clear();
			float distanceSq = maxDistance * maxDistance;
			foreach (BaseARPGCharacterBehaviour character in CurrentAllActiveARPGCharacterBehaviourCollection)
			{
				if (!character.gameObject.activeInHierarchy)
				{
					continue;
				}
				if (!character.CharacterDataValid)
				{
					continue;
				}
				if (character is EnemyARPGCharacterBehaviour enemy)
				{
					if (withoutInvincible)
					{
						if (enemy.ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum.Invincible_All_WD_完全无敌) ==
						    BuffAvailableType.Available_TimeInAndMeetRequirement ||
						    enemy.ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum
							    .InvincibleByDirector_All_WD_来自机制的完全无敌) ==
						    BuffAvailableType.Available_TimeInAndMeetRequirement)
						{
							continue;
						}
					}
					float tmpDistanceSq = Vector3.SqrMagnitude(fromPos - enemy.transform.position);
					if (tmpDistanceSq < distanceSq)
					{
						resultCollection.Add(enemy);
					}
				}
			}

			return resultCollection;
		}


		/// <summary>
		/// <para>查询一定范围内的所有敌人，按距离排序</para>
		/// <para>注意使用的容器，如果没有传入调用者自备的容器，则会使用COM内的通用容器，这个容器只能立刻使用的，因为它是一个内部会循环使用的容器</para>
		/// </summary>
		public List<EnemyARPGCharacterBehaviour> GetEnemyListInRangeSortByDistance(
			Vector3 fromPos,
			float maxDistance,
			bool withoutInvincible = true,
			List<EnemyARPGCharacterBehaviour> collection = null)
		{
			List<EnemyARPGCharacterBehaviour> resultCollection =
				collection == null ? _internalQueryEnemyList : collection;

			resultCollection = GetEnemyListInRange(fromPos, maxDistance, withoutInvincible, collection);

			resultCollection.Sort((a, b) =>
			{
				float aDistanceSq = Vector3.SqrMagnitude(fromPos - a.transform.position);
				float bDistanceSq = Vector3.SqrMagnitude(fromPos - b.transform.position);
				return aDistanceSq.CompareTo(bDistanceSq);
			});
			return resultCollection;
		}

		/// <summary>
		/// <para>随机选取一个敌人，会考虑忽略列表（黑名单）</para>
		/// </summary>
		/// <returns></returns>
		public EnemyARPGCharacterBehaviour GetRandomEnemy(List<CharacterNamedTypeEnum> ignoreList)
		{
			List<int> tmpList = CollectionPool<List<int>, int>.Get();
			tmpList.Clear();
			for (int i = 0; i < CurrentAllActiveARPGCharacterBehaviourCollection.Count; i++)
			{
				if (!CurrentAllActiveARPGCharacterBehaviourCollection[i].CharacterDataValid)
				{
					continue;
				}
				if (CurrentAllActiveARPGCharacterBehaviourCollection[i] is EnemyARPGCharacterBehaviour)
				{
					if (ignoreList.Contains(CurrentAllActiveARPGCharacterBehaviourCollection[i].SelfBehaviourNamedType))
					{
						continue;
					}
					tmpList.Add(i);
				}
			}
			tmpList.Shuffle();
			if (tmpList.Count > 0)
			{
				var tt = CurrentAllActiveARPGCharacterBehaviourCollection[tmpList[0]] as EnemyARPGCharacterBehaviour;
				CollectionPool<List<int>, int>.Release(tmpList);
				return tt;
				;
			}
			return null;
		}

		public EnemyARPGCharacterBehaviour GetRandomEnemy(bool includeVoidEntity = false)
		{
			List<int> tmpList = CollectionPool<List<int>, int>.Get();
			tmpList.Clear();
			for (int i = 0; i < CurrentAllActiveARPGCharacterBehaviourCollection.Count; i++)
			{
				if (!CurrentAllActiveARPGCharacterBehaviourCollection[i].CharacterDataValid)
				{
					continue;
				}
				if (CurrentAllActiveARPGCharacterBehaviourCollection[i] is EnemyARPGCharacterBehaviour)
				{
					if (!includeVoidEntity &&
					    CurrentAllActiveARPGCharacterBehaviourCollection[i].SelfBehaviourNamedType ==
					    CharacterNamedTypeEnum.Utility_VoidEntity_空实体)
					{
						continue;
					}
					tmpList.Add(i);
				}
			}
			tmpList.Shuffle();
			if (tmpList.Count > 0)
			{
				var tt = CurrentAllActiveARPGCharacterBehaviourCollection[tmpList[0]] as EnemyARPGCharacterBehaviour;
				CollectionPool<List<int>, int>.Release(tmpList);
				return tt;
				;
			}
			return null;
		}


		/// <summary>
		/// <para>根据一个搜索信息来获取相对应的角色的列表。</para>
		/// <para>返回的是内部的通用搜索列表。如果</para>
		/// </summary>
		public List<BaseARPGCharacterBehaviour> SearchTargetBehaviourByCharacterSearchInfo(
			CharacterSearchComponent info,
			List<BaseARPGCharacterBehaviour> collection = null)
		{
			List<BaseARPGCharacterBehaviour> resultCollection =
				collection == null ? _internalQueryCharacterList : collection;
			resultCollection.Clear();


			foreach (BaseARPGCharacterBehaviour perBaseBehaviour in CurrentAllActiveARPGCharacterBehaviourCollection)
			{
				if (!perBaseBehaviour.CharacterDataValid)
				{
					continue;
				}
				if (CheckIfBehaviourRequireSearchInfo(perBaseBehaviour, info))
				{
					resultCollection.Add(perBaseBehaviour);
				}
			}

			return resultCollection;
		}


		private bool CheckIfBehaviourRequireSearchInfo(
			BaseARPGCharacterBehaviour behaviour,
			CharacterSearchComponent info)
		{
			//对于 【不能包含】的，只要满足任何一个就算作不符合整体条件
			foreach (BaseCharacterSearchOption perNotContain in info.SearchOptions_NotContain)
			{
				if (perNotContain.IsCharacterMatched(behaviour))
				{
					return false;
				}
			}

			bool oneMatch = false;
			//对于【必须包含任一】的，如果全都没有包含  则 算作不符合整体条件
			if (info.SearchOptions_OneMatch.Count > 0)
			{
				foreach (BaseCharacterSearchOption perOneMatch in info.SearchOptions_OneMatch)
				{
					if (perOneMatch.IsCharacterMatched(behaviour))
					{
						oneMatch = true;
						break;
					}
				}
			}
			else
			{
				oneMatch = true;
			}
			if (!oneMatch)
			{
				return false;
			}


			//对于【必须全部符合】的，如果有任何一个不符合 ，则算作不符合整体条件

			foreach (BaseCharacterSearchOption perAllMatch in info.SearchOptions_AllMatch)
			{
				if (!perAllMatch.IsCharacterMatched(behaviour))
				{
					return false;
				}
			}
			return true;
		}


#region 通用查询

		protected List<BaseARPGCharacterBehaviour> _internalQueryBehaviourList = new List<BaseARPGCharacterBehaviour>();
		public List<BaseARPGCharacterBehaviour> GetBehaviourListInRange(
			Vector3 fromPos,
			float maxDistance,
			FactionTypeEnumFlag factionTypeEnumFlag,
			List<BaseARPGCharacterBehaviour> collection = null)
		{
			List<BaseARPGCharacterBehaviour> resultCollection =
				collection == null ? _internalQueryBehaviourList : collection;

			resultCollection.Clear();
			float distanceSq = maxDistance * maxDistance;
			foreach (BaseARPGCharacterBehaviour character in CurrentAllActiveARPGCharacterBehaviourCollection)
			{
				if (!character.gameObject.activeInHierarchy)
				{
					continue;
				}
				switch (character)
				{
					case EnemyARPGCharacterBehaviour enemyArpgCharacterBehaviour:
						if ((factionTypeEnumFlag & FactionTypeEnumFlag.Enemy_敌人) == 0)
						{
							continue;
						}
						break;
					case AllyARPGCharacterBehaviour allyArpgCharacterBehaviour:
						if ((factionTypeEnumFlag & FactionTypeEnumFlag.PlayerAlly_玩家的友军) == 0)
						{
							continue;
						}
						break;
					case PlayerARPGConcreteCharacterBehaviour playerArpgConcreteCharacterBehaviour:
						if ((factionTypeEnumFlag & FactionTypeEnumFlag.Player_玩家角色) == 0)
						{
							continue;
						}
						break;
				}
				float tmpDistanceSq = Vector3.SqrMagnitude(fromPos - character.transform.position);
				if (tmpDistanceSq < distanceSq)
				{
					resultCollection.Add(character);
				}
			}

			return resultCollection;
		}

#endregion

#endregion

#region 死亡 、 销毁  、 回收

		/// <summary>
		/// <para>直接死亡，跳过了尸体的过程。移除并销毁。</para>
		/// </summary>
		/// <param name="ds"></param>
		private void _ABC_OnNPCBehaviourDieDirect(DS_ActionBusArguGroup ds)
		{
			var targetBehaviour = ds.ObjectArgu1 as BaseARPGCharacterBehaviour;
			;


			if (CurrentAllActiveARPGCharacterBehaviourCollection.Contains((targetBehaviour)))
			{
				CurrentAllActiveARPGCharacterBehaviourCollection.Remove(targetBehaviour);
			}
			else
			{
				DBug.LogError($"角色死亡时，{targetBehaviour.name}不在当前活跃角色列表中，这不应该发生。");
				return;
			}

			DS_ActionBusArguGroup ds_destroy = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
				.G_CharacterOnMap_OnCharacterBehaviourPreDestroyed_当一个角色行为将要被销毁了);
			ds_destroy.ObjectArgu1 = targetBehaviour;
			GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_destroy);
			targetBehaviour.ClearBeforeDestroy();
			Destroy(targetBehaviour.gameObject);
		}

		private void _ABC_OnNPCBehaviourDeathToCorpse(DS_ActionBusArguGroup ds)
		{
			var targetBehaviour = ds.ObjectArgu1 as BaseARPGCharacterBehaviour;


			if (_currentCorpseInfoCollection.Exists((info => targetBehaviour == info.RelatedBehaviourRef)))
			{
				return;
			}


			NPCCorpseInfo newCorpseInfo = UnityEngine.Pool.GenericPool<NPCCorpseInfo>.Get();
			newCorpseInfo.Reset();

			newCorpseInfo.RelatedBehaviourRef = targetBehaviour;
			newCorpseInfo.RelatedArtHelperRef = targetBehaviour.GetRelatedArtHelper();
			Float_RPDataEntry corpseEntry =
				targetBehaviour.GetFloatDataEntryByType(RP_DataEntry_EnumType.CorpseDuration_尸体存在时间);
			float corpseDuration = 3f;
			if (corpseEntry != null)
			{
				corpseDuration = corpseEntry.CurrentValue;
			}

			newCorpseInfo.RemainingExistDuration = corpseDuration;
			newCorpseInfo.BeginTime = _currentTime;

			if (CurrentAllActiveARPGCharacterBehaviourCollection.Contains((targetBehaviour)))
			{
				CurrentAllActiveARPGCharacterBehaviourCollection.Remove(targetBehaviour);
			}

			_currentCorpseInfoCollection.Add(newCorpseInfo);
		}


		/// <summary>
		/// <para>当某个NPC尸体需要消失了！</para>
		/// </summary>
		private void OnNPCCorpseVanish(NPCCorpseInfo corpseInfo)
		{
			DS_ActionBusArguGroup ds_vanish = new DS_ActionBusArguGroup();
			if (corpseInfo.RelatedBehaviourRef is EnemyARPGCharacterBehaviour)
			{
				ds_vanish = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
					.G_CharacterOnMap_OnEnemyCorpseVanish_敌人尸体消失);
			}
			else if (corpseInfo.RelatedBehaviourRef is AllyARPGCharacterBehaviour)
			{
				ds_vanish = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
					.G_CharacterOnMap_OnAllyCorpseVanish_友军尸体消失);
			}

			ds_vanish.ObjectArgu1 = corpseInfo.RelatedBehaviourRef;

			GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_vanish);
		}

#endregion

#region 对于首领敌人的RenderObject 的RenderFeature设置

		private RenderObjects _rf_renderObjectsRef;
		private EnemyARPGCharacterBehaviour _bossRef;


		private void _ABC_TryEnableRenderObjects(DS_ActionBusArguGroup ds)
		{
			if (ds.ObjectArgu1 is not EnemyARPGCharacterBehaviour enemy)
			{
				return;
			}
			if (enemy.ApplyDamage_CheckTargetBuff(RolePlay_BuffTypeEnum._EnemyTag_BossEnemy_首领敌人) ==
			    BuffAvailableType.NotExist)
			{
				return;
			}
			_bossRef = enemy;
			_rf_renderObjectsRef?.SetActive(true);
		}

		private void _ABC_TryDisableRenderObjects(DS_ActionBusArguGroup ds)
		{
			if (ds.ObjectArgu1 is not EnemyARPGCharacterBehaviour enemy)
			{
				return;
			}
			if (!ReferenceEquals(enemy, _bossRef))
			{
				return;
			}
			_bossRef = null;
			_rf_renderObjectsRef?.SetActive(false);
		}

#endregion

		public void ClearOnUnload()
		{
			foreach (BaseARPGCharacterBehaviour perBe in CurrentAllActiveARPGCharacterBehaviourCollection)
			{
				if (perBe != null)
				{
					Destroy(perBe.gameObject);
				}
			}
		}
	}
}