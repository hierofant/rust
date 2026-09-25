using System.Collections.Generic;
using UnityEngine;

public interface IPlacementDirectionProvider
{
	List<Vector3> GetSnapForwardDirections();
}
