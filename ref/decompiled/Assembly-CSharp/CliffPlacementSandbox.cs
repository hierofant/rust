using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using ProtoBuf;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
public class CliffPlacementSandbox : MonoBehaviour
{
	public enum TerrainPatch
	{
		Flat,
		SlopeX,
		Ridge,
		ConvexDome,
		ConcaveBowl
	}

	public enum TerrainSource
	{
		ProceduralReal,
		CannedPatch,
		MapFileRegion
	}

	private Material _gizmoMat;

	private readonly List<TerrainAnchor> _gizmoAnchors = new List<TerrainAnchor>();

	private readonly List<TerrainModifier> _gizmoModifiers = new List<TerrainModifier>();

	private readonly List<TerrainFootprint> _gizmoFootprints = new List<TerrainFootprint>();

	private int _gizmoTargetsFrame = -1;

	private readonly HashSet<Transform> _selectedGizmoRoots = new HashSet<Transform>();

	private static readonly Color AnchorColor = new Color(0.2f, 0.9f, 1f, 1f);

	private static readonly Color HeightSetColor = new Color(0.3f, 1f, 0.4f, 1f);

	private static readonly Color HeightRaiseColor = new Color(1f, 0.85f, 0.2f, 1f);

	private static readonly Color HeightAddColor = new Color(1f, 0.4f, 0.9f, 1f);

	private static readonly Color OtherModColor = new Color(0.8f, 0.8f, 0.8f, 1f);

	private static readonly Color FootprintColor = new Color(0.2f, 0.8f, 0.8f, 1f);

	private static readonly Color FootprintBandColor = new Color(0.2f, 0.8f, 0.8f, 0.35f);

	private static readonly Color FootprintSeatedColor = new Color(0.3f, 1f, 0.5f, 1f);

	private static readonly Color FootprintGapColor = new Color(1f, 0.35f, 0.2f, 1f);

	private static readonly Color FootprintInactiveColor = new Color(0.55f, 0.55f, 0.6f, 1f);

	[Tooltip("Paste the whole thing a server's `levelurl` prints, then press Download below. The file is cached next to the project and MapFilePath is pointed at it, ready to Initialize.")]
	[Header("Level URL download")]
	public string LevelUrl = string.Empty;

	[Tooltip("Where downloaded maps are cached. A relative path is taken from the project root, i.e. alongside Assets rather than inside it, so Unity never tries to import them.")]
	public string MapCacheFolder = "SandboxMaps";

	private WorldSerialization _mapSerialization;

	private short[] _mapHeights;

	private int _mapRes;

	private Vector3 _mapWorldPos;

	private Vector3 _mapWorldSize;

	private short[] _preCliffHeights;

	private int _preCliffRes;

	private bool _preCliffLoaded;

	private float[] _bakedRegion;

	private string _preCliffStatus = "pre-cliff baseline: not loaded";

	[Header("Terrain source")]
	[Tooltip("ProceduralReal runs the game's real base heightmap generator (GenerateHeight). CannedPatch uses a simple analytic patch.")]
	public TerrainSource Source;

	[Tooltip("World seed fed to the real generator. Also supplies the seed for the pre-cliff T0 bake in Map File Region mode when the map came from a levelurl - uploaded maps have the seed stripped out of their name, so run `seed` on the server and paste the value here. A map taken from the server's own root folder carries it in the name and overwrites this field on load. 0 = auto.")]
	[Header("Procedural (real Rust base heightmap)")]
	public uint Seed = 54321u;

	[Tooltip("Square map size in metres. Real maps are thousands; smaller = quicker but less varied.")]
	public float ProceduralMapSize = 2000f;

	[Tooltip("Vertical height range in metres (terrain Size.y).")]
	public float ProceduralHeightRange = 1000f;

	[Tooltip("Auto-slope finder: acceptable terrain steepness (degrees) for dropping the cliff.")]
	public int SlopeFinderMinAngle = 30;

	public int SlopeFinderMaxAngle = 65;

	[Tooltip("Unity heightmap resolution. Snapped to the nearest 2^n+1 by Unity.")]
	[Header("Terrain")]
	public int HeightmapResolution = 513;

	[Tooltip("World-space size of the sandbox terrain (x/z = extent, y = height range).")]
	public Vector3 TerrainSize = new Vector3(500f, 100f, 500f);

	[Tooltip("World-space origin (bottom-south-west corner) of the sandbox terrain.")]
	public Vector3 TerrainOrigin = Vector3.zero;

	[Tooltip("Which canned height patch to seed the terrain with.")]
	public TerrainPatch CurrentPatch = TerrainPatch.SlopeX;

	[Tooltip("Path to a real, shipped .map file. Use the inspector's drag/drop or picker to set it.")]
	[Header("Map file region (real .map crop)")]
	public string MapFilePath = string.Empty;

	[Tooltip("World-space X/Z centre of the region to crop out of the map (Y is ignored).")]
	public Vector3 RegionCenter = Vector3.zero;

	[Tooltip("Side length in metres of the square region cropped from the map.")]
	public float RegionSize = 300f;

	[Tooltip("World-Y that normalized height 0 maps to. Rust map heights are sea-level centred, so the terrain sits at y = -500 with a 1000m range (matches the shipped map loader). Nudge this if the terrain sits above/below the spawned cliffs.")]
	public float MapWorldYOffset = -500f;

	[Tooltip("Auto-pick the region resolution to match the source map's per-cell detail over the cropped window. Turn off to use MapRegionResolution directly.")]
	public bool AutoMapRegionResolution = true;

	[Tooltip("Heightmap resolution of the cropped sandbox terrain (snapped to 2^n+1). Used when AutoMapRegionResolution is off; otherwise shows the last auto-computed value.")]
	public int MapRegionResolution = 513;

	[Tooltip("Spawn the real cliff prefabs that the map placed inside the region (kept linked to their prefab assets, so editing the prefab and recalculating shows the effect).")]
	public bool SpawnRealCliffs = true;

	[Tooltip("Only spawn decor prefabs whose asset path looks like a cliff/rock, instead of all decor in the region.")]
	public bool CliffPrefabsOnly = true;

	[Tooltip("Use the cached pre-cliff terrain (T0) as the recalc baseline instead of the baked map terrain. T0 is captured once via 'Tools > Cliff Sandbox > Arm Pre-Cliff Terrain Capture' during a real generation of this map's seed/size. When off (or no cache), recalc falls back to the baked map terrain, which can produce spurious gaps.")]
	public bool UsePreCliffBaseline = true;

	[Tooltip("Procedural-generation scene the one-click 'Bake pre-cliff T0' button drives to capture T0. Must be a full generator scene (engine bootstrap + generating World Setup), e.g. the shipped 'Procedural Map' scene. Only used by the editor bake button.")]
	public string GenerationScenePath = "Assets/Scenes/Release/Procedural Map.unity";

	[Tooltip("The cliff prefab instance to place. Assign via the inspector dropdown or drag one in.")]
	[Header("Placement")]
	public Transform cliffRoot;

	[Tooltip("Anchor solve mode. PlaceCliffs uses MaximizeHeight for the first cliff of a chain.")]
	public TerrainAnchorMode AnchorMode = TerrainAnchorMode.MaximizeHeight;

	[Tooltip("Snap the cliff root to the anchored Y after placing, so it visually follows the solve.")]
	public bool SnapCliffToAnchoredHeight = true;

	[Tooltip("When recalculating in Map File Region mode, re-apply each cliff's terrain modifiers at its real recorded map position instead of re-solving its anchors and moving it. This makes the recalc a faithful preview of the generated terrain around the cliff (anchors are still reported accepted/rejected for info). Turn off to also re-solve and move cliffs.")]
	public bool RecalcKeepMapPositions = true;

	[Tooltip("Hot-reload: before Carve selected / Re-anchor selected run, re-spawn the selected cliff(s) from their source prefab assets so edits made in Prefab Mode (or the Project window) apply immediately - no need to exit and re-enter play mode. Spawned cliffs are plain clones not linked to the asset, so without this Carve/Re-anchor keep using the stale, pre-edit clone. Turn off to act on the exact instances in the scene (e.g. after hand-moving them).")]
	public bool HotReloadPrefabsBeforeAction = true;

	private int _nextSandboxCliffId = 1;

	[Tooltip("Replay each cliff's TerrainPlacement heightmap stamps during recalc, in addition to its TerrainModifiers. Rocks/formations flatten and blend the terrain under themselves with these stamps; the generator applies them before later pieces solve their anchors. Without replaying them the re-solve samples rougher, un-flattened terrain and rejects anchors the real map accepted. Turn off to replay only the height modifiers (the older behaviour).")]
	public bool ReplayTerrainPlacementsOnRecalc = true;

	[Tooltip("Measure and fill each cliff's TerrainFootprint during recalc, the way the generator does just before the prefab is added. The gap readout is always reported; turn this off to see what the terrain looks like without the fill while still being told how deep the gap is.")]
	public bool ApplyTerrainFootprintOnRecalc = true;

