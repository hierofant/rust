using UnityEngine;

[ExecuteAlways]
public class FakePhysicsRope : FacepunchBehaviour, IClientComponent
{
	public enum RenderMode
	{
		LineRenderer2D,
		TubeRenderer3D,
		Both
	}

	[Header("References")]
	public Transform startPoint;

	public Transform endPoint;

	public Transform leadPoint;

	public Vector3 endPointOffset;

	[Range(2f, 100f)]
	[Header("Settings")]
	public int linePoints = 10;

	[Tooltip("Value highly dependent on use case, a metal cable would have high stiffness, a rubber rope would have a low one")]
	public float stiffness = 350f;

	[Tooltip("0 is no damping, 50 is a lot")]
	public float damping = 15f;

	[Tooltip("How long is the rope. It will hang more or less from starting point to end point depending on this value")]
	public float ropeLength = 15f;

	[Tooltip("The Rope width set at start (changing this value during run time will produce no effect)")]
	public float ropeWidth = 0.1f;

	[Tooltip("Adjust the middle control point weight for the Rational Bezier curve")]
	[Range(1f, 15f)]
	public float midPointWeight = 1f;

	[Tooltip("Use local positions instead of world positions (relative to this object)")]
	public bool useLocalPositions;

	[Header("Rendering")]
	public RenderMode renderMode;

	[Header("Wind")]
	public bool AddFakeWind;

	public float windFrequency;

	public float windAmplitude;

	protected Vector3 EndPointPosition => endPoint.position + endPointOffset;

	public static Vector3 GetRationalBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t, float w0 = 1f, float w1 = 1f, float w2 = 1f)
	{
		Vector3 vector = w0 * p0;
		Vector3 vector2 = w1 * p1;
		Vector3 vector3 = w2 * p2;
		float num = w0 * Mathf.Pow(1f - t, 2f) + 2f * w1 * (1f - t) * t + w2 * Mathf.Pow(t, 2f);
		return (vector * Mathf.Pow(1f - t, 2f) + vector2 * 2f * (1f - t) * t + vector3 * Mathf.Pow(t, 2f)) / num;
	}
}
