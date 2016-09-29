using System;
using TriangleNet.Data;

namespace TriangleNet.Geometry
{
	public interface ISegment
	{
		int Boundary
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

		ITriangle GetTriangle(int index);

		Vertex GetVertex(int index);
	}
}