	[Header("Placement gizmos (play mode)")]
	[Tooltip("Draw TerrainAnchor / TerrainModifier gizmos in the Game view while playing (the built-in gizmos only show in the Scene view and are disabled in play mode).")]
	public bool ShowPlacementGizmos = true;

	[Tooltip("Include TerrainAnchor gizmos (vertical solve range + radius).")]
	public bool GizmoAnchors = true;

	[Tooltip("Include TerrainHeightSet modifier gizmos (radius ring).")]
	public bool GizmoModifierHeightSet = true;

	[Tooltip("Include TerrainHeightRaise modifier gizmos (radius ring).")]
	public bool GizmoModifierHeightRaise = true;

	[Tooltip("Include TerrainHeightAdd modifier gizmos (radius ring).")]
	public bool GizmoModifierHeightAdd = true;

	[Tooltip("Include any other (non-height) TerrainModifier gizmos (radius ring).")]
	public bool GizmoModifierOther = true;

	[Tooltip("Include TerrainFootprint gizmos: the captured base ring, its rim band, and a vertical marker at each ring point showing whether the terrain there is seated or has fallen away. Red markers are the gaps the anchors could not see.")]
	public bool GizmoFootprint = true;

	[Tooltip("Only draw placement gizmos within this many metres of the camera (0 = no limit). Keeps dense real-map regions readable by hiding far-away gizmos.")]
	public float GizmoDrawDistance = 60f;

	[Tooltip("Click-to-select mode: instead of drawing every gizmo in range, left-click a cliff to toggle its gizmos on, click again to turn them off. Several cliffs can be selected at once.")]
	public bool GizmoSelectionMode = true;

	[Header("Scale reference")]
	[Tooltip("Spawn a bright, roughly player-sized capsule on the terrain so you can judge scale against the cliffs. Use 'Marker where I'm looking' or the J key to drop it under the aim.")]
	public bool ShowPlayerScaleReference = true;

	[Tooltip("Height of the scale-reference capsule in metres (Rust player is about 1.8m).")]
	public float PlayerReferenceHeight = 1.8f;

	[Header("Camera (play-mode freecam)")]
	[Tooltip("Hold right-mouse in Game view to fly: WASD move, Q/E down/up, Shift sprint, scroll = speed.")]
	public bool EnableFreecam = true;

	[Tooltip("Base freecam move speed in metres/second (adjust live with the scroll wheel while flying).")]
	public float FreecamMoveSpeed = 400f;

	[Tooltip("Speed multiplier while holding Shift.")]
	public float FreecamSprintMultiplier = 5f;

	[Tooltip("Mouse-look sensitivity (degrees per pixel of mouse delta).")]
	public float FreecamLookSensitivity = 0.1f;

	[Tooltip("On Initialize, move the main camera to a vantage overlooking the terrain centre.")]
	public bool MoveCameraOnInitialize = true;

	[Tooltip("After Place / Auto-place / Recalculate, fly the camera over to frame the cliff.")]
	public bool FrameCameraOnPlacedCliff = true;

	[Header("Hotkeys / UI")]
	public bool DrawOnScreenControls = true;

	private GameObject _terrainGO;

	private TerrainData _terrainData;

	private TerrainMeta _meta;

	private TerrainHeightMap _heightmap;

	private int _res;

	private float[] _baseline;

	private bool _initialized;

	private float[] _preCarve;

	private string _preCarveSelectionKey;

	private string _lastPlaceInfo = "-";

	private string _lastAnchorBreakdown = string.Empty;

	private readonly List<GameObject> _spawnedCliffs = new List<GameObject>();

	private bool _freecamActive;

	private float _freecamYaw;

	private float _freecamPitch;

	private GameObject _playerScaleRef;

	private int SelectedGizmoCount
	{
		get
		{
			_selectedGizmoRoots.RemoveWhere((Transform r) => r == null);
			return _selectedGizmoRoots.Count;
		}
	}

	public string PreCliffStatus => _preCliffStatus;

	public bool PreCliffLoaded => _preCliffLoaded;

	private void FrameCameraOnCliff(Transform root)
	{
		Camera main = Camera.main;
		if (!(main == null) && !(root == null))
		{
			Bounds bounds = CalcHierarchyBounds(root);
			float num = Mathf.Max(bounds.extents.magnitude, 5f) * 3f;
			Vector3 normalized = (Quaternion.Euler(30f, -45f, 0f) * Vector3.forward).normalized;
			main.transform.position = bounds.center - normalized * num;
			main.transform.rotation = Quaternion.LookRotation(normalized, Vector3.up);
			if (main.farClipPlane < num * 4f)
			{
				main.farClipPlane = num * 4f;
			}
		}
	}

	private static Bounds CalcHierarchyBounds(Transform root)
	{
		Renderer[] componentsInChildren = root.GetComponentsInChildren<Renderer>();
		if (componentsInChildren.Length == 0)
		{
			return new Bounds(root.position, Vector3.one * 5f);
		}
		Bounds bounds = componentsInChildren[0].bounds;
		for (int i = 1; i < componentsInChildren.Length; i++)
		{
			bounds.Encapsulate(componentsInChildren[i].bounds);
		}
		return bounds;
	}

	private void EnsureGizmoMaterial()
	{
		if (!(_gizmoMat != null))
		{
			Shader shader = Shader.Find("Hidden/Internal-Colored");
			_gizmoMat = new Material(shader)
			{
				hideFlags = HideFlags.HideAndDontSave
			};
			_gizmoMat.SetInt("_SrcBlend", 5);
			_gizmoMat.SetInt("_DstBlend", 10);
			_gizmoMat.SetInt("_Cull", 0);
			_gizmoMat.SetInt("_ZWrite", 0);
			_gizmoMat.SetInt("_ZTest", 8);
		}
	}

	private void RefreshGizmoTargets()
	{
		if (_gizmoTargetsFrame == Time.frameCount)
		{
			return;
		}
		_gizmoTargetsFrame = Time.frameCount;
		_gizmoAnchors.Clear();
		_gizmoModifiers.Clear();
		_gizmoFootprints.Clear();
		if (GizmoSelectionMode)
		{
			RefreshGizmoTargetsFromSelection();
			return;
		}
		Camera main = Camera.main;
		bool flag = main != null && GizmoDrawDistance > 0f;
		Vector3 vector = ((main != null) ? main.transform.position : Vector3.zero);
		float num = GizmoDrawDistance * GizmoDrawDistance;
		if (GizmoAnchors)
		{
			TerrainAnchor[] array = UnityEngine.Object.FindObjectsByType<TerrainAnchor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
			foreach (TerrainAnchor terrainAnchor in array)
			{
				if ((!(_terrainGO != null) || !terrainAnchor.transform.IsChildOf(_terrainGO.transform)) && (!flag || !((terrainAnchor.transform.position - vector).sqrMagnitude > num)))
				{
					_gizmoAnchors.Add(terrainAnchor);
				}
			}
		}
		if (AnyModifierGizmoEnabled())
		{
			TerrainModifier[] array2 = UnityEngine.Object.FindObjectsByType<TerrainModifier>(FindObjectsInactive.Include, FindObjectsSortMode.None);
			foreach (TerrainModifier terrainModifier in array2)
			{
				if ((!(_terrainGO != null) || !terrainModifier.transform.IsChildOf(_terrainGO.transform)) && IsModifierGizmoEnabled(terrainModifier) && (!flag || !((terrainModifier.transform.position - vector).sqrMagnitude > num)))
				{
					_gizmoModifiers.Add(terrainModifier);
				}
			}
		}
		if (!GizmoFootprint)
		{
			return;
		}
		TerrainFootprint[] array3 = UnityEngine.Object.FindObjectsByType<TerrainFootprint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		foreach (TerrainFootprint terrainFootprint in array3)
		{
			if ((!(_terrainGO != null) || !terrainFootprint.transform.IsChildOf(_terrainGO.transform)) && (!flag || !((terrainFootprint.transform.position - vector).sqrMagnitude > num)))
			{
				_gizmoFootprints.Add(terrainFootprint);
			}
		}
	}

	private void RefreshGizmoTargetsFromSelection()
	{
		_selectedGizmoRoots.RemoveWhere((Transform r) => r == null);
		foreach (Transform selectedGizmoRoot in _selectedGizmoRoots)
		{
			if (GizmoAnchors)
			{
				TerrainAnchor[] componentsInChildren = selectedGizmoRoot.GetComponentsInChildren<TerrainAnchor>(includeInactive: true);
				foreach (TerrainAnchor item in componentsInChildren)
				{
					_gizmoAnchors.Add(item);
				}
			}
			if (AnyModifierGizmoEnabled())
			{
				TerrainModifier[] componentsInChildren2 = selectedGizmoRoot.GetComponentsInChildren<TerrainModifier>(includeInactive: true);
				foreach (TerrainModifier terrainModifier in componentsInChildren2)
				{
					if (IsModifierGizmoEnabled(terrainModifier))
					{
						_gizmoModifiers.Add(terrainModifier);
					}
				}
			}
			if (GizmoFootprint)
			{
				TerrainFootprint[] componentsInChildren3 = selectedGizmoRoot.GetComponentsInChildren<TerrainFootprint>(includeInactive: true);
				foreach (TerrainFootprint item2 in componentsInChildren3)
				{
					_gizmoFootprints.Add(item2);
				}
			}
		}
	}

