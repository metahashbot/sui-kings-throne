using System;
using Global;
using RPGCore.Projectile.Layout;
using RPGCore.Skill.Config.Effector;
using RPGCore.Skill.Config.Requirement;
using RPGCore.Skill.Config.Selector;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RPGCore.Skill.Config
{
    [Serializable]
    [CreateAssetMenu(fileName = "单个技能", menuName = "#SO Assets#/#RPG Core#/单个技能, order = 144")]
    public class SOConfig_RPSkill : ScriptableObject
    {
#if UNITY_EDITOR
        [OnInspectorGUI]
        private void RedrawConstantly() { Sirenix.Utilities.Editor.GUIHelper.RequestRepaint(); }
#endif

        
        
#if UNITY_EDITOR
        [Button("把SO文件的名字改成Type的名字"), GUIColor(1f, 1f, 0f)]
        private void Rename()
        {

            //rename so file name to RolePlay_BuffTypeEnum
            var soPath = UnityEditor.AssetDatabase.GetAssetPath(this);
            var soName = ConcreteSkillFunction.SkillType.ToString();
            UnityEditor.AssetDatabase.RenameAsset(soPath, soName);

        }
        [Button("刷新一下技能的SO库"), GUIColor(1f, 1f, 0f)]
        private void RefreshCollection()
        {
            //call SOCollection_RPBuff Refresh
            var gcahh = Addressables.LoadAssetAsync<GameObject>("GCAHH").WaitForCompletion()
                .GetComponent<GlobalConfigurationAssetHolderHelper>();
            gcahh.Collection_SkillConfig.Refresh();

        }
        [Button("打开以编辑↓这个script↓"), PropertyOrder(20), GUIColor(1f, 1f, 0f)]
        private void _BUTTON_OpenScript()
        {
            if (ConcreteSkillFunction == null)
            {
                return;
            }
            //find concrete class name
            var concreteClassName = ConcreteSkillFunction.GetType().Name;
            //find all .cs file by assetdatabse
            var allScriptFiles = UnityEditor.AssetDatabase.FindAssets("t:Script");
            foreach (var scriptFile in allScriptFiles)
            {
                var scriptFilePath = UnityEditor.AssetDatabase.GUIDToAssetPath(scriptFile);
                if (scriptFilePath.Contains(concreteClassName))
                {
                    UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(scriptFilePath, 0);
                    break;
                }
            }
        }
#endif

        [SerializeReference, LabelText("具体的Skill Handler"), PropertyOrder(30)]
        public BaseRPSkill ConcreteSkillFunction;



#if UNITY_EDITOR

        // [Button("迁移所有技能的PAEC")]
        // private void _ConvertAllPAECInSkill()
        // {
        //     var allSkills = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_RPSkill");
        //     foreach (var skill in allSkills)
        //     {
        //         var skillPath = UnityEditor.AssetDatabase.GUIDToAssetPath(skill);
        //         var skillInstance = UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_RPSkill>(skillPath);
        //         if (skillInstance.ConcreteSkillFunction == null)
        //         {
        //             continue;
        //         }
        //         var concreteSkill = skillInstance.ConcreteSkillFunction;
        //
        //         UnityEditor.EditorUtility.SetDirty(skillInstance);
        //     }
        //
        // }
#endif
    }
}