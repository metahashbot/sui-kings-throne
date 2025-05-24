using System;
using System.Collections;
using System.Collections.Generic;
using ARPG.Common;
using ARPG.Equipment;
using ARPG.Manager;
using Global;
using Global.ActionBus;
using Global.AreaOnMap.Config;
using Global.GlobalConfig;
using Global.Loot;
using Global.UI;
using Global.UIBase;
using Global.Utility;
using RegionMap.LevelSelect.LevelDescComponent;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;
namespace ARPG.UI.Panel.BattleConclusion
{

	public class UIP_BattleConclusionPanel : UI_UIBasePanel
	{
		[LabelText("holder_成功结算"), SerializeField, TitleGroup("===Widget==="), Required]
		protected RectTransform _holder_SuccessConclusionHolder;
		[LabelText("Text_具体通关时间"), SerializeField, TitleGroup("===Widget==="), Required]
		protected TextMeshProUGUI _text_LevelClearTime;
		[LabelText("image_通关时间评分图片"), SerializeField, TitleGroup("===Widget==="), Required]
		protected Image _image_LevelClearTimeEvaluate;
		[LabelText("text_具体击杀数量"), SerializeField, TitleGroup("===Widget==="), Required]
		protected TextMeshProUGUI _text_KillCount;
		[LabelText("image_击杀数量评分图片"), SerializeField, TitleGroup("===Widget==="), Required]
		protected Image _image_KillCountEvaluate;
		[LabelText("text_受伤数量"), SerializeField, TitleGroup("===Widget==="), Required]
		protected TextMeshProUGUI _text_HurtCount;
		[LabelText("image_受伤数量评分图片"), SerializeField, TitleGroup("===Widget==="), Required]
		protected Image _image_HurtCountEvaluate;
		[LabelText("text_评价点数"), SerializeField, TitleGroup("===Widget==="), Required]
		protected TextMeshProUGUI _text_EvaluatePoint;
		[LabelText("image_评价点数评分图片"), SerializeField, TitleGroup("===Widget==="), Required]
		protected Image _image_EvaluatePointEvaluate;
		[LabelText("button_返回世界地图"), SerializeField, TitleGroup("===Widget==="), Required]
		protected Button _button_backToWorldMap;
		[LabelText("HL_通关奖励布局"), SerializeField, TitleGroup("===Widget==="), Required]
		protected HorizontalLayoutGroup _HL_PassRewardLayout;
        [LabelText("HL_掉落物品布局"), SerializeField, TitleGroup("===Widget==="), Required]
        protected HorizontalLayoutGroup _HL_DropItemLayout;
		[LabelText("holder_失败结算"), SerializeField, TitleGroup("===Widget==="), Required]
		protected RectTransform _holder_FailedConclusionHolder;
		[LabelText("button_返回世界地图_失败"), SerializeField, TitleGroup("===Widget==="), Required]
		protected Button _button_backToWorldMapFailed;
        [LabelText("prefab_单个掉落物"), SerializeField, TitleGroup("===Prefab==="), Required]
		protected GameObject _prefab_perLootEntry;
        [LabelText("sprite_S"), SerializeField, TitleGroup("===Sprite==="), Required]
        protected Sprite sprite_S;
        [LabelText("sprite_A"), SerializeField, TitleGroup("===Sprite==="), Required]
        protected Sprite sprite_A;
        [LabelText("sprite_B"), SerializeField, TitleGroup("===Sprite==="), Required]
        protected Sprite sprite_B;
        [LabelText("sprite_C"), SerializeField, TitleGroup("===Sprite==="), Required]
        protected Sprite sprite_C;
        [LabelText("sprite_D"), SerializeField, TitleGroup("===Sprite==="), Required]
        protected Sprite sprite_D;

