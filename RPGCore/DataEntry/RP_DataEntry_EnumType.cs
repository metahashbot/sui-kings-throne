namespace RPGCore.DataEntry
{
    public enum RP_DataEntry_EnumType
    {
        None = 0 ,
        
        CharacterLevel = 10,
        CurrentExp = 11,
		LevelUpExp = 12,
		DropExp = 13,


        #region 50,000 ~ 99,999 是通用的RPG体系数值



        M_Strength_主力量 = 50001,
	    M_Dexterity_主敏捷 = 50002,
	    M_Vitality_主体质 = 50003,
	    M_Spirit_主精神 = 50004,
	    M_Intellect_主智力 = 50005,
	    M_Charm_主魅力 = 50006,
	    
	    HPMax_最大HP = 50011,
	    CurrentHP_当前HP = 50012,
        HPSRegen_每秒HP回复 = 50013,

        SPMax_最大SP = 50016,
	    CurrentSP_当前SP = 50017,
        SPSRegen_每秒SP回复 = 50018,

        // UPMax_最大UP = 50018,
        // CurrentUP_当前UP = 50019,

        AttackPower_攻击力 = 50021,
	    Defense_防御力 = 50022,
	    AttackSpeed_攻击速度 = 50023,
	    /// <summary>
	    /// 初始值是0，100表示每消耗1s的时间算作2s的cd
	    /// </summary>
	    SkillAccelerate_技能加速 = 50024,
	    Toughness_韧性 = 50025,
	    CriticalRate_暴击率 = 50026,
	    CriticalBonus_暴击伤害 = 50027,
	    Accuracy_命中率 = 50028,
	    DodgeRate_闪避率 = 50029,
	    
	    SkillCDReduce_技能CD缩减 = 50031,
	    
	    CorpseDuration_尸体存在时间= 50041,
		OverDamageOverride_过伤覆盖时长 = 50042,
	    

	    MoveSpeed_移速 = 53001,
	    MovementMass_重量 =  53002,
	    
	    
	    
	    
	    AnimationSpeed_动画速度 = 58901,
	    
	    
	    /// <summary>
	    /// 这个是【额外】加速，如果这个数是0那就表明不需要额外加速。百分比，40%表示动画会以1.4倍速播放。如果是负数就会变慢，
	    /// <para> 23.9.27 当前版本仅对 Sheet动画的Default播放参数生效</para>
	    /// </summary>
	    SkillCastingAccelerate_技能施法额外加速 = 58902,
	    
	    
	    
	    DeathNeutralizeRange_死亡消弹范围 = 60003,
	    
	    
	    
	    
	    

#endregion

    }
}
