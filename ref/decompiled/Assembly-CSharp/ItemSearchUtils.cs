using System;
using System.Globalization;
using System.Linq;
using UnityEngine;

public static class ItemSearchUtils
{
	public static IOrderedEnumerable<ItemDefinition> SearchForItems(string searchString, Func<ItemDefinition, bool> validFilter = null)
	{
		if (searchString == "")
		{
			searchString = "BALLS BALLS BALLS";
		}
		return from y in ItemManager.itemList.Where((ItemDefinition x) => IsValidSearchResult(searchString, x, validFilter == null) && (validFilter == null || validFilter(x))).Take(60)
			orderby ScoreSearchResult(searchString, y)
			select y;
	}

	public static bool IsValidSearchResult(string search, ItemDefinition target, bool checkItemIsValid)
	{
		if (checkItemIsValid && (!(target.isRedirectOf != null) || target.redirectVendingBehaviour != ItemDefinition.RedirectVendingBehaviour.ListAsUniqueItem) && target.Hidden())
		{
			return false;
		}
		if (string.IsNullOrEmpty(target.displayName.translated))
		{
			return false;
		}
		string translated = target.displayDescription.translated;
		bool flag = !string.IsNullOrEmpty(translated);
		if (!target.shortname.Contains(search, CompareOptions.IgnoreCase) && !target.displayName.translated.Contains(search, CompareOptions.IgnoreCase) && (!flag || !translated.Contains(search, CompareOptions.IgnoreCase)) && CultureInfo.CurrentCulture.CompareInfo.IndexOf(target.displayName.translated, search, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) < 0)
		{
			if (flag)
			{
				return CultureInfo.CurrentCulture.CompareInfo.IndexOf(translated, search, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;
			}
			return false;
		}
		return true;
	}

	public static float ScoreSearchResult(string search, ItemDefinition target)
	{
		float num = 0f;
		if (target.shortname.Equals(search, StringComparison.CurrentCultureIgnoreCase) || target.displayName.translated.Equals(search, StringComparison.CurrentCultureIgnoreCase))
		{
			num -= (float)(500 - search.Length);
		}
		float a = (target.shortname.Contains(search, CompareOptions.IgnoreCase) ? ((float)search.Length / (float)target.shortname.Length) : 0f);
		float b = (target.displayName.translated.Contains(search, CompareOptions.IgnoreCase) ? ((float)search.Length / (float)target.displayName.translated.Length) : 0f);
		float num2 = Mathf.Max(a, b);
		num -= 50f * num2;
		if (!string.IsNullOrEmpty(target.displayDescription.translated) && target.displayDescription.translated.Contains(search, CompareOptions.IgnoreCase))
		{
			num -= (float)search.Length;
		}
		return num;
	}
}
