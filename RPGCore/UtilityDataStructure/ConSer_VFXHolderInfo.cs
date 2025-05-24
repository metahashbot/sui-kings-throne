using System;
using RPGCore.Interface;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.UtilityDataStructure
{
	[Serializable]
	public class ConSer_VFXHolderInfo
	{
		public static readonly string _VFXAnchorName_OnlySelf = "仅自身";
		public static readonly string _VFXAnchorName_OnlyScale = "仅自身缩放";
		public static readonly string _VFXAnchorName_ScaleAndRotate = "缩放并旋转";
		public static readonly string _VFXAnchorName_SRF = "缩放旋转翻转";
		public static readonly string _VFXAnchorName_CollisionCenter = "【碰撞中心】";
		
		
		[SerializeField, LabelText("对应的跟随配置名"), GUIColor(206f / 255f, 177f / 255f, 227f / 255f)]
		public string FollowConfigName;
		
		[InfoBox("使用碰撞中心时，不能使用“挂点”，只能使用位置本身" , VisibleIf =  "UseCollisionCenter")]
		[SerializeField,LabelText("使用碰撞中心？"),ToggleButtons("使用碰撞中心","正常挂点") ]
		public bool UseCollisionCenter;
		
		

		/// <summary>
		/// <para>自己的Container接口引用</para>
		/// </summary>
		[NonSerialized, HideInInspector]
		protected I_RP_ContainVFXContainer _selfContainerRef;
		
		
		public void Init(I_RP_ContainVFXContainer selfContainerRef)
		{
			_selfContainerRef = selfContainerRef;
		}

		[SerializeField, LabelText("挂点 —— 关联的Transform")]
		[HideIf("UseCollisionCenter",false)]
		public Transform SelfAnchorTransform;
		
		
		
	}
}