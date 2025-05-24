using System;
using GameplayEvent;
namespace RPGCore.UtilityDataStructure
{
	/// <summary>
	/// <para>一切经由EP_BaseArea所触发的游戏性事件，obj1必定是对应的BaseArea，obj2可能会有触发的那个角色的GroupRef</para>
	/// </summary>
	[Serializable]
	public struct DS_GameplayEventArguGroup
	{
		public string GameplayEventUID;
		public GameplayEventScopeEnum GameplayEventScope;
		public int GameplayEventIndex;
		public string ArguName;
		public Nullable<int> IntArgu1;
		public Nullable<int> IntArgu2;
		public Nullable<float> FloatArgu1;
		public Nullable<float> FloatArgu2;
		public Nullable<float> FloatArgu3;
		public Nullable<float> FloatArgu4;
		public object ObjectArguStr;
		public object ObjectArgu1;
		public object ObjectArgu2;


		/// <summary>
		/// <para>只要任何一个参数有意义，就表明它被使用了，会装箱并传入ActionBus。否则不会传入</para>
		/// </summary>
		/// <returns></returns>
		public bool IfArguGroupUsed()
		{
			if (!string.IsNullOrEmpty(GameplayEventUID))
			{
				return true;
			}
			if(GameplayEventScope != GameplayEventScopeEnum.None_空域)
			{
				return true;
			}
			if (GameplayEventIndex != 0)
			{
				return true;
			}
			if (!string.IsNullOrEmpty(ArguName))
			{
				return true;
			}
			if (IntArgu1.HasValue)
			{
				return true;
			}
			if (IntArgu2.HasValue)
			{
				return true;
			}
			if (FloatArgu1.HasValue)
			{
				return true;
			}
			if (FloatArgu2.HasValue)
			{
				return true;
			}
			if (FloatArgu3.HasValue)
			{
				return true;
			}
			if (FloatArgu4.HasValue)
			{
				return true;
			}
			if (ObjectArguStr != null)
			{
				return true;
			}
			if (ObjectArgu1 != null)
			{
				return true;
			}
			if (ObjectArgu2 != null)
			{
				return true;
			}
			return false;
			
		}
	}
	
}