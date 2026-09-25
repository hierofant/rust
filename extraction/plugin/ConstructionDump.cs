// ConstructionDump - runtime data extraction plugin for offline reconstruction of Rust's
// construction/deployable mechanics. See A:/Server/TASK.md for the full spec this implements.
//
// Design note on the universal serializer's "expand vs $ref" rule (this is the one genuinely
// ambiguous part of the spec, resolved here):
//   - Always fully expand (never $ref): the object passed as dump root; anything deriving from
//     PrefabAttribute when it belongs to the prefab currently being dumped (sockets, socketmods,
//     construction grades, deploy volumes - these were baked out of the GameObject hierarchy by
//     the server at boot into PrefabAttribute.server, so they must be captured this way or lost
//     entirely); anything deriving from ScriptableObject (BuildingGrade, ProtectionProperties -
//     small shared config assets whose actual values TASK.md explicitly asks for); Component
//     instances that are attached to the same GameObject/hierarchy as the current dump root
//     (e.g. ItemMod* on an ItemDefinition).
//   - $ref (do not expand): GameObject, Mesh, Material/PhysicsMaterial, ItemDefinition reached as
//     a cross reference, any other Component not owned by the current root, and PrefabAttribute
//     instances that belong to a DIFFERENT prefab than the one being dumped (e.g. a spawned
//     BuildingBlock's `blockDefinition` pointing back at the wall prefab's shared Construction -
//     that data already lives once in prefabs/<id>.json, so re-expanding it per-instance in
//     basedump would blow up the output for no reason).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Unity.Collections;
using Oxide.Core;

namespace Oxide.Plugins
{
    [Info("ConstructionDump", "extraction", "0.1.0")]
    [Description("Dumps runtime construction/deployable/item data for offline reconstruction")]
    public class ConstructionDump : RustPlugin
    {
        // ------------------------------------------------------------------
        // Paths / bootstrap
        // ------------------------------------------------------------------

        private static readonly List<string> Failures = new List<string>();
        private static string _buildId;

        private string RepoRoot => Path.GetFullPath(Path.Combine(Interface.Oxide.RootDirectory, ".."));
        private string DataRoot => Path.Combine(RepoRoot, "data", GetBuildId());

        private string GetBuildId()
        {
            if (_buildId != null) return _buildId;
            try
            {
                string acf = Path.Combine(Interface.Oxide.RootDirectory, "steamapps", "appmanifest_258550.acf");
                if (File.Exists(acf))
                {
                    string text = File.ReadAllText(acf);
                    var m = System.Text.RegularExpressions.Regex.Match(text, "\"buildid\"\\s*\"(\\d+)\"");
                    if (m.Success)
                    {
                        _buildId = m.Groups[1].Value;
                        return _buildId;
                    }
                }
            }
            catch (Exception ex)
            {
                Failures.Add("GetBuildId: " + ex.Message);
            }
            _buildId = "unknown";
            return _buildId;
        }

        private static void EnsureDir(string path) => Directory.CreateDirectory(path);

        private static void WriteJson(string path, JToken token)
        {
            EnsureDir(Path.GetDirectoryName(path));
            using (var sw = new StreamWriter(path, false, new UTF8Encoding(false)))
            using (var jw = new JsonTextWriter(sw) { Formatting = Formatting.Indented, FloatFormatHandling = FloatFormatHandling.String })
            {
                token.WriteTo(jw);
            }
        }

        // ------------------------------------------------------------------
        // Universal reflection serializer
        // ------------------------------------------------------------------

        private sealed class RefComparer : IEqualityComparer<object>
        {
            public new bool Equals(object x, object y) => ReferenceEquals(x, y);
            public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }

        private static readonly RefComparer RefEq = new RefComparer();

        private class DumpCtx
        {
            public HashSet<object> Ancestors = new HashSet<object>(RefEq);
            public GameObject RootGameObject;
            public uint CurrentPrefabId;
            public Dictionary<int, Mesh> PendingMeshes = new Dictionary<int, Mesh>();
            public Dictionary<int, Mesh> PendingVisualMeshes = new Dictionary<int, Mesh>();

            // Every UnityEngine.Object instance that gets fully expanded (for whatever reason -
            // isRoot, ownership, same-prefab attribute) is recorded here so a SECOND reference to
            // the exact same instance downgrades to a cheap ref instead of re-expanding. Without
            // this, large prefabs with lateral cross-references between many sibling components
            // (e.g. the cargo ship's ~276k BasePathNode AI-path graph, where each node references
            // several neighbors that are all "owned" by the same root) blow up combinatorially -
            // confirmed in testing (458MB for one prefab before this fix).
            public HashSet<object> ExpandedOnce = new HashSet<object>(RefEq);

            // Any GameObjectRef -> GameObject seen while serializing gets its resourceID recorded
            // here. TASK.md 2.3 needs full prefab dumps for grade skins (ConstructionGrade.skinObject)
            // and conditional-model spawned prefabs (corners/steps/edges) - both are just GameObjectRef
            // fields, so instead of special-casing those two, builddump does a BFS over every
            // GameObjectRef target discovered anywhere and fully dumps it too, same as any other prefab.
            public HashSet<uint> DiscoveredPrefabIds = new HashSet<uint>();
        }

        private const int MaxDepth = 8;

