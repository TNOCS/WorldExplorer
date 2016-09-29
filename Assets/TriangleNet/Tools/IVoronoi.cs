using System.Collections.Generic;
using TriangleNet.Geometry;

namespace TriangleNet.Tools
{
	public interface IVoronoi
	{
		Point[] Points
		{
			get;
		}

		List<VoronoiRegion> Regions
		{
			get;
		}
	}
}