using System;
using System.Collections.Generic;
using ARPG.Character.Base;
using ARPG.Character.Enemy.AI.Decision.DecisionComponent;
using ARPG.Equipment;
using RPGCore.Buff;
using RPGCore.Skill;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Player
{
	[Serializable]
	public class PACConfigInfo
	{

#if UNITY_EDITOR
		public string _EDITOR_GetElementCount()
		{
			return "包含 " + (CCList_VFX.Count + PAECList_Dis.Count + PAEC_LayoutList.Count + AbsorbList_PAEC.Count +
			                EventList_PAEC.Count + +AudioList_PAEC.Count + +OtherPAECList.Count).ToString() + " 个元素";
		}
#endif
		
		

		[NonSerialized]
		public List<BasePlayerAnimationEventCallback> AllAECList_Runtime;

		[NonSerialized]
		public bool HasBuild = false;

		public void BuildRuntimePAEC()
		{
			AllAECList_Runtime = new List<BasePlayerAnimationEventCallback>();
			AllAECList_Runtime.AddRange(CCList_VFX);
			AllAECList_Runtime.AddRange(PAECList_Dis);
			AllAECList_Runtime.AddRange(PAEC_LayoutList);
			AllAECList_Runtime.AddRange(AbsorbList_PAEC);
			AllAECList_Runtime.AddRange(EventList_PAEC);
			AllAECList_Runtime.AddRange(AudioList_PAEC);
			AllAECList_Runtime.AddRange(OtherPAECList);
			
		}

		/// <summary>
		/// 直接执行所有效果。不考虑监听，不考虑Tick，内部检查是否已经构建
		/// </summary>
		public void JustExecuteAllEffectBySkill(BaseARPGCharacterBehaviour behaviour, BaseRPSkill skill)
		{
			if (!HasBuild)
			{
				HasBuild = true;
				 BuildRuntimePAEC();
			}

			foreach (BasePlayerAnimationEventCallback effect in AllAECList_Runtime)
			{
				effect.ExecuteBySkill(behaviour, skill);
			}
		}

		public void JsutExecuteAllEffectByBuff(BaseARPGCharacterBehaviour behaviour, BaseRPBuff buff)
		{
			if (!HasBuild)
			{
				HasBuild = true;
				BuildRuntimePAEC();
			}

			foreach (BasePlayerAnimationEventCallback effect in AllAECList_Runtime)
			{
				effect.ExecuteByBuff(behaviour, buff);
			}
		}

		/// <summary>
		/// 直接执行所有效果。不考虑监听，不考虑Tick，内部检查是否已经构建
		/// </summary>
		public void JustExecuteAllEffectByWeapon(BaseARPGCharacterBehaviour behaviour , BaseWeaponHandler weapon)
		{
			if (!HasBuild)
			{
				HasBuild = true;
				BuildRuntimePAEC();
			}

			foreach (BasePlayerAnimationEventCallback effect in AllAECList_Runtime)
			{
				effect.ExecuteByWeapon( behaviour, weapon);
			}
		}



#region VFX 相关

		[SerializeReference, LabelText("【特效】相关作用")]
		[PropertyOrder(10)]
#if UNITY_EDITOR
		[FoldoutGroup("@ShowVFX()", false)]
		[ListDrawerSettings(HideAddButton = true,
			OnBeginListElementGUI = "VFXList_Begin",
			OnEndListElementGUI = "VFXList_End",
			DefaultExpandedState = true)]
#endif
		public List<BasePlayerAnimationEventCallback> CCList_VFX = new List<BasePlayerAnimationEventCallback>();




#if UNITY_EDITOR

		private string ShowVFX()
		{
			return CCList_VFX.Count > 0 ? "【特效】♣" : "【特效】";
		}

		[FoldoutGroup("@ShowVFX()", false)]
		// [HorizontalGroup ("@ShowVFX()"+"/1")]
		[PropertyOrder(11)]
		[Button("播放特效", Icon = SdfIconType.Plus, Stretch = false)]
		private void _Button_AddPlayVFX()
		{
			var newVFX = new PAEC_生成特效配置_SpawnVFXFromConfig();
			CCList_VFX.Add(newVFX);
		}

		[Button("停止特效", Icon = SdfIconType.Plus, Stretch = false)]
		[FoldoutGroup("@ShowVFX()", false)]
		// [HorizontalGroup("@ShowVFX()" + "/1")]
		[PropertyOrder(11)]
		private void _Button_AddStopVFX()
		{
			var newVFX = new PAEC_停止特效配置_StopVFXFromConfig();
			CCList_VFX.Add(newVFX);
		}

		[Button("前摇蓄力特效", Icon = SdfIconType.Plus, Stretch = false)]
		[FoldoutGroup("@ShowVFX()", false)]
		// [HorizontalGroup("@ShowVFX()" + "/1")]
		[PropertyOrder(11)]
		private void _Button_AddChargeVFX()
		{
			var newVFX = new PAEC_响应前摇蓄力的特效配置_SpawnVFXConfigAffectByCharge();
			CCList_VFX.Add(newVFX);
		}


		private void VFXList_Begin(int i)
		{
			Sirenix.Utilities.Editor.SirenixEditorGUI.BeginBox(this.CCList_VFX[i].GetElementNameInList());
		}

		private void VFXList_End(int i)
		{
			Sirenix.Utilities.Editor.SirenixEditorGUI.EndBox();
		}


#endif

#endregion


#region 位移 相关

		[SerializeReference, LabelText("【位移】相关作用")]
		[PropertyOrder(14)]
#if UNITY_EDITOR
		[FoldoutGroup("@ShowDis()", false)]
		[ListDrawerSettings(HideAddButton = true,
			OnBeginListElementGUI = "DisList_Begin",
			OnEndListElementGUI = "DisList_End",
			DefaultExpandedState = true)]
#endif
		public List<BasePlayerAnimationEventCallback> PAECList_Dis = new List<BasePlayerAnimationEventCallback>();



#if UNITY_EDITOR


		private string ShowDis()
		{
			return PAECList_Dis.Count > 0 ? "【位移】♣" : "【位移】";
		}

		[FoldoutGroup("@ShowDis()", false)]
		// [HorizontalGroup ("@ShowDis()"+"/1")]
		[PropertyOrder(15)]
		[Button("开始位移", Icon = SdfIconType.Plus, Stretch = false)]
		private void _button_BeginMovementOperation()
		{
			var newVFX = new PAEC_开始一段位移_StartDisplacement();
			PAECList_Dis.Add(newVFX);
		}

		[FoldoutGroup("@ShowDis()", false)]
		// [HorizontalGroup("@ShowDis()"+"/1")]
		[PropertyOrder(15)]
		[Button("调整作用时长的位移", Icon = SdfIconType.Plus, Stretch = false)]
		private void _button_TurnFace()
		{
			var newVFX = new PAEC_调整作用中时长的位移_StartDisplacementAndModifyEffectDuration();
			PAECList_Dis.Add(newVFX);
		}

		private void DisList_Begin(int i)
		{
			Sirenix.Utilities.Editor.SirenixEditorGUI.BeginBox(this.PAECList_Dis[i].GetElementNameInList());
		}

		private void DisList_End(int i)
		{
			Sirenix.Utilities.Editor.SirenixEditorGUI.EndBox();
		}


#endif

#endregion


#region 版面 相关

		[SerializeReference, LabelText("【版面】相关作用")]
		[PropertyOrder(16)]
#if UNITY_EDITOR
		[FoldoutGroup("@ShowLayout()", false)]
		[ListDrawerSettings(HideAddButton = true,
			OnBeginListElementGUI = "LayoutList_Begin",
			OnEndListElementGUI = "LayoutList_End",
			DefaultExpandedState = true)]
#endif
		public List<BasePlayerAnimationEventCallback> PAEC_LayoutList = new List<BasePlayerAnimationEventCallback>();



#if UNITY_EDITOR


		private string ShowLayout()
		{
			return PAEC_LayoutList.Count > 0 ? "【版面】♣" : "【版面】";
		}


		[FoldoutGroup("@ShowLayout()", false)]
		// [HorizontalGroup("@ShowLayout()"+"/1")]
		[PropertyOrder(17)]
		[Button("生成版面", Icon = SdfIconType.Plus, Stretch = false)]
		private void _button_BeginLayoutOperation()
		{
			var newVFX = new PAEC_生成版面_SpawnLayout();
			PAEC_LayoutList.Add(newVFX);
		}




		[FoldoutGroup("@ShowLayout()", false)]
		// [HorizontalGroup("@ShowLayout()"+"/1")]
		[PropertyOrder(17)]
		[Button("前摇蓄力的版面", Icon = SdfIconType.Plus, Stretch = false)]
		private void _button_StopLayoutOperation()
		{
			var newVFX = new PAEC_响应蓄力的版面生成_SpawnLayoutAffectByCharge();
			PAEC_LayoutList.Add(newVFX);
		}

		private void LayoutList_Begin(int i)
		{
			Sirenix.Utilities.Editor.SirenixEditorGUI.BeginBox(this.PAEC_LayoutList[i].GetElementNameInList());
		}

		private void LayoutList_End(int i)
		{
			Sirenix.Utilities.Editor.SirenixEditorGUI.EndBox();
		}


#endif

#endregion



#region 吸附 相关

		[SerializeReference, LabelText("【吸附】相关作用")]
		[PropertyOrder(16)]
#if UNITY_EDITOR
		[FoldoutGroup("@ShowAbsorb()", false)]
		[ListDrawerSettings(HideAddButton = true,
			OnBeginListElementGUI = "AbsorbList_Begin",
			OnEndListElementGUI = "AbsorbList_End",
			DefaultExpandedState = true)]
#endif
		public List<BasePlayerAnimationEventCallback> AbsorbList_PAEC = new List<BasePlayerAnimationEventCallback>();

		private bool AbsorbList_PAECShow()
		{
			return AbsorbList_PAEC.Count > 0;
		}


#if UNITY_EDITOR

		private string ShowAbsorb()
		{
			return AbsorbList_PAEC.Count > 0 ? "【吸附】♣" : "【吸附】";
		}

		[FoldoutGroup("@ShowAbsorb()", false)]
		// [HorizontalGroup("@ShowAbsorb()"+"/1")]
		[PropertyOrder(17)]
		[Button("刷新吸附状态", Icon = SdfIconType.Plus, Stretch = false), ListDrawerSettings()]
		private void _button_BeginAbsorbOperation()
		{
			var newVFX = new PAEC_刷新吸附状态_RefreshAbsorbState();
			AbsorbList_PAEC.Add(newVFX);
		}

		[FoldoutGroup("@ShowAbsorb()", false)]
		// [HorizontalGroup("@ShowAbsorb()"+"/1")]
		[PropertyOrder(17)]
		[Button("进行一次吸附", Icon = SdfIconType.Plus, Stretch = false)]
		private void _button_StopAbsorbOperation()
		{
			var newVFX = new PAEC_进行一次吸附_ProcessAbsorb();
			AbsorbList_PAEC.Add(newVFX);
		}

		private void AbsorbList_Begin(int i)
		{
			Sirenix.Utilities.Editor.SirenixEditorGUI.BeginBox(this.AbsorbList_PAEC[i].GetElementNameInList());
		}

		private void AbsorbList_End(int i)
		{
			Sirenix.Utilities.Editor.SirenixEditorGUI.EndBox();
		}


#endif

#endregion

#region 事件 相关

		[SerializeReference, LabelText("【事件】相关作用")]
		[PropertyOrder(16)]
#if UNITY_EDITOR
		[FoldoutGroup("@ShowEvent()", false)]
		[ListDrawerSettings(HideAddButton = true,
			OnBeginListElementGUI = "EventList_Begin",
			OnEndListElementGUI = "EventList_End",
			DefaultExpandedState = true)]
#endif
		public List<BasePlayerAnimationEventCallback> EventList_PAEC = new List<BasePlayerAnimationEventCallback>();

#if UNITY_EDITOR

		private string ShowEvent()
		{
			return EventList_PAEC.Count > 0 ? "【事件】♣" : "【事件】";
		}

		[FoldoutGroup("@ShowEvent()", false)]
		// [HorizontalGroup("@ShowEvent()"+"/1")]
		[PropertyOrder(17)]
		[Button("生成事件", Icon = SdfIconType.Plus, Stretch = false), ListDrawerSettings()]
		private void _button_BeginEventOperation()
		{
			var newVFX = new PAEC_生成游戏事件_SpawnGameplayEvent();
			EventList_PAEC.Add(newVFX);
		}

		[FoldoutGroup("@ShowEvent()", false)]
		// [HorizontalGroup("@ShowEvent()"+"/1")]
		[PropertyOrder(17)]
		[Button("运行事件命令", Icon = SdfIconType.Plus, Stretch = false), ListDrawerSettings()]
		private void _button_ExecuteEEHOperation()
		{
			var newVFX = new PAEC_直接运行事件命令_ExecuteGEHDirectly();
			EventList_PAEC.Add(newVFX);
		}
		
		
		[FoldoutGroup("@ShowEvent()", false)]
		// [HorizontalGroup("@ShowEvent()"+"/1")]
		[PropertyOrder(17)]
		[Button("响应蓄力的事件", Icon = SdfIconType.Plus, Stretch = false)]
		private void _button_StopEventOperation()
		{
			var newVFX = new PAEC_响应蓄力的事件触发_LaunchEventAffectByCharge();
			EventList_PAEC.Add(newVFX);
		}

		private void EventList_Begin(int i)
		{
			Sirenix.Utilities.Editor.SirenixEditorGUI.BeginBox(this.EventList_PAEC[i].GetElementNameInList());
		}

		private void EventList_End(int i)
		{
			Sirenix.Utilities.Editor.SirenixEditorGUI.EndBox();
		}


#endif

#endregion





#region 音效 相关

		[SerializeReference, LabelText("【音效】相关作用")]
		[PropertyOrder(16)]
#if UNITY_EDITOR
		[FoldoutGroup("@ShowAudio()", false)]
		[ListDrawerSettings(HideAddButton = true,
			OnBeginListElementGUI = "AudioList_Begin",
			OnEndListElementGUI = "AudioList_End",
			DefaultExpandedState = true)]
#endif
		public List<BasePlayerAnimationEventCallback> AudioList_PAEC = new List<BasePlayerAnimationEventCallback>();

#if UNITY_EDITOR

		private string ShowAudio()
		{
			return AudioList_PAEC.Count > 0 ? "【音效】♣" : "【音效】";
		}

		[FoldoutGroup("@ShowAudio()", false)]
		// [HorizontalGroup("@ShowAudio()"+"/1")]
		[PropertyOrder(17)]
		[Button("播放音效", Icon = SdfIconType.Plus, Stretch = false), ListDrawerSettings()]
		private void _button_BeginAudioOperation()
		{
			var newVFX = new PAEC_播放音效_PlayAudio();
			AudioList_PAEC.Add(newVFX);
		}
		
		[FoldoutGroup("@ShowAudio()", false)]
		// [HorizontalGroup("@ShowAudio()"+"/1")]
		[PropertyOrder(17)]
		[Button("停止音效", Icon = SdfIconType.Plus, Stretch = false), ListDrawerSettings()]
		private void _button_StopAudioOperation()
		{
			var newVFX = new PAEC_停止音效_StopAudio();
			AudioList_PAEC.Add(newVFX);
		}


		private void AudioList_Begin(int i)
		{
			Sirenix.Utilities.Editor.SirenixEditorGUI.BeginBox(this.AudioList_PAEC[i].GetElementNameInList());
		}

		private void AudioList_End(int i)
		{
			Sirenix.Utilities.Editor.SirenixEditorGUI.EndBox();
		}


#endif

#endregion
		
		
		
		
		
		
		
		
		
		
		
		

		[SerializeReference, LabelText("【其他】相关作用")]
		[PropertyOrder(1000)]
#if UNITY_EDITOR
		[FoldoutGroup("@ShowOther()", false)]
		[ListDrawerSettings(ListElementLabelName = "@GetElementNameInList()",
			OnBeginListElementGUI = "OtherList_Begin",
			OnEndListElementGUI = "OtherList_End",
			DefaultExpandedState = true)]
#endif
		public List<BasePlayerAnimationEventCallback> OtherPAECList = new List<BasePlayerAnimationEventCallback>();


#if UNITY_EDITOR
		private string ShowOther()
		{
			return OtherPAECList.Count > 0 ? "【其他】♣" : "【其他】";
		}


		private void OtherList_Begin(int i)
		{
			Sirenix.Utilities.Editor.SirenixEditorGUI.BeginBox(this.OtherPAECList[i].GetElementNameInList());
		}

		private void OtherList_End(int i)
		{
			Sirenix.Utilities.Editor.SirenixEditorGUI.EndBox();
		}
#endif







#if UNITY_EDITOR

#region Convert

		public void ReceiveConversion(ref List<BasePlayerAnimationEventCallback> paecList)
		{
			//clear 
			CCList_VFX.Clear();
			PAECList_Dis.Clear();
			PAEC_LayoutList.Clear();
			AbsorbList_PAEC.Clear();
			EventList_PAEC.Clear();
			AudioList_PAEC.Clear();
			OtherPAECList.Clear();
			for (int i = 0; i < paecList.Count; i++)
			{
				var perPAEC = paecList[i];
				switch (perPAEC)
				{
					case PAEC_停止特效配置_StopVFXFromConfig:
					case PAEC_响应前摇蓄力的特效配置_SpawnVFXConfigAffectByCharge:
					case PAEC_生成特效配置_SpawnVFXFromConfig:

						CCList_VFX.Add(perPAEC);
						break;
					case PAEC_停止音效_StopAudio:
					case PAEC_播放音效_PlayAudio:
						AudioList_PAEC.Add(perPAEC);
						break;
					case PAEC_刷新吸附状态_RefreshAbsorbState:
					case PAEC_进行一次吸附_ProcessAbsorb:
						AbsorbList_PAEC.Add(perPAEC);
						break;
					case PAEC_响应蓄力的事件触发_LaunchEventAffectByCharge:
					case PAEC_生成游戏事件_SpawnGameplayEvent:
						EventList_PAEC.Add(perPAEC);
						break;
					case PAEC_响应蓄力的版面生成_SpawnLayoutAffectByCharge:
					case PAEC_生成版面_SpawnLayout:
						PAEC_LayoutList.Add(perPAEC);
						break;
					case PAEC_调整作用中时长的位移_StartDisplacementAndModifyEffectDuration:
					case PAEC_开始一段位移_StartDisplacement:
						PAECList_Dis.Add(perPAEC);
						break;
				
					case PAEC_亚瑟的连击:
						case PAEC_播放Timeline特效_PlayTimelineVFX:
						OtherPAECList.Add(perPAEC);
						
						break;
				}
			}
			paecList.Clear();
		}

#endregion
		
#endif
	}
}