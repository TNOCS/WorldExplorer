using System;
using System.Reflection;
using TriangleNet;
using TriangleNet.Geometry;

namespace TriangleNet.Data
{
	public class Vertex : Point
	{
		internal int hash;

		internal VertexType type;

		internal Otri tri;

		public double this[int i]
		{
			get
			{
				if (i == 0)
				{
					return this.x;
				}
				if (i != 1)
				{
					throw new ArgumentOutOfRangeException("Index must be 0 or 1.");
				}
				return this.y;
			}
		}

		public VertexType Type
		{
			get
			{
				return this.type;
			}
		}

		public Vertex() : this(0, 0, 0, 0)
		{
		}

		public Vertex(double x, double y) : this(x, y, 0, 0)
		{
		}

		public Vertex(double x, double y, int mark) : this(x, y, mark, 0)
		{
		}

		public Vertex(double x, double y, int mark, int attribs) : base(x, y, mark)
		{
			this.type = VertexType.InputVertex;
			if (attribs > 0)
			{
				this.attributes = new double[attribs];
			}
		}

		public override int GetHashCode()
		{
			return this.hash;
		}
	}
}