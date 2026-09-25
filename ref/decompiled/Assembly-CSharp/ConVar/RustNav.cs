using Rust.Ai.Gen2.Nav;
using UnityEngine;

namespace ConVar;

[Factory("rustnav")]
public class RustNav : ConsoleSystem
{
	[ServerVar]
	public static float drawRadius = 100f;

	[ServerVar]
	public static float drawRefreshRate = 0.25f;

	[ServerVar(Help = "Max KB per second of navmesh tiles streamed to each rustnav.draw viewer")]
	public static float drawKBps = 512f;

	[ServerVar]
	public static int drawManifestInterval = 10;

	private const float traceLength = 500f;

	[ServerVar(Help = "Navmesh tile builder worker thread count. Applied when the builder is (re)created - at boot, or via rustnav.setnumthreads at runtime. ~half the cores is the sweet spot for full-map bakes")]
	public static int numThreads = 4;

	[ServerVar(Help = "Main-thread time budget per frame (ms) for navmesh tile geometry collection. The full-map bake wall clock is roughly tiles / (budget-worth of tiles per frame) / fps, so raise this while baking to trade frame time for bake speed")]
	public static float collectBudgetMs = 1f;

	[ServerVar(Help = "Metres of open water past which default navmesh tiles are dropped instead of baked. A tile only drops when every point in it is that far from land, so shores keep their navmesh and so do rivers and lakes, which are never that wide. Independent navmeshes (oilrigs, tropical islands, ghost ships) are never touched. 0 or less bakes the open sea like before. Applies to tiles queued after the change")]
	public static float maxShoreDistance = 100f;

	[ServerVar(Help = "Detail mesh sample distance as a multiple of cellSize (Recast default 6). Larger = cheaper detail mesh, coarser surface height. Applies to tiles built after the change")]
	public static float detailSampleDistMult = 12f;

	[ServerVar(Help = "Detail mesh max sample error as a multiple of cellHeight (Recast default 1). Larger = cheaper detail mesh, more height error. Applies to tiles built after the change")]
	public static float detailSampleMaxErrorMult = 1f;

	[ServerVar]
	public static bool enableVerboseLogs = false;

	[ServerVar(Help = "LZ4-compress navmesh saves (smaller .navmesh files, no extra save time)")]
	public static bool saveCompression = true;

	[ServerVar(Help = "Worker threads for navmesh save/load compression. 0 = auto")]
	public static int saveThreads = 0;

	[ServerVar(Help = "A/B kill switch: build navmesh tiles with the stock pre optimization algorithms (clip rasterizer, full ledge and region rescans, unfused filters). Combine with detailsampledistmult 6 to approximate the old build end to end. Applies to tiles built after the change")]
	public static bool legacyBuild = false;

	[ServerVar(Help = "Collect navmesh bake statistics (rustnav.bakestats to read them). Defaults on in the editor, off on servers; flip on live to diagnose bake issues, then reset with rustnav.bakestats true")]
	public static bool bakeStatsEnabled = Application.isEditor;

	[ServerVar]
	public static void Draw(Arg arg)
	{
		if (AI.useUnityNavmesh)
		{
			arg.ReplyWith("Restart the server without command line argument -useOldNavmesh");
			return;
		}
		if (RustNavigation.Instance == null || RustNavigation.Instance.DefaultNavmesh == null || !RustNavigation.Instance.DefaultNavmesh.IsValid())
		{
			arg.ReplyWith("Navmesh is not initialized");
			return;
		}
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (arg.GetBool(0, !basePlayer.IsInvoking(basePlayer.DrawNavmesh)))
		{
			basePlayer.ResetNavmeshDrawState();
			RustNavigation.AddDrawViewer(basePlayer);
			basePlayer.InvokeRepeating(basePlayer.DrawNavmesh, 0f, drawRefreshRate);
		}
		else
		{
			basePlayer.CancelInvoke(basePlayer.DrawNavmesh);
			RustNavigation.RemoveDrawViewer(basePlayer);
			basePlayer.ResetNavmeshDrawState();
			basePlayer.ClientRPC(RpcTarget.Player("StopDrawingNavmesh", basePlayer));
		}
	}

	[ServerVar]
	public static void SetNumThreads(Arg arg)
	{
		if (!RustNavigation.EnsureNewNavmesh())
		{
			arg.ReplyWith("Restart the server without command line argument -useOldNavmesh");
		}
		else if (!(RustNavigation.Instance == null))
		{
			int @int = arg.GetInt(0, numThreads);
			if (@int == numThreads)
			{
				arg.ReplyWith($"RustNav.numThreads is already set to {numThreads}");
				return;
			}
			numThreads = @int;
			RustNavigation.Instance.ResetTileBuilder();
			arg.ReplyWith($"Set RustNav.numThreads to {numThreads}");
		}
	}

