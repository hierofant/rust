using System;
using System.Collections.Generic;
using UnityEngine;

public class RainSurfaceAmbience : SingletonComponent<RainSurfaceAmbience>, IClientComponent
{
	[Serializable]
	public class SurfaceSound
	{
		public AmbienceDefinitionList baseAmbience;

		public List<PhysicsMaterial> materials = new List<PhysicsMaterial>();
	}

	public List<SurfaceSound> surfaces = new List<SurfaceSound>();

	public GameObjectRef emitterPrefab;

	public Dictionary<ParticlePatch, AmbienceEmitter> spawnedEmitters = new Dictionary<ParticlePatch, AmbienceEmitter>();
}
