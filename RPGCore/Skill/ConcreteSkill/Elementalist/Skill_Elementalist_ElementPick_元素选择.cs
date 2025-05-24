using System;
using RPGCore.Buff;
using RPGCore.Buff.ConcreteBuff;
using RPGCore.PlayerAnimationMotion;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Skill.ConcreteSkill.Elementalist
{
	[Serializable]
	public class Skill_Elementalist_ElementPick_元素选择 : BaseRPSkill
	{
		/*
		 * 普通前摇（空垫帧）
施法中（空垫帧）
不可取消
普通后摇（空垫帧）
生成一个和元素选择结果匹配的特效，用来提示将要切换的元素属性
包含多段脱手
多段前摇（空垫帧）
多段施法中（空垫帧）不可取消
将元素属性切换过去。
多段后摇（空垫帧）
结束多段施法过程，结束整个技能
		 */ 
		
		[SerializeField,LabelText("元素选择间隔"),TitleGroup("===配置===")]
		public float _elementPickInterval = 1f;
		[SerializeField, LabelText("元素切换"), FoldoutGroup("配置", true), TitleGroup("配置/VFX"),
		 GUIColor(187f / 255f, 1f, 0f)]
		public string _vfxId_element_switch;
		private PerVFXInfo _vfxInfo_Element_Switch;
		[SerializeField, LabelText("火反馈特效"), FoldoutGroup("配置", true), TitleGroup("配置/VFX"),
		 GUIColor(187f / 255f, 1f, 0f)]
		public string _vfx_fire_handoff;
		private PerVFXInfo _vfxInfo_Fire_HandOff;
		[SerializeField, LabelText("水反馈特效"), FoldoutGroup("配置", true), TitleGroup("配置/VFX"),
		 GUIColor(187f / 255f, 1f, 0f)]
		public string _vfx_water_handoff;
		private PerVFXInfo _vfxInfo_Water_HandOff;
		[SerializeField, LabelText("风反馈特效"), FoldoutGroup("配置", true), TitleGroup("配置/VFX"),
		 GUIColor(187f / 255f, 1f, 0f)]
		public string _vfx_wind_handoff;
		private PerVFXInfo _vfxInfo_Wind_HandOff;
		[SerializeField, LabelText("土反馈特效"), FoldoutGroup("配置", true), TitleGroup("配置/VFX"),
		 GUIColor(187f / 255f, 1f, 0f)]
		public string _vfx_earth_handoff;
		private PerVFXInfo _vfxInfo_Earth_HandOff;
		
		protected override bool IfReactToInput_OffhandState()
		{
			if (_Internal_CheckIfContainsFlagBuff(RP_BuffInternalFunctionFlagTypeEnum.DisableCommonMovement_禁用普通移动 |
			                                      RP_BuffInternalFunctionFlagTypeEnum.BlockByStrongStoic_被强霸体屏蔽 |
			                                      RP_BuffInternalFunctionFlagTypeEnum.ResistByWeakStoic_被弱霸体抵抗))
			{
				return false;
			}
			return true;
		}

        /// <summary>
        /// <para>根据当前元素切换元素</para>
        /// </summary>
        public override bool _Internal_TryPrepareSkill()
        {
            if (!base._Internal_TryPrepareSkill())
            {
                return false;
            }
            else
            {
                var changeDamageTypeBuffRef = _characterBehaviourRef.ReleaseSkill_GetRelatedBuff(
                        RolePlay_BuffTypeEnum.ChangeCommonDamageType_常规伤害类型更改) as Buff_ChangeCommonDamageType;

                switch (changeDamageTypeBuffRef.CurrentDamageType)
                {
                    case DamageTypeEnum.AoNengFeng_奥能风:
                        changeDamageTypeBuffRef.AssignAndBroadcastChangeDamageType(DamageTypeEnum.AoNengHuo_奥能火);
                        _vfxInfo_Fire_HandOff = _VFX_GetAndSetBeforePlay(_vfx_fire_handoff)?._VFX__10_PlayThis();
                        _vfxInfo_Element_Switch = _VFX_GetAndSetBeforePlay(_vfxId_element_switch)?.VFX_StopThis(true);
                        break;
                    case DamageTypeEnum.AoNengHuo_奥能火:
                        changeDamageTypeBuffRef.AssignAndBroadcastChangeDamageType(DamageTypeEnum.AoNengShui_奥能水);
                        _vfxInfo_Water_HandOff = _VFX_GetAndSetBeforePlay(_vfx_water_handoff)?._VFX__10_PlayThis();
                        _vfxInfo_Element_Switch = _VFX_GetAndSetBeforePlay(_vfxId_element_switch)?.VFX_StopThis(true);
                        break;
                    case DamageTypeEnum.AoNengShui_奥能水:
                        changeDamageTypeBuffRef.AssignAndBroadcastChangeDamageType(DamageTypeEnum.AoNengTu_奥能土);
                        _vfxInfo_Earth_HandOff = _VFX_GetAndSetBeforePlay(_vfx_earth_handoff)?._VFX__10_PlayThis();
                        _vfxInfo_Element_Switch = _VFX_GetAndSetBeforePlay(_vfxId_element_switch)?.VFX_StopThis(true);
                        break;
                    case DamageTypeEnum.AoNengTu_奥能土:
                        changeDamageTypeBuffRef.AssignAndBroadcastChangeDamageType(DamageTypeEnum.AoNengFeng_奥能风);
                        _vfxInfo_Wind_HandOff = _VFX_GetAndSetBeforePlay(_vfx_wind_handoff)?._VFX__10_PlayThis();
                        _vfxInfo_Element_Switch = _VFX_GetAndSetBeforePlay(_vfxId_element_switch)?.VFX_StopThis(true);
                        break;
                }
                return true;
            }
        }

        protected override void _InternalProgress_Post_CompleteOn_Post()
        {
            base._InternalProgress_Post_CompleteOn_Post();
            _InternalSkillEffect_SkillDefaultFinishEffect();
        }
	}
}