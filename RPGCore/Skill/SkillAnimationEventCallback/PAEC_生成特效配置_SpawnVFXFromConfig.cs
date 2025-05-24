using System;
using ARPG.Character.Base;
using ARPG.Character.Enemy.AI.Decision;
using ARPG.Equipment;
using RPGCore;
using RPGCore.Skill;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Player
{
	[Serializable]
	public class PAEC_生成特效配置_SpawnVFXFromConfig : BasePlayerAnimationEventCallback
	{


		[LabelText("关联特效配置"), SerializeField, GUIColor(187f / 255f, 1f, 0f)]
		public string _vfx_UIDToPlay;
		

		
		
		[LabelText("包含伤害变体？")]
		public bool _vfx_IncludeDamageVariant = false;

		[NonSerialized]
		public PerVFXInfo RuntimeVFXInfoRef;
		
		
		[LabelText("包含额外校准？"),SerializeField]
		public bool _vfx_IncludeExtraAlign = false;



		[LabelText("√:按前方校准 | 口:按右方校准"), SerializeField]
		[ShowIf(nameof(_vfx_IncludeExtraAlign))]
		public bool _vfx_AlignByForwardOrRight = true;
		
		//
		// [LabelText("校准朝向到指针朝向？"),SerializeField] 
		// [ShowIf( nameof(_vfx_IncludeExtraAlign))]
		// public bool _vfx_AlignToPointer = false;

		[LabelText("√:校准世界朝向  | 口:校准本地朝向"), SerializeField]
		[ShowIf(nameof(_vfx_IncludeExtraAlign))]
		public bool _vfx_AlignToWorld = true;
		
		
		

		public override BasePlayerAnimationEventCallback ExecuteBySkill(
			BaseARPGCharacterBehaviour behaviour,
			BaseRPSkill skillRef,
			bool createNewWhenExist = false)
		{
			RuntimeVFXInfoRef = skillRef._VFX_GetAndSetBeforePlay(_vfx_UIDToPlay,
				true,
				_vfx_IncludeDamageVariant,
				skillRef.GetCurrentDamageType,
				skillRef.ToString());
			if (RuntimeVFXInfoRef == null)
			{
				return this;
			}

			if (_vfx_IncludeExtraAlign)
			{
				var direction = skillRef.RecordedAttackDirection ?? behaviour.GetCasterForwardDirection();
				if(_vfx_AlignByForwardOrRight)
				{
					if (_vfx_AlignToWorld)
					{
						RuntimeVFXInfoRef._VFX__3_SetDirectionOnForwardOnGlobalY0(direction);
					}
					else
					{
						RuntimeVFXInfoRef._VFX__3_SetDirectionOnForwardOnLocal(direction);
					}
				}
				else
				{
					if (_vfx_AlignToWorld)
					{
						RuntimeVFXInfoRef._VFX__3_SetDirectionOnRightOnGlobalY0(direction);
					}
					else
					{
						RuntimeVFXInfoRef._VFX__3_SetDirectionOnRightOnLocalY0(direction);
					}
					
				}
			}
			RuntimeVFXInfoRef._VFX__10_PlayThis(true, false);
			return this;
		}
		public override BasePlayerAnimationEventCallback ExecuteByWeapon(
			BaseARPGCharacterBehaviour behaviour,
			BaseWeaponHandler weaponHandler,
			bool createNewWhenExist = false)
		{

			RuntimeVFXInfoRef = weaponHandler._VFX_PS_GetAndSetBeforePlay(_vfx_UIDToPlay,
				true,
				behaviour.GetRelatedVFXContainer(),
				_vfx_IncludeDamageVariant,
				weaponHandler.GetCurrentDamageType,
				weaponHandler.ToString());
			if (_vfx_IncludeExtraAlign)
			{
				var direction = weaponHandler.RecordedAttackDirection ?? behaviour.GetCasterForwardDirection();
				if (_vfx_AlignToWorld)
				{
					RuntimeVFXInfoRef._VFX__3_SetDirectionOnForwardOnGlobalY0(direction);
				}
				else
				{
					RuntimeVFXInfoRef._VFX__3_SetDirectionOnForwardOnLocal(direction);
				}
			}
			RuntimeVFXInfoRef._VFX__10_PlayThis(true, false);
			return this;
		}

		public override string GetElementNameInList()
		{
			return $"{GetBaseCustomName()} 生成特效：_{_vfx_UIDToPlay}_";
		}
	}
}