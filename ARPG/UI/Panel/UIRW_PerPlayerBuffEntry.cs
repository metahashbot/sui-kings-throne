using System;
using ARPG.Character;
using DG.Tweening;
using Global;
using Global.UIBase;
using RPGCore.Buff;
using RPGCore.Buff.ConcreteBuff;
using RPGCore.Buff.ConcreteBuff.Common;
using RPGCore.Buff.ConcreteBuff.Element;
using RPGCore.Buff.ConcreteBuff.Element.Second;
using RPGCore.Buff.Config;
using RPGCore.Skill;
using RPGCore.Skill.Config;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ARPG.UI.Panel
{
	public class UIRW_PerPlayerBuffEntry : UI_UISingleRuntimeWidget
	{

#region Widget

		[SerializeField, Required, LabelText("Image_Icon本体"),TitleGroup("===Widget===")]
		private Image _image_IconSelf;

		[SerializeField, Required, LabelText("Image_本体底板"), TitleGroup("===Widget===")]
		private Image _image_SelfBackgroundImage;
		
		[SerializeField,Required,LabelText("Image_层数底板"),TitleGroup("===Widget===")]
		 private Image _image_StackBaseImage;


		[SerializeField, Required, LabelText("text_层数文本"), TitleGroup("===Widget===")]
		private TextMeshProUGUI _text_StackText;

		[SerializeField, Required, LabelText("text_Buff名字文本"), TitleGroup("===Widget===")]
 		private TextMeshProUGUI _text_BuffNameText;

		[SerializeField, Required, LabelText("text_剩余时长文本"), TitleGroup("===Widget===")]
		private TextMeshProUGUI _text_RemainTimeText;
		
		
		[SerializeField,LabelText("闪烁时目标透明度") ,TitleGroup("===Config===")]
		private float _blinkTargetAlpha = 0.5f;
		
		
		[SerializeField,LabelText("闪烁时单过程时长") ,TitleGroup("===Config===")]
		private float _blinkSingleDuration = 0.5f;
		
		private float _currentBlinkAlpha;
		
		

		
		

#endregion
#region Assets

#endregion

		public I_BuffContentMayDisplayOnUI RelatedBuffRef { get; private set; }
		public PlayerARPGConcreteCharacterBehaviour RelatedPlayerBehaviourRef { get; private set; }



		private Tweener _tween_Blink;

		private UIP_PlayerBuffPanel _parentPanel;

		private void _Internal_SetAllAlpha(float a)
		{
			_currentBlinkAlpha = a;
			var color = new Color(1, 1, 1, _currentBlinkAlpha);
			 
			_image_IconSelf.color = color;
			_image_SelfBackgroundImage.color = color;
			_image_StackBaseImage.color = color;
			_text_StackText.color = color;
			_text_BuffNameText.color = color;
			_text_RemainTimeText.color = color;


		}

		public void InstantiateInitialize(I_BuffContentMayDisplayOnUI interfaceRef , UIP_PlayerBuffPanel parentPanel)
		{
			_parentPanel = parentPanel;
			RelatedBuffRef = interfaceRef;
			_text_RemainTimeText.text = RelatedBuffRef.UI_GetBuffContent_RemainingTimeText();
			_text_BuffNameText.text = RelatedBuffRef.UI_GetBuffContent_NameText();
			var fe_sprite = GlobalConfigurationAssetHolderHelper.GetGCAHH().FE_GeneralSpriteResource;
			_image_IconSelf.sprite = interfaceRef.RelatedBuffDisplayOnUIInfo.GetIconSprite();
			
			

			switch (interfaceRef.RelatedBuffDisplayOnUIInfo.BuffUIType)
			{
				case BuffUITypeEnum.NeutralBuff_个人中性Buff:
					_image_SelfBackgroundImage.sprite = parentPanel._sprite_NeutralBuffEntryBackground;
				
					
					break;
				case BuffUITypeEnum.SelfPositiveBuff_个人正面Buff:
					_image_SelfBackgroundImage.sprite = parentPanel._sprite_PositiveBuffEntryBackground;
					break;
				case BuffUITypeEnum.SelfNegativeBuff_个人负面Buff:
					_image_SelfBackgroundImage.sprite = parentPanel._sprite_NegativeBuffEntryBackground;
				
					break;
			}
			if (interfaceRef is BaseRPBuff)
			{
				RelatedPlayerBehaviourRef =
					(interfaceRef as BaseRPBuff).Parent_SelfBelongToObject as PlayerARPGConcreteCharacterBehaviour;
			}
			else if (interfaceRef is PerBuffTimedStack stack)
			{
				RelatedPlayerBehaviourRef =
					(stack.RelatedBuffRef as BaseRPBuff).Parent_SelfBelongToObject as
					PlayerARPGConcreteCharacterBehaviour;
			}

			RefreshContent();
		}

		public void UpdateTick(float ct, int cf, float delta)
		{
			if (!gameObject.activeInHierarchy)
			{
				return;
			}

			if (cf % 10 == 0)
			{
				RefreshContent();
			}
		}


		/// <summary>
		/// <para>显式地要求刷新内容。因为有可能这个UIRW并不活跃，那时候是不会刷新的，而如果是刚刚激活(通常来源于角色切换)，则需要立刻显式刷新</para>
		/// </summary>
		public void RefreshContent()
		{
			string text_time = RelatedBuffRef.UI_GetBuffContent_RemainingTimeText();
			if (!_text_RemainTimeText.text.Equals(text_time))
			{
				_text_RemainTimeText.text = text_time;
			}
			string text_name = RelatedBuffRef.UI_GetBuffContent_NameText();
			if (!_text_BuffNameText.text.Equals(text_name))
			{
				_text_BuffNameText.text = text_name;
			}

			var stack = RelatedBuffRef.UI_GetBuffContent_Stack();
			if (stack == null)
			{
				_image_StackBaseImage.gameObject.SetActive(false);
			}
			else
			{
				_image_StackBaseImage.gameObject.SetActive(true);
				if (!_text_StackText.text.Equals(stack.Value))
				{
					_text_StackText.text = stack.Value.ToString();
				}
			}

			//如果需要闪烁，但是当前没在闪烁
			if (RelatedBuffRef.IfNeedBlink() && _tween_Blink == null)
			{
				_currentBlinkAlpha = 1f;
				_tween_Blink = DOTween
					.To(() => _currentBlinkAlpha,
						(x) => _Internal_SetAllAlpha(x),
						_blinkTargetAlpha,
						_blinkSingleDuration).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
			}
			else if (!RelatedBuffRef.IfNeedBlink() && _tween_Blink != null)
			{
				_tween_Blink.Kill();
				_currentBlinkAlpha = 1f;
				_Internal_SetAllAlpha(_currentBlinkAlpha);
			}
		}
	}

}