        private static JToken Serialize(object obj, DumpCtx ctx, int depth = 0, bool isRoot = false)
        {
            if (obj == null) return JValue.CreateNull();

            Type t = obj.GetType();

            if (obj is string str) return new JValue(str);
            if (obj is bool bl) return new JValue(bl);
            if (t.IsEnum) return SerializeEnum(obj, t);
            if (obj is float f) return new JValue((double)f);
            if (obj is double d) return new JValue(d);
            if (obj is int || obj is uint || obj is long || obj is ulong || obj is short || obj is ushort || obj is byte || obj is sbyte)
                return new JValue(Convert.ToInt64(obj is ulong ul ? (object)(long)ul : obj));

            if (obj is Vector2 v2) return Vec(v2.x, v2.y);
            if (obj is Vector3 v3) return Vec(v3.x, v3.y, v3.z);
            if (obj is Vector4 v4) return Vec(v4.x, v4.y, v4.z, v4.w);
            if (obj is Vector2Int v2i) return Vec(v2i.x, v2i.y);
            if (obj is Vector3Int v3i) return Vec(v3i.x, v3i.y, v3i.z);
            if (obj is Quaternion q) return new JObject { ["x"] = (double)q.x, ["y"] = (double)q.y, ["z"] = (double)q.z, ["w"] = (double)q.w };
            if (obj is Bounds bo) return new JObject { ["center"] = Serialize(bo.center, ctx, depth + 1), ["size"] = Serialize(bo.size, ctx, depth + 1) };
            if (obj is Matrix4x4 m)
            {
                var arr = new JArray();
                for (int i = 0; i < 16; i++) arr.Add((double)m[i]);
                return arr;
            }
            if (obj is LayerMask lm) return new JValue(lm.value);
            if (obj is Color col) return new JObject { ["r"] = (double)col.r, ["g"] = (double)col.g, ["b"] = (double)col.b, ["a"] = (double)col.a };
            if (obj is Color32 col32) return new JObject { ["r"] = col32.r, ["g"] = col32.g, ["b"] = col32.b, ["a"] = col32.a };
            if (obj is Ray ry) return new JObject { ["origin"] = Serialize(ry.origin, ctx, depth + 1), ["direction"] = Serialize(ry.direction, ctx, depth + 1) };

            // PrefabAttribute.gameManager / .prefabAttribute are back-references to global
            // singletons (GameManager holds every loaded prefab; PrefabAttribute.Library/
            // AttributeCollection hold the WHOLE PrefabAttribute.server registry). Neither is a
            // UnityEngine.Object, so without this guard the generic "plain object, reflect
            // fields" path below would re-expand the entire prefab database from inside every
            // single attribute instance - confirmed in testing (800MB+ per prefab file).
            if (t == typeof(GameManager) || t == typeof(PrefabAttribute.Library) || t == typeof(PrefabAttribute.AttributeCollection))
                return new JObject { ["$ref"] = t.Name, ["note"] = "global registry singleton, omitted (not per-object data)" };

            bool isValueType = obj is ValueType;

            if (!isValueType && ctx.Ancestors.Contains(obj))
                return new JObject { ["$cycle"] = true, ["type"] = t.FullName };
            if (depth > MaxDepth)
                return new JObject { ["$truncated"] = true, ["type"] = t.FullName, ["reason"] = "max-depth" };

            if (IsResourceRef(t, out Type refTarget))
                return SerializeResourceRef(obj, refTarget, ctx);

            if (obj is UnityEngine.Object uo)
            {
                // NOTE: PrefabAttribute only auto-expands when it belongs to the SAME prefab
                // currently being dumped (sibling attributes, e.g. Socket_Base.socketMods or
                // ConditionalModel.conditions - all registered under the same prefabID). A
                // PrefabAttribute belonging to a DIFFERENT prefab (e.g. a spawned BuildingBlock's
                // blockDefinition pointing at its wall prefab's shared Construction) is $ref'd -
                // that data already lives once in prefabs/<id>.json. Do NOT also expand when
                // ctx.CurrentPrefabId == 0 (no-context calls like items.json/basedump) - that
                // would defeat the point and re-inline the whole shared object every time.
                bool expand = isRoot
                    || IsSmallConfigScriptableObject(uo)
                    || (obj is PrefabAttribute pa && ctx.CurrentPrefabId != 0 && pa.prefabID == ctx.CurrentPrefabId)
                    || (obj is Component comp && ctx.RootGameObject != null && IsOwnedBy(comp, ctx.RootGameObject));

                if (!expand)
                    return SerializeUnityRef(uo, ctx);

                if (!ctx.ExpandedOnce.Add(uo))
                {
                    // already fully dumped once elsewhere in this same prefab/root - don't redo it.
                    var already = SerializeUnityRef(uo, ctx);
                    already["$alreadyDumpedElsewhereInThisFile"] = true;
                    return already;
                }
                // else: fall through to generic field reflection below
            }

            if (obj is IDictionary dict)
            {
                var o = new JObject();
                if (!isValueType) ctx.Ancestors.Add(obj);
                foreach (DictionaryEntry e in dict)
                    o[Convert.ToString(e.Key, CultureInfo.InvariantCulture)] = Serialize(e.Value, ctx, depth + 1);
                if (!isValueType) ctx.Ancestors.Remove(obj);
                return o;
            }

            if (obj is IEnumerable en)
            {
                var arr = new JArray();
                if (!isValueType) ctx.Ancestors.Add(obj);
                foreach (object item in en) arr.Add(Serialize(item, ctx, depth + 1));
                if (!isValueType) ctx.Ancestors.Remove(obj);
                return arr;
            }

            // Plain object / component: reflect fields.
            {
                var result = new JObject { ["$type"] = t.FullName };
                if (obj is UnityEngine.Object uo2)
                {
                    result["$name"] = SafeUnityName(uo2);
                    if (obj is PrefabAttribute pa2)
                    {
                        result["prefabID"] = pa2.prefabID;
                        result["hierachyName"] = pa2.hierachyName;
                    }
                }

                CaptureSpecialComponentData(obj, ctx, result);

                if (!isValueType) ctx.Ancestors.Add(obj);
                foreach (FieldInfo field in GetSerializableFields(t))
                {
                    object val;
                    try { val = field.GetValue(obj); }
                    catch (Exception ex) { Failures.Add($"field read {t.FullName}.{field.Name}: {ex.Message}"); continue; }
                    try { result[field.Name] = Serialize(val, ctx, depth + 1); }
                    catch (Exception ex) { Failures.Add($"field serialize {t.FullName}.{field.Name}: {ex.Message}"); }
                }
                if (!isValueType) ctx.Ancestors.Remove(obj);
                return result;
            }
        }

        // ScriptableObject is a grab-bag base class covering both small per-grade config blobs
        // (BuildingGrade: 5 instances total) AND huge shared asset graphs (SoundDefinition /
        // AmbienceDefinition - referenced from audio components on big prefabs, thousands of
        // instances cross-linking WeightedAudioClip/AnimationCurve data). Blanket-expanding
        // ScriptableObject blew a single monument prefab's JSON up to 800MB+ in testing. Only the
        // types TASK.md actually needs for construction mechanics get expanded inline; everything
        // else (audio, etc.) is $ref'd like any other asset - out of scope for this dump.
        private static bool IsSmallConfigScriptableObject(UnityEngine.Object obj)
        {
            return obj is BuildingGrade || obj is ProtectionProperties || obj is EntityListScriptableObject;
        }

