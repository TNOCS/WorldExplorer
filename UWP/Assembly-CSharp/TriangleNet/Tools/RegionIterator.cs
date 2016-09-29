using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TriangleNet;
using TriangleNet.Data;

namespace TriangleNet.Tools
{
	public class RegionIterator
	{
		private Mesh mesh;

		private List<Triangle> viri;

		public RegionIterator(Mesh mesh)
		{
			this.mesh = mesh;
			this.viri = new List<Triangle>();
		}

		public void Process(Triangle triangle)
		{
			this.Process(triangle, (Triangle tri) => tri.region = triangle.region);
		}

		public void Process(Triangle triangle, Action<Triangle> func)
		{
			if (triangle != Mesh.dummytri && !Otri.IsDead(triangle))
			{
				triangle.infected = true;
				this.viri.Add(triangle);
				this.ProcessRegion(func);
			}
			this.viri.Clear();
		}

		private void ProcessRegion(Action<Triangle> func)
		{
			Otri item = new Otri();
			Otri otri = new Otri();
			Osub osub = new Osub();
			Behavior behavior = this.mesh.behavior;
			for (int i = 0; i < this.viri.Count; i++)
			{
				item.triangle = this.viri[i];
				item.Uninfect();
				func(item.triangle);
				item.orient = 0;
				while (item.orient < 3)
				{
					item.Sym(ref otri);
					item.SegPivot(ref osub);
					if (otri.triangle != Mesh.dummytri && !otri.IsInfected() && osub.seg == Mesh.dummysub)
					{
						otri.Infect();
						this.viri.Add(otri.triangle);
					}
					item.orient = item.orient + 1;
				}
				item.Infect();
			}
			foreach (Triangle virus in this.viri)
			{
				virus.infected = false;
			}
			this.viri.Clear();
		}
	}
}