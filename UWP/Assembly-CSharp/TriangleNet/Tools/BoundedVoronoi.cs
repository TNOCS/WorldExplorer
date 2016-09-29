using System;
using System.Collections.Generic;
using TriangleNet;
using TriangleNet.Data;
using TriangleNet.Geometry;

namespace TriangleNet.Tools
{
	public class BoundedVoronoi : IVoronoi
	{
		private Mesh mesh;

		private Point[] points;

		private List<VoronoiRegion> regions;

		private int segIndex;

		private Dictionary<int, Segment> subsegMap;

		private bool includeBoundary = true;

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

		public BoundedVoronoi(Mesh mesh) : this(mesh, true)
		{
		}

		public BoundedVoronoi(Mesh mesh, bool includeBoundary)
		{
			this.mesh = mesh;
			this.includeBoundary = includeBoundary;
			this.Generate();
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
			}
		}

		private void ConstructBoundaryBvdCell(Vertex vertex)
		{
			Vertex vertex1;
			Point point;
			VoronoiRegion voronoiRegion = new VoronoiRegion(vertex);
			this.regions.Add(voronoiRegion);
			Otri otri = new Otri();
			Otri otri1 = new Otri();
			Otri otri2 = new Otri();
			Otri otri3 = new Otri();
			Osub item = new Osub();
			Osub osub = new Osub();
			int count = this.mesh.triangles.Count;
			List<Point> points = new List<Point>();
			vertex.tri.Copy(ref otri1);
			if (otri1.Org() != vertex)
			{
				throw new Exception("ConstructBoundaryBvdCell: inconsistent topology.");
			}
			otri1.Copy(ref otri);
			otri1.Onext(ref otri2);
			otri1.Oprev(ref otri3);
			if (otri3.triangle != Mesh.dummytri)
			{
				while (otri3.triangle != Mesh.dummytri && !otri3.Equal(otri1))
				{
					otri3.Copy(ref otri);
					otri3.OprevSelf();
				}
				otri.Copy(ref otri1);
				otri.Onext(ref otri2);
			}
			if (otri3.triangle == Mesh.dummytri)
			{
				point = new Point(vertex.x, vertex.y)
				{
					id = count + this.segIndex
				};
				this.points[count + this.segIndex] = point;
				this.segIndex = this.segIndex + 1;
				points.Add(point);
			}
			Vertex vertex2 = otri.Org();
			Vertex vertex3 = otri.Dest();
			point = new Point((vertex2.X + vertex3.X) / 2, (vertex2.Y + vertex3.Y) / 2)
			{
				id = count + this.segIndex
			};
			this.points[count + this.segIndex] = point;
			this.segIndex = this.segIndex + 1;
			points.Add(point);
			do
			{
				Point point1 = this.points[otri.triangle.id];
				if (otri2.triangle != Mesh.dummytri)
				{
					Point point2 = this.points[otri2.triangle.id];
					if (otri.triangle.infected)
					{
						item.seg = this.subsegMap[otri.triangle.hash];
						Vertex vertex4 = item.SegOrg();
						Vertex vertex5 = item.SegDest();
						if (otri2.triangle.infected)
						{
							osub.seg = this.subsegMap[otri2.triangle.hash];
							if (!item.Equal(osub))
							{
								if (this.SegmentsIntersect(vertex4, vertex5, point1, point2, out point, true))
								{
									point.id = count + this.segIndex;
									this.points[count + this.segIndex] = point;
									this.segIndex = this.segIndex + 1;
									points.Add(point);
								}
								if (this.SegmentsIntersect(osub.SegOrg(), osub.SegDest(), point1, point2, out point, true))
								{
									point.id = count + this.segIndex;
									this.points[count + this.segIndex] = point;
									this.segIndex = this.segIndex + 1;
									points.Add(point);
								}
							}
							else if (this.SegmentsIntersect(vertex4, vertex5, new Point((vertex2.X + vertex3.X) / 2, (vertex2.Y + vertex3.Y) / 2), point2, out point, false))
							{
								point.id = count + this.segIndex;
								this.points[count + this.segIndex] = point;
								this.segIndex = this.segIndex + 1;
								points.Add(point);
							}
						}
						else
						{
							vertex3 = otri.Dest();
							vertex1 = otri.Apex();
							if (this.SegmentsIntersect(vertex4, vertex5, new Point((vertex3.X + vertex1.X) / 2, (vertex3.Y + vertex1.Y) / 2), point1, out point, false))
							{
								point.id = count + this.segIndex;
								this.points[count + this.segIndex] = point;
								this.segIndex = this.segIndex + 1;
								points.Add(point);
							}
							if (this.SegmentsIntersect(vertex4, vertex5, point1, point2, out point, true))
							{
								point.id = count + this.segIndex;
								this.points[count + this.segIndex] = point;
								this.segIndex = this.segIndex + 1;
								points.Add(point);
							}
						}
					}
					else
					{
						points.Add(point1);
						if (otri2.triangle.infected)
						{
							osub.seg = this.subsegMap[otri2.triangle.hash];
							if (this.SegmentsIntersect(osub.SegOrg(), osub.SegDest(), point1, point2, out point, true))
							{
								point.id = count + this.segIndex;
								this.points[count + this.segIndex] = point;
								this.segIndex = this.segIndex + 1;
								points.Add(point);
							}
						}
					}
					otri2.Copy(ref otri);
					otri2.OnextSelf();
				}
				else
				{
					if (!otri.triangle.infected)
					{
						points.Add(point1);
					}
					vertex2 = otri.Org();
					vertex1 = otri.Apex();
					point = new Point((vertex2.X + vertex1.X) / 2, (vertex2.Y + vertex1.Y) / 2)
					{
						id = count + this.segIndex
					};
					this.points[count + this.segIndex] = point;
					this.segIndex = this.segIndex + 1;
					points.Add(point);
					break;
				}
			}
			while (!otri.Equal(otri1));
			voronoiRegion.Add(points);
		}

