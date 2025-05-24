using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RPGCore.Skill.SkillSelector.SO
{
    [Serializable]
    [CreateAssetMenu(fileName = "Per SSP_Sector",
        menuName = "#SO Assets#/#选择指示器#/一个扇形的")]
    public class SO_SKillSelectorPattern_Sector : ScriptableObject
    {
        public RPSkillIndicatorType_Sector Type;
        public AssetReferenceT<GameObject> TargetPrefab;


        // public GameObject TargetPrefab;
    }
}