using System;
using System.Collections.Generic;
using ARPG.Common;
using ARPG.Manager;
using Sirenix.OdinInspector;
using UnityEngine;
using Global;
using Global.ActionBus;
using Global.GlobalConfig;
using RPGCore.Buff;
using RPGCore.Skill;
using UnityEngine.InputSystem;
namespace ARPG.Character.Player
{
	/// <summary>
	/// <para>ARPG中玩家控制器。内部控制多个具体的玩家角色</para>
	/// <para>所有角色通用的数据都会在这里存。比如输入信息（直接 & 逻辑）</para>
	/// </summary>
	public class PlayerCharacterBehaviourController : BasePlayerCharacterController
	{

#if UNITY_EDITOR
		// redraw constantly
		[OnInspectorGUI]
		private void RedrawConstantly() { Sirenix.Utilities.Editor.GUIHelper.RequestRepaint(); }
#endif
#region 外部引用

		private SubGameplayLogicManager_ARPG _glmRef;

		private InputAction_ARPG _inputActionRef;
		private GlobalConfigSO.ContentInGCSO _gcsoRef;

		private EditorProxy_ARPGEditor _arpgEditorProxy;
		private UnityEngine.Camera _mainCameraRef;

#endregion

#region 内部字段


	[ShowInInspector, LabelText("维持的所有玩家角色行为"), FoldoutGroup("运行时")]
		public List<PlayerARPGConcreteCharacterBehaviour> CurrentAllCharacterBehaviourList;

		private PlayerARPGConcreteCharacterBehaviour GetPlayerBehaviourAtSlot(int index)
		{
			if (index == 0)
			{
				return CurrentControllingBehaviour;
			}
			else
			{
				int findIndex = CurrentAllCharacterBehaviourList.FindIndex((behaviour => behaviour.CurrentPlayerSlot == index));
				if (findIndex == -1)
				{
					return null;
				}
				else
				{
				return	CurrentAllCharacterBehaviourList[findIndex];
				}
			}
		}

		[ShowInInspector, LabelText("--当前直接操作的玩家角色--"), FoldoutGroup("运行时", true)]
		public PlayerARPGConcreteCharacterBehaviour CurrentControllingBehaviour;

#endregion


#region 初始化

		public void AwakeInitialize(SubGameplayLogicManager_ARPG glm)
		{
			CurrentAllCharacterBehaviourList = new List<PlayerARPGConcreteCharacterBehaviour>();


			_gcsoRef = GlobalConfigSO.RuntimeContent();


			_inputActionRef = GameReferenceService_ARPG.Instance.InputActionInstance;
			

			
			
			_arpgEditorProxy = FindObjectOfType<EditorProxy_ARPGEditor>();
			BasePlayerAnimationEventCallback._pcbRef = this;
		}




		/// <summary>
		/// <para>注册一个新的角色Behaviour到玩家控制器来，通常是加载的时候</para>
		/// </summary>
		public void RegisterNewCharacterBehaviour(PlayerARPGConcreteCharacterBehaviour behaviourRef)
		{
			if (CurrentAllCharacterBehaviourList.Contains(behaviourRef))
			{
				DBug.LogError($"玩家控制器中重复添加了角色{behaviourRef.SelfBehaviourNamedType}");
				return;
			}
			CurrentAllCharacterBehaviourList.Add(behaviourRef);
		}


		public void StartInitialize()
		{
			_glmRef = SubGameplayLogicManager_ARPG.Instance;
			PlayerARPGConcreteCharacterBehaviour.StaticInitialize(_inputActionRef);
			Input_AimSpeed = GlobalConfigurationAssetHolderHelper.Instance.MiscSetting_Runtime.SettingContent
				.BattleGameRightJoystickSpeed;
			_mainCameraRef = GameReferenceService_ARPG.Instance.CameraBehaviourRef.MainCamera;
			BindingEvents();
			_inputActionRef.BattleGeneral.ChangeCharacterOnSlot1.performed += _IC_TryChangeToSlot1;
			_inputActionRef.BattleGeneral.ChangeCharacterOnSlot2.performed += _IC_TryChangeToSlot2;
			_inputActionRef.BattleGeneral.ChangeCharacterOnSlot3.performed += _IC_TryChangeToSlot3;

			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_Player_OnPlayerCharacterExitBattle_当玩家角色退场,
				_ABC_AutoSwitchToNextPlayer_OnPlayerCharacterExitBattle);

			GlobalActionBus.GetGlobalActionBus()
				.RegisterAction(ActionBus_ActionTypeEnum.Dev_GoToTeleportPosition_开发中进行传送, _ABC_DevTeleport);




		}


		private void BindingEvents()
		{
		}

		public void LateLoadInitialize()
		{


		}

