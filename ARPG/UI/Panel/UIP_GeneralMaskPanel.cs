using DG.Tweening;
using GameplayEvent.Handler.CommonUtility;
using Global;
using Global.ActionBus;
using Global.UIBase;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
namespace ARPG.UI.Panel
{
	[TypeInfoBox(" 通用遮罩面板，用于遮罩全屏")]
	public class UIP_GeneralMaskPanel : UI_UIBasePanel
	{
		[SerializeField,LabelText("初始淡入时长")]
		protected float _initialFadeInDuration = 1f;
		
		[SerializeField,LabelText("初始淡入延迟")]
		protected float _initialFadeInDelay = 0.5f;
		
		
		[SerializeField, LabelText("底图Image"), Required]
		protected Image _image_BaseImage;
		
		[SerializeField,LabelText("常规淡入淡出时长")]
		protected float _duration = 0.5f;

		
		[SerializeField,LabelText("默认黑屏时长")]
		protected float _defaultMaskDuration = 1f;
		private Tweener _tweener;
		

		public override void StartInitializeByUIM()
		{
			_tweener?.Kill();
			ShowThisPanel();
			_image_BaseImage.gameObject.SetActive((true));
			_image_BaseImage.color = Color.black;
			_tweener = _image_BaseImage.DOColor(Color.clear, _initialFadeInDuration).SetDelay(
				_initialFadeInDelay).OnComplete(() =>{HideThisPanel();});
			base.StartInitializeByUIM();


			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_UI_General_RequireScreenMaskDuration_UI要求屏幕遮罩时长,
				_ABC_ProcessMask_OnMaskInfoReceive);
		}



		private void _ABC_ProcessMask_OnMaskInfoReceive(DS_ActionBusArguGroup ds)
		{
			if (ds.ObjectArgu1 is GEH_快速UI全屏遮罩_QuickFullScreenMaskOverlay maskGEH)
			{
			

				ShowThisPanel();


				_image_BaseImage.gameObject.SetActive((true));
				if (maskGEH.AbortOtherSame)
				{
					_tweener?.Kill();
				}
				if (maskGEH.StartFadeInTransparent)
				{
					_image_BaseImage.color = Color.clear;
				}
				_tweener = _image_BaseImage.DOColor(maskGEH.MidPhaseColor, maskGEH.FadeInDuration).SetUpdate( maskGEH.IgnoreTimeScale)
					.SetEase(maskGEH.EaseType);
				_tweener.onComplete += (() =>
				{
					_image_BaseImage.DOColor(Color.clear, maskGEH.FadeOutDuration).SetUpdate(maskGEH.IgnoreTimeScale).SetDelay(maskGEH.WaitDuration)
						.onComplete += (() =>
					{
						HideThisPanel();
					});
				});

			}
			else
			{
				if (IsPanelCurrentSelfActive)
				{
					if (!ds.FloatArgu4.HasValue)
					{
						return;
					}
				}

				ShowThisPanel();


				_image_BaseImage.gameObject.SetActive((true));

				
				_tweener?.Kill();
				var duration = ds.FloatArgu1 ?? _defaultMaskDuration;


				Color fromColor = new Color(0f, 0f, 0f, 0f);
				Color toColor = new Color(0f, 0f, 0f, 1f);
				_image_BaseImage.color = fromColor;
				_tweener = _image_BaseImage.DOColor(toColor, _duration).SetEase(Ease.InOutSine);
				_tweener.onComplete += () =>
				{
					_image_BaseImage.DOColor(fromColor, _duration).SetDelay(duration).onComplete += () =>
					{
						HideThisPanel();
					};
				};
			}
		}

		

	}
}