		public override void StartInitializeByUIM()
		{
			base.StartInitializeByUIM();

			_button_backToWorldMap.onClick.RemoveAllListeners();
			_button_backToWorldMap.onClick.AddListener(_Button_BackToWorldMap);
			_button_backToWorldMapFailed.onClick.RemoveAllListeners();
			_button_backToWorldMapFailed.onClick.AddListener(_Button_BackToWorldMap);

			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_ConclusionAsAllExit_结算按照全员退场,
				_ABC_PopupGameOverMenu);
			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_GE_RequireLevelClearConclusion_要求进行通关结算,
				_ABC_ProcessBattleConclusion_OnRequiredConclusion);
		}

		public override void ShowThisPanel(bool clearShow = true, bool notBroadcast = false)
		{
			base.ShowThisPanel(clearShow);

			_holder_FailedConclusionHolder.gameObject.SetActive(false);
			_holder_SuccessConclusionHolder.gameObject.SetActive(false);
		}

        private void _Button_BackToWorldMap()
        {
            var lr = GlobalConfigurationAssetHolderHelper.Instance.RuntimeRecordHelper_Level;
            lr.GetLoadLevelHandle("索尔特里亚行省_东部", true, -1).FinallyLoad();
        }

        private void _ABC_PopupGameOverMenu(DS_ActionBusArguGroup ds)
        {
            ShowThisPanel();
            _holder_FailedConclusionHolder.gameObject.SetActive(true);

            var ds_pause = new DS_ActionBusArguGroup(
                ActionBus_ActionTypeEnum.G_Global_RequirePauseGame_要求暂停游戏);
            ds_pause.ObjectArgu1 = this;
            GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_pause);
        }

        /// <summary>
        /// <para>接受来自事件的结算要求时，进行结算。</para>
        /// <para>目前结算逻辑(挑选、修改存档等）是在这里进行的，如果需要可以迁移到其他地方去</para>
        /// </summary>
        /// <param name="ds"></param>
        private void _ABC_ProcessBattleConclusion_OnRequiredConclusion(DS_ActionBusArguGroup ds)
        {
            ShowThisPanel();
            _holder_SuccessConclusionHolder.gameObject.SetActive(true);

            //显示通关评价
            ShowEvaluatePoint();

            //显示通关奖励和掉落
            ShowDropItemAndShiftToInventory();
            ShowPassRewardAndShiftToInventory();
        }

        private void ShowEvaluatePoint()
        {
            var service = ActivityManager_ARPG.Instance.BattleConclusionServiceSubActivity;
            int time = (int)service.CostTime;
            _text_LevelClearTime.text = $"{(time / 60):D2}:{(time % 60):D2}";
            if(time <= 60)
            {
                _image_LevelClearTimeEvaluate.sprite = sprite_S;
            }
            else if (time <= 180)
            {
                _image_LevelClearTimeEvaluate.sprite = sprite_A;
            }
            else if (time <= 300)
            {
                _image_LevelClearTimeEvaluate.sprite = sprite_B;
            }
            else
            {
                _image_LevelClearTimeEvaluate.sprite = sprite_C;
            }

            int count = service.KillCount;
            _text_KillCount.text = $"{count}";
            _image_KillCountEvaluate.sprite = sprite_S;

            int damage = (int)service.TakenDamage;
            _text_HurtCount.text = $"{damage}";
            if (damage <= 1000)
            {
                _image_HurtCountEvaluate.sprite = sprite_S;
            }
            else if (damage <= 2000)
            {
                _image_HurtCountEvaluate.sprite = sprite_A;
            }
            else if (damage <= 3000)
            {
                _image_HurtCountEvaluate.sprite = sprite_B;
            }
            else
            {
                _image_HurtCountEvaluate.sprite = sprite_C;
            }

            _text_EvaluatePoint.text = "";
            _image_EvaluatePointEvaluate.sprite = sprite_A;
        }

        /// <summary>
        /// <para>展示临时背包中的掉落物品并加入长期背包</para>
        /// </summary>
        private void ShowDropItemAndShiftToInventory()
        {
            foreach (var item in GCSOExtend.GetTmpEquipmentList())
            {
                var newOBJ = Instantiate(_prefab_perLootEntry, _HL_DropItemLayout.transform);
                var newUIRW = newOBJ.GetComponent<UIRW_PerLootEntry_BattleConclusionPanel>();
                var info = GCAHHExtend.GetEquipmentRawInfo(item.EquipmentUID);
                Sprite iconSprite = info.IconSprite;
                Sprite borderSprite = BaseUIManager.QuickGetIconBorder(info.QualityType, true);
                newUIRW.InstantiateInitialize(iconSprite, borderSprite);
                GCSOExtend.AddEquipment(item);
            }

            foreach (var item in GCSOExtend.GetTmpItemList())
            {
                var newOBJ = Instantiate(_prefab_perLootEntry, _HL_DropItemLayout.transform);
                var newUIRW = newOBJ.GetComponent<UIRW_PerLootEntry_BattleConclusionPanel>();
                var info = GCAHHExtend.GetProperty(item.UID);
                Sprite iconSprite = info.IconSprite;
                Sprite borderSprite = BaseUIManager.QuickGetIconBorder(info.Quality, true);
                newUIRW.InstantiateInitialize(iconSprite, borderSprite, item.Count);
                GCSOExtend.AddItem(item.UID, item.Count);
            }

            GCSOExtend.GetTmpEquipmentList().Clear();
            GCSOExtend.GetTmpItemList().Clear();
        }

        /// <summary>
        /// <para>展示关卡通关的固定奖励，在关卡配置里配置</para>
        /// </summary>
        private void ShowPassRewardAndShiftToInventory()
		{
			int missionUID = FindAnyObjectByType<EditorProxy_AreaLogicHolder>().MissionUID;
			var generator = GCAHHExtend.GetDropConfig(
				SOFE_DropGroupInfo.DropSourceEnum.Mission_关卡掉落, missionUID);
			var dropItemDic = generator.GetDropItem();

            foreach (var itemPair in dropItemDic)
			{
				if (GCAHHExtend.IsWeapon(itemPair.Key) || GCAHHExtend.IsArmor(itemPair.Key))
                {
					for(int i = 0; i < itemPair.Value; i++)
                    {
                        var newOBJ = Instantiate(_prefab_perLootEntry, _HL_PassRewardLayout.transform);
                        var newUIRW = newOBJ.GetComponent<UIRW_PerLootEntry_BattleConclusionPanel>();
                        var info = GCAHHExtend.GetEquipmentRawInfo(itemPair.Key);
                        Sprite iconSprite = info.IconSprite;
                        Sprite borderSprite = BaseUIManager.QuickGetIconBorder(info.QualityType, true);
                        newUIRW.InstantiateInitialize(iconSprite, borderSprite);
                        GCSOExtend.AddEquipment(itemPair.Key);
                    }
                }
				else
                {
                    var newOBJ = Instantiate(_prefab_perLootEntry, _HL_PassRewardLayout.transform);
                    var newUIRW = newOBJ.GetComponent<UIRW_PerLootEntry_BattleConclusionPanel>();
                    var info = GCAHHExtend.GetProperty(itemPair.Key);
                    Sprite iconSprite = info.IconSprite;
                    Sprite borderSprite = BaseUIManager.QuickGetIconBorder(info.Quality, true);
                    newUIRW.InstantiateInitialize(iconSprite, borderSprite, itemPair.Value);
					GCSOExtend.AddItem(itemPair.Key, itemPair.Value);
                }
			}
		}

        private IEnumerator AutoBack()
        {
            yield return new WaitForSeconds(5f);
            _Button_BackToWorldMap();
        }
    }
}