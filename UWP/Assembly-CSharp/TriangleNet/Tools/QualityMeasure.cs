using System;
using System.Collections.Generic;
using TriangleNet;
using TriangleNet.Data;
using TriangleNet.Geometry;

namespace TriangleNet.Tools
{
	public class QualityMeasure
	{
		private QualityMeasure.AreaMeasure areaMeasure;

		private QualityMeasure.AlphaMeasure alphaMeasure;

		private QualityMeasure.Q_Measure qMeasure;

		private Mesh mesh;

		public double AlphaArea
		{
			get
			{
				return this.alphaMeasure.alpha_area;
			}
		}

		public double AlphaAverage
		{
			get
			{
				return this.alphaMeasure.alpha_ave;
			}
		}

		public double AlphaMaximum
		{
			get
			{
				return this.alphaMeasure.alpha_max;
			}
		}

		public double AlphaMinimum
		{
			get
			{
				return this.alphaMeasure.alpha_min;
			}
		}

		public double AreaMaximum
		{
			get
			{
				return this.areaMeasure.area_max;
			}
		}

		public double AreaMinimum
		{
			get
			{
				return this.areaMeasure.area_min;
			}
		}

		public double AreaRatio
		{
			get
			{
				return this.areaMeasure.area_max / this.areaMeasure.area_min;
			}
		}

		public double Q_Area
		{
			get
			{
				return this.qMeasure.q_area;
			}
		}

		public double Q_Average
		{
			get
			{
				return this.qMeasure.q_ave;
			}
		}

		public double Q_Maximum
		{
			get
			{
				return this.qMeasure.q_max;
			}
		}

		public double Q_Minimum
		{
			get
			{
				return this.qMeasure.q_min;
			}
		}

		public QualityMeasure()
		{
			this.areaMeasure = new QualityMeasure.AreaMeasure();
			this.alphaMeasure = new QualityMeasure.AlphaMeasure();
			this.qMeasure = new QualityMeasure.Q_Measure();
		}

		public int Bandwidth()
		{
			if (this.mesh == null)
			{
				return 0;
			}
			int num = 0;
			int num1 = 0;
			foreach (Triangle value in this.mesh.triangles.Values)
			{
				for (int i = 0; i < 3; i++)
				{
					int vertex = value.GetVertex(i).id;
					for (int j = 0; j < 3; j++)
					{
						int vertex1 = value.GetVertex(j).id;
						num1 = Math.Max(num1, vertex1 - vertex);
						num = Math.Max(num, vertex - vertex1);
					}
				}
			}
			return num + 1 + num1;
		}

		private void Compute()
		{
			int num = 0;
			foreach (Triangle value in this.mesh.triangles.Values)
			{
				num++;
				Point point = value.vertices[0];
				Point point1 = value.vertices[1];
				Point point2 = value.vertices[2];
				double num1 = point.x - point1.x;
				double num2 = point.y - point1.y;
				double num3 = Math.Sqrt(num1 * num1 + num2 * num2);
				double num4 = point1.x - point2.x;
				num2 = point1.y - point2.y;
				double num5 = Math.Sqrt(num4 * num4 + num2 * num2);
				double num6 = point2.x - point.x;
				num2 = point2.y - point.y;
				double num7 = Math.Sqrt(num6 * num6 + num2 * num2);
				double num8 = this.areaMeasure.Measure(point, point1, point2);
				this.alphaMeasure.Measure(num3, num5, num7, num8);
				this.qMeasure.Measure(num3, num5, num7, num8);
			}
			this.alphaMeasure.Normalize(num, this.areaMeasure.area_total);
			this.qMeasure.Normalize(num, this.areaMeasure.area_total);
		}

		public void Update(Mesh mesh)
		{
			this.mesh = mesh;
			this.areaMeasure.Reset();
			this.alphaMeasure.Reset();
			this.qMeasure.Reset();
			this.Compute();
		}

		private class AlphaMeasure
		{
			public double alpha_min;

			public double alpha_max;

			public double alpha_ave;

