using UnityEngine;

public class HexagonTile : BaseCombatEntity, IDetector
{
	public GameObject[] variants;

	public AnimationCurve tweenCurve;

	private MeshRenderer mesh;

	public bool ShouldTrigger()
	{
		return true;
	}

	public void OnObjects()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Busy, b: true);
	}

	public void OnObjectAdded(GameObject obj, Collider col)
	{
	}

	public void OnEmpty()
	{
	}

	public override void OnDied(HitInfo info)
	{
		base.OnDied(info);
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Busy, b: true);
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if ((old & Flags.Busy) == Flags.Busy != ((next & Flags.Busy) == Flags.Busy) && base.isServer)
		{
			Invoke(delegate
			{
				Kill();
			}, 1.2f);
		}
	}
}