		private void OnDestroy()
		{
			_inputActionRef.BattleGeneral.ChangeCharacterOnSlot1.performed -= _IC_TryChangeToSlot1;
			_inputActionRef.BattleGeneral.ChangeCharacterOnSlot2.performed -= _IC_TryChangeToSlot2;
			_inputActionRef.BattleGeneral.ChangeCharacterOnSlot3.performed -= _IC_TryChangeToSlot3;
		}

#endregion


#region Tick
		public override void UpdateTick(float currentTime, int currentFrame, float delta)
		{
			base.UpdateTick( currentTime,  currentFrame,  delta);
			RefreshInputInfo(_mainCameraRef,
				_inputActionRef.BattleGeneral.Move,
				_inputActionRef.BattleGeneral.Aim,
				delta);
		}

		
		

		public override void FixedUpdateTick(float currentTime, int currentFrame, float delta)
		{
			base.FixedUpdateTick(currentTime, currentFrame, delta);
			foreach (var perBehaviour in CurrentAllCharacterBehaviourList)
			{
				perBehaviour.FixedUpdateTick(currentTime, currentFrame, delta);
			}


		}

#endregion

#region 输入响应

		private void _IC_TryChangeToSlot1(InputAction.CallbackContext context)
		{
			//不能换当前的
			if (CurrentControllingBehaviour.CurrentPlayerSlot == 1)
			{
				return;
			}


			var behaviour = GetPlayerBehaviourAtSlot(1);


			if (behaviour == null)
			{
				return;
			}
			if (CheckIfCanChangePlayerCharacter(behaviour))
			{
				ProcessChangePlayer(behaviour);
			}

		}




		private void _IC_TryChangeToSlot2(InputAction.CallbackContext context)
		{
			//不能换当前的
			if (CurrentControllingBehaviour.CurrentPlayerSlot == 2)
			{
				return;
			}
			PlayerARPGConcreteCharacterBehaviour behaviour = GetPlayerBehaviourAtSlot(2);
			if (behaviour == null)
			{
				return;
			}
			if (CheckIfCanChangePlayerCharacter(behaviour))
			{
				ProcessChangePlayer(behaviour);
			}
		}

		private void _IC_TryChangeToSlot3(InputAction.CallbackContext context)
		{
			//不能换当前的
			if (CurrentControllingBehaviour.CurrentPlayerSlot == 3)
			{
				return;
			}
			PlayerARPGConcreteCharacterBehaviour behaviour = GetPlayerBehaviourAtSlot(3);
			if (behaviour == null)
			{
				return;
			}
			if (CheckIfCanChangePlayerCharacter(behaviour))
			{
				ProcessChangePlayer(behaviour);
			}
		}

		private void ProcessChangePlayer(PlayerARPGConcreteCharacterBehaviour newBehaviour)
		{
			newBehaviour.transform.position = CurrentControllingBehaviour.transform.position;
			if (CurrentControllingBehaviour.CurrentState ==
			    PlayerARPGConcreteCharacterBehaviour.PlayerCharacterStateTypeEnum.Using_当前使用)
			{
				CurrentControllingBehaviour.MakeCharacterStateTransition(PlayerARPGConcreteCharacterBehaviour
					.PlayerCharacterTransitionTypeEnum.ChangeDown_换下这个角色);
			}
			
			newBehaviour.MakeCharacterStateTransition(PlayerARPGConcreteCharacterBehaviour
				.PlayerCharacterTransitionTypeEnum.ChangeUp_换出这个角色);
			CurrentControllingBehaviour.ProcessBuffTransferOnChangeCurrentBehaviour(newBehaviour);
			
			CurrentControllingBehaviour = newBehaviour;
			
			
			//检查是不是需要立刻释放连协技能
			var skillRef = newBehaviour.GetRelatedSkillHolder().GetSkillOnSlot(SkillSlotTypeEnum.ChainSkill_连协槽位);

			if (skillRef != null)
			{
				if (skillRef.ConcreteSkillFunction.GetSkillReadyType() == SkillReadyTypeEnum.Ready)
				{
					skillRef.ConcreteSkillFunction._Internal_TryPrepareSkill();
				}
			}
		
			
			GlobalActionBus.GetGlobalActionBus()
				.TriggerActionByType(ActionBus_ActionTypeEnum.G_PC_OnTeamMemberChanged_队伍成员更换);
		}
		
		
		
