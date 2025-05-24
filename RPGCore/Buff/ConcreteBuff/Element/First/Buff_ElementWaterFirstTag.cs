using System;
using System.Collections.Generic;
using DG.Tweening;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.Element.First
{
	[TypeInfoBox("作为一级元素反应，主要作用是标签，需要监听 L_Buff_OnBuffInitialized_一个Buff被确定初始化了")]
	[Serializable]
	public class Buff_ElementWaterFirstTag : FirstElementTagBuff
	{

		protected override void _ABC_ProcessElementAffection(DS_ActionBusArguGroup ds)
		{
			RolePlay_BuffTypeEnum targetType = (RolePlay_BuffTypeEnum)ds.IntArgu1.Value;
			List<BaseBuffLogicPassingComponent> blpList = null;
			if (ds.ActionType == ActionBus_ActionTypeEnum.L_Buff_OnBuffInitialized_一个Buff被确定初始化了)
			{
				blpList = ds.GetObj2AsT<List<BaseBuffLogicPassingComponent>>();
			}
			else if (ds.ActionType == ActionBus_ActionTypeEnum.L_Buff_OnBuffRefreshed_已存在Buff被刷新)
			{
				blpList = ds.ObjectArguStr as List<BaseBuffLogicPassingComponent>;
			}

			switch (targetType)
			{
				case RolePlay_BuffTypeEnum.ElementFirstLightTag_Guang_一级光标签:
					Parent_SelfBelongToObject.ReceiveBuff_TryApplyBuff(
						RolePlay_BuffTypeEnum.ElementSecondDazzle_XuanGuang_二级炫光,
						Parent_SelfBelongToObject as I_RP_Buff_ObjectCanApplyBuff,
						Parent_SelfBelongToObject,
						blpList);
					break;
				case RolePlay_BuffTypeEnum.ElementFirstWindTag_Feng_一级风标签:
					Parent_SelfBelongToObject.ReceiveBuff_TryApplyBuff(
						RolePlay_BuffTypeEnum.ElementSecondFrostStorm_ShuangJuan_二级霜卷,
						Parent_SelfBelongToObject as I_RP_Buff_ObjectCanApplyBuff,
						Parent_SelfBelongToObject,
						blpList);

					break;
				case RolePlay_BuffTypeEnum.ElementSecondDazzle_XuanGuang_二级炫光:
					case RolePlay_BuffTypeEnum.ElementSecondFrostStorm_ShuangJuan_二级霜卷:
					ModifyStack(true, -1);
					break;
			}
		}

	}
}