using UnityEngine;

namespace RPGCore.Interface
{
    /// <summary>
    /// <para>该对象包含 NonPlayerAttackSlot ，记录于PlayerAttackSlotManager</para>
    /// </summary>
    public interface I_RP_ObjectContainNonPlayerAttackSlot
    {
        public abstract Transform GetSelfTransformAtAllocateTask();

        public abstract void RegisterSelfToPlayerAttackSlotManager();
        public abstract void UnRegisterSelfFromAttackSlotManager();
    }
}