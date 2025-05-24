using System;
using System.Collections.Generic;
using ARPG.Character.Player;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character
{
	[Serializable]
	[CreateAssetMenu(fileName = "预设玩家动画响应事件", menuName = "#SO Assets#/#RPG Core#/#武器#/预设玩家动画响应事件", order = 143)]
	public class SOConfig_PresetPlayerAnimationEventCallback : ScriptableObject
	{
		[SerializeField]
		public PACConfigInfo PACConfigInfo;

	}
}