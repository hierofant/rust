using UnityEngine;

public static class CardinalDirectionEx
{
	public static CardinalDirection Opposite(this CardinalDirection direction)
	{
		return direction switch
		{
			CardinalDirection.North => CardinalDirection.South, 
			CardinalDirection.East => CardinalDirection.West, 
			CardinalDirection.South => CardinalDirection.North, 
			CardinalDirection.West => CardinalDirection.East, 
			_ => CardinalDirection.None, 
		};
	}

	public static Vector3 ToVectorDirection(this CardinalDirection direction)
	{
		return direction switch
		{
			CardinalDirection.North => new Vector3(0f, 0f, 1f), 
			CardinalDirection.East => new Vector3(1f, 0f, 0f), 
			CardinalDirection.South => new Vector3(0f, 0f, -1f), 
			CardinalDirection.West => new Vector3(-1f, 0f, 0f), 
			_ => Vector3.zero, 
		};
	}
}
