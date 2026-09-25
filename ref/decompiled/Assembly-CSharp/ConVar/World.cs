using System;
using System.IO;
using Facepunch;
using UnityEngine;

namespace ConVar;

[Factory("world")]
public class World : ConsoleSystem
{
	[ServerVar]
	[ClientVar(Help = "(Generated) When enabled, caches world data for faster loading; disabled by default in editor server builds to ensure fresh data during development")]
	public static bool cache = true;

	[ClientVar(Help = "(Generated) When enabled, world assets are streamed in and out based on proximity; disable to force all world data to stay loaded at once")]
	public static bool streaming = true;

	[ServerVar(Help = "(Generated) World generation config string passed directly to the procedural map generator; overrides the config file if set")]
	public static string configString = string.Empty;

	[ServerVar(Help = "(Generated) Path to a world generation config file used by the procedural map generator; used when configString is empty")]
	public static string configFile = string.Empty;

	[ClientVar(Help = "(Generated) Prints a table of all monuments on the current map including type, display name, prefab path, and world position; admin/developer only")]
	[ServerVar(Help = "(Generated) Prints a table of all monuments on the current map including type, display name, prefab path, and world position; admin/developer only")]
	public static void monuments(Arg arg)
	{
		if (!TerrainMeta.Path)
		{
			return;
		}
		using TextTable textTable = Facepunch.Pool.Get<TextTable>();
		textTable.AddColumn("type");
		textTable.AddColumn("name");
		textTable.AddColumn("prefab");
		textTable.AddColumn("pos");
		foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
		{
			textTable.AddRow(monument.Type.ToString(), monument.displayPhrase.translated, monument.name, monument.transform.position.ToString());
		}
		arg.ReplyWith(textTable.ToString());
	}

