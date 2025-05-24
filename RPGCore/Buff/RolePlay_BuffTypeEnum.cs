public enum RolePlay_BuffTypeEnum
{
	None = 0,
	测试A = 11,
	RemoveBuff_移除Buff = 12,
	
	
	StiffnessOnHit_受击硬直 = 101,
	Silence_DisableAllSkill = 102,
	DragMovement_牵引推拉 = 103,
	UnbalanceMovement_失衡推拉 = 104,
	
	WeakStoic_弱霸体 = 111,
	StrongStoic_强霸体 = 112,
    LevelUp_升级 = 113,
    Recovery_治疗 = 114,


    RegisterToDeath_已记录死亡 = 121,
	DeathNoRegister_死亡时不会记录 = 122,
	
	
	
	
	
	InvincibleByDirector_All_WD_来自机制的完全无敌 = 1010,
	Invincible_All_WD_完全无敌 = 1011,
	Invincible_Physics_物理免疫 = 1012,
	Invincible_Magical_法术免疫 = 1013,
	ShieldObtainBonus_护盾额外增量 = 1041,
	
	ProcessDamageOffsetViaFrontOrBack_背后或正面伤害修正 = 1101,
	
	
	CriticalBonusOffset_暴击倍率修正 = 1312,
	SkillCoolDownBoost_技能加速 = 1320,
	NormalAttackDamageOffset_普攻伤害倍率修正 = 1321,
	
	
	ColdStack_严寒可叠层 = 1357,
	Frostbite_冻伤 = 1358,
	Warm_A1_温暖_A1 = 1359,
	DragonFlame_龙炎值 =1360,
    DragonFlameDefence_龙炎守御 = 1361,
    DragonFlameFireArea_烈鸟龙火圈 = 1362,
    FireAreaEffectToPlayerByFlameDragon_烈鸟龙的火圈对玩家效果 = 1363,
    SafeAreaAlongFlameRoarByFlameDragon_龙炎悲鸣期间的安全区域 = 1364,
    BurningAndFireBeadByFlameDragon_焚焰与喷射炎珠 = 1365,
    FireCoreInstantDeathEffectByFlameDragon_烈鸟龙火核的秒杀效果 = 1366,
    RingOfFireBurns_火圈灼烧 = 1367,




    DeathResist_SWKJ_死亡抗拒 = 46001,
	LockHealthPower_SX_锁血 = 46002,
	Silence_沉默_CM = 47001,
	Taunt_嘲讽_CF = 47002,
	ThreatReduce_威胁降低_WXJD = 47003,
	TransparentAndCloaking_透明隐身 = 47004,
	TriggerGameplayEventByCurrentHP_血量触发事件 = 49005,
	LightSuppress_光等压制 = 50000,
	Dizzy_眩晕 = 50001,
	BloodMark_鲜血咒印 = 100001,
	EagleEye_鹰眼 = 100002,
	Buff_FromBuff_来自破绽剑意_WillOfSword = 100003,
	BloodGasValue_血气值 = 100004,
	BloodPoison_血毒 = 100005,
	RedFang_BloodOverflow_血气已满 = 100006,
	RedFang_蝙蝠_Bat = 100007,
	
	DisableSkillCoolDown_技能停止CD = 110101,
	ChangeCommonDamageType_常规伤害类型更改 = 110201,
	
	
	
	Frozen_冻结_DJ = 110205,
	Poisoning_中毒 = 111001,



#region 技能部分

	FromSkill_BladeDanceA1_剑舞A1 = 120101,
	FromSkill_EnergyBurst_能量狂涌 = 120102,
	FromSkill_剑心_HeartOfSword = 120103,
	FromSkill_来自看破的破绽标记_WeaknessMarkFromSeeThrough = 120104,
	FromSkill_Elementalist_ElementalChainBullet_元素使奥术飞弹 = 120201,
	FromSkill_RedFang_ScarletDarkness_猩月黯影 = 120301,
	Aura_CommonHealAura_通用治疗光环 = 140101,
	Aura_SimpleFireAura_简单灼烧光环 = 140102,
	Aura_SimplePushAura_简单无硬直推人光环 = 140103,
	Aura_SimplePullAura_简单无硬直拉人光环 = 140104,
	

#endregion

#region 通用Buff

	DamageTruncate_伤害截断 = 150001,
	CommonDataEntryModifyStackable_通用数据项修正可叠层 = 150002,
	CommonEnemyWeakness_通用敌人弱点 = 150003,
	DamageTweak_NormalEnemy_普通敌人伤害微调 = 150004,
	DamageTweak_EliteEnemy_精英敌人伤害微调 = 150005,

#endregion

#region 敌人角色专属

	ForestBigPurpleFlower_森林大紫花 = 160001,

#endregion

#region 甲盾

	Shield_Energy_属性能量甲盾 = 170001,

#endregion

#region 关卡机制

	Level_图腾回血_HealTotem = 180001,
	Level_图腾免伤_ResistTotem = 180002,
	Level_图腾召唤_SummonTotem = 180003,
	Level_攻击反回血图腾_AttackBonusAntiHeal = 180011,
	Level_防御反免伤图腾_DefenseBonusAntiResist = 180012,
	Level_速度反召唤图腾_SpeedBonusAntiSummon = 180013,
	
	Level_来自图腾的免伤_ResistFromTotem = 180021,
	
	
	Level_雪山图腾_SnowTotemLevel =  180030,
	Level_角色受雪山图腾柱影响的叠层效果_CharacterAffectByTotemInLevelSnow = 180031,
	Level_雪山篝火_SnowBonfireLevel = 180032,
	
	

#endregion


#region 元素效果部分

	Frozen_结冰_JB = 200001,
	OnFiring_着火_ZH = 200002,
	BaseFirstElementHandler_基本一级元素反应处理 = 210001,
	ElementFirstLightTag_Guang_一级光标签 = 210301,
	ElementFirstElectricTag_Dian_一级电标签 = 210302,
	ElementFirstFireTag_Huo_一级火标签 = 210303,
	ElementFirstWaterTag_Shui_一级水标签 = 210304,
	ElementFirstWindTag_Feng_一级风标签 = 210305,
	ElementFirstEarthTag_Tu_一级土标签 = 210306,
	ElementFirstSoulTag_Ling_一级灵标签 = 210307,
	ElementSecondElectricalShock_GanDian_二级感电 = 210321,
	
	ElementSecondDazzle_XuanGuang_二级炫光 = 210322,
	ElementSecondSpiritOppression_LingWei_二级灵威 = 210323,
	ElementSecondFog_NingWu_二级凝雾 = 210324,
	ElementSecondSandStorm_ShaJuan_二级砂卷 = 210325,
	ElementSecondFireStorm_RongJuan_二级熔卷 = 210326,
	ElementSecondFrostStorm_ShuangJuan_二级霜卷 = 210327,
	ElementSecondThunderBoom_LeiBao_二级雷暴 = 210351,
	ElementSecondCrush_BengJie_二级崩解 = 210352,
	ElementSecondOverload_ChaoZai_二级超载 = 210353,
	ElementSecondWindSuppress_FengYa_二级风压 = 210354,
	
	
	
	
	ElementThirdCollapse_BengHuai_三级崩坏 = 210381,
	ElementThirdSublimation_ShengHua_三级升华 = 210382,
	ElementThirdThunderStorm_LeiMing_三级雷鸣 = 210383,
	ElementThirdColdHell_HanYu_三级寒狱 = 210384,
	ElementThirdLavaStorm_RongJuan_三级熔卷 = 210385,
	ElementThirdFrostDomain_ShuangTian_三级霜天 = 210386,
	ElementThirdSoulPower_LingWei_三级灵威 = 210387,
	
	
	
	
	
	Enemy_CommonShieldBuilding_基本盾构 = 220000,
	Enemy_ShockWaveShieldBuilding_冲击波盾构 = 220001,
	Enemy_CommonWild_基本狂化 = 220011,
	Enemy_SingleBattleWild_单体战斗狂化 = 220012,
	Enemy_CommonSplit_通用分裂 = 220021,

#endregion

	ARPG_2PUtil_STRConversion_ARPG力量转换 = 300001,
	ARPG_2PUtil_DEXConversion_ARPG敏捷转换 = 300002,
	ARPG_2PUtil_VITConversion_ARPG体质转换 = 300003,
	ARPG_2PUtil_SPRConversion_ARPG精神转换 = 300004,
	ARPG_2PUtil_INTConversion_ARPG智力转换 = 300005,
    ARPG_2PUtil_INTConversion_ARPG魅力转换 = 300006,
    SRPG_SyncHPPartialOnMaxChanged_当HP最大值变动时同步当前 = 300010,
	SRPG_SyncSPPartialOnMaxChanged_当SP最大值变动时同步当前 = 300011,
	ARPG_PlayerUltraPowerUtility_玩家基本UP功能 = 300012,
	ARPG_PlayerSwitchCharacterBlock_玩家更换角色阻塞 = 300013,


    StrengthEnhance_主力量提升 = 310001,
    DexterityEnhance_主敏捷提升 = 310002,
    VitalityEnhance_主体质提升 = 310003,
    SpiritEnhance_主精神提升 = 310004,
    IntellectEnhance_主智力提升 = 310005,
    CharmEnhance_主魅力提升 = 310006,
    HPMaxEnhance_最大HP提升 = 310007,
    HPSRegenEnhance_每秒HP回复提升 = 310008,
    SPMaxEnhance_最大SP提升 = 310009,
    SPSRegenEnhance_每秒SP回复提升 = 310010,
    AttackPowerEnhance_攻击力提升 = 310011,
    DefenseEnhance_防御力提升 = 310012,
    AttackSpeedEnhance_攻击速度提升 = 310013,
    CriticalRateEnhance_暴击率提升 = 310014,
    CriticalBonusEnhance_暴击伤害提升 = 310015,


    #region 装备Perk

    EP_暗杀者_Assassin = 400001,
	EP_奥能盾卫_ArcaneShielder = 400002,
	EP_奥能猎手_ArcaneHunter = 400003,
	EP_薄暮_Dusk = 400004,
	EP_悲亡者_Mourner = 400005,
	EP_超凡思维_TranscendentalMind = 400006,
	EP_沉稳步伐_SteadyStride = 400007,
	EP_地鸣攻_EarthshakerAttack = 400008,
	EP_地鸣卫_EarthshakerDefense = 400009,
	EP_电极攻_ElectrodeAttack = 400010,
	EP_电极卫_ElectrodeDefense = 400011,
	EP_愤怒伤害_RageDamage = 400012,
	EP_风暴攻_StormAttack = 400013,
	EP_风暴卫_StormDefense = 400014,
	EP_风息_WindBreath = 400015,
	EP_蝮蛇吐息_ViperBreath = 400016,
	EP_钢铁护卫_IronGuardian = 400017,
	EP_钢牙撕裂_SteelFangRipper = 400018,
	EP_光晕攻_HaloAttack = 400019,
	EP_光晕卫_HaloDefense = 400020,
	EP_嚎哭者_Howler = 400021,
	EP_护甲穿透_ArmorPenetration = 400022,
	EP_汲魂_SoulDrain = 400023,
	EP_极限武力_UltimateForce = 400024,
	EP_祭灵攻_SpiritSacrificeAttack = 400025,
	EP_祭灵卫_SpiritSacrificeDefense = 400026,
	EP_坚毅不倒_Unyielding = 400027,
	EP_精灵天敌_ElfNemesis = 400028,
	EP_精神源泉_MentalSource = 400029,
	EP_精英天敌_EliteNemesis = 400030,
	EP_精准索敌_PreciseTargeting = 400031,
	EP_苦痛打击_PainfulStrike = 400032,
	EP_狂浪攻_WildWaveAttack = 400033,
	EP_狂浪卫_WildWaveDefense = 400034,
	EP_力量源泉_PowerSource = 400035,
	EP_连环追击精神_ChainChaseSpirit = 400036,
	EP_连环追击力量_ChainChasePower = 400037,
	EP_连环追击敏捷_ChainChaseAgility = 400038,
	EP_连环追击体质_ChainChaseConstitution = 400039,
	EP_连环追击智慧_ChainChaseWisdom = 400040,
	EP_灵能盾卫_PsionicShielder = 400041,
	EP_灵能猎手_PsionicHunter = 400042,
	EP_灵巧舞者_GracefulDancer = 400043,
	EP_领主天敌_LordNemesis = 400044,
	EP_蛮种天敌_BarbarianNemesis = 400045,
	EP_秘语者_SecretSpeaker = 400046,
	EP_敏捷源泉_AgilitySource = 400047,
	EP_魔物天敌_DemonNemesis = 400048,
	EP_祈星者_Stargazer = 400049,
	EP_泣鸣者_Cryer = 400050,
	EP_青冥弦月_DarkMoonBow = 400051,
	EP_人类天敌_HumanNemesis = 400052,
	EP_锐利锋芒_SharpEdge = 400053,
	EP_森林的眷念_ForestRemembrance = 400054,
	EP_砂暴_Sandstorm = 400055,
	EP_赏金_Bounty = 400056,
	EP_生命祝福_LifeBlessing = 400057,
	EP_兽人天敌_OrcNemesis = 400058,
	EP_霜星_FrostStar = 400059,
	EP_体质源泉_ConstitutionSource = 400060,
	EP_旺盛精力_VigorousEnergy = 400061,
	EP_无尘者_Dustless = 400062,
	EP_猩红攻_CrimsonAttack = 400063,
	EP_猩红卫_CrimsonDefense = 400064,
	EP_迅捷体术_SwiftBodyArt = 400065,
	EP_亚龙天敌_DragonNemesis = 400066,
	EP_炎铸_FlameForged = 400067,
	EP_永夜天敌_EternalNightNemesis = 400068,
	EP_勇士天敌_WarriorNemesis = 400069,
	EP_幽能盾卫_PhantomShielder = 400070,
	EP_幽能猎手_PhantomHunter = 400071,
	EP_源能盾卫_PrimalShielder = 400072,
	EP_源能猎手_PrimalHunter = 400073,
	EP_炙炎攻_BlazingAttack = 400074,
	EP_炙炎卫_BlazingDefense = 400075,
	EP_致密钛金_DenseTitanium = 400076,
	EP_智慧源泉_WisdomSource = 400077,
	EP_逐梦者_DreamChaser = 400078,
	EP_自然律法_NaturalLaw = 400079,
	EPU_赏金标记_BountyMark = 400080,
	EPrefix_审判的 = 490001,
	EPrefix_灵魂的 = 490002,
	EPrefix_命运的 = 490003,
	EPrefix_裁决的 = 490004,
	EPrefix_强力的 = 490005,
	EPrefix_祝福的 = 490006,
	EPrefix_坚韧的 = 490007,
	EPrefix_威能的 = 490008,
	EPrefix_真审判的 = 490011,
	EPrefix_真灵魂的 = 490012,
	EPrefix_真命运的 = 490013,
	EPrefix_真裁决的 = 490014,
	EPrefix_真强力的 = 490015,
	EPrefix_真祝福的 = 490016,
	EPrefix_真坚韧的 = 490017,
	EPrefix_真威能的 = 490018,
	EPrefix_奥义审判的 = 490021,
	EPrefix_奥义灵魂的 = 490022,
	EPrefix_奥义命运的 = 490023,
	EPrefix_奥义裁决的 = 490024,
	EPrefix_奥义强力的 = 490025,
	EPrefix_奥义祝福的 = 490026,
	EPrefix_奥义坚韧的 = 490027,
	EPrefix_奥义威能的 = 490028,

#endregion

#region ARPG 标签

	ARPG_Tag_ClearSelfProjectileOnDeath_死亡时销毁所有自身投射物 = 5000001,

#endregion

	_EnemyTag_NormalEnemy_普通敌人 = 6010002,
	_EnemyTag_EliteEnemy_精英敌人 = 6010003,
	_EnemyTag_BossEnemy_首领敌人 = 6010004,
	_EnemyTag_WildBreed_蛮种敌人 = 6011001,
	_EnemyTag_Monster_魔物敌人 = 6011002,
	_EnemyTag_Dragon_龙类敌人 = 6011003,
	_EnemyTag_Human_人类敌人 = 6011101,
	_EnemyTag_Erf_精灵敌人 = 6011102,
	_EnemyTag_Orc_兽人敌人 = 6011103,
	_EnemyTag_Undead_永夜敌人 = 6011104,
	_EnemyTag_BySplit_分裂产生的敌人 = 6012001,
	_BehaviourTag_DeathType1_已死亡标准类型 = 7015001,
	_BehaviourTag_DeathAndReviving_已死亡但正在复活 = 7015002,
	_BuffTag_SJED_伺机而动 = 44110001,
	_BuffTag_CXZJ_处刑专家 = 44110002,
	_BuffTag_JLCP_精力充沛 = 44110013,
	_BuffTag_YJXR_养精蓄锐 = 44110014,
	_BuffTag_DZWQ_锻造武器 = 44110015, }