using System;
using ARPG.Character;
using ARPG.Character.Player;
using ARPG.Manager;
using Global;
using Global.ActionBus;
using RPGCore.Interface;
using RPGCore.Projectile.Layout;
using RPGCore.Skill.Config;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
namespace RPGCore.Skill.ConcreteSkill.Swordman
{
	[Serializable]
	public class Skill_Swordman_QuickStrike : BaseRPSkill
	{
		private SOConfig_ProjectileLayout _selfLayoutRef;
		public override void InitOnObtain(
			RPSkill_SkillHolder skillHolderRef,
			SOConfig_RPSkill configRuntimeInstance,
			I_RP_ObjectCanReleaseSkill parent,
			SkillSlotTypeEnum slot)
		{
			base.InitOnObtain(skillHolderRef, configRuntimeInstance, parent, slot);
			
		}
		protected override bool IfSkillCanCDTick()
		{
			return base.IfSkillCanCDTick();
		}

		protected override void _IC_ReceiveSkillInput_Start(InputAction.CallbackContext context)
		{
			base._IC_ReceiveSkillInput_Start(context);
		}

		public override bool _Internal_TryPrepareSkill()
		{
			if (base._Internal_TryPrepareSkill())
			{
				// QuickFaceToPointer();
				return true;
			}
			else
			{
				return false;
			}
		}

		protected override void PAEC_SpawnLayout(PAEC_生成版面_SpawnLayout paec, bool autoStart = true)
		{
			_selfLayoutRef = paec.RelatedConfig.SpawnLayout_NoAutoStart(_characterBehaviourRef);
			_selfLayoutRef.LayoutHandlerFunction.OverrideSpawnFromDirection =
				_characterBehaviourRef.GetSelfRolePlayArtHelper().CurrentFaceLeft
					? BaseGameReferenceService.CurrentBattleLogicLeftDirection
					: BaseGameReferenceService.CurrentBattleLogicRightDirection;
			_selfLayoutRef.LayoutHandlerFunction.StartLayout();
			_layoutList.Add(_selfLayoutRef);
		}
		
		
	}
}