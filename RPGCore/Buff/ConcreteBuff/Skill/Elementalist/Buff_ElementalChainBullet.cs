using System;
using System.Collections.Generic;
using ARPG.Character.Base;
using ARPG.Manager;
using Global;
using Global.ActionBus;
using Global.Utility;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.ConcreteBuff.Common;
using RPGCore.Buff.Config;
using RPGCore.Interface;
using RPGCore.Projectile;
using RPGCore.Projectile.Layout;
using RPGCore.Projectile.Layout.LayoutComponent;
using RPGCore.Skill.ConcreteSkill.Elementalist;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;
namespace RPGCore.Buff.ConcreteBuff.Skill.Elementalist
{
	[Serializable]
	public class Buff_ElementalChainBullet : BaseRPBuff , I_BuffCanPassToOther
	{

		[LabelText("发射间隔"), FoldoutGroup("运行时", true), TitleGroup("运行时/Buff", Alignment = TitleAlignments.Centered)]
		public float ShootInterval = 0.5f;
		[LabelText("环绕数量"), FoldoutGroup("运行时", true), TitleGroup("运行时/Buff", Alignment = TitleAlignments.Centered)]
		public int SurroundCount = 3;
		[LabelText("环绕半径"), FoldoutGroup("运行时", true), TitleGroup("运行时/环绕", Alignment = TitleAlignments.Centered)]
		public float BulletSurroundRadius;
		[LabelText("子弹旋转高度"), FoldoutGroup("运行时", true), TitleGroup("运行时/环绕", Alignment = TitleAlignments.Centered)]
		public float BulletRotatingHeightOffset = 1f;
		[NonSerialized, LabelText("环绕角速度"), FoldoutGroup("运行时", true),
		 TitleGroup("运行时/环绕", Alignment = TitleAlignments.Centered)]
		public float BulletSurroundAngleSpeed ;
		[LabelText("子弹飞行速度"), FoldoutGroup("运行时", true), TitleGroup("运行时/版面", Alignment = TitleAlignments.Centered)]
		public float BulletFlySpeed = 5f;
		[LabelText("子弹诱导最大角速度"), FoldoutGroup("运行时", true), TitleGroup("运行时/版面", Alignment = TitleAlignments.Centered)]
		public float BulletTrackingAngleSpeed = 120f;
		[LabelText("子弹最大生命周期"), FoldoutGroup("运行时", true), TitleGroup("运行时/版面", Alignment = TitleAlignments.Centered)]
		public float BulletLifetime = 5f;
		
		
		[Serializable]
		public class BLP_奥术飞弹初始化参数_ChainBulletInitBLP : BaseBuffLogicPassingComponent
		{
			public float ShootInterval;
			public int SurroundCount;
			public float BulletSurroundRadius;
			public float BulletFlySpeed;
			public float BulletTrackingAngleSpeed;
			public float BulletLifetime;
			public float BulletSurroundAngleSpeed;
			
			public override void ReleaseOnReturnToPool()
			{
				GenericPool<BLP_奥术飞弹初始化参数_ChainBulletInitBLP>.Release(this);
			}
		}
		[SerializeField, LabelText("基准投射物版面")]
		public SOConfig_ProjectileLayout BaseProjectileLayoutConfig;
		[SerializeField,LabelText("火属性投射物") ]
		public string _ProjectileTypeID_Fire = "元素使火飞弹";
		[SerializeField,LabelText("水属性投射物") ]
		public string _ProjectileTypeID_Water = "元素使水飞弹";
		[SerializeField,LabelText("风属性投射物") ]
		public string _ProjectileTypeID_Wind = "元素使风飞弹";
		[SerializeField,LabelText("土属性投射物") ]
		public string _ProjectileTypeID_Earth = "元素使土飞弹";
		

		
		[SerializeField,LabelText("环绕特效ID"), GUIColor(187f / 255f, 1f, 0f)]
		public string _VFXID_Surround = "元素使飞弹环绕";
	
		/// <summary>
		/// 挂点的Trasform引用
		/// </summary>
		private Transform _followerTransformRef;

		private List<RotatingInfo> _surroundVFXInfoList;


		private class RotatingInfo
		{
			public int Index;
			public ParticleSystem PerPS;
			public float CurrentAngle;
		}
		

		/// <summary>
		/// <para>下次生成的时间点</para>
		/// </summary>
		private float _nextSpawnTime;

		private static CharacterOnMapManager _comRef;


