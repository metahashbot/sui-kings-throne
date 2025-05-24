using System;
using Global;
using Global.ActionBus;
using Global.UIBase;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.UI.Panel.Pointer
{
	[Serializable]
	public class UIP_PointerPanel : UI_UIBasePanel
	{

		[SerializeField,Required,LabelText("rect-指针holder")]
		private RectTransform _rect_PointerHolder;

		[SerializeField,Required,LabelText("image_指针图标")]
		private UnityEngine.UI.Image _image_PointerIcon;
		
		[SerializeField,LabelText("指针尺寸")]
		private float _pointerSize = 1f;
		
		
		[SerializeField,LabelText("sprite-尖指针"),TitleGroup("===Sprite===")]
		private Sprite _sprite_DefaultPointer;
		
		[SerializeField,LabelText("sprite-战斗_剑"),TitleGroup("===Sprite===")]
		private Sprite _sprite_BattleWithSword;


		
		[ShowInInspector]
		public PointerStateTypeEnum CurrentPointerStateType { get; private set; }

		public enum PointerStateTypeEnum
		{
			DefaultPointer_默认尖指针 =0,
			BattleWithSword_战斗_剑 = 1,
			
			
		}


		protected override void Awake()
		{
			base.Awake();
			// Cursor.visible = false;
			_image_PointerIcon.transform.localScale = Vector3.one * _pointerSize;
			SwitchPointerState(PointerStateTypeEnum.DefaultPointer_默认尖指针);
		}


		protected override void BindingEvent()
		{
			base.BindingEvent();
			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_UI_Pointer_RequireChangePointerType_要求改变指针类型,
				_ABC_ChangePointerType);
		}

		public override void UpdateTick(float currentTime, int currentFrameCount, float deltaTime)
		{
			base.UpdateTick(currentTime, currentFrameCount, deltaTime);

			Vector2 mousePosition = Input.mousePosition;
			Vector2 localPoint;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(_selfCanvasRect,
				mousePosition,
				null,
				out localPoint);
			_rect_PointerHolder.localPosition = localPoint;

		}


		private void SwitchPointerState(PointerStateTypeEnum stateType)
		{
			CurrentPointerStateType = stateType;
			switch (stateType)
			{
				case PointerStateTypeEnum.DefaultPointer_默认尖指针:
					_image_PointerIcon.sprite = _sprite_DefaultPointer;
					break;
				case PointerStateTypeEnum.BattleWithSword_战斗_剑:
					_image_PointerIcon.sprite = _sprite_BattleWithSword;
					break;
			}
		}



		private void _ABC_ChangePointerType(DS_ActionBusArguGroup ds)
		{
			if (ds.IntArgu1.HasValue)
			{
				SwitchPointerState((PointerStateTypeEnum) ds.IntArgu1.Value);
			}
			if (ds.FloatArgu1.HasValue)
			{
				_pointerSize = ds.FloatArgu1.Value;
				_image_PointerIcon.transform.localScale = Vector3.one * _pointerSize;
			}
		}
		
	}
}