        // Some baked PrefabAttribute instances in PrefabAttribute.server have already had their
        // underlying native Unity object destroyed by the time builddump runs (observed for a
        // meaningful fraction of prefabs - TerrainAnchor/DecorComponent/etc, likely leftovers from
        // map-generation-only scene objects). Unlike a normal destroyed-object check, accessing
        // `.name` on one of these throws NullReferenceException from native code rather than
        // returning Unity's usual "fake null". Every OTHER field is a plain managed field and
        // reads fine regardless - so this one property access is guarded to avoid losing the
        // entire object's data over a single unreadable native string.
        private static string SafeUnityName(UnityEngine.Object uo)
        {
            try { return uo.name; }
            catch { return "<destroyed native object>"; }
        }

        private static bool IsOwnedBy(Component comp, GameObject root)
        {
            try
            {
                if (comp.gameObject == root) return true;
                return comp.transform != null && root.transform != null && comp.transform.IsChildOf(root.transform);
            }
            catch { return false; }
        }

        private static JObject Vec(params float[] c)
        {
            var o = new JObject();
            string[] names = { "x", "y", "z", "w" };
            for (int i = 0; i < c.Length; i++) o[names[i]] = (double)c[i];
            return o;
        }

        private static JObject Vec(params int[] c)
        {
            var o = new JObject();
            string[] names = { "x", "y", "z" };
            for (int i = 0; i < c.Length; i++) o[names[i]] = c[i];
            return o;
        }

        private static JObject SerializeEnum(object obj, Type t)
        {
            long numeric;
            try { numeric = Convert.ToInt64(obj); } catch { numeric = 0; }
            var o = new JObject
            {
                ["$enum"] = t.FullName,
                ["name"] = obj.ToString(),
                ["value"] = numeric
            };
            if (t.IsDefined(typeof(FlagsAttribute), false))
            {
                var names = new JArray();
                foreach (object flagVal in Enum.GetValues(t))
                {
                    long fv = Convert.ToInt64(flagVal);
                    if (fv != 0 && (numeric & fv) == fv) names.Add(flagVal.ToString());
                }
                o["flagNames"] = names;
            }
            return o;
        }

        private static readonly Dictionary<Type, FieldInfo[]> FieldCache = new Dictionary<Type, FieldInfo[]>();

        private static FieldInfo[] GetSerializableFields(Type t)
        {
            if (FieldCache.TryGetValue(t, out var cached)) return cached;

            var seen = new HashSet<string>();
            var list = new List<FieldInfo>();
            for (Type cur = t; cur != null && cur != typeof(object); cur = cur.BaseType)
            {
                foreach (FieldInfo fi in cur.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (fi.IsStatic) continue;
                    if (!seen.Add(fi.Name)) continue;
                    bool include = fi.IsPublic || fi.GetCustomAttribute<SerializeField>() != null;
                    if (!include) continue;
                    list.Add(fi);
                }
            }
            var arr = list.ToArray();
            FieldCache[t] = arr;
            return arr;
        }

        private static bool IsResourceRef(Type t, out Type target)
        {
            for (Type cur = t; cur != null; cur = cur.BaseType)
            {
                if (cur.IsGenericType && cur.GetGenericTypeDefinition().Name.StartsWith("ResourceRef"))
                {
                    target = cur.GetGenericArguments()[0];
                    return true;
                }
            }
            target = null;
            return false;
        }

        private static JObject SerializeResourceRef(object obj, Type targetType, DumpCtx ctx)
        {
            var o = new JObject { ["$ref"] = targetType.Name };
            try
            {
                FieldInfo guidField = obj.GetType().GetField("guid", BindingFlags.Public | BindingFlags.Instance);
                string guid = guidField?.GetValue(obj) as string;
                o["guid"] = guid;
                string path = string.IsNullOrEmpty(guid) ? null : GameManifest.GUIDToPath(guid);
                o["resourcePath"] = path;
                uint resourceId = string.IsNullOrEmpty(path) ? 0u : StringPool.Get(path);
                o["resourceID"] = resourceId;

                if (resourceId != 0 && targetType == typeof(GameObject))
                    ctx.DiscoveredPrefabIds.Add(resourceId);
            }
            catch (Exception ex)
            {
                o["$error"] = ex.Message;
            }
            return o;
        }

        private static JObject SerializeUnityRef(UnityEngine.Object uo, DumpCtx ctx)
        {
            var o = new JObject { ["$ref"] = uo.GetType().Name, ["name"] = SafeUnityName(uo) };
            try
            {
                if (uo is Mesh mesh)
                {
                    o["meshId"] = MeshFileId(mesh);
                    ctx.PendingMeshes[mesh.GetInstanceID()] = mesh;
                }
                else if (uo is ItemDefinition idef)
                {
                    o["itemid"] = idef.itemid;
                    o["shortname"] = idef.shortname;
                }
                else if (uo is GameObject go)
                {
                    var be = go.GetComponent<BaseEntity>();
                    if (be != null)
                    {
                        // The game compares by prefabID, so a bare GameObject name is useless -
                        // always resolve the actual spawnable path too.
                        o["prefabID"] = be.prefabID;
                        o["resourcePath"] = StringPool.Get(be.prefabID);
                    }
                    o["instanceID"] = go.GetInstanceID();
                }
                else if (uo is PrefabAttribute pa)
                {
                    // Confirmed empirically (foundation.prefab's Twig ConstructionGrade, which is
                    // reachable both via Construction.grades[0] and Construction.defaultGrade):
                    // for a baked PrefabAttribute whose native GameObject has since been destroyed,
                    // comp.GetInstanceID() does NOT throw - it silently returns 0, a useless value -
                    // and the very next line (comp.gameObject) then throws NullReferenceException,
                    // which is what previously produced the "$error" alongside "instanceID: 0".
                    // PrefabAttribute caches instanceID/hierachyName/prefabID as plain managed fields
                    // at bake time (PreProcess), before any native destruction, so use those instead
                    // of touching native-backed members at all for this type.
                    o["instanceID"] = pa.instanceID;
                    o["prefabID"] = pa.prefabID;
                    o["path"] = StringPool.Get(pa.prefabID);
                    o["hierachyName"] = pa.hierachyName;
                }
                else if (uo is Component comp)
                {
                    o["instanceID"] = comp.GetInstanceID();
                    try { o["goName"] = comp.gameObject != null ? SafeUnityName(comp.gameObject) : null; }
                    catch (Exception ex) { o["goNameError"] = ex.Message; }
                    if (comp is BaseEntity be2)
                    {
                        o["prefabID"] = be2.prefabID;
                        o["resourcePath"] = StringPool.Get(be2.prefabID);
                    }
                }
                else
                {
                    o["instanceID"] = uo.GetInstanceID();
                }
            }
            catch (Exception ex)
            {
                o["$error"] = ex.Message;
            }
            return o;
        }

