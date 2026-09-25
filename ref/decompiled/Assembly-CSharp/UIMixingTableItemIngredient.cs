using UnityEngine;
using UnityEngine.UI;

public class UIMixingTableItemIngredient : MonoBehaviour
{
	public Image ItemIcon;

	public Text ItemCount;

	public Tooltip ToolTip;

	public void Init(Recipe.RecipeIngredient ingredient, ItemDefinition producedItem)
	{
		ItemIcon.sprite = ingredient.Ingredient.iconSprite;
		int ingredientCount = ingredient.GetIngredientCount(producedItem);
		ItemCount.text = ingredientCount.ToString();
		ItemIcon.enabled = true;
		ItemCount.enabled = true;
		ToolTip.Text = $"{ingredientCount} x {ingredient.Ingredient.displayName.translated}";
		ToolTip.enabled = true;
	}

	public void InitBlank()
	{
		ItemIcon.enabled = false;
		ItemCount.enabled = false;
		ToolTip.enabled = false;
	}
}
