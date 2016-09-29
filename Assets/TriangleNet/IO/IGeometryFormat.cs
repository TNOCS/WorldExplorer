using System;
using TriangleNet.Geometry;

namespace TriangleNet.IO
{
	public interface IGeometryFormat
	{
		InputGeometry Read(string filename);
	}
}