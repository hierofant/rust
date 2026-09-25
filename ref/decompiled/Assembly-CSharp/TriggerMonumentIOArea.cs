using UnityEngine;

public class TriggerMonumentIOArea : TriggerBase
{
	internal override GameObject InterestedInObject(GameObject obj)
	{
		if (obj.GetComponent<BasePlayer>() == null)
		{
			return null;
		}
		return base.InterestedInObject(obj);
	}
}
