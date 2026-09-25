using System;
using UnityEngine;

namespace Rust.Rendering.IndirectInstancing;

internal struct Frustum
{
	public Plane left;

	public Plane right;

	public Plane down;

	public Plane up;

	public Plane near;

	public Plane far;

	[ThreadStatic]
	private static Plane[] reusable_plane_array;

	public Frustum(Camera camera)
	{
		GeometryUtility.CalculateFrustumPlanes(camera, reusable_plane_array);
		left = reusable_plane_array[0];
		right = reusable_plane_array[1];
		down = reusable_plane_array[2];
		up = reusable_plane_array[3];
		near = reusable_plane_array[4];
		far = reusable_plane_array[5];
	}

	static Frustum()
	{
		reusable_plane_array = new Plane[6];
	}

	public static implicit operator Rust.Rendering.IndirectInstancing.Frustum(Camera camera)
	{
		return new Rust.Rendering.IndirectInstancing.Frustum(camera);
	}
}
