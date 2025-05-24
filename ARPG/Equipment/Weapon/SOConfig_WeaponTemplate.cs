using System;
using System.Collections.Generic;
using ARPG.Character.Base;
using ARPG.Character.Enemy.AI;
using RPGCore.Projectile.Layout;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Equipment
{
	/// <summary>
	/// <para>一个武器的SOConfig。类似RPBuff结构，运行时需要一个SO的运行时实例。</para>
	/// <para>普通的装备目前不需要RPBuff结构。因为武器业务比较多，尤其是和输入有关的。所以用这样的结构来处理复杂的输入和响应问题</para>
	/// <para>武器文档：https://xxi1p77cfp.feishu.cn/wiki/JorTwcZApigjQbkZSLAcWkftnNj#mindmap</para>
	/// </summary>
	[CreateAssetMenu(fileName = "武器SO配置", menuName = "#SO Assets#/#RPG Core#/#武器#/武器SO配置", order = 143)]
	[Serializable]
	public class SOConfig_WeaponTemplate : ScriptableObject
	{
#if UNITY_EDITOR
		[OnInspectorGUI]
		private void RedrawConstantly() { Sirenix.Utilities.Editor.GUIHelper.RequestRepaint(); }
#endif
		

		[SerializeReference, LabelText("具体的武器Handler")]
		public BaseWeaponHandler WeaponFunction;

		public virtual void AnimationCustomEventProxy(string s)
		{
			WeaponFunction.RelatedCharacterBehaviourRef.GetSelfRolePlayArtHelper()._SheetCallback_CustomEvent(s);
		}

		private void OnDestroy()
		{
			
			WeaponFunction.UnloadAndClear();
			
		}


#if UNITY_EDITOR
		// [Button("转换所有武器上的动画配置")]
		// private void _ConvertAllAnimationOnWeapon()
		// {
		// 	var allWeaponConfig = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_WeaponTemplate");
		// 	foreach (var weaponConfig in allWeaponConfig)
		// 	{
		// 		var weaponConfigPath = UnityEditor.AssetDatabase.GUIDToAssetPath(weaponConfig);
		// 		SOConfig_WeaponTemplate weaponConfigInstance = UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_WeaponTemplate>(weaponConfigPath);
		// 		if (weaponConfigInstance.WeaponFunction == null)
		// 		{
		// 			continue;
		// 		}
		//
		// 		// create SOConfig_PresetAnimationInfoBase
		// 		SOConfig_PresetAnimationInfoBase newAnimationConfigFile = ScriptableObject.CreateInstance<SOConfig_PresetAnimationInfoBase>();
		// 		newAnimationConfigFile.name = newAnimationConfigFile.name.Replace(weaponConfigInstance.name,
		// 			weaponConfigInstance.name + "_动画配置");
		//
		// 		newAnimationConfigFile.SelfAllPresetAnimationInfoList = new List<AnimationInfoBase>();
		//
		// 		foreach (var perANI in weaponConfigInstance.WeaponFunction.SelfAllPresetAnimationInfoList_Serialize)
		// 		{
		// 			newAnimationConfigFile.SelfAllPresetAnimationInfoList.Add(perANI);
		// 		}
		//
		// 		weaponConfigInstance.WeaponFunction.PresetAnimationInfoList_File.Add(newAnimationConfigFile);
		//
		// 		//save file aside weaponConfigInstance
		// 		var newAnimationConfigPath = weaponConfigPath.Replace(".asset", "_动画配置.asset");
		// 		UnityEditor.AssetDatabase.CreateAsset(newAnimationConfigFile, newAnimationConfigPath);
		// 		
		// 	}
		// }
#endif
	}
}