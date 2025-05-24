using System;
using System.Collections.Generic;
using ARPG.Character.Base;
using Global.ActionBus;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Listen
{
	[Serializable]
	
	public class AIListen_图组变动行为模式映射监听_SheetGroupChangeListen : BaseAIListenComponent
	{
		[Serializable]
		public class PerSheetGroupChangeContent
		{
			[LabelText("处于行为模式ID")]
			public string BehaviourPatternID;

			[LabelText("映射关系数值")]
			public int OffsetValue;
		}
		
		[LabelText("配置信息")]
		public List<PerSheetGroupChangeContent> SheetGroupChangeContentList = new List<PerSheetGroupChangeContent>();


		public override void InitializeAndProcessRegister(SOConfig_AIBrain brainRef)
		{
			base.InitializeAndProcessRegister(brainRef);
			brainRef.BrainHandlerFunction.SelfLocalActionBusRef.RegisterAction(
				ActionBus_ActionTypeEnum.L_AnimationHelper_OnSpriteSheetIndexChanged_当图片表索引改变,
				_ABC_ProcessSheetIndexChange_OnSheetIndexRefresh);
		}

		private void _ABC_ProcessSheetIndexChange_OnSheetIndexRefresh(DS_ActionBusArguGroup ds)
		{
			BaseCharacterSheetAnimationHelper.SpriteGroupIndexContent changeContent = ds.GetObj1AsT<BaseCharacterSheetAnimationHelper.SpriteGroupIndexContent>();
			var matchPatternIndex = SheetGroupChangeContentList.FindIndex((content =>
				content.BehaviourPatternID.Equals(RelatedAIBrainRuntimeInstance.BrainHandlerFunction
					.CurrentActiveBehaviourPattern.BehaviourPatternTypeID)));
			if (matchPatternIndex != -1)
			{
				PerSheetGroupChangeContent matchContent = SheetGroupChangeContentList[matchPatternIndex];
				changeContent.TargetSpriteIndex += matchContent.OffsetValue;
			}



		}


		public override void UnRegisterListenInActionBus()
		{
			RelatedAIBrainRuntimeInstance.BrainHandlerFunction.SelfLocalActionBusRef.RemoveAction(
				ActionBus_ActionTypeEnum.L_AnimationHelper_OnSpriteSheetIndexChanged_当图片表索引改变,
				_ABC_ProcessSheetIndexChange_OnSheetIndexRefresh);
			base.UnRegisterListenInActionBus();
		}


	}
}