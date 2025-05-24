using System;
using System.Collections.Generic;
using ARPG.Character;
using ARPG.Character.Enemy;
using ARPG.Config.BattleLevelConfig;
using ARPG.Manager;
using GameplayEvent;
using Global;
using Global.ActionBus;
using Global.AreaOnMap.Config;
using Global.Character;
using Global.GlobalConfig;
using Global.RuntimeRecord;
using RegionMap.LevelSelect;
using RPGCore.Projectile.Layout;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using WorldMapScene.Manager;
using WorldMapScene;
using WorldMapScene.RegionMap;
namespace ARPG.Common
{
	/// <summary>
	/// <para>ARPG场景的通用编辑期辅助</para>
	/// <para>通常用来加载逻辑物件，以及提供一些编辑期小工具</para>
	/// </summary>

	[AddComponentMenu("!#编辑期辅助#/【场景配置代理】/这是一个战斗场景")]
	public class EditorProxy_ARPGEditor : BaseEditorProxy_FullProxy
	{
		public static EditorProxy_ARPGEditor Instance;

        [FoldoutGroup("=====初始化功能====="), PropertyOrder(-1)]
        public string MissionName;
        [SerializeField, LabelText("【区域配置信息_首通】")]
        [FoldoutGroup("=====初始化功能====="), PropertyOrder(2)]
        public GameObject AreaLogicHolderPrefab_First;
        [SerializeField, LabelText("【区域配置信息_常规】")]
        [FoldoutGroup("=====初始化功能====="), PropertyOrder(3)]
        public GameObject AreaLogicHolderPrefab_Normal;
        [FormerlySerializedAs("DebugLevelFullConfig"), Header("========================")]
		[SerializeField, LabelText("这关实际对应的关卡配置"), Required] [TitleGroup("===配置===")]
		public SOConfig_LevelFullConfig RelatedLevelFullConfig;
		[SerializeField, LabelText("调试——开启难度选择")] [FoldoutGroup("调试指定", true)]
		public bool SetDifficulty = false;
		[SerializeField, LabelText("调试——难度指定")] [FoldoutGroup("调试指定", true)] [ShowIf(nameof(SetDifficulty))]
		public LevelDifficultyFullPresetTypeEnum DifficultyPreset = LevelDifficultyFullPresetTypeEnum.None;
		[SerializeField, LabelText("调试——时间指定")] [FoldoutGroup("调试指定", true)]
		public int TimeSet = 1;
		[SerializeField, LabelText("调试——天气指定")] [FoldoutGroup("调试指定", true)]
		public int WeatherSet = 1;

		[SerializeField, LabelText("如果需要使用额外的角色信息存档配置"),]
		[InlineEditor(InlineEditorObjectFieldModes.Boxed)]
		public DebugSO_CharacterInfo DebugSO_CharacterInfo;

