using System;
using System.Collections.Generic;
using ARPG.Character.Enemy;
using ARPG.Manager.Component;
using Global;
using Global.ActionBus;
using RPGCore.Buff;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Manager.Config
{
	[Serializable]
	[TypeInfoBox("每消灭一个敌人将会累加计数并再次生成那个敌人，达到计数点后不会再生成新的敌人\n" +
	             "注意，此处的【不会再生成】只是对于这个SECT来说的，如果此时整个区域还活跃着其他SECT，仍然有可能继续生成，那些生成与这个SECT是无关的\n" +
	             "可以为每个具体敌人指定消灭计数，在附加值内选 _指定消灭计数_即可")]
	public class SECT_计数并补充_CountAndSupplySpawnEnemyConfigType : BaseSpawnEnemyConfigTypeHandler
	{
		public enum SupplyMethodTypeEnum
		{
			OnlySameConfig_仅相同配置 = 1,
			AssignedConfig_指定的配置 = 2,
			InsideSECT_在该SECT内的任意配置 = 3,
		}


		[LabelText("视作完成的计数点"), SerializeField, FoldoutGroup("配置", true)]
		public int FinishCountPoint = 100;

		[SerializeField, FoldoutGroup("配置", true), LabelText("普通敌人计数点")]
		public int GeneralNormalEnemyCountPoint = 10;

		[SerializeField, FoldoutGroup("配置", true), LabelText("精英敌人计数点")]
		public int GeneralEliteEnemyCountPoint = 20;

		[SerializeField, FoldoutGroup("配置", true), LabelText("首领敌人计数点")]
		public int GeneralBossEnemyCountPoint = 100;
        
		[NonSerialized, LabelText("当前计数点"), ShowInInspector, ReadOnly]
		public int CurrentCountPoint = 0;

		[LabelText("补充时的类型"), SerializeField, FoldoutGroup("配置", true)]
		public SupplyMethodTypeEnum SupplyMethodType = SupplyMethodTypeEnum.OnlySameConfig_仅相同配置;


		[LabelText("指定的配置们"),SerializeField,FoldoutGroup("配置",true),ShowIf("@this.SupplyMethodType ==SupplyMethodTypeEnum.AssignedConfig_指定的配置")]
		[InlineEditor(InlineEditorObjectFieldModes.Boxed)]
		public List<SOConfig_PerEnemyTypeSpawnConfig> SupplyConfigOnAssigned;
		
		[LabelText("包含对生成位置信息的覆盖？"), SerializeField, FoldoutGroup("配置", true)]
		public bool IncludeSpawnPositionOverride = false;

		[ShowIf(nameof(IncludeSpawnPositionOverride)), FoldoutGroup("配置", true), SerializeField]
		public List<SimpleSpawnPositionInfo> SpawnPositionInfoOverrides;
		
		
		[NonSerialized]
		public List<SOConfig_SpawnEnemyConfig.PerEnemyTypeInfo> _relatedSingleSpawnInfoListRuntime = new List<SOConfig_SpawnEnemyConfig.PerEnemyTypeInfo>();
		
		 
		
		

		public override void InitializeOnInstantiate(SOConfig_SpawnEnemyConfig configRef, SOConfig_SpawnEnemyConfig rawTemplate,string areaID)
		{
			base.InitializeOnInstantiate(configRef, rawTemplate, areaID);

			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_CharacterOnMap_OnEnemyCorpseVanish_敌人尸体消失,
				_ABC_CheckIfNeedReSpawn_OnEnemyCorpseVanish);

			if (SupplyConfigOnAssigned != null && SupplyConfigOnAssigned.Count > 0)
			{
				foreach (SOConfig_PerEnemyTypeSpawnConfig perSO in SupplyConfigOnAssigned)
				{
					foreach (SOConfig_SpawnEnemyConfig.PerEnemyTypeInfo perETI in perSO.EnemyTypeInfoList)
					{
						_relatedSingleSpawnInfoListRuntime.Add(perETI);
					}
				}
			}



		}
		protected override void  CheckIfSpawnHandlerFinished()
		{
			if (CurrentCountPoint >= FinishCountPoint && _relatedSingleSpawnInfoListRef.Count == 0)
			{
				RelatedSpawnConfigRuntimeRef.ConfigFinished = true;
			}
		}

		private void _ABC_CheckIfNeedReSpawn_OnEnemyCorpseVanish(DS_ActionBusArguGroup ds)
		{
			CheckIfSpawnHandlerFinished();
			if (ds.ObjectArgu1 is not EnemyARPGCharacterBehaviour enemy)
			{
				return;
			}
			if (CurrentCountPoint >= FinishCountPoint)
			{
				return;
			}


			SOConfig_SpawnEnemyConfig relatedSpawnConfig = enemy.RelatedSpawnConfigInstance;
			//就是来自自己的生成配置实例
			if (!System.Object.ReferenceEquals(relatedSpawnConfig, RelatedSpawnConfigRuntimeRef))
			{
				return;
			}
			
			
			
			//计数，然后看能不能生成
			if (enemy.ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum._EnemyTag_NormalEnemy_普通敌人) !=
			    BuffAvailableType.NotExist)
			{
				CurrentCountPoint += GeneralNormalEnemyCountPoint;
			}
			else if (enemy.ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum._EnemyTag_EliteEnemy_精英敌人) !=
			         BuffAvailableType.NotExist)
			{
				if (enemy.ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum._EnemyTag_BySplit_分裂产生的敌人) == BuffAvailableType.NotExist)
				{
					CurrentCountPoint += GeneralEliteEnemyCountPoint;
				}
			}
			else if (enemy.ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum._EnemyTag_BossEnemy_首领敌人) != BuffAvailableType.NotExist)
			{
				CurrentCountPoint += GeneralBossEnemyCountPoint;
			}



			SimpleSpawnPositionInfo spawnPositionOverride = null;
			
			if (IncludeSpawnPositionOverride)
			{
				if (SpawnPositionInfoOverrides == null || SpawnPositionInfoOverrides.Count == 0)
				{
					DBug.LogError( $"SECT_计数并补充_CountAndSupplySpawnEnemyConfigType {RelatedSpawnConfigRuntimeRef.name} 没有配置覆盖，但是勾选了IncludeSpawnPositionOverride");
					return;
				}
				spawnPositionOverride = SpawnPositionInfoOverrides[UnityEngine.Random.Range(0, SpawnPositionInfoOverrides.Count)];
			}


			switch (SupplyMethodType)
			{
				case SupplyMethodTypeEnum.OnlySameConfig_仅相同配置:
					EnemySpawnService_SubActivityService.SingleSpawnInfo singleInfo =
						enemy.RelatedSpawnConfigInstance_SingleSpawnInfo;
					AddNewSingleSpawnInfoToListByPerEnemyTypeInfo(singleInfo.RawSpawnInfo, 1, spawnPositionOverride);
					break;
				case SupplyMethodTypeEnum.AssignedConfig_指定的配置:
					if (SupplyConfigOnAssigned == null || SupplyConfigOnAssigned.Count == 0)
					{
						DBug.LogError($"SECT_计数并补充_CountAndSupplySpawnEnemyConfigType {RelatedSpawnConfigRuntimeRef.name} 没有配置指定的配置，但是生成类型为AssignedConfig_指定的配置");
						return;
					}
					var perEnemyTypeSpawnConfig = _relatedSingleSpawnInfoListRuntime[UnityEngine.Random.Range(0,
						_relatedSingleSpawnInfoListRuntime.Count)];
					AddNewSingleSpawnInfoToListByPerEnemyTypeInfo(perEnemyTypeSpawnConfig, 1, spawnPositionOverride);

					break;
				case SupplyMethodTypeEnum.InsideSECT_在该SECT内的任意配置:
					var randomInSECT = EnemySpawnCollection_RuntimeFinal[UnityEngine.Random.Range(0,
						EnemySpawnCollection_RuntimeFinal.Count)];
					AddNewSingleSpawnInfoToListByPerEnemyTypeInfo(randomInSECT, 1, spawnPositionOverride);
					break;
			}
			
		}
		
		
		
		public override void ClearBeforeDestroy()
		{
			base.ClearBeforeDestroy();

			GlobalActionBus.GetGlobalActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.G_CharacterOnMap_OnEnemyCorpseVanish_敌人尸体消失,
				_ABC_CheckIfNeedReSpawn_OnEnemyCorpseVanish);

		}


	}
}