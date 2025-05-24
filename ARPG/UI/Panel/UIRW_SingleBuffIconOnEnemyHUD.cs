using ARPG.Equipment;
using ARPG.UI.Panel.EnemyTopHUD;
using Global.UIBase;
using RPGCore.Buff;
using RPGCore.Buff.ConcreteBuff.Common;
using RPGCore.Buff.ConcreteBuff.Element;
using RPGCore.Buff.ConcreteBuff.Element.Second;
using RPGCore.Buff.Config;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace ARPG.UI.Panel
{
	/// <summary>
	/// <para>一个用在敌人HUD上面的Buff图表。会循环利用。</para>
	/// </summary>
	public class UIRW_SingleBuffIconOnEnemyHUD : UI_UISingleRuntimeWidget
	{

		[SerializeField, LabelText("image_图标"), Required, TitleGroup("===Widget===")]
		private SpriteRenderer image_BuffIcon;
		
		[LabelText("sprite_遮罩sprite") ,SerializeField,Required,TitleGroup("===Widget===")]
		private SpriteRenderer sprite_MaskSprite;
		
		[LabelText("SpriteMask_遮罩图标"),SerializeField,Required,TitleGroup("===Widget===")]
		private SpriteMask spriteMask_BuffIcon;

		[LabelText("text_层数文本") ,SerializeField,Required,TitleGroup("===Widget===")]
 		private TextMeshPro text_StackCountText;		
		


		private UIRW_EnemyAboveHUDItem _parentRef;


		[ShowInInspector]
		public I_BuffContentMayDisplayOnUI SelfTargetBuffInterfaceRef { get; private set; }


		public void SetTargetBuff(UIRW_EnemyAboveHUDItem parent, I_BuffContentMayDisplayOnUI buff)
		{
			gameObject.SetActive(true);
			
			_parentRef = parent;
			SelfTargetBuffInterfaceRef = buff;
			image_BuffIcon.sprite = buff.RelatedBuffDisplayOnUIInfo.IconSprite;
			spriteMask_BuffIcon.sprite = buff.RelatedBuffDisplayOnUIInfo.IconSprite;

			sprite_MaskSprite.material.SetFloat("_FillUp", SelfTargetBuffInterfaceRef.GetRemainingPartial());

			text_StackCountText.gameObject.SetActive((false));
			
			if (buff is I_BuffAsElementAmplification amp)
			{
				text_StackCountText.gameObject.SetActive(true);
				text_StackCountText.text = amp.GetCurrentStack().ToString();
			}
		}


		public void RefreshRemainingPartial()
		{
			if (SelfTargetBuffInterfaceRef != null)
			{
				sprite_MaskSprite.material.SetFloat("_FillUp", 1f-SelfTargetBuffInterfaceRef.GetRemainingPartial());
			}
		}
		
		
		
		
		
	}
}