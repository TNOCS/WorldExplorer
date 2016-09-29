using System;

namespace TriangleNet.Geometry
{
	public class Point : IComparable<Point>, IEquatable<Point>
	{
		internal int id;

		internal double x;

		internal double y;

		internal int mark;

		internal double[] attributes;

		public double[] Attributes
		{
			get
			{
				return this.attributes;
			}
		}

		public int Boundary
		{
			get
			{
				return this.mark;
			}
		}

		public int ID
		{
			get
			{
				return this.id;
			}
		}

		public double X
		{
			get
			{
				return this.x;
			}
		}

		public double Y
		{
			get
			{
				return this.y;
			}
		}

		public Point() : this(0, 0, 0)
		{
		}

		public Point(double x, double y) : this(x, y, 0)
		{
		}

		public Point(double x, double y, int mark)
		{
			this.x = x;
			this.y = y;
			this.mark = mark;
		}

		public int CompareTo(Point other)
		{
			if (this.x == other.x && this.y == other.y)
			{
				return 0;
			}
			if (this.x >= other.x && (this.x != other.x || this.y >= other.y))
			{
				return 1;
			}
			return -1;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			Point point = obj as Point;
			if (point == null)
			{
				return false;
			}
			if (this.x != point.x)
			{
				return false;
			}
			return this.y == point.y;
		}

		public bool Equals(Point p)
		{
			if (p == null)
			{
				return false;
			}
			if (this.x != p.x)
			{
				return false;
			}
			return this.y == p.y;
		}

		public override int GetHashCode()
		{
			return this.x.GetHashCode() ^ this.y.GetHashCode();
		}

		public static bool operator ==(Point a, Point b)
		{
			if ((object)a == (object)b)
			{
				return true;
			}
			if (a == null || b == null)
			{
				return false;
			}
			return a.Equals(b);
		}

		public static bool operator !=(Point a, Point b)
		{
			return !(a == b);
		}

		public override string ToString()
		{
			return string.Format("[{0},{1}]", this.x, this.y);
		}
	}
}