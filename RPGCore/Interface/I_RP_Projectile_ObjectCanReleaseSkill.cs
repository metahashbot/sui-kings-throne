using System.Collections.Generic;
using Global.ActionBus;
using RPGCore.DataEntry;
using UnityEngine;
namespace RPGCore.Interface
{
    /// <summary>
    /// <para>用于 当作用Damage/Buff/DataEntry的时候来源是一个 Projectile，这时候需要获取相关数据的时候，</para>
    /// <para>由这个接口 将 Projectile 转换为 RPBehaviour或Tower</para>
    /// </summary>
    public interface I_RP_Projectile_ObjectCanReleaseProjectile
    {
        public abstract LocalActionBus GetRelatedActionBus();
        public abstract Vector3 GetCasterPosition();
        /// <summary>
        /// <para>获取Caster的前方。这个前方是子弹飞的那个方向，不是角色的前方。应该是右方</para>
        /// </summary>
        /// <returns></returns>
        public abstract Vector3 GetCasterForwardDirection();
        public abstract Vector3 GetCasterY0Position();
        public abstract void FillDataCache(Dictionary<RP_DataEntry_EnumType, float> dataEntryCache);

    }
}