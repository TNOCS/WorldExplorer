using System;
using System.Collections.Generic;
using System.Linq;
using TriangleNet;
using TriangleNet.Data;
using TriangleNet.Geometry;

namespace TriangleNet.Tools
{
	public class QuadTree
	{
		private QuadNode root;

		internal ITriangle[] triangles;

		internal int sizeBound;

		internal int maxDepth;

		public QuadTree(Mesh mesh, int maxDepth = 10, int sizeBound = 10)
		{
			this.maxDepth = maxDepth;
			this.sizeBound = sizeBound;
			this.triangles = mesh.Triangles.ToArray<Triangle>();
			int num = 0;
			this.root = new QuadNode(mesh.Bounds, this, true);
			int num1 = num + 1;
			num = num1;
			this.root.CreateSubRegion(num1);
		}

		internal static double DotProduct(Point p, Point q)
		{
			return p.X * q.X + p.Y * q.Y;
		}

		internal static bool IsPointInTriangle(Point p, Point t0, Point t1, Point t2)
		{
			Point point = new Point(t1.X - t0.X, t1.Y - t0.Y);
			Point point1 = new Point(t2.X - t0.X, t2.Y - t0.Y);
			Point point2 = new Point(p.X - t0.X, p.Y - t0.Y);
			Point point3 = new Point(-point.Y, point.X);
			Point point4 = new Point(-point1.Y, point1.X);
			double num = QuadTree.DotProduct(point2, point4) / QuadTree.DotProduct(point, point4);
			double num1 = QuadTree.DotProduct(point2, point3) / QuadTree.DotProduct(point1, point3);
			if (num >= 0 && num1 >= 0 && num + num1 <= 1)
			{
				return true;
			}
			return false;
		}

		public ITriangle Query(double x, double y)
		{
			Point point = new Point(x, y);
			List<int> nums = this.root.FindTriangles(point);
			List<ITriangle> triangles = new List<ITriangle>();
			foreach (int num in nums)
			{
				ITriangle triangle = this.triangles[num];
				if (!QuadTree.IsPointInTriangle(point, triangle.GetVertex(0), triangle.GetVertex(1), triangle.GetVertex(2)))
				{
					continue;
				}
				triangles.Add(triangle);
			}
			return triangles.FirstOrDefault<ITriangle>();
		}
	}
}