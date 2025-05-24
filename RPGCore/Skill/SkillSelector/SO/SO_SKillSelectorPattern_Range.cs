using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RPGCore.Skill.SkillSelector.SO
{
    [Serializable]
    [CreateAssetMenu(fileName = "Per SSP_Range",
        menuName = "#SO Assets#/#选择指示器#/一个范围型的")]
    public class SO_SKillSelectorPattern_Range : ScriptableObject
    {
        public RPSkillIndicatorType_Range Type;
        public AssetReferenceT<GameObject> TargetPrefab;
        // public GameObject TargetPrefab;
    }
}