			public double alpha_area;

			public AlphaMeasure()
			{
			}

			private double acos(double c)
			{
				if (c <= -1)
				{
					return 3.14159265358979;
				}
				if (1 <= c)
				{
					return 0;
				}
				return Math.Acos(c);
			}

			public double Measure(double ab, double bc, double ca, double area)
			{
				double num;
				double num1;
				double num2;
				double num3 = double.MaxValue;
				double num4 = ab * ab;
				double num5 = bc * bc;
				double num6 = ca * ca;
				if (ab != 0 || bc != 0 || ca != 0)
				{
					num = (ca == 0 || ab == 0 ? 3.14159265358979 : this.acos((num6 + num4 - num5) / (2 * ca * ab)));
					num1 = (ab == 0 || bc == 0 ? 3.14159265358979 : this.acos((num4 + num5 - num6) / (2 * ab * bc)));
					num2 = (bc == 0 || ca == 0 ? 3.14159265358979 : this.acos((num5 + num6 - num4) / (2 * bc * ca)));
				}
				else
				{
					num = 2.0943951023932;
					num1 = 2.0943951023932;
					num2 = 2.0943951023932;
				}
				num3 = Math.Min(num3, num);
				num3 = Math.Min(num3, num1);
				num3 = Math.Min(num3, num2);
				num3 = num3 * 3 / 3.14159265358979;
				this.alpha_ave = this.alpha_ave + num3;
				this.alpha_area = this.alpha_area + area * num3;
				this.alpha_min = Math.Min(num3, this.alpha_min);
				this.alpha_max = Math.Max(num3, this.alpha_max);
				return num3;
			}

			public void Normalize(int n, double area_total)
			{
				if (n <= 0)
				{
					this.alpha_ave = 0;
				}
				else
				{
					this.alpha_ave = this.alpha_ave / (double)n;
				}
				if (0 >= area_total)
				{
					this.alpha_area = 0;
					return;
				}
				this.alpha_area = this.alpha_area / area_total;
			}

			public void Reset()
			{
				this.alpha_min = double.MaxValue;
				this.alpha_max = double.MinValue;
				this.alpha_ave = 0;
				this.alpha_area = 0;
			}
		}

		private class AreaMeasure
		{
			public double area_min;

			public double area_max;

			public double area_total;

			public int area_zero;

			public AreaMeasure()
			{
			}

			public double Measure(Point a, Point b, Point c)
			{
				double num = 0.5 * Math.Abs(a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y));
				this.area_min = Math.Min(this.area_min, num);
				this.area_max = Math.Max(this.area_max, num);
				this.area_total = this.area_total + num;
				if (num == 0)
				{
					this.area_zero = this.area_zero + 1;
				}
				return num;
			}

			public void Reset()
			{
				this.area_min = double.MaxValue;
				this.area_max = double.MinValue;
				this.area_total = 0;
				this.area_zero = 0;
			}
		}

		private class Q_Measure
		{
			public double q_min;

			public double q_max;

			public double q_ave;

			public double q_area;

			public Q_Measure()
			{
			}

			public double Measure(double ab, double bc, double ca, double area)
			{
				double num = (bc + ca - ab) * (ca + ab - bc) * (ab + bc - ca) / (ab * bc * ca);
				this.q_min = Math.Min(this.q_min, num);
				this.q_max = Math.Max(this.q_max, num);
				this.q_ave = this.q_ave + num;
				this.q_area = this.q_area + num * area;
				return num;
			}

			public void Normalize(int n, double area_total)
			{
				if (n <= 0)
				{
					this.q_ave = 0;
				}
				else
				{
					this.q_ave = this.q_ave / (double)n;
				}
				if (area_total <= 0)
				{
					this.q_area = 0;
					return;
				}
				this.q_area = this.q_area / area_total;
			}

			public void Reset()
			{
				this.q_min = double.MaxValue;
				this.q_max = double.MinValue;
				this.q_ave = 0;
				this.q_area = 0;
			}
		}
	}
}