	private void ToggleGizmoSelection(Transform root)
	{
		if (!(root == null) && !_selectedGizmoRoots.Remove(root))
		{
			_selectedGizmoRoots.Add(root);
		}
	}

	private void ClearGizmoSelection()
	{
		_selectedGizmoRoots.Clear();
	}

	private bool AnyModifierGizmoEnabled()
	{
		if (!GizmoModifierHeightSet && !GizmoModifierHeightRaise && !GizmoModifierHeightAdd)
		{
			return GizmoModifierOther;
		}
		return true;
	}

	private bool IsModifierGizmoEnabled(TerrainModifier modifier)
	{
		if (modifier is TerrainHeightSet)
		{
			return GizmoModifierHeightSet;
		}
		if (modifier is TerrainHeightRaise)
		{
			return GizmoModifierHeightRaise;
		}
		if (modifier is TerrainHeightAdd)
		{
			return GizmoModifierHeightAdd;
		}
		return GizmoModifierOther;
	}

	private void OnRenderObject()
	{
		if (ShowPlacementGizmos && Application.isPlaying && (GizmoAnchors || AnyModifierGizmoEnabled() || GizmoFootprint))
		{
			RefreshGizmoTargets();
			EnsureGizmoMaterial();
			_gizmoMat.SetPass(0);
			GL.PushMatrix();
			GL.Begin(1);
			for (int i = 0; i < _gizmoAnchors.Count; i++)
			{
				DrawAnchorGizmo(_gizmoAnchors[i]);
			}
			for (int j = 0; j < _gizmoModifiers.Count; j++)
			{
				DrawModifierGizmo(_gizmoModifiers[j]);
			}
			for (int k = 0; k < _gizmoFootprints.Count; k++)
			{
				DrawFootprintGizmo(_gizmoFootprints[k]);
			}
			GL.End();
			GL.PopMatrix();
		}
	}

	private void DrawAnchorGizmo(TerrainAnchor anchor)
	{
		Vector3 position = anchor.transform.position;
		Vector3 lossyScale = anchor.transform.lossyScale;
		float num = 1f + anchor.SlopeScale * Mathf.InverseLerp(0f, 90f, Vector3.Angle(Vector3.up, anchor.transform.up));
		float num2 = anchor.Extents * lossyScale.y * num;
		float num3 = anchor.Offset * lossyScale.y * num;
		Vector3 vector = position + Vector3.up * (num3 - num2);
		Vector3 vector2 = position + Vector3.up * (num3 + num2);
		DrawLine(vector, vector2, AnchorColor);
		DrawLine(vector - Vector3.right * 0.5f, vector + Vector3.right * 0.5f, AnchorColor);
		DrawLine(vector2 - Vector3.right * 0.5f, vector2 + Vector3.right * 0.5f, AnchorColor);
		if (anchor.Radius > 0f)
		{
			DrawCircleY(position, anchor.Radius, AnchorColor);
		}
	}

	private void DrawModifierGizmo(TerrainModifier modifier)
	{
		float num = modifier.transform.lossyScale.y * modifier.Radius;
		if (!(num <= 0f))
		{
			DrawCircleY(modifier.transform.position, num, ModifierColor(modifier));
		}
	}

	private void DrawFootprintGizmo(TerrainFootprint footprint)
	{
		if (!footprint.HasRing)
		{
			return;
		}
		Transform transform = footprint.transform;
		Quaternion rotation = transform.rotation;
		bool flag = footprint.IsActive(rotation);
		Color color = (flag ? FootprintColor : FootprintInactiveColor);
		float fillOffset = footprint.FillOffset;
		Vector3 vector = transform.TransformPoint(footprint.Center);
		for (int i = 0; i < footprint.RunCount; i++)
		{
			footprint.GetRun(i, out var start, out var end, out var closed);
			int num = end - start;
			if (num < 2)
			{
				continue;
			}
			int num2 = (closed ? num : (num - 1));
			for (int j = 0; j < num2; j++)
			{
				Vector3 a = transform.TransformPoint(footprint.Ring[start + j]);
				Vector3 b = transform.TransformPoint(footprint.Ring[start + (j + 1) % num]);
				DrawLine(a, b, color);
			}
			for (int k = 0; k < num; k++)
			{
				Vector3 vector2 = transform.TransformPoint(footprint.Ring[start + k]);
				Vector3 vector3 = vector - vector2;
				vector3.y = 0f;
				vector3 = ((vector3.sqrMagnitude > 1E-06f) ? vector3.normalized : Vector3.zero);
				DrawLine(vector2 - vector3 * footprint.Feather, vector2 + vector3 * footprint.RimWidth, FootprintBandColor);
				if (flag && (bool)TerrainMeta.HeightMap)
				{
					float height = TerrainMeta.HeightMap.GetHeight(vector2);
					float num3 = vector2.y + fillOffset;
					DrawLine(new Vector3(vector2.x, height, vector2.z), new Vector3(vector2.x, num3, vector2.z), (height >= num3) ? FootprintSeatedColor : FootprintGapColor);
				}
			}
		}
	}

	private static Color ModifierColor(TerrainModifier modifier)
	{
		if (modifier is TerrainHeightSet)
		{
			return HeightSetColor;
		}
		if (modifier is TerrainHeightRaise)
		{
			return HeightRaiseColor;
		}
		if (modifier is TerrainHeightAdd)
		{
			return HeightAddColor;
		}
		return OtherModColor;
	}

	private static void DrawLine(Vector3 a, Vector3 b, Color color)
	{
		GL.Color(color);
		GL.Vertex(a);
		GL.Vertex(b);
	}

	private static void DrawCircleY(Vector3 center, float radius, Color color)
	{
		GL.Color(color);
		Vector3 v = center + new Vector3(radius, 0f, 0f);
		for (int i = 1; i <= 32; i++)
		{
			float f = (float)i / 32f * MathF.PI * 2f;
			Vector3 vector = center + new Vector3(Mathf.Cos(f) * radius, 0f, Mathf.Sin(f) * radius);
			GL.Vertex(v);
			GL.Vertex(vector);
			v = vector;
		}
	}

	private bool TryLoadMapForRegion(out string error)
	{
		error = null;
		if (string.IsNullOrEmpty(MapFilePath) || !File.Exists(MapFilePath))
		{
			error = "map file not found: '" + MapFilePath + "'. Set one in the inspector.";
			return false;
		}
		WorldSerialization worldSerialization = new WorldSerialization();
		try
		{
			worldSerialization.Load(MapFilePath);
		}
		catch (Exception ex)
		{
			error = "failed to read '" + MapFilePath + "': " + ex.Message;
			return false;
		}
		MapData map = worldSerialization.GetMap("terrain");
		if (map == null || map.data == null || map.data.Length == 0)
		{
			error = "the .map has no 'terrain' heightmap layer.";
			return false;
		}
		int num = Mathf.RoundToInt(Mathf.Sqrt((float)map.data.Length / 2f));
		if (num * num * 2 != map.data.Length)
		{
			error = $"unexpected terrain map size ({map.data.Length} bytes) - not a square short grid.";
			return false;
		}
		short[] array = new short[num * num];
		Buffer.BlockCopy(map.data, 0, array, 0, map.data.Length);
		_mapSerialization = worldSerialization;
		_mapHeights = array;
		_mapRes = num;
		float num2 = worldSerialization.world.size;
		_mapWorldPos = new Vector3((0f - num2) * 0.5f, MapWorldYOffset, (0f - num2) * 0.5f);
		_mapWorldSize = new Vector3(num2, 1000f, num2);
		return true;
	}

	private int ComputeMapRegionResolution()
	{
		float b = _mapWorldSize.x / (float)Mathf.Max(1, _mapRes - 1);
		int num = Mathf.CeilToInt(RegionSize / Mathf.Max(0.0001f, b)) + 1;
		int num2 = 33;
		while (num2 < num && num2 < 4097)
		{
			num2 = (num2 - 1) * 2 + 1;
		}
		return Mathf.Clamp(num2, 65, 2049);
	}

	private void FillFromMapRegion()
	{
		float num = Mathf.Max(1f, RegionSize) * 0.5f;
		float num2 = RegionCenter.x - num;
		float num3 = RegionCenter.z - num;
		float num4 = ((_res <= 1) ? 1 : (_res - 1));
		for (int i = 0; i < _res; i++)
		{
			float worldZ = num3 + (float)i / num4 * RegionSize;
			for (int j = 0; j < _res; j++)
			{
				float worldX = num2 + (float)j / num4 * RegionSize;
				_heightmap.SetHeight(j, i, SampleMapHeight01(worldX, worldZ));
			}
		}
	}

