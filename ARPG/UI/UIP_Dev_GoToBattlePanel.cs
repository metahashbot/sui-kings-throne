using System;
using System.Collections.Generic;
using Global.UIBase;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace ARPG.UI
{
	[TypeInfoBox("前往战斗场景的面板")]
	public class UIP_Dev_GoToBattlePanel : UI_UIBasePanel
	{

		[SerializeField, LabelText("Button-前往战斗场景"), FoldoutGroup("配置", true)]
		private Button _button_ConfirmEnter;

		[SerializeField, LabelText("TG_选择时间 开关组"), FoldoutGroup("配置", true)]
		private ToggleGroup _tg_TimeChoiceToggleGroup;



		[SerializeField, LabelText("Toggle_早晨"), FoldoutGroup("配置", true)]
		private Toggle _toggle_Morning;

		[SerializeField, LabelText("Toggle_中午"), FoldoutGroup("配置", true)]
		private Toggle _toggle_Noon;
		[SerializeField, LabelText("Toggle_黄昏"), FoldoutGroup("配置", true)]
		private Toggle _toggle_Dusk;

		[SerializeField, LabelText("Toggle_夜晚"), FoldoutGroup("配置", true)]
		private Toggle _toggle_Night;

		[SerializeField, LabelText("TG_选择天气 开关组"), FoldoutGroup("配置", true)]
		private ToggleGroup _tg_WeatherChoiceToggleGroup;


		[SerializeField, LabelText("Toggle_常规"), FoldoutGroup("配置", true)]
		private Toggle _toggle_Normal;

		[SerializeField, LabelText("Toggle_小雨"), FoldoutGroup("配置", true)]
		private Toggle _toggle_LightRain;

		[SerializeField, LabelText("Toggle_大雨"), FoldoutGroup("配置", true)]
		private Toggle _toggle_HeavyRain;




		[Serializable]
		private struct TargetSceneInfoPair
		{
			public string Time;
			public string Weather;
			public string TargetScene;
		}

		[SerializeField, LabelText("目标场景信息配对")]
		private List<TargetSceneInfoPair> TargetSceneInfoList = new List<TargetSceneInfoPair>();


		private TargetSceneInfoPair _currentActiveSceneInfo = new TargetSceneInfoPair
		{
			Time = "夜晚",
			Weather = "暴雨",
		};
		public override void StartInitializeByUIM()
		{
			base.StartInitializeByUIM();


			_button_ConfirmEnter.onClick.AddListener(_Button_GoToTargetScene);
			_toggle_Dusk.onValueChanged.AddListener(_Toggle_SetTargetToDusk);
			_toggle_Morning.onValueChanged.AddListener(_Toggle_SetTargetToMorning);
			_toggle_Noon.onValueChanged.AddListener(_Toggle_SetTargetToNoon);
			_toggle_Night.onValueChanged.AddListener(_ToggleSetTargetToNight);
			_toggle_Normal.onValueChanged.AddListener(_Toggle_SetWeatherToNormal);
			_toggle_LightRain.onValueChanged.AddListener(_Toggle_SetWeatherToLightRain);
			_toggle_HeavyRain.onValueChanged.AddListener(_Toggle_SetWeatherToHeavyRain);



			_toggle_Morning.isOn = true;
			_toggle_Normal.isOn = true;
		}

		private void _Toggle_SetTargetToMorning(bool to)
		{
			if (to)
			{
				_currentActiveSceneInfo.Time = "清晨";
			}
		}
		private void _Toggle_SetTargetToNoon(bool to)
		{
			if (to)
			{
				_currentActiveSceneInfo.Time = "中午";
			}
		}
		private void _Toggle_SetTargetToDusk(bool to)
		{
			if (to)
			{
				_currentActiveSceneInfo.Time = "黄昏";
			}
		}

		private void _ToggleSetTargetToNight(bool to)
		{
			if (to)
			{
				_currentActiveSceneInfo.Time = "夜晚";
			}
		}

		private void _Toggle_SetWeatherToNormal(bool to)
		{
			if (to)
			{
				_currentActiveSceneInfo.Weather = "常规";
			}
		}
		private void _Toggle_SetWeatherToLightRain(bool to)
		{
			if (to)
			{
				_currentActiveSceneInfo.Weather = "小雨";
			}
		}
		private void _Toggle_SetWeatherToHeavyRain(bool to)
		{
			if (to)
			{
				_currentActiveSceneInfo.Weather = "大雨";
			}
		}




		private void _Button_GoToTargetScene()
		{
			var sceneName = TargetSceneInfoList.Find((pair =>
				pair.Weather.Equals(_currentActiveSceneInfo.Weather, StringComparison.OrdinalIgnoreCase) &&
				pair.Time.Equals(_currentActiveSceneInfo.Time, StringComparison.OrdinalIgnoreCase))).TargetScene;

			SceneManager.LoadScene(sceneName);
		}












	}
}