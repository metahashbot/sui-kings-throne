using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RPGCore.Skill.SkillSelector.SO
{
    [Serializable]
    [CreateAssetMenu(fileName = "Per SSP_Position",
        menuName = "#SO Assets#/#选择指示器#/一个位置型的")]
    public class SO_SKillSelectorPattern_Position : ScriptableObject
    {
        public RPSkillIndicatorType_Position Type;
        public AssetReferenceT<GameObject> TargetPrefab;
        // public GameObject TargetPrefab;
    }
}