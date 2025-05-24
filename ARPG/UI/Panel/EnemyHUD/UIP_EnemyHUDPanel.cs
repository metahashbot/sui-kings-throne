using System;
using System.Collections.Generic;
using ARPG.Character.Enemy;
using ARPG.Manager;
using Global;
using Global.ActionBus;
using Global.UIBase;
using RPGCore.Buff;
using RPGCore.Buff.ConcreteBuff.Common;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.UI.Panel
{
	[TypeInfoBox("作为敌人HUD显示的面板\n" +
	             "敌人的HUD并不全是UI。根据24.1版本的迭代，现在为如下情形：\n" +
	             "普通怪：血量不满时显示血条，不显示身后元素标签，不显示Buff\n" +
	             "精英和勇士：常驻血条，身后元素，紫色名字，头上Buff条\n" +
	             "子面板：首领状态面板：全都在屏幕上方")]
	public class UIP_EnemyHUDPanel : UI_UIBasePanel
	{
		
	
		/*
		 * 普通怪和精英怪（之后的描述中，勇士也是精英怪的一种）的HUD，虽然叫UIRW，但它们实际上并不是UI(没有Canvas)，也不在UIPanel之下。
		 * 它们只是由这个UIP所维护。
		 * 
		 */
		
		[SerializeField, Required, LabelText("自身RectTransform"),TitleGroup("本体")]
		private RectTransform _selfRectTransform;

		
		
		

		[SerializeField, LabelText("敌人换算屏幕高度时用的乘数"), TitleGroup("本体")]
		private float _mulConvert = 30f;


		[Serializable]
		public class BuffIconMaterialArguInfo
		{
			[LabelText("关联Buff类型")]
			public RolePlay_BuffTypeEnum Type;
			[LabelText("指定材质资产"),AssetsOnly]
			public Material TargetMaterial;
		}
		

		private CharacterOnMapManager _comRef;


#region 通用响应：

		private void _ABC_SpawnRelatedItem_OnNewBehaviourSpawned(DS_ActionBusArguGroup ds)
		{
			EnemyARPGCharacterBehaviour targetEnemyBehaviour = ds.ObjectArgu1 as EnemyARPGCharacterBehaviour;

			//boss将会使用Boss的顶部UI HUD
			if (targetEnemyBehaviour.ApplyDamage_CheckTargetBuff(RolePlay_BuffTypeEnum._EnemyTag_BossEnemy_首领敌人) ==
			    BuffAvailableType.Available_TimeInAndMeetRequirement)
			{
				targetEnemyBehaviour.GetRelatedActionBus().RegisterAction(
					ActionBus_ActionTypeEnum.L_Weakness_WeaknessGroupActiveStateChanged_弱点组的活跃状态被改变,
					_ABC_ReactToWeaknessSliderRequirement);
				targetEnemyBehaviour.GetRelatedActionBus().RegisterAction(
					ActionBus_ActionTypeEnum.L_ARPGBehaviour_BehaviourDataInvalid_角色行为数据不再有效,
					_ABC_ClearRegistrationOnBoss_OnEnemyDataInvalid);
				return;
			}
			else if (
				targetEnemyBehaviour.ApplyDamage_CheckTargetBuff(RolePlay_BuffTypeEnum._EnemyTag_NormalEnemy_普通敌人) ==
				BuffAvailableType.Available_TimeInAndMeetRequirement ||
				targetEnemyBehaviour.ApplyDamage_CheckTargetBuff(RolePlay_BuffTypeEnum._EnemyTag_EliteEnemy_精英敌人) ==
				BuffAvailableType.Available_TimeInAndMeetRequirement)
			{
				_GenerateEnemyStateGroupHUD(targetEnemyBehaviour);
				_GenerateElementGroup(targetEnemyBehaviour);
			}
		}

		private void _ABC_ClearRegistrationOnBoss_OnEnemyDataInvalid(DS_ActionBusArguGroup ds)
		{

			EnemyARPGCharacterBehaviour targetEnemyBehaviour = ds.ObjectArgu1 as EnemyARPGCharacterBehaviour;

			//boss将会使用Boss的顶部UI HUD
			if (targetEnemyBehaviour.ApplyDamage_CheckTargetBuff(RolePlay_BuffTypeEnum._EnemyTag_BossEnemy_首领敌人) ==
			    BuffAvailableType.Available_TimeInAndMeetRequirement)
			{
				targetEnemyBehaviour.GetRelatedActionBus().RemoveAction(
					ActionBus_ActionTypeEnum.L_Weakness_WeaknessGroupActiveStateChanged_弱点组的活跃状态被改变,
					_ABC_ReactToWeaknessSliderRequirement);
				return;
			}
		}



		public override void StartInitializeByUIM()
		{
			base.StartInitializeByUIM();
			UIRW_SingleEnemyWeaknessEntry.StaticInitialize(this);
		}

		public override void LateInitializeByUIM()
		{
			base.LateInitializeByUIM();
			_comRef = SubGameplayLogicManager_ARPG.Instance.CharacterOnMapManagerReference;
		}





		protected override void BindingEvent()
		{
			base.BindingEvent();
			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_CharacterOnMap_SpawnNewEnemy_生成了一个新敌人,
				_ABC_SpawnRelatedItem_OnNewBehaviourSpawned);
		}


		public override void UpdateTick(float currentTime, int currentFrameCount, float deltaTime)
		{
			// foreach (var perHUD in _selfAllEnemyHUDItemList)
			// {
			//
			// 	// var enemyTrans = perHUD.RelatedEnemyBehaviourRef.transform;
			// 	// var enemyPos = perHUD.RelatedEnemyBehaviourRef.transform.position;
			// 	// var rotAnchor = perHUD.RelatedEnemyBehaviourRef.GetRelatedArtHelper()._rotateAnchor;
			// 	// var upDir = rotAnchor.InverseTransformDirection(rotAnchor.up);
			// 	// enemyPos += upDir * (_mulConvert * perHUD.RelatedEnemyBehaviourRef.HUDHeight);
			//
			//
			// 	perHUD.UpdateTick(currentTime, currentFrameCount, deltaTime);
			// }

			UpdateTick_Weakness(currentTime, currentFrameCount, deltaTime);
			
			_UpdateTick_ElementGroup( currentTime,  currentFrameCount,  deltaTime);

			foreach (UIRW_EnemyAboveHUDItem perUIRW in _selfBusyEnemyHUDItemList)
			{
				perUIRW.UpdateTick( currentTime,  currentFrameCount,  deltaTime);
			}
		}




#endregion

#region 头上信息HUD

		[TitleGroup("头上HUD")]
		[SerializeField, Required, LabelText("Prefab_单项敌人HUD"), TitleGroup("头上HUD/===Prefab===")]
		private GameObject _sf_EnemyHUDItemPrefab;


		[SerializeField, Required, LabelText("Prefab_单项敌人Buff Icon"), TitleGroup("头上HUD/===Prefab===")]
		private GameObject _sf_EnemyBuffIconPrefab;



		[ShowInInspector, LabelText("当前空闲的HUD UIRW"), FoldoutGroup("运行时", true)]
		private List<UIRW_EnemyAboveHUDItem> _selfFreeEnemyHUDItemList = new List<UIRW_EnemyAboveHUDItem>();

		[ShowInInspector, LabelText("当前忙碌的HUD UIRW"), FoldoutGroup("运行时", true)]
		private List<UIRW_EnemyAboveHUDItem> _selfBusyEnemyHUDItemList = new List<UIRW_EnemyAboveHUDItem>();

		[SerializeField, LabelText("Buff图标材质覆写参数"), TitleGroup("头上HUD/===Config===")]
		public List<BuffIconMaterialArguInfo> BuffIconMaterialArguInfoList = new List<BuffIconMaterialArguInfo>();


		[SerializeField, LabelText("默认名字尺寸"), TitleGroup("头上HUD/===Config===")]
		public float _defaultNameTextFontSize = 285f;

		[SerializeField, LabelText("不显示血条的等待受击时长"), TitleGroup("头上HUD/===Config===")]
		public float _waitTimeToHideHPBar = 2f;
		
		
		public Material GetMaterialTypeByBuffType(RolePlay_BuffTypeEnum type)
		{
			int findI = BuffIconMaterialArguInfoList.FindIndex((argu) => argu.Type == type);
			if (findI == -1)
			{
				return null;
			}
			return BuffIconMaterialArguInfoList[findI].TargetMaterial;
		}


		
		
		public UIRW_EnemyAboveHUDItem GetFreeAboveHUDItem()
		{

			for (int i = _selfFreeEnemyHUDItemList.Count - 1; i >= 0; i--)
			{
				var tmpHUDItem = _selfFreeEnemyHUDItemList[i];
				if (!tmpHUDItem.gameObject.activeInHierarchy)
				{
					_selfFreeEnemyHUDItemList.RemoveAt(i);
					_selfBusyEnemyHUDItemList.Add(tmpHUDItem);

					return tmpHUDItem;
				}
			}
			var newP = CreateNewAboveHUDItem();
			newP.transform.SetParent(_rootGO.transform);

			_selfBusyEnemyHUDItemList.Add(newP);
			return newP;
		}


		private UIRW_EnemyAboveHUDItem CreateNewAboveHUDItem()
		{
			var tmpNewHUDItem = Instantiate(_sf_EnemyHUDItemPrefab, _rootGO.transform);
			tmpNewHUDItem.gameObject.SetActive(false);
			var newUIRW = tmpNewHUDItem.GetComponent<UIRW_EnemyAboveHUDItem>();
			newUIRW.InstantiateInitialize(this);
			return newUIRW;
		}

		//return
		public void ReturnAboveHUDItem(UIRW_EnemyAboveHUDItem targetHUDItem)
		{
			targetHUDItem.gameObject.SetActive(false);
			targetHUDItem.transform.SetParent(transform);
			if (_selfBusyEnemyHUDItemList.Contains(targetHUDItem))
			{
				_selfBusyEnemyHUDItemList.Remove(targetHUDItem);
			}
			if (!_selfFreeEnemyHUDItemList.Contains(targetHUDItem))
			{
				_selfFreeEnemyHUDItemList.Add(targetHUDItem);
			}
			
			
			
			
			
			
			
		}



		
		
		
		
		/// <summary>
		/// 到这一步说明是普通敌人或者精英敌人，内部再判断是啥
		/// </summary>
		/// <param name="enemy"></param>
		private void _GenerateEnemyStateGroupHUD(EnemyARPGCharacterBehaviour enemy)
		{
			//处于COM的通用无视列表中，那就return
			if (_comRef._DefaultIgnoreEnemyTypeList.Contains(enemy.SelfBehaviourNamedType))
			{
				return;
			}

			UIRW_EnemyAboveHUDItem aboveHUD = GetFreeAboveHUDItem();
			aboveHUD.SetRelatedEnemyRefAndInitializeFunction(enemy);

		}


#region  的  Buff图标们




		[ShowInInspector, LabelText("当前空闲的Buff Icon UIRW"), FoldoutGroup("运行时", true)]
		private List<UIRW_SingleBuffIconOnEnemyHUD> _selfFreeBuffIconUIRWList =
			new List<UIRW_SingleBuffIconOnEnemyHUD>();

		[ShowInInspector, LabelText("当前忙碌的Buff Icon UIRW"), FoldoutGroup("运行时", true)]
		private List<UIRW_SingleBuffIconOnEnemyHUD> _selfBusyBuffIconUIRWList =
			new List<UIRW_SingleBuffIconOnEnemyHUD>();



		public UIRW_SingleBuffIconOnEnemyHUD GetFreeBuffIcon()
		{
			for (int i = _selfFreeBuffIconUIRWList.Count - 1; i >= 0; i--)
			{
				UIRW_SingleBuffIconOnEnemyHUD tmpHUDItem = _selfFreeBuffIconUIRWList[i];
				if (!tmpHUDItem.gameObject.activeInHierarchy)
				{
					_selfFreeBuffIconUIRWList.RemoveAt(i);
					_selfBusyBuffIconUIRWList.Add(tmpHUDItem);
					tmpHUDItem.transform.localRotation = Quaternion.Euler(
						BaseMainCameraBehaviour.BaseInstance.CurrentCharacterPitchAngle,
						BaseMainCameraBehaviour.BaseInstance.CurrentCharacterYawAngle,
						0f);
					return tmpHUDItem;
				}
			}
			var newP = CreateNewBuffIcon();
			_selfBusyBuffIconUIRWList.Add(newP);
			return newP;
		}


		private UIRW_SingleBuffIconOnEnemyHUD CreateNewBuffIcon()
		{
			var tmpNewBuffIcon = Instantiate(_sf_EnemyBuffIconPrefab, _rootGO.transform);
			tmpNewBuffIcon.gameObject.SetActive(false);
			var newUIRW = tmpNewBuffIcon.GetComponent<UIRW_SingleBuffIconOnEnemyHUD>();
			return newUIRW;
		}

		public void ReturnBuffIconUIRW(UIRW_SingleBuffIconOnEnemyHUD icon)
		{
			icon.gameObject.SetActive(false);
			icon.gameObject.transform.SetParent(transform);
			if (_selfBusyBuffIconUIRWList.Contains(icon))
			{
				_selfBusyBuffIconUIRWList.Remove(icon);
			}
			
			if (!_selfFreeBuffIconUIRWList.Contains(icon))
			{
				_selfFreeBuffIconUIRWList.Add(icon);
			}
			
		}


#endregion
		
		
		

#endregion
		
		
		
		
		

#region 一级元素标签组HUD

		[TitleGroup("元素组"),SerializeField]
		[TitleGroup("元素组/===prefab===")]
		[LabelText("prefab-单个元素组prefab"), Required, AssetsOnly]
		private GameObject _sf_SingleElementGroupPrefab;

		[TitleGroup("元素组/===config===")]
		[LabelText("sprite-电标签")]
		public Sprite _sprite_electric_dian;

		[TitleGroup("元素组/===config===")]
		[LabelText("sprite-光标签")]
		public Sprite _sprite_light_guang;
		
		
		[TitleGroup("元素组/===config===")]
		[LabelText("sprite-火标签")]
		public Sprite _sprite_fire_huo;
		
		[TitleGroup("元素组/===config===")]
		[LabelText("sprite-水标签")]
		public Sprite _sprite_water_shui;

		[TitleGroup("元素组/===config===")]
		[LabelText("sprite-风标签")]
		public Sprite _sprite_wind_feng;
		
		[TitleGroup("元素组/===config===")]
		[LabelText("sprite-土标签")]
		public Sprite _sprite_earth_tu;
		
		[TitleGroup("元素组/===config===")]
		[LabelText("sprite-灵标签")]
		public Sprite _sprite_spirit_ling;


		[TitleGroup("元素组/===config===")]
		[LabelText("元素组检测翻面的帧数间隔")]
		public int _elementGroupCheckFlipFrameInterval = 3;

		[TitleGroup("元素组/===config===")]
		[LabelText("元素组整体缩放 - 额外乘算")]
		public float _config_elementGroupScale = 1.5f;

		[TitleGroup("元素组/===config===")]
		[LabelText("元素组 竖直方向距离的额外乘算")]
		public float _config_elementGroupHeightMul = 1.1f;

		[TitleGroup("元素组/===config===")]
		[LabelText("元素组 水平方向距离的额外乘算")]
		public float _config_elementGroupWidthMul = 1.5f;


		private List<UIRW_PerEnemyElementGroupHUD> _currentActiveElementGroupList =
			new List<UIRW_PerEnemyElementGroupHUD>();
		
		private List<UIRW_PerEnemyElementGroupHUD> _currentFreeElementGroupList =
			new List<UIRW_PerEnemyElementGroupHUD>();


		private UIRW_PerEnemyElementGroupHUD GetFreeElementGroupUIRW()
		{
			for (int i = _currentFreeElementGroupList.Count - 1; i >= 0; i--)
			{
				UIRW_PerEnemyElementGroupHUD tmpHUDItem = _currentFreeElementGroupList[i];
				if (!tmpHUDItem.gameObject.activeInHierarchy)
				{
					_currentFreeElementGroupList.RemoveAt(i);
					_currentActiveElementGroupList.Add(tmpHUDItem);
					tmpHUDItem.gameObject.SetActive(true);
					return tmpHUDItem;
				}
			}
			var newP = CreateNewElementGroupUIRW();
			_currentActiveElementGroupList.Add(newP);
			return newP;
		}

		private UIRW_PerEnemyElementGroupHUD CreateNewElementGroupUIRW()
		{
			var tmpNewElementGroup = Instantiate(_sf_SingleElementGroupPrefab, _rootGO.transform);
			tmpNewElementGroup.gameObject.SetActive(true);
			var newUIRW = tmpNewElementGroup.GetComponent<UIRW_PerEnemyElementGroupHUD>();
			_currentActiveElementGroupList.Add(newUIRW);
			return newUIRW;
		}



		private void _GenerateElementGroup(EnemyARPGCharacterBehaviour enemy)
		{
			if (_comRef._DefaultIgnoreEnemyTypeList.Contains(enemy.SelfBehaviourNamedType))
			{
				return;
			}

			UIRW_PerEnemyElementGroupHUD groupUIRW = GetFreeElementGroupUIRW();
			groupUIRW.InitializeWithRelateBehaviour(enemy, this);
			


		}


		public void _Internal_ReturnElementGroup(UIRW_PerEnemyElementGroupHUD uirw)
		{
			uirw.gameObject.SetActive(false);
			_currentActiveElementGroupList.Remove(uirw);
			_currentFreeElementGroupList.Add(uirw);
			uirw.transform.SetParent(_rootGO.transform);
		}



		private void _UpdateTick_ElementGroup(float ct, int cf, float delta)
		{
			foreach (var perGroup in _currentActiveElementGroupList)
			{
				perGroup.UpdateTick(ct, cf, delta);
			}
		}
#endregion


#region 击破 & 瓦解 提示条

		[LabelText("prefab-击破条"), SerializeField, Required, TitleGroup("击破条/===prefab===")]
		private GameObject _sf_WeaknessBarPrefab;

		List<UIRW_SingleEnemyWeaknessEntry> _list_currentActiveWeaknessBarUIRWList =
			new List<UIRW_SingleEnemyWeaknessEntry>();

		[LabelText("击破条尺寸曲线"), SerializeField, Required, TitleGroup("击破条/===Config===")]
		private AnimationCurve _weaknessBarSizeCurve;

		[LabelText("曲线X为0时表示的敌人到摄像机距离"), SerializeField, TitleGroup("击破条/===Config===")]
		private float _weaknessBarSizeCurveX0 = 10f;


		[LabelText("曲线X为1时表示的敌人到摄像机距离"), SerializeField, TitleGroup("击破条/===Config===")]
		private float _weaknessBarSizeCurveX1 = 30f;

		[LabelText("曲线Y为0时的击破条尺寸 "), SerializeField, TitleGroup("击破条/===Config===")]
		private float _weaknessBarSizeCurveY0 = 1.2f;

		[LabelText("曲线Y为1时的击破条尺寸 "), SerializeField, TitleGroup("击破条/===Config===")]
		private float _weaknessBarSizeCurveY1 = 0.75f;

		/// <summary>
		/// <para>响应对跟随式弱点条的需求。 常见于Boss的非【常驻计量条】的【机制计量条】</para>
		/// </summary>
		/// <param name="ds"></param>
		private void _ABC_ReactToWeaknessSliderRequirement(DS_ActionBusArguGroup ds)
		{
			var group = ds.GetObj1AsT<Buff_通用敌人弱点_CommonEnemyWeakness.WeaknessInfoGroup>();
			if (group.RequireUIDisplay)
			{
				//1开 0关
				if (ds.IntArgu1.Value == 1)
				{
					var uirw = CreateNewWeaknessBar(group);
				}
				else
				{
					var fi = _list_currentActiveWeaknessBarUIRWList.FindIndex((argu) => argu.RelatedGroupRef == group);
					if (fi != -1)
					{
						var uirw = _list_currentActiveWeaknessBarUIRWList[fi];
						_list_currentActiveWeaknessBarUIRWList.RemoveAt(fi);
						uirw.gameObject.SetActive(false);
						Destroy(uirw.gameObject);
					}
				}
			}
		}



		
		
		
		private UIRW_SingleEnemyWeaknessEntry CreateNewWeaknessBar(Buff_通用敌人弱点_CommonEnemyWeakness.WeaknessInfoGroup group)
		{
			var tmpNewWeaknessBar = Instantiate(_sf_WeaknessBarPrefab, _rootGO.transform);
			var newUIRW = tmpNewWeaknessBar.GetComponent<UIRW_SingleEnemyWeaknessEntry>();
			newUIRW.InitializeOnInstantiate(group);
			newUIRW.gameObject.SetActive(false);
			_list_currentActiveWeaknessBarUIRWList.Add(newUIRW);

			return newUIRW;
		}
		
		
		/// <summary>
		/// <para>为已经活跃着的计量条，计算UI上的跟随位置。计量条本身的计数是PerUIRW的Tick</para>
		/// </summary>
		/// <param name="currentTime"></param>
		/// <param name="currentFrameCount"></param>
		/// <param name="deltaTime"></param>
		private void UpdateTick_Weakness( float currentTime, int currentFrameCount, float deltaTime)
		{
			
			foreach (UIRW_SingleEnemyWeaknessEntry perUIRW in _list_currentActiveWeaknessBarUIRWList)
			{
				perUIRW.UpdateTick(currentTime, currentFrameCount, deltaTime);
				
				
				if (perUIRW.isActiveAndEnabled)
				{
					//校准世界位置
					Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(GameReferenceService_ARPG.Instance.CameraBehaviourRef.MainCamera,
						perUIRW.RelatedEnemyBehaviourRef._WeaknessMatchingGameObject.transform.position);
					
					var posInViewport = BaseMainCameraBehaviour.BaseInstance.UICamera.ScreenToViewportPoint(screenPos);
					var pos = ViewportToCanvasPosition(_selfCanvas, _selfCanvasRect, posInViewport);
					perUIRW.transform.localPosition = pos;


					float distanceToCamera =
						Vector3.Distance(
							GameReferenceService_ARPG.Instance.CameraBehaviourRef.MainCamera.transform.position,
							perUIRW.RelatedEnemyBehaviourRef._WeaknessMatchingGameObject.transform.position);
					
					
					 //evaluate to curve
                        float curveValue = _weaknessBarSizeCurve.Evaluate(
                            Mathf.InverseLerp(_weaknessBarSizeCurveX0, _weaknessBarSizeCurveX1, distanceToCamera));
					 						float size = Mathf.Lerp(_weaknessBarSizeCurveY0, _weaknessBarSizeCurveY1, curveValue);
										    perUIRW.transform.localScale = Vector3.one * size;


				}
			}
				
			
			
			
			
			
		}

#endregion
	}
	
}