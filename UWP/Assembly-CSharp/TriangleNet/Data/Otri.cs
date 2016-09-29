using System;
using TriangleNet;

namespace TriangleNet.Data
{
	internal struct Otri
	{
		public Triangle triangle;

		public int orient;

		private readonly static int[] plus1Mod3;

		private readonly static int[] minus1Mod3;

		static Otri()
		{
			Otri.plus1Mod3 = new int[] { 1, 2, 0 };
			Otri.minus1Mod3 = new int[] { 2, 0, 1 };
		}

		public Vertex Apex()
		{
			return this.triangle.vertices[this.orient];
		}

		public void Bond(ref Otri o2)
		{
			this.triangle.neighbors[this.orient].triangle = o2.triangle;
			this.triangle.neighbors[this.orient].orient = o2.orient;
			o2.triangle.neighbors[o2.orient].triangle = this.triangle;
			o2.triangle.neighbors[o2.orient].orient = this.orient;
		}

		public void Copy(ref Otri o2)
		{
			o2.triangle = this.triangle;
			o2.orient = this.orient;
		}

		public Vertex Dest()
		{
			return this.triangle.vertices[Otri.minus1Mod3[this.orient]];
		}

		public void Dissolve()
		{
			this.triangle.neighbors[this.orient].triangle = Mesh.dummytri;
			this.triangle.neighbors[this.orient].orient = 0;
		}

		public void Dnext(ref Otri o2)
		{
			o2.triangle = this.triangle.neighbors[this.orient].triangle;
			o2.orient = this.triangle.neighbors[this.orient].orient;
			o2.orient = Otri.minus1Mod3[o2.orient];
		}

		public void DnextSelf()
		{
			int num = this.orient;
			this.orient = this.triangle.neighbors[num].orient;
			this.triangle = this.triangle.neighbors[num].triangle;
			this.orient = Otri.minus1Mod3[this.orient];
		}

		public void Dprev(ref Otri o2)
		{
			o2.triangle = this.triangle;
			o2.orient = Otri.plus1Mod3[this.orient];
			int num = o2.orient;
			o2.orient = o2.triangle.neighbors[num].orient;
			o2.triangle = o2.triangle.neighbors[num].triangle;
		}

		public void DprevSelf()
		{
			this.orient = Otri.plus1Mod3[this.orient];
			int num = this.orient;
			this.orient = this.triangle.neighbors[num].orient;
			this.triangle = this.triangle.neighbors[num].triangle;
		}

		public bool Equal(Otri o2)
		{
			if (this.triangle != o2.triangle)
			{
				return false;
			}
			return this.orient == o2.orient;
		}

		public void Infect()
		{
			this.triangle.infected = true;
		}

		public static bool IsDead(Triangle tria)
		{
			return tria.neighbors[0].triangle == null;
		}

		public bool IsInfected()
		{
			return this.triangle.infected;
		}

		public static void Kill(Triangle tria)
		{
			tria.neighbors[0].triangle = null;
			tria.neighbors[2].triangle = null;
		}

		public void Lnext(ref Otri o2)
		{
			o2.triangle = this.triangle;
			o2.orient = Otri.plus1Mod3[this.orient];
		}

		public void LnextSelf()
		{
			this.orient = Otri.plus1Mod3[this.orient];
		}

		public void Lprev(ref Otri o2)
		{
			o2.triangle = this.triangle;
			o2.orient = Otri.minus1Mod3[this.orient];
		}

		public void LprevSelf()
		{
			this.orient = Otri.minus1Mod3[this.orient];
		}

		public void Onext(ref Otri o2)
		{
			o2.triangle = this.triangle;
			o2.orient = Otri.minus1Mod3[this.orient];
			int num = o2.orient;
			o2.orient = o2.triangle.neighbors[num].orient;
			o2.triangle = o2.triangle.neighbors[num].triangle;
		}