		public override void Awake()
        {
            Instance = this;
            base.Awake();

            // 验证关卡配置
            if (RelatedLevelFullConfig == null)
            {
#if UNITY_EDITOR
                UnityEditor.EditorUtility.DisplayDialog("错误",
                    $"场景{SceneManager.GetActiveScene().name}的【EditorProxy-编辑期辅助】\n" + 
					$"上面没有指定【当前的完整关卡配置】",
                    "确定");
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                return;
            }

            // 验证区域配置
            if (AreaLogicHolderPrefab_First == null && AreaLogicHolderPrefab_Normal == null)
            {
#if UNITY_EDITOR
                UnityEditor.EditorUtility.DisplayDialog("错误",
                    $"场景{SceneManager.GetActiveScene().name}的【EditorProxy-编辑期辅助】\n" +
                    $"上面没有指定【区域配置信息】",
                    "确定");
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                return;
            }

            //如果是直接从场景运行的DEBUG模式，使用当前设置的区域信息
            //否则按规则使用区域信息
            if (GlobalConfigurationAssetHolderHelper.Instance != null)
            {
                if (AreaLogicHolderPrefab_First != null && !IfFirstPass())
                {
                    AreaLogicHolderPrefab = AreaLogicHolderPrefab_First;
                }
                else if (AreaLogicHolderPrefab_Normal != null)
                {
                    AreaLogicHolderPrefab = AreaLogicHolderPrefab_Normal;
                }
                else
                {
                    AreaLogicHolderPrefab = AreaLogicHolderPrefab_First != null ?
                        AreaLogicHolderPrefab_First : AreaLogicHolderPrefab_Normal;
                }
            }

            //生成区域配置信息
            var alh = FindObjectOfType<EditorProxy_AreaLogicHolder>(true);
            if (alh != null)
            {
                DestroyImmediate(alh.gameObject);
            }
            Instantiate(AreaLogicHolderPrefab);

            //生成GRS模块
            var grs_edit = FindObjectOfType<GameReferenceService_ARPG>(true);
            if (grs_edit != null)
            {
                DestroyImmediate(grs_edit.gameObject);
            }
            var grsObject = Addressables.LoadAssetAsync<GameObject>("GRS_ARPG").WaitForCompletion();
            Instantiate(grsObject);

            GlobalActionBus.GetGlobalActionBus().RegisterAction(
                ActionBus_ActionTypeEnum.G_GE_RequireLevelClearConclusion_要求进行通关结算,
                FirstPass);
           
            _Volume = FindObjectOfType<UnityEngine.Rendering.Volume>()?.gameObject;                      
            _VolumeFog = FindObjectOfType<VolumetricFogAndMist2.VolumetricFogManager>()?.gameObject;
        }

        public override void Start()
        {
            base.Start();

            //加载存档
            if (GlobalConfigurationAssetHolderHelper.Instance.GlobalConfigSO_Runtime.DebugLoadDirect)
            {
                GlobalConfigSO.RuntimeContent().ReloadSaverByDCSO(DebugSO_CharacterInfo);
                GlobalConfigSO.RuntimeContent().SetTimeAndWeather_Debug(TimeSet, WeatherSet);
                GlobalConfigurationAssetHolderHelper.Instance.RuntimeRecordHelper_Level =
                    new RuntimeRecordHelper_Level
                    {
                        CurrentFullLevelConfigRef = RelatedLevelFullConfig,
                        DifficultyFullPreset = DifficultyPreset
                    };
            }

            GlobalActionBus.GetGlobalActionBus().RegisterAction(ActionBus_ActionTypeEnum.G_WM_LateLoadDone,
                _PostFunction_OnLateLoadDone);
		}        

        public bool IfFirstPass()
        {
            var info = GCSOExtend.FindEventInfo($"{MissionName}已通关");
            if (info == null || info.Value == 0)
            {
                return false;
            }
            return true;
        }

        public void FirstPass(DS_ActionBusArguGroup ds)
        {
            GCSOExtend.AddEventVariable($"{MissionName}已通关", 1);
        }

		protected virtual  void _PostFunction_OnLateLoadDone(DS_ActionBusArguGroup ds)
		{

			var rlf = GlobalConfigurationAssetHolderHelper.Instance.RuntimeRecordHelper_Level.CurrentFullLevelConfigRef;
			if (rlf != null)
			{
				var eList2 = rlf.RelatedExtraConfigList_InsideFullLevelConfig;
				if (eList2 != null)
				{

					for (int i = 0; i < eList2.Count; i++)
					{
						if (eList2[i] != null)
						{
							GameplayEventManager.Instance.StartGameplayEvent(eList2[i]);
						}
					}

				}
			}
			//
			// var rlc = GlobalConfigurationAssetHolderHelper.Instance.RuntimeRecordHelper_Level
			// 	.CurrentSingleLevelConfigRef;
			// //把SingleLevel里面的事件都运行了
			// if (rlc != null)
			// {
			//
			// 	var eList = rlc.SingleLevelConfigEvents;
			// 	if (eList != null)
			// 	{
			//
			// 		for (int i = 0; i < eList.Count; i++)
			// 		{
			// 			if (eList[i] != null)
			// 			{
			// 				GameplayEventManager.Instance.StartGameplayEvent(eList[i]);
			// 			}
			// 		}
			// 	}
			// }
		}

#if UNITY_EDITOR


		protected virtual  void OnValidate()
		{
			transform.position = Vector3.zero;
		}
#endif
	}
}