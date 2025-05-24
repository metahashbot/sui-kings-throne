using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace RPGCore.Skill.SkillSelector
{
    public class SkillIndicator_Range : SkillIndicatorBase
    {
        [SerializeField, Required, LabelText("基准贴花")]
        private DecalProjector _mainDecal;
        private Material _mainDecalMaterial => _mainDecal.material;

        [SerializeField, LabelText("y轴修正高度？")]
        private float _yOffset = 0.25f;

        [SerializeField,LabelText("包含装饰层？")]
        private bool _selfContainDecoration;

        [SerializeField, Required, LabelText("装饰贴花"), ShowIf(nameof(_selfContainDecoration))]
        private DecalProjector _decorationDecal;
        private Material _decorationDecalMaterial => _decorationDecal.material;
        
        
        

        public SkillIndicator_Range SetMainColor(Color mainColor)
        {

            _mainDecalMaterial.SetColor(_mp_MainColor, mainColor);
            return this;
        }
        public SkillIndicator_Range SetDecoration(Color mainColor, Color secondaryColor, float distortSpeed)
        {
            if (_selfContainDecoration)
            {
                _decorationDecalMaterial.SetColor(_mp_MainColor, mainColor);
                _decorationDecalMaterial.SetColor(_mp_SecondaryColor, secondaryColor);
                _decorationDecalMaterial.SetFloat(_mp_NoiseSpeed, distortSpeed);
            }
            return this;
        }
        public SkillIndicator_Range SetEmission(Color emissionColor, float emission)
        {
            _mainDecalMaterial.SetColor(_mp_EmissionColor, emissionColor);
            _mainDecalMaterial.SetFloat(_mp_EmissionPower, emission);
            if (_selfContainDecoration)
            {
                _decorationDecalMaterial.SetColor(_mp_EmissionColor, emissionColor);
                _decorationDecalMaterial.SetFloat(_mp_EmissionPower, emission);
            }
            return this;
        }

        public SkillIndicator_Range SetFromPosition(Vector3 pos)
        {
            transform.position = pos + Vector3.up * _yOffset;
            return this;
        }

        public SkillIndicator_Range SetRadius(float radius)
        {
            Vector3 targetSize = new Vector3(radius * 2f, radius * 2f, 10f);
            _mainDecal.size = targetSize;
            if (_selfContainDecoration)
            {
                _decorationDecal.size = targetSize;
            }
            return this;
        }

        public SkillIndicator_Range SetSize(Vector2 sizeInXZ)
        {
            Vector3 targetSize = new Vector3(sizeInXZ.x, sizeInXZ.y, 10f);
            _mainDecal.size = targetSize;
            if (_selfContainDecoration)
            {
                _decorationDecal.size = targetSize;
            }
            return this;
        }

        public void UpdateTick(Vector3 fromPosition)
        {
            fromPosition.y += _yOffset;
            transform.position = fromPosition;

        }
        
    }
}