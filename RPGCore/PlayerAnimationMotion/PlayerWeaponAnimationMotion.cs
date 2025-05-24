using System;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.PlayerAnimationMotion
{
	[Serializable]
	public abstract class PlayerWeaponAnimationMotion : BasePlayerAnimationMotion
	{


#region 后摇


		
		[FoldoutGroup("===后摇===")]
		[PropertyOrder(100)]
		[SerializeField, LabelText("动画_基本后摇"), GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		public string _ancn_PostAnimationName;

		

#endregion

		public override bool ContainsAnimationConfig(string configName)
		{
			if(base.ContainsAnimationConfig(configName))
			{
				return true;
			}
			if(string.Equals( configName, _ancn_PostAnimationName))
			{
				return true;
			}
			return false;
		}


	}
}