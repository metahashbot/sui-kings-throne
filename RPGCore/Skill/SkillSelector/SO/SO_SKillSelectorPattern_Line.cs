using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RPGCore.Skill.SkillSelector.SO
{
    [Serializable]
    [CreateAssetMenu(fileName = "Per SSP_Line",
        menuName = "#SO Assets#/#选择指示器#/一个线型的")]
    public class SO_SKillSelectorPattern_Line : ScriptableObject
    {
        public RPSkillIndicatorType_Line Type;
        public AssetReferenceT<GameObject> TargetPrefab;
        // public GameObject TargetPrefab;
    }
}