
using Sirenix.OdinInspector;
using UnityEngine;

#pragma warning disable CS0162
#pragma warning disable CS0414

namespace RPGCore.Projectile
{
	/// <summary>
	/// <para>一个投射物行为的编辑期辅助MonoBehaviour。</para>
	/// <para>真正在运行时不会真的存在，只是 #if UNITY_EDITOR 的</para>
	/// </summary>
	[TypeInfoBox("这是一个投射物行为的编辑期辅助MonoBehaviour。\n真正在运行时不会真的存在，只是 #if UNITY_EDITOR 的")]
	public sealed class ProjectileBehaviour_EditorHelper : MonoBehaviour
    {
        
        [ShowInInspector,LabelText("关联的ProjectileBehaviour"),FoldoutGroup("运行时",true)]
        public ProjectileBehaviour_Runtime SelfRelatedProjectileBehaviour;
	    
	    

#if UNITY_EDITOR
	    private void OnDrawGizmos()
	    {
		    if (SelfRelatedProjectileBehaviour == null)
		    {
			    return;
		    }
	        
		    Gizmos.color = Color.magenta;
		    if (!SelfRelatedProjectileBehaviour.CurrentCollisionActive)
		    {
			    return;
		    }

		    var pos = SelfRelatedProjectileBehaviour.RelatedGORef.transform.position;
		    var radius = SelfRelatedProjectileBehaviour.ProjectileColliderRadius *
		                 SelfRelatedProjectileBehaviour.CurrentLocalSize;
		    Gizmos.DrawWireSphere(pos, radius);



	    }
#endif


    }
}