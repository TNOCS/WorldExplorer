using System;
using System.Collections;
using System.Collections.Generic;
using TriangleNet;
using TriangleNet.Data;

namespace TriangleNet.Geometry
{
	public class EdgeEnumerator : IEnumerator<Edge>, IDisposable, IEnumerator
	{
		private IEnumerator<Triangle> triangles;

		private Otri tri;

		private Otri neighbor;

		private Osub sub;

		private Edge current;

		private Vertex p1;

		private Vertex p2;

		public Edge Current
		{
			get
			{
				return this.current;
			}
		}

		object System.Collections.IEnumerator.Current
		{
			get
			{
				return this.current;
			}
		}

		public EdgeEnumerator(Mesh mesh)
		{
			this.triangles = mesh.triangles.Values.GetEnumerator();
			this.triangles.MoveNext();
			this.tri.triangle = this.triangles.Current;
			this.tri.orient = 0;
		}

		public void Dispose()
		{
			this.triangles.Dispose();
		}

		public bool MoveNext()
		{
			if (this.tri.triangle == null)
			{
				return false;
			}
			this.current = null;
			while (this.current == null)
			{
				if (this.tri.orient == 3)
				{
					if (!this.triangles.MoveNext())
					{
						return false;
					}
					this.tri.triangle = this.triangles.Current;
					this.tri.orient = 0;
				}
				this.tri.Sym(ref this.neighbor);
				if (this.tri.triangle.id < this.neighbor.triangle.id || this.neighbor.triangle == Mesh.dummytri)
				{
					this.p1 = this.tri.Org();
					this.p2 = this.tri.Dest();
					this.tri.SegPivot(ref this.sub);
					this.current = new Edge(this.p1.id, this.p2.id, this.sub.seg.boundary);
				}
				this.tri.orient = this.tri.orient + 1;
			}
			return true;
		}

		public void Reset()
		{
			this.triangles.Reset();
		}
	}
}