namespace RPGCore.Projectile
{
    public enum ProjectileBehaviourMode
    {
        /// <summary>飞向某个特定目标，由 追击模式 初始化</summary>
        FlyAndHomingTarget,

        /// <summary>飞向某个特定位置</summary>
        FlyAndSpecifyPosition,

        ///<summary>出生即直接生效</summary>
        Burst,
    
        /// <summary>
        /// <para>从天而降</para>
        /// </summary>
        Drop,
    }
}