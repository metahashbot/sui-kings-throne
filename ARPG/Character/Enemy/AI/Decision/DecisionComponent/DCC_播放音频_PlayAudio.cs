using System;
using System.Collections.Generic;
using System.Linq;
using Global;
using Global.Audio;
using Global.Utility;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{
	[Serializable]
	public class DCC_播放音频_PlayAudio : BaseDecisionCommonComponent
	{

#if UNITY_EDITOR
		[InlineButton("IB_ShowThisClip", "查看音频", Icon = SdfIconType.OpticalAudio)]
#endif
		[SerializeField, LabelText("播放的音频ID")]
		public string AudioID;

		[SerializeField, LabelText(""), ToggleButtons("包含随机音频", "单音频"),]
		public bool RandomAudio = false;


		[ShowIf("RandomAudio"), LabelText("随机音频ID")]
#if UNITY_EDITOR
		[ValidateInput("_VI_ValidateAudioList", "@this._errorString")]
		[ListDrawerSettings(OnBeginListElementGUI = "RandomAudioIDsList_Begin",
			OnEndListElementGUI = "RandomAudioIDsList_End")]
#endif
		public List<string> RandomAudioIDs = new List<string>();

		private static List<string> _internalShuffleList = new List<string>();

		
		
		public override void EnterComponent(SOConfig_AIBrain relatedBrain)
		{
			string targetClipID = AudioID;
			if (RandomAudio)
			{
				_internalShuffleList.Clear();
				if (RandomAudioIDs.Count > 0)
				{
					for (int i = 0; i < RandomAudioIDs.Count; i++)
					{
						if (!string.IsNullOrEmpty(RandomAudioIDs[i]))
						{
							_internalShuffleList.Add(RandomAudioIDs[i]);
						}
					}
					if (_internalShuffleList.Count > 0)
					{
						_internalShuffleList.Shuffle();
						targetClipID = _internalShuffleList[0];
					}
					else
					{
						DBug.LogError($" 在播放音频时，开启了随机音频，但是音频列表为空");
						return;
					}
				}
				else
				{
					DBug.LogError($" 在播放音频时，开启了随机音频，但是音频列表为空");
					return;
				}

			}

			GeneralAudioManager.Instance.PlayAudioBy(targetClipID, PlayOptions, null);
		
		}

#if UNITY_EDITOR

		private void RandomAudioIDsList_Begin(int i)
		{
			Sirenix.Utilities.Editor.SirenixEditorGUI.BeginHorizontalToolbar();
			// if (RandomAudioIDs[i] != null)
			// {
			// 	Sirenix.Utilities.Editor.SirenixEditorGUI.BeginBox(RandomAudioIDs[i]);
			// }
			// else
			// {
			// 	Sirenix.Utilities.Editor.SirenixEditorGUI.BeginBox("空");
			// }
		}

		private void RandomAudioIDsList_End(int i)
		{
			if (Sirenix.Utilities.Editor.SirenixEditorGUI.ToolbarButton(new GUIContent("\u266a 预览")))
			{
				ShowPreview(RandomAudioIDs[i]);

			}
			Sirenix.Utilities.Editor.SirenixEditorGUI.EndHorizontalToolbar();

			// Sirenix.Utilities.Editor.SirenixEditorGUI.EndBox();
		}

		[NonSerialized]
		private string _errorString;
		private bool _VI_ValidateAudioList()
		{
			_errorString = "";
			if (RandomAudioIDs == null || RandomAudioIDs.Count == 0)
			{
				_errorString += "音频列表不能为空";

			}
			for (int i = 0; i < RandomAudioIDs.Count; i++)
			{
				if (string.IsNullOrEmpty(RandomAudioIDs[i]))
				{
					_errorString += "音频ID不能为空";
				}
			}
			//如果RandomAudioIDs 包含重复的，也报错
			if (RandomAudioIDs.Count != RandomAudioIDs.Distinct().Count())
			{
				_errorString += "音频ID不能重复";
			}
			if (string.IsNullOrEmpty(_errorString))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		private string GetErrorString()
		{
			return _errorString;
		}
#endif

		[SerializeField, LabelText("播放参数")]
		public MMSoundManagerPlayOptions PlayOptions = MMSoundManagerPlayOptions.Default;



		[InfoBox("对于一切由BaseArea触发的事件，都会试图将播放点至于实际位置，如果不需要，则不勾选下方的[自动匹配区域位置]")]

		[LabelText("不在原点播放？")]
		public bool NoPositionInfo = true;

#if UNITY_EDITOR

		private void IB_ShowThisClip()
		{
			ShowPreview(AudioID);

		}
		
		
		
		
		private void ShowPreview(string id)
		{

			AudioClip clip = GlobalConfigurationAssetHolderHelper.GetGCAHH().FE_GeneralAudioResource
				.GetAudioClipByID(id);


			if (clip == null)
			{
				if (UnityEditor.EditorUtility.DisplayDialog("错误", "SO数据库中没有这个音频", "打开Excel", "定位SO库"))
				{
					var path = "Assets/ExcelData/Resource/通用音乐音效资源表.xlsx";
					var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
					//open 
					UnityEditor.AssetDatabase.OpenAsset(obj);
				}
				else
				{
					var path = "Assets/ExcelData/音频0_通用音频.asset";
					var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
					UnityEditor.EditorUtility.OpenPropertyEditor(obj);
				}
				return;
			}
			else
			{
				UnityEditor.EditorUtility.OpenPropertyEditor(clip);
			}


		}
#endif

	}
}