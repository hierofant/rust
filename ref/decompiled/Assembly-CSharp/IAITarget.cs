using UnityEngine;

public interface IAITarget
{
	Vector3? Position { get; }

	bool IsValid(BoatAI self);

	bool IsReached(BoatAI self);
}
