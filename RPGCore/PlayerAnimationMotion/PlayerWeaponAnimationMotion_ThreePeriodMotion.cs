using System;
using Sirenix.OdinInspector;
namespace RPGCore.PlayerAnimationMotion
{
	[Serializable]
	public class PlayerWeaponAnimationMotion_ThreePeriodMotion : PlayerWeaponAnimationMotion
	{

		[LabelText("包含攻击方向限制吗？")]
		[PropertyOrder(-200)]
		public bool ContainAttackDirectionRestriction = true;

		[LabelText("    攻击方向限制角度|左右各这么多"), ShowIf(nameof(ContainAttackDirectionRestriction))]
		[PropertyOrder(-200)]
		public float AttackDirectionRestrictAngle = 45f;



		[LabelText("后摇前允许自动接续的预输入时长阈值")]
		[PropertyOrder(-200)]
		public float TimeDurationBeforeAutoRelease;



		public override bool ContainsAnimationConfig(string configName)
		{
			if (base.ContainsAnimationConfig(configName))
			{
				return true;
			}
			if (string.Equals(configName, _ancn_PostAnimationName))
			{
				return true;
			}
			if (string.Equals(configName, _ancn_PrepareAnimationName))
			{
				return true;
			}
			if (string.Equals(configName, _ancn_MiddlePartAnimationName))
			{
				return true;
			}
			if (!string.IsNullOrEmpty(_prepareAnimation_ChangeAnimationAfterChargeAnimationName) && _prepareAnimation_ChangeAnimationAfterChargeAnimationName.Equals(configName,
				StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			return false;
		}
	}
}