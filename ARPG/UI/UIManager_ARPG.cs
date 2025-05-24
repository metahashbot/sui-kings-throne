using ARPG.Manager;
using ARPG.UI.Panel.PlayerCharacter;
using Global.UI;
using UnityEngine;

namespace ARPG.UI
{
	public class UIManager_ARPG : BaseUIManager
	{
		public static UIManager_ARPG Instance;

		public static Material UIDefaultMaterial;

		private UIP_PlayerCharacterFullPanel _playerCharacterFullPanelRef;



		public override void AwakeInitialize()
		{
			Instance = this;

			base.AwakeInitialize();
		}


		public override void StartInitialize()
		{
			base.StartInitialize();
			_playerCharacterFullPanelRef = GetPanel<UIP_PlayerCharacterFullPanel>();

		}


		protected override UnityEngine.Camera GetCurrentUICamera()
		{
			return GameReferenceService_ARPG.Instance.CameraBehaviourRef.UICamera;
		}
		public override void UpdateTick(float ct, int cf, float delta)
		{
			base.UpdateTick(ct, cf, delta); 
			if (Input.GetKeyDown(KeyCode.I) && !_playerCharacterFullPanelRef.IsPanelCurrentSelfActive)
			{
				_playerCharacterFullPanelRef.ShowThisPanel();
			}

		}
	}
}