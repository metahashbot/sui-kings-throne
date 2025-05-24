using System;
using System.Collections.Generic;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine.Pool;
namespace RPGCore.Buff.ConcreteBuff.Level
{
	[Serializable]
	public class Buff_AffectByTotem_Resist_受图腾影响免伤 : BaseRPBuff
	{


		[ShowInInspector]private List<Buff_Totem_图腾免伤_ResistTotem> _listOfResistTotemBuff;

		/// <summary>
		/// <para>当前每层抵抗比例，百分比</para>
		/// </summary>
		private float _currentResistPerStackPartial;





		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			_listOfResistTotemBuff =
				CollectionPool<List<Buff_Totem_图腾免伤_ResistTotem>, Buff_Totem_图腾免伤_ResistTotem>.Get();

			parent.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeRearMul_对接收方将要进行后乘最终算区计算,
				_ABC_ReduceDamage);
			_listOfResistTotemBuff.Clear();
		}


		private void _ABC_ReduceDamage(DS_ActionBusArguGroup ds)
		{
			var dar = ds.GetObj1AsT<RP_DS_DamageApplyResult>();
			dar.CP_DamageAmount_DPart.MultiplyPart -=
				_currentResistPerStackPartial / 100f * _listOfResistTotemBuff.Count;
			if (dar.CP_DamageAmount_DPart.MultiplyPart < 0.01f)
			{
				dar.CP_DamageAmount_DPart.MultiplyPart = 0.01f;
			}
		}
		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_来自图腾的免伤信息_ResistFromTotem fromTotem:
					_currentResistPerStackPartial = fromTotem.PerStackResistPercent;

					if (_listOfResistTotemBuff.Contains(fromTotem.ParentBuffRef))
					{
						return;
					}
					else
					{
						_listOfResistTotemBuff.Add(fromTotem.ParentBuffRef);
					}

					break;
			}
		}

		/// <summary>
		/// <para>试图移除这个效果</para>
		/// </summary>
		/// <param name="resistTotem"></param>
		public void TryRemoveFromTotem(Buff_Totem_图腾免伤_ResistTotem resistTotem)
		{
			if (_listOfResistTotemBuff.Contains(resistTotem))
			{
				_listOfResistTotemBuff.Remove(resistTotem);
			}
		}


		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeRearMul_对接收方将要进行后乘最终算区计算,
				_ABC_ReduceDamage);
			CollectionPool<List<Buff_Totem_图腾免伤_ResistTotem>, Buff_Totem_图腾免伤_ResistTotem>.Release(
				_listOfResistTotemBuff);
			return base.OnBuffPreRemove();
		}

		public class BLP_来自图腾的免伤信息_ResistFromTotem : BaseBuffLogicPassingComponent
		{
			public Buff_Totem_图腾免伤_ResistTotem ParentBuffRef;
			/// <summary>
			/// <para>每层抵抗百分比</para>
			/// </summary>
			public float PerStackResistPercent;

			public override void ReleaseOnReturnToPool()
			{
				GenericPool<BLP_来自图腾的免伤信息_ResistFromTotem>.Release(this);
			}
		}
	}
}