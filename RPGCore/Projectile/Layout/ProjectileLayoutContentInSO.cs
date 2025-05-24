using System;
using System.Collections.Generic;
using ARPG.Common.HitEffect;
using Global;
using Global.ActionBus;
using RPGCore.Interface;
using RPGCore.Projectile.Layout;
using RPGCore.Projectile.Layout.LayoutComponent;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
namespace RPGCore.Projectile.Layout
{



	/// <summary>
	/// <para>投射物布局 位于SO中的通用配置</para>
	/// </summary>
	[Serializable]
	public sealed class ProjectileLayoutContentInSO
	{

		[NonSerialized]
		public SOConfig_ProjectileLayout SelfConfigSO;
		[SerializeField, LabelText("版面UID"), FoldoutGroup("配置", true)]
		public string LayoutUID;


		[SerializeField, LabelText("关联特效的配置信息"), FoldoutGroup("配置", true)]
		public HitConfigInSource HitVFXConfig;

		[SerializeField, LabelText("包含[受击染色]效果"), FoldoutGroup("配置", true)]
		public bool ContainHitColorEffect = true;
		[SerializeField, LabelText("将要使用的[受击染色]效果配置名"), FoldoutGroup("配置", true)]
		[ShowIf(nameof(ContainHitColorEffect))]
		public string HitCharacterColorConfigNameID = "标准默认";


		[SerializeField, LabelText("尺寸——乘给投射物，艺术和碰撞都会生效"), FoldoutGroup("配置", true)]
		public float RelatedProjectileScale = 1f;


		[LabelText("关联的投射物类型，根据[显示名称]匹配，多个就随机"), ListDrawerSettings(DefaultExpandedState = true)]
		[ValueDropdown(nameof(VD_GetDisplayNameVD)), GUIColor(255f / 255f, 186f / 255f, 239f / 255f)]
		public List<string> RelatedProjectileTypeDisplayNameList = new List<string>
		{
			"空圆形",
		};


		private List<string> VD_GetDisplayNameVD()
		{
			return GlobalConfigurationAssetHolderHelper.GetGCAHH().FE_ProjectileLoad.GetDisplayNameList();
		}


		[LabelText("额外生成高度附加值"), FoldoutGroup("配置", true)]
		public float ExtraSpawnHeightAddon;

		[LabelText("整体初始延迟"), FoldoutGroup("配置/发射次数", true)]
		public float OverallStartDelay = 0f;

		[LabelText("发射系列次数"), FoldoutGroup("配置/发射次数", true)]
		public int SeriesCount = 1;

		[LabelText("系列的基本间隔"), FoldoutGroup("配置/发射次数", true)]
		public float SeriesInterval = 0.5f;

		[LabelText("每系列的间隔修正，负数减短，正数延长"), FoldoutGroup("配置/发射次数", true)]
		public float SeriesIntervalOffset = 0f;



		[LabelText("每系列发射次数"), FoldoutGroup("配置/发射次数", true)]
		public int ShootsCountPerSeries = 1;

		[LabelText("单系列内多发的 基准发射间隔"), FoldoutGroup("配置/发射次数", true)]
		public float BaseShootIntervalInsideSeries = 0.25f;


		[LabelText("单系列内多发的 发射间隔修正，负数减短，正数延长"), FoldoutGroup("配置/发射次数", true)]
		public float ShootsIntervalOffset = 0f;


		[SerializeField, FoldoutGroup("配置", true), LabelText("碰撞信息")]
		public ConSer_CollisionInfo CollisionInfo;


		[SerializeField, FoldoutGroup("配置", true), LabelText("伤害信息")]
		public ConSer_DamageApplyInfo DamageApplyInfo;


		[Header("↓========版式组件========↓")]
		[SerializeReference, LabelText("版式组件们")]
		public List<BaseProjectileLayoutComponent> LayoutComponentList;


		[Header("↑========版式组件========↑")]
		[Space]
		[Header("↓========投射物组件=======↓")]
		[SerializeReference, LabelText("投射物组件们")]
		public List<ProjectileBaseFunctionComponent> FunctionComponentsOverride;


	}

	public enum ColliderTypeInCollisionInfo
	{
		Circle_圆形 = 1, Box_等腰梯形 = 2,
	}

	/// <summary>
	/// <para>通用的手算碰撞信息</para>
	/// </summary>
	[Serializable]
	public class ConSer_CollisionInfo
	{
		/// <summary>
		/// <para>碰撞层关系。在代码中对于碰撞层的修改需要在Spawn(/NoAutoStart/)之后，因为默认操作是包含层的修改的</para>
		/// <para> 1位：对敌人生效，常规；</para>
		/// <para> 2位：对玩家生效， 常规</para>
		/// <para> 3位：对敌人子弹生效，</para>
		/// <para> 4位： 对玩家子弹生效</para>
		/// </summary>
		[LabelText("碰撞层关系,通常玩家的子弹是5,敌人子弹是10")]
		public int CollisionLayerMask;

		[LabelText("碰撞信息组容器"), SerializeField]
		public List<SingleCollisionInfo> CollisionInfoList;
	}


	[Serializable]
	public class SingleCollisionInfo
	{

		public ColliderTypeInCollisionInfo ColliderType = ColliderTypeInCollisionInfo.Circle_圆形;

		[LabelText("圆形|扇形：半径 ； 等腰梯形：水平方向长度")]
		public float Radius = 1f;

		[LabelText("等腰梯形：竖直方向倾斜比率；"), HideIf("@this.ColliderType != ColliderTypeInCollisionInfo.Box_等腰梯形")]
		public float InclineRatio = 0.25f;

		[LabelText("等腰梯形：竖直方向长度"), HideIf("@this.ColliderType == ColliderTypeInCollisionInfo.Circle_圆形")]
		public float Length = 1f;

		[LabelText("用于RPBehaviour：中心点XZ偏移；")]
		public float2 OffsetPos;

		
		

		
	}


	public static class ProjectileLayoutConfigExtent
	{
		// public static ProjectileLayoutContentInSO GetBaseProjectileLayoutConfig(this ProjectileLayoutContentInSO config,
		// 	I_RP_Projectile_ObjectCanReleaseProjectile caster)
		// {
		// 	return SubGameplayLogicManager_BattleGame.Instance.ProjectileLayoutManagerInstance.Pre_SpawnLayout_Base(config, caster);
		// }
		//
		// public static void SpawnThis(this ProjectileLayoutContentInSO config)
		// {
		// 	SubGameplayLogicManager_BattleGame.Instance.ProjectileLayoutManagerInstance.AddConfigInstanceToCollectionAndStart(config);
		// }

		// public static ProjectileLayoutContentInSO CreateNewRuntimeInstanceAndSpawnIt(this ProjectileLayoutContentInSO config,
		// 	I_RP_Projectile_ObjectCanReleaseProjectile caster)
		// {
		// 	var runtimeInstance =
		// 	config.GetBaseProjectileLayoutConfig(caster);
		// 	runtimeInstance.SpawnThis();
		//
		// 	return runtimeInstance;
		//
		// }
	}
}