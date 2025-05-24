using System;
using Global;
using Global.ActionBus;
using Global.UIBase;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace ARPG.UI.Panel.PlayerCharacter
{
	public class UIRW_CharacterInTeamInsidePlayerCharacterFullPanel : UI_UISingleRuntimeWidget
	{
		[ShowInInspector, LabelText("该UIRW关联的角色")]
		public int RelatedCharacterID { get; protected set; }

		[SerializeField, LabelText("button_本体按键"), Required]
		private Button _button_selfButton;


		[SerializeField, LabelText("image_角色明亮"), Required]
		public Image _image_characterSelfImage;


		[SerializeField, LabelText("image_高亮边框"), Required]
		private Image _image_highlightBorder;


		[SerializeField, LabelText("LE_本体布局"), Required]
		public LayoutElement _le_selfLayoutElement;


		private UnityAction<int> _callback_ClickSelfButton;



		public void InitializeOnInstantiate(int characterID, UnityAction<int> callback_ClickSelfButton)
		{
			_callback_ClickSelfButton = callback_ClickSelfButton;
			_button_selfButton.onClick.RemoveAllListeners();
			_button_selfButton.onClick.AddListener(_Button_CheckIfChangeSelectCharacter_OnSelfButtonClicked);
			_image_highlightBorder.gameObject.SetActive(false);
			RelatedCharacterID = characterID;
		}


		/// <summary>
		/// <para>当自身按钮被点击时，检查是否需要广播更换选择了新的队伍内角色。因为有可能的情况是点的和当前显示的是同一个，这时候无事发生</para>
		/// </summary>
		private void _Button_CheckIfChangeSelectCharacter_OnSelfButtonClicked()
		{
			_callback_ClickSelfButton.Invoke(RelatedCharacterID);


			_image_highlightBorder.gameObject.SetActive(true);







			var ds = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
				.G_UI_General_RequireGeneralInteractVFX_UI要求通用交互VFX);
			ds.ObjectArgu1 = transform;
			GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds);
		}

        public void SelectThisUIRW()
        {
            _image_highlightBorder.gameObject.SetActive(true);
        }

        public void DeselectThisUIRW()
		{
			_image_highlightBorder.gameObject.SetActive(false);
		}






		public void ClearBeforeDestroy()
		{
		}
	}
}