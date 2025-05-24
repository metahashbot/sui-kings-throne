using System;
using System.Collections.Generic;
using Global.ActionBus;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using RPGCore.UtilityDataStructure.BuffLogicPassingComponent;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.EquipmentPerk
{
	[Serializable]
	public class Buff_EP_赏金_Bounty : BaseRPBuff,   I_BuffCanAsEquipmentPerk
	{



		[Serializable]
		public struct BuffData
		{
			[SerializeField, LabelText("Perk等级")]
			public int Level;
			[SerializeField, LabelText("作用间隔"), SuffixLabel("秒")]
			public float ApplyInterval;
			[SerializeField,LabelText("受到伤害提高百分比"),SuffixLabel("%")]
			public float DamageIncrease;

		}
		
			
		[SerializeField,LabelText("逐等级数据"),ListDrawerSettings(ShowFoldout = true, AddCopiesLastElement = true),
		 FoldoutGroup("配置", true)]
		public List<BuffData> BuffDataList = new List<BuffData>();


		[ShowInInspector,ReadOnly,FoldoutGroup("运行时"),LabelText("当前等级的间隔")]
		private float _currentLevelInterval;
		[ShowInInspector,ReadOnly,FoldoutGroup("运行时"),LabelText("当前等级的伤害提高")]
		private float _currentDamageIncrease;

		//下次检测的时间点
		[ShowInInspector,ReadOnly,FoldoutGroup("运行时"),LabelText("下次检测的时间点")]
		private float _nextCheckTime;
		
		
		
		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			return base.OnBuffInitialized(blps);
		}


		public override void FromHolder_FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.FromHolder_FixedUpdateTick(currentTime, currentFrameCount, delta);
			if (currentTime > _nextCheckTime)
			{
				_nextCheckTime = currentTime + _currentLevelInterval;
				_ProcessEffect();
			}
			
		}


		private void _ProcessEffect()
		{
			//扫场上所有enemy
			var enemy =
			_characterOnMapManagerRef.GetRandomEnemy();
			if (enemy == null)
			{
				return;
			}
			enemy.ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.EPU_赏金标记_BountyMark,
				Parent_SelfBelongToObject as I_RP_Buff_ObjectCanApplyBuff,
				Parent_SelfBelongToObject);
			
			
			var targetBountyMark =
				enemy.ApplyBuff_GetRelatedBuff(RolePlay_BuffTypeEnum.EPU_赏金标记_BountyMark) as Buff_EP_赏金标记_BountyMark;
			
			
			targetBountyMark.CurrentDamageBonusPartial =
				Mathf.Max(targetBountyMark.CurrentDamageBonusPartial, _currentDamageIncrease);
			
			
		}
		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_作为Perk的等级_ActAsPerkLevel perkLevel:
					//find by level
					BuffData buffData = BuffDataList.Find(x => x.Level == perkLevel.Level);
					_currentLevelInterval = buffData.ApplyInterval;
					_currentDamageIncrease = buffData.DamageIncrease;
					_nextCheckTime = Time.time + _currentLevelInterval;
					break;
			}
		}
		protected override void ClearAndUnload()
		{
			base.ClearAndUnload();
		}
		
	}
}