	[ServerVar(Clientside = true, Help = "Renders a high resolution PNG of the current map")]
	public static void rendermap(Arg arg)
	{
		float @float = arg.GetFloat(0, 1f);
		int imageWidth;
		int imageHeight;
		Color background;
		byte[] array = MapImageRenderer.Render(out imageWidth, out imageHeight, out background, @float, lossy: false);
		if (array == null)
		{
			arg.ReplyWith("Failed to render the map (is a map loaded now?)");
			return;
		}
		string fullPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, $"map_{global::World.Size}_{global::World.Seed}.png"));
		File.WriteAllBytes(fullPath, array);
		arg.ReplyWith("Saved map render to: " + fullPath);
	}

	[ServerVar(Clientside = true, Help = "Renders a PNG of the current map's tunnel network")]
	public static void rendertunnels(Arg arg)
	{
		RenderMapLayerToFile(arg, "tunnels", MapLayer.TrainTunnels);
	}

	[ServerVar(Clientside = true, Help = "Renders a PNG of the current map's underwater labs, for a specific floor")]
	public static void renderlabs(Arg arg)
	{
		int underwaterLabFloorCount = MapLayerRenderer.GetOrCreate().GetUnderwaterLabFloorCount();
		int @int = arg.GetInt(0);
		if (@int < 0 || @int >= underwaterLabFloorCount)
		{
			arg.ReplyWith($"Floor number must be between 0 and {underwaterLabFloorCount}");
		}
		else
		{
			RenderMapLayerToFile(arg, $"labs_{@int}", (MapLayer)(1 + @int));
		}
	}

	private static void RenderMapLayerToFile(Arg arg, string name, MapLayer layer)
	{
		try
		{
			MapLayerRenderer orCreate = MapLayerRenderer.GetOrCreate();
			orCreate.Render(layer);
			string fullPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, $"{name}_{global::World.Size}_{global::World.Seed}.png"));
			RenderTexture targetTexture = orCreate.renderCamera.targetTexture;
			Texture2D texture2D = new Texture2D(targetTexture.width, targetTexture.height);
			RenderTexture active = RenderTexture.active;
			try
			{
				RenderTexture.active = targetTexture;
				texture2D.ReadPixels(new Rect(0f, 0f, targetTexture.width, targetTexture.height), 0, 0);
				texture2D.Apply();
				File.WriteAllBytes(fullPath, texture2D.EncodeToPNG());
			}
			finally
			{
				RenderTexture.active = active;
				UnityEngine.Object.DestroyImmediate(texture2D);
			}
			arg.ReplyWith("Saved " + name + " render to: " + fullPath);
		}
		catch (Exception message)
		{
			Debug.LogWarning(message);
			arg.ReplyWith("Failed to render " + name);
		}
	}

	[ServerVar(Help = "(Generated) Draws flat wireframe boxes in the world showing world bounds (red), terrain margin (yellow), deep sea bounds (cyan), and portal bounds (green/magenta) for the given duration in seconds")]
	public static void drawbounds(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null)
		{
			return;
		}
		float @float = arg.GetFloat(0, 60f);
		float y2 = 1f;
		if (SingletonComponent<ValidBounds>.Instance != null)
		{
			DrawBoundsFlat(basePlayer, SingletonComponent<ValidBounds>.Instance.worldBounds, y2, @float, Color.red, "WorldBounds");
		}
		if ((bool)TerrainMeta.TerrainRenderer)
		{
			float x = TerrainMeta.Position.x - TerrainMeta.Size.x;
			float x2 = TerrainMeta.Position.x + TerrainMeta.Size.x + TerrainMeta.Size.x;
			float z = TerrainMeta.Position.z - TerrainMeta.Size.z;
			float z2 = TerrainMeta.Position.z + TerrainMeta.Size.z + TerrainMeta.Size.z;
			Bounds bounds2 = default(Bounds);
			bounds2.SetMinMax(new Vector3(x, 0f, z), new Vector3(x2, 0f, z2));
			DrawBoundsFlat(basePlayer, bounds2, y2, @float, Color.yellow, "TerrainMargin");
		}
		DrawBoundsFlat(basePlayer, DeepSeaManager.DeepSeaBounds, y2, @float, Color.cyan, "DeepSea");
		if (!DeepSea.enabled || !(DeepSeaManager.Get(server: true) != null))
		{
			return;
		}
		foreach (DeepSeaPortal serverPortal in DeepSeaManager.ServerPortals)
		{
			bool flag = serverPortal.PortalMode == DeepSeaPortal.PortalModeEnum.Entrance;
			Color color2 = (flag ? Color.green : Color.magenta);
			color2 = (serverPortal.IsOpen() ? color2 : color2.WithAlpha(0.1f));
			DrawBoundsFlat(basePlayer, serverPortal.WorldSpaceBounds().ToBounds(), y2, @float, color2, flag ? "Entrance Portal" : "Exit Portal");
		}
		static void DrawBoundsFlat(BasePlayer player, Bounds bounds, float y, float duration, Color color, string label)
		{
			Vector3 vector = new Vector3(bounds.min.x, y, bounds.min.z);
			Vector3 vector2 = new Vector3(bounds.max.x, y, bounds.min.z);
			Vector3 vector3 = new Vector3(bounds.max.x, y, bounds.max.z);
			Vector3 vector4 = new Vector3(bounds.min.x, y, bounds.max.z);
			UnityEngine.DDraw.Line(player, vector, vector2, color, duration, distanceFade: false);
			UnityEngine.DDraw.Line(player, vector2, vector3, color, duration, distanceFade: false);
			UnityEngine.DDraw.Line(player, vector3, vector4, color, duration, distanceFade: false);
			UnityEngine.DDraw.Line(player, vector4, vector, color, duration, distanceFade: false);
			UnityEngine.DDraw.Text(player, vector, label, color, duration, distanceFade: true, zTest: false, 1f);
			UnityEngine.DDraw.Text(player, vector2, label, color, duration, distanceFade: true, zTest: false, 1f);
			UnityEngine.DDraw.Text(player, vector3, label, color, duration, distanceFade: true, zTest: false, 1f);
			UnityEngine.DDraw.Text(player, vector4, label, color, duration, distanceFade: true, zTest: false, 1f);
		}
	}
}
