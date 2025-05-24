using System.Numerics;

namespace RPGCore.UtilityDataStructure
{
    public struct RP_DS_DisplacementEffectInsideDamage
    {
        /// <summary>
        /// <para>归一化后的一个方向，用来计算位移方向</para>
        /// </summary>
        public Vector3 DisplacementDirection;

        /// <summary>
        /// <para>是否能够造成地形杀的？会影响位移计算的方式</para>
        /// </summary>
        public bool ContainDropCheck;

        /// <summary>
        /// <para>位移力度。-5~5。ForceDisplacement才有用，对于NormalDisplacement没有意义</para>
        /// </summary>
        public float DisplacementForce;


        public float AttackRadius;
        public float AttackAngle;
    }
}