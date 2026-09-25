using UnityEngine;

public class ModelConditionTest_FoundationCornerRight : ModelConditionTest
{
	private const string square_south = "foundation/sockets/corner/1-r";

	private const string square_north = "foundation/sockets/corner/3-r";

	private const string square_west = "foundation/sockets/corner/2-r";

	private const string square_east = "foundation/sockets/corner/4-r";

	private const string triangle_south = "foundation.triangle/sockets/corner/1-r";

	private const string triangle_northwest = "foundation.triangle/sockets/corner/2-r";

	private const string triangle_northeast = "foundation.triangle/sockets/corner/3-r";

	private string socket = string.Empty;

	protected void OnDrawGizmosSelected()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.color = Color.gray;
		Gizmos.DrawWireCube(new Vector3(1.5f, 1.5f, 0f), new Vector3(3f, 3f, 3f));
	}

	protected override void AttributeSetup(GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		Vector3 vector = worldRotation * Vector3.right;
		if (name.Contains("foundation.triangle"))
		{
			if (vector.z < -0.9f)
			{
				socket = "foundation.triangle/sockets/corner/1-r";
			}
			if (vector.x < -0.1f)
			{
				socket = "foundation.triangle/sockets/corner/2-r";
			}
			if (vector.x > 0.1f)
			{
				socket = "foundation.triangle/sockets/corner/3-r";
			}
			return;
		}
		if (vector.z < -0.9f)
		{
			socket = "foundation/sockets/corner/1-r";
		}
		if (vector.z > 0.9f)
		{
			socket = "foundation/sockets/corner/3-r";
		}
		if (vector.x < -0.9f)
		{
			socket = "foundation/sockets/corner/2-r";
		}
		if (vector.x > 0.9f)
		{
			socket = "foundation/sockets/corner/4-r";
		}
	}

	public override bool DoTest(BaseEntity ent)
	{
		EntityLink entityLink = ent.FindLink(socket);
		if (entityLink == null)
		{
			return false;
		}
		for (int i = 0; i < entityLink.connections.Count; i++)
		{
			BuildingBlock buildingBlock = entityLink.connections[i].owner as BuildingBlock;
			if (!(buildingBlock == null) && (!(buildingBlock.blockDefinition.info.name.token != "foundation") || !(buildingBlock.blockDefinition.info.name.token != "foundation_triangle")))
			{
				return false;
			}
		}
		return true;
	}
}
