using RPGCore.DataEntry;
using Sirenix.OdinInspector;
using UnityEngine.Pool;
namespace RPGCore.Buff.ConcreteBuff
{
	public class SingleBuffTimedStack_WithDataModifier : PerBuffTimedStack
	{
		[ShowInInspector,LabelText("关联修饰项"),FoldoutGroup("运行时",true)]
		public Float_ModifyEntry_RPDataEntry ModifyEntryRef;
		[ShowInInspector, LabelText("关联数据项"), FoldoutGroup("运行时", true)]
		public Float_RPDataEntry EntryRef;
		public static SingleBuffTimedStack_WithDataModifier GetFromPool()
		{
			return GenericPool<SingleBuffTimedStack_WithDataModifier>.Get();
		}
		public override void OnStackAdd()
		{
			EntryRef.AddDataEntryModifier(ModifyEntryRef);
		}
		public override void OnStackRemove()
		{
			base.OnStackRemove();
			EntryRef.RemoveEntryModifier(ModifyEntryRef);
			ModifyEntryRef.ReleaseToPool();
			ModifyEntryRef = null;
			GenericPool<SingleBuffTimedStack_WithDataModifier>.Release(this);
		}
		public override void OnStackBlock()
		{

			if (EntryRef.ModifyListContains(ModifyEntryRef))
			{
				EntryRef.RemoveEntryModifier(ModifyEntryRef);
			}
			
		}
		public override void OnStackRestore()
		{
			if (EntryRef.ModifyListContains(ModifyEntryRef))
			{
				EntryRef.AddDataEntryModifier(ModifyEntryRef);
			}
		}

		public override void OnStackFixedTick(float ct, int cf, float delta)
		{
			
		}

		public override string ToString()
		{
			string s = $"目标修饰项:{EntryRef.RP_DataEntryType},修饰为{ModifyEntryRef.ToString()},起始时间{FromTime},生命周期{InitDuration}";
			return s ;
		}
	}
}