		private void ConstructBvdCell(Vertex vertex)
		{
			Point point;
			VoronoiRegion voronoiRegion = new VoronoiRegion(vertex);
			this.regions.Add(voronoiRegion);
			Otri otri = new Otri();
			Otri otri1 = new Otri();
			Otri otri2 = new Otri();
			Osub item = new Osub();
			Osub osub = new Osub();
			int count = this.mesh.triangles.Count;
			List<Point> points = new List<Point>();
			vertex.tri.Copy(ref otri1);
			if (otri1.Org() != vertex)
			{
				throw new Exception("ConstructBvdCell: inconsistent topology.");
			}
			otri1.Copy(ref otri);
			otri1.Onext(ref otri2);
			do
			{
				Point point1 = this.points[otri.triangle.id];
				Point point2 = this.points[otri2.triangle.id];
				if (otri.triangle.infected)
				{
					item.seg = this.subsegMap[otri.triangle.hash];
					if (otri2.triangle.infected)
					{
						osub.seg = this.subsegMap[otri2.triangle.hash];
						if (!item.Equal(osub))
						{
							if (this.SegmentsIntersect(item.SegOrg(), item.SegDest(), point1, point2, out point, true))
							{
								point.id = count + this.segIndex;
								this.points[count + this.segIndex] = point;
								this.segIndex = this.segIndex + 1;
								points.Add(point);
							}
							if (this.SegmentsIntersect(osub.SegOrg(), osub.SegDest(), point1, point2, out point, true))
							{
								point.id = count + this.segIndex;
								this.points[count + this.segIndex] = point;
								this.segIndex = this.segIndex + 1;
								points.Add(point);
							}
						}
					}
					else if (this.SegmentsIntersect(item.SegOrg(), item.SegDest(), point1, point2, out point, true))
					{
						point.id = count + this.segIndex;
						this.points[count + this.segIndex] = point;
						this.segIndex = this.segIndex + 1;
						points.Add(point);
					}
				}
				else
				{
					points.Add(point1);
					if (otri2.triangle.infected)
					{
						osub.seg = this.subsegMap[otri2.triangle.hash];
						if (this.SegmentsIntersect(osub.SegOrg(), osub.SegDest(), point1, point2, out point, true))
						{
							point.id = count + this.segIndex;
							this.points[count + this.segIndex] = point;
							this.segIndex = this.segIndex + 1;
							points.Add(point);
						}
					}
				}
				otri2.Copy(ref otri);
				otri2.OnextSelf();
			}
			while (!otri.Equal(otri1));
			voronoiRegion.Add(points);
		}

		private void Generate()
		{
			this.mesh.Renumber();
			this.mesh.MakeVertexMap();
			this.points = new Point[this.mesh.triangles.Count + this.mesh.subsegs.Count * 5];
			this.regions = new List<VoronoiRegion>(this.mesh.vertices.Count);
			this.ComputeCircumCenters();
			this.TagBlindTriangles();
			foreach (Vertex value in this.mesh.vertices.Values)
			{
				if (value.type == VertexType.FreeVertex || value.Boundary == 0)
				{
					this.ConstructBvdCell(value);
				}
				else
				{
					if (!this.includeBoundary)
					{
						continue;
					}
					this.ConstructBoundaryBvdCell(value);
				}
			}
		}