        // Mesh filenames need to be stable across runs: name + short hash of vertex data.
        private static string MeshFileId(Mesh mesh)
        {
            string safeName = string.IsNullOrEmpty(mesh.name) ? "mesh" : mesh.name;
            foreach (char c in Path.GetInvalidFileNameChars()) safeName = safeName.Replace(c, '_');
            uint hash = 2166136261;
            unchecked
            {
                foreach (char c in mesh.name ?? "")
                {
                    hash ^= c;
                    hash *= 16777619;
                }
                hash ^= (uint)mesh.GetInstanceID();
            }
            return $"{safeName}_{hash:x8}";
        }

        // Colliders expose their shape via native-backed C# *properties*, not fields, so the
        // generic reflection above finds nothing useful on them - handle them explicitly here,
        // as TASK.md requires. Same story for MeshFilter/MeshRenderer (visual mesh detection).
        private static void CaptureSpecialComponentData(object obj, DumpCtx ctx, JObject result)
        {
            try
            {
                if (obj is BoxCollider box)
                {
                    result["colliderType"] = "Box";
                    result["center"] = Serialize(box.center, ctx);
                    result["size"] = Serialize(box.size, ctx);
                    CommonCollider(box, ctx, result);
                }
                else if (obj is SphereCollider sph)
                {
                    result["colliderType"] = "Sphere";
                    result["center"] = Serialize(sph.center, ctx);
                    result["radius"] = (double)sph.radius;
                    CommonCollider(sph, ctx, result);
                }
                else if (obj is CapsuleCollider cap)
                {
                    result["colliderType"] = "Capsule";
                    result["center"] = Serialize(cap.center, ctx);
                    result["radius"] = (double)cap.radius;
                    result["height"] = (double)cap.height;
                    result["direction"] = cap.direction;
                    CommonCollider(cap, ctx, result);
                }
                else if (obj is MeshCollider mc)
                {
                    result["colliderType"] = "Mesh";
                    result["convex"] = mc.convex;
                    if (mc.sharedMesh != null)
                    {
                        result["sharedMesh"] = SerializeUnityRef(mc.sharedMesh, ctx);
                        ctx.PendingMeshes[mc.sharedMesh.GetInstanceID()] = mc.sharedMesh;
                    }
                    CommonCollider(mc, ctx, result);
                }
                else if (obj is TerrainCollider)
                {
                    result["colliderType"] = "Terrain";
                    CommonCollider((Collider)obj, ctx, result);
                }
                else if (obj is WheelCollider wc)
                {
                    result["colliderType"] = "Wheel";
                    result["center"] = Serialize(wc.center, ctx);
                    result["radius"] = (double)wc.radius;
                    result["enabled"] = wc.enabled;
                }
                else if (obj is MeshFilter mf && mf.sharedMesh != null)
                {
                    result["sharedMesh"] = SerializeUnityRef(mf.sharedMesh, ctx);
                    ctx.PendingVisualMeshes[mf.sharedMesh.GetInstanceID()] = mf.sharedMesh;
                }
                else if (obj is MeshRenderer mr)
                {
                    var mats = new JArray();
                    foreach (var mat in mr.sharedMaterials)
                        mats.Add(mat != null ? (JToken)SerializeUnityRef(mat, ctx) : JValue.CreateNull());
                    result["sharedMaterials"] = mats;
                    result["enabled"] = mr.enabled;
                }
            }
            catch (Exception ex)
            {
                result["$specialCaptureError"] = ex.Message;
            }
        }

        private static void CommonCollider(Collider col, DumpCtx ctx, JObject result)
        {
            result["isTrigger"] = col.isTrigger;
            result["enabled"] = col.enabled;
            if (col.sharedMaterial != null) result["physicMaterial"] = SerializeUnityRef(col.sharedMaterial, ctx);
        }

        // ------------------------------------------------------------------
        // items.json
        // ------------------------------------------------------------------

        private JArray DumpItems()
        {
            ItemManager.Initialize();
            var arr = new JArray();
            foreach (ItemDefinition def in ItemManager.itemList)
            {
                try
                {
                    var ctx = new DumpCtx { RootGameObject = def.gameObject, CurrentPrefabId = 0 };
                    var itemJson = (JObject)Serialize(def, ctx, 0, isRoot: true);

                    // ItemModDeployable is a separate MonoBehaviour on the item's GameObject, not
                    // an ItemMod, so it never shows up inside the itemMods field above.
                    var deployable = def.GetComponent<ItemModDeployable>();
                    itemJson["itemModDeployable"] = deployable != null
                        ? Serialize(deployable, ctx, 0, isRoot: true)
                        : JValue.CreateNull();

                    var extra = new JArray();
                    foreach (Component c in def.GetComponentsInChildren<Component>(true))
                    {
                        if (c == null || c is Transform || c is ItemMod || c is ItemModDeployable) continue;
                        if (ReferenceEquals(c, def)) continue;
                        try { extra.Add(Serialize(c, ctx, 0, isRoot: true)); }
                        catch (Exception ex) { Failures.Add($"item {def.shortname} extraComponent {c.GetType().FullName}: {ex.Message}"); }
                    }
                    itemJson["extraComponents"] = extra;

                    arr.Add(itemJson);
                }
                catch (Exception ex)
                {
                    Failures.Add($"item {def?.shortname}: {ex}");
                }
            }
            return arr;
        }

        // ------------------------------------------------------------------
        // prefabs/<prefabID>.json
        // ------------------------------------------------------------------

        private HashSet<uint> CollectRelevantPrefabIds()
        {
            var ids = new HashSet<uint>();

            // every prefab that has any baked PrefabAttribute (Construction, Socket_*, SocketMod*,
            // DeployVolume*, Deployable, ConstructionSkin, ...) - this is the authoritative set
            // per TASK.md's own premise about server-side prefab preprocessing.
            foreach (uint id in PrefabAttribute.server.prefabs.Keys) ids.Add(id);

            // plus every entityPrefab referenced by an ItemModDeployable, in case an item points
            // at a prefab that (for whatever reason) registered no PrefabAttribute of its own.
            ItemManager.Initialize();
            foreach (ItemDefinition def in ItemManager.itemList)
            {
                var deployable = def.GetComponent<ItemModDeployable>();
                if (deployable?.entityPrefab != null && deployable.entityPrefab.isValid)
                {
                    uint id = deployable.entityPrefab.resourceID;
                    if (id != 0) ids.Add(id);
                }
            }
            return ids;
        }

