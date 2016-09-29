using System;
using System.Collections.Generic;
using TriangleNet.Geometry;

namespace TriangleNet.Tools
{
	internal class QuadNode
	{
		private const int SW = 0;

		private const int SE = 1;

		private const int NW = 2;

		private const int NE = 3;

		private const double EPS = 1E-06;

		private readonly static byte[] BITVECTOR;

		private BoundingBox bounds;

		private Point pivot;

		private QuadTree tree;

		private QuadNode[] regions;

		private List<int> triangles;

		private byte bitRegions;

		static QuadNode()
		{
			QuadNode.BITVECTOR = new byte[] { 1, 2, 4, 8 };
		}

		public QuadNode(BoundingBox box, QuadTree tree) : this(box, tree, false)
		{
		}

		public QuadNode(BoundingBox box, QuadTree tree, bool init)
		{
			this.tree = tree;
			this.bounds = new BoundingBox(box.Xmin, box.Ymin, box.Xmax, box.Ymax);
			this.pivot = new Point((box.Xmin + box.Xmax) / 2, (box.Ymin + box.Ymax) / 2);
			this.bitRegions = 0;
			this.regions = new QuadNode[4];
			this.triangles = new List<int>();
			if (init)
			{
				this.triangles.Capacity = (int)tree.triangles.Length;
				ITriangle[] triangleArray = tree.triangles;
				for (int i = 0; i < (int)triangleArray.Length; i++)
				{
					ITriangle triangle = triangleArray[i];
					this.triangles.Add(triangle.ID);
				}
			}
		}

		private void AddToRegion(int index, int region)
		{
			if ((this.bitRegions & QuadNode.BITVECTOR[region]) == 0)
			{
				this.regions[region].triangles.Add(index);
				this.bitRegions = (byte)(this.bitRegions | QuadNode.BITVECTOR[region]);
			}
		}

		private void AddTriangleToRegion(Point[] triangle, int index)
		{
			this.bitRegions = 0;
			if (QuadTree.IsPointInTriangle(this.pivot, triangle[0], triangle[1], triangle[2]))
			{
				this.AddToRegion(index, 0);
				this.AddToRegion(index, 1);
				this.AddToRegion(index, 2);
				this.AddToRegion(index, 3);
				return;
			}
			this.FindTriangleIntersections(triangle, index);
			if (this.bitRegions == 0)
			{
				int num = this.FindRegion(triangle[0]);
				this.regions[num].triangles.Add(index);
			}
		}

		public void CreateSubRegion(int currentDepth)
		{
			BoundingBox boundingBox = new BoundingBox(this.bounds.Xmin, this.bounds.Ymin, this.pivot.X, this.pivot.Y);
			this.regions[0] = new QuadNode(boundingBox, this.tree);
			boundingBox = new BoundingBox(this.pivot.X, this.bounds.Ymin, this.bounds.Xmax, this.pivot.Y);
			this.regions[1] = new QuadNode(boundingBox, this.tree);
			boundingBox = new BoundingBox(this.bounds.Xmin, this.pivot.Y, this.pivot.X, this.bounds.Ymax);
			this.regions[2] = new QuadNode(boundingBox, this.tree);
			boundingBox = new BoundingBox(this.pivot.X, this.pivot.Y, this.bounds.Xmax, this.bounds.Ymax);
			this.regions[3] = new QuadNode(boundingBox, this.tree);
			Point[] vertex = new Point[3];
			foreach (int triangle in this.triangles)
			{
				ITriangle triangle1 = this.tree.triangles[triangle];
				vertex[0] = triangle1.GetVertex(0);
				vertex[1] = triangle1.GetVertex(1);
				vertex[2] = triangle1.GetVertex(2);
				this.AddTriangleToRegion(vertex, triangle1.ID);
			}
			for (int i = 0; i < 4; i++)
			{
				if (this.regions[i].triangles.Count > this.tree.sizeBound && currentDepth < this.tree.maxDepth)
				{
					this.regions[i].CreateSubRegion(currentDepth + 1);
				}
			}
		}

