using System;
using TriangleNet;
using TriangleNet.Geometry;

namespace TriangleNet.Data
{
	public class Segment : ISegment
	{
		internal int hash;

		internal Osub[] subsegs;

		internal Vertex[] vertices;

		internal Otri[] triangles;

		internal int boundary;

		public int Boundary
		{
			get
			{
				return this.boundary;
			}
		}

		public int P0
		{
			get
			{
				return this.vertices[0].id;
			}
		}

		public int P1
		{
			get
			{
				return this.vertices[1].id;
			}
		}

		public Segment()
		{
			this.subsegs = new Osub[2];
			this.subsegs[0].seg = Mesh.dummysub;
			this.subsegs[1].seg = Mesh.dummysub;
			this.vertices = new Vertex[4];
			this.triangles = new Otri[2];
			this.triangles[0].triangle = Mesh.dummytri;
			this.triangles[1].triangle = Mesh.dummytri;
			this.boundary = 0;
		}

		public override int GetHashCode()
		{
			return this.hash;
		}

		public ITriangle GetTriangle(int index)
		{
			if (this.triangles[index].triangle == Mesh.dummytri)
			{
				return null;
			}
			return this.triangles[index].triangle;
		}

		public Vertex GetVertex(int index)
		{
			return this.vertices[index];
		}

		public override string ToString()
		{
			return string.Format("SID {0}", this.hash);
		}
	}
}