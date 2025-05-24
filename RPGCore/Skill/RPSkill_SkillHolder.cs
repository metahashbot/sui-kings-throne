using System.Collections.Generic;
using Global;
using Global.GlobalConfig;
using Global.ActionBus;
using Global.Character;
using RPGCore.Buff;
using RPGCore.Buff.Requirement;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.Projectile.Layout;
using RPGCore.Skill.Config;
using RPGCore.Skill.Config.Requirement;
using Sirenix.OdinInspector;
using UnityEngine;

#pragma warning disable 1522
namespace RPGCore.Skill
{
    public partial class RPSkill_SkillHolder
    
    {
        [ShowInInspector,LabelText("释放者")]
        private I_RP_ObjectCanReleaseSkill _parentCanReleaseSkill;

        [ShowInInspector,LabelText("运行时技能容器")]
        protected Dictionary<RPSkill_SkillTypeEnum, SOConfig_RPSkill> _selfRelatedSkillDictionary;


        public void GetCurrentSkillList(List<SOConfig_RPSkill> skillList)
        {
            foreach (var perSKill in _selfRelatedSkillDictionary.Values)
            {
                skillList.Add(perSKill);

            }
        }

        public SOConfig_RPSkill GetSkillOnSlot(SkillSlotTypeEnum slotTypeEnum)
        {
            foreach (var perSKill in _selfRelatedSkillDictionary.Values)
            {
                if (perSKill.ConcreteSkillFunction.SkillSlot == slotTypeEnum)
                {
                    return perSKill;
                }
            }

            return null;
        }

        
        
        public SOConfig_RPSkill GetSkillRuntimeByType(RPSkill_SkillTypeEnum type)
        {
            return _selfRelatedSkillDictionary[type];
        }

        public RPSkill_SkillHolder(I_RP_ObjectCanReleaseSkill _parent)
        {
            _parentCanReleaseSkill = _parent;
            _selfRelatedSkillDictionary = new Dictionary<RPSkill_SkillTypeEnum, SOConfig_RPSkill>();
        }

        public void InitializeOnInstantiate(int id)
        {
            GCSO_PerCharacterInfo info = GlobalConfigSO.RuntimeContent()
                .AllCharacterInfoCollection.Find((characterInfo => characterInfo.CharacterID == id));
            int skillIndex = info.Components.FindIndex((component => component is CIC_PlayerSkillInfo));
            if (skillIndex != -1)
            {
                CIC_PlayerSkillInfo skillInfo = info.Components[skillIndex] as CIC_PlayerSkillInfo;
                foreach (CIC_PlayerSkillInfo.PlayerSkillInfoEntry perSkill in skillInfo.ObtainedSkillList)
                {
                    AddNewSkillToHolder(perSkill.SkillType, perSkill.SkillLevel, perSkill.SkillSlot);
                }
            }
            
            
            
            
        }



#region 初始化 & 增加


        /// <summary>
        /// <para>试图添加一个技能，带有等级。</para>
        /// <para>如果已经拥有，则会先移除再添加</para>
        /// </summary>
        public void AddNewSkillToHolder(RPSkill_SkillTypeEnum type, int level, SkillSlotTypeEnum slot)
        {
            //总是会试图移除重复
            RemoveSkill(type);

            SOConfig_RPSkill configRaw = GlobalConfigurationAssetHolderHelper.Instance.Collection_SkillConfig
                .GetRPSkillConfigByTypeAndLevel(type, level);
            SOConfig_RPSkill configRuntime = Object.Instantiate(configRaw);
            configRuntime.ConcreteSkillFunction.InitOnObtain(this, configRuntime, _parentCanReleaseSkill ,slot);



            _parentCanReleaseSkill.ReleaseSkill_GetActionBus().TriggerActionByType(
                ActionBus_ActionTypeEnum.L_Skill_OnCharacterObtainNewSkill_角色获得新技能,
                new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_Skill_OnCharacterObtainNewSkill_角色获得新技能,
                    configRuntime));

            _selfRelatedSkillDictionary.Add(type, configRuntime);

        }



#endregion


#region Tick


        public void UpdateTick(float currentTime, int currentFrameCount, float delta)
        {
            foreach (var skillRuntime in _selfRelatedSkillDictionary.Values)
            {
                skillRuntime.ConcreteSkillFunction.UpdateTick(currentTime, currentFrameCount, delta);
            }
        }

        public void FixedUpdateTick(float currentTime, int currentFrameCount,float delta)
        {
            foreach (var skillRuntime in _selfRelatedSkillDictionary.Values)
            {
                skillRuntime.ConcreteSkillFunction.FixedUpdateTick(currentTime, currentFrameCount,delta);
            }
        }

#endregion



#region 移除



        public void RemoveSkill(RPSkill_SkillTypeEnum type)
        {
            if (_selfRelatedSkillDictionary.ContainsKey(type))
            {
                SOConfig_RPSkill targetConfig = _selfRelatedSkillDictionary[type];
                targetConfig.ConcreteSkillFunction.ClearBeforeRemove();
                Object.Destroy(targetConfig);
                _selfRelatedSkillDictionary.Remove(type);
            }
        }


#endregion

#region 查询



        /// <summary>
        /// <para>获取指定技能的可用状态。</para>
        /// </summary>
        public SkillReadyTypeEnum GetTargetSkillTypeReadyType(RPSkill_SkillTypeEnum skillType)
        {
            if (skillType == RPSkill_SkillTypeEnum.None)
            {
                return SkillReadyTypeEnum.None;
            }

            if (!_selfRelatedSkillDictionary.ContainsKey(skillType))
            {
                return SkillReadyTypeEnum.None;
            }

            return _selfRelatedSkillDictionary[skillType].ConcreteSkillFunction.GetSkillReadyType();
        }

        
        
        

#endregion

        

    }
}