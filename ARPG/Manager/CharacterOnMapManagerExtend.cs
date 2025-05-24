using System.Collections.Generic;
using ARPG.Character.Enemy;
using RPGCore.Buff;
using UnityEngine;
namespace ARPG.Manager
{
	public static class CharacterOnMapManagerExtend 
	{
		/// <summary>
		/// <para>将传入的敌人容器，剔除掉默认搜索排除的那部分角色</para>
		/// <para>【默认剔除】的配置使用的当前CharacterOnMapManager里所序列化配置的</para>
		/// </summary>
		/// <returns></returns>
		public static List<EnemyARPGCharacterBehaviour> ClipEnemyListOnDefaultType(this List<EnemyARPGCharacterBehaviour> list)
		{
			var defaultList = SubGameplayLogicManager_ARPG.Instance.CharacterOnMapManagerReference
				._DefaultIgnoreEnemyTypeList;	
			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (defaultList.Contains(list[i].SelfBehaviourNamedType))
				{
					list.RemoveAt(i);
				}
			}
			return list;
		}
		public static List<EnemyARPGCharacterBehaviour> ClipEnemyListByDistance(
			this List<EnemyARPGCharacterBehaviour> list,
			float distance)
		{
			var disSq = distance * distance;
			var playerPos = SubGameplayLogicManager_ARPG.Instance.PlayerCharacterBehaviourControllerReference
				.CurrentControllingBehaviour.transform.position;
			playerPos.y = 0f;
			for (int i = list.Count - 1; i >= 0; i--)
			{
				var pos = list[i].GetCollisionCenter();
				pos.y = 0f;
				 if(Vector3.SqrMagnitude( pos - playerPos) > disSq)
				 {
					 list.RemoveAt(i);
				 }
			}
			return list;
		}
		
		public static bool IfIsInDefaultClipType(this EnemyARPGCharacterBehaviour enemy)
		{
			var defaultList = SubGameplayLogicManager_ARPG.Instance.CharacterOnMapManagerReference
				._DefaultIgnoreEnemyTypeList;	
			return defaultList.Contains(enemy.SelfBehaviourNamedType);
		}

		public static List<EnemyARPGCharacterBehaviour> ClipEnemyListOnCharacterType(
			this List<EnemyARPGCharacterBehaviour> list,
			List<CharacterNamedTypeEnum> notSearchTypes)
		{
			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (notSearchTypes.Contains(list[i].SelfBehaviourNamedType))
				{
					list.RemoveAt(i);
				}
			}
			return list;

		}

		
		//sort by distance to player
		public static List<EnemyARPGCharacterBehaviour> SortEnemyListByDistanceToPlayer(
			this List<EnemyARPGCharacterBehaviour> list)
		{
			list.Sort((a, b) =>
			{
				var playerPos = SubGameplayLogicManager_ARPG.Instance.PlayerCharacterBehaviourControllerReference
					.CurrentControllingBehaviour.transform.position;
				playerPos.y = 0f;
				var disA = Vector3.SqrMagnitude(a.GetCollisionCenter() - playerPos);
				var disB = Vector3.SqrMagnitude(b.GetCollisionCenter() - playerPos);
				return disA.CompareTo(disB);
			});
			return list;
		}

		public static List<EnemyARPGCharacterBehaviour> ClipEnemyListOnInvincibleBuff(
			this List<EnemyARPGCharacterBehaviour> list)
		{
			for (int i = list.Count - 1; i >= 0; i --)
			{
				if (list[i].ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum.Invincible_All_WD_完全无敌) ==
				    BuffAvailableType.Available_TimeInAndMeetRequirement ||
				    list[i].ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum.InvincibleByDirector_All_WD_来自机制的完全无敌) ==
				    BuffAvailableType.Available_TimeInAndMeetRequirement) 
				{
					list.RemoveAt(i);
				}
			}
			return list;
		}


		public static List<EnemyARPGCharacterBehaviour> ClipEnemyListOnCharacterType(
			this List<EnemyARPGCharacterBehaviour> list, CharacterNamedTypeEnum type)
		{
			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (list[i].SelfBehaviourNamedType == type)
				{
					list.RemoveAt(i);
				}
			}
			return list;
		}
		
		

		public static List<EnemyARPGCharacterBehaviour> ClipEnemyListOnBuffType(
			this List<EnemyARPGCharacterBehaviour> list,
			RolePlay_BuffTypeEnum type)
		{
			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (list[i].ReceiveBuff_CheckTargetBuff(type) ==
				    BuffAvailableType.Available_TimeInAndMeetRequirement)
				{
					list.RemoveAt(i);
				}
			}
			return list;
		}
	}
}