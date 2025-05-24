using System;
using RPGCore.Interface;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character
{


	[Serializable]
	public class DS_OccupationInfo
	{
		[ShowInInspector, LabelText("占用信息组名字"), GUIColor(33f / 255f, 229f / 255f, 255f / 255f)]
		public string OccupationInfoConfigName;


		[ShowInInspector, LabelText("占用来自ID"),NonSerialized,ReadOnly, HorizontalGroup("Bool")]
		public I_RP_NeedReactToOccupy RelatedInterface;

		[ShowInInspector, LabelText("当前生效的占用等级"), NonSerialized, ReadOnly, HorizontalGroup("Bool")]
		public int CurrentActiveOccupationLevel;
		
		[ShowInInspector, LabelText("激活时占用等级[动画开始时]")]
		public int OccupationLevel;
		[ShowInInspector, LabelText("取消等级")]
		public int CancelLevel;

		[SerializeField, LabelText("动画结束后会调整占用级吗"), HorizontalGroup("Bool2")]
		public bool ModifyOccupationLevelAfterAnimation;

		[SerializeField,ShowIf(nameof(ModifyOccupationLevelAfterAnimation)), LabelText("动画结束后占用等级"),
		 HorizontalGroup("Bool2")]
		public int OccupationLevelAfterAnimation;


		public override string ToString()
		{
			return
				$"占用信息组{this.OccupationInfoConfigName},来自{this.RelatedInterface},占用等级{this.OccupationLevel},取消等级{this.CancelLevel}";
		}
	}

	;

}