		public void OnextSelf()
		{
			this.orient = Otri.minus1Mod3[this.orient];
			int num = this.orient;
			this.orient = this.triangle.neighbors[num].orient;
			this.triangle = this.triangle.neighbors[num].triangle;
		}

		public void Oprev(ref Otri o2)
		{
			o2.triangle = this.triangle.neighbors[this.orient].triangle;
			o2.orient = this.triangle.neighbors[this.orient].orient;
			o2.orient = Otri.plus1Mod3[o2.orient];
		}

		public void OprevSelf()
		{
			int num = this.orient;
			this.orient = this.triangle.neighbors[num].orient;
			this.triangle = this.triangle.neighbors[num].triangle;
			this.orient = Otri.plus1Mod3[this.orient];
		}

		public Vertex Org()
		{
			return this.triangle.vertices[Otri.plus1Mod3[this.orient]];
		}

		public void Rnext(ref Otri o2)
		{
			o2.triangle = this.triangle.neighbors[this.orient].triangle;
			o2.orient = this.triangle.neighbors[this.orient].orient;
			o2.orient = Otri.plus1Mod3[o2.orient];
			int num = o2.orient;
			o2.orient = o2.triangle.neighbors[num].orient;
			o2.triangle = o2.triangle.neighbors[num].triangle;
		}

		public void RnextSelf()
		{
			int num = this.orient;
			this.orient = this.triangle.neighbors[num].orient;
			this.triangle = this.triangle.neighbors[num].triangle;
			this.orient = Otri.plus1Mod3[this.orient];
			num = this.orient;
			this.orient = this.triangle.neighbors[num].orient;
			this.triangle = this.triangle.neighbors[num].triangle;
		}

		public void Rprev(ref Otri o2)
		{
			o2.triangle = this.triangle.neighbors[this.orient].triangle;
			o2.orient = this.triangle.neighbors[this.orient].orient;
			o2.orient = Otri.minus1Mod3[o2.orient];
			int num = o2.orient;
			o2.orient = o2.triangle.neighbors[num].orient;
			o2.triangle = o2.triangle.neighbors[num].triangle;
		}

		public void RprevSelf()
		{
			int num = this.orient;
			this.orient = this.triangle.neighbors[num].orient;
			this.triangle = this.triangle.neighbors[num].triangle;
			this.orient = Otri.minus1Mod3[this.orient];
			num = this.orient;
			this.orient = this.triangle.neighbors[num].orient;
			this.triangle = this.triangle.neighbors[num].triangle;
		}

		public void SegBond(ref Osub os)
		{
			this.triangle.subsegs[this.orient] = os;
			os.seg.triangles[os.orient] = this;
		}

		public void SegDissolve()
		{
			this.triangle.subsegs[this.orient].seg = Mesh.dummysub;
		}

		public void SegPivot(ref Osub os)
		{
			os = this.triangle.subsegs[this.orient];
		}

		public void SetApex(Vertex ptr)
		{
			this.triangle.vertices[this.orient] = ptr;
		}

		public void SetDest(Vertex ptr)
		{
			this.triangle.vertices[Otri.minus1Mod3[this.orient]] = ptr;
		}

		public void SetOrg(Vertex ptr)
		{
			this.triangle.vertices[Otri.plus1Mod3[this.orient]] = ptr;
		}

		public void Sym(ref Otri o2)
		{
			o2.triangle = this.triangle.neighbors[this.orient].triangle;
			o2.orient = this.triangle.neighbors[this.orient].orient;
		}

		public void SymSelf()
		{
			int num = this.orient;
			this.orient = this.triangle.neighbors[num].orient;
			this.triangle = this.triangle.neighbors[num].triangle;
		}

		public override string ToString()
		{
			if (this.triangle == null)
			{
				return "O-TID [null]";
			}
			return string.Format("O-TID {0}", this.triangle.hash);
		}

		public void Uninfect()
		{
			this.triangle.infected = false;
		}
	}
}