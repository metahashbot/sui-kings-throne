using RPGCore.Skill.Config.Selector;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace RPGCore.Skill.SkillSelector
{
	public class SkillIndicator_Sector : SkillIndicatorBase
	{
		[SerializeField, LabelText("y轴修正高度？")]
		private float _yOffset = 0.25f;

		[SerializeField, LabelText("半径比例：实际贴图的半径是贴花半径的多少?")]
		private float _radiusPartial = 0.8f;


		[SerializeField, LabelText("使用的是带装饰层的材质？")]
		private bool _containDecoration = true;

		[SerializeField, LabelText("扇形部分基本贴花")]
		private DecalProjector _sf_MainDecal;

		private Material _mainDecalMaterial => _sf_MainDecal.material;


		[SerializeField, LabelText("包含左右边界？")]
		private bool _containBorder;

		[SerializeField, LabelText("左边界贴花"),ShowIf(nameof(_containBorder))]
		private DecalProjector _sf_leftBorderDecal;

		private Material _leftBorderMaterial => _sf_leftBorderDecal.material;

		[SerializeField, LabelText("右边界贴花"), ShowIf(nameof(_containBorder))]
		private DecalProjector _sf_rightBorderDecal;

		private Material _rightBorderMaterial => _sf_rightBorderDecal.material;

		public SkillIndicator_Sector SetMainColor(Color mainColor)
		{
			_mainDecalMaterial.SetColor(_mp_MainColor, mainColor);
			if (_containBorder)
			{
				_leftBorderMaterial.SetColor(_mp_MainColor, mainColor);
				_rightBorderMaterial.SetColor(_mp_MainColor, mainColor);
			}
			return this;
		}
		public SkillIndicator_Sector SetDecorationColor(Color decorationColor)
		{
			_mainDecalMaterial.SetColor(_mp_SecondaryColor, decorationColor);
			if (_containBorder)
			{
				_leftBorderMaterial.SetColor(_mp_SecondaryColor, decorationColor);
				_rightBorderMaterial.SetColor(_mp_SecondaryColor, decorationColor);
			}
			return this;
		}


		public SkillIndicator_Sector SetFromPosition(Vector3 pos)
		{
			transform.position = pos + Vector3.up * _yOffset;
			return this;
		}

		public SkillIndicator_Sector SetRadius(float radius)
		{
			Vector3 targetSize = new Vector3(radius * 2f / _radiusPartial, radius * 2f / _radiusPartial, 10f);
			_sf_MainDecal.size = targetSize;
			if (_containBorder)
			{
				_sf_leftBorderDecal.size = targetSize;
				_sf_rightBorderDecal.size = targetSize;
			}
			return this;
		}
		
		public SkillIndicator_Sector SetAngle(float angleInDegree)
		{
			_mainDecalMaterial.SetFloat(_mp_Expand, angleInDegree / 360f);
			if (_containBorder)
			{
				float targetZ = angleInDegree / 2f;
				_sf_leftBorderDecal.transform.rotation = Quaternion.Euler(90f, 0f, targetZ);
				_sf_rightBorderDecal.transform.rotation = Quaternion.Euler(90f, 0f, -targetZ);
			}
			return this;
		}
		
		public void UpdateTick(Vector3 fromPosition, Vector3 endPos)
		{
			Vector3 targetDirection = endPos - transform.position;
			targetDirection.y = 0;
			Quaternion targetRotation = Quaternion.FromToRotation(Vector3.forward, targetDirection);
			transform.position = fromPosition + Vector3.up * _yOffset;
			transform.rotation = targetRotation;
			
		}
		




		// protected override void Awake()
		// {
		//     base.Awake();
		//     _mainMaterial = _sf_MainDecal.material;
		//     if (_sf_ContainBorder)
		//     {
		//         _borderLeftMaterial = _sf_BorderLeftDecal.material;
		//         _borderRightMaterial = _sf_BorderRightDecal.material;
		//     }
		// }
		//
		// public override void FixedUpdateTick(Vector3 fromPos, Vector3 inputDirection,
		//     Vector3 inputPositionOnFloor,float disOffset)
		// {
		//     transform.position = fromPos + inputDirection * disOffset;
		//     transform.LookAt(inputPositionOnFloor);
		//
		//     if (_containDecoration)
		//     {
		//         _offsetX += Time.deltaTime * _distortSpeed;
		//         _offsetX = _offsetX > 1f ? -1f : _offsetX;
		//         _offsetY += Time.deltaTime * _distortSpeed;
		//         _offsetY = _offsetY > 1f ? -1f : _offsetY;
		//         _mainMaterial.SetFloat(OffsetX, _offsetX);
		//         _mainMaterial.SetFloat(OffsetY, _offsetY);
		//         if (_sf_ContainBorder)
		//         {
		//             _borderLeftMaterial.SetFloat(OffsetX, _offsetX);
		//             _borderLeftMaterial.SetFloat(OffsetY, _offsetY);
		//             _borderRightMaterial.SetFloat(OffsetX, _offsetX);
		//             _borderRightMaterial.SetFloat(OffsetY, _offsetY);
		//         }
		//     }
		//
		//     transform.position += Vector3.up;
		// }
		//
		//
		// public void SetIndicator(float angle, float innerRange)
		// {
		//     float targetRange = innerRange * 2.5f;
		//     _mainMaterial.SetFloat(Expand, angle / 360f);
		//     _sf_MainDecal.size = new Vector3(targetRange, targetRange, 5f);
		//     if (_sf_ContainBorder)
		//     {
		//         float targetZ = angle / 2f;
		//         _sf_BorderLeftDecal.transform.rotation = Quaternion.Euler(90f, 0f, targetZ);
		//         _sf_BorderRightDecal.transform.rotation = Quaternion.Euler(90f, 0f, -targetZ);
		//
		//         _sf_BorderLeftDecal.size = new Vector3(targetRange, targetRange, 5f);
		//         _sf_BorderRightDecal.size = new Vector3(targetRange, targetRange, 5f);
		//     }
		//
		//     if (_containDecoration)
		//     {
		//         _distortSpeed = Random.Range(0f, 1f);
		//
		//         float distortRandom = Random.Range(0.05f, 0.15f);
		//         _mainMaterial.SetFloat(DistortX, distortRandom);
		//         if (_sf_ContainBorder)
		//         {
		//             _borderLeftMaterial.SetFloat(DistortX, distortRandom);
		//             _borderRightMaterial.SetFloat(DistortX, distortRandom);
		//         }
		//
		//         distortRandom = Random.Range(0.05f, 0.15f);
		//         _mainMaterial.SetFloat(DistortY, distortRandom);
		//         if (_sf_ContainBorder)
		//         {
		//             _borderLeftMaterial.SetFloat(DistortY, distortRandom);
		//             _borderRightMaterial.SetFloat(DistortY, distortRandom);
		//         }
		//     }
		// }
		//
		// public void SetIndicatorBySelectorConfig(RPSkillIndicatorConfig_Sector indicatorConfigSector, float angle,
		//     float innerRange)
		// {
		//     float targetRange = innerRange * 2.5f;
		//     _mainMaterial.SetFloat(Expand, angle / 360f);
		//     _sf_MainDecal.size = new Vector3(targetRange, targetRange, 5f);
		//
		//
		//
		//     _mainMaterial.SetColor(_sp_MainColor, indicatorConfigSector.MainColor);
		//
		//
		//     if (_sf_ContainBorder)
		//     {
		//         float targetZ = angle / 2f;
		//         _sf_BorderLeftDecal.transform.rotation = Quaternion.Euler(90f, 0f, targetZ);
		//         _sf_BorderRightDecal.transform.rotation = Quaternion.Euler(90f, 0f, -targetZ);
		//
		//         _sf_BorderLeftDecal.size = new Vector3(targetRange, targetRange, 5f);
		//         _sf_BorderRightDecal.size = new Vector3(targetRange, targetRange, 5f);
		//
		//         if (_containDecoration)
		//         {
		//             _borderLeftMaterial.SetColor(_sp_MainColor, indicatorConfigSector.MainColor);
		//             _borderLeftMaterial.SetColor(_sp_SecondaryColor, indicatorConfigSector.SecondaryColor);
		//             _borderRightMaterial.SetColor(_sp_MainColor, indicatorConfigSector.MainColor);
		//             _borderRightMaterial.SetColor(_sp_SecondaryColor, indicatorConfigSector.SecondaryColor);
		//         }
		//         else
		//         {
		//             _borderLeftMaterial.SetColor(_sp_MainColor, indicatorConfigSector.MainColor);
		//             _borderLeftMaterial.SetColor(_sp_FillColor, indicatorConfigSector.SecondaryColor);
		//             _borderRightMaterial.SetColor(_sp_MainColor, indicatorConfigSector.MainColor);
		//             _borderRightMaterial.SetColor(_sp_FillColor, indicatorConfigSector.SecondaryColor);
		//         }
		//     
		//     }
		//
		//     if (_containDecoration)
		//     {
		//         _distortSpeed = Random.Range(0f, 1f);
		//     
		//         _mainMaterial.SetColor(_sp_SecondaryColor, indicatorConfigSector.SecondaryColor);
		//
		//         float distortRandom = Random.Range(0.05f, 0.15f);
		//         _mainMaterial.SetFloat(DistortX, distortRandom);
		//         if (_sf_ContainBorder)
		//         {
		//             _borderLeftMaterial.SetFloat(DistortX, distortRandom);
		//             _borderRightMaterial.SetFloat(DistortX, distortRandom);
		//         }
		//
		//         distortRandom = Random.Range(0.05f, 0.15f);
		//         _mainMaterial.SetFloat(DistortY, distortRandom);
		//         if (_sf_ContainBorder)
		//         {
		//             _borderLeftMaterial.SetFloat(DistortY, distortRandom);
		//             _borderRightMaterial.SetFloat(DistortY, distortRandom);
		//         }
		//     }
		//     else
		//     {
		//         _mainMaterial.SetColor(_sp_FillColor, indicatorConfigSector.SecondaryColor);
		//     }
		// }
	}
}