using System;
using RPGCore.Skill.Config.Selector;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace RPGCore.Skill.SkillSelector
{
	public class SkillIndicator_Direction : SkillIndicatorBase
	{
		// public void FixedUpdateTick(Vector3 fromPosition, Vector3 endPosition)
		// {
		// 	fromPosition.y += _yOffset;
		// 	transform.position = fromPosition;
		//
		// 	transform.position = playerPos + inputDirection * disOffset;
		// 	if (disOffset > 0f)
		// 	{
		// 		float disSqr = disOffset * disOffset;
		// 		float inputDisSqr = Vector3.SqrMagnitude(inputPositionOnFloor - playerPos);
		// 		if (inputDisSqr < disSqr)
		// 		{
		// 			inputPositionOnFloor += inputDirection.normalized * disOffset;
		// 		}
		// 	}
		// 	transform.LookAt(inputPositionOnFloor);
		// }

		// /// <summary>
		// /// <para>设置Direction Selector的信息</para>
		// /// </summary>
		// public void SetSelectorBySelectorConfig(Vector3 position, RPSkillIndicatorConfig_Direction configDirection,
		//     float radius, float width)
		// {
		//     transform.position = position;
		//     _selfDecalMaterial.SetColor(MainColor, configDirection.MainColor);
		//     _selfDecalMaterial.SetColor(FillColor, configDirection.FillColor);
		//     SetRadius(radius);
		//     SetWidth(width);
		//     _selfDecal.gameObject.SetActive(true);
		// }
		//
		// private void SetRadius(float r)
		// {
		//     transform.localScale = new Vector3(3f, 3f, r);
		// }
		//
		// private void SetWidth(float w)
		// {
		//
		//     _selfDecal.size = new Vector3(w, _selfDecal.size.y, _selfDecal.size.z);
		// }
		//
		// /// <summary>
		// /// <para>playerPosition将会是transform.position</para>
		// /// </summary>
		// public override void FixedUpdateTick(Vector3 playerPos, Vector3 inputDirection,
		//     Vector3 inputPositionOnFloor, float disOffset)
		// {
		//     transform.position = playerPos + inputDirection * disOffset;
		//     if (disOffset > 0f)
		//     {
		//         float disSqr = disOffset * disOffset;
		//         float inputDisSqr = Vector3.SqrMagnitude(inputPositionOnFloor - playerPos);
		//         if (inputDisSqr < disSqr)
		//         {
		//             inputPositionOnFloor += inputDirection.normalized * disOffset;
		//         }
		//     }
		//
		//
		//     transform.LookAt(inputPositionOnFloor);
		// }
	}
}