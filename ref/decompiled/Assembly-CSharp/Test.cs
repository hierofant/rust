using PoolPhysics;
using UnityEngine;

public class Test : MonoBehaviour
{
	[Header("Table")]
	public float TableWidth = 2.54f;

	public float TableHeight = 1.27f;

	[Header("Ball")]
	public float BallRadius = 0.02875f;

	private Engine poolEngine;

	private void Start()
	{
		poolEngine = new Engine();
		float num = TableWidth * 0.5f;
		float num2 = TableHeight * 0.5f;
		float num3 = BallRadius * 2f;
		poolEngine.AddWall(MakeWall(new Vector2(0f - num + num3, 0f - num2), new Vector2(0f - num3, 0f - num2)));
		poolEngine.AddWall(MakeWall(new Vector2(num3, 0f - num2), new Vector2(num - num3, 0f - num2)));
		poolEngine.AddWall(MakeWall(new Vector2(0f - num + num3, num2), new Vector2(0f - num3, num2)));
		poolEngine.AddWall(MakeWall(new Vector2(num3, num2), new Vector2(num - num3, num2)));
		poolEngine.AddWall(MakeWall(new Vector2(0f - num, 0f - num2 + num3), new Vector2(0f - num, num2 - num3)));
		poolEngine.AddWall(MakeWall(new Vector2(num, 0f - num2 + num3), new Vector2(num, num2 - num3)));
		Data.Ball ball = default(Data.Ball);
		ball.Id = 0;
		ball.Position = new Vector2(-1f, 0f);
		ball.Velocity = new Vector2(1f, 0f);
		ball.Radius = BallRadius;
		ball.IsKinematic = false;
		Data.Ball ball2 = ball;
		ball = default(Data.Ball);
		ball.Id = 1;
		ball.Position = new Vector2(-1f, 0f);
		ball.Velocity = new Vector2(0f, 0f);
		ball.Radius = BallRadius;
		ball.IsKinematic = false;
		Data.Ball ball3 = ball;
		ball = default(Data.Ball);
		ball.Id = 2;
		ball.Position = new Vector2(0.5f, 0f);
		ball.Velocity = new Vector2(0f, 0f);
		ball.Radius = BallRadius;
		ball.IsKinematic = false;
		Data.Ball ball4 = ball;
		ball = default(Data.Ball);
		ball.Id = 3;
		ball.Position = new Vector2(-0.5f, 0f);
		ball.Velocity = new Vector2(0f, 0f);
		ball.Radius = BallRadius;
		ball.IsKinematic = false;
		Data.Ball ball5 = ball;
		ball = default(Data.Ball);
		ball.Id = 4;
		ball.Position = new Vector2(0f, 0.5f);
		ball.Velocity = new Vector2(0f, 0f);
		ball.Radius = BallRadius;
		ball.IsKinematic = false;
		Data.Ball ball6 = ball;
		poolEngine.AddBall(ball2);
		poolEngine.AddBall(ball3);
		poolEngine.AddBall(ball4);
		poolEngine.AddBall(ball5);
		poolEngine.AddBall(ball6);
		poolEngine.ApplyForce(0, new Vector2(5f, 0f));
		poolEngine.ApplyForce(0, new Vector2(-5f, -2f));
	}

	private void Update()
	{
		if (poolEngine.IsReady)
		{
			poolEngine.Tick(Time.deltaTime);
		}
	}

	private void OnDrawGizmos()
	{
		float num = TableWidth * 0.5f;
		float num2 = TableHeight * 0.5f;
		Gizmos.color = Color.green;
		Vector3 vector = new Vector3(0f - num, 0f, 0f - num2);
		Vector3 vector2 = new Vector3(num, 0f, 0f - num2);
		Vector3 vector3 = new Vector3(0f - num, 0f, num2);
		Vector3 vector4 = new Vector3(num, 0f, num2);
		Gizmos.DrawLine(vector, vector2);
		Gizmos.DrawLine(vector2, vector4);
		Gizmos.DrawLine(vector4, vector3);
		Gizmos.DrawLine(vector3, vector);
		if (poolEngine == null)
		{
			return;
		}
		foreach (Data.Ball ball in poolEngine.Balls)
		{
			Gizmos.color = ((ball.Id == 0) ? Color.white : Color.yellow);
			Gizmos.DrawWireSphere(new Vector3(ball.Position.x, 0f, ball.Position.y), ball.Radius);
		}
		Gizmos.color = Color.red;
		foreach (Data.Wall wall in poolEngine.Walls)
		{
			Vector3 from = new Vector3(wall.A.x, 0f, wall.A.y);
			Vector3 to = new Vector3(wall.B.x, 0f, wall.B.y);
			Gizmos.DrawLine(from, to);
		}
	}

	private Data.Wall MakeWall(Vector2 a, Vector2 b)
	{
		Vector2 vector = b - a;
		Vector2 normalized = new Vector2(0f - vector.y, vector.x).normalized;
		Data.Wall result = default(Data.Wall);
		result.A = a;
		result.B = b;
		result.Normal = normalized;
		return result;
	}
}