	private float SampleMapHeight01(float worldX, float worldZ)
	{
		float num = Mathf.Clamp01((worldX - _mapWorldPos.x) / _mapWorldSize.x);
		float num2 = Mathf.Clamp01((worldZ - _mapWorldPos.z) / _mapWorldSize.z);
		float num3 = num * (float)(_mapRes - 1);
		float num4 = num2 * (float)(_mapRes - 1);
		int num5 = Mathf.Clamp((int)num3, 0, _mapRes - 1);
		int num6 = Mathf.Clamp((int)num4, 0, _mapRes - 1);
		int num7 = Mathf.Min(num5 + 1, _mapRes - 1);
		int num8 = Mathf.Min(num6 + 1, _mapRes - 1);
		float t = num3 - (float)num5;
		float t2 = num4 - (float)num6;
		float a = BitUtility.Short2Float(_mapHeights[num6 * _mapRes + num5]);
		float b = BitUtility.Short2Float(_mapHeights[num6 * _mapRes + num7]);
		float a2 = BitUtility.Short2Float(_mapHeights[num8 * _mapRes + num5]);
		float b2 = BitUtility.Short2Float(_mapHeights[num8 * _mapRes + num7]);
		float a3 = Mathf.Lerp(a, b, t);
		float b3 = Mathf.Lerp(a2, b2, t);
		return Mathf.Lerp(a3, b3, t2);
	}

	private void SpawnRealCliffsInRegion()
	{
	}

