using Global.UIBase;
using RPGCore.Buff;
using RPGCore.Buff.ConcreteBuff.Common;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.UI.Panel.EnemyTopHUD
{
	public class UIRW_EnemyBuffHUDEntry_EnemyTopHUD : UI_UISingleRuntimeWidget
	{

		[SerializeField, LabelText("image_图标"), Required, TitleGroup("===Widget===")]
		private UnityEngine.UI.Image image_BuffIcon;
		
		
		 
		private UIRW_PerEliteTopHUD_EnemyTopHUDPanel _parentRef;


		public I_BuffContentMayDisplayOnUI SelfTargetBuffInterfaceRef { get; private set; }


		public void SetTargetBuff(I_BuffContentMayDisplayOnUI buff)
		{
			SelfTargetBuffInterfaceRef = buff;
			image_BuffIcon.sprite = buff.RelatedBuffDisplayOnUIInfo.IconSprite;
		}
	}
}