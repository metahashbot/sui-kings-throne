using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ARPG.Manager;
using DG.Tweening;
using Global;
using Global.ActionBus;
using Global.Managers;
using Global.Setting;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;
using RPGCore.Buff.ConcreteBuff.Common;
using RPGCore.DataEntry;
using Random = UnityEngine.Random;
using ARPG.Character;

namespace RPGCore.AssistBusiness
{
	/// <summary>
	/// <para>伤害飘字的辅助类</para>
	/// </summary>
	public class DamageHintManager : MonoBehaviour
	{

		[SerializeField, LabelText("Prefab-跳字使用的TMP预制件"), Required, AssetsOnly]
		private GameObject _damageHintPrefab;


		[FoldoutGroup("常规配置组")]
		[SerializeField, LabelText("正常伤害 - Config_使用的配置文件"), Required, InlineEditor(InlineEditorObjectFieldModes.Boxed)]
		private SOConfig_DamageHintConfiguration _normalDamage_DamageHintConfig;

		[FoldoutGroup("常规配置组")]
		[SerializeField, LabelText("暴击伤害 - Config_使用的配置文件"), Required, InlineEditor(InlineEditorObjectFieldModes.Boxed)]
		private SOConfig_DamageHintConfiguration _CriticalDamage_DamageHintConfig;

		[FoldoutGroup("常规配置组")]
		[SerializeField, LabelText("治疗 —— Config_使用的配置文件"), Required, InlineEditor(InlineEditorObjectFieldModes.Boxed)]
		private SOConfig_DamageHintConfiguration _Heal_DamageHintConfig;

		[FoldoutGroup("常规配置组")]
		[SerializeField, LabelText("超伤 —— Config_使用的配置文件"), Required, InlineEditor(InlineEditorObjectFieldModes.Boxed)]
		private SOConfig_DamageHintConfiguration _SuperInjury_DamageHintConfig;

		[FoldoutGroup("常规配置组")]
		[SerializeField, LabelText("斩杀 —— Config_使用的配置文件"), Required, InlineEditor(InlineEditorObjectFieldModes.Boxed)]
		private SOConfig_DamageHintConfiguration _ChopKill_DamageHintConfig;

		[FoldoutGroup("常规配置组")]
		[SerializeField, LabelText("护盾 —— Config_使用的配置文件"), Required, InlineEditor(InlineEditorObjectFieldModes.Boxed)]
		private SOConfig_DamageHintConfiguration _Shields_DamageHintConfig;

		private static readonly Color _color_onlyAlpha0 = new Color(1f, 1f, 1f, 0f);

		private StringBuilder _selfSB = new StringBuilder();

		[FoldoutGroup("通用参数", true)]
		[SerializeField, LabelText("开启 屏幕空间 的纵深高度修正")]
		private bool _enableScreenSpaceZDepthFix = true;

		/// <summary>
		/// 每在Z轴大于本体1单位，跳字高度就提高这么多
		/// </summary>
		[SerializeField, LabelText("屏幕空间的纵深高度修正的乘数")]
		[ShowIf(nameof(_enableScreenSpaceZDepthFix))] [FoldoutGroup("通用参数", true)]
		private float _screenSpaceZDepthFixValue = 0.4f;


		protected Camera _mainCameraRef;

		protected float _globalZOffset = 0f;
		protected readonly float _globalZOffsetStep = 0.001f;

		public void AwakeInitialize()
		{
			_normalDamage_DamageHintConfig?.InitBuild();
			_CriticalDamage_DamageHintConfig?.InitBuild();
			_Heal_DamageHintConfig?.InitBuild();
			_SuperInjury_DamageHintConfig?.InitBuild();
			_ChopKill_DamageHintConfig?.InitBuild();
			_Shields_DamageHintConfig?.InitBuild();
			 _BackAttack_DamageHintConfig?.InitBuild();
			 
			
		}




		[SerializeField, LabelText("不进行玩家的伤害跳字")]
		[FoldoutGroup("通用参数", true)]
		private bool _notPopupPlayerHint = false;

		private class PerDamageHintEntryInfoPair
		{
			public Transform TargetTransform;
			public TextMeshPro TargetTMP;
			public float RemainingReturnTime;
			public float ElapsedTime;
			public SOConfig_DamageHintConfiguration _relatedConfigRef;
			public Vector3 StartPosition;
		}