        private void DumpPrefab(uint prefabId, DumpCtx sharedCtx)
        {
            string path = Path.Combine(DataRoot, "prefabs", $"{prefabId}.json");
            try
            {
                GameObject root = GameManager.server.FindPrefab(StringPool.Get(prefabId));
                var result = new JObject
                {
                    ["prefabID"] = prefabId,
                    ["path"] = StringPool.Get(prefabId)
                };

                if (root == null)
                {
                    result["$error"] = "GameManager.server.FindPrefab returned null";
                    Failures.Add($"prefab {prefabId} ({StringPool.Get(prefabId)}): FindPrefab returned null");
                }
                else
                {
                    BaseEntity rootEntity = root.GetComponent<BaseEntity>();
                    result["rootClassChain"] = ClassChain(root.GetType());
                    if (rootEntity != null)
                    {
                        result["rootClassChain"] = ClassChain(rootEntity.GetType());
                        try { result["entityBounds"] = Serialize(rootEntity.bounds, sharedCtx); }
                        catch (Exception ex) { Failures.Add($"prefab {prefabId} entityBounds: {ex.Message}"); }
                    }

                    var ctx = new DumpCtx { RootGameObject = root, CurrentPrefabId = prefabId };
                    result["hierarchy"] = DumpHierarchy(root.transform, ctx);
                    MergeCtx(ctx, sharedCtx);

                    var attrCtx = new DumpCtx { RootGameObject = root, CurrentPrefabId = prefabId };
                    result["prefabAttributes"] = DumpAllAttributes(prefabId, attrCtx);
                    MergeCtx(attrCtx, sharedCtx);
                }

                WriteJson(path, result);
            }
            catch (Exception ex)
            {
                Failures.Add($"prefab {prefabId}: {ex}");
            }
        }

        private static void MergeCtx(DumpCtx from, DumpCtx into)
        {
            foreach (var kv in from.PendingMeshes) into.PendingMeshes[kv.Key] = kv.Value;
            foreach (var kv in from.PendingVisualMeshes) into.PendingVisualMeshes[kv.Key] = kv.Value;
            foreach (uint id in from.DiscoveredPrefabIds) into.DiscoveredPrefabIds.Add(id);
        }

        private static JArray ClassChain(Type t)
        {
            var arr = new JArray();
            for (Type cur = t; cur != null; cur = cur.BaseType) arr.Add(cur.FullName);
            return arr;
        }

        private static JObject DumpHierarchy(Transform node, DumpCtx ctx)
        {
            var o = new JObject
            {
                ["name"] = SafeUnityName(node.gameObject),
                ["layer"] = new JObject { ["index"] = node.gameObject.layer, ["name"] = LayerMask.LayerToName(node.gameObject.layer) },
                ["tag"] = SafeTag(node.gameObject),
                ["customTags"] = SafeCustomTags(node.gameObject),
                ["activeSelf"] = node.gameObject.activeSelf,
                ["localPosition"] = Serialize(node.localPosition, ctx),
                ["localRotation"] = Serialize(node.localRotation, ctx),
                ["localScale"] = Serialize(node.localScale, ctx)
            };

            var comps = new JArray();
            foreach (Component c in node.GetComponents<Component>())
            {
                if (c == null) continue;
                if (c is Transform) continue; // structure already captured above
                try { comps.Add(Serialize(c, ctx, 0, isRoot: true)); }
                catch (Exception ex) { Failures.Add($"component {c.GetType().FullName} on {node.name}: {ex.Message}"); }
            }
            o["components"] = comps;

            var children = new JArray();
            foreach (Transform child in node)
                children.Add(DumpHierarchy(child, ctx));
            o["children"] = children;

            return o;
        }

        private static string SafeTag(GameObject go)
        {
            try { return go.tag; } catch { return null; }
        }

        // GameObjectTag is a second, independent tagging system layered on top of Unity's builtin
        // string tag (see TagComponentEx.HasCustomTag) - DeployVolume checks GameObjectTag.BlockPlacement,
        // SocketMod_RoadCheck checks .Road, etc. Report every flag value that's actually set on this node.
        private static JArray SafeCustomTags(GameObject go)
        {
            var arr = new JArray();
            try
            {
                foreach (GameObjectTag tag in Enum.GetValues(typeof(GameObjectTag)))
                {
                    if (go.HasCustomTag(tag)) arr.Add(tag.ToString());
                }
            }
            catch (Exception ex)
            {
                Failures.Add($"customTags on {go?.name}: {ex.Message}");
            }
            return arr;
        }

        // Reflects PrefabAttribute.AttributeCollection's private `attributes` dictionary so we
        // enumerate every registered type without hardcoding a list (see TASK.md 2.1 "universal").
        private static FieldInfo _attributeCollectionField;

        private static JObject DumpAllAttributes(uint prefabId, DumpCtx ctx)
        {
            var result = new JObject();
            if (!PrefabAttribute.server.prefabs.TryGetValue(prefabId, out var collection))
                return result;

            _attributeCollectionField ??= typeof(PrefabAttribute.AttributeCollection).GetField("attributes", BindingFlags.NonPublic | BindingFlags.Instance);
            if (_attributeCollectionField == null)
            {
                Failures.Add("DumpAllAttributes: could not reflect AttributeCollection.attributes field");
                return result;
            }

            var byType = _attributeCollectionField.GetValue(collection) as IDictionary;
            if (byType == null) return result;

            foreach (DictionaryEntry entry in byType)
            {
                Type type = (Type)entry.Key;
                var listArr = new JArray();
                foreach (object attr in (IEnumerable)entry.Value)
                {
                    try { listArr.Add(Serialize(attr, ctx, 0, isRoot: true)); }
                    catch (Exception ex) { Failures.Add($"attribute {type.FullName} on prefab {prefabId}: {ex.Message}"); }
                }
                result[type.FullName] = listArr;
            }
            return result;
        }

        // ------------------------------------------------------------------
        // meshes/ + meshes_visual/
        // ------------------------------------------------------------------

        private void WriteMeshes(Dictionary<int, Mesh> meshes, string subfolder)
        {
            string dir = Path.Combine(DataRoot, subfolder);
            EnsureDir(dir);
            var unreadable = new List<string>();
            foreach (Mesh mesh in meshes.Values)
            {
                if (mesh == null) continue;
                string id = MeshFileId(mesh);
                string path = Path.Combine(dir, id + ".obj");
                if (File.Exists(path)) continue;
                try
                {
                    string obj = mesh.isReadable ? MeshToObj(mesh) : null;
                    if (obj == null && TryExportUnreadableMesh(mesh, out obj)) { /* got it via AcquireReadOnlyMeshData */ }
                    if (obj == null)
                    {
                        unreadable.Add($"{mesh.name} ({id})");
                        continue;
                    }
                    File.WriteAllText(path, obj, new UTF8Encoding(false));
                }
                catch (Exception ex)
                {
                    Failures.Add($"mesh export {mesh.name}: {ex.Message}");
                }
            }
            if (unreadable.Count > 0)
                Failures.Add($"{subfolder}: {unreadable.Count} unreadable meshes could not be exported: " + string.Join(", ", unreadable.Take(50)));
        }

