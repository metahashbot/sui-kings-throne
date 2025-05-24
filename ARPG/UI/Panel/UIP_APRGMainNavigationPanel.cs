using System;
using System.Collections;
using ARPG.Manager;
using ARPG.UI.Panel.BattleConclusion;
using ARPG.UI.Panel.PlayerCharacter;
using Global;
using Global.ActionBus;
using Global.AssetLoad;
using Global.UIBase;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WorldMapScene;
using WorldMapScene.UI;
using WorldMapScene.UI.RenderingInfo;

namespace ARPG.UI.Panel
{
	[TypeInfoBox("顶部的那两个简单按钮")]
	public class UIP_APRGMainNavigationPanel : UI_UIBasePanel
	{
		[LabelText("button-显示玩家详情面板"), SerializeField, Required, TitleGroup("===Widget===")]
		private Button _Button_PlayerPanel;
        
		[LabelText("button-打开钱包"), SerializeField, Required, TitleGroup("===Widget===")]
        private Button _Wallet;

        [LabelText("Button-暂停选单"), SerializeField, Required, TitleGroup("===Widget===")]
		private Button _Button_PauseMenu;

		[SerializeField, LabelText("holder-暂停后内容"), Required, TitleGroup("===Widget===")]
		private RectTransform _Holder_PauseMenu;

		[LabelText("text_中心文本 "), SerializeField, Required, TitleGroup("===Widget===")]
		private TextMeshProUGUI _text_MainText;

		[SerializeField, LabelText("取消暂停"), Required, TitleGroup("===Widget===")]
		private Button _Button_Unpause;
		
		[SerializeField,LabelText("Button_更换输入方式"),Required,TitleGroup("===Widget===")]
		private Button _Button_ChangeInputType;
		
		[SerializeField,LabelText("text-更换输入方式") ,Required,TitleGroup("===Widget===")]
		private TextMeshProUGUI _text_ChangeInputMode;

		[SerializeField, LabelText("返回世界地图"), Required, TitleGroup("===Widget===")]
		private Button _Button_AbandonAndBack;
		
		public bool PanelRequirePause { get; private set; } = false;


		public override void StartInitializeByUIM()
		{
			base.StartInitializeByUIM();
			PanelRequirePause = false;

			_Holder_PauseMenu.gameObject.SetActive(false);
			_Button_PauseMenu.onClick.AddListener(OpenPauseMenu);
			_Button_Unpause.onClick.AddListener(UnpauseGame);
			_Button_PlayerPanel.onClick.AddListener(OpenPlayerPanel);
            _Wallet.onClick.AddListener(OpenWalletPanel);            

            _Button_AbandonAndBack.onClick.AddListener(ReturnToWorldMap);
			_Button_ChangeInputType.onClick.AddListener(ChangeInputType);

			GlobalActionBus.GetGlobalActionBus().RegisterAction(ActionBus_ActionTypeEnum.G_ConclusionAsAllExit_结算按照全员退场,
				_ABC_ProcessGameOver);
			switch (GlobalConfigurationAssetHolderHelper.GetCurrentLoadPhase())
			{
				case GeneralGameAssetLoadPhaseEnum.WorldMap:
				case GeneralGameAssetLoadPhaseEnum.RegionMap:
					_Button_PauseMenu.gameObject.SetActive(false);
					break;
				case GeneralGameAssetLoadPhaseEnum.ARPG:
					_Button_PauseMenu.gameObject.SetActive(true);
					break;
			}

			UpdateTextOnChangeInputType();
		}

		public override void ShowThisPanel(bool clearShow = true, bool notBroadcast = false)
		{
			base.ShowThisPanel(clearShow, notBroadcast);
			//根据GCSO中的纯键盘设置，来显示切换输入上的文本
			var gcso = GlobalConfigurationAssetHolderHelper.GetGCAHH().GlobalConfigSO_Runtime.Content;
			
			UpdateTextOnChangeInputType();
		
		}


        #region 角色面板

        private void OpenWalletPanel()
        {
            switch (GlobalConfigurationAssetHolderHelper.GetCurrentLoadPhase())
            {
                case GeneralGameAssetLoadPhaseEnum.WorldMap:
                case GeneralGameAssetLoadPhaseEnum.RegionMap:
                    GameReferenceService_WorldMap.Instance.UIManagerReference.GetPanel<UIP_Wallet>()
                        .ShowThisPanel();


                    break;
                case GeneralGameAssetLoadPhaseEnum.ARPG:
                    if (UIManager_ARPG.Instance != null)
                    {
                        UIManager_ARPG.Instance.GetPanel<UIP_Wallet>().ShowThisPanel();


                    }
                    break;
            }
        }

        private void OpenPlayerPanel()
		{
			switch (GlobalConfigurationAssetHolderHelper.GetCurrentLoadPhase())
			{
				case GeneralGameAssetLoadPhaseEnum.WorldMap:
				case GeneralGameAssetLoadPhaseEnum.RegionMap:
					GameReferenceService_WorldMap.Instance.UIManagerReference.GetPanel<UIP_PlayerCharacterFullPanel>()
						.ShowThisPanel();


					break;
				case GeneralGameAssetLoadPhaseEnum.ARPG:
					if (UIManager_ARPG.Instance != null)
					{
						UIManager_ARPG.Instance.GetPanel<UIP_PlayerCharacterFullPanel>().ShowThisPanel();


					}
					break;
			}
		}

#endregion




#region 暂停按钮

