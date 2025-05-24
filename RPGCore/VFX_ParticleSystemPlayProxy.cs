using System;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore
{
	[TypeInfoBox("【粒子系统特效编辑期信息】\n" +
	             "！！！！！如果出现以下情况，必须要手动挂载！！！！！\n" +
	             "·：这个特效在使用时会调整播放速度\n" +
	             "·：这个特效使用了 生命周期上的旋转 + 曲线 \n")]
	[AddComponentMenu("!#编辑期辅助#/【特效】粒子系统特效编辑期信息" , -1)]
	public class VFX_ParticleSystemPlayProxy : MonoBehaviour
	{
		[SerializeField, LabelText("手动指定播放时长？")]
		public bool ManualSetDuration;

		[SerializeField, Required, LabelText("播放一次需要的时长")]
		[ShowIf("ManualSetDuration")]
		public float VFXSelfDuration;



		public ParticleSystem MainParticleSystemRef { get; private set; }
		

		public void SetMainPS(ParticleSystem ps)
		{
			MainParticleSystemRef = ps;
			// psArray = GetComponentsInChildren<ParticleSystem>(true);
			var main = MainParticleSystemRef.main;
			main.playOnAwake = false;
			foreach (ParticleSystem perPS in GetComponentsInChildren<ParticleSystem>(true))
			{
				if (perPS == MainParticleSystemRef)
				{
					continue;
				}
				else
				{
					if (perPS.main.stopAction == ParticleSystemStopAction.Disable)
					{
						var module = perPS.main;
						module.stopAction = ParticleSystemStopAction.None;
#if UNITY_EDITOR
						Debug.LogError($"【特效】{perPS.name}的停止行为被修改为【None】，请注意检查，其父级为{MainParticleSystemRef.gameObject.name}");
#endif
					}
				}
			}
			// foreach (ParticleSystem perPS in psArray)
			// {
			// 	var module = perPS.main;
			// 	module.playOnAwake = false;
			// }
			
		}


		// private ParticleSystem[] psArray;

		public enum ParticleSystemPlayProxyStateTypeEnum
		{
			None_未指定 = 0,
			Inactive_不活跃 = 1,
			Playing_正常播放 = 2,
			WaitVanish_等待消散 = 3, }

		[NonSerialized]
		public ParticleSystemPlayProxyStateTypeEnum CurrentState = ParticleSystemPlayProxyStateTypeEnum.None_未指定;
		/// <summary>
		/// 已经过的时间。不计算SimulationSpeed的乘算影响
		/// </summary>
		[NonSerialized]
		public float ElapsedTime;

		[NonSerialized]
		public float SimulationSpeed = 1f;

		/// <summary>
		/// <para>当前模拟时间</para>
		/// </summary>
		[NonSerialized]
		public float CurrentSimulationTime;
		/// <summary>
		/// 额外的不在粒子本身中记录的延迟时间，会加到可播放时间上。常见于预警后生成的子弹
		/// </summary>
		[NonSerialized]
		public float ExtraDelayDuration;

		public void Play(bool reset = true)
		{
			CurrentState = ParticleSystemPlayProxyStateTypeEnum.Playing_正常播放;
		
			gameObject.SetActive(false);
			gameObject.SetActive(true);

			if (reset)
			{
				Reset();
			}
			MainParticleSystemRef.Play(false);
			MainParticleSystemRef.Pause(false);
			// foreach (ParticleSystem perPS in psArray)
			// {
			// 	perPS.Play(false);
			// 	perPS.Pause(false);
			// }

			
		}
		
		public void SetStopAction( ParticleSystemStopAction action)
		{
			var main = MainParticleSystemRef.main;
			main.stopAction = action;
		}

		public void Reset()
		{
			CurrentSimulationTime = 0f;
			ElapsedTime = 0f;
			SimulationSpeed = 1f;
			ExtraDelayDuration = 0f;
		}


		public void UpdateTick(float ct, int cf, float delta)
		{
			switch (CurrentState)
			{
				case ParticleSystemPlayProxyStateTypeEnum.None_未指定:
					break;
				case ParticleSystemPlayProxyStateTypeEnum.Inactive_不活跃:
					break;
				case ParticleSystemPlayProxyStateTypeEnum.Playing_正常播放:
					if (!gameObject.activeInHierarchy)
					{
						return;
					}
					ElapsedTime += delta;
					CurrentSimulationTime += delta * SimulationSpeed;
					
					// Debug.Log($"{MainParticleSystemRef.name } ____{delta} ____  {CurrentSimulationTime}");

					MainParticleSystemRef.Simulate(CurrentSimulationTime, true, true);
					// foreach (ParticleSystem perPS in psArray)
					// {
					// 	if (!perPS.isStopped)
					// 	{
					// 		perPS.Simulate(CurrentSimulationTime, false, true);
					// 	}
					// }
					if (ManualSetDuration && ElapsedTime > (VFXSelfDuration / SimulationSpeed))
					{
						StopEmitNewParticle();
					}
					else if(!ManualSetDuration && !MainParticleSystemRef.main.loop && ElapsedTime > (MainParticleSystemRef.main.duration / SimulationSpeed))
					{
						StopEmitNewParticle();
					}
					break;
				case ParticleSystemPlayProxyStateTypeEnum.WaitVanish_等待消散:
					ElapsedTime += delta;
					CurrentSimulationTime += delta * SimulationSpeed;
					MainParticleSystemRef.Simulate(CurrentSimulationTime, true, false, true);
					if (MainParticleSystemRef.particleCount == 0)
					{
						StopImmediately();
					}
					break;
			}
		}


		/// <summary>
		/// 停止产生新的粒子。已经存在的粒子仍然会持续播放，直到自然结束。区别于StopImmediate
		/// </summary>
		public void StopEmitNewParticle(bool clearExist = false)
		{
			CurrentState = ParticleSystemPlayProxyStateTypeEnum.WaitVanish_等待消散;
			MainParticleSystemRef.Stop(true);
			if (clearExist)
			{
				MainParticleSystemRef.Clear(false);
			}
		}

		/// <summary>
		/// 立刻停止所有。清除
		/// </summary>
		public void StopImmediately()
		{
			CurrentState = ParticleSystemPlayProxyStateTypeEnum.Inactive_不活跃;
			MainParticleSystemRef.Stop(true);
			MainParticleSystemRef.Clear(true);
			gameObject.SetActive(false);
		}


		public void Replay(bool reset = true)
		{
			StopEmitNewParticle();
			Play(reset);
		}
	}
}