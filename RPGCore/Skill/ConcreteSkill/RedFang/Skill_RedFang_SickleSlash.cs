using System;
using System.Collections.Generic;
using ARPG.Character;
using ARPG.Character.Base;
using ARPG.Character.Base.CustomSpineData;
using ARPG.Character.Enemy;
using ARPG.Character.Player;
using ARPG.Manager;
using Global;
using Global.ActionBus;
using RPGCore.Buff;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.ConcreteBuff;
using RPGCore.Buff.ConcreteBuff.Common;
using RPGCore.Buff.ConcreteBuff.Skill.RedFang;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.PlayerAnimationMotion;
using RPGCore.Projectile;
using RPGCore.Projectile.Layout;
using RPGCore.Skill.Config;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace RPGCore.Skill.ConcreteSkill.RedFang
{
	[Serializable]
	public class Skill_RedFang_SickleSlash : BaseRPSkill
	{

		/// <summary>
		/// <para>当前充能？</para>
		/// </summary>
		public bool ActAsCharged { get; private set; }


		[SerializeField, LabelText("PAM记录_前摇_普通的"), TitleGroup("===配置==="),
		 GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		public string _pam_ani_NormalPrepareAnimation;
		[SerializeField, LabelText("PAM记录_前摇_充能的"), TitleGroup("===配置==="),
		 GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		public string _pam_ani_ChargedPrepareAnimation;



		[SerializeField, LabelText("PAM记录_施法_普通的"), TitleGroup("===配置==="),
		 GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		public string _pam_ani_NormalReleaseAnimation;
		[SerializeField, LabelText("PAM记录_施法_充能的"), TitleGroup("===配置==="),
		 GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		public string _pam_ani_ChargedReleaseAnimation;


		[SerializeField, LabelText("PAM记录_后摇_普通的"), TitleGroup("===配置==="),
		 GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		public string _pam_ani_NormalPostAnimation;
		[SerializeField, LabelText("PAM记录_后摇_充能的"), TitleGroup("===配置==="),
		 GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		public string _pam_ani_ChargedPostAnimation;



		[InfoBox("此处的斩杀线配置 会 【覆盖】版面中的配置")]
		[SerializeField, LabelText("斩杀血线_未充能_对普通"), SuffixLabel("%"), TitleGroup("===配置===")]
		public float _eliminateBloodLine_NonCharge = 15f;

		[SerializeField, LabelText("斩杀血线_已充能_对普通"), SuffixLabel("%"), TitleGroup("===配置===")]
		public float _eliminateBloodLine_Charged = 30f;

		[SerializeField, LabelText("斩杀血线_未充能_对精英"), SuffixLabel("%"), TitleGroup("===配置===")]
		public float _eliminateBloodLine_Elite_NonCharge = 10f;

		[SerializeField, LabelText("斩杀血线_已充能_对精英"), SuffixLabel("%"), TitleGroup("===配置===")]
		public float _eliminateBloodLine_Elite_Charged = 20f;


		[SerializeField, LabelText("默认剔除外 不斩杀的类型"), TitleGroup("===配置===")]
		public List<CharacterNamedTypeEnum> _AddonClipTypeList = new List<CharacterNamedTypeEnum>();
		[SerializeField, LabelText("给敌人挂的斩杀特效UID"), GUIColor(187f / 255f, 1f, 0f), TitleGroup("===配置===")]
		public string _vfxID_Enemy;

		[SerializeField, LabelText("斩杀时回复最大生命百分比"), TitleGroup("===配置===")]
		public float EliminateReplyHP = 0.1f;

		[SerializeField, LabelText("回血特效"), TitleGroup("===配置==="), GUIColor(187f / 255f, 1f, 0f)]
		public string _vfx_ReplyHP;

		private PerVFXInfo _vfxInfo_ReplyHP;

		/// <summary>
		/// <para>已记录为将要恢复为不充能。这将在Post(技能默认结束)那里确定恢复</para>
		/// </summary>
		private bool _registerRestoreNonCharged;

		protected class RegisteredEliminateInfo
		{
			public ParticleSystem PSHandlerRef;
			public EnemyARPGCharacterBehaviour BehaviourRef;
			public Float_RPDataEntry MaxHPRef;
			public FloatPresentValue_RPDataEntry CurrentHPRef;
			public bool IsElite;
		}

		protected List<RegisteredEliminateInfo> _registeredEliminateList = new List<RegisteredEliminateInfo>();



		/// <summary>
		/// <para>内部记录了刷新CD？将会在试图准备技能时/被打断时置false。在命中时置true。如果技能已经结束，则刷新CD。如果技能没有结束，则会在结束后刷新。</para>
		/// <para>这个flag是用来 [结束后刷新]的</para>
		/// </summary>
		private bool _selfRegisteredToRefreshCD;

		private float _nextRefreshTime;
		/// <summary>
		/// <para>每隔0.25秒刷新一次场上的敌人，</para>
		/// </summary>
		private float _refreshInterval = 0.5f;

		private CharacterOnMapManager _characterOnMapManagerRef;




		private PerVFXInfo _eliminateVFXInfo;
		public override void InitOnObtain(
			RPSkill_SkillHolder skillHolderRef,
			SOConfig_RPSkill configRuntimeInstance,
			I_RP_ObjectCanReleaseSkill parent,
			SkillSlotTypeEnum slot)
		{
			base.InitOnObtain(skillHolderRef, configRuntimeInstance, parent, slot);
			parent.ReleaseSkill_GetActionBus()
				.RegisterAction(ActionBus_ActionTypeEnum.L_Buff_OnBuffInitialized_一个Buff被确定初始化了,
					_ABC_ChangePAMToCharged);
			parent.ReleaseSkill_GetActionBus()
				.RegisterAction(ActionBus_ActionTypeEnum.L_Buff_OnBuffRemoved, _ABC_ChangePAMToNormal);
			parent.ReleaseSkill_GetActionBus()
				.RegisterAction(ActionBus_ActionTypeEnum.L_Damage_ToCaster_ResultToEliminateSlash_施加方_造成了斩杀效果,
					_ABC_TryRefreshCDOnEliminate);
			parent.ReleaseSkill_GetActionBus()
				.RegisterAction(ActionBus_ActionTypeEnum.G_PC_OnCurrentUsingCharacterChanged_当前使用的角色更换,
					_ABC_TryDisableAllOutlineOnChangeToOtherCharacter);
			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_EnemyBehaviour_OnEnemyDieDirect_敌人直接死亡没有尸体,
				_ABC_TryRemoveInfoOnEnemyDie);
			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_EnemyBehaviour_OnEnemyDieToCorpse_敌人死亡到尸体,
				_ABC_TryRemoveInfoOnEnemyDie);

			_characterBehaviourRef.ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.BloodGasValue_血气值,
				_characterBehaviourRef,
				_characterBehaviourRef);
			_eliminateVFXInfo = AllVFXInfoList.Find((info => info._VFX_InfoID == _vfxID_Enemy));
			_characterOnMapManagerRef = SubGameplayLogicManager_ARPG.Instance.CharacterOnMapManagerReference;
		}


		public override bool _Internal_TryPrepareSkill()
		{
			if (!base._Internal_TryPrepareSkill())
			{
				return false;
			}
			else
			{
				QuickFaceToPointer();

				return true;
			}
		}


		private float GetEliminateBloodLine(bool isElite)
		{
			if (isElite)
			{
				return ActAsCharged ? _eliminateBloodLine_Elite_Charged : _eliminateBloodLine_Elite_NonCharge;
			}
			else
			{
				return ActAsCharged ? _eliminateBloodLine_Charged : _eliminateBloodLine_NonCharge;
			}
		}
		public override void FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.FixedUpdateTick(currentTime, currentFrameCount, delta);
			if (currentTime > _nextRefreshTime)
			{
				if (!ReferenceEquals(_characterBehaviourRef, _playerControllerRef.CurrentControllingBehaviour))
				{
					_Internal_ClearAllEliminateVFX();
					return;
				}

				_nextRefreshTime = currentTime + _refreshInterval;


				//首先清理掉已经死亡的
				//并对已经有的进行刷新
				for (int i = _registeredEliminateList.Count - 1; i >= 0; i--)
				{
					if (_registeredEliminateList[i].BehaviourRef == null ||
					    !_registeredEliminateList[i].BehaviourRef.CharacterDataValid)
					{
						VFXPoolManager.Instance.ReturnParticleSystemToPool(_eliminateVFXInfo.Prefab,
							_registeredEliminateList[i].PSHandlerRef);
						GenericPool<RegisteredEliminateInfo>.Release(_registeredEliminateList[i]);
						_registeredEliminateList.RemoveAt(i);
					}
					else
					{
						var tmpInfo = _registeredEliminateList[i];
						float currentBloodLine = tmpInfo.CurrentHPRef.CurrentValue / tmpInfo.MaxHPRef.CurrentValue;
						float eliminateBloodLine = GetEliminateBloodLine(tmpInfo.IsElite) / 100f;
						if (currentBloodLine > eliminateBloodLine)
						{
							VFXPoolManager.Instance.ReturnParticleSystemToPool(_eliminateVFXInfo.Prefab,
								tmpInfo.PSHandlerRef);
							GenericPool<RegisteredEliminateInfo>.Release(tmpInfo);
							_registeredEliminateList.RemoveAt(i);
						}
					}
				}


				var list = _characterOnMapManagerRef.GetAllEnemy().ClipEnemyListOnDefaultType();
				//新增过程，不新增已经存在的、首领、默认剔除列表的
				foreach (EnemyARPGCharacterBehaviour perEnemy in list)
				{
					//不新增
					if (_registeredEliminateList.Exists((info => ReferenceEquals(perEnemy, info.BehaviourRef))))
					{
						continue;
					}
					if (perEnemy.ApplyDamage_CheckTargetBuff(RolePlay_BuffTypeEnum._EnemyTag_BossEnemy_首领敌人) !=
					    BuffAvailableType.NotExist)
					{
						continue;
					}
					if (perEnemy.IfIsInDefaultClipType())
					{
						continue;
					}
					if (_AddonClipTypeList.Contains(perEnemy.SelfBehaviourNamedType))
					{
						continue;
					}

					var currentBloodPartial =
						perEnemy.GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentHP_当前HP).CurrentValue /
						perEnemy.GetFloatDataEntryByType(RP_DataEntry_EnumType.HPMax_最大HP).CurrentValue;
					var currentEliminateLine = GetEliminateBloodLine(
						perEnemy.ApplyDamage_CheckTargetBuff(RolePlay_BuffTypeEnum._EnemyTag_EliteEnemy_精英敌人) !=
						BuffAvailableType.NotExist) / 100f;
					if (currentBloodPartial > currentEliminateLine)
					{
						continue;
					}


					var newInfo = GenericPool<RegisteredEliminateInfo>.Get();
					newInfo.BehaviourRef = perEnemy;
					newInfo.MaxHPRef = perEnemy.GetFloatDataEntryByType(RP_DataEntry_EnumType.HPMax_最大HP);
					newInfo.CurrentHPRef = perEnemy.GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentHP_当前HP);
					newInfo.IsElite =
						perEnemy.ApplyDamage_CheckTargetBuff(RolePlay_BuffTypeEnum._EnemyTag_EliteEnemy_精英敌人) !=
						BuffAvailableType.NotExist;
					PerVFXInfo selfVFXInfo = PerVFXInfo._VFX_GetByUID(AllVFXInfoList, _vfxID_Enemy, false, null, null);
					var t = selfVFXInfo._VFX_GetPSHandle(true, perEnemy.GetRelatedVFXContainer());

					newInfo.PSHandlerRef = t.GetCurrentMainPSRef;
					t._VFX__10_PlayThis();
					_registeredEliminateList.Add(newInfo);
				}
			}
		}


		
		
		/// <summary>
		/// <para>清理所有斩杀特效。常见于当前角色不再是wwa的时候，其他角色不需要显示这些特效</para>
		/// </summary>
		private void _Internal_ClearAllEliminateVFX()
		{
			for (int i = _registeredEliminateList.Count - 1; i >= 0; i--)
			{
				VFXPoolManager.Instance.ReturnParticleSystemToPool(_eliminateVFXInfo.Prefab,
					_registeredEliminateList[i].PSHandlerRef);
				GenericPool<RegisteredEliminateInfo>.Release(_registeredEliminateList[i]);
				_registeredEliminateList.RemoveAt(i);
			}
		}





		/// <summary>
		/// <para>当自己被换出去的时候，直接关闭所有描边</para>
		/// </summary>
		/// <param name="ds"></param>
		private void _ABC_TryDisableAllOutlineOnChangeToOtherCharacter(DS_ActionBusArguGroup ds)
		{
			//自己被换出去了
			if (ReferenceEquals(ds.GetObj2AsT<PlayerARPGConcreteCharacterBehaviour>(), _characterBehaviourRef))
			{
				for (int i = _registeredEliminateList.Count - 1; i >= 0; i--)
				{
					VFXPoolManager.Instance.ReturnParticleSystemToPool(_eliminateVFXInfo.Prefab,
						_registeredEliminateList[i].PSHandlerRef);
					GenericPool<RegisteredEliminateInfo>.Release(_registeredEliminateList[i]);
					_registeredEliminateList.RemoveAt(i);
				}
			}
		}

		private void _ABC_TryRemoveInfoOnEnemyDie(DS_ActionBusArguGroup ds)
		{
			var enemyRef = ds.GetObj1AsT<EnemyARPGCharacterBehaviour>();
			var fi = _registeredEliminateList.FindIndex((info => ReferenceEquals(info.BehaviourRef, enemyRef)));
			if (fi != -1)
			{
				VFXPoolManager.Instance.ReturnParticleSystemToPool(_eliminateVFXInfo.Prefab,
					_registeredEliminateList[fi].PSHandlerRef);
				GenericPool<RegisteredEliminateInfo>.Release(_registeredEliminateList[fi]);
				_registeredEliminateList.RemoveAt(fi);
			}
		}

		/// <summary>
		/// <para>造成斩杀效果时，试图刷新技能CD</para>
		/// </summary>
		/// <param name="ds"></param>
		private void _ABC_TryRefreshCDOnEliminate(DS_ActionBusArguGroup ds)
		{
			var dar = ds.GetObj1AsT<RP_DS_DamageApplyResult>();
			if (dar.RelatedProjectileRuntimeRef == null)
			{
				return;
			}
			if (!SkillPowerAffectLayoutUID.Contains(dar.RelatedProjectileRuntimeRef.SelfLayoutConfigReference
				.LayoutContentInSO.LayoutUID))
			{
				return;
			}
			//技能已经结束，则立刻刷新
			if (SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType ==
			    PlayerAnimationMotionProgressTypeEnum.NoneSelf_不是自己)
			{
				_selfRegisteredToRefreshCD = false;
				RemainingCoolDownDuration = 0f;
			}
			else
			{
				_selfRegisteredToRefreshCD = true;
				//回血
				float MaxHp = _characterBehaviourRef.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.HPMax_最大HP)
					.CurrentValue;
				_characterBehaviourRef.ReceiveBuff_GetFloatPresentValue(RP_DataEntry_EnumType.CurrentHP_当前HP)
					.AddDataEntryModifier(Float_ModifyEntry_RPDataEntry.GetNewFromPool(MaxHp * EliminateReplyHP,
						RPDM_DataEntry_ModifyFrom.FromBuff_Buff,
						ModifyEntry_CalculatePosition.FrontAdd));
				_vfxInfo_ReplyHP = _VFX_GetAndSetBeforePlay(_vfx_ReplyHP, true)?._VFX__10_PlayThis(true, true);
			}
		}


		protected override void PAEC_SpawnLayout(PAEC_生成版面_SpawnLayout paec, bool autoStart)
		{
			var layoutRef = paec.RelatedConfig.SpawnLayout_NoAutoStart(_characterBehaviourRef);
			var fi = layoutRef.LayoutContentInSO.FunctionComponentsOverride.FindIndex((component =>
				component is PFC_斩杀_Eliminate));
			if (fi == -1)
			{
				layoutRef.LayoutContentInSO.FunctionComponentsOverride.Add(new PFC_斩杀_Eliminate());
				fi = layoutRef.LayoutContentInSO.FunctionComponentsOverride.Count - 1;
			}
			PFC_斩杀_Eliminate pfc = layoutRef.LayoutContentInSO.FunctionComponentsOverride[fi] as PFC_斩杀_Eliminate;
			pfc.AvailableToElite = true;
			pfc.EliminatePartial_ToNormal = ActAsCharged ? _eliminateBloodLine_Charged : _eliminateBloodLine_NonCharge;
			pfc.EliminatePartial_ToElite =
				ActAsCharged ? _eliminateBloodLine_Elite_Charged : _eliminateBloodLine_Elite_NonCharge;
			layoutRef.LayoutHandlerFunction.OverrideSpawnFromDirection = RecordedAttackDirection ??
			                                                             _characterBehaviourRef
				                                                             .GetCurrentPlayerFaceDirection();
			layoutRef.LayoutHandlerFunction.StartLayout();
			_layoutList.Add(layoutRef);
		}


		protected override void _InternalSkillEffect_SkillDefaultFinishEffect()
		{
			base._InternalSkillEffect_SkillDefaultFinishEffect();
			if (_selfRegisteredToRefreshCD)
			{
				RemainingCoolDownDuration = 0f;
				_selfRegisteredToRefreshCD = false;
			}
			if (_registerRestoreNonCharged)
			{
				ActAsCharged = false;
				SelfPlayerSkillAnimationMotion._ancn_PrepareAnimationName = _pam_ani_NormalPrepareAnimation;
				SelfPlayerSkillAnimationMotion._ancn_MiddlePartAnimationName = _pam_ani_NormalReleaseAnimation;
				SelfPlayerSkillAnimationMotion._ancn_PostPartAnimationName = _pam_ani_NormalPostAnimation;
			}
		}





		/// <summary>
		/// <para>当猩月黯影buff添加时，将PAM更换为充能的</para>
		/// </summary>
		/// <param name="ds"></param>
		private void _ABC_ChangePAMToCharged(DS_ActionBusArguGroup ds)
		{
			if ((RolePlay_BuffTypeEnum)ds.IntArgu1.Value != RolePlay_BuffTypeEnum.RedFang_BloodOverflow_血气已满)
			{
				return;
			}

			ActAsCharged = true;
			_registerRestoreNonCharged = false;
			SelfPlayerSkillAnimationMotion._ancn_PrepareAnimationName = _pam_ani_ChargedPrepareAnimation;
			SelfPlayerSkillAnimationMotion._ancn_MiddlePartAnimationName = _pam_ani_ChargedReleaseAnimation;
			SelfPlayerSkillAnimationMotion._ancn_PostPartAnimationName = _pam_ani_ChargedPostAnimation;
		}


		/// <summary>
		/// <para>当猩月黯影buff移除时，将PAM更换为普攻的</para>
		/// </summary>
		/// <param name="ds"></param>
		private void _ABC_ChangePAMToNormal(DS_ActionBusArguGroup ds)
		{
			if ((RolePlay_BuffTypeEnum)ds.IntArgu1.Value != RolePlay_BuffTypeEnum.RedFang_BloodOverflow_血气已满)
			{
				return;
			}

			_registerRestoreNonCharged = true;
		
		}




		public override DS_ActionBusArguGroup ClearBeforeRemove()
		{
			_characterBehaviourRef.ReleaseSkill_GetActionBus()
				.RemoveAction(ActionBus_ActionTypeEnum.L_Buff_OnBuffInitialized_一个Buff被确定初始化了, _ABC_ChangePAMToCharged);
			_characterBehaviourRef.ReleaseSkill_GetActionBus()
				.RemoveAction(ActionBus_ActionTypeEnum.L_Buff_OnBuffRemoved, _ABC_ChangePAMToNormal);
			_characterBehaviourRef.ReleaseSkill_GetActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_Damage_ToCaster_ResultToEliminateSlash_施加方_造成了斩杀效果,
				_ABC_TryRefreshCDOnEliminate);
			_characterBehaviourRef.ReleaseSkill_GetActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.G_PC_OnCurrentUsingCharacterChanged_当前使用的角色更换,
				_ABC_TryDisableAllOutlineOnChangeToOtherCharacter);
			return base.ClearBeforeRemove();
		}


		public override Sprite GetCurrentSprite(DamageTypeEnum @override = DamageTypeEnum.None)
		{
			if (ActAsCharged)
			{
				return SpritePairs.Find((pair => pair.Desc.Contains("已充能"))).SpriteAsset;
			}
			else
			{
				return SpritePairs.Find((pair => pair.Desc.Contains("未充能"))).SpriteAsset;
			}
		}

		protected override bool IfAnimationConfigBelongSelf(string configName)
		{
			if (configName.Equals(_pam_ani_NormalPostAnimation) || configName.Equals(_pam_ani_NormalPrepareAnimation) ||
			    configName.Equals(_pam_ani_NormalReleaseAnimation) ||
			    configName.Equals(_pam_ani_ChargedPostAnimation) ||
			    configName.Equals(_pam_ani_ChargedPrepareAnimation) ||
			    configName.Equals(_pam_ani_ChargedReleaseAnimation))
			{
				return true;
			}
			return false;
		}

	}
}