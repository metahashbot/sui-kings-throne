using System;
using System.Collections.Generic;
using ARPG.Character.Enemy.AI.Decision.ConcreteDecision;
using ARPG.Character.Enemy.AI.Decision.DecisionComponent;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI
{
	[Serializable]
	public class DCCConfigInfo
	{


		public void ProcessRuntimeBuild()
		{
			FileRuntime = new List<SOConfig_PresetSideEffects>();
			CommonComponents_RuntimeAll = new List<BaseDecisionCommonComponent>();
			BuildRuntimeDCC(ref CommonComponents_RuntimeAll);
		}

#region VFX 相关


		[SerializeReference, LabelText("【特效】相关作用")]
		[PropertyOrder(10)] 
#if UNITY_EDITOR
		[FoldoutGroup("@ShowVFX()", false)]
		[ListDrawerSettings(HideAddButton = true,
			OnBeginListElementGUI = "VFXList_Begin",
			OnEndListElementGUI = "VFXList_End",DefaultExpandedState = true)]
#endif
		public List<BaseDecisionCommonComponent> CCList_VFX = new List<BaseDecisionCommonComponent>();


		

#if UNITY_EDITOR

		private string ShowVFX()
		{
			return CCList_VFX.Count > 0 ? "【特效】♣" : "【特效】";
		}
		
		[FoldoutGroup("@ShowVFX()",false)]
		// [HorizontalGroup ("@ShowVFX()"+"/1")]
		[PropertyOrder(11)]
		[Button("播放特效", Icon = SdfIconType.Plus,  Stretch = false)]
		private void _Button_AddPlayVFX()
		{
			var newVFX = new DCC_生成特效配置_SpawnVFXFromConfig();
			CCList_VFX.Add(newVFX);
		}

		[Button("停止特效", Icon = SdfIconType.Plus, Stretch = false)]
		[FoldoutGroup("@ShowVFX()", false)]
		// [HorizontalGroup("@ShowVFX()" + "/1")]
		[PropertyOrder(11)]
		private void _Button_AddStopVFX()
		{
			var newVFX = new DCC_停止特效配置_StopVFXFromConfig();
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


#region 决策 相关
		
		 

		[SerializeReference, LabelText("【决策】相关作用")]
		[PropertyOrder(13)]
#if UNITY_EDITOR
		 [FoldoutGroup( "@ShowDecisionGroupName()", false)]
		[ListDrawerSettings(HideAddButton = true,
			OnBeginListElementGUI = "DecisionList_Begin",
			OnEndListElementGUI = "DecisionList_End",
			DefaultExpandedState = true)]
#endif
		public List<BaseDecisionCommonComponent> CCList_Decision = new List<BaseDecisionCommonComponent>();


#if UNITY_EDITOR

		private string ShowDecisionGroupName()
		{
			return CCList_Decision.Count > 0 ? "【决策】♣" : "【决策】";
		}
		

		[FoldoutGroup("@ShowDecisionGroupName()", false)]//[ HorizontalGroup("@ShowDecisionGroupName()"+"/1")]
		[PropertyOrder(13)]
		[Button("排队决策", Icon = SdfIconType.Plus, Stretch = false)]
		private void _Button_AddDecision()
		{
			var newDecision = new DCC_要求排队决策_RequireQueueDecision();
			CCList_Decision.Add(newDecision);
		}


		[FoldoutGroup("@ShowDecisionGroupName()", false)] //[HorizontalGroup("@ShowDecisionGroupName()" + "/1")]
		// [GUIColor("ShowDecisionGroupColor")]
		[PropertyOrder(13)]
		[Button("抢占决策", Icon = SdfIconType.Plus, Stretch = false)]
		private void _Button_PreemptDecision()
		{
			var newDecision = new DCC_要求抢占决策_RequirePreemptDecision();
			CCList_Decision.Add(newDecision);
		}


		[FoldoutGroup("@ShowDecisionGroupName()", false)]// [HorizontalGroup("@ShowDecisionGroupName()" + "/1")]
		[PropertyOrder(13)]
		[Button("调权", Icon = SdfIconType.Plus, Stretch = false)]
		private void _Button_3()
		{
			var newDecision = new DCC_调整决策权重_ModifyDecisionWeight();
			CCList_Decision.Add(newDecision);
		}




		[FoldoutGroup("@ShowDecisionGroupName()", false)] //[HorizontalGroup("@ShowDecisionGroupName()" + "/1")]
		[PropertyOrder(13)]
		[Button("调整可用性", Icon = SdfIconType.Plus, Stretch = false)]
		private void _Button_4()
		{
			var newDecision = new DCC_调整决策自主推演可用性_ToggleDecisionAutoDeduce();
			CCList_Decision.Add(newDecision);
		}



		[FoldoutGroup("@ShowDecisionGroupName()", false)] //[HorizontalGroup("@ShowDecisionGroupName()" + "/2")]
		[PropertyOrder(13)]
		[Button("按组加入", Icon = SdfIconType.Plus, Stretch = false)]
		private void _Button_5()
		{
			var newDecision = new DCC_按组加入决策至队列_AddDecisionGroupToQueue();
			CCList_Decision.Add(newDecision);
		}

		[FoldoutGroup("@ShowDecisionGroupName()", false)] //[HorizontalGroup("@ShowDecisionGroupName()" + "/2")]
		[PropertyOrder(13)]
		[Button("黑白名单", Icon = SdfIconType.Plus, Stretch = false)]
		private void _Button_6()
		{
			var newDecision = new DCC_黑白名单调整决策自主推演可用性_ExceptionToggleDecisionAutoDeduce();
			CCList_Decision.Add(newDecision);
		}

		[FoldoutGroup("@ShowDecisionGroupName()", false)] //[HorizontalGroup("@ShowDecisionGroupName()" + "/2")]
		[PropertyOrder(13)]
		[Button("立刻推演", Icon = SdfIconType.Plus, Stretch = false)]
		private void _Button_7()
		{
			var newDecision = new DCC_要求立刻自主推演_RequireAutoDeduce();
			CCList_Decision.Add(newDecision);
		}



		[FoldoutGroup("@ShowDecisionGroupName()", false)]// [HorizontalGroup("@ShowDecisionGroupName()" + "/2")]
		[PropertyOrder(13)]
		[Button("解除队列锁", Icon = SdfIconType.Plus, Stretch = false)]
		private void _Button_8()
		{
			var newDecision = new DCC_解除队列锁_DisableQueueLock();
			CCList_Decision.Add(newDecision);
		}

		private void DecisionList_Begin(int i)
		{
			Sirenix.Utilities.Editor.SirenixEditorGUI.BeginBox(this.CCList_Decision[i].GetElementNameInList());
		}

		private void DecisionList_End(int i)
		{
			Sirenix.Utilities.Editor.SirenixEditorGUI.EndBox();
		}
#endif

		
#endregion



#region 位移 相关
		
        

		[SerializeReference, LabelText("【位移】相关作用")]
		[PropertyOrder(14)]
#if UNITY_EDITOR
		[FoldoutGroup("@ShowDis()",false)]
		[ListDrawerSettings(HideAddButton = true,
			OnBeginListElementGUI = "DisList_Begin",
			OnEndListElementGUI = "DisList_End",
			DefaultExpandedState = true)]
#endif
		public List<BaseDecisionCommonComponent> DCCList_Dis = new List<BaseDecisionCommonComponent>();



#if UNITY_EDITOR

 
		 private string ShowDis()
		 {
			 return DCCList_Dis.Count > 0 ? "【位移与运动】♣" : "【位移与运动】";
		 }

		 [FoldoutGroup("@ShowDis()", false)]
		// [HorizontalGroup ("@ShowDis()"+"/1")]
		[PropertyOrder(15)]
		[Button("开始位移", Icon = SdfIconType.Plus, Stretch = false)]
 		private void _button_BeginMovementOperation()
		{
			var newVFX = new DCC_进行一段位移操作_BeginMovementOperation();
			DCCList_Dis.Add(newVFX);
		}

		[FoldoutGroup("@ShowDis()", false)]
		// [HorizontalGroup("@ShowDis()"+"/1")]
		[PropertyOrder(15)]
		[Button("转面", Icon = SdfIconType.Plus, Stretch = false)]
	 		private void _button_TurnFace()
		{
			var newVFX = new DCC_转身_TurnFace();
			DCCList_Dis.Add(newVFX);
		}
		[FoldoutGroup("@ShowDis()", false)]
		// [HorizontalGroup("@ShowDis()"+"/1")]
		[PropertyOrder(15)]
		[Button("面向仇恨", Icon = SdfIconType.Plus, Stretch = false)]
		private void _button_FaceHate()
		{
			var newVFX = new DCC_重新面向当前仇恨_ReFaceToCurrentHatred();
			DCCList_Dis.Add(newVFX);
		}
		
		private void DisList_Begin(int i)
		{
			Sirenix.Utilities.Editor.SirenixEditorGUI.BeginBox(this.DCCList_Dis[i].GetElementNameInList());
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
		 [FoldoutGroup( "@ShowLayout()",false)]
		[ListDrawerSettings(HideAddButton = true,
			OnBeginListElementGUI = "LayoutList_Begin",
			OnEndListElementGUI = "LayoutList_End",
			DefaultExpandedState = true)]
#endif
		public List<BaseDecisionCommonComponent> DCC_LayoutList = new List<BaseDecisionCommonComponent>();



#if UNITY_EDITOR

 
		 private string ShowLayout()
		 {
			 return DCC_LayoutList.Count > 0 ? "【版面】♣" : "【版面】";
		 }


		 [FoldoutGroup("@ShowLayout()", false)]
		// [HorizontalGroup("@ShowLayout()"+"/1")]
		[PropertyOrder(17)]
		[Button("开始版面", Icon = SdfIconType.Plus, Stretch = false)]
		private void _button_BeginLayoutOperation()
		{
			var newVFX = new DCC_生成版面_SpawnLayout();
			DCC_LayoutList.Add(newVFX);
		}




		[FoldoutGroup("@ShowLayout()", false)]
		// [HorizontalGroup("@ShowLayout()"+"/1")]
		[PropertyOrder(17)]
		[Button("停止版面", Icon = SdfIconType.Plus, Stretch = false)]
		private void _button_StopLayoutOperation()
		{
			var newVFX = new DCC_停止版面_StopLayout();
			DCC_LayoutList.Add(newVFX);
		}

		private void LayoutList_Begin(int i)
		{
			Sirenix.Utilities.Editor.SirenixEditorGUI.BeginBox(this.DCC_LayoutList[i].GetElementNameInList());
		}

		private void LayoutList_End(int i)
		{

			Sirenix.Utilities.Editor.SirenixEditorGUI.EndBox();
		}


#endif

#endregion


#region 事件 相关
		
		
		[SerializeReference, LabelText("【事件】相关作用")]
		[PropertyOrder(16)]
#if UNITY_EDITOR
		[FoldoutGroup("@ShowEvent()",false)]
		[ListDrawerSettings(HideAddButton = true,
			OnBeginListElementGUI = "EventList_Begin",
			OnEndListElementGUI = "EventList_End",
			DefaultExpandedState = true)]
#endif
		public List<BaseDecisionCommonComponent> EventList_DCC = new List<BaseDecisionCommonComponent>();

		private bool EventList_DCCShow()
		{
			return EventList_DCC.Count > 0;
		}


#if UNITY_EDITOR

		 private string ShowEvent()
		 {
			 return EventList_DCC.Count > 0 ? "【事件】♣" : "【事件】";
		 }

		 [FoldoutGroup("@ShowEvent()", false)]
		// [HorizontalGroup("@ShowEvent()"+"/1")]
		[PropertyOrder(17)]
		[Button("开始事件", Icon = SdfIconType.Plus, Stretch = false),ListDrawerSettings( )]
		private void _button_BeginEventOperation()
		{
			var newVFX = new DCC_触发游戏性事件_LaunchGameplayEvent();
			EventList_DCC.Add(newVFX);
		}

		[FoldoutGroup("@ShowEvent()", false)]
		// [HorizontalGroup("@ShowEvent()"+"/1")]
		[PropertyOrder(17)]
		[Button("停止事件", Icon = SdfIconType.Plus, Stretch = false)]
		private void _button_StopEventOperation()
		{
			var newVFX = new DCC_停止游戏性事件_StopGameplayEvent();
			EventList_DCC.Add(newVFX);
		}
		
		private void EventList_Begin(int i)
		{
			Sirenix.Utilities.Editor.SirenixEditorGUI.BeginBox(this.EventList_DCC[i].GetElementNameInList());
		}

		private void EventList_End(int i)
		{
			Sirenix.Utilities.Editor.SirenixEditorGUI.EndBox();
		}


#endif

#endregion


#region Buff 相关

		
		[SerializeReference, LabelText("【Buff】相关作用")]
		[PropertyOrder(16)]
#if UNITY_EDITOR
		[FoldoutGroup("@ShowBuff()",false)]
		[ListDrawerSettings(HideAddButton = true,
			OnBeginListElementGUI = "BuffList_Begin",
			OnEndListElementGUI = "BuffList_End",
			DefaultExpandedState = true)]
#endif
		public List<BaseDecisionCommonComponent> BuffList_DCC = new List<BaseDecisionCommonComponent>();



#if UNITY_EDITOR


		private string ShowBuff()
		{
			return BuffList_DCC.Count > 0 ? "【Buff】♣" : "【Buff】";
		}

		[FoldoutGroup("@ShowBuff()", false)]
		// [HorizontalGroup("@ShowBuff()"+"/1")]
		[PropertyOrder(17)]
		[Button("增加Buff", Icon = SdfIconType.Plus, Stretch = false)]
		private void _button_AddBuffOperation()
		{
			var newVFX = new DCC_施加Buff_ApplyBuffWithArguments();
			BuffList_DCC.Add(newVFX);
		}




		[FoldoutGroup("@ShowBuff()", false)]
		// [HorizontalGroup("@ShowBuff()"+"/1")]
		[PropertyOrder(17)]
		[Button("移除Buff", Icon = SdfIconType.Plus, Stretch = false)]
		private void _button_RemoveBuffOperation()
		{
			var newVFX = new DCC_移除Buff_RemoveBuffWithArguments();
			BuffList_DCC.Add(newVFX);
		}
		
		private void BuffList_Begin(int i)
		{
			Sirenix.Utilities.Editor.SirenixEditorGUI.BeginBox(this.BuffList_DCC[i].GetElementNameInList());
		}

		private void BuffList_End(int i)
		{
			Sirenix.Utilities.Editor.SirenixEditorGUI.EndBox();
		}


#endif

#endregion



#region 快速玩法操作 相关


		[SerializeReference, LabelText("【gameplay】相关作用")]
		[PropertyOrder(16)]
#if UNITY_EDITOR
		[FoldoutGroup("@ShowGameplay()", false)]
		[ListDrawerSettings(HideAddButton = true,
			OnBeginListElementGUI = "GameplayList_Begin",
			OnEndListElementGUI = "GameplayList_End",
			DefaultExpandedState = true)]
#endif
		public List<BaseDecisionCommonComponent> GameplayList_DCC = new List<BaseDecisionCommonComponent>();



#if UNITY_EDITOR


		private string ShowGameplay()
		{
			return GameplayList_DCC.Count > 0 ? "【gameplay】♣" : "【gameplay】";
		}

		[FoldoutGroup("@ShowGameplay()", false)]
		// [HorizontalGroup("@ShowBuff()"+"/1")]
		[PropertyOrder(17)]
		[Button("修改碰撞信息组", Icon = SdfIconType.Plus, Stretch = false)]
		private void _button_SwitchCollisionInfo()
		{
			var newVFX = new DCC_更换碰撞信息组_SwitchCollisionInfoGroup();
			GameplayList_DCC.Add(newVFX);
		}
		

		private void GameplayList_Begin(int i)
		{
			Sirenix.Utilities.Editor.SirenixEditorGUI.BeginBox(this.GameplayList_DCC[i].GetElementNameInList());
		}

		private void GameplayList_End(int i)
		{
			Sirenix.Utilities.Editor.SirenixEditorGUI.EndBox();
		}


#endif

#endregion

		
#region 描边 相关
		
		
		[SerializeReference, LabelText("【描边】相关作用")]
		[PropertyOrder(16)]
#if UNITY_EDITOR
		 [FoldoutGroup( "@ShowOutline()",false)]
		[ListDrawerSettings(HideAddButton = true,
			OnBeginListElementGUI = "OutlineList_Begin",
			OnEndListElementGUI = "OutlineList_End",
			DefaultExpandedState = true)]
#endif
		public List<BaseDecisionCommonComponent> OutlineList_DCC = new List<BaseDecisionCommonComponent>();
		 
#if UNITY_EDITOR

		 private string ShowOutline()
		 {
			 return OutlineList_DCC.Count > 0 ? "【描边】♣" : "【描边】";
		 }

		 [FoldoutGroup("@ShowOutline()", false)]
		// [HorizontalGroup("@ShowOutline()"+"/1")]
		[PropertyOrder(17)]
		[Button("开始描边", Icon = SdfIconType.Plus, Stretch = false)]
		private void _button_BeginOutlineOperation()
		{
			var newVFX = new DCC_开启描边预设任务_StartOutlinePresetTask();
			OutlineList_DCC.Add(newVFX);
		}

		[FoldoutGroup("@ShowOutline()", false)]
		// [HorizontalGroup("@ShowOutline()"+"/1")]
		[PropertyOrder(17)] 
		[Button("停止描边", Icon = SdfIconType.Plus, Stretch = false)]
		private void _button_StopOutlineOperation()
		{
			var newVFX = new DCC_关闭描边预设任务_StopOutlinePresetTask();
			OutlineList_DCC.Add(newVFX);
		}
		
		private void OutlineList_Begin(int i)
		{
			Sirenix.Utilities.Editor.SirenixEditorGUI.BeginBox(this.OutlineList_DCC[i].GetElementNameInList());
		}
		 
		private void OutlineList_End(int i)
		{
			Sirenix.Utilities.Editor.SirenixEditorGUI.EndBox();
		}
		
#endif
		 #endregion



#region 行为模式 相关
		
		
		[SerializeReference, LabelText("【行为模式】相关作用")]
		[PropertyOrder(16)]
#if UNITY_EDITOR
		[FoldoutGroup("@ShowBehaviorPattern()",false)]
		[ListDrawerSettings(HideAddButton = true,
			OnBeginListElementGUI = "BPList_Begin",
			OnEndListElementGUI = "BPList_End",
			DefaultExpandedState = true)]
#endif
		public List<BaseDecisionCommonComponent> BPList_DCC = new List<BaseDecisionCommonComponent>();

#if UNITY_EDITOR
		
		 private string ShowBehaviorPattern()
		 {
			 return BPList_DCC.Count > 0 ? "【行为模式】♣" : "【行为模式】";
		 }


		 [FoldoutGroup("@ShowBehaviorPattern()", false)]
		// [HorizontalGroup("@ShowBehaviorPattern()"+"/1")]
		[PropertyOrder(17)]
		[Button("切换行为模式", Icon = SdfIconType.Plus, Stretch = false)]
		private void _button_ChangeBehaviorPatternOperation()
		{
			var newVFX = new DCC_要求切换行为模式_RequireSwitchBehaviourPattern();
			BPList_DCC.Add(newVFX);
		}
		
		 

		private void BPList_Begin(int i)
		{
			Sirenix.Utilities.Editor.SirenixEditorGUI.BeginBox(this.BPList_DCC[i].GetElementNameInList());
		}

		private void BPList_End(int i)
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
		public List<BaseDecisionCommonComponent> AudioList_PAEC = new List<BaseDecisionCommonComponent>();

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
			var newVFX = new DCC_播放音频_PlayAudio();
			AudioList_PAEC.Add(newVFX);
		}

		// [FoldoutGroup("@ShowAudio()", false)]
		// // [HorizontalGroup("@ShowAudio()"+"/1")]
		// [PropertyOrder(17)]
		// [Button("停止音效", Icon = SdfIconType.Plus, Stretch = false), ListDrawerSettings()]
		// private void _button_StopAudioOperation()
		// {
		// 	var newVFX = new PAEC_停止音效_StopAudio();
		// 	AudioList_PAEC.Add(newVFX);
		// }


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
		[ListDrawerSettings(ListElementLabelName = "@GetElementNameInList()")]
#if UNITY_EDITOR
		[FoldoutGroup("@ShowOther()",false)]
		
#endif
		public List<BaseDecisionCommonComponent> CommonComponents = new List<BaseDecisionCommonComponent>();


#if UNITY_EDITOR
		private string ShowOther()
		{
			return CommonComponents.Count > 0 ? "【其他】♣" : "【其他】";
		}
		
#endif

#if UNITY_EDITOR
		[FoldoutGroup("@ShowOther()", false)]
#endif
		[PropertyOrder(2000)]
		[SerializeField, LabelText("【作用-文件】文件配置的作用组件们】",false),
		 ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true), InlineEditor(InlineEditorObjectFieldModes.Boxed)]
		public List<SOConfig_PresetSideEffects> CommonComponents_File;







		[NonSerialized, LabelText("【决策】运行时副作用组件们"), FoldoutGroup("运行时")]
		public List<BaseDecisionCommonComponent> CommonComponents_RuntimeAll;


		[NonSerialized]
		public List<SOConfig_PresetSideEffects> FileRuntime;

		public void BuildRuntimeDCC(ref List<BaseDecisionCommonComponent> collection)
		{
			collection.AddRange(BPList_DCC);
			collection.AddRange(CCList_VFX);
			collection.AddRange(GameplayList_DCC);
			collection.AddRange(CCList_Decision);
			collection.AddRange(DCCList_Dis);
			collection.AddRange(DCC_LayoutList);
			collection.AddRange(EventList_DCC);
			collection.AddRange(BuffList_DCC);
			collection.AddRange(OutlineList_DCC);
			collection.AddRange(CommonComponents);
			collection.AddRange(AudioList_PAEC);
			if (CommonComponents_File != null && CommonComponents_File.Count > 0)
			{
				if (FileRuntime != null)
				{
					FileRuntime = new List<SOConfig_PresetSideEffects>();
				}
				foreach (SOConfig_PresetSideEffects perFile in CommonComponents_File)
				{
					if (perFile == null)
					{
						continue;
					}
					if (FileRuntime == null)
					{
						FileRuntime = new List<SOConfig_PresetSideEffects>();
					}
					var newObj = UnityEngine.Object.Instantiate(perFile);
					FileRuntime.Add(newObj);
					newObj.DCCConfigInfo.BuildRuntimeDCC(ref collection);
				}
			}
		}


		public void ClearOnUnload()
		{
			
			CommonComponents_RuntimeAll.Clear();
			if (FileRuntime != null)
			{
				foreach (SOConfig_PresetSideEffects perFile in FileRuntime)
				{
					UnityEngine.Object.Destroy(perFile);
				}
			}
		}
	}
}