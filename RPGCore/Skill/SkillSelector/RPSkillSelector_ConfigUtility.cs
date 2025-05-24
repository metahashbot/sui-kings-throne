using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RPGCore.Skill.SkillSelector
{
    [Serializable]
    public class RPSkillIndicatorConfig_Sector
    {
        public RPSkillIndicatorType_Sector IndicatorType;
        [HideIf("@IndicatorType == RPSkillIndicatorType_Sector.None")]
        public Color MainColor = Color.blue;
        [HideIf("@IndicatorType == RPSkillIndicatorType_Sector.None")]
        public Color SecondaryColor = Color.cyan;
    
    }

    public enum RPSkillIndicatorType_Sector
    {
	    None = 0,
        Sector_NoBorder_Simple_P1 = 1001,
        Sector_NoBorder_Holo_P1 = 1201,
        Sector_HasBorder_Simple_P1 = 1401,
        Sector_HasBorder_Holo_P1 = 1601,
    }



    [Serializable]
    public class RPSkillIndicatorConfig_Line
    {
        public RPSkillIndicatorType_Line IndicatorType;
        public Color MainColor = Color.blue;
    
    }

    public enum RPSkillIndicatorType_Line
    {
        Line_A1L1 = 2001,
        Line_A1L2 = 2002,
        Line_A1L3 = 2003, 
        Line_A2L2 = 2004,
        Line_A3L2 = 2005,
        Line_A4L2 =2006,


    }


    [Serializable]
    public class RPSkillIndicatorConfig_Direction
    {
        public RPSkillIndicatorType_Direction IndicatorType;
        public Color MainColor = Color.blue;
        public Color FillColor = Color.cyan;

    }

    public enum RPSkillIndicatorType_Direction
    {
        Direction_P1 = 3001,
        Direction_P2 = 3002,
        Direction_P3  = 3003,

    }

    [Serializable]
    public class RPSkillIndicatorConfig_Position
    {
        public RPSkillIndicatorType_Position indicatorType;
        public Color MainColor = Color.blue;
        public Color FillColor = Color.cyan;
#if UNITY_EDITOR
        [ShowIf(nameof(ShowDeco))]
#endif
        public Color DecorationMainColor = Color.blue;
#if UNITY_EDITOR
        [ShowIf(nameof(ShowDeco))]
#endif
        public Color DecorationSecondaryColor = Color.cyan;

#if UNITY_EDITOR
        private bool ShowDeco()
        {
            return (int)indicatorType > 4500;
        }
    
#endif

    }

    public enum RPSkillIndicatorType_Position
    {
        Position_NoDeco_P1 = 4001,
        Position_NoDeco_P2 =4002,
        Position_NoDeco_P3 = 4003,
        Position_HasDeco_P1 = 4501,
        Position_HasDeco_P2 = 4502,
        Position_HasDeco_P3 = 4503,
        
        PartialCircle_NoDeco_P1 = 4601

    }

    /// <summary>
    /// <para>5500以下为不带装饰的，5501及以上为带装饰的</para>
    /// </summary>
    [Serializable]
    public class RPSkillIndicatorConfig_Range
    {
        public RPSkillIndicatorType_Range indicatorType;
        public Color MainColor = Color.yellow;
#if UNITY_EDITOR
        [ShowIf(nameof(showDeco))]
#endif
        public Color DecorationMainColor = Color.yellow;
#if UNITY_EDITOR
        [ShowIf(nameof(showDeco))]
#endif
        public Color DecorationSecondaryColor = Color.cyan;

#if UNITY_EDITOR
        private bool showDeco()
        {
            return (int)indicatorType > 5500;
        }
#endif
    
    }

    public enum RPSkillIndicatorType_Range
    {
        Range_NoDeco_P1 = 5001,
        Range_NoDeco_P2 = 5002,
        Range_NoDeco_P3 = 5003,
        Range_HasDeco_P1 = 5501,
        Range_HasDeco_P2 = 5502,
        Range_HasDeco_P3 = 5503,
        Range_HasDeco_P4 = 5504,
        Range_HasDeco_P5 = 5505,


    }
}