		private void FindIntersectionsWithX(double dx, double dy, Point[] triangle, int index, int k)
		{
			double x = (this.pivot.X - triangle[k].X) / dx;
			if (x < 1.000001 && x > -1E-06)
			{
				double y = triangle[k].Y + x * dy;
				if (y < this.pivot.Y)
				{
					if (y >= this.bounds.Ymin)
					{
						this.AddToRegion(index, 0);
						this.AddToRegion(index, 1);
					}
				}
				else if (y <= this.bounds.Ymax)
				{
					this.AddToRegion(index, 2);
					this.AddToRegion(index, 3);
				}
			}
			x = (this.bounds.Xmin - triangle[k].X) / dx;
			if (x < 1.000001 && x > -1E-06)
			{
				double num = triangle[k].Y + x * dy;
				if (num <= this.pivot.Y && num >= this.bounds.Ymin)
				{
					this.AddToRegion(index, 0);
				}
				else if (num >= this.pivot.Y && num <= this.bounds.Ymax)
				{
					this.AddToRegion(index, 2);
				}
			}
			x = (this.bounds.Xmax - triangle[k].X) / dx;
			if (x < 1.000001 && x > -1E-06)
			{
				double y1 = triangle[k].Y + x * dy;
				if (y1 <= this.pivot.Y && y1 >= this.bounds.Ymin)
				{
					this.AddToRegion(index, 1);
					return;
				}
				if (y1 >= this.pivot.Y && y1 <= this.bounds.Ymax)
				{
					this.AddToRegion(index, 3);
				}
			}
		}

		private void FindIntersectionsWithY(double dx, double dy, Point[] triangle, int index, int k)
		{
			double y = (this.pivot.Y - triangle[k].Y) / dy;
			if (y < 1.000001 && y > -1E-06)
			{
				double x = triangle[k].X + y * dy;
				if (x > this.pivot.X)
				{
					if (x <= this.bounds.Xmax)
					{
						this.AddToRegion(index, 1);
						this.AddToRegion(index, 3);
					}
				}
				else if (x >= this.bounds.Xmin)
				{
					this.AddToRegion(index, 0);
					this.AddToRegion(index, 2);
				}
			}
			y = (this.bounds.Ymin - triangle[k].Y) / dy;
			if (y < 1.000001 && y > -1E-06)
			{
				double num = triangle[k].X + y * dx;
				if (num <= this.pivot.X && num >= this.bounds.Xmin)
				{
					this.AddToRegion(index, 0);
				}
				else if (num >= this.pivot.X && num <= this.bounds.Xmax)
				{
					this.AddToRegion(index, 1);
				}
			}
			y = (this.bounds.Ymax - triangle[k].Y) / dy;
			if (y < 1.000001 && y > -1E-06)
			{
				double x1 = triangle[k].X + y * dx;
				if (x1 <= this.pivot.X && x1 >= this.bounds.Xmin)
				{
					this.AddToRegion(index, 2);
					return;
				}
				if (x1 >= this.pivot.X && x1 <= this.bounds.Xmax)
				{
					this.AddToRegion(index, 3);
				}
			}
		}

		private int FindRegion(Point point)
		{
			int num = 2;
			if (point.Y < this.pivot.Y)
			{
				num = 0;
			}
			if (point.X > this.pivot.X)
			{
				num++;
			}
			return num;
		}

		private void FindTriangleIntersections(Point[] triangle, int index)
		{
			int num = 2;
			int num1 = 0;
			while (num1 < 3)
			{
				double x = triangle[num1].X - triangle[num].X;
				double y = triangle[num1].Y - triangle[num].Y;
				if (x != 0)
				{
					this.FindIntersectionsWithX(x, y, triangle, index, num);
				}
				if (y != 0)
				{
					this.FindIntersectionsWithY(x, y, triangle, index, num);
				}
				int num2 = num1;
				num1 = num2 + 1;
				num = num2;
			}
		}

		public List<int> FindTriangles(Point searchPoint)
		{
			int num = this.FindRegion(searchPoint);
			if (this.regions[num] == null)
			{
				return this.triangles;
			}
			return this.regions[num].FindTriangles(searchPoint);
		}
	}
}