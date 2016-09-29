using System;
using TriangleNet.Data;
using TriangleNet.Geometry;

namespace TriangleNet.IO
{
	public class InputTriangle : ITriangle
	{
		internal int[] vertices;

		internal int region;

		internal double area;

		public double Area
		{
			get
			{
				return this.area;
			}
			set
			{
				this.area = value;
			}
		}

		public int ID
		{
			get
			{
				return 0;
			}
		}

		public int N0
		{
			get
			{
				return -1;
			}
		}

		public int N1
		{
			get
			{
				return -1;
			}
		}

		public int N2
		{
			get
			{
				return -1;
			}
		}

		public int P0
		{
			get
			{
				return this.vertices[0];
			}
		}

		public int P1
		{
			get
			{
				return this.vertices[1];
			}
		}

		public int P2
		{
			get
			{
				return this.vertices[2];
			}
		}

		public int Region
		{
			get
			{
				return JustDecompileGenerated_get_Region();
			}
			set
			{
				JustDecompileGenerated_set_Region(value);
			}
		}

		public int JustDecompileGenerated_get_Region()
		{
			return this.region;
		}

		public void JustDecompileGenerated_set_Region(int value)
		{
			this.region = value;
		}

		public bool SupportsNeighbors
		{
			get
			{
				return false;
			}
		}

		public InputTriangle(int p0, int p1, int p2)
		{
			this.vertices = new int[] { p0, p1, p2 };
		}

		public ITriangle GetNeighbor(int index)
		{
			return null;
		}

		public ISegment GetSegment(int index)
		{
			return null;
		}

		public Vertex GetVertex(int index)
		{
			return null;
		}
	}
}