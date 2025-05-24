using System;
using System.Collections.Generic;
using System.Linq;
using ARPG.Character;
using ARPG.Character.Base;
using ARPG.Character.Enemy;
using ARPG.Manager;
using Global;
using Global.ActionBus;
using Global.Setting;
using RPGCore;
using RPGCore.Buff;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
namespace ARPG.DropItem
{
	[Serializable]
	[CreateAssetMenu(fileName = "__一个掉落物控制", menuName = "#SO Assets#/#战斗关卡配置#/掉落物控制")]
	public class SOConfig_DropItemControl : ScriptableObject
	{

		[SerializeField, LabelText("所有关联的掉落物配置们")]
		public List<PerDropItemConfig> DropItemConfigList;


		[NonSerialized]
		public SOConfig_DropItemControl RawTemplate;
		

		private static ConSer_MiscSettingInSO _settingRef;


		public class PerTypeDropItemInfoRuntimeGroup
		{
			public string DropTypeID;
			public PerDropItemConfig SelfRawConfigTemplate;
			public List<DropItemRuntimeInfo> SelfFreeItemList = new List<DropItemRuntimeInfo>();
			public List<DropItemRuntimeInfo> SelfBusyItemList = new List<DropItemRuntimeInfo>();


			public void Initialize(PerDropItemConfig configTemplate)
			{
				DropTypeID = configTemplate.DropItemConfigID;
				SelfRawConfigTemplate = configTemplate;
			}

			public void UpdateTick(float ct, int cf, float delta)
			{
				var player = SubGameplayLogicManager_ARPG.Instance.PlayerCharacterBehaviourControllerReference
					.CurrentControllingBehaviour;

				var playerPos = player.transform.position;
				playerPos.y = 0f;
				if (cf % _settingRef?._UF_DropItemPlayerAbsorbingPickupCheck == 0)
				{
					for (int i = SelfBusyItemList.Count - 1; i >= 0; i--)
					{
						DropItemRuntimeInfo tmpInfo = SelfBusyItemList[i];

						Vector3 selfPos = tmpInfo.RelatedGO.transform.position;
						selfPos.y = 0f;
						if (ct < tmpInfo.AbleToPickTime)
						{
							continue;
						}
						if (Vector3.SqrMagnitude(selfPos - playerPos) <
						    tmpInfo.CurrentAbsorbingRadius * tmpInfo.CurrentAbsorbingRadius)
						{
							tmpInfo.CurrentAbsorbing = true;
						}
						else
						{
							tmpInfo.CurrentAbsorbing = false;
						}
					}
				}

				foreach (DropItemRuntimeInfo perItem in SelfBusyItemList)
				{
					if (perItem.CurrentAbsorbing)
					{
						if (ct < perItem.AbleToPickTime)
						{
							continue;
						}
						SelfRawConfigTemplate.Picking.UpdateTick(perItem.RelatedGO, player.transform, ct, cf, delta);
					}
				}

				if (cf % _settingRef?._UF_DropItemPlayerPickupCheck == 0)
				{
					for (int i = SelfBusyItemList.Count - 1; i >= 0; i--)
					{
						DropItemRuntimeInfo tmpInfo = SelfBusyItemList[i];

						Vector3 selfPos = tmpInfo.RelatedGO.transform.position;
						selfPos.y = 0f;
						if (ct < tmpInfo.AbleToPickTime)
						{
							continue;
						}
						if (Vector3.SqrMagnitude(selfPos  - playerPos) <
						    tmpInfo.CurrentPickupRadius * tmpInfo.CurrentPickupRadius)
						{
							DS_ActionBusArguGroup ds_picked = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
								.G_DropItem_OnPickedUpDropItem_活动ARPG掉落物_当拾取到一个掉落物);
							ds_picked.ObjectArgu1 = tmpInfo;
							GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_picked);
							tmpInfo.TypeInfoRef.ProcessOnPickup(player);
							tmpInfo.TypeInfoRef.SelfFreeItemList.Add(tmpInfo);
							tmpInfo.TypeInfoRef.SelfBusyItemList.RemoveAt(i);
							tmpInfo.RelatedGO.SetActive(false);
							tmpInfo.CurrentActive = false;
							continue;
						}
					}
				}
			}
			public void ProcessOnPickup(PlayerARPGConcreteCharacterBehaviour player)
			{
				foreach (BaseDropItemPickedEffect perEffect in SelfRawConfigTemplate.PickupEffectList)
				{
					perEffect.OnPickup(player, SelfRawConfigTemplate);
				}
			}

