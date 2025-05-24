using ARPG.Character;
using Global;
using Global.ActionBus;
using Global.UIBase;
using RPGCore.DataEntry;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.UI.Panel.HPWarningPanel
{
	public class UIP_HPWarningPanel : UI_UIBasePanel
	{

		[SerializeField, LabelText("颜色渐变")]
		public Gradient GradientColor;

		[SerializeField, LabelText("颜色渐变整条时长")]
		public float GradientColorDuration = 2f;
		 

		[SerializeField,LabelText("image_效果图片")]
		private UnityEngine.UI.Image _image_Effect;
		
		[SerializeField,LabelText("开始警示的血量比例")]
		private float _startWarningHPPercent = 0.15f;
		
		
		
		protected PlayerARPGConcreteCharacterBehaviour _currentActivePlayerBehaviourRef;

		public override void StartInitializeByUIM()
		{
			_image_Effect.gameObject.SetActive(false);
			base.StartInitializeByUIM();
			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_PC_OnCurrentUsingCharacterChanged_当前使用的角色更换,
				_ABC_RefreshPanelState_OnCurrentActiveCharacterChanged,
				100);
		}

		private bool _currentActive;
		private float _evaAccu;

		public override void UpdateTick(float currentTime, int currentFrameCount, float deltaTime)
		{
			base.UpdateTick(currentTime, currentFrameCount, deltaTime);

			if (_currentActive)
			{
				_evaAccu += deltaTime / GradientColorDuration;

				_image_Effect.color = GradientColor.Evaluate(_evaAccu % 1f);
			}
			
		}




		private void _ABC_RefreshPanelState_OnCurrentActiveCharacterChanged(DS_ActionBusArguGroup ds)
		{
			_currentActivePlayerBehaviourRef = ds.ObjectArgu1 as PlayerARPGConcreteCharacterBehaviour;


			if (ds.ObjectArgu2 != null)
			{
				(ds.ObjectArgu2 as PlayerARPGConcreteCharacterBehaviour).GetRelatedActionBus().RemoveAction(
					ActionBus_ActionTypeEnum.L_DataEntry_OnDataEntryValueChanged_数据项的值被改变,
					_ABC_CheckIfEnableHPWarning_OnHPValueChanged);
			}

			_currentActivePlayerBehaviourRef.GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_DataEntry_OnDataEntryValueChanged_数据项的值被改变,
				_ABC_CheckIfEnableHPWarning_OnHPValueChanged);
			_Internal_CheckIfWarning();

		}


		private void _Internal_CheckIfWarning()
		{
			var currentHP = _currentActivePlayerBehaviourRef
				.GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentHP_当前HP).CurrentValue;
			var currentMaxHP = _currentActivePlayerBehaviourRef
				.GetFloatDataEntryByType(RP_DataEntry_EnumType.HPMax_最大HP).CurrentValue;


			if (currentHP / currentMaxHP < _startWarningHPPercent)
			{
				if (!_currentActive)
				{
					_currentActive = true;
					_image_Effect.gameObject.SetActive(true);

				}
			}
			else
			{
				if (_currentActive)
				{

					_currentActive = false;
					_image_Effect.gameObject.SetActive(false);
				}
			}

		}


		private void _ABC_CheckIfEnableHPWarning_OnHPValueChanged(DS_ActionBusArguGroup ds)
		{
			if ((RP_DataEntry_EnumType)ds.IntArgu1.Value != RP_DataEntry_EnumType.CurrentHP_当前HP)
			{
				return;
			}
			_Internal_CheckIfWarning();

		}
	}
}
