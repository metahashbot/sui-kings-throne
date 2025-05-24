using RPGCore.Skill.Config.Selector;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace RPGCore.Skill.SkillSelector
{
    public class SkillIndicator_Position : SkillIndicatorBase
    {
        public bool SelectObjectOrJustPosition { get; private set; }
        private float _maxRange;

        private float _rotateSpeed;
        private float _distortSpeed;
        private static readonly int OffsetX = Shader.PropertyToID("_OffsetX");
        private static readonly int OffsetY = Shader.PropertyToID("_OffsetY");
        private float _offsetX;
        private float _offsetY;

        private bool _selfContainDecoration = false;


        private DecalProjector _decorationDecal;
        private Material _decorationMaterial;
        private static readonly int MainColor = Shader.PropertyToID("_MainColor");
        private static readonly int FillColor = Shader.PropertyToID("_FillColor");
        private static readonly int SecondaryColor = Shader.PropertyToID("_SecondaryColor");
        private static readonly int DistortX = Shader.PropertyToID("_DistortX");
        private static readonly int DistortY = Shader.PropertyToID("_DistortY");
        private static readonly int _SP_Fill = Shader.PropertyToID("_Fill");

        // protected override void Awake()
        // {
        //     foreach (DecalProjector projector in GetComponentsInChildren<DecalProjector>())
        //     {
        //         if(projector.name.Contains("ase"))
        //         {
        //             _selfDecal = projector;
        //             _selfDecalMaterial = projector.material;
        //         }
        //
        //         if (projector.name.Contains("eco"))
        //         {
        //             _decorationDecal = projector;
        //             _decorationMaterial = _decorationDecal.material;
        //         }
        //     }
        // }
        //
        // public void SetSelectorBySelectorConfig(RPSkillIndicatorConfig_Position indicatorConfigPosition,
        //    
        //     bool selectObjectOrJustPosition, float innerRange,float outRange,float radius = 1f)
        // {
        //     SelectObjectOrJustPosition = selectObjectOrJustPosition;
        //     if ((int)indicatorConfigPosition.indicatorType > 4500)
        //     {
        //         _selfContainDecoration = true;
        //     
        //     }
        //
        //     _selfDecalMaterial.SetColor(MainColor, indicatorConfigPosition.MainColor);
        //     _selfDecalMaterial.SetColor(FillColor, indicatorConfigPosition.FillColor);
        //
        //
        //     if (_selfContainDecoration)
        //     {
        //         _decorationMaterial.SetColor(MainColor, indicatorConfigPosition.DecorationMainColor);
        //         _decorationMaterial.SetColor(SecondaryColor, indicatorConfigPosition.DecorationSecondaryColor);
        //     
        //
        //         float distortRandom = Random.Range(0.01f, 0.02f);
        //         _decorationMaterial.SetFloat(DistortX, distortRandom);
        //         distortRandom = Random.Range(0.01f, 0.02f);
        //         _decorationMaterial.SetFloat(DistortY,distortRandom);
        //     }
        //
        //     _maxRange = outRange;
        //     _rotateSpeed = Random.Range(3f, 6f);
        //     _distortSpeed = Random.Range(0f, 1f);
        //
        //     if (!Mathf.Approximately(radius, 1f))
        //     {
        //         _selfDecal.size = new Vector3(radius, radius, 2.5f);
        //         if (_selfContainDecoration)
        //         {
        //             _decorationDecal.size = new Vector3(radius, radius, 2.5f);
        //         }
        //     }
        //
        // }
        //
        // public void PositionIndicator_SetDistort(Vector2 range)
        // {
        //     if (_selfContainDecoration)
        //     {
        //         float rx = Random.Range(range.x, range.y);
        //         _decorationMaterial.SetFloat(DistortX, rx);
        //         float ry = Random.Range(range.x, range.y);
        //         _decorationMaterial.SetFloat(DistortY, ry);
        //     }
        //
        // }
        //
        // public void PositionIndicator_SetRotateSpeed(Vector2 range)
        // {
        //     _rotateSpeed = Random.Range(range.x, range.y);
        // }
        //
        // /// <summary>
        // /// <para>其位置将会被设置为 inputPositionOnRaycast的位置。；</para>
        // /// <para>disOffset为y轴上的额外距离补偿，本身就会有1，这是额外加的</para>
        // /// 
        // /// </summary>
        // public override void  FixedUpdateTick(Vector3 playerPosition, Vector3 inputDirection,
        //     Vector3 inputPositionOnFloor,float disOffset)
        // {
        //     if (_selfContainDecoration)
        //     {
        //         _offsetX += Time.deltaTime * _distortSpeed;
        //         _offsetX = _offsetX > 1f ? -1f : _offsetX;
        //         _offsetY += Time.deltaTime * _distortSpeed;
        //         _offsetY = _offsetY > 1f ? -1f : _offsetY;
        //         _decorationMaterial.SetFloat(OffsetX, _offsetX);
        //         _decorationMaterial.SetFloat(OffsetY, _offsetY);
        //     
        //     }
        //
        //     float distance = Vector3.Distance(inputPositionOnFloor, playerPosition);
        //     Vector3 targetPosition = inputPositionOnFloor;
        //     if (distance > _maxRange)
        //     {
        //         targetPosition = playerPosition + inputDirection * _maxRange;
        //     }
        //
        //     
        //     targetPosition.y += disOffset;
        //     targetPosition.y += 1f;
        //     
        //     transform.position = targetPosition;
        //
        //     transform.Rotate(0f,  _rotateSpeed * Time.deltaTime, 0f);
        // }
        //
        //
        // public void SetMainColor(Color c)
        // {
        //     _selfDecalMaterial.SetColor(MainColor, c);
        //     if (_selfContainDecoration)
        //     {
        //         _decorationMaterial.SetColor(MainColor, c);
        //     }
        // }
        //
        // public void SetSecondaryColor(Color c)
        // {
        //     _selfDecalMaterial.SetColor(FillColor, c);
        //     if (_selfContainDecoration)
        //     {
        //         _decorationMaterial.SetColor(SecondaryColor, c);
        //     }
        // }
        //
        // /// <summary>
        // /// <para>直接设置Fill的比例，0~1</para>
        // /// </summary>
        // public void SetFill(float fill)
        // {
        //     _selfDecalMaterial.SetFloat(_SP_Fill, fill);
        // }
    }
}