		/// <summary>
		/// <para>检查具体能不能更换到这个槽位上的角色</para>
		/// <para>需要还活着，需要没有退场CD(更换阻塞)，连携技能CD已好</para>
		/// <para>检查连协：如果拥有连携技(且不是死亡退场)，则如果连协技能没好，也不能切换</para>
		/// </summary>
		private bool CheckIfCanChangePlayerCharacter(PlayerARPGConcreteCharacterBehaviour changeTo , bool exitAsDie = false , bool checkChain = true)
		{
			//如果是死亡退场，那不管换出的人连协好不好，都是直接释放
			if (exitAsDie)
			{
				if(changeTo.CurrentState == PlayerARPGConcreteCharacterBehaviour.PlayerCharacterStateTypeEnum.ExitBattle_已退场)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			
			
			//不是死亡退场，那正常检查
			//需要不处于CD并且没有退场
			if (changeTo.CurrentState == PlayerARPGConcreteCharacterBehaviour.PlayerCharacterStateTypeEnum.UnusableCD_不可使用换下CD ||
			     changeTo.CurrentState == PlayerARPGConcreteCharacterBehaviour.PlayerCharacterStateTypeEnum.ExitBattle_已退场)
			{
				return false;
			}
			//拿一下阻塞换人的buff
			var blockBuff_target = changeTo.ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum.ARPG_PlayerSwitchCharacterBlock_玩家更换角色阻塞);
			//如果是有效，则目前不能换人
			if (blockBuff_target == BuffAvailableType.Available_TimeInAndMeetRequirement)
			{
				return false;
			}
			
			//发起方也不能有这个buff
			// var blockBuff_from =
			// 	CurrentControllingBehaviour.ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum
			// 		.ARPG_PlayerSwitchCharacterBlock_玩家更换角色阻塞);
			// if (blockBuff_from == BuffAvailableType.Available_TimeInAndMeetRequirement)
			// {
			// 	return false;
			// }

			if (checkChain)
			{
				//检查连携技能
				var chainSkillRef = changeTo.GetRelatedSkillHolder().GetSkillOnSlot(SkillSlotTypeEnum.ChainSkill_连协槽位);
				if (chainSkillRef != null)
				{
					//释放技能不在这个函数里面，要Process那个地方
					if (chainSkillRef.ConcreteSkillFunction.GetSkillReadyType() != SkillReadyTypeEnum.Ready)
					{
						return false;
					}
					else
					{
						return true;
					}
				}
			}
			
			//超时或被阻塞则可以换人
			if (!CurrentControllingBehaviour.TryOccupyByOccupationInfo(CurrentControllingBehaviour.CurrentActiveWeaponConfigRuntimeInstance.WeaponFunction
				._Cache_ANInfo_SwitchEnter.OccupationInfo,true,FixedOccupiedCancelTypeEnum.Logic_SwitchCharacter_机制换人断))
			{
				return false;
			}
			return true;
		}


		private List<PlayerARPGConcreteCharacterBehaviour.PlayerCharacterStateTypeEnum> _playerCharacterStateList =
			new List<PlayerARPGConcreteCharacterBehaviour.PlayerCharacterStateTypeEnum>();

		private void RefreshPlayerCharacterStateList()
		{
			_playerCharacterStateList.Clear();
			for (int i = 0; i < CurrentAllCharacterBehaviourList.Count; i++)
			{
				PlayerARPGConcreteCharacterBehaviour tmp = CurrentAllCharacterBehaviourList[i];
				_playerCharacterStateList.Add(tmp.CurrentState);
			}
		}
		
		
		/// <summary>
		/// <para>当玩家角色退场的时候，自动切换到下一个角色</para>
		/// </summary>
		private void _ABC_AutoSwitchToNextPlayer_OnPlayerCharacterExitBattle(DS_ActionBusArguGroup ds)
		{ 
			PlayerARPGConcreteCharacterBehaviour exitBehaviour = ds.ObjectArgu1 as PlayerARPGConcreteCharacterBehaviour;
			RefreshPlayerCharacterStateList();
			int exitCharacterIndex = CurrentAllCharacterBehaviourList.FindIndex((behaviour => behaviour == exitBehaviour));
			int currentIndex = exitCharacterIndex + 1;
			int behaviourListCapacity = CurrentAllCharacterBehaviourList.Count;
			int trySwitchCount = 0;
			
			//如果所有角色都退场了，那么就直接GameOver
			if (_playerCharacterStateList.TrueForAll((state => state == PlayerARPGConcreteCharacterBehaviour.PlayerCharacterStateTypeEnum.ExitBattle_已退场)))
			{
				var ds_allExit =
					new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_Player_OnPlayerAllCharacterExited_当玩家所有角色退场);
				GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_allExit);
				return;
			}
			if (currentIndex >= behaviourListCapacity)
			{
				currentIndex = 0;
			}
			
			//试图换 当前角色数量减1次。
			//这是 正常尝试，检查连协。会优先换出来连协已经不在CD的
			while (trySwitchCount < (behaviourListCapacity -1))
			{
				var newBehaviour = CurrentAllCharacterBehaviourList[currentIndex];
				if (CheckIfCanChangePlayerCharacter(newBehaviour, true,true))
				{
					ProcessChangePlayer(newBehaviour);
					return;
				}
				
				if(currentIndex >= behaviourListCapacity)
				{
					currentIndex = 0;
				}
				trySwitchCount += 1;
			}
			trySwitchCount = 0;
			currentIndex = exitCharacterIndex + 1;
			if (currentIndex >= behaviourListCapacity)
			{
				currentIndex = 0;
			}
			while (trySwitchCount < (behaviourListCapacity - 1))
			{
				var newBehaviour = CurrentAllCharacterBehaviourList[currentIndex];
				if (CheckIfCanChangePlayerCharacter(newBehaviour, true, false))
				{
					ProcessChangePlayer(newBehaviour);
					return;
				}

				if (currentIndex >= behaviourListCapacity)
				{
					currentIndex = 0;
				}
				trySwitchCount += 1;
			}
			var ds_allExit2 =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_Player_OnPlayerAllCharacterExited_当玩家所有角色退场);
			GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_allExit2);
			


		}
		

