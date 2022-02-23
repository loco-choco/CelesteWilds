using UnityEngine;

namespace CelesteWilds
{
    public class FollowTransform : MonoBehaviour
	{
		public Transform targetTransform;

		public Vector3 localPosition;
		public Vector3 localFowardOrientation;
		public Vector3 localUpOrientation;

		private void Update()
		{
			UpdateParameters();
		}

		public void UpdateParameters() 
		{
			if (targetTransform == null)
				return;
			transform.position = targetTransform.TransformPoint(localPosition);

			transform.rotation = Quaternion.LookRotation(targetTransform.TransformDirection(localFowardOrientation)
													, targetTransform.TransformDirection(localUpOrientation));
		}
		public void SetPosition(Vector3 position) 
		{
			localPosition = targetTransform.InverseTransformPoint(position);
		}

		public void SetRotation(Vector3 foward, Vector3 up)
		{
			localFowardOrientation = targetTransform.InverseTransformDirection(foward);
			localUpOrientation = targetTransform.InverseTransformDirection(up);
		}

		public void SetFollow(Transform targetTransform)
		{
			this.targetTransform = targetTransform;
			SetPosition(transform.position);
			SetRotation(transform.forward, transform.up);
		}
	}

}