			public DropItemRuntimeInfo GetNewFreeItem()
			{
				if (SelfFreeItemList.Count > 0)
				{
					DropItemRuntimeInfo item = SelfFreeItemList[SelfFreeItemList.Count - 1];
					item.CurrentActive = true;
					item.RelatedGO.SetActive(true);

					SelfBusyItemList.Add(item);
					SelfFreeItemList.RemoveAt(SelfFreeItemList.Count - 1);
					return item;
				}
				else
				{
					return CreateNewItem();
				}
			}

			public DropItemRuntimeInfo CreateNewItem()
			{
				DropItemRuntimeInfo tmpNew = new DropItemRuntimeInfo();
				tmpNew.CurrentActive = true;

				tmpNew.RelatedGO = Object.Instantiate(SelfRawConfigTemplate._prefab_Item);

				tmpNew.TypeInfoRef = this;
				SelfBusyItemList.Add(tmpNew);
				return tmpNew;
			}

		}
		
		
		
		
		public class DropItemRuntimeInfo
		{
			public bool CurrentActive;
			public bool CurrentAbsorbing = false;
			public PerTypeDropItemInfoRuntimeGroup TypeInfoRef;
			public GameObject RelatedGO;
			//能够被拾取的时间点
			public float AbleToPickTime;
			public float RemainingTime;
			public float CurrentSize;
			public float CurrentPickupRadius;
			public float CurrentAbsorbingRadius;

			public void ProcessSpawnComponent(BaseARPGCharacterBehaviour behaviour)
			{
				TypeInfoRef.SelfRawConfigTemplate.SpawnComponent.OnSpawnDropItem(RelatedGO, behaviour);
				CurrentAbsorbing = false;
			}
			
			
			
			

		}
		public Dictionary<string, PerTypeDropItemInfoRuntimeGroup> DropItemRuntimeInfoDict =
			new Dictionary<string, PerTypeDropItemInfoRuntimeGroup>();



		private DropItemRuntimeInfo GetFreeDropItemRuntimeInfo(string typeID)
		{
			_settingRef = GlobalConfigurationAssetHolderHelper.Instance.MiscSetting_Runtime.SettingContent;
			if (!DropItemRuntimeInfoDict.ContainsKey(typeID))
			{
				var newTypeInfo = new PerTypeDropItemInfoRuntimeGroup();
				var ff = DropItemConfigList.FindIndex((per) => (per.DropItemConfigID.Equals(typeID)));
				if (ff == -1)
				{
					throw  new Exception($"DropItemConfigList中没有找到对应的类型{typeID}");
				}
				newTypeInfo.Initialize(DropItemConfigList[ff]);
				DropItemRuntimeInfoDict.Add(typeID, newTypeInfo);
			}
			var infoGroup = DropItemRuntimeInfoDict[typeID];

			return infoGroup.GetNewFreeItem();
		}

		/// <summary>
		/// <para>直接要求进行掉落，会掉落这个配置下的所有掉落配置。通常来源于  根据配置进行掉落 的事件，</para>
		/// </summary>
		public void DirectDrop(BaseARPGCharacterBehaviour behaviourRef)
		{
			foreach (PerDropItemConfig perDropItemConfig in DropItemConfigList)
			{
				DropItemRuntimeInfo dropItem = GetFreeDropItemRuntimeInfo(perDropItemConfig.DropItemConfigID);
				if (dropItem != null)
				{
					ProcessConcreteDrop(dropItem, behaviourRef, perDropItemConfig);
				}
			}
		}


		public void InitializeOnSetToCurrentControl()
		{
			GlobalActionBus.GetGlobalActionBus()
				.RegisterAction(ActionBus_ActionTypeEnum.G_EnemyBehaviour_OnEnemyDieToCorpse_敌人死亡到尸体,
					_ABC_OnEnemyDieToCorpse);
			GlobalActionBus.GetGlobalActionBus().RegisterAction(ActionBus_ActionTypeEnum.G_CharacterBehaviour_RequireDropItemReaction_角色行为要求掉落物反应,
				_ABC_OnRequireDropItemReaction
				);
			
			
		
		}

		private void _ABC_OnRequireDropItemReaction(DS_ActionBusArguGroup ds)
		{
			var str = ds.ObjectArguStr as string;
			var obj = ds.ObjectArgu1 as BaseARPGCharacterBehaviour;
			foreach (PerDropItemConfig perDIC in DropItemConfigList)
			{
				foreach (DropComponent_DropCondition perC in perDIC.DropConditions)
				{
					if (perC is DropWithDropRequirementActionBusTriggered withAB)
					{
						if(withAB!=null && withAB.ListenEventArgus.Contains(str))
						{
							DropItemRuntimeInfo dropItem = null;

							dropItem = GetFreeDropItemRuntimeInfo(perDIC.DropItemConfigID);

							if (dropItem != null)
							{
								ProcessConcreteDrop(dropItem, obj, perDIC);
							}
						}
					}
				}
			}

		}
		

		private void _ABC_OnEnemyDieToCorpse(DS_ActionBusArguGroup ds)
		{
			//内部使用的敌人种类，0是普攻小怪，1是精英怪，2是Boss
			int enemyTypeIndex = 0;
			var targetBehaviour = ds.ObjectArgu1 as EnemyARPGCharacterBehaviour;

		
			
			foreach (PerDropItemConfig perDropItemConfig in DropItemConfigList)
			{
				foreach (DropComponent_DropCondition perDropCondition in perDropItemConfig.DropConditions)
				{
					perDropCondition.InitializeOnLoad();
					switch (perDropCondition)
					{
						case MayDropOnEnemyWithBuffKilled mayDropWithBuffKilled:
							DropItemRuntimeInfo dropItem = null;
							
							bool buffCheckPass = true;
							foreach (RolePlay_BuffTypeEnum perBuff in mayDropWithBuffKilled.NeedHaveAllBuffArray)
							{
								if (targetBehaviour.ReceiveBuff_CheckTargetBuff(perBuff) !=
								    BuffAvailableType.Available_TimeInAndMeetRequirement)
								{
									buffCheckPass = false;
									break;
								}
							}
							if (!buffCheckPass)
							{
								continue;
							}
							
							
							if (mayDropWithBuffKilled.DropAsChange)
							{
								if (Random.Range(0, 100) < mayDropWithBuffKilled.DropChance)
								{
									dropItem =GetFreeDropItemRuntimeInfo(perDropItemConfig.DropItemConfigID);
								}
							}
							else
							{
								mayDropWithBuffKilled.RemainingInterval -= 1;
								if (mayDropWithBuffKilled.RemainingInterval <= 0)
								{
									mayDropWithBuffKilled.RemainingInterval = mayDropWithBuffKilled.DropInterval;
									dropItem = GetFreeDropItemRuntimeInfo(perDropItemConfig.DropItemConfigID);
								}
							}
							
							
							if (dropItem != null)
							{
								ProcessConcreteDrop(dropItem, targetBehaviour, perDropItemConfig);
							}
							
							break;
					}
				}
			}
			
		}


		private void ProcessConcreteDrop(DropItemRuntimeInfo dropItem, BaseARPGCharacterBehaviour behaviour,
			PerDropItemConfig perDropItemConfig)
		{

			PerTypeDropItemInfoRuntimeGroup relatedInfoGroup =
				DropItemRuntimeInfoDict[perDropItemConfig.DropItemConfigID];

			dropItem.RelatedGO.SetActive(true);
			dropItem.CurrentActive = true;
			dropItem.RelatedGO.transform.position = behaviour.transform.position;
			dropItem.ProcessSpawnComponent(behaviour);
			dropItem.RemainingTime = relatedInfoGroup.SelfRawConfigTemplate.DropItemExistDuration;
			dropItem.AbleToPickTime = BaseGameReferenceService.CurrentFixedTime + perDropItemConfig.SpawnPickupTime;
			dropItem.CurrentSize = Random.Range(relatedInfoGroup.SelfRawConfigTemplate.SizeRange.x,
				relatedInfoGroup.SelfRawConfigTemplate.SizeRange.y);
			dropItem.RelatedGO.transform.localScale = Vector3.one * dropItem.CurrentSize;
			dropItem.CurrentPickupRadius = relatedInfoGroup.SelfRawConfigTemplate.FinalPickupDistance;
			dropItem.CurrentAbsorbingRadius = relatedInfoGroup.SelfRawConfigTemplate.BeginAbsorbingRadiusMul;

		}

		public void UpdateTick(float ct, int cf, float delta)
		{
			foreach (PerTypeDropItemInfoRuntimeGroup perType in DropItemRuntimeInfoDict.Values)
			{
				perType.UpdateTick(ct, cf, delta);
			}
		}


		public void ClearBeforeRemove()
		{
			GlobalActionBus.GetGlobalActionBus()
				.RemoveAction(ActionBus_ActionTypeEnum.G_EnemyBehaviour_OnEnemyDieToCorpse_敌人死亡到尸体,
					_ABC_OnEnemyDieToCorpse);

			foreach (var kvp in DropItemRuntimeInfoDict)
			{
				foreach (DropItemRuntimeInfo perItem in kvp.Value.SelfBusyItemList)
				{
					Destroy(perItem.RelatedGO);
				}
				kvp.Value.SelfBusyItemList.Clear();
				foreach (DropItemRuntimeInfo perItem in kvp.Value.SelfFreeItemList)
				{
					Destroy(perItem.RelatedGO);
				}
				kvp.Value.SelfFreeItemList.Clear();
			}
			DropItemRuntimeInfoDict.Clear();
		}

	}
}