using UnityEngine;

public class ParticleDisableOnParentDestroy : MonoBehaviour, IOnParentDestroying
{
	public float destroyAfterSeconds;

	public void OnParentDestroying()
	{
		ParticleSystem component = GetComponent<ParticleSystem>();
		if ((bool)component)
		{
			component.enableEmission = false;
		}
		if (!PoolableEx.IsPooledPrefabChild(base.gameObject))
		{
			base.transform.parent = null;
			if (destroyAfterSeconds > 0f)
			{
				GameManager.Destroy(base.gameObject, destroyAfterSeconds);
			}
		}
	}
}
