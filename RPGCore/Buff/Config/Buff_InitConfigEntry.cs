using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;

namespace RPGCore.Buff.Config
{
    [Serializable]
    public class Buff_InitConfigEntry
    {
#if UNITY_EDITOR
        public void _定位原始SO()
        {
            var buffSO = AssetDatabase.LoadAssetAtPath<SOCollection_RPBuff>(
                "Assets/SOAssets/RPGCore/RPBuff Collection.asset");
            foreach (var so in buffSO.BuffCollection)
            {
                if (so.ConcreteBuffFunction.SelfBuffType == this.Type)
                {
                    Selection.activeObject = so;
                    return;
                }
            }
        }
    
    
        [InlineButton(nameof(_定位原始SO))]
#endif
    
    
         public RolePlay_BuffTypeEnum Type;


         public List<PerBuffInfoGroup> BuffSaveInfo;
    }
}
namespace RPGCore.Buff.Config
{
}