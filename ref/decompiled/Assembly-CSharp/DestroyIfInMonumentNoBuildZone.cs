using UnityEngine;

public class DestroyIfInMonumentNoBuildZone : MonoBehaviour
{
	protected void Start()
	{
		if (ConstructionErrors.IsBuildBlockedByMonument(base.transform.position))
		{
			GameManager.Destroy(base.gameObject);
		}
	}
}