		/// <summary>
		/// 当前活跃的伤害类型，只会在重新释放技能时刷新
		/// </summary>
		private DamageTypeEnum _currentActiveDamageType;
/// <summary>
/// 当前活跃的特效配置，这个只会在重新释放技能的时候刷新
/// </summary>
		private PerVFXInfo _currentActiveVFXInfoRef; 

		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			_surroundVFXInfoList = new List<RotatingInfo>();
			_comRef = SubGameplayLogicManager_ARPG.Instance.CharacterOnMapManagerReference;
		}

		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_奥术飞弹初始化参数_ChainBulletInitBLP chain:
					ShootInterval = chain.ShootInterval;
					SurroundCount = chain.SurroundCount;
					BulletSurroundRadius = chain.BulletSurroundRadius;
					BulletFlySpeed = chain.BulletFlySpeed;
					BulletTrackingAngleSpeed = chain.BulletTrackingAngleSpeed;
					BulletLifetime = chain.BulletLifetime;
					BulletSurroundAngleSpeed = chain.BulletSurroundAngleSpeed;
					 BulletRotatingHeightOffset = chain.BulletSurroundRadius;
					break;
			}
		}
		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			_currentActiveDamageType =
				(Parent_SelfBelongToObject.ReceiveBuff_GetTargetBuff(RolePlay_BuffTypeEnum
					.ChangeCommonDamageType_常规伤害类型更改) as Buff_ChangeCommonDamageType).CurrentDamageType;
			_currentActiveVFXInfoRef = PerVFXInfo._VFX_GetAndSetBeforePlay(AllVFXInfoList,
				_VFXID_Surround,
				(Parent_SelfBelongToObject as BaseARPGCharacterBehaviour).GetRelatedVFXContainer(),
				true,
				GetCurrentDamageType);
			CheckAndSpawnSurroundBullet();
			return base.OnBuffInitialized(blps);
		}

		public override DS_ActionBusArguGroup OnExistBuffRefreshed(I_RP_Buff_ObjectCanApplyBuff caster, List<BaseBuffLogicPassingComponent> blps)
		{
			CheckAndSpawnSurroundBullet();
			return base.OnExistBuffRefreshed(caster, blps);
		}

		/// <summary>
		/// <para>检查一下看是否需要(重新)生成环绕的子弹PS</para>
		/// </summary>
		public void CheckAndSpawnSurroundBullet()
		{
			// 当前可用粒子数量  不匹配 将要生成的环绕物数量，那就全删了然后重新生成
			 ClearVFX();
			 _nextSpawnTime = BaseGameReferenceService.CurrentFixedTime;
			 for (int i = 0; i < SurroundCount; i++)
			 {
				 RotatingInfo newInfo = GenericPool<RotatingInfo>.Get();
				 newInfo.Index = i;
				 var tmpNewPS =
					 VFXPoolManager.Instance.GetParticleSystemRuntimeByPrefab(_currentActiveVFXInfoRef.Prefab);
				 newInfo.PerPS = tmpNewPS;
				 _followerTransformRef = Parent_SelfBelongToObject.ReceiveBuff_GetRelatedArtHelper()._scaleAnchor;
				 tmpNewPS.transform.SetParent(_followerTransformRef);
				 tmpNewPS.transform.localScale = Vector3.one;
				 _surroundVFXInfoList.Add(newInfo);
				 tmpNewPS.gameObject.SetActive(true);
				 tmpNewPS.Play();
			 }
			 //设置他们的初始旋转
			 Vector3 fromRot = Vector3.right;
			 Vector3 fromPos = _followerTransformRef.position;
			 //只是改他们的xz位置，其他信息保留
			 for (int i = 0; i < _surroundVFXInfoList.Count; i++)
			 {
			 	float angle = 360f / _surroundVFXInfoList.Count * i;
			 	_surroundVFXInfoList[i].CurrentAngle = angle;
			 	Vector3 rotateOffset = Vector3.right * BulletSurroundRadius;
			 	rotateOffset = MathExtend.Vector3RotateOnXOZ(rotateOffset, angle);
			 	Vector3 offsetHeight = Vector3.up * BulletRotatingHeightOffset;
			 	_surroundVFXInfoList[i].PerPS.transform.position = fromPos + rotateOffset + offsetHeight;
			
			 }
			
			
			
			
			

		}


		public override void FromHolder_FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.FromHolder_FixedUpdateTick(currentTime, currentFrameCount, delta);
			if (currentTime > _nextSpawnTime)
			{
				_nextSpawnTime += ShootInterval;
				
				Vector3 playerPosy0 = Parent_SelfBelongToObject.ReceiveBuff_GetRelatedArtHelper().transform.position;
				playerPosy0.y = 0f;

				//从当前活跃的环绕VFX上各生成一轮
				foreach (var perPSInfo in _surroundVFXInfoList)
				{
					var fromPosition = perPSInfo.PerPS.transform.position;
					SOConfig_ProjectileLayout currentLayoutRuntime =
						BaseProjectileLayoutConfig.SpawnLayout_NoAutoStart(
							Parent_SelfBelongToObject as I_RP_Projectile_ObjectCanReleaseProjectile);
					currentLayoutRuntime.LayoutContentInSO.RelatedProjectileTypeDisplayNameList.Clear();
					switch (_currentActiveDamageType)
					{
						case DamageTypeEnum.AoNengTu_奥能土:
							currentLayoutRuntime.LayoutContentInSO.RelatedProjectileTypeDisplayNameList.Add(
								_ProjectileTypeID_Earth);
							currentLayoutRuntime.LayoutContentInSO.DamageApplyInfo.DamageType =
								DamageTypeEnum.AoNengTu_奥能土;
							break;
						case DamageTypeEnum.AoNengShui_奥能水:
							currentLayoutRuntime.LayoutContentInSO.RelatedProjectileTypeDisplayNameList.Add(
								_ProjectileTypeID_Water);
							currentLayoutRuntime.LayoutContentInSO.DamageApplyInfo.DamageType =
								DamageTypeEnum.AoNengShui_奥能水;
							break;
						case DamageTypeEnum.AoNengHuo_奥能火:
							currentLayoutRuntime.LayoutContentInSO.RelatedProjectileTypeDisplayNameList.Add(
								_ProjectileTypeID_Fire);
							currentLayoutRuntime.LayoutContentInSO.DamageApplyInfo.DamageType = DamageTypeEnum.AoNengHuo_奥能火;
							break;
						case DamageTypeEnum.AoNengFeng_奥能风:
							currentLayoutRuntime.LayoutContentInSO.RelatedProjectileTypeDisplayNameList.Add(
								_ProjectileTypeID_Wind);
							currentLayoutRuntime.LayoutContentInSO.DamageApplyInfo.DamageType = 
								DamageTypeEnum.AoNengFeng_奥能风;
							break;
					}
					currentLayoutRuntime.LayoutHandlerFunction.OverrideSpawnFromPosition = fromPosition;
				
					LC_JustSpawn justSpawnRef =
						currentLayoutRuntime.LayoutContentInSO.LayoutComponentList.Find((component =>
							component is LC_JustSpawn)) as LC_JustSpawn;
					PFC_诱导_TrackingObject trackingRef = currentLayoutRuntime.LayoutContentInSO.FunctionComponentsOverride.Find(
						(component => component is PFC_诱导_TrackingObject)) as PFC_诱导_TrackingObject;
					trackingRef.MaxTrackingAngleSpeed = BulletTrackingAngleSpeed;

					var getRandomTarget = _comRef.GetRandomEnemy();
					Vector3 fromPosY0 = fromPosition;
					fromPosY0.y = 0f;
					//如果当前没有敌人，那么诱导设置的目标是空，覆写方向是角色到自身的连线方向
					if (getRandomTarget == null)
					{
						trackingRef.SetTrackingTarget(null);
						
					}
					else
					{
						trackingRef.SetTrackingTarget(getRandomTarget.gameObject);
					}
					currentLayoutRuntime.LayoutHandlerFunction.OverrideSpawnFromDirection =
						Vector3.Cross((playerPosy0 - fromPosY0).normalized, Vector3.down);
					justSpawnRef.SetLifetime = BulletLifetime;
					justSpawnRef.TotalSpawnCount = 1;
					justSpawnRef.SetFlySpeed = BulletFlySpeed;
					currentLayoutRuntime.LayoutHandlerFunction.StartLayout();




				}
				
				
			}

			if (_surroundVFXInfoList.Count > 0)
			{

				var rotateFromPos = Parent_SelfBelongToObject.ReceiveBuff_GetRelatedArtHelper().transform.position;
				float height = _surroundVFXInfoList[0].PerPS.transform.position.y;
				rotateFromPos.y = height;



				var fromPos = _followerTransformRef.position;
				foreach (var perPSInfo in _surroundVFXInfoList)
				{
					// rotate on XOZ 
					perPSInfo.CurrentAngle += BulletSurroundAngleSpeed * delta;
					Vector3 rotateOffset = Vector3.right * BulletSurroundRadius;
					rotateOffset = MathExtend.Vector3RotateOnXOZ(rotateOffset, perPSInfo.CurrentAngle);
					Vector3 offsetHeight = Vector3.up * BulletRotatingHeightOffset;
					perPSInfo.PerPS.transform.position = fromPos + rotateOffset + offsetHeight;
				}
			}
			
			
			
			
			
			
		}



		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			ClearVFX();
			return base.OnBuffPreRemove();
		}



		private void ClearVFX()
		{
			foreach (var perPSInfo in _surroundVFXInfoList)
			{
				VFXPoolManager.Instance.ReturnParticleSystemToPool(_currentActiveVFXInfoRef.Prefab, perPSInfo.PerPS);
				GenericPool<RotatingInfo>.Release(perPSInfo);
			}
			_surroundVFXInfoList.Clear();
		}
		
		
		
	}





}