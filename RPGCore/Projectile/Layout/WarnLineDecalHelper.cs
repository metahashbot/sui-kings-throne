using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace RPGCore.Projectile.Layout
{
	public class WarnLineDecalHelper : MonoBehaviour
	{
		[SerializeField, LabelText("基准校准y高度修正")]
		public float BaseYOffset = 0.5f;
		
		[SerializeField, Required, LabelText("基准源Decal")]
		public DecalProjector OriginalDecalProjector;


		public void FixedUpdateTick(float currentTime, int currentFrame, float delta)
		{
			
		}

		public void SetEndPosition(Vector3 endPos)
		{
			float distance = Vector3.Distance(endPos, transform.position);
			Vector3 originalSize = OriginalDecalProjector.size;

			Vector3 cEndPos = endPos;
			cEndPos.y = 0f;
			Vector3 cSelfPos = transform.position;
			cSelfPos.y = 0f;
			OriginalDecalProjector.size = new Vector3(originalSize.x, distance * 2f, originalSize.z);


			transform.LookAt(endPos, transform.up);



		}

	}
}