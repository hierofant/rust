using UnityEngine;

[CreateAssetMenu(fileName = "NewEntityList", menuName = "Rust/EntityList")]
public class EntityListScriptableObject : ScriptableObject
{
	public string AutoPopulateFromAssetLabel;

	[SerializeField]
	public BaseEntity[] entities;
}
