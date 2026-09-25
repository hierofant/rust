using UnityEngine;

public interface IRFObject
{
	int GetFrequency();

	Vector3 GetPosition();

	float GetMaxRange();

	void RFSignalUpdate(bool on);
}
