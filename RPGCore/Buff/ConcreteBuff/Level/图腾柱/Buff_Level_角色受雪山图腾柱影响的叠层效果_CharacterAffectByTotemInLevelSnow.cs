using System;
using Global.ActionBus;
using RPGCore.DataEntry;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.Level.图腾柱
{
	[Serializable]
	public class Buff_Level_角色受雪山图腾柱影响的叠层效果_CharacterAffectByTotemInLevelSnow : BaseRPBuff
	{

#region 加攻加防

		[SerializeField, LabelText("加攻加防加闪的计算位置"), TitleGroup("===数值===")]
		protected ModifyEntry_CalculatePosition _SelfCP = ModifyEntry_CalculatePosition.FrontMul;
		
		
		protected Float_RPDataEntry _entry_ATK;
		[ShowInInspector]
		protected Float_ModifyEntry_RPDataEntry _modify_ATK;

		protected Float_RPDataEntry _entry_DEF;
		protected Float_ModifyEntry_RPDataEntry _modify_DEF;






		/// <summary>
		/// <para>接收一层新的加攻加防效果</para>
		/// </summary>
		public void ReceiveNewATKAndDefenseStack( float atk, float defense )
		{
			if (_modify_ATK == null)
			{
				_entry_ATK =
					Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.AttackPower_攻击力);
				_modify_ATK = Float_ModifyEntry_RPDataEntry.GetNewFromPool(0f,
					RPDM_DataEntry_ModifyFrom.FromBuff_Buff,
					_SelfCP);
			}
			if (_modify_DEF == null)
			{
				_entry_DEF =
					Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.Defense_防御力);
				_modify_DEF = Float_ModifyEntry_RPDataEntry.GetNewFromPool(0f,
					RPDM_DataEntry_ModifyFrom.FromBuff_Buff,
					_SelfCP);
			}


			_modify_ATK.ModifyValue += atk;
			_entry_ATK.Recalculate();

			_modify_DEF.ModifyValue += defense;
			_entry_DEF.Recalculate();
			;
		}
		
		
		


#endregion

#region 加闪避

		protected Float_RPDataEntry _entry_Dodge;
		
		protected Float_ModifyEntry_RPDataEntry _modify_Dodge;

		public void ReceiveNewDodgeStack(float dodge)
		{
			if (_modify_Dodge == null)
			{
				_entry_Dodge =
					Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.DodgeRate_闪避率);
				_modify_Dodge = Float_ModifyEntry_RPDataEntry.GetNewFromPool(0f,
					RPDM_DataEntry_ModifyFrom.FromBuff_Buff,
					_SelfCP);
			}

			_modify_Dodge.ModifyValue += dodge;
			_entry_Dodge.Recalculate();
		}

#endregion

#region 回血

		protected Float_RPDataEntry _entry_MaxHP;
		
		protected FloatPresentValue_RPDataEntry _entry_CurrentHP;


		/// <summary>
		/// 是个小数！
		/// </summary>
		/// <param name="partial"></param>
		public void ReceiveHPRestore(float partial)
		{
			if (_entry_MaxHP == null)
			{
				_entry_MaxHP =
					Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.HPMax_最大HP);
				_entry_CurrentHP =
					Parent_SelfBelongToObject.ReceiveBuff_GetFloatPresentValue(RP_DataEntry_EnumType.CurrentHP_当前HP);
			}

			_entry_CurrentHP.AddDataEntryModifier(Float_ModifyEntry_RPDataEntry.GetNewFromPool(
				_entry_MaxHP.CurrentValue * partial,
				RPDM_DataEntry_ModifyFrom.FromBuff_Buff,
				ModifyEntry_CalculatePosition.FrontAdd));
		}

#endregion



		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			if (_modify_ATK != null)
			{
				_entry_ATK.RemoveEntryModifier(_modify_ATK);
				_modify_ATK.ReleaseToPool();

			}
			if (_modify_DEF != null)
			{
				_entry_DEF.RemoveEntryModifier(_modify_DEF);
				_modify_DEF.ReleaseToPool();
			}
			if (_modify_Dodge != null)
			{
				_entry_Dodge.RemoveEntryModifier(_modify_Dodge);
				_modify_Dodge.ReleaseToPool();
			}
			
			
			return base.OnBuffPreRemove();
		}




	}
}