using System.Text;
using Facepunch;
using UnityEngine;

public class GrowableGenes
{
	public GrowableGene[] Genes;

	private static GrowableGenetics.GeneWeighting[] baseWeights = new GrowableGenetics.GeneWeighting[6];

	private static GrowableGenetics.GeneWeighting[] slotWeights = new GrowableGenetics.GeneWeighting[6];

	public GrowableGenes()
	{
		Clear();
	}

	private void Clear()
	{
		Genes = new GrowableGene[6];
		for (int i = 0; i < 6; i++)
		{
			Genes[i] = new GrowableGene();
		}
	}

	public void GenerateFavourableGenes(GrowableEntity growable, float geneChance = -1f)
	{
		if (!(growable == null) && !(growable.Properties.Genes == null))
		{
			CalculateBaseWeights(growable.Properties.Genes);
			for (int i = 0; i < 6; i++)
			{
				CalculateSlotWeights(growable.Properties.Genes, i);
				Genes[i].Set(PickFavourableGeneType(geneChance), firstSet: true);
			}
		}
	}

	public void GenerateRandom(GrowableEntity growable)
	{
		if (!(growable == null) && !(growable.Properties.Genes == null))
		{
			CalculateBaseWeights(growable.Properties.Genes);
			for (int i = 0; i < 6; i++)
			{
				CalculateSlotWeights(growable.Properties.Genes, i);
				Genes[i].Set(PickWeightedGeneType(), firstSet: true);
			}
		}
	}

	private void CalculateBaseWeights(GrowableGeneProperties properties)
	{
		int num = 0;
		GrowableGeneProperties.GeneWeight[] weights = properties.Weights;
		for (int i = 0; i < weights.Length; i++)
		{
			GrowableGeneProperties.GeneWeight geneWeight = weights[i];
			baseWeights[num].GeneType = (slotWeights[num].GeneType = (GrowableGenetics.GeneType)num);
			baseWeights[num].Weighting = geneWeight.BaseWeight;
			num++;
		}
	}

	private void CalculateSlotWeights(GrowableGeneProperties properties, int slot)
	{
		int num = 0;
		GrowableGeneProperties.GeneWeight[] weights = properties.Weights;
		for (int i = 0; i < weights.Length; i++)
		{
			GrowableGeneProperties.GeneWeight geneWeight = weights[i];
			slotWeights[num].Weighting = baseWeights[num].Weighting + geneWeight.SlotWeights[slot];
			num++;
		}
	}

	private GrowableGenetics.GeneType PickWeightedGeneType()
	{
		float num = 0f;
		GrowableGenetics.GeneWeighting[] array = slotWeights;
		for (int i = 0; i < array.Length; i++)
		{
			GrowableGenetics.GeneWeighting geneWeighting = array[i];
			num += geneWeighting.Weighting;
		}
		GrowableGenetics.GeneType result = GrowableGenetics.GeneType.Empty;
		float num2 = Random.Range(0f, num);
		float num3 = 0f;
		array = slotWeights;
		for (int i = 0; i < array.Length; i++)
		{
			GrowableGenetics.GeneWeighting geneWeighting2 = array[i];
			num3 += geneWeighting2.Weighting;
			if (num2 < num3)
			{
				result = geneWeighting2.GeneType;
				break;
			}
		}
		return result;
	}

	private GrowableGenetics.GeneType PickFavourableGeneType(float favourableGeneChance = -1f)
	{
		if (favourableGeneChance < 0f)
		{
			favourableGeneChance = PlanterBoxStatic.FavourableGeneChance;
		}
		BufferList<GrowableGenetics.GeneWeighting> obj = Pool.Get<BufferList<GrowableGenetics.GeneWeighting>>();
		BufferList<GrowableGenetics.GeneWeighting> obj2 = Pool.Get<BufferList<GrowableGenetics.GeneWeighting>>();
		float num = 0f;
		float num2 = 0f;
		GrowableGenetics.GeneWeighting[] array = slotWeights;
		for (int i = 0; i < array.Length; i++)
		{
			GrowableGenetics.GeneWeighting element = array[i];
			if (GrowableGene.IsPositive(element.GeneType))
			{
				obj.Add(element);
				num += element.Weighting;
			}
			else
			{
				obj2.Add(element);
				num2 += element.Weighting;
			}
		}
		float num3 = Mathx.RemapValClamped(Mathf.Clamp(favourableGeneChance, 0f, 1f), 0f, 1f, 1f, 0f);
		float maxInclusive = num + num2 * num3;
		float num4 = Random.Range(0f, maxInclusive);
		float num5 = 0f;
		foreach (GrowableGenetics.GeneWeighting item in obj)
		{
			num5 += item.Weighting;
			if (num4 < num5)
			{
				Pool.FreeUnmanaged(ref obj);
				Pool.FreeUnmanaged(ref obj2);
				return item.GeneType;
			}
		}
		foreach (GrowableGenetics.GeneWeighting item2 in obj2)
		{
			num5 += item2.Weighting * num3;
			if (num4 < num5)
			{
				Pool.FreeUnmanaged(ref obj);
				Pool.FreeUnmanaged(ref obj2);
				return item2.GeneType;
			}
		}
		Pool.FreeUnmanaged(ref obj);
		Pool.FreeUnmanaged(ref obj2);
		return GrowableGenetics.GeneType.Empty;
	}

	public int GetGeneTypeCount(GrowableGenetics.GeneType geneType)
	{
		int num = 0;
		GrowableGene[] genes = Genes;
		for (int i = 0; i < genes.Length; i++)
		{
			if (genes[i].Type == geneType)
			{
				num++;
			}
		}
		return num;
	}

	public int GetPositiveGeneCount()
	{
		int num = 0;
		GrowableGene[] genes = Genes;
		for (int i = 0; i < genes.Length; i++)
		{
			if (genes[i].IsPositive())
			{
				num++;
			}
		}
		return num;
	}

	public int GetNegativeGeneCount()
	{
		int num = 0;
		GrowableGene[] genes = Genes;
		for (int i = 0; i < genes.Length; i++)
		{
			if (!genes[i].IsPositive())
			{
				num++;
			}
		}
		return num;
	}

	public void Save(BaseNetworkable.SaveInfo info)
	{
		info.msg.growableEntity.genes = GrowableGeneEncoding.EncodeGenesToInt(this);
		info.msg.growableEntity.previousGenes = GrowableGeneEncoding.EncodePreviousGenesToInt(this);
	}

	public void Load(BaseNetworkable.LoadInfo info)
	{
		if (info.msg.growableEntity != null)
		{
			GrowableGeneEncoding.DecodeIntToGenes(info.msg.growableEntity.genes, this);
			GrowableGeneEncoding.DecodeIntToPreviousGenes(info.msg.growableEntity.previousGenes, this);
		}
	}

	public void DebugPrint()
	{
		Debug.Log(GetDisplayString(previousGenes: false));
	}

	private string GetDisplayString(bool previousGenes)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < 6; i++)
		{
			stringBuilder.Append(GrowableGene.GetDisplayCharacter(previousGenes ? Genes[i].PreviousType : Genes[i].Type));
		}
		return stringBuilder.ToString();
	}
}
