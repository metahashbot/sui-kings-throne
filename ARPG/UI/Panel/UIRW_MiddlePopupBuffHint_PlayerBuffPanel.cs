using DG.Tweening;
using Global.UIBase;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
namespace ARPG.UI.Panel
{
	public class UIRW_MiddlePopupBuffHint_PlayerBuffPanel : UI_UISingleRuntimeWidget
	{
		
		[SerializeField,LabelText("rect_本体Rect")]
		private RectTransform _rectTransform;


		[SerializeField, LabelText("用来动的Holder")]
		private RectTransform _rect_ToHolder;
		
		[SerializeField,LabelText("image_方向Image")]
		private Image _image_FixedHint;
		[SerializeField,LabelText("image_BuffImage")]
		private Image _image_Buff;


		private int finish2;
		
		private static UIP_PlayerBuffPanel _parentPanel;

		public void InstantiateInitialize(Sprite image , UIP_PlayerBuffPanel parentPanel)
		{
			finish2 = 0;
			_parentPanel = parentPanel;
			Color fromColor = new Color(1, 1, 1, parentPanel._color_in_alpha);
			_rect_ToHolder.anchoredPosition = new Vector2(0, -parentPanel._height_in_offset);
			_rect_ToHolder.DOAnchorPosY(0f, parentPanel._height_in_duration).SetEase(parentPanel._height_in_ease)
				.OnComplete(_Height_DelayAndOut);

			_image_Buff.sprite = image;

			_image_Buff.color = fromColor;
			_image_FixedHint.color = fromColor;
			_image_Buff.DOColor(Color.white, _parentPanel._color_in_duration).SetEase(_parentPanel._color_in_ease).OnComplete((() =>
			{
				_Color_DelayAndOut();
			}));
			_image_FixedHint.DOColor(Color.white, _parentPanel._color_in_duration).SetEase(_parentPanel._color_in_ease);

		}

		private void _Height_DelayAndOut()
		{
			_rect_ToHolder.DOAnchorPosY(_parentPanel._height_out_Offset, _parentPanel._height_out_duration)
				.SetEase(_parentPanel._height_out_ease).OnComplete(TryDisableSelf)
				.SetDelay(_parentPanel._heightDurationStay);
		}

		private void _Color_DelayAndOut()
		{
			var newColor = new Color(1, 1, 1, _parentPanel._color_out_alpha);
			_image_Buff.DOColor(newColor, _parentPanel._color_out_duration).SetEase(_parentPanel._color_out_ease)
				.SetDelay(_parentPanel._colorDurationStay).OnComplete(TryDisableSelf);
			_image_FixedHint.DOColor(newColor, _parentPanel._color_out_duration).SetEase(_parentPanel._color_out_ease)
				.SetDelay(_parentPanel._colorDurationStay);
		}
		private void TryDisableSelf()
		{
			finish2 += 1;
			if (finish2 == 2)
			{
				gameObject.SetActive(false);
			}

		}
		
		
		
		
	}
}