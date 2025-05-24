using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RPGCore.Skill.SkillSelector.SO
{
    [Serializable]
    [CreateAssetMenu(fileName = "Per SSP_Direction",
        menuName = "#SO Assets#/#选择指示器#/一个方向型的")]
    public class SO_SKillSelectorPattern_Direction : ScriptableObject
    {
        public RPSkillIndicatorType_Direction Type;
        public AssetReferenceT<GameObject> TargetPrefab;
        // public GameObject TargetPrefab;
    }
}