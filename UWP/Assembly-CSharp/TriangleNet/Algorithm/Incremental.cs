using System;
using System.Collections.Generic;
using TriangleNet;
using TriangleNet.Data;
using TriangleNet.Geometry;
using TriangleNet.Log;

namespace TriangleNet.Algorithm
{
	internal class Incremental
	{
		private Mesh mesh;

		public Incremental()
		{
		}

		private void GetBoundingBox()
		{
			Otri otri = new Otri();
			BoundingBox boundingBox = this.mesh.bounds;
			double width = boundingBox.Width;
			if (boundingBox.Height > width)
			{
				width = boundingBox.Height;
			}
			if (width == 0)
			{
				width = 1;
			}
			this.mesh.infvertex1 = new Vertex(boundingBox.Xmin - 50 * width, boundingBox.Ymin - 40 * width);
			this.mesh.infvertex2 = new Vertex(boundingBox.Xmax + 50 * width, boundingBox.Ymin - 40 * width);
			this.mesh.infvertex3 = new Vertex(0.5 * (boundingBox.Xmin + boundingBox.Xmax), boundingBox.Ymax + 60 * width);
			this.mesh.MakeTriangle(ref otri);
			otri.SetOrg(this.mesh.infvertex1);
			otri.SetDest(this.mesh.infvertex2);
			otri.SetApex(this.mesh.infvertex3);
			Mesh.dummytri.neighbors[0] = otri;
		}

		private int RemoveBox()
		{
			Otri otri = new Otri();
			Otri otri1 = new Otri();
			Otri otri2 = new Otri();
			Otri otri3 = new Otri();
			Otri otri4 = new Otri();
			Otri otri5 = new Otri();
			bool poly = !this.mesh.behavior.Poly;
			otri3.triangle = Mesh.dummytri;
			otri3.orient = 0;
			otri3.SymSelf();
			otri3.Lprev(ref otri4);
			otri3.LnextSelf();
			otri3.SymSelf();
			otri3.Lprev(ref otri1);
			otri1.SymSelf();
			otri3.Lnext(ref otri2);
			otri2.SymSelf();
			if (otri2.triangle == Mesh.dummytri)
			{
				otri1.LprevSelf();
				otri1.SymSelf();
			}
			Mesh.dummytri.neighbors[0] = otri1;
			int num = -2;
			while (!otri3.Equal(otri4))
			{
				num++;
				otri3.Lprev(ref otri5);
				otri5.SymSelf();
				if (poly && otri5.triangle != Mesh.dummytri)
				{
					Vertex vertex = otri5.Org();
					if (vertex.mark == 0)
					{
						vertex.mark = 1;
					}
				}
				otri5.Dissolve();
				otri3.Lnext(ref otri);
				otri.Sym(ref otri3);
				this.mesh.TriangleDealloc(otri.triangle);
				if (otri3.triangle != Mesh.dummytri)
				{
					continue;
				}
				otri5.Copy(ref otri3);
			}
			this.mesh.TriangleDealloc(otri4.triangle);
			return num;
		}

		public int Triangulate(Mesh mesh)
		{
			this.mesh = mesh;
			Otri otri = new Otri();
			this.GetBoundingBox();
			foreach (Vertex value in mesh.vertices.Values)
			{
				otri.triangle = Mesh.dummytri;
				Osub osub = new Osub();
				if (mesh.InsertVertex(value, ref otri, ref osub, false, false) != InsertVertexResult.Duplicate)
				{
					continue;
				}
				if (Behavior.Verbose)
				{
					SimpleLog.Instance.Warning("A duplicate vertex appeared and was ignored.", "Incremental.IncrementalDelaunay()");
				}
				value.type = VertexType.UndeadVertex;
				Mesh mesh1 = mesh;
				mesh1.undeads = mesh1.undeads + 1;
			}
			return this.RemoveBox();
		}
	}
}