		[ShowInInspector, LabelText("当前活跃(忙碌)的跳字对象列表")] [FoldoutGroup("运行时",
			false,
			VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()")]
		private List<PerDamageHintEntryInfoPair> _currentAllBusyEntryList = new List<PerDamageHintEntryInfoPair>();

		[ShowInInspector, LabelText("当前空闲的跳字对象列表")] [FoldoutGroup("运行时",
			false,
			VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()")]
		private List<PerDamageHintEntryInfoPair> _currentAllFreeEntryList = new List<PerDamageHintEntryInfoPair>();





		public void LateInitialize()
		{
			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_DamageAssistService_OnNewDamageEntryGenerated,
				_ABC_PopUpNewDamageHint_OnDamageHintServiceBuildDamageEntry);


			GlobalActionBus.GetGlobalActionBus().RegisterAction(ActionBus_ActionTypeEnum.G_GE_OnChangeViewYaw_当改变视角偏转角,
				_ABC_ChangeRotation_OnViewYawOrPitchChanged);
			GlobalActionBus.GetGlobalActionBus().RegisterAction(ActionBus_ActionTypeEnum.G_GE_OnChangeViewPitch_改变视角俯仰角,
				_ABC_ChangeRotation_OnViewYawOrPitchChanged);
			GlobalActionBus.GetGlobalActionBus()
				.RegisterAction(ActionBus_ActionTypeEnum.G_DamageHint_PopupSPEntry_SP条目跳字,
					_ABC_GenerateNewSPEntry_OnPopupSP);
			_mainCameraRef = GameReferenceService_ARPG.Instance.CameraBehaviourRef.MainCamera;
			//构建对象池
			for (int i = 0; i < 16; i++)
			{
				CreateNewEntryIntoFreePool();
			}
		}

		/// <summary>
		/// 生成一个新的到空闲列表中去
		/// </summary>
		private PerDamageHintEntryInfoPair CreateNewEntryIntoFreePool()
		{
			PerDamageHintEntryInfoPair newPair = new PerDamageHintEntryInfoPair();
			GameObject tmpNewObject = GameObject.Instantiate(_damageHintPrefab);
			tmpNewObject.transform.SetParent(transform);
			tmpNewObject.name = $"伤害跳字_{_currentAllFreeEntryList.Count + _currentAllBusyEntryList.Count}";
			TextMeshPro tmpNewTMP = tmpNewObject.GetComponent<TextMeshPro>();
			newPair.TargetTransform = tmpNewObject.transform;
			newPair.TargetTMP = tmpNewTMP;
			newPair.TargetTransform.rotation = currentRotation_ByViewForward;
			tmpNewObject.gameObject.SetActive(false);
			_currentAllFreeEntryList.Add(newPair);
			return newPair;
		}
		/// <summary>
		/// <para>获取一个空闲项。获取到的时候它已经被添加到busy列表中去了，不用再次添加。</para>
		/// <para>如果当前没有空闲项就会实例化一个新的</para>
		/// </summary>
		private PerDamageHintEntryInfoPair GetFreeEntry(SOConfig_DamageHintConfiguration config)
		{
			if (_currentAllFreeEntryList.Count < 1)
			{
				var entry = CreateNewEntryIntoFreePool();
				_currentAllFreeEntryList.RemoveAt(0);
				_currentAllBusyEntryList.Add(entry);
				entry.TargetTransform.gameObject.SetActive(true);
				entry.RemainingReturnTime = 9999f;
				entry._relatedConfigRef = config;
				return entry;
			}
			else
			{
				var entry = _currentAllFreeEntryList[_currentAllFreeEntryList.Count - 1];
				_currentAllFreeEntryList.RemoveAt(_currentAllFreeEntryList.Count - 1);
				entry.TargetTransform.gameObject.SetActive(true);
				entry.RemainingReturnTime = 9999f;
				entry._relatedConfigRef = config;
				_currentAllBusyEntryList.Add(entry);
				return entry;
			}
		}

		public void UpdateTick(float currentTime, int currentFrame, float delta)
		{
			int activeCount = 0;
			for (int i = _currentAllBusyEntryList.Count - 1; i >= 0; i--)
			{
				var entryInfoPair = _currentAllBusyEntryList[i];
				entryInfoPair.RemainingReturnTime -= delta;
				entryInfoPair.ElapsedTime += delta;

				float eva_size = entryInfoPair._relatedConfigRef.Scale1Curve.Evaluate(entryInfoPair.ElapsedTime /
				                                                                      entryInfoPair._relatedConfigRef
					                                                                      .DefaultSizeDuration);

				float targetSize = Mathf.Lerp(entryInfoPair._relatedConfigRef.DefaultInitializeSize, 1f, eva_size);

				entryInfoPair.TargetTransform.localScale = targetSize * Vector3.one;

				float eva_height = entryInfoPair._relatedConfigRef.PopProgressCurve.Evaluate(entryInfoPair.ElapsedTime /
				                                                                             entryInfoPair
					                                                                             ._relatedConfigRef
					                                                                             .DefaultPopDuration);

				float targetHeight = Mathf.Lerp(0f, entryInfoPair._relatedConfigRef.PopYDistance, eva_height);

				entryInfoPair.TargetTransform.localPosition = entryInfoPair.StartPosition + targetHeight * Vector3.up;


				if (entryInfoPair.RemainingReturnTime < 0f)
				{
					entryInfoPair.TargetTransform.gameObject.SetActive(false);
					_currentAllBusyEntryList.RemoveAt(i);
					_currentAllFreeEntryList.Add(entryInfoPair);
				}
				else
				{
					activeCount += 1;
				}
			}
			if (activeCount == 0)
			{
				_globalZOffset = 0f;
			}
		}

		/// <summary>
		/// <para>由DamageAssistService在伤害流程中发出的事件，构建了一个新的DamageApplyResult</para>
		/// </summary>
		private void _ABC_PopUpNewDamageHint_OnDamageHintServiceBuildDamageEntry(DS_ActionBusArguGroup ds)
		{
			RP_DS_DamageApplyResult rpds_dar = ds.ObjectArgu1 as RP_DS_DamageApplyResult;
			_selfSB.Clear();


			if (rpds_dar.ResultLogicalType == RP_DamageResultLogicalType.NormalResult ||
			    rpds_dar.ResultLogicalType == RP_DamageResultLogicalType.DodgedSoNothing ||
			    rpds_dar.ResultLogicalType == RP_DamageResultLogicalType.ActAsHeal)
			{
				if (rpds_dar.ResultLogicalType != RP_DamageResultLogicalType.DodgedSoNothing)
				{
					//小于1伤害的不跳字
					if (rpds_dar.PopupDamageNumber < 1f)
					{
						return;
					}
				}



				SOConfig_DamageHintConfiguration targetConfig = null;
				PickDefaultPopupConfigGroup(out targetConfig, rpds_dar);


				if (_notPopupPlayerHint && rpds_dar.Receiver is PlayerARPGConcreteCharacterBehaviour &&
				    rpds_dar.ResultLogicalType != RP_DamageResultLogicalType.ActAsHeal)
				{
					return;
				}

				PerDamageHintEntryInfoPair entry_default = GetFreeEntry(targetConfig);

				var fromPos = GetPositionOffset(targetConfig,
					rpds_dar.ResultLogicalType == RP_DamageResultLogicalType.ActAsHeal
						? rpds_dar.Receiver?.ReceiveDamage_GetCurrentReceiverPosition()
						: rpds_dar.DamageWorldPosition ?? Vector3.zero,
					rpds_dar.DamageWorldPosition?? Vector3.zero);

				entry_default.TargetTransform.position = fromPos;
				entry_default.StartPosition = fromPos;

				ProcessPopupColor(entry_default, targetConfig, rpds_dar);
				ProcessPopupFontSize(entry_default,
					targetConfig,
					rpds_dar,
					rpds_dar.Caster != null
						? rpds_dar.Caster.ApplyDamage_GetRelatedDataEntry(RP_DataEntry_EnumType.AttackPower_攻击力)
							.CurrentValue
						: rpds_dar.RelatedProjectileRuntimeRef.SelfLayoutConfigReference.LayoutHandlerFunction
							.CasterDataEntryCache[RP_DataEntry_EnumType.AttackPower_攻击力]);


				entry_default.RemainingReturnTime = targetConfig.AllDuration;
				entry_default.ElapsedTime = 0f;
				PopupDefaultConfigEntryByDamageAmount(entry_default, rpds_dar);
			}
		}




		private void PickDefaultPopupConfigGroup(
			out SOConfig_DamageHintConfiguration targetConfig,
			RP_DS_DamageApplyResult rpds_dar)
		{
			if (rpds_dar.CauseEliminateEffect)
			{
				targetConfig = _ChopKill_DamageHintConfig;
			}
			else if (rpds_dar.ResultLogicalType == RP_DamageResultLogicalType.ActAsHeal)
			{
				targetConfig = _Heal_DamageHintConfig;
			}
			else if (rpds_dar.CauseOverloadDamageEffect)
			{
				targetConfig = _SuperInjury_DamageHintConfig;
			}
			else if (rpds_dar.IsDamageCauseCritical)
			{
				targetConfig = _CriticalDamage_DamageHintConfig;
			}
			else
			{
				targetConfig = _normalDamage_DamageHintConfig;
			}
		}

		void PopupDefaultConfigEntryByDamageAmount(
			PerDamageHintEntryInfoPair entry_default,
			RP_DS_DamageApplyResult rpds_dar)
		{
			//项设置
			float damageTaken = rpds_dar.PopupDamageNumber;


			if (rpds_dar.ResultLogicalType == RP_DamageResultLogicalType.NormalResult)
			{
				//过伤
				if (rpds_dar.CauseEliminateEffect || rpds_dar.CauseOverloadDamageEffect)
				{
					_selfSB.Append("<sprite name=斩杀图标>");
					var charArray = ((int)damageTaken).ToString().ToCharArray();
					foreach (char perChar in charArray)
					{
						switch (perChar)
						{
							case '0':
								_selfSB.Append("<sprite name=猩0>");
								break;
							case '1':
								_selfSB.Append("<sprite name=猩1>");
								break;
							case '2':
								_selfSB.Append("<sprite name=猩2>");
								break;
							case '3':
								_selfSB.Append("<sprite name=猩3>");
								break;
							case '4':
								_selfSB.Append("<sprite name=猩4>");
								break;
							case '5':
								_selfSB.Append("<sprite name=猩5>");
								break;
							case '6':
								_selfSB.Append("<sprite name=猩6>");
								break;
							case '7':
								_selfSB.Append("<sprite name=猩7>");
								break;
							case '8':
								_selfSB.Append("<sprite name=猩8>");
								break;
							case '9':
								_selfSB.Append("<sprite name=猩9>");
								break;
						}
					}
					entry_default.TargetTMP.text = _selfSB.ToString();
				}
				else if (rpds_dar.DamageType == DamageTypeEnum.TrueDamage_真伤)
				{
					var charArray = ((int)damageTaken).ToString().ToCharArray();
					foreach (char perChar in charArray)
					{
						switch (perChar)
						{
							case '0':
								_selfSB.Append("<sprite name=普0>");
								break;
							case '1':
								_selfSB.Append("<sprite name=普1>");
								break;
							case '2':
								_selfSB.Append("<sprite name=普2>");
								break;
							case '3':
								_selfSB.Append("<sprite name=普3>");
								break;
							case '4':
								_selfSB.Append("<sprite name=普4>");
								break;
							case '5':
								_selfSB.Append("<sprite name=普5>");
								break;
							case '6':
								_selfSB.Append("<sprite name=普6>");
								break;
							case '7':
								_selfSB.Append("<sprite name=普7>");
								break;
							case '8':
								_selfSB.Append("<sprite name=普8>");
								break;
							case '9':
								_selfSB.Append("<sprite name=普9>");
								break;
						}
					}
					entry_default.TargetTMP.text = _selfSB.ToString();
				}

				else if (rpds_dar.IsDamageCauseCritical)
				{
					var charArray = ((int)damageTaken).ToString().ToCharArray();

					string dmtype = "猩";

					if (rpds_dar.DamageType == DamageTypeEnum.AoNengHuo_奥能火)
					{
						dmtype = "火";
						_selfSB.Append("<sprite name=暴击火>");
					}
					else if (rpds_dar.DamageType == DamageTypeEnum.AoNengShui_奥能水)
					{
						dmtype = "水";
						_selfSB.Append("<sprite name=暴击水>");
					}
					else if (rpds_dar.DamageType == DamageTypeEnum.AoNengTu_奥能土)
					{
						dmtype = "土";
						_selfSB.Append("<sprite name=暴击土>");
					}
					else if (rpds_dar.DamageType == DamageTypeEnum.AoNengFeng_奥能风)
					{
						dmtype = "风";
						_selfSB.Append("<sprite name=暴击风>");
					}
					else if (rpds_dar.DamageType == DamageTypeEnum.LingNengLing_灵能灵)
					{
						dmtype = "灵";
						_selfSB.Append("<sprite name=暴击灵能>");
					}
					else if (rpds_dar.DamageType == DamageTypeEnum.YuanNengDian_源能电)
					{
						dmtype = "电";
						_selfSB.Append("<sprite name=暴击电>");
					}
					else if (rpds_dar.DamageType == DamageTypeEnum.YuanNengGuang_源能光)
					{
						dmtype = "光";
						_selfSB.Append("<sprite name=暴击光>");
					}
					else if (rpds_dar.DamageType == DamageTypeEnum.YouNengXingHong_幽能猩红)
					{
						dmtype = "猩";
						_selfSB.Append("<sprite name=暴击猩红>");
					}


					foreach (char perChar in charArray)
					{
						switch (perChar)
						{
							case '0':
								_selfSB.Append($"<sprite name={dmtype}0>");
								break;
							case '1':
								_selfSB.Append($"<sprite name={dmtype}1>");
								break;
							case '2':
								_selfSB.Append($"<sprite name={dmtype}2>");
								break;
							case '3':
								_selfSB.Append($"<sprite name={dmtype}3>");
								break;
							case '4':
								_selfSB.Append($"<sprite name={dmtype}4>");
								break;
							case '5':
								_selfSB.Append($"<sprite name={dmtype}5>");
								break;
							case '6':
								_selfSB.Append($"<sprite name={dmtype}6>");
								break;
							case '7':
								_selfSB.Append($"<sprite name={dmtype}7>");
								break;
							case '8':
								_selfSB.Append($"<sprite name={dmtype}8>");
								break;
							case '9':
								_selfSB.Append($"<sprite name={dmtype}9>");
								break;
						}
					}
					entry_default.TargetTMP.text = _selfSB.ToString();
				}
				else
				{
					string dmtype = "猩";

					if (rpds_dar.DamageType == DamageTypeEnum.AoNengHuo_奥能火)
					{
						dmtype = "火";
					}
					else if (rpds_dar.DamageType == DamageTypeEnum.AoNengShui_奥能水)
					{
						dmtype = "水";
					}
					else if (rpds_dar.DamageType == DamageTypeEnum.AoNengTu_奥能土)
					{
						dmtype = "土";
					}
					else if (rpds_dar.DamageType == DamageTypeEnum.AoNengFeng_奥能风)
					{
						dmtype = "风";
					}
					else if (rpds_dar.DamageType == DamageTypeEnum.LingNengLing_灵能灵)
					{
						dmtype = "灵";
					}
					else if (rpds_dar.DamageType == DamageTypeEnum.YuanNengDian_源能电)
					{
						dmtype = "电";
					}
					else if (rpds_dar.DamageType == DamageTypeEnum.YuanNengGuang_源能光)
					{
						dmtype = "光";
					}
					else if (rpds_dar.DamageType == DamageTypeEnum.YouNengXingHong_幽能猩红)
					{
						dmtype = "猩";
					}



					string charArray = ((int)damageTaken).ToString();

					foreach (char perChar in charArray)
					{
						switch (perChar)
						{
							case '0':
								_selfSB.Append($"<sprite name={dmtype}0>");
								break;
							case '1':
								_selfSB.Append($"<sprite name={dmtype}1>");
								break;
							case '2':
								_selfSB.Append($"<sprite name={dmtype}2>");
								break;
							case '3':
								_selfSB.Append($"<sprite name={dmtype}3>");
								break;
							case '4':
								_selfSB.Append($"<sprite name={dmtype}4>");
								break;
							case '5':
								_selfSB.Append($"<sprite name={dmtype}5>");
								break;
							case '6':
								_selfSB.Append($"<sprite name={dmtype}6>");
								break;
							case '7':
								_selfSB.Append($"<sprite name={dmtype}7>");
								break;
							case '8':
								_selfSB.Append($"<sprite name={dmtype}8>");
								break;
							case '9':
								_selfSB.Append($"<sprite name={dmtype}9>");
								break;
						}
					}
					entry_default.TargetTMP.text = _selfSB.ToString();
				}
			}
			else if (rpds_dar.ResultLogicalType == RP_DamageResultLogicalType.ActAsHeal)
			{
				_selfSB.Append("<sprite name=医疗>");
				var charArray = ((int)damageTaken).ToString().ToCharArray();
				foreach (char perChar in charArray)
				{
					switch (perChar)
					{
						case '0':
							_selfSB.Append("<sprite name=疗0>");
							break;
						case '1':
							_selfSB.Append("<sprite name=疗1>");
							break;
						case '2':
							_selfSB.Append("<sprite name=疗2>");
							break;
						case '3':
							_selfSB.Append("<sprite name=疗3>");
							break;
						case '4':
							_selfSB.Append("<sprite name=疗4>");
							break;
						case '5':
							_selfSB.Append("<sprite name=疗5>");
							break;
						case '6':
							_selfSB.Append("<sprite name=疗6>");
							break;
						case '7':
							_selfSB.Append("<sprite name=疗7>");
							break;
						case '8':
							_selfSB.Append("<sprite name=疗8>");
							break;
						case '9':
							_selfSB.Append("<sprite name=疗9>");
							break;
					}
				}
				entry_default.TargetTMP.text = _selfSB.ToString();
			}
			else
			{
				_selfSB.Append("<sprite name=miss>");
				entry_default.TargetTMP.text = _selfSB.ToString();
			}
		}
		private static void ProcessPopupFontSize(
			PerDamageHintEntryInfoPair entry_default,
			SOConfig_DamageHintConfiguration targetConfig,
			RP_DS_DamageApplyResult rpds_dar,
			Nullable<float> originalDamageAmountOfCaster)
		{
			/*
			 * 尺寸
			 */
			entry_default.TargetTransform.localScale = targetConfig.DefaultInitializeSize * Vector3.one;

			/*
			 * 其他设置
			 */
			//字体
			entry_default.TargetTMP.fontSize = targetConfig.FontSize;


			if (originalDamageAmountOfCaster != null)
			{
				if (rpds_dar.PopupDamageNumber < originalDamageAmountOfCaster.Value)
				{
					entry_default.TargetTMP.fontSize *= 2;
				}
				else if (rpds_dar.PopupDamageNumber >= originalDamageAmountOfCaster.Value * 8)
				{
					entry_default.TargetTMP.fontSize *= 4;
				}
				else
				{
					float a = rpds_dar.PopupDamageNumber / originalDamageAmountOfCaster.Value;
					entry_default.TargetTMP.fontSize *= a / 2 + 2;
				}
			}
		}
		private static void ProcessPopupColor(
			PerDamageHintEntryInfoPair entry_default,
			SOConfig_DamageHintConfiguration targetConfig,
			RP_DS_DamageApplyResult rpds_dar)
		{
			SOConfig_DamageHintConfiguration.DamageColorAndTypeInfoPair colorInfo = null;
			if (rpds_dar != null)
			{
				colorInfo = targetConfig.GetColorAndTypeInfoByType(rpds_dar.DamageType);
			}
			else
			{
				colorInfo = targetConfig.GetColorAndTypeInfoByType(DamageTypeEnum.NoType_无属性);
			}
			var targetColor = colorInfo.FontVertexColor;
			targetColor.a = 1f;
			entry_default.TargetTMP.color = targetColor;
			entry_default.TargetTMP.colorGradient = new VertexGradient(colorInfo.color0,
				colorInfo.color1,
				colorInfo.color2,
				colorInfo.color3);
			
			entry_default.TargetTMP.DOFade(0f, targetConfig.OutDuration)
				.SetDelay(targetConfig.OutDelay);

		}
		private Vector3 GetPositionOffset(
			SOConfig_DamageHintConfiguration targetConfig,
			Nullable<Vector3> receiverPosition,
			Vector3 damageWorldPosition)
		{
			Vector3 fromPosition;
			var diff_onRight = targetConfig.GetCurrentStop_Right() *
			               BaseGameReferenceService.CurrentBattleLogicRightDirection;
			var diff_onForward = -BaseGameReferenceService.CurrentBattleLogicalForwardDirection *
			                 targetConfig.PopZInitialOffset;
			var diff_onWorldUp = (targetConfig.PopYInitialOffset + targetConfig.GetCurrentStop_Up()) * Vector3.up;

			//是否开启了屏幕空间修正
			if (_enableScreenSpaceZDepthFix)
			{
				if (receiverPosition.HasValue)
				{
					if (receiverPosition.Value.z < damageWorldPosition.z)
					{
						var vpOfBehaviour = _mainCameraRef.WorldToViewportPoint(receiverPosition.Value);
						var vpOfDamage = _mainCameraRef.WorldToViewportPoint(damageWorldPosition);
						var zDiff = Mathf.Abs(vpOfDamage.y - vpOfBehaviour.y);
						diff_onWorldUp += Vector3.up * (zDiff * _screenSpaceZDepthFixValue);
						diff_onForward.z += (receiverPosition.Value.z - damageWorldPosition.z);
					}
				}
			}
			if (receiverPosition.HasValue)
			{
				fromPosition = receiverPosition.Value + diff_onRight + diff_onForward + diff_onWorldUp;
			}
			else
			{
				fromPosition = damageWorldPosition + diff_onRight + diff_onForward + diff_onWorldUp;
			}
			_globalZOffset += _globalZOffsetStep;
			fromPosition += Vector3.back * _globalZOffset;
			return fromPosition;
		}


		private Quaternion currentRotation_ByViewForward;


		private void _ABC_ChangeRotation_OnViewYawOrPitchChanged(DS_ActionBusArguGroup ds)
		{
			SetCurrentRotation();
			foreach (PerDamageHintEntryInfoPair perEntry in _currentAllFreeEntryList)
			{
				perEntry.TargetTransform.rotation = currentRotation_ByViewForward;
			}

			foreach (PerDamageHintEntryInfoPair perEntry in _currentAllBusyEntryList)
			{
				perEntry.TargetTransform.rotation = currentRotation_ByViewForward;
			}
		}
		private void SetCurrentRotation()
		{
			var cameraEulerX = BaseMainCameraBehaviour.BaseInstance.MainCamera.transform.rotation.eulerAngles.x;

			switch (BaseGameReferenceService.ViewYawDirection)
			{
				case BaseGameReferenceService.ViewYawDirectionTypeEnum.ViewToForward_朝前:
					currentRotation_ByViewForward = Quaternion.Euler(cameraEulerX, 0f, 0f);
					break;
				case BaseGameReferenceService.ViewYawDirectionTypeEnum.ViewToLeft_朝左:
					currentRotation_ByViewForward = Quaternion.Euler(cameraEulerX, -90f, 0f);
					break;
				case BaseGameReferenceService.ViewYawDirectionTypeEnum.ViewToBack_朝后:
					currentRotation_ByViewForward = Quaternion.Euler(cameraEulerX, 180f, 0f);
					break;
				case BaseGameReferenceService.ViewYawDirectionTypeEnum.ViewToRight_朝右:
					currentRotation_ByViewForward = Quaternion.Euler(cameraEulerX, 90f, 0f);
					break;
			}
		}



#region 回蓝跳字

		[SerializeField, LabelText("回蓝 —— Config_使用的配置文件"), Required, InlineEditor(InlineEditorObjectFieldModes.Boxed)]
		private SOConfig_DamageHintConfiguration _SP_DamageHintConfig;



		private void _ABC_GenerateNewSPEntry_OnPopupSP(DS_ActionBusArguGroup ds)
		{
			// var relatedBehaviour = ds.GetObj1AsT<RolePlay_BaseBehaviour>();
			// PerDamageHintEntryInfoPair entry_default = GetFreeEntry(_SP_DamageHintConfig);
			// var targetConfig = _SP_DamageHintConfig;
			// var spRestoreDirectValue = ds.FloatArgu1.Value;
			// /*
			//  * 生成位置
			//  */
			//
			//
			// UnityEngine.Vector3 diff_onRight = targetConfig.GetCurrentStop_Right() *
			//                                    BaseGameReferenceService.CurrentBattleLogicRightDirection;
			// UnityEngine.Vector3 diff_onForward = -BaseGameReferenceService.CurrentBattleLogicalForwardDirection *
			//                                      targetConfig.PopZInitialOffset;
			// Vector3 diff_onWorldUp = (targetConfig.PopYInitialOffset + targetConfig.GetCurrentStop_Up()) * Vector3.up;
			//
			// UnityEngine.Vector3 fromPos = relatedBehaviour.transform.position + diff_onRight + diff_onForward +
			//                               diff_onWorldUp;
			// entry_default.TargetTransform.position = fromPos;
			// entry_default.StartPosition = fromPos;
			//
			//
			// /*
			//  * 颜色
			//  */
			//
			//
			// SOConfig_DamageHintConfiguration.DamageColorAndTypeInfoPair colorInfo = null;
			// colorInfo = targetConfig.GetColorAndTypeInfoByType(DamageTypeEnum.NoType_无属性);
			//
			//
			// entry_default.TargetTMP.color = colorInfo.FontVertexColor;
			// entry_default.TargetTMP.colorGradient = new VertexGradient(colorInfo.color0,
			// 	colorInfo.color1,
			// 	colorInfo.color2,
			// 	colorInfo.color3);
			//
			//
			// /*
			//  * 尺寸
			//  */
			// entry_default.TargetTransform.localScale = targetConfig.DefaultInitializeSize * Vector3.one;
			//
			// /*
			//  * 其他设置
			//  */
			// //字体
			// entry_default.TargetTMP.fontSize = targetConfig.FontSize;
			//
			//
			// //项设置
			// entry_default.RemainingReturnTime = targetConfig.AllDuration;
			// entry_default.ElapsedTime = 0f;
			// entry_default.TargetTMP.color = Color.white;
			// entry_default.TargetTMP.DOColor(_color_onlyAlpha0, targetConfig.OutDuration)
			// 	.SetDelay(targetConfig.OutDelay);
			//
			// float damageTaken = spRestoreDirectValue;
			//
			// _selfSB.Clear();
			//
			// entry_default.TargetTMP.text = _selfSB.ToString();
			//
			// string charArray = ((int)damageTaken).ToString();
			// string dmtype = "蓝";
			// _selfSB.Append("<sprite name=回蓝>");
			// foreach (char perChar in charArray)
			// {
			// 	switch (perChar)
			// 	{
			// 		case '0':
			// 			_selfSB.Append($"<sprite name={dmtype}0>");
			// 			break;
			// 		case '1':
			// 			_selfSB.Append($"<sprite name={dmtype}1>");
			// 			break;
			// 		case '2':
			// 			_selfSB.Append($"<sprite name={dmtype}2>");
			// 			break;
			// 		case '3':
			// 			_selfSB.Append($"<sprite name={dmtype}3>");
			// 			break;
			// 		case '4':
			// 			_selfSB.Append($"<sprite name={dmtype}4>");
			// 			break;
			// 		case '5':
			// 			_selfSB.Append($"<sprite name={dmtype}5>");
			// 			break;
			// 		case '6':
			// 			_selfSB.Append($"<sprite name={dmtype}6>");
			// 			break;
			// 		case '7':
			// 			_selfSB.Append($"<sprite name={dmtype}7>");
			// 			break;
			// 		case '8':
			// 			_selfSB.Append($"<sprite name={dmtype}8>");
			// 			break;
			// 		case '9':
			// 			_selfSB.Append($"<sprite name={dmtype}9>");
			// 			break;
			// 	}
			// }
			// entry_default.TargetTMP.text = _selfSB.ToString();
		}

#endregion

#region 背击跳字

		[SerializeField, LabelText("背击跳字配置")]
		private SOConfig_DamageHintVariant_BackHit _BackAttack_DamageHintConfig;

		public void PopupBackAttackHint( Vector3 popupFromPosition )
		{
			if (BaseGameReferenceService.CurrentFixedTime > _BackAttack_DamageHintConfig.NextAvailablePopupTime)
			{
				_BackAttack_DamageHintConfig.NextAvailablePopupTime = BaseGameReferenceService.CurrentFixedTime +
				                                                      _BackAttack_DamageHintConfig.PopupHintCooldown;
				PerDamageHintEntryInfoPair entry_default = GetFreeEntry(_BackAttack_DamageHintConfig);
				var targetConfig = _BackAttack_DamageHintConfig;
				entry_default.TargetTMP.text = "背面攻击";
				var fromPos = GetPositionOffset(targetConfig, popupFromPosition, popupFromPosition);
				entry_default.TargetTransform.position = fromPos;
				entry_default.StartPosition = fromPos;
				ProcessPopupColor(entry_default, targetConfig, null);

				entry_default.TargetTMP.fontSize = targetConfig.FontSize;
				entry_default.RemainingReturnTime = targetConfig.AllDuration;
				entry_default.ElapsedTime = 0f;
			}
			
			   
			 
		}

#endregion


#if UNITY_EDITOR
		// [Button]
		// private void 在摄像机注视的地形位置弹一个伤害()
		// {
		//     var newEntry = GetFreeEntry();
		//     var cameraB = GameReferenceService_SRPG.Instance.CameraBehaviourRef.CameraTargetTransform;
		//     SetInfoPairAndLaunch(newEntry, cameraB.transform.position, 100, Color.red, Color.red, false);
		// }
#endif
	}
}