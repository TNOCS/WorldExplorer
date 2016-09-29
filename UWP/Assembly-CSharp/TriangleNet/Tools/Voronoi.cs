using System;
using System.Collections.Generic;
using TriangleNet;
using TriangleNet.Data;
using TriangleNet.Geometry;

namespace TriangleNet.Tools
{
	public class Voronoi : IVoronoi
	{
		private Mesh mesh;

		private Point[] points;

		private List<VoronoiRegion> regions;

		private Dictionary<int, Point> rayPoints;

		private int rayIndex;

		private BoundingBox bounds;

		public Point[] Points
		{
			get
			{
				return this.points;
			}
		}

		public List<VoronoiRegion> Regions
		{
			get
			{
				return this.regions;
			}
		}

		public Voronoi(Mesh mesh)
		{
			this.mesh = mesh;
			this.Generate();
		}

		private bool BoxRayIntersection(Point pt, double dx, double dy, out Vertex intersect)
		{
			double num;
			double num1;
			double num2;
			double num3;
			double num4;
			double num5;
			double x = pt.X;
			double y = pt.Y;
			double xmin = this.bounds.Xmin;
			double xmax = this.bounds.Xmax;
			double ymin = this.bounds.Ymin;
			double ymax = this.bounds.Ymax;
			if (x < xmin || x > xmax || y < ymin || y > ymax)
			{
				intersect = null;
				return false;
			}
			if (dx < 0)
			{
				num = (xmin - x) / dx;
				num1 = xmin;
				num2 = y + num * dy;
			}
			else if (dx <= 0)
			{
				num = double.MaxValue;
				double num6 = 0;
				num2 = num6;
				num1 = num6;
			}
			else
			{
				num = (xmax - x) / dx;
				num1 = xmax;
				num2 = y + num * dy;
			}
			if (dy < 0)
			{
				num3 = (ymin - y) / dy;
				num4 = x + num3 * dx;
				num5 = ymin;
			}
			else if (dx <= 0)
			{
				num3 = double.MaxValue;
				double num7 = 0;
				num5 = num7;
				num4 = num7;
			}
			else
			{
				num3 = (ymax - y) / dy;
				num4 = x + num3 * dx;
				num5 = ymax;
			}
			if (num >= num3)
			{
				intersect = new Vertex(num4, num5, -1);
			}
			else
			{
				intersect = new Vertex(num1, num2, -1);
			}
			return true;
		}

		private void ComputeCircumCenters()
		{
			Otri otri = new Otri();
			double num = 0;
			double num1 = 0;
			foreach (Triangle value in this.mesh.triangles.Values)
			{
				otri.triangle = value;
				Point point = Primitives.FindCircumcenter(otri.Org(), otri.Dest(), otri.Apex(), ref num, ref num1);
				point.id = value.id;
				this.points[value.id] = point;
				this.bounds.Update(point.x, point.y);
			}
			double num2 = Math.Max(this.bounds.Width, this.bounds.Height);
			this.bounds.Scale(num2, num2);
		}

		private void ConstructVoronoiRegion(Vertex vertex)
		{
			Vertex vertex1;
			Vertex vertex2;
			VoronoiRegion voronoiRegion = new VoronoiRegion(vertex);
			this.regions.Add(voronoiRegion);
			List<Point> points = new List<Point>();
			Otri otri = new Otri();
			Otri otri1 = new Otri();
			Otri otri2 = new Otri();
			Otri otri3 = new Otri();
			Osub osub = new Osub();
			vertex.tri.Copy(ref otri1);
			otri1.Copy(ref otri);
			otri1.Onext(ref otri2);
			if (otri2.triangle == Mesh.dummytri)
			{
				otri1.Oprev(ref otri3);
				if (otri3.triangle != Mesh.dummytri)
				{
					otri1.Copy(ref otri2);
					otri1.OprevSelf();
					otri1.Copy(ref otri);
				}
			}
			while (otri2.triangle != Mesh.dummytri)
			{
				points.Add(this.points[otri.triangle.id]);
				if (otri2.Equal(otri1))
				{
					voronoiRegion.Add(points);
					return;
				}
				otri2.Copy(ref otri);
				otri2.OnextSelf();
			}
			voronoiRegion.Bounded = false;
			int count = this.mesh.triangles.Count;
			otri.Lprev(ref otri2);
			otri2.SegPivot(ref osub);
			int num = osub.seg.hash;
			points.Add(this.points[otri.triangle.id]);
			if (!this.rayPoints.ContainsKey(num))
			{
				vertex1 = otri.Org();
				Vertex vertex3 = otri.Apex();
				this.BoxRayIntersection(this.points[otri.triangle.id], vertex1.y - vertex3.y, vertex3.x - vertex1.x, out vertex2);
				vertex2.id = count + this.rayIndex;
				this.points[count + this.rayIndex] = vertex2;
				this.rayIndex = this.rayIndex + 1;
				points.Add(vertex2);
				this.rayPoints.Add(num, vertex2);
			}
			else
			{
				points.Add(this.rayPoints[num]);
			}
			points.Reverse();
			otri1.Copy(ref otri);
			otri.Oprev(ref otri3);
			while (otri3.triangle != Mesh.dummytri)
			{
				points.Add(this.points[otri3.triangle.id]);
				otri3.Copy(ref otri);
				otri3.OprevSelf();
			}
			otri.SegPivot(ref osub);
			num = osub.seg.hash;
			if (!this.rayPoints.ContainsKey(num))
			{
				vertex1 = otri.Org();
				Vertex vertex4 = otri.Dest();
				this.BoxRayIntersection(this.points[otri.triangle.id], vertex4.y - vertex1.y, vertex1.x - vertex4.x, out vertex2);
				vertex2.id = count + this.rayIndex;
				this.points[count + this.rayIndex] = vertex2;
				this.rayIndex = this.rayIndex + 1;
				points.Add(vertex2);
				this.rayPoints.Add(num, vertex2);
			}
			else
			{
				points.Add(this.rayPoints[num]);
			}
			points.Reverse();
			voronoiRegion.Add(points);
		}

		private void Generate()
		{
			this.mesh.Renumber();
			this.mesh.MakeVertexMap();
			this.points = new Point[this.mesh.triangles.Count + this.mesh.hullsize];
			this.regions = new List<VoronoiRegion>(this.mesh.vertices.Count);
			this.rayPoints = new Dictionary<int, Point>();
			this.rayIndex = 0;
			this.bounds = new BoundingBox();
			this.ComputeCircumCenters();
			foreach (Vertex value in this.mesh.vertices.Values)
			{
				this.ConstructVoronoiRegion(value);
			}
		}
	}
}