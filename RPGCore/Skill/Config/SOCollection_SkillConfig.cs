using System;
using System.Collections.Generic;
using Global;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RPGCore.Skill.Config
{
    [Serializable]
    [CreateAssetMenu(fileName = "RPSkill Collection", menuName = "#SO Assets#/#RPG Core#/#Collection#/(S)RPSkill Collection")]
    public class SOCollection_SkillConfig : SOCollectionBase
    {
#if UNITY_EDITOR
        [Button("刷新：将库内容刷新成与文件夹内容一致的样子"), PropertyOrder(2)]
        public override void Refresh()
        {
            UnityEditor.AssetDatabase.Refresh();
            RPSkillCollection.Clear();
            var path = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_RPSkill");
            foreach (var perGUID in path)
            {
                var perPath = UnityEditor.AssetDatabase.GUIDToAssetPath(perGUID);
                var perSO = UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_RPSkill>(perPath);

                if (perSO.ConcreteSkillFunction == null)
                {
                    DBug.LogWarning( "在" + perPath + "中，ConcreteSkillFunction为空，跳过这个SO");
                    continue;
                }
                RPSkillCollection.Add(perSO);
            }
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
        }

        [Header(("==============================="))]
#endif

        public List<SOConfig_RPSkill> RPSkillCollection;

        public SOConfig_RPSkill GetRPSkillConfigByTypeAndLevel(RPSkill_SkillTypeEnum type, int level = 1)
        {
            if (type == RPSkill_SkillTypeEnum.None)
            {
                Debug.LogWarning("要求加载了一个None的技能，可能是配置错误，检查一下");
                return null;
            }
            
            
            for (int i = 0; i < RPSkillCollection.Count; i++)
            {
                if (RPSkillCollection[i].ConcreteSkillFunction.SkillType == type &&
                    RPSkillCollection[i].ConcreteSkillFunction.SkillLevel == level)
                { ;
                    return RPSkillCollection[i];
                }
            }
#if UNITY_EDITOR
            Debug.LogError("没有在RPSkillCollection中查找到类型为" + type + "等级为" + level + "的技能配置信息，是否为未刷新？");

#endif
            return null;
        }
    }
}
namespace RPGCore.Skill.Config
{
}