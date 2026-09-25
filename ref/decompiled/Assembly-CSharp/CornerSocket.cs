using UnityEngine;

public class CornerSocket : Socket_Base
{
	public override bool ConnectsBuildings => false;

	private void OnDrawGizmosSelected()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.DrawWireCube(selectCenter, selectSize);
	}

	public override bool TestTarget(Construction.Target target)
	{
		return false;
	}

	public override bool CanConnect(Vector3 position, Quaternion rotation, Socket_Base socket, Vector3 socketPosition, Quaternion socketRotation)
	{
		if (!base.CanConnect(position, rotation, socket, socketPosition, socketRotation))
		{
			return false;
		}
		Matrix4x4 matrix4x = Matrix4x4.TRS(position, rotation, Vector3.one);
		Matrix4x4 matrix4x2 = Matrix4x4.TRS(socketPosition, socketRotation, Vector3.one);
		Vector3 from = matrix4x.MultiplyVector(worldRotation * Vector3.forward);
		Vector3 to = matrix4x2.MultiplyVector(socket.worldRotation * Vector3.forward);
		if (Vector3.Angle(from, to) > 2f)
		{
			return false;
		}
		OBB selectBounds = GetSelectBounds(position, rotation);
		OBB selectBounds2 = socket.GetSelectBounds(socketPosition, socketRotation);
		return selectBounds.Intersects(selectBounds2);
	}
}
