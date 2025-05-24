using System;
using System.Collections.Generic;
using Global;
using Global.ActionBus;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Projectile.Layout.LayoutComponent
{
	[TypeInfoBox("响应时，会在当前所有激活着的投射物上根据指定的尺寸和伤害信息，通常都是直接对HP运算的")]
	[Serializable]
	public class LC_追加投射物于响应_AppendProjectileOnAction : BaseProjectileLayoutComponent
	{

		[LabelText("匹配响应ID")]
		public string MatchingID = "追加";


		[SerializeField]
		private float _size;
		[SerializeField]
		private float _lifeTime;

		[SerializeField, LabelText("DAI")]
		public ConSer_DamageApplyInfo DAI;

		private int _indexInternal;

		public override void InitializeBeforeStart(
			SOConfig_ProjectileLayout config,
			float currentTime,
			int currentFrame)
		{
			base.InitializeBeforeStart(config, currentTime, currentFrame);
			GetSelfLocalActionBusRef()
				.RegisterAction(ActionBus_ActionTypeEnum.L_PLC_Spawn_InternalAppendProjectile_内部要求追加投射物, Spawn);
		}


		private void Spawn(DS_ActionBusArguGroup ds)
		{
			if (ds.ObjectArguStr == null ||
			    !(ds.ObjectArguStr as string).Equals(MatchingID, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			var dict = _parentProjectileLayoutConfig.LayoutHandlerFunction.GetAllSeriesDict();

			var obj = ds.ObjectArgu1 as ProjectileBehaviour_Runtime;

			var p = _parentProjectileLayoutConfig.LayoutHandlerFunction.GetLayoutRelatedAvailableProjectile(
				"ARPGProjectile_VoidCircle_空圆形");
			if (obj.RelatedSeriesIndex == -1)
			{
				return;
			}
			p.StartDirection = Vector3.right;
			p.StartPosition = obj.RelatedGORef.transform.position;
			p.StartLocalSize = _size;
			p.StartLifetime = _lifeTime;
			p.StartSpeed = 0f;
			p.SetSeriesAndShoots(-1, _indexInternal);

			BaseLayoutHandler.WaitToSpawnProjectileBehaviourInfo newWait =
				GenericPool<BaseLayoutHandler.WaitToSpawnProjectileBehaviourInfo>.Get();
			newWait.SeriesIndex = -1;
			newWait.ShootsIndex = _indexInternal;
			newWait.ProjectileBehaviour = p;
			newWait.WillSpawnTime = BaseGameReferenceService.CurrentFixedTime;
			_parentProjectileLayoutConfig.LayoutHandlerFunction.WaitToSpawnProjectileList.Add(newWait);


			DS_ActionBusArguGroup ds_projectileBaseSetDone = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
				.L_PLC_Spawn_OnProjectileRespondToSpawnBaseSetDone_一个在响应生成时的原始投射物基础设置完成);
			ds_projectileBaseSetDone.IntArgu1 = _indexInternal;
			ds_projectileBaseSetDone.FloatArgu1 = BaseGameReferenceService.CurrentFixedTime;
			ds_projectileBaseSetDone.ObjectArgu1 = p;
			ds_projectileBaseSetDone.ObjectArgu2 = _parentProjectileLayoutConfig.LayoutHandlerFunction
				.WaitToSpawnProjectileList;

			GetSelfLocalActionBusRef().TriggerActionByType(
				ActionBus_ActionTypeEnum.L_PLC_Spawn_OnProjectileRespondToSpawnBaseSetDone_一个在响应生成时的原始投射物基础设置完成,
				ds_projectileBaseSetDone);

			_indexInternal += 1;
		}

		public override void ClearAndUnload(SOConfig_ProjectileLayout parentLayoutRef)
		{
			GetSelfLocalActionBusRef()
				.RemoveAction(ActionBus_ActionTypeEnum.L_PLC_Spawn_InternalAppendProjectile_内部要求追加投射物, Spawn);
			base.ClearAndUnload(parentLayoutRef);
		}
	}
}