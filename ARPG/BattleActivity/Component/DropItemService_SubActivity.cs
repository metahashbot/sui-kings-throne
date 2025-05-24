using System;
using System.Collections.Generic;
using ARPG.Character.Base;
using ARPG.DropItem;
using Global.ActionBus;
using Global;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;
using ARPG.Character.Enemy;
using ARPG.UI.Panel.BattleConclusion;
using Global.GlobalConfig;
namespace ARPG.Manager.Component
{
	[TypeInfoBox("掉落物管理")]
	[Serializable]
	public class DropItemService_SubActivity : BaseSubActivityService
	{

		[LabelText("当前活跃着的掉落物配置")]
		public List<SOConfig_DropItemControl> CurrentActiveDropItemControlInstance =
			new List<SOConfig_DropItemControl>();

		/// <summary>
		/// 掉落系统初始化，绑定事件
		/// </summary>
		public void AwakeInitialize()
		{
            GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_EnemyBehaviour_OnEnemyDieToCorpse_敌人死亡到尸体,
                DropItemInTmpBack);
        }

		private void DropItemInTmpBack(DS_ActionBusArguGroup ds)
		{
			var enemy = ds.ObjectArgu1 as EnemyARPGCharacterBehaviour;
			if (enemy != null)
			{
				int enemyUID = (int)enemy.SelfBehaviourNamedType;
				var dropConfig = GCAHHExtend.GetDropConfig(
					Global.Loot.SOFE_DropGroupInfo.DropSourceEnum.Monster_怪物掉落, enemyUID);

				if (dropConfig == null)
				{
					return;
				}

				var dropItems = dropConfig.GetDropItem();
                foreach (var itemPair in dropItems)
                {
					int uid = itemPair.Key;
					int count = itemPair.Value;
					if(GCAHHExtend.IsWeapon(uid) || GCAHHExtend.IsArmor(uid))
                    {
                        GCSOExtend.AddTmpEquipment(itemPair.Key);
                    }
					else
					{
						GCSOExtend.AddTmpItem(uid, count);
					}
                }
            }
        }


        public SOConfig_DropItemControl InitializeDropItemControl(SOConfig_DropItemControl dropItemControl)
		{
			if (CurrentActiveDropItemControlInstance.Exists((control => control.RawTemplate == dropItemControl)))
			{
				Debug.LogWarning($"在加入新的掉落物配置中，出现了重复的添加：{dropItemControl.name}，已跳过这个添加");
				return CurrentActiveDropItemControlInstance.Find((control => control.RawTemplate == dropItemControl));
			}
			
			var newInstance = Object.Instantiate(dropItemControl);
			newInstance.RawTemplate = dropItemControl;
			CurrentActiveDropItemControlInstance.Add(newInstance);
			newInstance.InitializeOnSetToCurrentControl();
			return newInstance;
		}


		/// <summary>
		/// 通过一个单独的掉落物配置，要求直接掉落。内部会检查这个掉落配置是否已经注册过了，如果没有注册会注册一下，然后使用已经初始化过的配置进行直接掉落
		/// </summary>
		public void ProcessDropBySingleConfig(
			SOConfig_DropItemControl configDropItemControl,
			BaseARPGCharacterBehaviour behaviourRef)
		{
			SOConfig_DropItemControl initializedConfig = null;
			for (int i = 0; i < CurrentActiveDropItemControlInstance.Count; i++)
			{
				if (CurrentActiveDropItemControlInstance[i].RawTemplate == configDropItemControl)
				{
					//已经注册过了，跳出，直接下方掉落
					initializedConfig = CurrentActiveDropItemControlInstance[i];
					break;
				}
			}
			//还是空的，则说明这是第一次要求这个配置进行掉落，之前没有过，那就在此对其初始化
			if (initializedConfig == null)
			{
				initializedConfig = InitializeDropItemControl(configDropItemControl);
			}

			initializedConfig.DirectDrop(behaviourRef);
		}
		
		
		public override void UpdateTick(float ct, int cf, float delta)
		{
			foreach (SOConfig_DropItemControl perDC in CurrentActiveDropItemControlInstance)
			{
				perDC?.UpdateTick(ct, cf, delta);
			}
		}
	}
}