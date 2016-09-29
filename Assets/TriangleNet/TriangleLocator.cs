using System;
using System.Collections.Generic;
using TriangleNet.Data;
using TriangleNet.Geometry;

namespace TriangleNet
{
	internal class TriangleLocator
	{
		private Sampler sampler;

		private Mesh mesh;

		internal Otri recenttri;

		public TriangleLocator(Mesh mesh)
		{
			this.mesh = mesh;
			this.sampler = new Sampler();
		}

		public LocateResult Locate(Point searchpoint, ref Otri searchtri)
		{
			double x;
			Otri item = new Otri();
			Vertex vertex = searchtri.Org();
			double num = (searchpoint.X - vertex.x) * (searchpoint.X - vertex.x) + (searchpoint.Y - vertex.y) * (searchpoint.Y - vertex.y);
			if (this.recenttri.triangle != null && !Otri.IsDead(this.recenttri.triangle))
			{
				vertex = this.recenttri.Org();
				if (vertex.x == searchpoint.X && vertex.y == searchpoint.Y)
				{
					this.recenttri.Copy(ref searchtri);
					return LocateResult.OnVertex;
				}
				x = (searchpoint.X - vertex.x) * (searchpoint.X - vertex.x) + (searchpoint.Y - vertex.y) * (searchpoint.Y - vertex.y);
				if (x < num)
				{
					this.recenttri.Copy(ref searchtri);
					num = x;
				}
			}
			this.sampler.Update(this.mesh);
			int[] samples = this.sampler.GetSamples(this.mesh);
			for (int i = 0; i < (int)samples.Length; i++)
			{
				int num1 = samples[i];
				item.triangle = this.mesh.triangles[num1];
				if (!Otri.IsDead(item.triangle))
				{
					vertex = item.Org();
					x = (searchpoint.X - vertex.x) * (searchpoint.X - vertex.x) + (searchpoint.Y - vertex.y) * (searchpoint.Y - vertex.y);
					if (x < num)
					{
						item.Copy(ref searchtri);
						num = x;
					}
				}
			}
			vertex = searchtri.Org();
			Vertex vertex1 = searchtri.Dest();
			if (vertex.x == searchpoint.X && vertex.y == searchpoint.Y)
			{
				return LocateResult.OnVertex;
			}
			if (vertex1.x == searchpoint.X && vertex1.y == searchpoint.Y)
			{
				searchtri.LnextSelf();
				return LocateResult.OnVertex;
			}
			double num2 = Primitives.CounterClockwise(vertex, vertex1, searchpoint);
			if (num2 < 0)
			{
				searchtri.SymSelf();
			}
			else if (num2 == 0 && vertex.x < searchpoint.X == searchpoint.X < vertex1.x && vertex.y < searchpoint.Y == searchpoint.Y < vertex1.y)
			{
				return LocateResult.OnEdge;
			}
			return this.PreciseLocate(searchpoint, ref searchtri, false);
		}

		public LocateResult PreciseLocate(Point searchpoint, ref Otri searchtri, bool stopatsubsegment)
		{
			bool flag;
			Otri otri = new Otri();
			Osub osub = new Osub();
			Vertex vertex = searchtri.Org();
			Vertex vertex1 = searchtri.Dest();
			for (Vertex i = searchtri.Apex(); i.x != searchpoint.X || i.y != searchpoint.Y; i = searchtri.Apex())
			{
				double num = Primitives.CounterClockwise(vertex, i, searchpoint);
				double num1 = Primitives.CounterClockwise(i, vertex1, searchpoint);
				if (num <= 0)
				{
					if (num1 <= 0)
					{
						if (num == 0)
						{
							searchtri.LprevSelf();
							return LocateResult.OnEdge;
						}
						if (num1 != 0)
						{
							return LocateResult.InTriangle;
						}
						searchtri.LnextSelf();
						return LocateResult.OnEdge;
					}
					flag = false;
				}
				else
				{
					flag = (num1 <= 0 ? true : (i.x - searchpoint.X) * (vertex1.x - vertex.x) + (i.y - searchpoint.Y) * (vertex1.y - vertex.y) > 0);
				}
				if (!flag)
				{
					searchtri.Lnext(ref otri);
					vertex = i;
				}
				else
				{
					searchtri.Lprev(ref otri);
					vertex1 = i;
				}
				otri.Sym(ref searchtri);
				if (this.mesh.checksegments & stopatsubsegment)
				{
					otri.SegPivot(ref osub);
					if (osub.seg != Mesh.dummysub)
					{
						otri.Copy(ref searchtri);
						return LocateResult.Outside;
					}
				}
				if (searchtri.triangle == Mesh.dummytri)
				{
					otri.Copy(ref searchtri);
					return LocateResult.Outside;
				}
			}
			searchtri.LprevSelf();
			return LocateResult.OnVertex;
		}

		public void Reset()
		{
			this.recenttri.triangle = null;
		}

		public void Update(ref Otri otri)
		{
			otri.Copy(ref this.recenttri);
		}
	}
}