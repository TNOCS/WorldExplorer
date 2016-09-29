using System;
using TriangleNet;

namespace TriangleNet.Data
{
	internal struct Osub
	{
		public Segment seg;

		public int orient;

		public void Bond(ref Osub o2)
		{
			this.seg.subsegs[this.orient] = o2;
			o2.seg.subsegs[o2.orient] = this;
		}

		public void Copy(ref Osub o2)
		{
			o2.seg = this.seg;
			o2.orient = this.orient;
		}

		public Vertex Dest()
		{
			return this.seg.vertices[1 - this.orient];
		}

		public void Dissolve()
		{
			this.seg.subsegs[this.orient].seg = Mesh.dummysub;
		}

		public bool Equal(Osub o2)
		{
			if (this.seg != o2.seg)
			{
				return false;
			}
			return this.orient == o2.orient;
		}

		public static bool IsDead(Segment sub)
		{
			return sub.subsegs[0].seg == null;
		}

		public static void Kill(Segment sub)
		{
			sub.subsegs[0].seg = null;
			sub.subsegs[1].seg = null;
		}

		public int Mark()
		{
			return this.seg.boundary;
		}

		public void Next(ref Osub o2)
		{
			o2 = this.seg.subsegs[1 - this.orient];
		}

		public void NextSelf()
		{
			this = this.seg.subsegs[1 - this.orient];
		}

		public Vertex Org()
		{
			return this.seg.vertices[this.orient];
		}

		public void Pivot(ref Osub o2)
		{
			o2 = this.seg.subsegs[this.orient];
		}

		public void PivotSelf()
		{
			this = this.seg.subsegs[this.orient];
		}

		public Vertex SegDest()
		{
			return this.seg.vertices[3 - this.orient];
		}

		public Vertex SegOrg()
		{
			return this.seg.vertices[2 + this.orient];
		}

		public void SetDest(Vertex ptr)
		{
			this.seg.vertices[1 - this.orient] = ptr;
		}

		public void SetMark(int value)
		{
			this.seg.boundary = value;
		}

		public void SetOrg(Vertex ptr)
		{
			this.seg.vertices[this.orient] = ptr;
		}

		public void SetSegDest(Vertex ptr)
		{
			this.seg.vertices[3 - this.orient] = ptr;
		}

		public void SetSegOrg(Vertex ptr)
		{
			this.seg.vertices[2 + this.orient] = ptr;
		}

		public void Sym(ref Osub o2)
		{
			o2.seg = this.seg;
			o2.orient = 1 - this.orient;
		}

		public void SymSelf()
		{
			this.orient = 1 - this.orient;
		}

		public override string ToString()
		{
			if (this.seg == null)
			{
				return "O-TID [null]";
			}
			return string.Format("O-SID {0}", this.seg.hash);
		}

		public void TriDissolve()
		{
			this.seg.triangles[this.orient].triangle = Mesh.dummytri;
		}

		public void TriPivot(ref Otri ot)
		{
			ot = this.seg.triangles[this.orient];
		}
	}
}