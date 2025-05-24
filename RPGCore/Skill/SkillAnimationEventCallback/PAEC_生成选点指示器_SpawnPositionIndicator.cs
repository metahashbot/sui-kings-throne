using System;
using ARPG.Character.Base;
using RPGCore.Skill;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Player
{
	[Serializable]
	public class PAEC_生成选点指示器_SpawnPositionIndicator : BasePlayerAnimationEventCallback
	{
		[NonSerialized]
		public bool _isIndicatorActive = false;

		[SerializeField,LabelText("修正高度")]
		public float _heightOffset = 0.5f;

		[SerializeField, LabelText("包含 外圈范围指示？")]
		public bool _includeOuterRangeIndicator = false;

		[ShowIf(nameof(_includeOuterRangeIndicator))]
		[SerializeField, LabelText("prefab_外圈范围指示")]
		public GameObject _prefab_OuterRangeIndicator;

		[NonSerialized]
		public GameObject _objectR_OuterRangeIndicator;

		[SerializeField, LabelText("包含 选点内圈指示？")]
		public bool _includeInnerRangeIndicator = false;

		[NonSerialized]
		public float _innerRangeLimit;

		[ShowIf(nameof(_includeInnerRangeIndicator))]
		[SerializeField, LabelText("prefab_选点内圈指示")]
		public GameObject _prefab_InnerRangeIndicator;

		[ShowIf(nameof(_includeInnerRangeIndicator))]
		[SerializeField, LabelText("√:内圈包含方向计算？")]
		public bool _includeInnerRangeDirection = false;

		[NonSerialized]
		public GameObject _objectR_InnerRangeIndicator;




		public override BasePlayerAnimationEventCallback ExecuteBySkill(
			BaseARPGCharacterBehaviour behaviour,
			BaseRPSkill skillRef,
			bool createNewWhenExist = false)
		{
			return base.ExecuteBySkill(behaviour, skillRef, createNewWhenExist);
		}

		public void ActiveAndSetIndicator(
			Vector3 outerRangeCenter,
			float outerRangeRadius,
			Vector3 innerRangePosition,
			float innerRangeRadius,
			Vector3 innerRangeDirectionLocal)
		{
			_isIndicatorActive = true;

			if (_includeInnerRangeIndicator)
			{
				if (_objectR_InnerRangeIndicator == null)
				{
					_objectR_InnerRangeIndicator = UnityEngine.Object.Instantiate(_prefab_InnerRangeIndicator);
				}
				else
				{
					_objectR_InnerRangeIndicator.gameObject.SetActive(true);
				}
			}

			if (_includeOuterRangeIndicator)
			{
				if (_objectR_OuterRangeIndicator == null)
				{
					_objectR_OuterRangeIndicator = UnityEngine.Object.Instantiate(_prefab_OuterRangeIndicator);
				}
				else
				{
					_objectR_OuterRangeIndicator.gameObject.SetActive(true);
				}
			}

			_innerRangeLimit = innerRangeRadius;



			_isIndicatorActive = true;
			if (_includeOuterRangeIndicator)
			{
				_objectR_OuterRangeIndicator.transform.position = outerRangeCenter + Vector3.up * _heightOffset; 
				_objectR_OuterRangeIndicator.transform.localScale = Vector3.one * (outerRangeRadius);
			}

			if (_includeInnerRangeIndicator)
			{
				_objectR_InnerRangeIndicator.transform.position = innerRangePosition + Vector3.up * _heightOffset;
				_objectR_InnerRangeIndicator.transform.localScale = Vector3.one * (innerRangeRadius);
				if (_includeInnerRangeDirection)
				{
					var dir = innerRangeDirectionLocal;
					dir.y = 0f;
					_objectR_InnerRangeIndicator.transform.forward =
						_objectR_InnerRangeIndicator.transform.TransformDirection(dir);
				}
			}
		}


		public void UpdateTick_ProcessIndicatorPosition(
			Vector3 outerRangeCenter,
			Vector3 innerRangePosition,
			Vector3 innerRangeDirectionLocal)
		{
			if (!_isIndicatorActive)
			{
				return ;
			}
			if (_includeOuterRangeIndicator)
			{
				_objectR_OuterRangeIndicator.transform.position = outerRangeCenter + Vector3.up * _heightOffset;
			}

			if (_includeInnerRangeIndicator)
			{
				var dis = Vector3.Distance(outerRangeCenter, innerRangePosition);
				if (dis > _innerRangeLimit)
				{
					innerRangePosition = outerRangeCenter +
					                     (innerRangePosition - outerRangeCenter).normalized * _innerRangeLimit; 
					 
				}
				_objectR_InnerRangeIndicator.transform.position = innerRangePosition + Vector3.up * _heightOffset;
				 

				var dir = innerRangeDirectionLocal;
				dir.y = 0f;
				if (_includeInnerRangeDirection)
				{
					_objectR_InnerRangeIndicator.transform.forward =
						_objectR_InnerRangeIndicator.transform.TransformDirection(dir);
				}
			}
		}



		public void DisableIndicator()
		{
			_isIndicatorActive = false;
			if (_objectR_InnerRangeIndicator != null)
			{
				_objectR_InnerRangeIndicator.gameObject.SetActive(false);
			}

			if (_objectR_OuterRangeIndicator != null)
			{
				_objectR_OuterRangeIndicator.gameObject.SetActive(false);
			}
		}

	}
}