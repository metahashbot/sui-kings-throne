using Global.ActionBus;
using RPGCore.Buff;
using RPGCore.DataEntry;
using RPGCore.Skill;
using UnityEngine;

namespace RPGCore.Interface
{
    /// <summary>
    /// <para>表明一个RPObject可能可以释放技能</para>
    /// <para>RP Behaviour</para>
    /// </summary>
    public interface I_RP_ObjectCanReleaseSkill
    {
        public abstract Float_RPDataEntry ReleaseSkill_GetRelatedFloatDataEntry(RP_DataEntry_EnumType type);
        public abstract FloatPresentValue_RPDataEntry ReleaseSkill_GetPresentDataEntry(RP_DataEntry_EnumType type);
        public abstract BuffAvailableType ReleaseSkill_CheckTargetBuff(RolePlay_BuffTypeEnum type);
    
        public abstract BaseRPBuff ReleaseSkill_GetRelatedBuff(RolePlay_BuffTypeEnum buffType);
        public abstract LocalActionBus ReleaseSkill_GetActionBus();
        
        public abstract BaseRPSkill ReleaseSkill_GetTargetSkill(RPSkill_SkillTypeEnum skillType);


        public abstract Vector3 GetCasterFromPosition(bool alignY = true);

        public abstract RolePlay_ArtHelperBase ReleaseSkill_GetRelatedArtHelper();

        public virtual I_RP_ContainVFXContainer GetRelatedVFXContainer()
        {
            return ReleaseSkill_GetRelatedArtHelper() as I_RP_ContainVFXContainer;
        }



    }
}
