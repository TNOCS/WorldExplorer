using System;
using TriangleNet.Data;

namespace TriangleNet.Geometry
{
	public interface ITriangle
	{
		double Area
		{
			get;
			set;
		}

		int ID
		{
			get;
		}

		int N0
		{
			get;
		}

		int N1
		{
			get;
		}

		int N2
		{
			get;
		}

		int P0
		{
			get;
		}

		int P1
		{
			get;
		}

		int P2
		{
			get;
		}

		int Region
		{
			get;
		}

		bool SupportsNeighbors
		{
			get;
		}

		ITriangle GetNeighbor(int index);

		ISegment GetSegment(int index);

		Vertex GetVertex(int index);
	}
}