        private static string MeshToObj(Mesh mesh)
        {
            var sb = new StringBuilder();
            sb.Append("# ").Append(mesh.name).Append('\n');
            Vector3[] verts = mesh.vertices;
            foreach (Vector3 v in verts)
                sb.Append("v ").Append(v.x.ToString("G9", CultureInfo.InvariantCulture)).Append(' ')
                  .Append(v.y.ToString("G9", CultureInfo.InvariantCulture)).Append(' ')
                  .Append(v.z.ToString("G9", CultureInfo.InvariantCulture)).Append('\n');
            Vector3[] normals = mesh.normals;
            foreach (Vector3 n in normals)
                sb.Append("vn ").Append(n.x.ToString("G9", CultureInfo.InvariantCulture)).Append(' ')
                  .Append(n.y.ToString("G9", CultureInfo.InvariantCulture)).Append(' ')
                  .Append(n.z.ToString("G9", CultureInfo.InvariantCulture)).Append('\n');
            Vector2[] uvs = mesh.uv;
            foreach (Vector2 uv in uvs)
                sb.Append("vt ").Append(uv.x.ToString("G9", CultureInfo.InvariantCulture)).Append(' ')
                  .Append(uv.y.ToString("G9", CultureInfo.InvariantCulture)).Append('\n');

            bool hasN = normals.Length > 0, hasUv = uvs.Length > 0;
            string Idx(int i1) => hasN && hasUv ? $"{i1}/{i1}/{i1}" : hasUv ? $"{i1}/{i1}" : hasN ? $"{i1}//{i1}" : $"{i1}";

            for (int sub = 0; sub < mesh.subMeshCount; sub++)
            {
                int[] tris = mesh.GetTriangles(sub);
                sb.Append("g submesh").Append(sub).Append('\n');
                for (int i = 0; i < tris.Length; i += 3)
                    sb.Append("f ").Append(Idx(tris[i] + 1)).Append(' ').Append(Idx(tris[i + 1] + 1)).Append(' ').Append(Idx(tris[i + 2] + 1)).Append('\n');
            }
            return sb.ToString();
        }

        private static bool TryExportUnreadableMesh(Mesh mesh, out string obj)
        {
            obj = null;
            Mesh.MeshDataArray dataArray = default;
            try
            {
                dataArray = Mesh.AcquireReadOnlyMeshData(mesh);
                if (dataArray.Length == 0) return false;
                Mesh.MeshData data = dataArray[0];

                var verts = new NativeArray<Vector3>(data.vertexCount, Allocator.Temp);
                data.GetVertices(verts);

                var sb = new StringBuilder();
                sb.Append("# ").Append(mesh.name).Append(" (via AcquireReadOnlyMeshData)\n");
                foreach (Vector3 v in verts)
                    sb.Append("v ").Append(v.x.ToString("G9", CultureInfo.InvariantCulture)).Append(' ')
                      .Append(v.y.ToString("G9", CultureInfo.InvariantCulture)).Append(' ')
                      .Append(v.z.ToString("G9", CultureInfo.InvariantCulture)).Append('\n');

                for (int sm = 0; sm < data.subMeshCount; sm++)
                {
                    UnityEngine.Rendering.SubMeshDescriptor subMesh = data.GetSubMesh(sm);
                    var idx = new NativeArray<int>(subMesh.indexCount, Allocator.Temp);
                    data.GetIndices(idx, sm);
                    sb.Append("g submesh").Append(sm).Append('\n');
                    for (int i = 0; i + 2 < idx.Length; i += 3)
                        sb.Append("f ").Append(idx[i] + 1).Append(' ').Append(idx[i + 1] + 1).Append(' ').Append(idx[i + 2] + 1).Append('\n');
                    idx.Dispose();
                }
                verts.Dispose();
                obj = sb.ToString();
                return true;
            }
            catch (Exception ex)
            {
                Failures.Add($"AcquireReadOnlyMeshData({mesh.name}): {ex.Message}");
                return false;
            }
            finally
            {
                try { dataArray.Dispose(); } catch { }
            }
        }

        // ------------------------------------------------------------------
        // layers.json
        // ------------------------------------------------------------------

        private JObject DumpLayers()
        {
            var result = new JObject();
            var names = new JObject();
            for (int i = 0; i < 32; i++) names[i.ToString()] = LayerMask.LayerToName(i);
            result["names"] = names;

            var matrix = new JArray();
            for (int a = 0; a < 32; a++)
            {
                var row = new JArray();
                for (int b = 0; b < 32; b++)
                    row.Add(!Physics.GetIgnoreLayerCollision(a, b));
                matrix.Add(row);
            }
            result["collisionMatrix_note"] = "matrix[a][b] == true means a and b DO collide (i.e. NOT ignored)";
            result["collisionMatrix"] = matrix;

            result["masks"] = DumpStaticConstClasses(typeof(Rust.Layers));
            return result;
        }

        private static JObject DumpStaticConstClasses(Type root)
        {
            var o = new JObject();
            foreach (FieldInfo fi in root.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                try { o[fi.Name] = Convert.ToInt64(fi.GetValue(null)); }
                catch (Exception ex) { Failures.Add($"layers const {root.FullName}.{fi.Name}: {ex.Message}"); }
            }
            foreach (Type nested in root.GetNestedTypes(BindingFlags.Public))
                o[nested.Name] = DumpStaticConstClasses(nested);
            return o;
        }

        // ------------------------------------------------------------------
        // convars.json
        // ------------------------------------------------------------------

        private static readonly string[] ConVarKeywords = { "stability", "decay", "build", "construct", "deploy", "place", "socket", "upkeep", "server.max", "antihack" };

