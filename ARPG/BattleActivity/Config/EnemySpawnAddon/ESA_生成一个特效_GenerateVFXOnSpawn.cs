using System;
using System.Collections.Generic;
using ARPG.Manager.Config;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace ARPG.BattleActivity.Config.EnemySpawnAddon
{

	[Serializable]
	public class ESA_生成一个特效_GenerateVFXOnSpawn : BaseEnemySpawnAddon
	{
		[SerializeField, LabelText("特效预制件")]
		public GameObject _prefab_VFXPrefab;
		
		
		[SerializeField,LabelText("生成高度设置为离地这么高")]
		public float _SpawnHeight = 0.1f;
		
		[SerializeField,LabelText("生成尺寸设置为")]
		public float _SpawnScale = 1;
		
		
		

		public override void ResetOnReturn()
		{

			GenericPool<ESA_生成一个特效_GenerateVFXOnSpawn>.Release(this);


		}
		
		
		
	}
}