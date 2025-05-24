using System;
using Sirenix.OdinInspector;
namespace ARPG.Character.Enemy.AI.Listen
{
	[Serializable]
	public abstract class BaseAIListenComponent
	{
		[NonSerialized, LabelText("关联的AIBrain"), ShowInInspector,FoldoutGroup("运行时",true)]
		public SOConfig_AIBrain RelatedAIBrainRuntimeInstance;

		protected static float _currentTime;
		protected static int _currentFrame;
		public static void FixedUpdateTime(float ct, int cf)
		{
			_currentTime = ct;
			_currentFrame = cf;
		}


		public virtual void InitializeAndProcessRegister(SOConfig_AIBrain brainRef)
		{
			RelatedAIBrainRuntimeInstance = brainRef;
		}

		/// <summary>
		/// <para>解除有关监听的注册。只是【解除】，并不是【销毁】</para>
		/// </summary>
		public virtual void UnRegisterListenInActionBus()
		{
			
		}

	}

	public interface I_AIListenNeedTick
	{
		public abstract void FixedUpdateTick(float ct, int cf, float delta);
	}
}