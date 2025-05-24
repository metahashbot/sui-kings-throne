using System;
using ARPG.Character.Enemy;
using DG.Tweening;
using Global.ActionBus;
using Global.UIBase;
using RPGCore.Buff;
using RPGCore.Buff.ConcreteBuff.Common;
using RPGCore.DataEntry;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.UI.Panel
{
	[TypeInfoBox("敌人的弱点条，这个是UI。【机制计量条】。")]
	public class UIRW_SingleEnemyWeaknessEntry : UI_UISingleRuntimeWidget
	{
		private static UIP_EnemyHUDPanel _enemyHUDPanelRef;
		
		
		
		[LabelText("rect-自身RectTransform ") ,Required, SerializeField, TitleGroup("===Widget===")]
		private RectTransform _rect_SelfRectTransform;
		[LabelText("rect-动画定位点RectTransform") ,Required, SerializeField, TitleGroup("===Widget===")]
		private RectTransform _rect_AnimationPositionRectTransform;
		
		
		[LabelText("slider_本体滑条") ,Required, SerializeField, TitleGroup("===Widget===")]
		private UnityEngine.UI.Slider _slider_SelfSlider;
		
		[LabelText("image_外部光晕图片") ,Required,SerializeField,TitleGroup("===Widget===")]
		private UnityEngine.UI.Image _image_OutlineImage;
		
		[LabelText("image_滑条的填充部分") ,Required,SerializeField,TitleGroup("===Widget===")]
		private UnityEngine.UI.Image _image_SliderFillImage;



		[LabelText("起始尺寸"), SerializeField, TitleGroup("===动画===")]
		private float _startScale = 1.5f;
		 
		[LabelText("起始缩放到1的时长") ,SerializeField,TitleGroup("===动画===")]
		private float _startScaleTo1Duration = 0.35f;
		
		[LabelText("起始抖动时长") ,SerializeField,TitleGroup("===动画===")]
		private float _startShakeDuration = 0.85f;
		
		[LabelText("起始抖动强度") ,SerializeField,TitleGroup("===动画===")]
		private float _startShakeStrength = 10f;
		
		[LabelText("变动时抖动时长") ,SerializeField,TitleGroup("===动画===")]
		private float _changeShakeDuration = 0.35f;
		
		[LabelText("变动时抖动强度——变动0%时") ,SerializeField,TitleGroup("===动画===")]
		private float _changeShakeStrength = 5f;
		
		[LabelText("变动时抖动强度——变动100%时") ,SerializeField,TitleGroup("===动画===")]
		private float _changeShakeStrength_100 = 10f;
		 

		private Tweener _selfShakeTween;


		[ShowInInspector, LabelText("关联的敌人Behaviour"), FoldoutGroup("运行时", true)]
		public EnemyARPGCharacterBehaviour RelatedEnemyBehaviourRef
		{
			get;
			private set;
		}

		public Buff_通用敌人弱点_CommonEnemyWeakness.WeaknessInfoGroup RelatedGroupRef
		{
			get;
			private set;
		}
		


		
		public static void StaticInitialize(UIP_EnemyHUDPanel panel)
		{
			_enemyHUDPanelRef = panel;
		}


		public void InitializeOnInstantiate(Buff_通用敌人弱点_CommonEnemyWeakness.WeaknessInfoGroup  groupRef)
		{
			RelatedGroupRef = groupRef;
			RelatedEnemyBehaviourRef = groupRef.RelatedBuff.Parent_SelfBelongToObject as EnemyARPGCharacterBehaviour;
			_lastValue = 0f;

			
			// transform.SetParent(behaviour._WeaknessMatchingGameObject.transform, false);
			transform.localPosition = Vector3.zero;
			// _holder_HeightHolder.transform.localPosition = new Vector3(0f, behaviour.HUDHeight, 0f);
			transform.localRotation = Quaternion.identity;

		}


		private Tweener _tween_shake_Minor;
		private Tweener _tween_shake_Major;
		
		
		
		private float _lastValue = 0f;

		public void UpdateTick(float ct, int cf, float delta)
		{
			if (!RelatedGroupRef.CurrentGroupActive)
			{
				if (gameObject.activeSelf)
				{
					gameObject.SetActive(false);
				}
				return;
			}
			
			if (RelatedGroupRef.CurrentGroupActive)
			{
				if (!gameObject.activeSelf)
				{
					gameObject.SetActive(true);

					_rect_AnimationPositionRectTransform.localScale = Vector3.one * _startScale;
					_rect_AnimationPositionRectTransform.DOScale(1f, _startScaleTo1Duration).SetEase(Ease.InBack)
						.OnComplete((() => _image_OutlineImage.gameObject.SetActive(false)));
					_selfShakeTween = _rect_AnimationPositionRectTransform.DOShakePosition(_startShakeDuration,
						_startShakeStrength);
				}

				if (Math.Abs(_lastValue - RelatedGroupRef.CurrentAmount) > float.Epsilon)
				{
					//发生了变动
					var deltaValue = RelatedGroupRef.CurrentAmount - _lastValue;
					var shakeValue = Mathf.Lerp(_changeShakeStrength, _changeShakeStrength_100, deltaValue);
					_selfShakeTween?.Kill();
					_selfShakeTween = _rect_AnimationPositionRectTransform.DOShakePosition(_changeShakeDuration,
						shakeValue);

					_slider_SelfSlider.value = 1f - RelatedGroupRef.CurrentAmount / RelatedGroupRef.TriggerAmount;
					// _slider_SelfSlider.DOValue( _relatedWeaknessBuffRef.CurrentWeaknessValue, _changeShakeDuration).SetEase(Ease.InOutBack);
				}
				_lastValue = RelatedGroupRef.CurrentAmount;
				
			}
			else
			{
				if (gameObject.activeSelf)
				{
					gameObject.SetActive(false);
				}
			}
		}
		
		
	}
}