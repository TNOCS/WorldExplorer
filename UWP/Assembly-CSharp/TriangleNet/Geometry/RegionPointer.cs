using System;

namespace TriangleNet.Geometry
{
	public class RegionPointer
	{
		internal Point point;

		internal int id;

		public RegionPointer(double x, double y, int id)
		{
			this.point = new Point(x, y);
			this.id = id;
		}
	}
}