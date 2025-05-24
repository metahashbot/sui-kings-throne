using System;
using System.Collections.Generic;
using ARPG.Character;
using ARPG.Manager;
using DG.Tweening;
using Global;
using Global.ActionBus;
using Global.UIBase;
using RPGCore.Buff;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.ConcreteBuff;
using RPGCore.Buff.ConcreteBuff.Common;
using RPGCore.Buff.Config;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ARPG.UI.Panel
{
	public class UIP_PlayerBuffPanel : UI_UIBasePanel
	{


		protected PlayerARPGConcreteCharacterBehaviour _currentPlayerBehaviourRef;

#region 资源

		[TitleGroup("===侧边栏===")]
		[SerializeField, LabelText("sprite-正面buff项底板"), TitleGroup("===侧边栏===/Asset"), Required]
		public Sprite _sprite_PositiveBuffEntryBackground;
		[SerializeField, LabelText("sprite-负面buff项底板")] [TitleGroup("===侧边栏===/Asset"), Required]
		public Sprite _sprite_NegativeBuffEntryBackground;
		[SerializeField, LabelText("sprite-中性buff项底板")] [TitleGroup("===侧边栏===/Asset"), Required]
		public Sprite _sprite_NeutralBuffEntryBackground;

#endregion

#region 侧边栏部分

		[SerializeField, LabelText("整体左侧Layout尺寸")] [TitleGroup("===侧边栏===/Widget")]
		private float _leftLayoutSize = 1f;

		[SerializeField, LabelText("layout_左侧整体")] [TitleGroup("===侧边栏===/Widget")]
		private LayoutGroup _layout_LeftSide;



		[SerializeField, Required, LabelText("layout_中性Buff布局")] [TitleGroup("===侧边栏===/Widget")]
		private LayoutGroup _layout_BuffGroup_NeutralBuffLayout;

		private List<UIRW_PerPlayerBuffEntry> _list_BuffEntryList_Neutral = new List<UIRW_PerPlayerBuffEntry>();


		[LabelText("layout_个人正面Buff布局"), SerializeField, Required] [TitleGroup("===侧边栏===/Widget")]
		private LayoutGroup _layout_BuffGroup_SelfPositive;

		private List<UIRW_PerPlayerBuffEntry> _list_BuffEntryList_SelfPositive = new List<UIRW_PerPlayerBuffEntry>();



		[LabelText("layout_个人负面Buff布局"), SerializeField, Required] [TitleGroup("===侧边栏===/Widget")]
		private LayoutGroup _layout_BuffGroup_SelfNegative;

		private List<UIRW_PerPlayerBuffEntry> _list_BuffEntryList_SelfNegative = new List<UIRW_PerPlayerBuffEntry>();


		[LabelText("prefab-单项Buff条目"), SerializeField, Required] [TitleGroup("===侧边栏===/Prefab")]
		private GameObject UIRW_PerPlayerBuffEntryPrefab;



		private UIRW_PerPlayerBuffEntry GetNewEntry()
		{
			var newObj = UnityEngine.Object.Instantiate(UIRW_PerPlayerBuffEntryPrefab, _rootGO.transform);
			var newUIRW = newObj.GetComponent<UIRW_PerPlayerBuffEntry>();
			newUIRW.gameObject.SetActive(true);
			return newUIRW;
		}


		/// <summary>
		/// <para>根据当前活跃的角色调整buff选项。</para>
		/// <para>实际上所有的buff项都是在容器里的，只是根据当前活跃的角色来区别它显不显示</para>
		/// </summary>
		private void CheckAndToggleBuffEntryOnActivePlayer()
		{
			// foreach (UIRW_PerPlayerBuffEntry perUIRW in _list_BuffEntryList_Neutral)
			// {
			// 	if (!perUIRW.gameObject.activeSelf)
			// 	{
			// 		perUIRW.gameObject.SetActive(true);
			// 	}
			// }
			if (_currentPlayerBehaviourRef == null)
			{
				return;
			}
			foreach (UIRW_PerPlayerBuffEntry perUIRW in _list_BuffEntryList_SelfNegative)
			{
				if (perUIRW.RelatedPlayerBehaviourRef &&
				    perUIRW.RelatedPlayerBehaviourRef == _currentPlayerBehaviourRef)
				{
					perUIRW.gameObject.SetActive(true);
					perUIRW.RefreshContent();
				}
				else
				{
					perUIRW.gameObject.SetActive(false);
				}
			}
			foreach (var perUIRW in _list_BuffEntryList_SelfPositive)
			{
				if (perUIRW.RelatedPlayerBehaviourRef &&
				    perUIRW.RelatedPlayerBehaviourRef == _currentPlayerBehaviourRef)
				{
					perUIRW.gameObject.SetActive(true);
					perUIRW.RefreshContent();
				}
				else
				{
					perUIRW.gameObject.SetActive(false);
				}
			}
			foreach (UIRW_PerPlayerBuffEntry perUIRW in _list_BuffEntryList_Neutral)
			{
				if (perUIRW.RelatedPlayerBehaviourRef &&
				    perUIRW.RelatedPlayerBehaviourRef == _currentPlayerBehaviourRef)
				{
					perUIRW.gameObject.SetActive(true);
					perUIRW.RefreshContent();
				}
				else
				{
					perUIRW.gameObject.SetActive(false);
				}
			}
		}

#endregion


#region 角色左右部分

		/*
		 * 动画过程是 Holder(UIRW)本身直接生成在那，然后UIRW内部跑一个DoTween划上去然后消失
		 * Holder跟随Tick
		 */

		[TitleGroup("===玩家跟随===")]
		[SerializeField, LabelText("中间显示Canvas的X偏移")]
		[TitleGroup("===玩家跟随===/Config")]
		private float _sideCanvasXOffset = 150f;


		[SerializeField, LabelText("中间显示Canvas的Y偏移")]
		[TitleGroup("===玩家跟随===/Config")]
		private float _sideViewportYOffset = 80f;

		[SerializeField, LabelText("弹出UIRW的尺寸缩放")]
		[TitleGroup("===玩家跟随===/Config")]
		private float _popupSize = 0.35f;


		[SerializeField, LabelText("负面buff的Prefab")]
		[TitleGroup("===玩家跟随===/Prefab")]
		private GameObject _prefab_NegativeBuffUIRW;
		private List<UIRW_MiddlePopupBuffHint_PlayerBuffPanel> _list_currentPopupHintLeft =
			new List<UIRW_MiddlePopupBuffHint_PlayerBuffPanel>();
		private UIRW_MiddlePopupBuffHint_PlayerBuffPanel GetFree_Left()
		{
			foreach (UIRW_MiddlePopupBuffHint_PlayerBuffPanel perUIRW in _list_currentPopupHintLeft)
			{
				if (!perUIRW.gameObject.activeSelf)
				{
					perUIRW.gameObject.SetActive(true);
					return perUIRW;
				}
			}

			var newObj = UnityEngine.Object.Instantiate(_prefab_NegativeBuffUIRW, _rootGO.transform);
			newObj.transform.localScale = Vector3.one * _popupSize;
			var newUIRW = newObj.GetComponent<UIRW_MiddlePopupBuffHint_PlayerBuffPanel>();
			_list_currentPopupHintLeft.Add(newUIRW);
			return newUIRW;
		}

		[SerializeField, LabelText("正面buff的Prefab")]
		[TitleGroup("===玩家跟随===/Prefab")]
		private GameObject _prefab_PositiveBuffUIRW;

		private List<UIRW_MiddlePopupBuffHint_PlayerBuffPanel> _list_currentPopupHintRight =
			new List<UIRW_MiddlePopupBuffHint_PlayerBuffPanel>();

		private UIRW_MiddlePopupBuffHint_PlayerBuffPanel GetFree_Right()
		{
			foreach (UIRW_MiddlePopupBuffHint_PlayerBuffPanel perUIRW in _list_currentPopupHintRight)
			{
				if (!perUIRW.gameObject.activeInHierarchy)
				{
					perUIRW.gameObject.SetActive(true);
					return perUIRW;
				}
			}

			var newObj = UnityEngine.Object.Instantiate(_prefab_PositiveBuffUIRW, _rootGO.transform);
			newObj.transform.localScale = Vector3.one * _popupSize;
			var newUIRW = newObj.GetComponent<UIRW_MiddlePopupBuffHint_PlayerBuffPanel>();
			_list_currentPopupHintRight.Add(newUIRW);
			return newUIRW;
		}


		[SerializeField, LabelText("高度 出现过程时长")]
		[TitleGroup("===玩家跟随===/Config")]
		[HorizontalGroup("===玩家跟随===/Config/1")]
		public float _height_in_duration = 0.5f;
		[SerializeField, LabelText("高度 出现过程 距离")]
		[TitleGroup("===玩家跟随===/Config")]
		[HorizontalGroup("===玩家跟随===/Config/1")]
		public float _height_in_offset = 120f;
		[SerializeField, LabelText("高度 出现过程 缓动类型")]
		[TitleGroup("===玩家跟随===/Config")]
		[HorizontalGroup("===玩家跟随===/Config/1")]
		public Ease _height_in_ease = Ease.InCubic;

		[SerializeField, LabelText("高度 滞留 时长")]
		[TitleGroup("===玩家跟随===/Config")]
		public float _heightDurationStay = 0.5f;

		[SerializeField, LabelText("高度 消失过程 时长")]
		[TitleGroup("===玩家跟随===/Config")]
		[HorizontalGroup("===玩家跟随===/Config/2")]
		public float _height_out_duration = 0.35f;
		[SerializeField, LabelText("高度 消失过程 距离")]
		[TitleGroup("===玩家跟随===/Config")]
		[HorizontalGroup("===玩家跟随===/Config/2")]
		public float _height_out_Offset = 120f;
		[SerializeField, LabelText("高度 消失过程 缓动类型")]
		[TitleGroup("===玩家跟随===/Config")]
		[HorizontalGroup("===玩家跟随===/Config/2")]
		public Ease _height_out_ease = Ease.OutCubic;



		[SerializeField, LabelText("颜色 出现过程时长")]
		[TitleGroup("===玩家跟随===/Config")]
		[HorizontalGroup("===玩家跟随===/Config/3")]
		public float _color_in_duration = 0.5f;
		[SerializeField, LabelText("颜色 出现过程 起始透明度")]
		[TitleGroup("===玩家跟随===/Config")]
		[HorizontalGroup("===玩家跟随===/Config/3")]
		public float _color_in_alpha = 0.3f;
		[SerializeField, LabelText("颜色 出现过程 缓动类型")]
		[TitleGroup("===玩家跟随===/Config")]
		[HorizontalGroup("===玩家跟随===/Config/3")]
		public Ease _color_in_ease = Ease.InCubic;

		[SerializeField, LabelText("颜色 滞留 时长")]
		[TitleGroup("===玩家跟随===/Config")]
		public float _colorDurationStay = 0.5f;

		[SerializeField, LabelText("颜色 消失过程 时长")]
		[TitleGroup("===玩家跟随===/Config")]
		[HorizontalGroup("===玩家跟随===/Config/4")]
		public float _color_out_duration = 0.35f;
		[SerializeField, LabelText("颜色 消失过程 目标透明度")]
		[TitleGroup("===玩家跟随===/Config")]
		[HorizontalGroup("===玩家跟随===/Config/4")]
		public float _color_out_alpha = 0.5f;
		[SerializeField, LabelText("颜色 消失过程 缓动类型")]
		[TitleGroup("===玩家跟随===/Config")]
		[HorizontalGroup("===玩家跟随===/Config/4")]
		public Ease _color_out_ease = Ease.OutCubic;


		private UnityEngine.Camera _currentMainCamera;


		/// <summary>
		/// <para>会判断是正面还是负面，然后在中间弹一下</para>
		/// </summary>
		/// <param name="ds"></param>
		private void _Internal_ProcessBuffHint_MiddlePart(DS_ActionBusArguGroup ds)
		{
			I_BuffContentMayDisplayOnUI I_DisplayRef = ds.ObjectArgu1 as I_BuffContentMayDisplayOnUI;
			var content = ds.ObjectArgu2 as ConcreteBuffDisplayOnUIInfo;
			var fe_sprite = GlobalConfigurationAssetHolderHelper.GetGCAHH().FE_GeneralSpriteResource;
			Sprite sprite = I_DisplayRef.RelatedBuffDisplayOnUIInfo.GetIconSprite();


			switch (content.BuffUIType)
			{
				case BuffUITypeEnum.SelfPositiveBuff_个人正面Buff:

					var uirw = GetFree_Right();
					uirw.InstantiateInitialize(sprite, this);
					break;
				case BuffUITypeEnum.SelfNegativeBuff_个人负面Buff:
					var uirw2 = GetFree_Left();
					uirw2.InstantiateInitialize(sprite, this);
					break;
			}
		}


		public override void LateUpdateTick(float currentTime, int currentFrameCount, float deltaTime)
		{
			Vector3 vp_Player = _currentMainCamera.WorldToViewportPoint(_currentPlayerBehaviourRef.transform.position);
			var aligned = GetLocalPositionOfAligned(_currentPlayerBehaviourRef.transform.position, false);
			aligned.x -= _sideCanvasXOffset;
			aligned.y += _sideViewportYOffset;


			foreach (UIRW_MiddlePopupBuffHint_PlayerBuffPanel perLeft in _list_currentPopupHintLeft)
			{
				if (perLeft.gameObject.activeInHierarchy)
				{
					perLeft.transform.localPosition = aligned;
				}
			}



			aligned.x += (2 * _sideCanvasXOffset);
			foreach (UIRW_MiddlePopupBuffHint_PlayerBuffPanel perRight in _list_currentPopupHintRight)
			{
				if (perRight.gameObject.activeInHierarchy)
				{
					perRight.transform.localPosition = aligned;
				}
			}
		}

#endregion


		public override void StartInitializeByUIM()
		{
			base.StartInitializeByUIM();
			_layout_LeftSide.transform.localScale = Vector3.one * _leftLayoutSize;
		}

		public override void LateInitializeByUIM()
		{
			base.LateInitializeByUIM();
			_currentMainCamera = GameReferenceService_ARPG.Instance.CameraBehaviourRef.MainCamera;
		}
		protected override void BindingEvent()
		{
			//绑定以下事件
			base.BindingEvent();
			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_PC_OnCurrentUsingCharacterChanged_当前使用的角色更换,
				_ABC_ProcessDisplayBuff_OnUsingCharacterChanged , -10);

			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_UI_RequireBuffDisplayContent_要求Buff显示内容,
				_ABC_ReactToBuffDisplayRequirement);
			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_UI_BuffDisplayContentClear_清理Buff显示内容,
				_ABC_ReactToBuffDisplayClear);
			
		}

		private void _ABC_ProcessDisplayBuff_OnUsingCharacterChanged(DS_ActionBusArguGroup ds)
		{
			_currentPlayerBehaviourRef = ds.ObjectArgu1 as PlayerARPGConcreteCharacterBehaviour;

			CheckAndToggleBuffEntryOnActivePlayer();
		}

		private void _ABC_ReactToBuffDisplayRequirement(DS_ActionBusArguGroup ds)
		{
			_Internal_ProcessBuffHint_MiddlePart(ds);
			I_BuffContentMayDisplayOnUI I_DisplayRef = ds.ObjectArgu1 as I_BuffContentMayDisplayOnUI;
			var content = ds.ObjectArgu2 as ConcreteBuffDisplayOnUIInfo;
		
			//如果列表中已经有了，就不会显示了
			switch (content.BuffUIType)
			{
				case BuffUITypeEnum.NeutralBuff_个人中性Buff:
					var find_1 = _list_BuffEntryList_Neutral.FindIndex((uirw =>
						ReferenceEquals(uirw.RelatedBuffRef, I_DisplayRef)));
					if (find_1 != -1)
					{
						_RefreshLeftSideContent_CheckDisplay();
						return;
					}
					var uirw_neutral = GetNewUIRW(_layout_BuffGroup_NeutralBuffLayout.transform, I_DisplayRef);
					I_DisplayRef.SelfUIRW_BuffEntryRef = uirw_neutral;
					_list_BuffEntryList_Neutral.Add(uirw_neutral);
					_RefreshLeftSideContent_CheckDisplay();
					break;
				case BuffUITypeEnum.SelfPositiveBuff_个人正面Buff:
					var find_2= _list_BuffEntryList_SelfPositive.FindIndex((uirw =>
						ReferenceEquals(uirw.RelatedBuffRef, I_DisplayRef)));
					
					if (find_2 !=-1)
					{
						_RefreshLeftSideContent_CheckDisplay();
						return;
					}
					var uirw_selfPositive = GetNewUIRW(_layout_BuffGroup_SelfPositive.transform, I_DisplayRef);
					I_DisplayRef.SelfUIRW_BuffEntryRef = uirw_selfPositive;
					_list_BuffEntryList_SelfPositive.Add(uirw_selfPositive);

					_RefreshLeftSideContent_CheckDisplay();
					break;
				case BuffUITypeEnum.SelfNegativeBuff_个人负面Buff:
					var find = _list_BuffEntryList_SelfNegative.FindIndex((uirw =>
						ReferenceEquals(uirw.RelatedBuffRef, I_DisplayRef)));
					if (find !=-1)
					{
						_RefreshLeftSideContent_CheckDisplay();
						return;
					}
					var uirw_selfNegative = GetNewUIRW(_layout_BuffGroup_SelfNegative.transform, I_DisplayRef);
					_list_BuffEntryList_SelfNegative.Add(uirw_selfNegative);
					I_DisplayRef.SelfUIRW_BuffEntryRef = uirw_selfNegative;
					_RefreshLeftSideContent_CheckDisplay();
					break;
				case BuffUITypeEnum.EnemyShowBuff_敌人需要显示的Buff:
					break;
			}

			UIRW_PerPlayerBuffEntry GetNewUIRW(Transform layout, I_BuffContentMayDisplayOnUI iRef)
			{
				var freeUIRW = GetNewEntry();
				freeUIRW.transform.SetParent(layout);
				freeUIRW.InstantiateInitialize(iRef, this);
				
				return freeUIRW;
			}
		}


		/// <summary>
		/// <para>每当任何UI变动的时候，都直接刷新所有左侧buff容器，检查他们的开关</para>
		/// </summary>
		private void _RefreshLeftSideContent_CheckDisplay()
		{
			var currentBehaviour = SubGameplayLogicManager_ARPG.Instance.PlayerCharacterBehaviourControllerReference
				.CurrentControllingBehaviour;
			if (currentBehaviour == null)
			{
				return;
			}
			foreach (UIRW_PerPlayerBuffEntry perUIRW in _list_BuffEntryList_SelfNegative)
			{
				if (perUIRW.RelatedPlayerBehaviourRef &&
				    perUIRW.RelatedPlayerBehaviourRef == currentBehaviour)
				{
					perUIRW.gameObject.SetActive(true);
					perUIRW.RefreshContent();
				}
				else if (perUIRW .RelatedBuffRef is I_BuffTransferWithinPlayer)
				{
					perUIRW.gameObject.SetActive(true);
				}
				else
				{
					perUIRW.gameObject.SetActive(false);
				}
			}
			foreach (var perUIRW in _list_BuffEntryList_SelfPositive)
			{
				if (perUIRW.RelatedPlayerBehaviourRef &&
				    perUIRW.RelatedPlayerBehaviourRef == currentBehaviour)
				{
					perUIRW.gameObject.SetActive(true);
					perUIRW.RefreshContent();
				}
				else if (perUIRW.RelatedBuffRef is I_BuffTransferWithinPlayer)
				{
					perUIRW.gameObject.SetActive(true);
				}
				else
				{
					perUIRW.gameObject.SetActive(false);
				}
			}
			foreach (UIRW_PerPlayerBuffEntry perUIRW in _list_BuffEntryList_Neutral)
			{
				if (perUIRW.RelatedPlayerBehaviourRef &&
				    perUIRW.RelatedPlayerBehaviourRef == currentBehaviour)
				{
					perUIRW.gameObject.SetActive(true);
					perUIRW.RefreshContent();
				}
				else if (perUIRW.RelatedBuffRef is I_BuffTransferWithinPlayer)
				{
					perUIRW.gameObject.SetActive(true);
				}
				else
				{
					perUIRW.gameObject.SetActive(false);
				}
			}
			

		}



		private void _ABC_ReactToBuffDisplayClear(DS_ActionBusArguGroup ds)
		{
			I_BuffContentMayDisplayOnUI I_DisplayRef = ds.ObjectArgu1 as I_BuffContentMayDisplayOnUI;
			var content = ds.ObjectArgu2 as ConcreteBuffDisplayOnUIInfo;
			if (content == null)
			{
				return;
			}
			switch (content.BuffUIType)
			{
				case BuffUITypeEnum.NeutralBuff_个人中性Buff:
					for (int i = _list_BuffEntryList_Neutral.Count - 1; i >= 0; i--)
					{
						UIRW_PerPlayerBuffEntry tmp = _list_BuffEntryList_Neutral[i];
						if (tmp == I_DisplayRef.SelfUIRW_BuffEntryRef)
						{
							_list_BuffEntryList_Neutral.Remove(tmp);
							UnityEngine.Object.Destroy(tmp.gameObject);
						}
					}
					_RefreshLeftSideContent_CheckDisplay();
					break;
				case BuffUITypeEnum.SelfPositiveBuff_个人正面Buff:
					for (int i = _list_BuffEntryList_SelfPositive.Count - 1; i >= 0; i--)
					{
						UIRW_PerPlayerBuffEntry tmp = _list_BuffEntryList_SelfPositive[i];
						if (tmp == I_DisplayRef.SelfUIRW_BuffEntryRef)
						{
							_list_BuffEntryList_SelfPositive.Remove(tmp);
							UnityEngine.Object.Destroy(tmp.gameObject);
						}
					}
					_RefreshLeftSideContent_CheckDisplay();
					break;
				case BuffUITypeEnum.SelfNegativeBuff_个人负面Buff:
					for (int i = _list_BuffEntryList_SelfNegative.Count - 1; i >= 0; i--)
					{
						UIRW_PerPlayerBuffEntry tmp = _list_BuffEntryList_SelfNegative[i];
						if (tmp == I_DisplayRef.SelfUIRW_BuffEntryRef)
						{
							_list_BuffEntryList_SelfNegative.Remove(tmp);
							UnityEngine.Object.Destroy(tmp.gameObject);
						}
					}
					_RefreshLeftSideContent_CheckDisplay();
					break;
				case BuffUITypeEnum.EnemyShowBuff_敌人需要显示的Buff:
					break;
			}
		}





		public override void UpdateTick(float currentTime, int currentFrameCount, float deltaTime)
		{
			base.UpdateTick(currentTime, currentFrameCount, deltaTime);
			foreach (UIRW_PerPlayerBuffEntry perUIRW in _list_BuffEntryList_Neutral)
			{
				perUIRW.UpdateTick(currentTime, currentFrameCount, deltaTime);
			}

			foreach (UIRW_PerPlayerBuffEntry perUIRW in _list_BuffEntryList_SelfNegative)
			{
				perUIRW.UpdateTick(currentTime, currentFrameCount, deltaTime);
			}

			foreach (UIRW_PerPlayerBuffEntry perUIRW in _list_BuffEntryList_SelfPositive)
			{
				perUIRW.UpdateTick(currentTime, currentFrameCount, deltaTime);
			}
		}




	}
}