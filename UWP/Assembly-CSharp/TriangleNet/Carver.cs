using System;
using System.Collections.Generic;
using TriangleNet.Data;
using TriangleNet.Geometry;
using TriangleNet.Tools;

namespace TriangleNet
{
	internal class Carver
	{
		private Mesh mesh;

		private List<Triangle> viri;

		public Carver(Mesh mesh)
		{
			this.mesh = mesh;
			this.viri = new List<Triangle>();
		}

		public void CarveHoles()
		{
			Otri otri = new Otri();
			Triangle[] triangleArray = null;
			if (!this.mesh.behavior.Convex)
			{
				this.InfectHull();
			}
			if (!this.mesh.behavior.NoHoles)
			{
				foreach (Point hole in this.mesh.holes)
				{
					if (!this.mesh.bounds.Contains(hole))
					{
						continue;
					}
					otri.triangle = Mesh.dummytri;
					otri.orient = 0;
					otri.SymSelf();
					if (Primitives.CounterClockwise(otri.Org(), otri.Dest(), hole) <= 0 || this.mesh.locator.Locate(hole, ref otri) == LocateResult.Outside || otri.IsInfected())
					{
						continue;
					}
					otri.Infect();
					this.viri.Add(otri.triangle);
				}
			}
			if (this.mesh.regions.Count > 0)
			{
				int num = 0;
				triangleArray = new Triangle[this.mesh.regions.Count];
				foreach (RegionPointer region in this.mesh.regions)
				{
					triangleArray[num] = Mesh.dummytri;
					if (this.mesh.bounds.Contains(region.point))
					{
						otri.triangle = Mesh.dummytri;
						otri.orient = 0;
						otri.SymSelf();
						if (Primitives.CounterClockwise(otri.Org(), otri.Dest(), region.point) > 0 && this.mesh.locator.Locate(region.point, ref otri) != LocateResult.Outside && !otri.IsInfected())
						{
							triangleArray[num] = otri.triangle;
							triangleArray[num].region = region.id;
						}
					}
					num++;
				}
			}
			if (this.viri.Count > 0)
			{
				this.Plague();
			}
			if (triangleArray != null)
			{
				RegionIterator regionIterator = new RegionIterator(this.mesh);
				for (int i = 0; i < (int)triangleArray.Length; i++)
				{
					if (triangleArray[i] != Mesh.dummytri && !Otri.IsDead(triangleArray[i]))
					{
						regionIterator.Process(triangleArray[i]);
					}
				}
			}
			this.viri.Clear();
		}

		private void InfectHull()
		{
			Otri otri = new Otri();
			Otri otri1 = new Otri();
			Otri otri2 = new Otri();
			Osub osub = new Osub();
			otri.triangle = Mesh.dummytri;
			otri.orient = 0;
			otri.SymSelf();
			otri.Copy(ref otri2);
			do
			{
				if (!otri.IsInfected())
				{
					otri.SegPivot(ref osub);
					if (osub.seg == Mesh.dummysub)
					{
						if (!otri.IsInfected())
						{
							otri.Infect();
							this.viri.Add(otri.triangle);
						}
					}
					else if (osub.seg.boundary == 0)
					{
						osub.seg.boundary = 1;
						Vertex vertex = otri.Org();
						Vertex vertex1 = otri.Dest();
						if (vertex.mark == 0)
						{
							vertex.mark = 1;
						}
						if (vertex1.mark == 0)
						{
							vertex1.mark = 1;
						}
					}
				}
				otri.LnextSelf();
				otri.Oprev(ref otri1);
				while (otri1.triangle != Mesh.dummytri)
				{
					otri1.Copy(ref otri);
					otri.Oprev(ref otri1);
				}
			}
			while (!otri.Equal(otri2));
		}

		private void Plague()
		{
			Otri item = new Otri();
			Otri otri = new Otri();
			Osub osub = new Osub();
			for (int i = 0; i < this.viri.Count; i++)
			{
				item.triangle = this.viri[i];
				item.Uninfect();
				item.orient = 0;
				while (item.orient < 3)
				{
					item.Sym(ref otri);
					item.SegPivot(ref osub);
					if (otri.triangle != Mesh.dummytri && !otri.IsInfected())
					{
						if (osub.seg != Mesh.dummysub)
						{
							osub.TriDissolve();
							if (osub.seg.boundary == 0)
							{
								osub.seg.boundary = 1;
							}
							Vertex vertex = otri.Org();
							Vertex vertex1 = otri.Dest();
							if (vertex.mark == 0)
							{
								vertex.mark = 1;
							}
							if (vertex1.mark == 0)
							{
								vertex1.mark = 1;
							}
						}
						else
						{
							otri.Infect();
							this.viri.Add(otri.triangle);
						}
					}
					else if (osub.seg != Mesh.dummysub)
					{
						this.mesh.SubsegDealloc(osub.seg);
						if (otri.triangle != Mesh.dummytri)
						{
							otri.Uninfect();
							otri.SegDissolve();
							otri.Infect();
						}
					}
					item.orient = item.orient + 1;
				}
				item.Infect();
			}
			foreach (Triangle virus in this.viri)
			{
				item.triangle = virus;
				item.orient = 0;
				while (item.orient < 3)
				{
					Vertex vertex2 = item.Org();
					if (vertex2 != null)
					{
						bool flag = true;
						item.SetOrg(null);
						item.Onext(ref otri);
						while (otri.triangle != Mesh.dummytri && !otri.Equal(item))
						{
							if (!otri.IsInfected())
							{
								flag = false;
							}
							else
							{
								otri.SetOrg(null);
							}
							otri.OnextSelf();
						}
						if (otri.triangle == Mesh.dummytri)
						{
							item.Oprev(ref otri);
							while (otri.triangle != Mesh.dummytri)
							{
								if (!otri.IsInfected())
								{
									flag = false;
								}
								else
								{
									otri.SetOrg(null);
								}
								otri.OprevSelf();
							}
						}
						if (flag)
						{
							vertex2.type = VertexType.UndeadVertex;
							Mesh mesh = this.mesh;
							mesh.undeads = mesh.undeads + 1;
						}
					}
					item.orient = item.orient + 1;
				}
				item.orient = 0;
				while (item.orient < 3)
				{
					item.Sym(ref otri);
					if (otri.triangle != Mesh.dummytri)
					{
						otri.Dissolve();
						Mesh mesh1 = this.mesh;
						mesh1.hullsize = mesh1.hullsize + 1;
					}
					else
					{
						Mesh mesh2 = this.mesh;
						mesh2.hullsize = mesh2.hullsize - 1;
					}
					item.orient = item.orient + 1;
				}
				this.mesh.TriangleDealloc(item.triangle);
			}
			this.viri.Clear();
		}
	}
}