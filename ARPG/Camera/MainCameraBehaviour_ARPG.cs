using ARPG.Character;
using ARPG.Manager;
using Cinemachine;
using Global;
using Global.ActionBus;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Camera
{
	public class MainCameraBehaviour_ARPG : BaseMainCameraBehaviour
	{

#region 外部引用


		private InputAction_ARPG _inputActionRef;


#endregion

		public override void AwakeInitialize()
		{
			base.AwakeInitialize();
			SelfFramingTransposerRef = VirtualCameraRef.GetCinemachineComponent<CinemachineFramingTransposer>();
		}
		public override void StartInitialize()
		{
			base.StartInitialize();
			
			
			var gab = GlobalActionBus.GetGlobalActionBus();
			gab.RegisterAction(ActionBus_ActionTypeEnum.G_PC_OnCurrentUsingCharacterChanged_当前使用的角色更换,
				_ABC_ChangeCurrentFocusTransformToCharacter_OnCurrentUsingCharacterChanged);


			GameReferenceService_ARPG.Instance.InputActionInstance.BattleGeneral.ScrollUp.performed +=
				_IC_ProcessScroll;

		}
		public override Vector3 GetCurrentFocusOnTerrainPosition()
		{

			return SubGameplayLogicManager_ARPG.Instance.GetAlignedTerrainPosition(
				new Ray(MainCamera.transform.position, MainCamera.transform.forward));
		}


#region TICK

#endregion


#region 事件绑定

		private void _ABC_ChangeCurrentFocusTransformToCharacter_OnCurrentUsingCharacterChanged(DS_ActionBusArguGroup ds)
		{
			var targetBehaviour = ds.ObjectArgu1 as PlayerARPGConcreteCharacterBehaviour;
			if (ds.IntArgu1.HasValue)
			{
				SelfFramingTransposerRef.m_XDamping = 0f;
				SelfFramingTransposerRef.m_YDamping = 0f;
				SelfFramingTransposerRef.m_ZDamping = 0f;
				GameReferenceService_ARPG.TimedTaskManagerInstance.AddTimeTask(((arg0, f) =>
					{
						_ResetCameraDamping();
					}),
					0.1f);

			}
			CameraTargetTransform.SetParent(targetBehaviour.GetSelfRolePlayArtHelper().transform);
			CameraTargetTransform.transform.localPosition = Vector3.zero;

		}


		private void _ResetCameraDamping()
		{
			SelfFramingTransposerRef.m_XDamping = 1f;
			SelfFramingTransposerRef.m_YDamping = 1f;
			SelfFramingTransposerRef.m_ZDamping = 1f;
		}

		 
		

#endregion

		protected override void OnDestroy()
		{
			base.OnDestroy();
			GameReferenceService_ARPG.Instance.InputActionInstance.BattleGeneral.ScrollUp.performed -=
				_IC_ProcessScroll;
		}
	}
}