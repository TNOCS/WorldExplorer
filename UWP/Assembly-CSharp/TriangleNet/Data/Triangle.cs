using System;
using TriangleNet;
using TriangleNet.Geometry;

namespace TriangleNet.Data
{
	public class Triangle : ITriangle
	{
		internal int hash;

		internal int id;

		internal Otri[] neighbors;

		internal Vertex[] vertices;

		internal Osub[] subsegs;

		internal int region;

		internal double area;

		internal bool infected;

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
				return this.id;
			}
		}

		public int N0
		{
			get
			{
				return this.neighbors[0].triangle.id;
			}
		}

		public int N1
		{
			get
			{
				return this.neighbors[1].triangle.id;
			}
		}

		public int N2
		{
			get
			{
				return this.neighbors[2].triangle.id;
			}
		}

		public int P0
		{
			get
			{
				if (this.vertices[0] == null)
				{
					return -1;
				}
				return this.vertices[0].id;
			}
		}

		public int P1
		{
			get
			{
				if (this.vertices[1] == null)
				{
					return -1;
				}
				return this.vertices[1].id;
			}
		}

		public int P2
		{
			get
			{
				if (this.vertices[2] == null)
				{
					return -1;
				}
				return this.vertices[2].id;
			}
		}

		public int Region
		{
			get
			{
				return this.region;
			}
		}

		public bool SupportsNeighbors
		{
			get
			{
				return true;
			}
		}

		public Triangle()
		{
			this.neighbors = new Otri[3];
			this.neighbors[0].triangle = Mesh.dummytri;
			this.neighbors[1].triangle = Mesh.dummytri;
			this.neighbors[2].triangle = Mesh.dummytri;
			this.vertices = new Vertex[3];
			this.subsegs = new Osub[3];
			this.subsegs[0].seg = Mesh.dummysub;
			this.subsegs[1].seg = Mesh.dummysub;
			this.subsegs[2].seg = Mesh.dummysub;
		}

		public override int GetHashCode()
		{
			return this.hash;
		}

		public ITriangle GetNeighbor(int index)
		{
			if (this.neighbors[index].triangle == Mesh.dummytri)
			{
				return null;
			}
			return this.neighbors[index].triangle;
		}

		public ISegment GetSegment(int index)
		{
			if (this.subsegs[index].seg == Mesh.dummysub)
			{
				return null;
			}
			return this.subsegs[index].seg;
		}

		public Vertex GetVertex(int index)
		{
			return this.vertices[index];
		}

		public override string ToString()
		{
			return string.Format("TID {0}", this.hash);
		}
	}
}