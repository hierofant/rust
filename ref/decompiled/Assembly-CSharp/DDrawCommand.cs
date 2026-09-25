using UnityEngine;

public class DDrawCommand
{
	public static string Sphere(Vector3 position, float duration, Color color, float radius, bool distanceFade = true, bool zTest = true, NetworkableId entityId = default(NetworkableId), string id = null)
	{
		return ConsoleSystem.BuildCommand("ddraw.sphere", duration, color, position, radius, distanceFade, zTest, entityId, id);
	}

	public static string Box(Vector3 position, float duration, Color color, Vector3 size, Quaternion rotation = default(Quaternion), bool distanceFade = true, bool zTest = true, NetworkableId entityId = default(NetworkableId), string id = null)
	{
		return ConsoleSystem.BuildCommand("ddraw.box", duration, color, position, size.ToString(), ((rotation != default(Quaternion)) ? rotation : Quaternion.identity).eulerAngles, distanceFade, zTest, entityId, id);
	}

	public static string Text(Vector3 position, float duration, Color color, string text, float scale = 2f, bool distanceFade = true, bool zTest = false, NetworkableId entityId = default(NetworkableId), string id = null)
	{
		return ConsoleSystem.BuildCommand("ddraw.text", duration, color, position, text, distanceFade, zTest, scale, entityId, id);
	}

	public static string Line(Vector3 pos1, Vector3 pos2, float duration, Color color, bool distanceFade = true, bool zTest = false, NetworkableId entityId = default(NetworkableId), string id = null)
	{
		return ConsoleSystem.BuildCommand("ddraw.line", duration, color, pos1, pos2, distanceFade, zTest, entityId, id);
	}

	public static string Bounds(Bounds bounds, float duration, Color color, bool distanceFade = true, bool zTest = true, NetworkableId entityId = default(NetworkableId), string id = null)
	{
		return Box(bounds.center, duration, color, bounds.size, Quaternion.identity, distanceFade, zTest, entityId, id);
	}
}