		private void OpenPauseMenu()
		{
			PanelRequirePause = true;
			_text_MainText.text = "暂停中";
			_Holder_PauseMenu.gameObject.SetActive(true);
			_Button_PauseMenu.interactable = false;
			var ds_pause = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_Global_RequirePauseGame_要求暂停游戏);
			ds_pause.ObjectArgu1 = this;
			GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_pause);
		}

#endregion

#region 暂停选单

		public void UnpauseGame()
		{
			PanelRequirePause = false;
			_Holder_PauseMenu.gameObject.SetActive(false);
			_Button_PauseMenu.interactable = true;
			var ds_pause = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_Global_RequireResumeGame_要求解除暂停);
			ds_pause.ObjectArgu1 = this;
			GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_pause);
		}

#endregion


#region GameOver时的界面


		private void _ABC_ProcessGameOver(DS_ActionBusArguGroup ds)
		{
			ShowThisPanel();

			var ds_pause = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_Global_RequirePauseGame_要求暂停游戏);
			ds_pause.ObjectArgu1 = this;
			GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_pause);
			_text_MainText.text = "挑战失败";


			_Holder_PauseMenu.gameObject.SetActive(true);
			_Button_Unpause.gameObject.SetActive(false);
			_Button_AbandonAndBack.gameObject.SetActive(true);
			_Button_ChangeInputType.gameObject.SetActive(false);
		}

#endregion





		public override void LateInitializeByUIM()
		{
			base.LateInitializeByUIM();
			// TimeText.text=ToTimeFormat(_time);	
		}
		public override void UpdateTick(float currentTime, int currentFrameCount, float deltaTime)
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				if (PanelRequirePause)
				{
					_Holder_PauseMenu.gameObject.SetActive(false);
					var ds_pause =
						new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_Global_RequireResumeGame_要求解除暂停);
					ds_pause.ObjectArgu1 = this;
					GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_pause);
					PanelRequirePause = false;
				}
				else
				{
					switch (GlobalConfigurationAssetHolderHelper.GetCurrentLoadPhase())
					{
						case GeneralGameAssetLoadPhaseEnum.WorldMap:
						case GeneralGameAssetLoadPhaseEnum.RegionMap: //如果当前已经暂停了，那就不再暂停了
							var tt = UIManager_WorldMap.Instance.GetPanel<UIP_PlayerCharacterFullPanel>();
							if (tt != null && tt.IsPanelCurrentSelfActive)
							{
								return;
							}

							break;
						case GeneralGameAssetLoadPhaseEnum.ARPG:
							var bc = UIManager_ARPG.Instance.GetPanel<UIP_BattleConclusionPanel>();
							if (bc != null && bc.IsPanelCurrentSelfActive)
							{
								return;
							}

							break;
					}
					_Holder_PauseMenu.gameObject.SetActive(true);
					var ds_pause = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_Global_RequirePauseGame_要求暂停游戏);
					ds_pause.ObjectArgu1 = this;
					GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_pause);
					PanelRequirePause = true;
				}
			}



			base.UpdateTick(currentTime, currentFrameCount, deltaTime);
		}
		// public static string ToTimeFormat(float time)
		// {
		// 	int seconds = (int)time;
		// 	int minute = seconds / 60;
		// 	seconds %=60;
		// 	//返回00:00时间格式
		// 	return string.Format("{0:D2}:{1:D2}", minute, seconds);
		// }
		//
#region 返回大地图

		private void ReturnToWorldMap()
		{
			var level = GlobalConfigurationAssetHolderHelper.GetGCAHH().RuntimeRecordHelper_Level
				.GetLoadLevelHandle("索尔特里亚行省_东部", true, -1);
			// var currentSceneName = SceneManager.GetActiveScene().name;
			level.FinallyLoad();
		}

#endregion
#region 
		private void ExitGame()
		{
			// Application.Quit();
		}

#endregion


#region 更换输入方式

		/// <summary>
		/// <para>是一个Toggle</para>
		/// </summary>
		private void ChangeInputType()
		{
			var gcso = GlobalConfigurationAssetHolderHelper.GetGCAHH().GlobalConfigSO_Runtime.Content;
			if (gcso.UsePureKeyboard)
			{
				gcso.UsePureKeyboard = false;
			}
			else
			{
				gcso.UsePureKeyboard = true;
			}
			var ds_changeStyle =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_Input_ChangeInputStyle_更换输入风格于同输入方式下);
			GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_changeStyle);
			UpdateTextOnChangeInputType();
			
		}

		private void UpdateTextOnChangeInputType()
		{
			var gcso = GlobalConfigurationAssetHolderHelper.GetGCAHH().GlobalConfigSO_Runtime.Content;
			if (!gcso.UsePureKeyboard)
			{
				_text_ChangeInputMode.text = "当前输入模式：键鼠\n点击更换为纯键盘";
			}
			else
			{
				_text_ChangeInputMode.text = "当前输入模式：纯键盘\n点击更换为键鼠";
			}
		}

		

#endregion

	}
}