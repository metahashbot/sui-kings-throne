using System;
using ARPG.Character.Base;
using ARPG.Equipment;

namespace ARPG.Character.Player
{
    [Serializable]
    public class PAEC_亚瑟的连击 : BasePlayerAnimationEventCallback
    {

        public bool ToggleToOn;
        public override BasePlayerAnimationEventCallback ExecuteByWeapon(BaseARPGCharacterBehaviour behaviour, BaseWeaponHandler weaponHandler,
            bool createNewWhenExist = false)
        {
            return base.ExecuteByWeapon(behaviour, weaponHandler, createNewWhenExist);
        }
        
        
        
        
        
    }
}