	private static bool LooksLikeCliff(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return false;
		}
		string text = path.ToLowerInvariant();
		if (!text.Contains("cliff") && !text.Contains("rock") && !text.Contains("formation") && !text.Contains("iceberg"))
		{
			return text.Contains("ice_sheet");
		}
		return true;
	}

	private void EnsurePlayerScaleReference(bool reposition)
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (!ShowPlayerScaleReference)
		{
			DestroyPlayerScaleRef();
		}
		else if (_initialized && !(_heightmap == null))
		{
			if (_playerScaleRef == null)
			{
				_playerScaleRef = CreatePlayerScaleReference();
				reposition = true;
			}
			_playerScaleRef.transform.localScale = new Vector3(0.5f, Mathf.Max(0.1f, PlayerReferenceHeight) * 0.5f, 0.5f);
			if (reposition)
			{
				Vector3 vector = TerrainMeta.Position + new Vector3(TerrainMeta.Size.x * 0.5f, 0f, TerrainMeta.Size.z * 0.5f);
				PlacePlayerRefAtXZ(vector.x, vector.z);
			}
		}
	}

	public void PlacePlayerScaleRefAtLookTarget()
	{
		if (!Application.isPlaying || !_initialized || _heightmap == null)
		{
			return;
		}
		ShowPlayerScaleReference = true;
		EnsurePlayerScaleReference(reposition: false);
		if (_playerScaleRef == null)
		{
			return;
		}
		Camera main = Camera.main;
		if (main == null)
		{
			return;
		}
		Vector3 position = main.transform.position;
		Vector3 forward = main.transform.forward;
		Vector3 vector;
		if (Physics.Raycast(position, forward, out var hitInfo, 100000f, -1, QueryTriggerInteraction.Ignore))
		{
			vector = hitInfo.point;
		}
		else
		{
			float num = TerrainMeta.Position.y + TerrainMeta.Size.y * 0.5f;
			if (Mathf.Abs(forward.y) > 0.0001f)
			{
				float num2 = (num - position.y) / forward.y;
				vector = ((num2 > 0f) ? (position + forward * num2) : position);
			}
			else
			{
				vector = position;
			}
		}
		PlacePlayerRefAtXZ(vector.x, vector.z);
	}

	public void GoToMarker()
	{
		if (Application.isPlaying && _initialized && !(_heightmap == null))
		{
			ShowPlayerScaleReference = true;
			EnsurePlayerScaleReference(reposition: false);
			Camera main = Camera.main;
			if (!(main == null))
			{
				Vector3 vector = ((_playerScaleRef != null) ? _playerScaleRef.transform.position : (TerrainMeta.Position + new Vector3(TerrainMeta.Size.x * 0.5f, 0f, TerrainMeta.Size.z * 0.5f)));
				float num = Mathf.Max(0.1f, PlayerReferenceHeight);
				Vector3 normalized = (Quaternion.Euler(20f, -45f, 0f) * Vector3.forward).normalized;
				float num2 = Mathf.Max(6f, num * 4f);
				main.transform.position = vector - normalized * num2 + Vector3.up * (num * 0.5f);
				main.transform.rotation = Quaternion.LookRotation((vector - main.transform.position).normalized, Vector3.up);
			}
		}
	}

	public void MovePlayerScaleRefToCamera()
	{
		if (Application.isPlaying && _initialized && !(_heightmap == null))
		{
			ShowPlayerScaleReference = true;
			EnsurePlayerScaleReference(reposition: false);
			if (!(_playerScaleRef == null))
			{
				Camera main = Camera.main;
				Vector3 vector = ((main != null) ? main.transform.position : _playerScaleRef.transform.position);
				PlacePlayerRefAtXZ(vector.x, vector.z);
			}
		}
	}

	private void PlacePlayerRefAtXZ(float worldX, float worldZ)
	{
		if (!(_playerScaleRef == null))
		{
			float x = TerrainMeta.Position.x;
			float max = TerrainMeta.Position.x + TerrainMeta.Size.x;
			float z = TerrainMeta.Position.z;
			float max2 = TerrainMeta.Position.z + TerrainMeta.Size.z;
			worldX = Mathf.Clamp(worldX, x, max);
			worldZ = Mathf.Clamp(worldZ, z, max2);
			float normX = TerrainMeta.NormalizeX(worldX);
			float normZ = TerrainMeta.NormalizeZ(worldZ);
			float height = _heightmap.GetHeight(normX, normZ);
			_playerScaleRef.transform.position = new Vector3(worldX, height + Mathf.Max(0.1f, PlayerReferenceHeight) * 0.5f, worldZ);
		}
	}

	private GameObject CreatePlayerScaleReference()
	{
		GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
		obj.name = "PlayerScaleReference";
		Collider component = obj.GetComponent<Collider>();
		if (component != null)
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(component);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(component);
			}
		}
		MeshRenderer component2 = obj.GetComponent<MeshRenderer>();
		if (component2 != null)
		{
			component2.shadowCastingMode = ShadowCastingMode.Off;
			component2.material.color = new Color(0.1f, 0.9f, 1f, 1f);
		}
		return obj;
	}

	private void DestroyPlayerScaleRef()
	{
		if (!(_playerScaleRef == null))
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(_playerScaleRef);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(_playerScaleRef);
			}
			_playerScaleRef = null;
		}
	}

	public bool TryGetMapSeedSize(out uint size, out uint seed, out string problem)
	{
		return TryResolveMapSeedSize(out size, out seed, out problem);
	}

	private bool TryResolveMapSeedSize(out uint size, out uint seed, out string problem)
	{
		problem = null;
		ParseSeedSizeFromMapName(out size, out seed);
		if (size != 0 && _mapWorldSize.x > 0f && Mathf.Abs((float)size - _mapWorldSize.x) > 1f)
		{
			problem = $"pre-cliff baseline: filename size {size} != map size {_mapWorldSize.x:0} " + "(is MapFilePath pointing at the right file?)";
			return false;
		}
		if (size == 0 && _mapWorldSize.x > 0f)
		{
			size = (uint)Mathf.RoundToInt(_mapWorldSize.x);
		}
		if (size == 0)
		{
			problem = "pre-cliff baseline: couldn't work out the map size. Point MapFilePath at a .map whose name carries it, or Initialize once so it can be read out of the file.";
			return false;
		}
		if (seed != 0)
		{
			Seed = seed;
		}
		else
		{
			seed = Seed;
		}
		if (seed == 0)
		{
			problem = $"pre-cliff baseline: size {size} is known, but this map's name carries no seed - " + "uploaded maps replace it with a generation timestamp. Run `seed` on the server and put the value in the Seed field, then reload.";
			return false;
		}
		return true;
	}

	private void ParseSeedSizeFromMapName(out uint size, out uint seed)
	{
		size = 0u;
		seed = 0u;
		if (string.IsNullOrEmpty(MapFilePath))
		{
			return;
		}
		string text = Path.GetFileNameWithoutExtension(MapFilePath);
		int num = text.IndexOf('_');
		if (num >= 0)
		{
			text = text.Substring(0, num);
		}
		string[] array = text.Split('.');
		List<ulong> list = new List<ulong>();
		for (int i = 0; i < array.Length; i++)
		{
			if (ulong.TryParse(array[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
			{
				list.Add(result);
			}
			else
			{
				list.Clear();
			}
		}
		if (list.Count >= 3)
		{
			ulong num2 = list[list.Count - 3];
			ulong num3 = list[list.Count - 2];
			if (num2 != 0L && num2 <= uint.MaxValue)
			{
				size = (uint)num2;
				seed = (uint)((num3 != 0 && num3 <= uint.MaxValue) ? num3 : 0u);
			}
		}
	}

	private bool TryLoadPreCliff()
	{
		_preCliffLoaded = false;
		_preCliffHeights = null;
		if (Source != TerrainSource.MapFileRegion)
		{
			_preCliffStatus = "pre-cliff baseline: only used in Map File Region mode";
			return false;
		}
		if (!TryResolveMapSeedSize(out var _, out var _, out var problem))
		{
			_preCliffStatus = problem;
			return false;
		}
		_preCliffStatus = "pre-cliff baseline: editor only";
		return false;
	}

	private float SamplePreCliffHeight01(float worldX, float worldZ)
	{
		float num = Mathf.Clamp01((worldX - _mapWorldPos.x) / _mapWorldSize.x);
		float num2 = Mathf.Clamp01((worldZ - _mapWorldPos.z) / _mapWorldSize.z);
		float num3 = num * (float)(_preCliffRes - 1);
		float num4 = num2 * (float)(_preCliffRes - 1);
		int num5 = Mathf.Clamp((int)num3, 0, _preCliffRes - 1);
		int num6 = Mathf.Clamp((int)num4, 0, _preCliffRes - 1);
		int num7 = Mathf.Min(num5 + 1, _preCliffRes - 1);
		int num8 = Mathf.Min(num6 + 1, _preCliffRes - 1);
		float t = num3 - (float)num5;
		float t2 = num4 - (float)num6;
		float a = BitUtility.Short2Float(_preCliffHeights[num6 * _preCliffRes + num5]);
		float b = BitUtility.Short2Float(_preCliffHeights[num6 * _preCliffRes + num7]);
		float a2 = BitUtility.Short2Float(_preCliffHeights[num8 * _preCliffRes + num5]);
		float b2 = BitUtility.Short2Float(_preCliffHeights[num8 * _preCliffRes + num7]);
		float a3 = Mathf.Lerp(a, b, t);
		float b3 = Mathf.Lerp(a2, b2, t);
		return Mathf.Lerp(a3, b3, t2);
	}

	private void ApplyPreCliffBaseline()
	{
		if (_baseline == null)
		{
			return;
		}
		float num = Mathf.Max(1f, RegionSize) * 0.5f;
		float num2 = RegionCenter.x - num;
		float num3 = RegionCenter.z - num;
		float num4 = ((_res <= 1) ? 1 : (_res - 1));
		if (_bakedRegion == null || _bakedRegion.Length != _res * _res)
		{
			_bakedRegion = new float[_res * _res];
		}
		for (int i = 0; i < _res; i++)
		{
			float worldZ = num3 + (float)i / num4 * RegionSize;
			for (int j = 0; j < _res; j++)
			{
				float worldX = num2 + (float)j / num4 * RegionSize;
				_bakedRegion[i * _res + j] = SampleMapHeight01(worldX, worldZ);
			}
		}
		if (!_preCliffLoaded)
		{
			return;
		}
		for (int k = 0; k < _res; k++)
		{
			float worldZ2 = num3 + (float)k / num4 * RegionSize;
			for (int l = 0; l < _res; l++)
			{
				float worldX2 = num2 + (float)l / num4 * RegionSize;
				_baseline[k * _res + l] = SamplePreCliffHeight01(worldX2, worldZ2);
			}
		}
	}

	private string ComputePreCliffValidation()
	{
		if (_bakedRegion == null || _heightmap == null)
		{
			return string.Empty;
		}
		float y = _mapWorldSize.y;
		double num = 0.0;
		float num2 = 0f;
		int num3 = 0;
		int num4 = _res * _res;
		for (int i = 0; i < _res; i++)
		{
			for (int j = 0; j < _res; j++)
			{
				float height = _heightmap.GetHeight01(j, i);
				float num5 = _bakedRegion[i * _res + j];
				float num6 = Mathf.Abs(height - num5);
				num += (double)num6;
				if (num6 > num2)
				{
					num2 = num6;
				}
				if (num6 <= 0.00025f)
				{
					num3++;
				}
			}
		}
		float num7 = (float)(num / (double)num4) * y;
		float num8 = num2 * y;
		float num9 = 100f * (float)num3 / (float)num4;
		return $"validation vs baked map: mean {num7:0.00}m, max {num8:0.0}m, {num9:0.0}% within 0.25m";
	}

	private void ClearPreCliffState()
	{
		_preCliffHeights = null;
		_preCliffLoaded = false;
		_bakedRegion = null;
		_preCliffStatus = "pre-cliff baseline: not loaded";
	}

	private int StableCliffKey(Transform root)
	{
		if (root == null)
		{
			return 0;
		}
		SandboxCliffSource component = root.GetComponent<SandboxCliffSource>();
		if (component != null && component.SandboxId != 0)
		{
			return component.SandboxId;
		}
		return root.GetInstanceID();
	}

	private void OnDisable()
	{
		Teardown();
	}

	private void OnDestroy()
	{
		Teardown();
	}

	public void CyclePatch()
	{
		CurrentPatch = (TerrainPatch)((int)(CurrentPatch + 1) % Enum.GetValues(typeof(TerrainPatch)).Length);
		if (_initialized)
		{
			FillPatch(CurrentPatch);
			SnapshotBaseline();
			_heightmap.ApplyToTerrain();
		}
	}

	public void InitializeSandbox()
	{
		Teardown();
		bool flag = Source == TerrainSource.ProceduralReal;
		bool flag2 = Source == TerrainSource.MapFileRegion;
		if (flag2 && !TryLoadMapForRegion(out var error))
		{
			Debug.LogError("[CliffSandbox] Map region load failed: " + error);
			return;
		}
		Vector3 vector;
		Vector3 vector2;
		int b;
		if (flag2)
		{
			float num = Mathf.Max(1f, RegionSize) * 0.5f;
			vector = new Vector3(RegionSize, _mapWorldSize.y, RegionSize);
			vector2 = new Vector3(RegionCenter.x - num, _mapWorldPos.y, RegionCenter.z - num);
			if (AutoMapRegionResolution)
			{
				MapRegionResolution = ComputeMapRegionResolution();
			}
			b = MapRegionResolution;
		}
		else
		{
			vector = (flag ? new Vector3(ProceduralMapSize, ProceduralHeightRange, ProceduralMapSize) : TerrainSize);
			vector2 = (flag ? (-0.5f * vector) : TerrainOrigin);
			vector2.y = 0f;
			b = HeightmapResolution;
		}
		if (flag)
		{
			World.InitSeed(Seed);
			if (World.Config == null)
			{
				World.Config = new WorldConfig();
			}
		}
		_terrainData = new TerrainData();
		_terrainData.heightmapResolution = Mathf.Max(33, b);
		_terrainData.size = vector;
		_res = _terrainData.heightmapResolution;
		_terrainGO = Terrain.CreateTerrainGameObject(_terrainData);
		_terrainGO.name = "CliffSandboxTerrain";
		_terrainGO.transform.position = vector2;
		_meta = _terrainGO.AddComponent<TerrainMeta>();
		_terrainGO.AddComponent<TerrainHeightMap>();
		_meta.terrainData = _terrainData;
		if (!_meta.terrainRenderer.HasTerrain && _terrainGO.TryGetComponent<Terrain>(out var component))
		{
			_meta.terrainRenderer.SetTerrain(component);
		}
		_meta.Init();
		_meta.SetupComponents();
		_heightmap = TerrainMeta.HeightMap;
		if (_heightmap == null)
		{
			Debug.LogError("[CliffSandbox] TerrainHeightMap did not initialize.");
			return;
		}
		_res = _heightmap.res;
		if (flag2)
		{
			FillFromMapRegion();
		}
		else if (flag)
		{
			GenerateRealBaseHeight();
		}
		else
		{
			FillPatch(CurrentPatch);
		}
		SnapshotBaseline();
		if (flag2)
		{
			ClearPreCliffState();
			if (UsePreCliffBaseline)
			{
				TryLoadPreCliff();
			}
			ApplyPreCliffBaseline();
		}
		_heightmap.ApplyToTerrain();
		_initialized = true;
		if (flag2 && SpawnRealCliffs)
		{
			SpawnRealCliffsInRegion();
		}
		EnsurePlayerScaleReference(reposition: true);
		if (MoveCameraOnInitialize)
		{
			MoveCameraToTerrainOverlook(vector2, vector);
		}
		Vector3 vector3 = vector2;
		Vector3 vector4 = vector2 + vector;
		string arg = (flag2 ? ("map region '" + Path.GetFileName(MapFilePath) + "'") : (flag ? $"real base heightmap (seed {World.Seed})" : $"patch '{CurrentPatch}'"));
		Debug.Log($"[CliffSandbox] Initialized {_res}x{_res} terrain from {arg}. " + $"Bounds X[{vector3.x:0}..{vector4.x:0}] Z[{vector3.z:0}..{vector4.z:0}] Y[{vector3.y:0}..{vector4.y:0}]. " + (flag2 ? "Edit a spawned cliff's prefab, then 'Recalculate selected' (G) to re-solve." : (flag ? "Use 'Auto-place on slope' (F) to drop the cliff on a suitable incline." : "Drop a cliff inside these bounds, assign 'cliffRoot', then Place.")));
	}

	private void MoveCameraToTerrainOverlook(Vector3 origin, Vector3 size)
	{
		Camera main = Camera.main;
		if (!(main == null) && !(_heightmap == null))
		{
			int num = _res / 2;
			float x = origin.x + ((float)num + 0.5f) / (float)_res * size.x;
			float z = origin.z + ((float)num + 0.5f) / (float)_res * size.z;
			float height = _heightmap.GetHeight(num, num);
			Vector3 vector = new Vector3(x, height, z);
			float num2 = Mathf.Clamp(size.x * 0.5f, 60f, 1500f);
			Vector3 vector2 = Quaternion.Euler(35f, -45f, 0f) * Vector3.forward;
			main.transform.position = vector - vector2 * num2;
			main.transform.rotation = Quaternion.LookRotation(vector2, Vector3.up);
			if (main.farClipPlane < num2 * 4f)
			{
				main.farClipPlane = num2 * 4f;
			}
		}
	}

	public void ResetTerrain()
	{
		if (EnsureReady())
		{
			RestoreBaseline();
			_heightmap.ApplyToTerrain();
		}
	}

	public void PlaceCliff()
	{
		if (!EnsureReady())
		{
			return;
		}
		if (cliffRoot == null)
		{
			Debug.LogWarning("[CliffSandbox] No cliffRoot assigned.");
			return;
		}
		RestoreBaseline();
		_lastAnchorBreakdown = string.Empty;
		_lastPlaceInfo = PlaceCliffInstance(cliffRoot, captureBreakdown: true);
		_heightmap.ApplyToTerrain();
		if (FrameCameraOnPlacedCliff)
		{
			FrameCameraOnCliff(cliffRoot);
		}
		bool flag = _lastPlaceInfo.Contains("REJECTED");
		Debug.Log("[CliffSandbox] Placed '" + cliffRoot.name + "': " + _lastPlaceInfo.Replace('\n', ' ') + (flag ? " | Anchors rejected: the terrain under the cliff doesn't fit the anchor extents. Orient the cliff to the slope (Auto-place F in Procedural mode), move it onto a steeper incline, or check the prefab's TerrainAnchor Extents/Offset." : string.Empty));
	}

	public void RecalculateSelectedCliffs()
	{
		if (!EnsureReady())
		{
			return;
		}
		List<Transform> list = CollectSelectedCliffRoots();
		if (list.Count == 0)
		{
			_lastPlaceInfo = "Recalculate selected: nothing selected. Enable 'Click-to-select cliffs' and click the cliff to test, then press this again.";
			_lastAnchorBreakdown = string.Empty;
			Debug.LogWarning("[CliffSandbox] Recalculate selected: no cliff selected.");
			return;
		}
		List<Transform> list2 = OrderCliffRootsBySpawn(list);
		RestoreBaseline();
		_lastAnchorBreakdown = string.Empty;
		int num = 0;
		List<string> list3 = new List<string>();
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Transform item in list2)
		{
			string text = PlaceCliffInstance(item, _lastAnchorBreakdown.Length == 0);
			if (text.Contains("REJECTED"))
			{
				list3.Add(item.name);
			}
			else
			{
				num++;
			}
			stringBuilder.Append("\n" + item.name + ": " + text.Replace('\n', ' '));
		}
		_heightmap.ApplyToTerrain();
		string text2 = ComputePreCliffValidation();
		_lastPlaceInfo = $"Recalculated {list2.Count} selected cliff(s), {num} accepted" + ((list3.Count > 0) ? string.Format(", {0} REJECTED (no single height fits their anchors): {1}", list3.Count, string.Join(", ", list3)) : string.Empty) + "." + stringBuilder.ToString();
		if (!string.IsNullOrEmpty(text2))
		{
			_lastPlaceInfo = _lastPlaceInfo + "\n" + text2;
		}
		Debug.Log("[CliffSandbox] " + _lastPlaceInfo.Replace('\n', ' '));
		if (!string.IsNullOrEmpty(_lastAnchorBreakdown))
		{
			Debug.Log("[CliffSandbox] Anchor breakdown (selected):\n" + _lastAnchorBreakdown);
		}
	}

	private List<Transform> CollectSelectedCliffRoots()
	{
		List<Transform> list = new List<Transform>();
		foreach (Transform selectedGizmoRoot in _selectedGizmoRoots)
		{
			if (selectedGizmoRoot != null && !list.Contains(selectedGizmoRoot))
			{
				list.Add(selectedGizmoRoot);
			}
		}
		return list;
	}

	public void ReAnchorSelectedCliffs()
	{
		if (!EnsureReady())
		{
			return;
		}
		List<Transform> list = CollectSelectedCliffRoots();
		if (list.Count == 0)
		{
			_lastPlaceInfo = "Re-anchor selected: nothing selected. Enable 'Click-to-select cliffs' and click the cliff to test, then press this again.";
			_lastAnchorBreakdown = string.Empty;
			Debug.LogWarning("[CliffSandbox] Re-anchor selected: no cliff selected.");
			return;
		}
		_lastAnchorBreakdown = string.Empty;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Transform item in list)
		{
			TerrainAnchor[] componentsInChildren = item.GetComponentsInChildren<TerrainAnchor>(includeInactive: true);
			if (componentsInChildren.Length == 0)
			{
				num3++;
				stringBuilder.Append("\n" + item.name + ": no anchors (nothing to solve)");
				continue;
			}
			PrefabAttribute[] attrs = componentsInChildren;
			PrimeAttributes(attrs, item);
			Vector3 position = item.position;
			Quaternion rotation = item.rotation;
			Vector3 lossyScale = item.lossyScale;
			if (string.IsNullOrEmpty(_lastAnchorBreakdown))
			{
				_lastAnchorBreakdown = $"instance @ ({position.x:0},{position.y:0},{position.z:0}):\n" + BuildAnchorBreakdown(componentsInChildren, position, rotation, lossyScale);
			}
			Vector3 pos = position;
			if (item.ApplyTerrainAnchors(componentsInChildren, ref pos, rotation, lossyScale, AnchorMode))
			{
				float num4 = pos.y - position.y;
				item.position = new Vector3(position.x, pos.y, position.z);
				num++;
				stringBuilder.Append($"\n{item.name}: solved, dY {num4:0.00} -> Y {pos.y:0.0}");
			}
			else
			{
				num2++;
				stringBuilder.Append("\n" + item.name + ": REJECTED (no single Y fits its anchors - see breakdown)");
			}
		}
		_lastPlaceInfo = $"Re-anchored {list.Count} selected cliff(s): {num} moved, {num2} rejected" + ((num3 > 0) ? $", {num3} anchor-less" : string.Empty) + " (terrain unchanged)." + stringBuilder.ToString();
		Debug.Log("[CliffSandbox] " + _lastPlaceInfo.Replace('\n', ' '));
		if (!string.IsNullOrEmpty(_lastAnchorBreakdown))
		{
			Debug.Log("[CliffSandbox] Anchor breakdown (selected):\n" + _lastAnchorBreakdown);
		}
	}

	public void CarveSelectedCliffs()
	{
		if (!EnsureReady())
		{
			return;
		}
		List<Transform> list = CollectSelectedCliffRoots();
		if (list.Count == 0)
		{
			_lastPlaceInfo = "Carve selected: nothing selected. Enable 'Click-to-select cliffs' and click the cliff to carve, then press this again.";
			Debug.LogWarning("[CliffSandbox] Carve selected: no cliff selected.");
			return;
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		StringBuilder stringBuilder = new StringBuilder();
		list.Sort((Transform a, Transform b) => StableCliffKey(a).CompareTo(StableCliffKey(b)));
		List<int> list2 = list.ConvertAll<int>(StableCliffKey);
		list2.Sort();
		string text = string.Join(",", list2);
		bool flag = false;
		if (_preCarve != null && _preCarve.Length == _res * _res && _preCarveSelectionKey == text)
		{
			for (int i = 0; i < _res; i++)
			{
				for (int j = 0; j < _res; j++)
				{
					_heightmap.SetHeight(j, i, _preCarve[i * _res + j]);
				}
			}
			flag = true;
		}
		else
		{
			_preCarve = new float[_res * _res];
			for (int k = 0; k < _res; k++)
			{
				for (int l = 0; l < _res; l++)
				{
					_preCarve[k * _res + l] = _heightmap.GetHeight01(l, k);
				}
			}
			_preCarveSelectionKey = text;
		}
		foreach (Transform item in list)
		{
			TerrainModifier[] componentsInChildren = item.GetComponentsInChildren<TerrainModifier>(includeInactive: true);
			TerrainPlacement[] componentsInChildren2 = item.GetComponentsInChildren<TerrainPlacement>(includeInactive: true);
			List<TerrainModifier> list3 = new List<TerrainModifier>(componentsInChildren.Length);
			PrefabAttribute[] attrs = componentsInChildren2;
			PrimeAttributes(attrs, item);
			for (int m = 0; m < componentsInChildren.Length; m++)
			{
				PrimeAttribute(componentsInChildren[m], item);
				if (IsHeightModifier(componentsInChildren[m]))
				{
					list3.Add(componentsInChildren[m]);
				}
			}
			bool flag2 = ReplayTerrainPlacementsOnRecalc && componentsInChildren2.Length != 0;
			if (list3.Count == 0 && !flag2)
			{
				num2++;
				stringBuilder.Append("\n" + item.name + ": no height modifiers (nothing to carve)");
				continue;
			}
			Vector3 position = item.position;
			Quaternion rotation = item.rotation;
			Vector3 lossyScale = item.lossyScale;
			int num4 = 0;
			if (flag2)
			{
				try
				{
					item.ApplyTerrainPlacements(componentsInChildren2, position, rotation, lossyScale);
					num4 = componentsInChildren2.Length;
				}
				catch (Exception ex)
				{
					Debug.LogWarning("[CliffSandbox] '" + item.name + "' TerrainPlacement replay failed: " + ex.Message);
				}
			}
			int num5 = 0;
			for (int n = 0; n < list3.Count; n++)
			{
				if (list3[n] is TerrainHeightAdd)
				{
					num5++;
				}
			}
			num3 += num5;
			if (list3.Count > 0)
			{
				item.ApplyTerrainModifiers(list3.ToArray(), position, rotation, lossyScale);
			}
			num++;
			stringBuilder.Append($"\n{item.name}: carved {list3.Count} height mod(s)" + ((num4 > 0) ? $", {num4} placement(s)" : string.Empty) + ((num5 > 0) ? $" ({num5} HeightAdd - accumulates on re-press)" : string.Empty));
		}
		_heightmap.ApplyToTerrain();
		_lastPlaceInfo = $"Carved {num} selected cliff(s) at their current position(s)" + ((num2 > 0) ? $", {num2} without height modifiers" : string.Empty) + (flag ? " (rewound previous carve first)" : string.Empty) + " (baseline untouched)." + ((num3 > 0) ? " Note: HeightAdd re-applies from the rewound base (re-press safe; switching selections back and forth still accumulates)." : string.Empty) + stringBuilder.ToString();
		Debug.Log("[CliffSandbox] " + _lastPlaceInfo.Replace('\n', ' '));
	}

	private List<Transform> OrderCliffRootsBySpawn(List<Transform> roots)
	{
		HashSet<Transform> hashSet = new HashSet<Transform>(roots);
		List<Transform> list = new List<Transform>(roots.Count);
		HashSet<Transform> hashSet2 = new HashSet<Transform>();
		for (int i = 0; i < _spawnedCliffs.Count; i++)
		{
			GameObject gameObject = _spawnedCliffs[i];
			if (!(gameObject == null))
			{
				Transform root = gameObject.transform.root;
				if (hashSet.Contains(root) && hashSet2.Add(root))
				{
					list.Add(root);
				}
			}
		}
		foreach (Transform root2 in roots)
		{
			if (hashSet2.Add(root2))
			{
				list.Add(root2);
			}
		}
		return list;
	}

	private string PlaceCliffInstance(Transform root, bool captureBreakdown)
	{
		TerrainAnchor[] componentsInChildren = root.GetComponentsInChildren<TerrainAnchor>(includeInactive: true);
		TerrainModifier[] componentsInChildren2 = root.GetComponentsInChildren<TerrainModifier>(includeInactive: true);
		TerrainPlacement[] componentsInChildren3 = root.GetComponentsInChildren<TerrainPlacement>(includeInactive: true);
		List<TerrainModifier> list = new List<TerrainModifier>(componentsInChildren2.Length);
		TerrainFootprint componentInChildren = root.GetComponentInChildren<TerrainFootprint>(includeInactive: true);
		PrefabAttribute[] attrs = componentsInChildren;
		PrimeAttributes(attrs, root);
		attrs = componentsInChildren3;
		PrimeAttributes(attrs, root);
		if ((bool)componentInChildren)
		{
			PrimeAttribute(componentInChildren, root);
			componentInChildren.InvalidateRootRing();
		}
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			PrimeAttribute(componentsInChildren2[i], root);
			if (IsHeightModifier(componentsInChildren2[i]))
			{
				list.Add(componentsInChildren2[i]);
			}
		}
		Vector3 position = root.position;
		Quaternion rotation = root.rotation;
		Vector3 lossyScale = root.lossyScale;
		float y = position.y;
		Vector3 pos = position;
		bool flag = true;
		if (componentsInChildren.Length != 0)
		{
			flag = root.ApplyTerrainAnchors(componentsInChildren, ref pos, rotation, lossyScale, AnchorMode);
		}
		if (captureBreakdown && componentsInChildren.Length != 0)
		{
			Vector3 position2 = root.position;
			_lastAnchorBreakdown = $"instance @ ({position2.x:0},{position2.y:0},{position2.z:0}):\n" + BuildAnchorBreakdown(componentsInChildren, position, rotation, lossyScale);
		}
		bool flag2 = RecalcKeepMapPositions && Source == TerrainSource.MapFileRegion;
		Vector3 pos2 = (flag2 ? position : pos);
		float num = 0f;
		bool flag3 = (bool)componentInChildren && componentInChildren.IsActive(rotation);
		bool flag4 = true;
		bool filled = false;
		if (flag3)
		{
			num = componentInChildren.MeasureGap(pos2, rotation, lossyScale);
			flag4 = componentInChildren.RejectAboveGap <= 0f || num <= componentInChildren.RejectAboveGap;
			if (ApplyTerrainFootprintOnRecalc && flag4)
			{
				componentInChildren.Fill(pos2, rotation, lossyScale);
				filled = true;
			}
		}
		int num2 = 0;
		if (ReplayTerrainPlacementsOnRecalc && componentsInChildren3.Length != 0)
		{
			try
			{
				root.ApplyTerrainPlacements(componentsInChildren3, pos2, rotation, lossyScale);
				num2 = componentsInChildren3.Length;
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[CliffSandbox] '" + root.name + "' TerrainPlacement replay failed: " + ex.Message);
			}
		}
		TerrainModifier[] array = list.ToArray();
		if (array.Length != 0)
		{
			root.ApplyTerrainModifiers(array, pos2, rotation, lossyScale);
		}
		if (!flag2 && SnapCliffToAnchoredHeight && componentsInChildren.Length != 0)
		{
			root.position = new Vector3(root.position.x, pos.y, root.position.z);
		}
		int num3 = componentsInChildren2.Length - array.Length;
		return string.Format("anchors: {0} ({1})\n", componentsInChildren.Length, flag ? "accepted" : "REJECTED") + string.Format("snap dY: {0:0.00}{1}\n", pos.y - y, flag2 ? " (locked to map pos)" : string.Empty) + $"height mods: {array.Length}" + ((num3 > 0) ? $" ({num3} non-height skipped)" : "") + ((num2 > 0) ? $", placements: {num2}" : string.Empty) + FootprintStatus(componentInChildren, rotation, flag3, flag4, filled, num);
	}

	private static string FootprintStatus(TerrainFootprint footprint, Quaternion rot, bool active, bool ok, bool filled, float gap)
	{
		if (!footprint)
		{
			return string.Empty;
		}
		if (!footprint.HasRing)
		{
			return "\nfootprint: no ring generated";
		}
		if (!active)
		{
			return $"\nfootprint: inactive (tilt {footprint.GetTilt(rot):0}° > MaxTiltDegrees {footprint.MaxTiltDegrees:0}°)";
		}
		string arg = ((!ok) ? $"REJECTED - over RejectAboveGap {footprint.RejectAboveGap:0.00}" : (filled ? $"filled, clamped at MaxFill {footprint.MaxFill:0.00}" : "measured only"));
		return $"\nfootprint gap: {gap:0.00} m ({arg})";
	}

	private static bool IsHeightModifier(TerrainModifier m)
	{
		if (!(m is TerrainHeightSet) && !(m is TerrainHeightRaise))
		{
			return m is TerrainHeightAdd;
		}
		return true;
	}

	private string BuildAnchorBreakdown(TerrainAnchor[] anchors, Vector3 pos, Quaternion rot, Vector3 scale)
	{
		if (anchors == null || anchors.Length == 0)
		{
			return "no anchors on this cliff";
		}
		float num = float.MinValue;
		float num2 = float.MaxValue;
		int num3 = -1;
		int num4 = -1;
		List<string> list = new List<string>(anchors.Length + 4);
		for (int i = 0; i < anchors.Length; i++)
		{
			TerrainAnchor terrainAnchor = anchors[i];
			Vector3 vector = rot * Vector3.Scale(terrainAnchor.worldPosition, scale);
			Vector3 pos2 = pos + vector;
			terrainAnchor.Apply(out var height, out var min, out var max, pos2, scale, rot);
			float num5 = min - vector.y;
			float num6 = max - vector.y;
			if (num5 > num)
			{
				num = num5;
				num3 = i;
			}
			if (num6 < num2)
			{
				num2 = num6;
				num4 = i;
			}
			list.Add($"  [{i}] {terrainAnchor.name}: terrainH {height:0.0}  root-Y window [{num5:0.0} .. {num6:0.0}]" + $"  (E{terrainAnchor.Extents:0.#}/O{terrainAnchor.Offset:0.#}/oY {vector.y:0.0})");
		}
		if (num2 > 1f && num < 1f)
		{
			num = 1f;
		}
		string text = ((!(num2 < num)) ? ($"OK: shared root-Y window [{num:0.0} .. {num2:0.0}]. MaximizeHeight seats it at {num2:0.0} " + $"(bound by anchor [{num4}] {anchors[num4].name}); current Y {pos.y:0.0} -> dY {num2 - pos.y:0.0}. " + ((Mathf.Abs(num2 - pos.y) < 0.05f) ? "It won't move because the lowest ceiling equals its current height - your new anchor isn't the binding one (its terrain isn't low enough, given its Extents/Offset)." : "It should move by that dY.")) : ($"REJECTED: no single Y fits all {anchors.Length} anchors. Highest floor {num:0.0} " + $"(anchor [{num3}] {anchors[num3].name}) is {num - num2:0.0}m above the " + $"lowest ceiling {num2:0.0} (anchor [{num4}] {anchors[num4].name}). " + $"Widen one of their Extents by >= {num - num2:0.0}m, or raise the floor anchor / lower the ceiling anchor."));
		return text + "\n" + string.Join("\n", list);
	}

	private unsafe void GenerateRealBaseHeight()
	{
		WorldConfig worldConfig2 = World.Config ?? (World.Config = new WorldConfig());
		short* unsafePtr = (short*)_heightmap.dst.GetUnsafePtr();
		GenerateHeight.Native_GenerateHeight(unsafePtr, _heightmap.res, TerrainMeta.Position, TerrainMeta.Size, World.Seed, TerrainMeta.LootAxisAngle, worldConfig2.PercentageTier0, worldConfig2.PercentageTier1, worldConfig2.PercentageTier2, TerrainMeta.BiomeAxisAngle, worldConfig2.PercentageBiomeArid, worldConfig2.PercentageBiomeTemperate, worldConfig2.PercentageBiomeTundra, worldConfig2.PercentageBiomeArctic);
	}

	public void AutoPlaceCliffOnSlope()
	{
		if (!EnsureReady())
		{
			return;
		}
		if (cliffRoot == null)
		{
			Debug.LogWarning("[CliffSandbox] No cliffRoot assigned.");
			return;
		}
		int res = _res;
		int num = Mathf.Max(1, res / 160);
		float num2 = 0.5f * (float)(SlopeFinderMinAngle + SlopeFinderMaxAngle);
		float num3 = float.NegativeInfinity;
		int num4 = -1;
		int num5 = -1;
		Vector3 vector = Vector3.up;
		for (int i = 1; i < res - 1; i += num)
		{
			for (int j = 1; j < res - 1; j += num)
			{
				Vector3 normal = _heightmap.GetNormal(j, i);
				float num6 = Vector3.Angle(Vector3.up, normal);
				if (!(num6 < (float)SlopeFinderMinAngle) && !(num6 > (float)SlopeFinderMaxAngle))
				{
					float num7 = 0f - Mathf.Abs(num6 - num2);
					if (num7 > num3)
					{
						num3 = num7;
						num4 = j;
						num5 = i;
						vector = normal;
					}
				}
			}
		}
		if (num4 < 0)
		{
			Debug.LogWarning($"[CliffSandbox] No slope in [{SlopeFinderMinAngle}..{SlopeFinderMaxAngle}] deg found. " + "Try a different seed/size or widen the range.");
			return;
		}
		float num8 = ((float)num4 + 0.5f) / (float)res;
		float num9 = ((float)num5 + 0.5f) / (float)res;
		float num10 = TerrainMeta.Position.x + num8 * TerrainMeta.Size.x;
		float num11 = TerrainMeta.Position.z + num9 * TerrainMeta.Size.z;
		float height = _heightmap.GetHeight(num4, num5);
		cliffRoot.position = new Vector3(num10, height, num11);
		cliffRoot.rotation = QuaternionEx.LookRotationForcedUp(vector, Vector3.up);
		Debug.Log($"[CliffSandbox] Auto-placed '{cliffRoot.name}' on {Vector3.Angle(Vector3.up, vector):0} deg " + $"slope at ({num10:0},{height:0},{num11:0}).");
		PlaceCliff();
	}

	private void PrimeAttributes(PrefabAttribute[] attrs, Transform root)
	{
		for (int i = 0; i < attrs.Length; i++)
		{
			PrimeAttribute(attrs[i], root);
		}
	}

	private void PrimeAttribute(PrefabAttribute attr, Transform root)
	{
		attr.worldPosition = root.InverseTransformPoint(attr.transform.position);
		attr.worldRotation = Quaternion.Inverse(root.rotation) * attr.transform.rotation;
		attr.worldForward = attr.worldRotation * Vector3.forward;
	}

	private bool EnsureReady()
	{
		if (_initialized && _heightmap != null && _heightmap.res == _res)
		{
			if (TerrainMeta.HeightMap == _heightmap)
			{
				return true;
			}
			Debug.LogWarning("[CliffSandbox] The active terrain heightmap changed out from under the sandbox (terrain was re-initialised elsewhere). Press Initialize to rebuild before placing or recalculating cliffs.");
			_initialized = false;
			return false;
		}
		Debug.LogWarning("[CliffSandbox] Not initialized. Press Initialize first.");
		return false;
	}

	private void SnapshotBaseline()
	{
		_preCarve = null;
		_preCarveSelectionKey = null;
		_baseline = new float[_res * _res];
		for (int i = 0; i < _res; i++)
		{
			for (int j = 0; j < _res; j++)
			{
				_baseline[i * _res + j] = _heightmap.GetHeight01(j, i);
			}
		}
	}

	private void RestoreBaseline()
	{
		if (_baseline == null)
		{
			return;
		}
		_preCarve = null;
		_preCarveSelectionKey = null;
		for (int i = 0; i < _res; i++)
		{
			for (int j = 0; j < _res; j++)
			{
				_heightmap.SetHeight(j, i, _baseline[i * _res + j]);
			}
		}
	}

	private void FillPatch(TerrainPatch patch)
	{
		for (int i = 0; i < _res; i++)
		{
			float v = ((_res > 1) ? ((float)i / (float)(_res - 1)) : 0f);
			for (int j = 0; j < _res; j++)
			{
				float u = ((_res > 1) ? ((float)j / (float)(_res - 1)) : 0f);
				_heightmap.SetHeight(j, i, Mathf.Clamp01(SamplePatch(patch, u, v)));
			}
		}
	}

	private static float SamplePatch(TerrainPatch patch, float u, float v)
	{
		switch (patch)
		{
		case TerrainPatch.Flat:
			return 0.3f;
		case TerrainPatch.SlopeX:
			return 0.12f + 0.45f * u;
		case TerrainPatch.Ridge:
		{
			float num2 = 1f - Mathf.Abs(2f * u - 1f);
			return 0.2f + 0.35f * num2;
		}
		case TerrainPatch.ConvexDome:
		{
			float num = DistFromCentre(u, v);
			return 0.15f + 0.4f * Mathf.Clamp01(1f - num);
		}
		case TerrainPatch.ConcaveBowl:
		{
			float value = DistFromCentre(u, v);
			return 0.18f + 0.37f * Mathf.Clamp01(value);
		}
		default:
			return 0.3f;
		}
	}

	private static float DistFromCentre(float u, float v)
	{
		float num = (u - 0.5f) * 2f;
		float num2 = (v - 0.5f) * 2f;
		return Mathf.Clamp01(Mathf.Sqrt(num * num + num2 * num2));
	}

	private void Teardown()
	{
		ClearSpawnedCliffs();
		DestroyPlayerScaleRef();
		ClearGizmoSelection();
		if (_gizmoMat != null)
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(_gizmoMat);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(_gizmoMat);
			}
			_gizmoMat = null;
		}
		if (_terrainGO != null)
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(_terrainGO);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(_terrainGO);
			}
			_terrainGO = null;
			_heightmap = null;
		}
		else if (_heightmap != null)
		{
			_heightmap.Dispose();
			_heightmap = null;
		}
		if (_terrainData != null)
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(_terrainData);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(_terrainData);
			}
			_terrainData = null;
		}
		_meta = null;
		_baseline = null;
		_preCarve = null;
		_preCarveSelectionKey = null;
		ClearPreCliffState();
		_initialized = false;
	}

	private void ClearSpawnedCliffs(bool immediate = false)
	{
		for (int i = 0; i < _spawnedCliffs.Count; i++)
		{
			GameObject gameObject = _spawnedCliffs[i];
			if (!(gameObject == null))
			{
				if (Application.isPlaying && !immediate)
				{
					UnityEngine.Object.Destroy(gameObject);
				}
				else
				{
					UnityEngine.Object.DestroyImmediate(gameObject);
				}
			}
		}
		_spawnedCliffs.Clear();
		cliffRoot = null;
	}
}
