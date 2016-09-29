using System;

namespace TriangleNet.Geometry
{
	public class BoundingBox
	{
		private double xmin;

		private double ymin;

		private double xmax;

		private double ymax;

		public double Height
		{
			get
			{
				return this.ymax - this.ymin;
			}
		}

		public double Width
		{
			get
			{
				return this.xmax - this.xmin;
			}
		}

		public double Xmax
		{
			get
			{
				return this.xmax;
			}
		}

		public double Xmin
		{
			get
			{
				return this.xmin;
			}
		}

		public double Ymax
		{
			get
			{
				return this.ymax;
			}
		}

		public double Ymin
		{
			get
			{
				return this.ymin;
			}
		}

		public BoundingBox()
		{
			this.xmin = double.MaxValue;
			this.ymin = double.MaxValue;
			this.xmax = double.MinValue;
			this.ymax = double.MinValue;
		}

		public BoundingBox(double xmin, double ymin, double xmax, double ymax)
		{
			this.xmin = xmin;
			this.ymin = ymin;
			this.xmax = xmax;
			this.ymax = ymax;
		}

		public bool Contains(Point pt)
		{
			if (pt.x < this.xmin || pt.x > this.xmax || pt.y < this.ymin)
			{
				return false;
			}
			return pt.y <= this.ymax;
		}

		public void Scale(double dx, double dy)
		{
			this.xmin = this.xmin - dx;
			this.xmax = this.xmax + dx;
			this.ymin = this.ymin - dy;
			this.ymax = this.ymax + dy;
		}

		public void Update(double x, double y)
		{
			this.xmin = Math.Min(this.xmin, x);
			this.ymin = Math.Min(this.ymin, y);
			this.xmax = Math.Max(this.xmax, x);
			this.ymax = Math.Max(this.ymax, y);
		}
	}
}