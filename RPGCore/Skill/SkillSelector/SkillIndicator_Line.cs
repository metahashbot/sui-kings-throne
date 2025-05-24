using RPGCore.Skill.Config.Selector;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace RPGCore.Skill.SkillSelector
{
    public class SkillIndicator_Line : SkillIndicatorBase
    {


        [SerializeField, Required, LabelText("线条本体Decal")]
        private DecalProjector _lineDecal;
        private Material _lineDecalMaterial;

        [SerializeField, Required, LabelText("箭头本体Decal")]
        private DecalProjector _arrowDecal;
        private Material _arrowDecalMaterial;


        [SerializeField, LabelText("y轴修正高度？")]
        private float _yOffset = 1f;


        private void Awake()
        {
            _lineDecalMaterial = _lineDecal.material;
            _arrowDecalMaterial = _arrowDecal.material;
        }





        public SkillIndicator_Line SetLineColor(Color lineColor)
        {
            _lineDecalMaterial.SetColor(_mp_MainColor, lineColor);
            return this;
        }
        public SkillIndicator_Line SetArrowColor(Color arrowColor)
        {
            _arrowDecalMaterial.SetColor(_mp_MainColor, arrowColor);
            return this;
        }
        public SkillIndicator_Line SetEmission(Color emissionColor, float emission)
        {
            _lineDecalMaterial.SetColor(_mp_EmissionColor, emissionColor);
            _lineDecalMaterial.SetFloat(_mp_EmissionPower, emission);
            _arrowDecalMaterial.SetColor(_mp_EmissionColor, emissionColor);
            _arrowDecalMaterial.SetFloat(_mp_EmissionPower, emission);
            return this;
        }



        public void UpdateTick(Vector3 fromPos, Vector3 endPos)
        {
            float distanceFromSelf = Vector3.Distance(endPos, fromPos);
            // if (distanceFromSelf < 1f)
            // {
            //     _lineDecal.size = new Vector3(1, 0, 2.5f);
            //     _arrowDecal.transform.position = inputPositionOnFloor;
            //     transform.LookAt(inputPositionOnFloor, transform.up);
            // }
            // else
            // {
            _lineDecal.size = new Vector3(1, (distanceFromSelf) * 2f, 16f);
            
            _arrowDecal.transform.position = endPos;

            // Vector3 targetDirection = endPos - fromPos;
            // targetDirection.y = 0; // ignore the y component
            // Quaternion targetRotation = Quaternion.FromToRotation(transform.forward, targetDirection);
            // transform.rotation = targetRotation;
            // transform.position = fromPos;
            Vector3 targetDirection = endPos - transform.position;
            targetDirection.y = 0; // ignore the y component
            Quaternion targetRotation = Quaternion.FromToRotation(Vector3.forward, targetDirection);
            transform.position = fromPos + Vector3.up * _yOffset;
            transform.rotation = targetRotation;
            // transform.rotation = targetRotation;
            // }

            


        }
        //
        // public override void FixedUpdateTick(Vector3 fromPos, Vector3 inputDirection,
        //     Vector3 inputPositionOnFloor,float disOffset)
        // {
        //     
        //
        //
        //     float distanceFromSelf = Vector3.Distance(inputPositionOnFloor, transform.position);
        //     // if (distanceFromSelf < 1f)
        //     // {
        //     //     _lineDecal.size = new Vector3(1, 0, 2.5f);
        //     //     _arrowDecal.transform.position = inputPositionOnFloor;
        //     //     transform.LookAt(inputPositionOnFloor, transform.up);
        //     // }
        //     // else
        //     // {
        //         if (distanceFromSelf > _maxDistance)
        //         {
        //             RaycastHit[] tmpHit = new RaycastHit[1];
        //             Vector3 tmpV3 = fromPos + inputDirection * disOffset + inputDirection * _maxDistance ;
        //             tmpV3.y += 100f;
        //             Physics.RaycastNonAlloc(tmpV3, Vector3.down,tmpHit, 9999f, 1 << 7);
        //             Vector3 finalPos = tmpHit[0].point;
        //             _lineDecal.size = new Vector3(1, (_maxDistance)*2f, 2.5f);
        //             _arrowDecal.transform.position = finalPos;
        //             transform.LookAt(finalPos, transform.up);
        //         }
        //         else
        //         {
        //         
        //             _lineDecal.size = new Vector3(1, (distanceFromSelf)*2f, 2.5f);
        //             _arrowDecal.transform.position = inputPositionOnFloor;
        //             transform.LookAt(inputPositionOnFloor, transform.up);
        //         }
        //     
        //     // }
        //     
        //     
        //     _arrowDecal.transform.localPosition += Vector3.up;
        //
        // }
    }
}