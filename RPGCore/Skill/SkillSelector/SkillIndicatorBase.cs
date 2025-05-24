using RPGCore.Skill.Config.Selector;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace RPGCore.Skill.SkillSelector
{
    public abstract class SkillIndicatorBase : MonoBehaviour
    {
        protected static readonly int _mp_MainColor = Shader.PropertyToID("_MainColor");
        
        protected static readonly int _mp_SecondaryColor = Shader.PropertyToID("_Secondary_Color");
        protected static readonly int _mp_NoiseSpeed = Shader.PropertyToID("_NoiseSpeed");
        protected static readonly int _mp_EmissionColor = Shader.PropertyToID("_EmissionColor");
        protected static readonly int _mp_EmissionPower = Shader.PropertyToID("_EmissionPower");
        protected static readonly int _mp_Expand = Shader.PropertyToID("_Expand");
    }
}