using System;
using System.Collections.Generic;
using ARPG.Character.Enemy.AI.Listen;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using RPGCore.UtilityDataStructure.BuffLogicPassingComponent;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.Common
{
	/// <summary>
	/// <para>这是敌人身上，单一的弱点buff。各个【计量条】实际上都是对应这个buff本身。</para>
	/// <para>里面包  List (WeaknessInfoGroup) 。相当于UID的组</para>
	/// </summary>
	[Serializable]
	[TypeInfoBox("通用的弱点配置")]
	public class Buff_通用敌人弱点_CommonEnemyWeakness : BaseRPBuff
	{
		/// <summary>
		/// 
		/// </summary>
		public class WeaknessInfoGroup
		{
			public string GroupUID => _relatedUID;
			public float TriggerAmount => _triggerAmount;
			public float CurrentAmount => _currentAmount;
			public bool AsResidentWeaknessGroup => _asResidentWeaknessGroup;

			private bool _requireUIDisplay;
			public bool RequireUIDisplay => _requireUIDisplay;

			private bool _autoDisableAfterActive;
			public bool AutoDisableAfterActive => _autoDisableAfterActive;

			public bool CurrentGroupActive
			{
				get { return _currentActive; }
				set
				{
					if (_currentActive != value)
					{
						var ds_changed = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
							.L_Weakness_WeaknessGroupActiveStateChanged_弱点组的活跃状态被改变);
						ds_changed.ObjectArgu1 = this;
						ds_changed.IntArgu1 = value ? 1 : 0;
						RelatedBuff.Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus()
							.TriggerActionByType(ds_changed);
					}
					_currentActive = value;
				}
			}

			public WeaknessInfoGroup(
				string relatedUid,
				float triggerAmount,
				bool requireUI, bool autoDisable,
				List<BaseWeaknessAffectRule> relatedRulesRef,
				Buff_通用敌人弱点_CommonEnemyWeakness parent, bool resident)
			{
				RelatedBuff = parent;

				_relatedUID = relatedUid;
				_triggerAmount = triggerAmount;
				_relatedRulesRef = relatedRulesRef;
				_requireUIDisplay = requireUI;
				_autoDisableAfterActive = autoDisable;
				_asResidentWeaknessGroup = resident;
				
				var ds_weaknessAdd =
					new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_Weakness_NewWeaknessGroupAdd_新的弱点组被添加);
				ds_weaknessAdd.ObjectArgu1 = this;
				parent.Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().TriggerActionByType(ds_weaknessAdd);
			}


			public void AddListenerRule(I_WeaknessRuleAsListener rule)
			{
				
				if (_relatedRulesRef.Contains(rule as BaseWeaknessAffectRule))
				{
					DBug.LogError($"重复添加了规则{rule}到弱点组{GroupUID}");
					return;
				}
				_relatedRulesRef.Add(rule as BaseWeaknessAffectRule);
				rule.ProcessOnRegisterToRuntimeGroup(this);
			}

			public void AddResultRule(I_WeaknessComponentAsResult result)
			{
				if (_relatedRulesRef.Contains(result as BaseWeaknessAffectRule))
				{
					DBug.LogError($"重复添加了规则{result}到弱点组{GroupUID}");
					return;
				}
				_relatedRulesRef.Add(result as BaseWeaknessAffectRule);
			}

			public void ModifyCounter(float modify)
			{
				_currentAmount += modify;
				if (_currentAmount >= _triggerAmount)
				{
					//处理效果
					for (int i = _relatedRulesRef.Count - 1; i >= 0; i--)
					{
						var perRule = _relatedRulesRef[i];
						if (perRule is I_WeaknessComponentAsResult result)
						{
							result.TriggerWeaknessResult();
						}
					}
					
					if (_autoDisableAfterActive)
					{
						CurrentGroupActive = false;
					}

					_currentAmount = 0f;
				}
				if (_currentAmount < 0)
				{
					_currentAmount = 0f;
				}
			}


			public void FixedUpdateTick(float ct, int cf, float delta)
			{
				foreach (BaseWeaknessAffectRule perRule in _relatedRulesRef)
				{
					perRule.FixedUpdateTick(ct, cf, delta);
				}
			}


			public void ClearAndUnload()
			{
				
			}

			public Buff_通用敌人弱点_CommonEnemyWeakness RelatedBuff { get; private set; }



			private string _relatedUID;

			private List<BaseWeaknessAffectRule> _relatedRulesRef;

			private float _triggerAmount;
			private bool _asResidentWeaknessGroup = false;

			private float _currentAmount;


			private bool _currentActive = false;

		}

		[NonSerialized, TitleGroup("运行时"), LabelText("弱点生效规则们")]
		[ShowInInspector]
		protected Dictionary<string, WeaknessInfoGroup> _selfAllRulesGroupDict = new Dictionary<string, WeaknessInfoGroup>();






		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			if (blp is BLP_弱点规则修饰_ModifyWeaknessRule weaknessRule)
			{
				ProcessSingleRule(weaknessRule.RelatedRule);
				if (weaknessRule.RelatedRuleList != null && weaknessRule.RelatedRuleList.Count > 0)
				{
					for (int i = 0; i < weaknessRule.RelatedRuleList.Count; i++)
					{
						ProcessSingleRule(weaknessRule.RelatedRuleList[i]);
					}
				}
			}



			void ProcessSingleRule(BaseWeaknessAffectRule rule)
			{

				//如果这个BLP就是包含的初始化规则条目，那就进行初始化。也会检查是否已有这条规则了
				if (rule is WAR_初始化规则条目_InitRuleEntry init)
				{
					if (_selfAllRulesGroupDict.ContainsKey(init.RelatedWeaknessUID.Trim()))
					{
						//重复了
						DBug.LogError($"出现了重复的规则组，于角色{Parent_SelfBelongToObject}上的规则组{init.RelatedWeaknessUID}");
						return;
					}

					var newGroup = CreateNewGroup(init.RelatedWeaknessUID,
						init.CountAmount,
						init.InitActive,
						init.ActiveRequireShowUI,
						init.AutoDisableAfterTrigger,
						init.IsResidentCounter);

					return;
				}

				//找一下对应UID的规则组，没有则会创建。按照默认的100-不激活的方式创建
				WeaknessInfoGroup group = null;
				if (rule == null)
				{
					return;
				}

				if (!_selfAllRulesGroupDict.ContainsKey(rule.RelatedWeaknessUID.Trim()))
				{
					group = CreateNewGroup(rule.RelatedWeaknessUID,
						100f,
						false,
						false,
						true,
						false);
				}
				else
				{
					group = _selfAllRulesGroupDict[rule.RelatedWeaknessUID.Trim()];
				}

				//一次性的规则，执行效果就完事了
				if (rule is I_WeaknessRuleAsOnetime oneTime)
				{
					oneTime.ProcessOnetimeEffect(group);

				}
				//带监听的规则，需要类似深拷贝的注册业务
				if (rule is I_WeaknessRuleAsListener listen)
				{
					listen.RegisterToGroup(group);

				}

				//作为结果，需要深拷贝的注册业务
				if (rule is I_WeaknessComponentAsResult result)
				{
					result.RegisterToGroup(group);
				}
			}
		}

		

		private WeaknessInfoGroup CreateNewGroup(string uid, float amount, bool initActive, bool requireUI, bool autoDisable ,bool asResident)
		{
			var newList = new List<BaseWeaknessAffectRule>();
			WeaknessInfoGroup newGroup =
				new WeaknessInfoGroup(uid, amount, requireUI, autoDisable, newList, this, asResident);
			newGroup.CurrentGroupActive = initActive;
			_selfAllRulesGroupDict.Add(uid.Trim(), newGroup);

			return newGroup;
		}

		public WeaknessInfoGroup GetWeaknessGroupByID(string id)
		{
			if (_selfAllRulesGroupDict.ContainsKey(id))
			{
				return _selfAllRulesGroupDict[id];
			}
			return null;
		}

		public WeaknessInfoGroup GetMainWeaknessInfoGroup()
		{
			foreach (var perGroup in _selfAllRulesGroupDict.Values)
			{
				if (perGroup.AsResidentWeaknessGroup)
				{
					return perGroup;
				}
				
			}
			return null;
		}


		public override void FromHolder_FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.FromHolder_FixedUpdateTick(currentTime, currentFrameCount, delta);
			foreach (var perGroup in _selfAllRulesGroupDict.Values)
			{
				perGroup.FixedUpdateTick( currentTime,  currentFrameCount,  delta);
			}
		}

		protected override void ClearAndUnload()
		{
			foreach (WeaknessInfoGroup perGroup in _selfAllRulesGroupDict.Values)
			{
				perGroup.ClearAndUnload();
				
			}
			_selfAllRulesGroupDict.Clear();
			base.ClearAndUnload();
		}
	}

}