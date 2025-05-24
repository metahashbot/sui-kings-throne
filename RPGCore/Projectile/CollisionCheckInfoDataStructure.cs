using ARPG.Character.Base;
using RPGCore.Projectile;
using Unity.Mathematics;
using UnityEngine.Serialization;
namespace RPGCore
{
	/// <summary>
	/// <para>碰撞检测信息，一个Projectile。基本碰撞信息</para>
	/// </summary>
	public struct CollisionCheckInfo_ProjectileFull
	{
		public ProjectileBehaviour_Runtime ProjectileBehaviourRef;

		public float ColliderCircleRadius;


		public CollisionCheckInfo_ProjectileFull(ProjectileBehaviour_Runtime pb,
			float colliderRadius) : this()
		{
			ProjectileBehaviourRef = pb;
			ColliderCircleRadius = colliderRadius;
		}

	}

	/// <summary>
	/// <para>在Job计算中实际将要使用的数据结构</para>
	/// <para>一个Projectile只能是圆的！</para>
	/// </summary>
	public struct CC_ProjectileInfoInJob
	{
		/// <summary>
		/// <para>碰撞层关系。</para>
		/// <para> 1位：对敌人生效，常规；</para>
		/// <para> 2位：对玩家生效， 常规</para>
		/// <para> 3位：对敌人子弹生效，</para>
		/// <para> 4位： 对玩家子弹生效</para>
		/// </summary>
		public int CollisionMaskLayer;
		public int Index;
		public float2 FromPosition;
		public float Radius;
	}
	/// <summary>
	/// <para>碰撞检测信息，可以造成RPBehaviour碰撞的东西</para>
	/// </summary>
	public struct CollisionCheckInfo_RPBehaviourFull
	{
		public BaseARPGCharacterBehaviour RPBehaviourRef;
		public int LayerMask;
		public float4 ColliderInfo;
		public float2 ColliderOffsetPos;
		public float2 FromPos;
	}

	public struct CC_RPBehaviourInfoInJob
	{
		/// <summary>
		/// <para>碰撞层关系，</para>
		/// <para>1位：这是敌人，常规</para>
		/// <para>2位：这是玩家，常规</para>
		/// </summary>
		public int CollisionMaskLayer;
		
		public int Index;
		public float2 FromPos;
		
		public float4 ColliderInfo;
		public float2 ColliderOffsetPos;

	}

	/// <summary>
	/// <para>碰撞检测结果，第一部分，常规子弹碰撞 - 常规RP对象碰撞 </para>
	/// </summary>
	public struct CCResult_ProjectileToRPBehaviour
	{
		public int ProjectileIndex;
		public int RPBehaviourIndex;
		public float2 CollisionPositionXZ;
	}


}