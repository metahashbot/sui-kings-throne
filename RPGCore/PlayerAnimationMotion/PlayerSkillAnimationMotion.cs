using System;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.PlayerAnimationMotion
{
	[Serializable]
	public class PlayerSkillAnimationMotion : BasePlayerAnimationMotion
	
	{

#region 后段

		[FoldoutGroup("===后摇===")]
		[SerializeField, LabelText("包含“完全脱手”部分")]
		public bool _Post_IncludeFullyOffHandPart = false;


		[FoldoutGroup("===后摇===")]
		[SerializeField, LabelText("动画_后摇基本"), GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		public string _ancn_PostPartAnimationName;



#endregion

#region 多段脱手

		[FoldoutGroup("===多段脱手===" , VisibleIf = "@this._Post_IncludeFullyOffHandPart") ]
		[SerializeField,LabelText(" 多段脱手的 前摇"), GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		[ShowIf(nameof(_Post_IncludeFullyOffHandPart))]
		public string _ancn_MultiPart_Prepare_AnimationName;


		[FoldoutGroup("===多段脱手===", VisibleIf = "@this._Post_IncludeFullyOffHandPart")]
		[SerializeField, LabelText(" 多段脱手的 施法中"), GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		[ShowIf(nameof(_Post_IncludeFullyOffHandPart))]
		public string _ancn_MultiPart_Middle_AnimationName;
		
		[FoldoutGroup("===多段脱手===", VisibleIf = "@this._Post_IncludeFullyOffHandPart")]
		[SerializeField, LabelText(" 多段脱手的 后摇"), GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		[ShowIf(nameof(_Post_IncludeFullyOffHandPart))]
		public string _ancn_MultiPart_Post_AnimationName;


#endregion

		public override bool ContainsAnimationConfig(string configName)
		{
			if(base.ContainsAnimationConfig(configName))
			{
				return true;
			}
			if(string.Equals( configName, _ancn_PostPartAnimationName))
			{
				return true;
			}
			if(string.Equals( configName, _ancn_MultiPart_Prepare_AnimationName))
			{
				return true;
			}
			if(string.Equals( configName, _ancn_MultiPart_Middle_AnimationName))
			{
				return true;
			}
			if(string.Equals( configName, _ancn_MultiPart_Post_AnimationName))
			{
				return true;
			}
			return false;
			
			
		}


	}
}