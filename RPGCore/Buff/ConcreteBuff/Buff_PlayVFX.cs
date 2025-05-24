using System;
using System.Collections.Generic;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff
{
	[InfoBox("需要监听一个OvertimeTick")]
	[Serializable]
	public class Buff_PlayVFX : BaseRPBuff
	{
		[TitleGroup("====配置====")]
		[SerializeField, LabelText("vfx_特效配置名"), GUIColor(187f / 255f, 1f, 0f)]
		protected string _vfxId;
		protected PerVFXInfo _vfxInfo;

		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
		}

		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			var ds = base.OnBuffInitialized(blps);
            _vfxInfo = _VFX_GetAndSetBeforePlay(_vfxId)
				._VFX_1_ApplyPresetTransform(Parent_SelfBelongToObject.GetRelatedVFXContainer())
				._VFX__10_PlayThis(true, true);
			return ds;
		}

		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
            _vfxInfo?.VFX_StopThis(true);
			var ds = base.OnBuffPreRemove();
			return ds;
		}
	}
}