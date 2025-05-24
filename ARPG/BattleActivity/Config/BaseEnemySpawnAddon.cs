using System;
using System.Collections.Generic;
namespace ARPG.Manager.Config
{
	/// <summary>
	/// <para>在敌人生成的时候，为其附加额外的内容。</para>
	/// <para>生成出来的敌人不应当直接“使用”（维持引用）基于Addon的内容——应当当做只读的数据信息，使用完成后就不管了的那种，毕竟这里只是一些配置</para>
	/// </summary>
	[Serializable]
	public abstract class BaseEnemySpawnAddon
	{

		public abstract void ResetOnReturn();
		
		
	}


}