		private bool SegmentsIntersect(Point p1, Point p2, Point p3, Point p4, out Point p, bool strictIntersect)
		{
			p = null;
			double x = p1.X;
			double y = p1.Y;
			double num = p2.X;
			double y1 = p2.Y;
			double x1 = p3.X;
			double num1 = p3.Y;
			double x2 = p4.X;
			double y2 = p4.Y;
			if (x == num && y == y1 || x1 == x2 && num1 == y2)
			{
				return false;
			}
			if (x == x1 && y == num1 || num == x1 && y1 == num1 || x == x2 && y == y2 || num == x2 && y1 == y2)
			{
				return false;
			}
			num = num - x;
			y1 = y1 - y;
			x1 = x1 - x;
			num1 = num1 - y;
			x2 = x2 - x;
			y2 = y2 - y;
			double num2 = Math.Sqrt(num * num + y1 * y1);
			double num3 = num / num2;
			double num4 = y1 / num2;
			double num5 = x1 * num3 + num1 * num4;
			num1 = num1 * num3 - x1 * num4;
			x1 = num5;
			double num6 = x2 * num3 + y2 * num4;
			y2 = y2 * num3 - x2 * num4;
			x2 = num6;
			if (num1 >= 0 || y2 >= 0)
			{
				if (!((num1 < 0 ? false : y2 >= 0) & strictIntersect))
				{
					double num7 = x2 + (x1 - x2) * y2 / (y2 - num1);
					if (num7 < 0 || num7 > num2 & strictIntersect)
					{
						return false;
					}
					p = new Point(x + num7 * num3, y + num7 * num4);
					return true;
				}
			}
			return false;
		}

		private void TagBlindTriangles()
		{
			int num = 0;
			this.subsegMap = new Dictionary<int, Segment>();
			Otri otri = new Otri();
			Otri otri1 = new Otri();
			Osub osub = new Osub();
			Osub osub1 = new Osub();
			foreach (Triangle value in this.mesh.triangles.Values)
			{
				value.infected = false;
			}
			foreach (Segment segment in this.mesh.subsegs.Values)
			{
				Stack<Triangle> triangles = new Stack<Triangle>();
				osub.seg = segment;
				osub.orient = 0;
				osub.TriPivot(ref otri);
				if (otri.triangle != Mesh.dummytri && !otri.triangle.infected)
				{
					triangles.Push(otri.triangle);
				}
				osub.SymSelf();
				osub.TriPivot(ref otri);
				if (otri.triangle != Mesh.dummytri && !otri.triangle.infected)
				{
					triangles.Push(otri.triangle);
				}
				while (triangles.Count > 0)
				{
					otri.triangle = triangles.Pop();
					otri.orient = 0;
					if (!this.TriangleIsBlinded(ref otri, ref osub))
					{
						continue;
					}
					otri.triangle.infected = true;
					num++;
					this.subsegMap.Add(otri.triangle.hash, osub.seg);
					otri.orient = 0;
					while (otri.orient < 3)
					{
						otri.Sym(ref otri1);
						otri1.SegPivot(ref osub1);
						if (otri1.triangle != Mesh.dummytri && !otri1.triangle.infected && osub1.seg == Mesh.dummysub)
						{
							triangles.Push(otri1.triangle);
						}
						otri.orient = otri.orient + 1;
					}
				}
			}
			num = 0;
		}

		private bool TriangleIsBlinded(ref Otri tri, ref Osub seg)
		{
			Point point;
			Vertex vertex = tri.Org();
			Vertex vertex1 = tri.Dest();
			Vertex vertex2 = tri.Apex();
			Vertex vertex3 = seg.Org();
			Vertex vertex4 = seg.Dest();
			Point point1 = this.points[tri.triangle.id];
			if (this.SegmentsIntersect(vertex3, vertex4, point1, vertex, out point, true))
			{
				return true;
			}
			if (this.SegmentsIntersect(vertex3, vertex4, point1, vertex1, out point, true))
			{
				return true;
			}
			if (this.SegmentsIntersect(vertex3, vertex4, point1, vertex2, out point, true))
			{
				return true;
			}
			return false;
		}
	}
}