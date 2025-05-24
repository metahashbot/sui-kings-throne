using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace ARPG.Character.Base
{
	[Serializable]
	public class SheetOutlineTaskInfo
	{
		public static SheetOutlineTaskInfo DeepCopyFromPool(SheetOutlineTaskInfo copyFrom)
		{
			var newOne = GenericPool<SheetOutlineTaskInfo>.Get();
			newOne.TaskName = copyFrom.TaskName;
			newOne.Priority = copyFrom.Priority;
			newOne.Duration = copyFrom.Duration;
			newOne.Delay = copyFrom.Delay;
			newOne.FromColor = copyFrom.FromColor;
			newOne.ToColor = copyFrom.ToColor;
			newOne.FromWidth = copyFrom.FromWidth;
			newOne.ToWidth = copyFrom.ToWidth;
			newOne.Ease = copyFrom.Ease;
			newOne.Loop = copyFrom.Loop;
			newOne.LoopType = copyFrom.LoopType;
			return newOne;
		}
		
		private static readonly int mp_outlineColor = Shader.PropertyToID("_OutlineColor");
		private static readonly int mp_outlineWidth = Shader.PropertyToID("_OutlineWidth");
		private static readonly int mp_outlineEnable = Shader.PropertyToID("_EnableOutline");

		[NonSerialized]
		public string TaskName;
		
		[LabelText("任务优先级")]
		public int Priority;
		[NonSerialized]
		public float RemainingTime;
		[NonSerialized]
		private Tweener _tweener_color;
		[NonSerialized]
		private Tweener _tweener_width;



		[LabelText("单程时长")]
		public float Duration = 1f;
		[LabelText("启动延迟")]
		public float Delay;
		[ColorUsage(true,true)]
		[LabelText("单程颜色")]
		public Color FromColor = Color.white;
		[ColorUsage(true, true)]
		[LabelText("单程目标颜色")]
		public Color ToColor = Color.white;
		[LabelText("单程起始宽度")]
		public float FromWidth = 0.02f;
		[LabelText("单程目标宽度")]
		public float ToWidth =0.04f;
		[LabelText("渐变方式，示例可以看Z:\\动画缓动方式示例.mp4")]
		public Ease Ease = DG.Tweening.Ease.Linear;
		[LabelText("循环次数，默认无限")]
		public int Loop = -1;
		[LabelText("循环方式：默认yoyo")]
		public LoopType LoopType = LoopType.Yoyo;


		private MeshRenderer _selfMRRef;
		public void StartTween(ref MeshRenderer mr)
		{
			_tweener_color?.Kill();
			_tweener_color = null;
			_selfMRRef = mr;
			mr.material.SetFloat(mp_outlineEnable, 1f);
			if (_tweener_color == null)
			{
				mr.material.SetColor( mp_outlineColor,FromColor);
				_tweener_color = mr.material.DOColor(ToColor, mp_outlineColor, Duration).SetDelay(Delay).SetEase(Ease)
					.SetLoops(Loop, LoopType).SetUpdate(true);

			}
			_tweener_width?.Kill();
			_tweener_width = null;
			if (_tweener_width==null)
			{
				mr.material.SetFloat(mp_outlineWidth,FromWidth);
				_tweener_width = mr.material.DOFloat(ToWidth, mp_outlineWidth, Duration).SetDelay(Delay).SetEase(Ease)
					.SetLoops(Loop, LoopType).SetUpdate(true);
			}
		}
		
		public void KillTween()
		{
			_tweener_color?.Kill();
			_tweener_color = null;
			_tweener_width?.Kill();
			_tweener_width = null;
			_selfMRRef?.material.SetFloat(mp_outlineWidth, 0f);

		}
		
		

	}


	[Serializable]
	public class SheetOutlinePresetTask
	{
		public string TaskPresetID;
		public SheetOutlineTaskInfo TaskInfo;
	}
}