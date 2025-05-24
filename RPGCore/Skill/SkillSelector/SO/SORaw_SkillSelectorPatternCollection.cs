using System;
using System.Collections.Generic;
using Global;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RPGCore.Skill.SkillSelector.SO
{
    [Serializable]
    [CreateAssetMenu(fileName = "Skill Selector Pattern Collection",
        menuName = "#SO Assets#/#选择指示器#/[集合]选择指示器集合")]
    public class SORaw_SkillSelectorPatternCollection : SOCollectionBase
    {
#if UNITY_EDITOR
        [Button("刷新：将库内容刷新成与文件夹内容一致的样子"), PropertyOrder(2)]
        public override void Refresh()
        {
            UnityEditor.AssetDatabase.Refresh();
            
            
            
            DirectionCollection.Clear();
            var contents = Resources.FindObjectsOfTypeAll<SO_SKillSelectorPattern_Direction>();
            foreach (var perSO in contents)
            {
                DirectionCollection.Add(perSO);
            }

            LineCollection.Clear();
            var contents2 = Resources.FindObjectsOfTypeAll<SO_SKillSelectorPattern_Line>();
            foreach (var perSO in contents2)
            {
                LineCollection.Add(perSO);
            }

            PositionCollection.Clear();
            var contents3 = Resources.FindObjectsOfTypeAll<SO_SKillSelectorPattern_Position>();
            foreach (var perSO in contents3)
            {
                PositionCollection.Add(perSO);
            }


            RangeCollection.Clear();
            var contents4 = Resources.FindObjectsOfTypeAll<SO_SKillSelectorPattern_Range>();
            foreach (var perSO in contents4)
            {
                RangeCollection.Add(perSO);
            }

            SectorCollection.Clear();
            var contents5 = Resources.FindObjectsOfTypeAll<SO_SKillSelectorPattern_Sector>();
            foreach (var perSO in contents5)
            {
                SectorCollection.Add(perSO);
            }

            
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
        }

        [Header(("==============================="))]
#endif

        public List<SO_SKillSelectorPattern_Direction> DirectionCollection;

        public List<SO_SKillSelectorPattern_Line> LineCollection;
        public List<SO_SKillSelectorPattern_Position> PositionCollection;
        public List<SO_SKillSelectorPattern_Range> RangeCollection;
        public List<SO_SKillSelectorPattern_Sector> SectorCollection;
    

    }
}