        private JArray DumpConVars()
        {
            var arr = new JArray();
            IEnumerable<Type> conVarTypes;
            try
            {
                conVarTypes = typeof(ConVar.Server).Assembly.GetTypes().Where(t => t.Namespace == "ConVar");
            }
            catch (Exception ex)
            {
                Failures.Add("DumpConVars: could not enumerate ConVar namespace: " + ex.Message);
                return arr;
            }

            foreach (Type type in conVarTypes)
            {
                string category = type.Name.ToLowerInvariant();
                foreach (FieldInfo fi in type.GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    object[] attrs = fi.GetCustomAttributes(false);
                    // The real attribute classes here (ServerVar/ReplicatedVar/ClientVar, defined in
                    // Facepunch.Console.dll - not part of the decompiled set) do NOT carry the usual
                    // "Attribute" suffix, confirmed live via reflection (GetType().FullName == "ServerVar").
                    object varAttr = attrs.FirstOrDefault(a => a.GetType().Name.EndsWith("Var"));
                    if (varAttr == null) continue;

                    string full = $"{category}.{fi.Name}";
                    bool matches = ConVarKeywords.Any(k => full.Contains(k));
                    if (!matches) continue;

                    string help = null;
                    try
                    {
                        Type at = varAttr.GetType();
                        help = (at.GetProperty("Help")?.GetValue(varAttr)
                            ?? at.GetField("Help")?.GetValue(varAttr)) as string;
                    }
                    catch { }

                    object val = null;
                    try { val = fi.GetValue(null); } catch (Exception ex) { Failures.Add($"convar {full}: {ex.Message}"); }

                    arr.Add(new JObject
                    {
                        ["name"] = full,
                        ["category"] = category,
                        ["field"] = fi.Name,
                        ["kind"] = varAttr.GetType().Name,
                        ["type"] = fi.FieldType.Name,
                        ["defaultValue"] = val == null ? JValue.CreateNull() : Serialize(val, new DumpCtx()),
                        ["help"] = help
                    });
                }
            }
            return arr;
        }

        // ------------------------------------------------------------------
        // player.json
        // ------------------------------------------------------------------

        private JObject DumpPlayer()
        {
            var o = new JObject
            {
                ["radius"] = (double)BasePlayer.GetRadius(),
                ["heightStanding"] = (double)BasePlayer.GetHeight(false),
                ["heightDucked"] = (double)BasePlayer.GetHeight(true),
                ["sizeStanding"] = Serialize(BasePlayer.GetSize(false), new DumpCtx()),
                ["sizeDucked"] = Serialize(BasePlayer.GetSize(true), new DumpCtx()),
                ["offsetStanding"] = Serialize(BasePlayer.GetOffset(false), new DumpCtx()),
                ["offsetDucked"] = Serialize(BasePlayer.GetOffset(true), new DumpCtx()),
                ["eyeOffset"] = Serialize(PlayerEyes.EyeOffset, new DumpCtx())
            };

            BasePlayer any = BasePlayer.activePlayerList.Count > 0 ? BasePlayer.activePlayerList[0] : null;
            if (any != null)
            {
                o["realCapsuleStanding"] = Serialize(any.playerColliderStanding, new DumpCtx());
                o["realCapsuleDucked"] = Serialize(any.playerColliderDucked, new DumpCtx());
                o["realCapsuleCrawling"] = Serialize(any.playerColliderCrawling, new DumpCtx());
                o["realCapsuleLyingDown"] = Serialize(any.playerColliderLyingDown, new DumpCtx());
            }
            else
            {
                Failures.Add("player.json: no connected player, real playerCollider.* CapsuleColliderInfo values unavailable (only the static gameplay-logic capsule was dumped)");
            }

            o["maxPlaceDistanceExampleWall"] = 4.0; // Construction.maxplaceDistance is per-prefab, see prefabs/*.json
            return o;
        }

        // ------------------------------------------------------------------
        // builddump command
        // ------------------------------------------------------------------

        [ConsoleCommand("builddump")]
        private void CmdBuildDump(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null && arg.Connection.authLevel < 2)
            {
                arg.ReplyWith("No permission");
                return;
            }

            Failures.Clear();
            var sw = System.Diagnostics.Stopwatch.StartNew();
            arg.ReplyWith("builddump: starting, this can take a while...");
            Puts("builddump: starting");

            try
            {
                EnsureDir(DataRoot);

                Puts("builddump: items.json");
                WriteJson(Path.Combine(DataRoot, "items.json"), DumpItems());

                Puts("builddump: prefabs/*.json");
                var sharedCtx = new DumpCtx();
                HashSet<uint> ids = CollectRelevantPrefabIds();
                var dumped = new HashSet<uint>();
                int done = 0;
                // Wave 0+: dump every known prefab, then follow any GameObjectRef discovered while
                // serializing (grade skins via ConstructionGrade.skinObject, corner/step/edge models
                // via ConditionalModel.prefab, etc. - see TASK.md 2.3) until no new prefabs turn up.
                var frontier = new List<uint>(ids);
                int wave = 0;
                while (frontier.Count > 0)
                {
                    wave++;
                    Puts($"builddump: prefabs wave {wave}, {frontier.Count} to dump");
                    var next = new List<uint>();
                    foreach (uint id in frontier)
                    {
                        if (!dumped.Add(id)) continue;
                        DumpPrefab(id, sharedCtx);
                        done++;
                        if (done % 200 == 0) Puts($"builddump: prefabs {done} dumped so far");
                    }
                    foreach (uint id in sharedCtx.DiscoveredPrefabIds)
                        if (!dumped.Contains(id)) next.Add(id);
                    sharedCtx.DiscoveredPrefabIds.Clear();
                    frontier = next;
                }
                ids = dumped;

                Puts($"builddump: meshes ({sharedCtx.PendingMeshes.Count} collider meshes, {sharedCtx.PendingVisualMeshes.Count} visual meshes)");
                WriteMeshes(sharedCtx.PendingMeshes, "meshes");
                if (sharedCtx.PendingVisualMeshes.Count > 0)
                    WriteMeshes(sharedCtx.PendingVisualMeshes, "meshes_visual");
                else
                    Failures.Add("no MeshFilter/MeshRenderer visual meshes found on any dumped server prefab (expected - server builds usually strip visuals)");

                Puts("builddump: layers.json");
                WriteJson(Path.Combine(DataRoot, "layers.json"), DumpLayers());

                Puts("builddump: convars.json");
                WriteJson(Path.Combine(DataRoot, "convars.json"), DumpConVars());

                Puts("builddump: player.json");
                WriteJson(Path.Combine(DataRoot, "player.json"), DumpPlayer());

                sw.Stop();
                var manifest = new JObject
                {
                    ["buildid"] = GetBuildId(),
                    ["dumpedAtUtc"] = DateTime.UtcNow.ToString("o"),
                    ["dumperVersion"] = "0.1.0",
                    ["serverVersion"] = Rust.Protocol.network.ToString(),
                    ["prefabCount"] = ids.Count,
                    ["itemCount"] = ItemManager.itemList?.Count ?? 0,
                    ["meshCount"] = sharedCtx.PendingMeshes.Count,
                    ["visualMeshCount"] = sharedCtx.PendingVisualMeshes.Count,
                    ["elapsedSeconds"] = sw.Elapsed.TotalSeconds,
                    ["failures"] = new JArray(Failures)
                };
                WriteJson(Path.Combine(DataRoot, "manifest.json"), manifest);

                string msg = $"builddump: done in {sw.Elapsed.TotalSeconds:0.0}s - {ids.Count} prefabs, {ItemManager.itemList?.Count ?? 0} items, {sharedCtx.PendingMeshes.Count} meshes, {Failures.Count} failures. Output: {DataRoot}";
                Puts(msg);
                arg.ReplyWith(msg);
            }
            catch (Exception ex)
            {
                Puts("builddump: FAILED - " + ex);
                arg.ReplyWith("builddump: FAILED - " + ex.Message);
            }
        }