	[ServerVar(Help = "Print accumulated navmesh bake statistics. Pass true to reset them instead")]
	public static void BakeStats(Arg arg)
	{
		if (!RustNavigation.EnsureNewNavmesh())
		{
			arg.ReplyWith("Restart the server without command line argument -useOldNavmesh");
		}
		else if (arg.GetBool(0))
		{
			Rust.Ai.Gen2.Nav.BakeStats.Reset();
			arg.ReplyWith("Bake stats reset");
		}
		else if (!bakeStatsEnabled)
		{
			arg.ReplyWith("Bake stat collection is off (rustnav.bakestatsenabled false) - nothing recorded");
		}
		else
		{
			arg.ReplyWith(Rust.Ai.Gen2.Nav.BakeStats.BuildReport());
		}
	}

	[ServerVar(Help = "List every navmesh (default + independent monuments/islands/ghostships) ranked by accumulated worker build time")]
	public static void NavmeshStats(Arg arg)
	{
		if (!RustNavigation.EnsureNewNavmesh())
		{
			arg.ReplyWith("Restart the server without command line argument -useOldNavmesh");
		}
		else
		{
			arg.ReplyWith(RustNavigation.Instance.ReportNavmeshStats());
		}
	}

	[ServerVar(Help = "Rebuild one default-navmesh tile synchronously and dump its assembled geometry + heightfield bounds to a file for the offline native-build harness: rustnav.dumptilegeo <tx> <ty> <path>")]
	public static void DumpTileGeo(Arg arg)
	{
		if (!RustNavigation.EnsureNewNavmesh())
		{
			arg.ReplyWith("Restart the server without command line argument -useOldNavmesh");
			return;
		}
		RustNavmesh defaultNavmesh = RustNavigation.Instance.DefaultNavmesh;
		if (defaultNavmesh == null || !defaultNavmesh.IsValid())
		{
			arg.ReplyWith("No default navmesh");
			return;
		}
		int @int = arg.GetInt(0, -1);
		int int2 = arg.GetInt(1, -1);
		string @string = arg.GetString(2);
		if (@int < 0 || int2 < 0 || string.IsNullOrEmpty(@string))
		{
			arg.ReplyWith("Usage: rustnav.dumptilegeo <tx> <ty> <path>");
			return;
		}
		BackgroundTileBuilder.DumpGeometryRequest = (@int, int2, @string);
		RustNavigation.Instance.RebuildTileSynchronous(defaultNavmesh, @int, int2);
		if (BackgroundTileBuilder.DumpGeometryRequest.HasValue)
		{
			BackgroundTileBuilder.DumpGeometryRequest = null;
			arg.ReplyWith($"Tile {@int},{int2} did not reach the heightfield stage (no geometry?), nothing dumped");
		}
		else
		{
			arg.ReplyWith($"Dumped tile {@int},{int2} geometry to {@string}");
		}
	}

	[ServerVar]
	public static void Rebuild(Arg arg)
	{
		if (!RustNavigation.EnsureNewNavmesh())
		{
			arg.ReplyWith("Restart the server without command line argument -useOldNavmesh");
			return;
		}
		bool @bool = arg.GetBool(0);
		RustNavigation.Instance.RebuildDefaultNavmesh(@bool);
	}

	private static RustNavmesh FindNavmeshAtPosition(Vector3 position, out IndependantNavmesh independantNavmesh)
	{
		independantNavmesh = null;
		if (!RustNavigation.EnsureNewNavmesh())
		{
			return null;
		}
		IndependantNavmesh independantNavmesh2 = IndependantNavmesh.FindNavmeshAtPosition(position);
		if (independantNavmesh2 == null || independantNavmesh2.Navmesh == null || !independantNavmesh2.Navmesh.IsValid())
		{
			return RustNavigation.Instance.DefaultNavmesh;
		}
		independantNavmesh = independantNavmesh2;
		return independantNavmesh2.Navmesh;
	}

	private static NavVector3 DebugWorldToNav(IndependantNavmesh independantNavmesh, Vector3 positionWS)
	{
		if (!(independantNavmesh != null))
		{
			return new NavVector3(positionWS);
		}
		return independantNavmesh.TransformPointFromWorldSpaceToNavSpace(positionWS);
	}

	private static Vector3 DebugNavToWorld(IndependantNavmesh independantNavmesh, NavVector3 positionNS)
	{
		if (!(independantNavmesh != null))
		{
			return positionNS.Value;
		}
		return independantNavmesh.TransformPointFromNavSpaceToWorldSpace(positionNS);
	}

	private static Vector3 DebugNavToWorldDirection(IndependantNavmesh independantNavmesh, NavVector3 directionNS)
	{
		if (!(independantNavmesh != null))
		{
			return directionNS.Value;
		}
		return independantNavmesh.TransformDirectionFromNavSpaceToWorldSpace(directionNS);
	}

	[ServerVar]
	public static void DebugPath(Arg arg)
	{
	}

	[ServerVar]
	public static void DebugRaycast(Arg arg)
	{
	}

	[ServerVar]
	public static void DebugSample(Arg arg)
	{
	}

	[ServerVar]
	public static void DebugRebuildTile(Arg arg)
	{
	}

	[ServerVar]
	public static void DebugSave(Arg arg)
	{
	}

	[ServerVar]
	public static void DebugLoad(Arg arg)
	{
	}
}
