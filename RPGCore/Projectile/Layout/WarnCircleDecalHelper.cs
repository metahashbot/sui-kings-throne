using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace RPGCore.Projectile.Layout
{
	public class WarnCircleDecalHelper : MonoBehaviour
	{
		[SerializeField, LabelText("基准校准y高度修正")]
		public float BaseYOffset = 0.5f;

		[SerializeField, Required, LabelText("基准源Decal")]
		public DecalProjector OriginalDecalProjector;

		[SerializeField,Required,LabelText("覆盖Decal")]
		public DecalProjector OverlayDecalProjector;

		public void SetPositionAndScale(Vector3 pos, float radius)
		{
			transform.position = pos;
			OriginalDecalProjector.size = new Vector3(radius, radius, 5f);
			OverlayDecalProjector.size = new Vector3(radius, radius, 5f);
		}

	}
}