#endregion
#region LocalActionBus响应

		
#endregion
#region 提供外部

		public PlayerARPGConcreteCharacterBehaviour GetBehaviourRef(int cid)
		{
			
			int findIndex = CurrentAllCharacterBehaviourList.FindIndex((behaviour => behaviour.SelfCharacterID == cid));
			if (findIndex == -1)
			{
				return null;
			}
			else
			{
				return CurrentAllCharacterBehaviourList[findIndex];
			}
		}
	
		public PlayerARPGConcreteCharacterBehaviour GetBehaviourRef(CharacterNamedTypeEnum type)
		{
			return GetBehaviourRef((int)type);
		}
		
		private class AimPlayerInfoPair
		{
			public PlayerARPGConcreteCharacterBehaviour CurrentPlayerBehaviour;
			public Vector3 PlayerPosition;
			public Vector3 ToPlayerDirectionNormalized;
		}

		private AimPlayerInfoPair AimPlayerInfoPairInstance = new AimPlayerInfoPair();


		public (Vector3, Vector3) GetAimPlayerOfPlayerPosition(Vector3 casterFromPosition,int offset)
		{
			AimPlayerInfoPairInstance.CurrentPlayerBehaviour = CurrentControllingBehaviour;
			if (offset != 0)
			{
				AimPlayerInfoPairInstance.PlayerPosition =
					CurrentControllingBehaviour.GetPositionRecordByFrameInterval(offset);
			}
			else
			{
				AimPlayerInfoPairInstance.PlayerPosition = CurrentControllingBehaviour.transform.position;

			}
			
			AimPlayerInfoPairInstance.ToPlayerDirectionNormalized =(casterFromPosition - CurrentControllingBehaviour.transform.position).normalized;
			
			
			DS_ActionBusArguGroup ds =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_Utility_GetAimToPlayerInfo_试图获取瞄准玩家的信息);
			ds.ObjectArgu1 = AimPlayerInfoPairInstance;
			
			CurrentControllingBehaviour.GetRelatedActionBus().TriggerActionByType(ActionBus_ActionTypeEnum
					.L_Utility_GetAimToPlayerInfo_试图获取瞄准玩家的信息,
				ds);
			
			
			return (AimPlayerInfoPairInstance.PlayerPosition, AimPlayerInfoPairInstance.ToPlayerDirectionNormalized);
		}

		public Func<Vector3> GetCurrentActivePlayerPositionFunc()
		{
			return GetCurrentActivePlayerPosition;
		}
		
		public Vector3 GetCurrentActivePlayerPosition()
		{
			return CurrentControllingBehaviour.GetCollisionCenter();
		}
		
#endregion


		protected override BaseGameplayLogicManager GetGLMRef()
		{
			return _glmRef;
		}
		protected override Vector3 GetCurrentActiveCharacterPosition()
		{
			return CurrentControllingBehaviour.transform.position;
		}
		protected override Vector2 GetCurrentActiveCharacterFaceDirection()
		{
			return CurrentControllingBehaviour.ReleaseSkill_GetRelatedArtHelper().CurrentFaceLeft
				? new Vector2(BaseGameReferenceService.CurrentBattleLogicLeftDirection.x,
					BaseGameReferenceService.CurrentBattleLogicLeftDirection.z) : new Vector2(
					BaseGameReferenceService.CurrentBattleLogicRightDirection.x,
					BaseGameReferenceService.CurrentBattleLogicRightDirection.z);
		}


		protected void _ABC_DevTeleport(DS_ActionBusArguGroup ds)
		{
			var teleport = ds.GetObj1AsT<Transform>();
			if (teleport != null)
			{
				foreach (PlayerARPGConcreteCharacterBehaviour perBehaviour in CurrentAllCharacterBehaviourList)
				{
					perBehaviour.transform.position = teleport.position;
				}
			}
		}
	}
}