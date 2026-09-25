using UnityEngine;

namespace Rust.Ai.Gen2;

public class RustNavmeshModifierVolume : MonoBehaviour, IServerComponent
{
	public static SparseGrid<RustNavmeshModifierVolume> AllModifierVolumes = new SparseGrid<RustNavmeshModifierVolume>();

	private void Awake()
	{
		AllModifierVolumes.Add(base.transform.position, this);
	}

	private void OnDestroy()
	{
		AllModifierVolumes.Remove(base.transform.position, this);
	}
}
