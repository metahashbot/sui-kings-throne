using System;
using UnityEngine;
namespace ARPG.UI.Panel.Test
{
	public class BattleScene_Test : MonoBehaviour
	{
		public static BattleScene_Test Instance;


		private void Awake()
		{
			Instance = this;
		}
	}
}