using System;
using ARPG.Character.Base;
using ARPG.Character.Enemy;
using ARPG.Common;
using ARPG.Equipment;
using ARPG.Manager;
using RPGCore.Projectile.Layout;
using RPGCore.Skill;
namespace ARPG.Character.Player
{
	[Serializable]
	public class PAEC_进行一次吸附_ProcessAbsorb : BasePlayerAnimationEventCallback
	{

		public override BasePlayerAnimationEventCallback ExecuteBySkill(
			BaseARPGCharacterBehaviour behaviour,
			BaseRPSkill skillRef,
			bool createNewWhenExist = false)
		{
			return base.ExecuteBySkill(behaviour, skillRef, createNewWhenExist);
		}
		public override BasePlayerAnimationEventCallback ExecuteByWeapon(
			BaseARPGCharacterBehaviour behaviour,
			BaseWeaponHandler weaponHandler,
			bool createNewWhenExist = false)
		{
			var d = base.ExecuteByWeapon(behaviour, weaponHandler, createNewWhenExist);
			;
			var info = weaponHandler.CurrentAutoAbsorbInfo;

			if (!info.CurrentAbsorbActive)
			{
				return d;
			}

			//扫当前所有敌人
			var allBehaviours = SubGameplayLogicManager_ARPG.Instance.CharacterOnMapManagerReference.GetAllEnemy()
				.ClipEnemyListOnDefaultType().ClipEnemyListOnCharacterType(info.ExcludeAbsorbTypes);
			if (info.IncludeDistanceLimit)
			{
				allBehaviours.ClipEnemyListByDistance(info.AbsorbDistance);
			}
			if (allBehaviours.Count == 0)
			{
				return d;
			}

			switch (info.AbsorbSortType)
			{
				case PlayerAutoAbsorbConfig.AbsorbSortTypeEnum.Distance_按距离排序:
					allBehaviours.SortEnemyListByDistanceToPlayer();
					break;
				case PlayerAutoAbsorbConfig.AbsorbSortTypeEnum.Angle_按角度排序:
					break;
				case PlayerAutoAbsorbConfig.AbsorbSortTypeEnum.Type_按类型排序:
					break;
			}

			EnemyARPGCharacterBehaviour target = allBehaviours[0];

			//进行吸附。
			//要转面，然后覆写输入信息

			var currentPlayerFaceLeft = behaviour.GetRelatedArtHelper().CurrentFaceLeft;


			/*
			 * targetPos应当是一个圆上计算得到的数据。
			 * 即获取 currentPlayerPos 到targetPos的方向，然后获取一个圆上的最近点
			 */
			var targetPos = target.GetCollisionCenter();
			targetPos.y = 0f;
			var currentPlayerPos = behaviour.transform.position;
			currentPlayerPos.y = 0f;
			
			var directionFromTargetToPlayer = (currentPlayerPos - targetPos).normalized;
			var radiusOfTarget = 0.1f;

			targetPos = targetPos + directionFromTargetToPlayer * radiusOfTarget;


			if (currentPlayerFaceLeft)
			{
				if (targetPos.x > currentPlayerPos.x)
				{
					behaviour.GetRelatedArtHelper().SetFaceLeft(false);
				}
				else
				{
					behaviour.GetRelatedArtHelper().SetFaceLeft(true);
				}
			}
			else
			{
				if (targetPos.x < currentPlayerPos.x)
				{
					behaviour.GetRelatedArtHelper().SetFaceLeft(true);
				}
				else
				{
					behaviour.GetRelatedArtHelper().SetFaceLeft(false);
				}
			}
			weaponHandler.RecordedAttackPosition = targetPos;
			weaponHandler.RecordedAttackDirection = (targetPos - currentPlayerPos).normalized;

			return d;
		}

		public override string GetElementNameInList()
		{
			return $"{GetBaseCustomName()} 进行一次吸附";
		}
	}
}