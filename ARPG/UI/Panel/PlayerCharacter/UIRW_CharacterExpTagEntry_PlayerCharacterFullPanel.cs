using Global;
using Global.Loot;
using Global.UIBase;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
namespace ARPG.UI.Panel.PlayerCharacter
{
	public class UIRW_CharacterExpTagEntry_PlayerCharacterFullPanel : UI_UISingleRuntimeWidget
	{


		[LabelText("image_阅历图标 "), SerializeField, Required, TitleGroup("===Widget===")]
		private UnityEngine.UI.Image _image_CharacterExpIcon;
		

		public SOFE_CharacterExpCollection.PerTypeInfo RelatedExpInfoRef { get; private set; }

		private UnityAction<string, BaseEventData> _callback_OnPointerEnter;

		private UnityAction _callback_OnPointerExit;



		public void InstantiateInitialize(
			string expName,
			UnityAction<string, BaseEventData> callback_OnPointerEnter,
			UnityAction callback_OnPointerExit)
		{
			RelatedExpInfoRef = GlobalConfigurationAssetHolderHelper.GetGCAHH().FE_CharacterExpTagInfo
				.GetExpInfoByTag(expName);



			_callback_OnPointerEnter = callback_OnPointerEnter;
			_callback_OnPointerExit = callback_OnPointerExit;


			_image_CharacterExpIcon.sprite = GlobalConfigurationAssetHolderHelper.GetGCAHH().FE_CharacterExpTagInfo
				.GetSpriteByTag(expName);
		}



		public void _ET_PointerEnter(BaseEventData baseEventData)
		{
			_callback_OnPointerEnter.Invoke(RelatedExpInfoRef.CharacterEXPTag, baseEventData);
		}

		public void _ET_PointerExit(BaseEventData baseEventData)
		{
			_callback_OnPointerExit.Invoke();
		}



	}
}