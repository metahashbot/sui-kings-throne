using System.Collections.Generic;
using ARPG.Character.Enemy;
using ARPG.UI.Panel.EnemyTopHUD;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Global;
using Global.ActionBus;
using Global.UIBase;
using RPGCore.Buff;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine.UI;

namespace ARPG.UI.Panel
{
	public class UIP_EnemyTopHUDPanel : UI_UIBasePanel
	{
		[LabelText("uirw_Boss的血条UIRW"), SerializeField, Required, TitleGroup("===Widget===")]
		private UIRW_PerEliteTopHUD_EnemyTopHUDPanel uirw_BossHPUIRW;

		
		
		[LabelText("Prefab-单个敌人Buff状态Icon") ,SerializeField,Required,TitleGroup("===Prefab===")]
		 public GameObject prefab_EnemyBuffIcon;




		public override void StartInitializeByUIM()
		{
			base.StartInitializeByUIM();
			
			uirw_BossHPUIRW.gameObject.SetActive(false);
			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_CharacterOnMap_SpawnNewEnemy_生成了一个新敌人,
				_ABC_ProcessNewEnemySpawn_OnSpawnNewEnemy,
				300);
		}

		public override void UpdateTick(float currentTime, int currentFrameCount, float deltaTime)
		{
			base.UpdateTick(currentTime, currentFrameCount, deltaTime);
			uirw_BossHPUIRW.UpdateTick( currentTime,  currentFrameCount,  deltaTime);
		
		}

#region 处理敌人生成

		private void _ABC_ProcessNewEnemySpawn_OnSpawnNewEnemy(DS_ActionBusArguGroup ds)
		{
			var enemyBehaviour = ds.ObjectArgu1 as EnemyARPGCharacterBehaviour;
			//这是个Boss，就放到boss血条上
			if (enemyBehaviour.ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum._EnemyTag_BossEnemy_首领敌人) !=
			    BuffAvailableType.NotExist)
			{
				uirw_BossHPUIRW.RefreshRelatedBehaviour(enemyBehaviour, this);
			}
		}


#endregion
		

	}
}