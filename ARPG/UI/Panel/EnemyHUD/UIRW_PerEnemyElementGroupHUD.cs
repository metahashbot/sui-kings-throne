using System.Collections.Generic;
using ARPG.Character.Base;
using ARPG.Character.Enemy;
using Global;
using Global.ActionBus;
using Global.RuntimeAssetHolder;
using Global.UIBase;
using RPGCore.Buff;
using RPGCore.Buff.ConcreteBuff.Element.First;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.UI.Panel
{
	[TypeInfoBox("这是一个用于敌人HUD的一级元素反应的HUD。")]
	public class UIRW_PerEnemyElementGroupHUD : UI_UISingleRuntimeWidget
	{

		private bool _BehaviourContainElementGroupHolder;
		
		[SerializeField, LabelText("holder_一个")]
		private GameObject _holder_OneCapacity;

		[SerializeField, LabelText("sprite-1.1")]
		private SpriteRenderer _sprite_1_1;
		
		[SerializeField,LabelText("holder_两个")]
		private GameObject _holder_TwoCapacity;
		
		[SerializeField,LabelText("sprite-2.1")]
		private SpriteRenderer _sprite_2_1;
		 
		[SerializeField,LabelText("sprite-2.2")]
		private SpriteRenderer _sprite_2_2;
		
		[SerializeField,LabelText("holder_三个")]
		private GameObject _holder_ThreeCapacity;
		 
		[SerializeField,LabelText("sprite-3.1")]
		private SpriteRenderer _sprite_3_1;
		 
		[SerializeField,LabelText("sprite-3.2")]
		private SpriteRenderer _sprite_3_2;
		 
		[SerializeField,LabelText("sprite-3.3")]
		private SpriteRenderer _sprite_3_3;
		
		[SerializeField,LabelText("holder_四个")]
		private GameObject _holder_FourCapacity;
		
		[SerializeField,LabelText("sprite-4.1")]
		private SpriteRenderer _sprite_4_1;

		[SerializeField, LabelText("sprite-4.2")]
		private SpriteRenderer _sprite_4_2;

		[SerializeField, LabelText("sprite-4.3")] 
		private SpriteRenderer _sprite_4_3;
		
		[SerializeField, LabelText("sprite-4.4")]
		private SpriteRenderer _sprite_4_4;



		[ShowInInspector]
		private BaseARPGCharacterBehaviour _relatedBehaviour;

		private List<FirstElementTagBuff> _relatedBuffList = new List<FirstElementTagBuff>();

		private static UIP_EnemyHUDPanel _parentPanelRef;

		private float _relatedBehaviourHalfVFXRadius;
		private float _posOffset_X;
		private float _posOffset_Y;
		public void InitializeWithRelateBehaviour(BaseARPGCharacterBehaviour behaviour , UIP_EnemyHUDPanel parent)
		{
			_relatedBuffList.Clear();
			_holder_OneCapacity.SetActive(false);
			_holder_TwoCapacity.SetActive(false);
			_holder_ThreeCapacity.SetActive(false);
			_holder_FourCapacity.SetActive(false);
			_parentPanelRef = parent;
			_relatedBehaviour = behaviour;
			
			var lab = _relatedBehaviour.GetRelatedActionBus();
			_relatedBehaviour.GetRelatedActionBus().RegisterAction(ActionBus_ActionTypeEnum.L_Buff_OnBuffInitialized_一个Buff被确定初始化了,_ABC_TryRefreshBuffLayoutOnBuffAdd);
			lab.RegisterAction(ActionBus_ActionTypeEnum.L_Buff_OnBuffRemoved, _ABC_TryRefreshBuffLayout_OnBuffRemove);
			lab.RegisterAction(ActionBus_ActionTypeEnum.L_ARPGBehaviour_BehaviourDataInvalid_角色行为数据不再有效,
				_ABC_ClearAndReturnToPool_OnRelatedBehaviourDataInvalid);


			_relatedBehaviourHalfVFXRadius = behaviour.GetRelatedArtHelper()._VFXScaleRadius / 2f;
			//设置位置到辣个位置
			if (behaviour._HUDElementGroupMatchTransform != null)
			{
				transform.SetParent(_relatedBehaviour._HUDElementGroupMatchTransform.transform);
				transform.localPosition = Vector3.zero;
				_BehaviourContainElementGroupHolder = true;
				_posOffset_X = _relatedBehaviour._HUDElementGroupMatchTransform.transform.localPosition.x;
				_posOffset_Y = _relatedBehaviour._HUDElementGroupMatchTransform.transform.localPosition.y;
			}
			else
			{
				_BehaviourContainElementGroupHolder = false;
				_posOffset_X = _relatedBehaviourHalfVFXRadius * _parentPanelRef._config_elementGroupWidthMul;

				transform.SetParent(_relatedBehaviour._HUDHeightMatchGameObject.transform);
				_posOffset_Y = _relatedBehaviourHalfVFXRadius * _parentPanelRef._config_elementGroupHeightMul;
				transform.localPosition = new Vector3(-_posOffset_X, -_posOffset_Y, 0f);
			}
			transform.localScale =
				Vector3.one * ((_relatedBehaviourHalfVFXRadius) * _parentPanelRef._config_elementGroupScale);

		}


		private void _ABC_TryRefreshBuffLayoutOnBuffAdd(DS_ActionBusArguGroup ds)
		{
			var buff = ds.GetObj1AsT<BaseRPBuff>();
			if (buff is not FirstElementTagBuff firstTag)
			{
				return;
			}
			_relatedBuffList.Add(firstTag);
			RefreshElementLayout();
			;

		}

		private void _ABC_TryRefreshBuffLayout_OnBuffRemove(DS_ActionBusArguGroup ds)
		{
			var buff = ds.GetObj1AsT<BaseRPBuff>();
			if (buff is not FirstElementTagBuff firstTag)
			{
				return;
			}
			_relatedBuffList.Remove(firstTag);
			RefreshElementLayout();
		}



		private void _ABC_ClearAndReturnToPool_OnRelatedBehaviourDataInvalid(DS_ActionBusArguGroup ds)
		{
			_holder_OneCapacity.SetActive(false);
			_holder_TwoCapacity.SetActive(false);
			_holder_ThreeCapacity.SetActive(false);
			_holder_FourCapacity.SetActive(false);
			_relatedBuffList.Clear();
			ReturnUIRW();
		}


		/// <summary>
		/// 跟随翻面
		/// </summary>
		/// <param name="ct"></param>
		/// <param name="cf"></param>
		/// <param name="delta"></param>
		public void UpdateTick(float ct, int cf, float delta)
		{
			if (cf % _parentPanelRef._elementGroupCheckFlipFrameInterval == 0)
			{
				if (_relatedBehaviour == null)
				{
					return;
				}
				//朝左的时候X是正的
				if (_relatedBehaviour.GetRelatedArtHelper().CurrentFaceLeft)
				{
					if (!_BehaviourContainElementGroupHolder)
					{
						transform.localPosition = new Vector3(_posOffset_X, -_posOffset_Y, 0f);
					}
					else
					{
						_relatedBehaviour._HUDElementGroupMatchTransform.transform.localPosition =
							new Vector3(-_posOffset_X, _posOffset_Y, 0f);
					}
				}
				//朝右的时候
				else
				{
					if (!_BehaviourContainElementGroupHolder)
					{
						transform.localPosition = new Vector3(-_posOffset_X, -_posOffset_Y, 0f);
					}
					else
					{
						_relatedBehaviour._HUDElementGroupMatchTransform.transform.localPosition =
							new Vector3(_posOffset_X, _posOffset_Y, 0f);
					}
				}
			}
		}


		private void ReturnUIRW()
		{
			_parentPanelRef._Internal_ReturnElementGroup(this);
		}



		private Sprite GetSprite(FirstElementTagBuff buff)
		{
			switch (buff.SelfBuffType)
			{
				case RolePlay_BuffTypeEnum.ElementFirstEarthTag_Tu_一级土标签:
					return _parentPanelRef._sprite_earth_tu;
				case RolePlay_BuffTypeEnum.ElementFirstFireTag_Huo_一级火标签:
					return _parentPanelRef._sprite_fire_huo;
				case RolePlay_BuffTypeEnum.ElementFirstWaterTag_Shui_一级水标签:
					return _parentPanelRef._sprite_water_shui;
				case RolePlay_BuffTypeEnum.ElementFirstWindTag_Feng_一级风标签:
					return _parentPanelRef._sprite_wind_feng;
				 case RolePlay_BuffTypeEnum.ElementFirstElectricTag_Dian_一级电标签:
					return _parentPanelRef._sprite_electric_dian;
				  case RolePlay_BuffTypeEnum.ElementFirstLightTag_Guang_一级光标签 :
					return _parentPanelRef._sprite_light_guang;
				  case RolePlay_BuffTypeEnum.ElementFirstSoulTag_Ling_一级灵标签:
					return _parentPanelRef._sprite_spirit_ling;
			}
			return null;
		}

		private void RefreshElementLayout()
		{
			var count = _relatedBuffList.Count;
			switch (count)
			{
				case 0:
					_holder_OneCapacity.SetActive(false);
					_holder_TwoCapacity.SetActive(false);
					_holder_ThreeCapacity.SetActive(false);
					_holder_FourCapacity.SetActive(false);
					break;
				case 1:
					_holder_OneCapacity.SetActive(true);
					_holder_TwoCapacity.SetActive(false);
					_holder_ThreeCapacity.SetActive(false);
					_holder_FourCapacity.SetActive(false);
					_sprite_1_1.sprite = GetSprite(_relatedBuffList[0]);
					break;
				case 2:
					_holder_OneCapacity.SetActive(false);
					_holder_TwoCapacity.SetActive(true);
					_holder_ThreeCapacity.SetActive(false);
					_holder_FourCapacity.SetActive(false);
					_sprite_2_1.sprite =  GetSprite(_relatedBuffList[0]);
					_sprite_2_2.sprite =  GetSprite(_relatedBuffList[1]);
					break;
				case 3:
					_holder_OneCapacity.SetActive(false);
					_holder_TwoCapacity.SetActive(false);
					_holder_ThreeCapacity.SetActive(true);
					_holder_FourCapacity.SetActive(false);
					_sprite_3_1.sprite =  GetSprite(_relatedBuffList[0]);
					_sprite_3_2.sprite =  GetSprite(_relatedBuffList[1]);
					_sprite_3_3.sprite =  GetSprite(_relatedBuffList[2]);
					 
					break;
				default:
					_holder_OneCapacity.SetActive(false);
					_holder_TwoCapacity.SetActive(false);
					_holder_ThreeCapacity.SetActive(false);
					_holder_FourCapacity.SetActive(true);
					_sprite_4_1.sprite =  GetSprite(_relatedBuffList[0]);
					_sprite_4_2.sprite =  GetSprite(_relatedBuffList[1]);
					_sprite_4_3.sprite =  GetSprite(_relatedBuffList[2]);
					_sprite_4_4.sprite =  GetSprite(_relatedBuffList[3]);
					break;
			}
		}





	}
}