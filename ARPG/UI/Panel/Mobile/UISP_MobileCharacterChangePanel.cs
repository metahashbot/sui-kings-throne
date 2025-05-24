using System.Collections.Generic;
using ARPG.Character;
using ARPG.Manager;
using Global;
using Global.ActionBus;
using Global.UIBase;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
namespace ARPG.UI.Panel.Mobile
{
	public class UISP_MobileCharacterChangePanel : UI_UIBaseSubPanel
	{
		[SerializeField, Required, AssetsOnly, LabelText("UIRW_候选")]
		private GameObject _uirw_CandidateEntry;

		[SerializeField, LabelText("Layout_布局"), Required]
		private LayoutGroup _layout_CandidateLayoutGroup;

		
		private List<UIRW_CharacterInCandidate> _uirw_CandidateList = new List<UIRW_CharacterInCandidate>();



		public override void StartInitializeBySP(UI_UIBasePanel parentUIP)
		{
			base.StartInitializeBySP(parentUIP);

			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_PC_OnTeamMemberChanged_队伍成员更换,
				_ABC_RefreshCharacterCandidate_OnCurrentActiveCharacterChanged);
		}
		public override void UpdateTickByParentPanel(float currentTime, int currentFrame, float delta)
		{
			base.UpdateTickByParentPanel(currentTime, currentFrame, delta);
		}


		private void _ABC_RefreshCharacterCandidate_OnCurrentActiveCharacterChanged(DS_ActionBusArguGroup ds)
		{
			// foreach (UIRW_CharacterInCandidate perUIRW in _uirw_CandidateList)
			// {
			// 	Destroy(perUIRW.gameObject);
			// }
			// _uirw_CandidateList.Clear();
			//
			//
			// var pcRef = SubGameplayLogicManager_ARPG.Instance.PlayerCharacterBehaviourControllerReference;
			// int index = 0;
			// foreach (PlayerARPGConcreteCharacterBehaviour pc in pcRef.CurrentAllCharacterBehaviourList)
			// {
			// 	if (pc == pcRef.CurrentControllingBehaviour)
			// 	{
			// 		continue;
			// 	}
			// 	index += 1;
			// 	UIRW_CharacterInCandidate newUIRW = Instantiate(_uirw_CandidateEntry, _layout_CandidateLayoutGroup.transform)
			// 		.GetComponent<UIRW_CharacterInCandidate>();
			// 	newUIRW.gameObject.SetActive(true);
			// 	_uirw_CandidateList.Add(newUIRW);
			// 	newUIRW.Initialize(pc, index);
			//
			// }






		}

	}
}