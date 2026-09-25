using UnityEngine;

[CreateAssetMenu(fileName = "MannequinPose", menuName = "MannequinPose")]
public class MannequinPose : BaseScriptableObject
{
	public Quaternion[] BoneRotations;
}