        // ------------------------------------------------------------------
        // basedump <radius> / baseload <file>   (Этап 3 - golden tests)
        // ------------------------------------------------------------------

        [ConsoleCommand("basedump")]
        private void CmdBaseDump(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null && arg.Connection.authLevel < 2) { arg.ReplyWith("No permission"); return; }
            BasePlayer player = arg.Player();
            if (player == null) { arg.ReplyWith("basedump must be run by a player (radius is centered on you)"); return; }

            float radius = arg.GetFloat(0, 50f);
            var ctx = new DumpCtx();
            var entities = new JArray();
            int count = 0;
            foreach (BaseNetworkable net in BaseNetworkable.serverEntities)
            {
                var ent = net as BaseEntity;
                if (ent == null) continue;
                if (Vector3.Distance(ent.transform.position, player.transform.position) > radius) continue;

                var e = new JObject
                {
                    ["prefabID"] = ent.prefabID,
                    ["path"] = StringPool.Get(ent.prefabID),
                    ["position"] = Serialize(ent.transform.position, ctx),
                    ["rotation"] = Serialize(ent.transform.rotation, ctx),
                    ["skinID"] = ent.skinID
                };

                if (ent is BuildingBlock bb)
                {
                    e["grade"] = Serialize(bb.grade, ctx);
                }
                if (ent is StabilityEntity se)
                {
                    e["cachedStability"] = se.cachedStability;
                }
                if (ent is DecayEntity de)
                {
                    e["buildingID"] = de.buildingID;
                }
                if (ent is BaseCombatEntity bce)
                {
                    e["health"] = bce.health;
                    e["maxHealth"] = bce.MaxHealth();
                }
                try
                {
                    BaseEntity parent = ent.GetParentEntity();
                    if (parent != null) e["parentEntityNetId"] = parent.net?.ID.Value;
                }
                catch { }

                e["flags"] = Serialize(ent.flags, ctx);
                e["netId"] = ent.net?.ID.Value;
                e["raw"] = Serialize(ent, new DumpCtx { CurrentPrefabId = 0 }, 0, isRoot: true);

                entities.Add(e);
                count++;
            }

            var result = new JObject
            {
                ["center"] = Serialize(player.transform.position, ctx),
                ["radius"] = radius,
                ["dumpedAtUtc"] = DateTime.UtcNow.ToString("o"),
                ["entityCount"] = count,
                ["entities"] = entities
            };

            string dir = Path.Combine(DataRoot, "golden");
            EnsureDir(dir);
            string file = Path.Combine(dir, $"basedump_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json");
            WriteJson(file, result);
            arg.ReplyWith($"basedump: {count} entities within {radius}m written to {file}");
        }

        [ConsoleCommand("baseload")]
        private void CmdBaseLoad(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null && arg.Connection.authLevel < 2) { arg.ReplyWith("No permission"); return; }
            string file = arg.GetString(0, null);
            if (string.IsNullOrEmpty(file)) { arg.ReplyWith("Usage: baseload <path-to-basedump.json>"); return; }
            if (!File.Exists(file)) file = Path.Combine(DataRoot, "golden", file);
            if (!File.Exists(file)) { arg.ReplyWith("File not found: " + file); return; }

            JObject data;
            try { data = JObject.Parse(File.ReadAllText(file)); }
            catch (Exception ex) { arg.ReplyWith("Failed to parse file: " + ex.Message); return; }

            int spawned = 0, failed = 0;
            foreach (JObject e in data["entities"].Children<JObject>())
            {
                try
                {
                    string path = e["path"]?.Value<string>();
                    if (string.IsNullOrEmpty(path)) { failed++; continue; }

                    JObject pos = (JObject)e["position"];
                    JObject rot = (JObject)e["rotation"];
                    Vector3 position = new Vector3(pos["x"].Value<float>(), pos["y"].Value<float>(), pos["z"].Value<float>());
                    Quaternion rotation = new Quaternion(rot["x"].Value<float>(), rot["y"].Value<float>(), rot["z"].Value<float>(), rot["w"].Value<float>());

                    GameObject go = GameManager.server.CreatePrefab(path, position, rotation, active: true);
                    BaseEntity ent = go != null ? GameObjectEx.ToBaseEntity(go) : null;
                    if (ent == null) { failed++; continue; }

                    if (e["skinID"] != null) ent.skinID = e["skinID"].Value<ulong>();

                    if (ent is BuildingBlock bb && e["grade"] != null)
                    {
                        var gradeName = e["grade"]["name"]?.Value<string>();
                        if (Enum.TryParse(gradeName, out BuildingGrade.Enum gradeEnum))
                            bb.SetGrade(gradeEnum);
                    }

                    ent.Spawn();
                    if (ent is BaseCombatEntity bce && e["health"] != null)
                        bce.SetHealth(e["health"].Value<float>());

                    spawned++;
                }
                catch (Exception ex)
                {
                    failed++;
                    Failures.Add("baseload entity: " + ex.Message);
                }
            }

            // Note: this does not attempt to re-link building-block sockets/parenting - Rust's
            // building system reconnects adjacent building blocks by proximity/socket search on
            // spawn in most cases, but grief-protection/upkeep bracket state and explicit
            // parent-entity relationships (e.g. deployables on shelves) are NOT restored here.
            // Good enough to re-place a base shape for a visual/mechanical spot check; not a
            // guaranteed bit-for-bit restore of the original save state.
            arg.ReplyWith($"baseload: spawned {spawned}, failed {failed} (see server log for details). Socket/parent relinking is best-effort only.");
        }
    }
}
