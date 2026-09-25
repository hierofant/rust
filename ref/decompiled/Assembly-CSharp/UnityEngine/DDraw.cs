namespace UnityEngine;

public class DDraw : MonoBehaviour
{
	public GUISkin skin;

	public static void BroadcastArrow(Vector3 start, Vector3 end, Color color, float duration = 10f, float headSize = 0.5f, bool distanceFade = true, bool zTest = true)
	{
		foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
		{
			if (!(activePlayer == null) && activePlayer.IsConnected)
			{
				activePlayer.SendConsoleCommand("ddraw.arrow", duration, color, start, end, headSize, distanceFade, zTest);
			}
		}
	}

	public static void BroadcastLine(Vector3 start, Vector3 end, Color color, float duration = 10f, bool distanceFade = true, bool zTest = true)
	{
		foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
		{
			if (!(activePlayer == null) && activePlayer.IsConnected)
			{
				activePlayer.SendConsoleCommand("ddraw.line", duration, color, start, end, distanceFade, zTest);
			}
		}
	}

	public static void BroadcastSphere(Vector3 pos, float radius, Color color, float duration = 10f, bool distanceFade = true, bool zTest = true)
	{
		foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
		{
			if (!(activePlayer == null) && activePlayer.IsConnected)
			{
				activePlayer.SendConsoleCommand("ddraw.sphere", duration, color, pos, radius, distanceFade, zTest);
			}
		}
	}

	public static void BroadcastText(Vector3 pos, string text, Color color, float duration = 10f, bool distanceFade = true, bool zTest = false)
	{
		foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
		{
			if (!(activePlayer == null) && activePlayer.IsConnected)
			{
				activePlayer.SendConsoleCommand("ddraw.text", duration, color, pos, text, distanceFade, zTest);
			}
		}
	}

	public static void BroadcastCapsule(Vector3 pos, Vector3 rot, float radius, float height, Color color, float duration = 10f, bool distanceFade = true, bool zTest = true)
	{
		foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
		{
			if (!(activePlayer == null) && activePlayer.IsConnected)
			{
				activePlayer.SendConsoleCommand("ddraw.capsule", duration, color, pos, rot, radius, height, distanceFade, zTest);
			}
		}
	}

	public static void BroadcastBox(Vector3 pos, Vector3 size, Vector3 rot, Color color, float duration = 10f, bool distanceFade = true, bool zTest = true)
	{
		foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
		{
			if (!(activePlayer == null) && activePlayer.IsConnected)
			{
				activePlayer.SendConsoleCommand("ddraw.box", duration, color, pos, $"{size.x} {size.y} {size.z}", rot, distanceFade, zTest);
			}
		}
	}

	public static void BroadcastBounds(Bounds bounds, Color color, float duration = 10f, bool distanceFade = true, bool zTest = true)
	{
		BroadcastBox(bounds.center, bounds.size, Vector3.zero, color, duration, distanceFade, zTest);
	}

	public static void BroadcastClear()
	{
		foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
		{
			if (!(activePlayer == null) && activePlayer.IsConnected)
			{
				activePlayer.SendConsoleCommand("ddraw.clear");
			}
		}
	}

	public static void Arrow(BasePlayer player, Vector3 start, Vector3 end, Color color, float duration = 10f, float headSize = 0.5f, bool distanceFade = true, bool zTest = true)
	{
		if (!(player == null) && player.IsConnected)
		{
			player.SendConsoleCommand("ddraw.arrow", duration, color, start, end, headSize, distanceFade, zTest);
		}
	}

	public static void Line(BasePlayer player, Vector3 start, Vector3 end, Color color, float duration = 10f, bool distanceFade = true, bool zTest = true)
	{
		if (!(player == null) && player.IsConnected)
		{
			player.SendConsoleCommand("ddraw.line", duration, color, start, end, distanceFade, zTest);
		}
	}

	public static void Sphere(BasePlayer player, Vector3 pos, float radius, Color color, float duration = 10f, bool distanceFade = true, bool zTest = true)
	{
		if (!(player == null) && player.IsConnected)
		{
			player.SendConsoleCommand("ddraw.sphere", duration, color, pos, radius, distanceFade, zTest);
		}
	}

	public static void Text(BasePlayer player, Vector3 pos, string text, Color color, float duration = 10f, bool distanceFade = true, bool zTest = false, float scale = 2f)
	{
		if (!(player == null) && player.IsConnected)
		{
			player.SendConsoleCommand("ddraw.text", duration, color, pos, text, distanceFade, zTest, scale);
		}
	}

	public static void Capsule(BasePlayer player, Vector3 pos, Vector3 rot, float radius, float height, Color color, float duration = 10f, bool distanceFade = true, bool zTest = true)
	{
		if (!(player == null) && player.IsConnected)
		{
			player.SendConsoleCommand("ddraw.capsule", duration, color, pos, rot, radius, height, distanceFade, zTest);
		}
	}

	public static void Bounds(BasePlayer player, Bounds bounds, Color color, float duration = 10f, bool distanceFade = true, bool zTest = true)
	{
		Box(player, bounds.center, bounds.size, Vector3.zero, color, duration, distanceFade, zTest);
	}

	public static void Box(BasePlayer player, Vector3 pos, Vector3 size, Vector3 rot, Color color, float duration = 10f, bool distanceFade = true, bool zTest = true)
	{
		if (!(player == null) && player.IsConnected)
		{
			player.SendConsoleCommand("ddraw.box", duration, color, pos, $"{size.x} {size.y} {size.z}", rot, distanceFade, zTest);
		}
	}

	public static void Clear(BasePlayer player)
	{
		if (!(player == null) && player.IsConnected)
		{
			player.SendConsoleCommand("ddraw.clear");
		}
	}
}
