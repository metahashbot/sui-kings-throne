using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Base
{
	/// <summary>
	/// <para>提供【逻辑】动画信息。用于武器和技能中。区别于【跟随】动画信息</para>
	/// </summary>
	[Serializable]
	public class LogicAnimationInfo
	{

		[SerializeReference, LabelText("记录的所有动画信息"), ListDrawerSettings(ShowFoldout = true)]
		private List<AnimationInfoBase> _animationInfos = new List<AnimationInfoBase>();
		




	}
}