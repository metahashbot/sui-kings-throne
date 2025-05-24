using System;
using System.Collections.Generic;
using Global;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using RPGCore.UtilityDataStructure.BuffLogicPassingComponent;
using Sirenix.OdinInspector;
namespace RPGCore.Buff.ConcreteBuff.Common
{
	[Serializable]
	[InfoBox("内置监听[G_CharacterOnMap_SpawnNewEnemy_生成了一个新敌人] + [G_CharacterOnMap_SpawnNewPlayerCharacter_生成了一个新的玩家角色]" +
	         "+[G_CharacterOnMap_OnCharacterBehaviourPreDestroyed_当一个角色行为将要被销毁了]，每次对此进行SearchInfo检查")]
	public class Buff_根据角色搜索结果调整Buff_ModifyBuffByCharacterSearchInfo : BaseRPBuff
	{
		public class ModifyInfo
		{
			public CharacterSearchComponent SearchInfo;
			public List<GeneralBuffModifyInfo> ModifyInfos;
		}


		public List<ModifyInfo> SearchInfos;
		
		

		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			var gab = GlobalActionBus.GetGlobalActionBus();
			gab.RegisterAction(ActionBus_ActionTypeEnum.G_CharacterOnMap_SpawnNewEnemy_生成了一个新敌人,
				_ABC_ProcessSearchInfo);
			gab.RegisterAction(ActionBus_ActionTypeEnum.G_CharacterOnMap_SpawnNewPlayerCharacter_生成了一个新的玩家角色,
				_ABC_ProcessSearchInfo);
			gab.RegisterAction(ActionBus_ActionTypeEnum.G_CharacterOnMap_OnCharacterBehaviourPreDestroyed_当一个角色行为将要被销毁了,
				_ABC_ProcessSearchInfo);
			
		}




		private void _ABC_ProcessSearchInfo(DS_ActionBusArguGroup ds)
		{
			
		}


		protected override void ClearAndUnload()
		{
			base.ClearAndUnload();
			var gab = GlobalActionBus.GetGlobalActionBus();
			gab.RemoveAction(ActionBus_ActionTypeEnum.G_CharacterOnMap_SpawnNewEnemy_生成了一个新敌人,
				_ABC_ProcessSearchInfo);
			gab.RemoveAction(ActionBus_ActionTypeEnum.G_CharacterOnMap_SpawnNewPlayerCharacter_生成了一个新的玩家角色,
				_ABC_ProcessSearchInfo);
			gab.RemoveAction(ActionBus_ActionTypeEnum.G_CharacterOnMap_OnCharacterBehaviourPreDestroyed_当一个角色行为将要被销毁了,
				_ABC_ProcessSearchInfo);
		}
	}
}