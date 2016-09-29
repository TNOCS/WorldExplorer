using System;
using System.Collections.Generic;
using TriangleNet;
using TriangleNet.Data;
using TriangleNet.Geometry;

namespace TriangleNet.Tools
{
	public class Statistic
	{
		public static long InCircleCount;

		public static long InCircleCountDecimal;

		public static long CounterClockwiseCount;

		public static long CounterClockwiseCountDecimal;

		public static long Orient3dCount;

		public static long HyperbolaCount;

		public static long CircumcenterCount;

		public static long CircleTopCount;

		public static long RelocationCount;

		private double minEdge;

		private double maxEdge;

		private double minAspect;

		private double maxAspect;

		private double minArea;

		private double maxArea;

		private double minAngle;

		private double maxAngle;

		private int inVetrices;

		private int inTriangles;

		private int inSegments;

		private int inHoles;

		private int outVertices;

		private int outTriangles;

		private int outEdges;

		private int boundaryEdges;

		private int intBoundaryEdges;

		private int constrainedEdges;

		private int[] angleTable;

		private int[] minAngles;

		private int[] maxAngles;

		private readonly static int[] plus1Mod3;

		private readonly static int[] minus1Mod3;

		public int[] AngleHistogram
		{
			get
			{
				return this.angleTable;
			}
		}

		public int BoundaryEdges
		{
			get
			{
				return this.boundaryEdges;
			}
		}

		public int ConstrainedEdges
		{
			get
			{
				return this.constrainedEdges;
			}
		}

		public int Edges
		{
			get
			{
				return this.outEdges;
			}
		}

		public int InputHoles
		{
			get
			{
				return this.inHoles;
			}
		}

		public int InputSegments
		{
			get
			{
				return this.inSegments;
			}
		}

		public int InputTriangles
		{
			get
			{
				return this.inTriangles;
			}
		}

		public int InputVertices
		{
			get
			{
				return this.inVetrices;
			}
		}

		public int InteriorBoundaryEdges
		{
			get
			{
				return this.intBoundaryEdges;
			}
		}

		public double LargestAngle
		{
			get
			{
				return this.maxAngle;
			}
		}

		public double LargestArea
		{
			get
			{
				return this.maxArea;
			}
		}

		public double LargestAspectRatio
		{
			get
			{
				return this.maxAspect;
			}
		}

		public double LongestEdge
		{
			get
			{
				return this.maxEdge;
			}
		}

		public int[] MaxAngleHistogram
		{
			get
			{
				return this.maxAngles;
			}
		}

		public int[] MinAngleHistogram
		{
			get
			{
				return this.minAngles;
			}
		}

		public double ShortestAltitude
		{
			get
			{
				return this.minAspect;
			}
		}

		public double ShortestEdge
		{
			get
			{
				return this.minEdge;
			}
		}

		public double SmallestAngle
		{
			get
			{
				return this.minAngle;
			}
		}

		public double SmallestArea
		{
			get
			{
				return this.minArea;
			}
		}

		public int Triangles
		{
			get
			{
				return this.outTriangles;
			}
		}

		public int Vertices
		{
			get
			{
				return this.outVertices;
			}
		}

		static Statistic()
		{
			Statistic.InCircleCount = (long)0;
			Statistic.InCircleCountDecimal = (long)0;
			Statistic.CounterClockwiseCount = (long)0;
			Statistic.CounterClockwiseCountDecimal = (long)0;
			Statistic.Orient3dCount = (long)0;
			Statistic.HyperbolaCount = (long)0;
			Statistic.CircumcenterCount = (long)0;
			Statistic.CircleTopCount = (long)0;
			Statistic.RelocationCount = (long)0;
			Statistic.plus1Mod3 = new int[] { 1, 2, 0 };
			Statistic.minus1Mod3 = new int[] { 2, 0, 1 };
		}

		public Statistic()
		{
		}

		private void GetAspectHistogram(Mesh mesh)
		{
			int[] numArray = new int[16];
			double[] numArray1 = new double[] { 1.5, 2, 2.5, 3, 4, 6, 10, 15, 25, 50, 100, 300, 1000, 10000, 100000, 0 };
			Otri otri = new Otri();
			Vertex[] vertexArray = new Vertex[3];
			double[] numArray2 = new double[3];
			double[] numArray3 = new double[3];
			double[] numArray4 = new double[3];
			otri.orient = 0;
			foreach (Triangle value in mesh.triangles.Values)
			{
				otri.triangle = value;
				vertexArray[0] = otri.Org();
				vertexArray[1] = otri.Dest();
				vertexArray[2] = otri.Apex();
				double num = 0;
				for (int i = 0; i < 3; i++)
				{
					int num1 = Statistic.plus1Mod3[i];
					int num2 = Statistic.minus1Mod3[i];
					numArray2[i] = vertexArray[num1].x - vertexArray[num2].x;
					numArray3[i] = vertexArray[num1].y - vertexArray[num2].y;
					numArray4[i] = numArray2[i] * numArray2[i] + numArray3[i] * numArray3[i];
					if (numArray4[i] > num)
					{
						num = numArray4[i];
					}
				}
				double num3 = Math.Abs((vertexArray[2].x - vertexArray[0].x) * (vertexArray[1].y - vertexArray[0].y) - (vertexArray[1].x - vertexArray[0].x) * (vertexArray[2].y - vertexArray[0].y)) / 2;
				double num4 = num / (num3 * num3 / num);
				int num5 = 0;
				while (num4 > numArray1[num5] * numArray1[num5] && num5 < 15)
				{
					num5++;
				}
				numArray[num5] = numArray[num5] + 1;
			}
		}

		public void Update(Mesh mesh, int sampleDegrees)
		{
			int num;
			int num1;
			int num2;
			this.inVetrices = mesh.invertices;
			this.inTriangles = mesh.inelements;
			this.inSegments = mesh.insegments;
			this.inHoles = mesh.holes.Count;
			this.outVertices = mesh.vertices.Count - mesh.undeads;
			this.outTriangles = mesh.triangles.Count;
			this.outEdges = mesh.edges;
			this.boundaryEdges = mesh.hullsize;
			this.intBoundaryEdges = mesh.subsegs.Count - mesh.hullsize;
			this.constrainedEdges = mesh.subsegs.Count;
			Point[] pointArray = new Point[3];
			sampleDegrees = 60;
			double[] numArray = new double[sampleDegrees / 2 - 1];
			double[] x = new double[3];
			double[] y = new double[3];
			double[] numArray1 = new double[3];
			double num3 = 3.14159265358979 / (double)sampleDegrees;
			double num4 = 57.2957795130823;
			this.angleTable = new int[sampleDegrees];
			this.minAngles = new int[sampleDegrees];
			this.maxAngles = new int[sampleDegrees];
			for (int i = 0; i < sampleDegrees / 2 - 1; i++)
			{
				numArray[i] = Math.Cos(num3 * (double)(i + 1));
				numArray[i] = numArray[i] * numArray[i];
			}
			for (int j = 0; j < sampleDegrees; j++)
			{
				this.angleTable[j] = 0;
			}
			this.minAspect = mesh.bounds.Width + mesh.bounds.Height;
			this.minAspect = this.minAspect * this.minAspect;
			this.maxAspect = 0;
			this.minEdge = this.minAspect;
			this.maxEdge = 0;
			this.minArea = this.minAspect;
			this.maxArea = 0;
			this.minAngle = 0;
			this.maxAngle = 2;
			bool flag = true;
			bool flag1 = true;
			double num5 = 1;
			foreach (Triangle value in mesh.triangles.Values)
			{
				double num6 = 0;
				num5 = 1;
				pointArray[0] = value.vertices[0];
				pointArray[1] = value.vertices[1];
				pointArray[2] = value.vertices[2];
				double num7 = 0;
				for (int k = 0; k < 3; k++)
				{
					num = Statistic.plus1Mod3[k];
					num1 = Statistic.minus1Mod3[k];
					x[k] = pointArray[num].X - pointArray[num1].X;
					y[k] = pointArray[num].Y - pointArray[num1].Y;
					numArray1[k] = x[k] * x[k] + y[k] * y[k];
					if (numArray1[k] > num7)
					{
						num7 = numArray1[k];
					}
					if (numArray1[k] > this.maxEdge)
					{
						this.maxEdge = numArray1[k];
					}
					if (numArray1[k] < this.minEdge)
					{
						this.minEdge = numArray1[k];
					}
				}
				double num8 = Math.Abs((pointArray[2].X - pointArray[0].X) * (pointArray[1].Y - pointArray[0].Y) - (pointArray[1].X - pointArray[0].X) * (pointArray[2].Y - pointArray[0].Y));
				if (num8 < this.minArea)
				{
					this.minArea = num8;
				}
				if (num8 > this.maxArea)
				{
					this.maxArea = num8;
				}
				double num9 = num8 * num8 / num7;
				if (num9 < this.minAspect)
				{
					this.minAspect = num9;
				}
				double num10 = num7 / num9;
				if (num10 > this.maxAspect)
				{
					this.maxAspect = num10;
				}
				for (int l = 0; l < 3; l++)
				{
					num = Statistic.plus1Mod3[l];
					num1 = Statistic.minus1Mod3[l];
					double num11 = x[num] * x[num1] + y[num] * y[num1];
					double num12 = num11 * num11 / (numArray1[num] * numArray1[num1]);
					num2 = sampleDegrees / 2 - 1;
					for (int m = num2 - 1; m >= 0; m--)
					{
						if (num12 > numArray[m])
						{
							num2 = m;
						}
					}
					if (num11 > 0)
					{
						this.angleTable[sampleDegrees - num2 - 1] = this.angleTable[sampleDegrees - num2 - 1] + 1;
						if (flag || num12 > this.maxAngle)
						{
							this.maxAngle = num12;
							flag = false;
						}
						if (flag1 || num12 > num5)
						{
							num5 = num12;
							flag1 = false;
						}
					}
					else
					{
						this.angleTable[num2] = this.angleTable[num2] + 1;
						if (num12 > this.minAngle)
						{
							this.minAngle = num12;
						}
						if (flag && num12 < this.maxAngle)
						{
							this.maxAngle = num12;
						}
						if (num12 > num6)
						{
							num6 = num12;
						}
						if (flag1 && num12 < num5)
						{
							num5 = num12;
						}
					}
				}
				num2 = sampleDegrees / 2 - 1;
				for (int n = num2 - 1; n >= 0; n--)
				{
					if (num6 > numArray[n])
					{
						num2 = n;
					}
				}
				this.minAngles[num2] = this.minAngles[num2] + 1;
				num2 = sampleDegrees / 2 - 1;
				for (int o = num2 - 1; o >= 0; o--)
				{
					if (num5 > numArray[o])
					{
						num2 = o;
					}
				}
				if (!flag1)
				{
					this.maxAngles[sampleDegrees - num2 - 1] = this.maxAngles[sampleDegrees - num2 - 1] + 1;
				}
				else
				{
					this.maxAngles[num2] = this.maxAngles[num2] + 1;
				}
				flag1 = true;
			}
			this.minEdge = Math.Sqrt(this.minEdge);
			this.maxEdge = Math.Sqrt(this.maxEdge);
			this.minAspect = Math.Sqrt(this.minAspect);
			this.maxAspect = Math.Sqrt(this.maxAspect);
			this.minArea = this.minArea * 0.5;
			this.maxArea = this.maxArea * 0.5;
			if (this.minAngle < 1)
			{
				this.minAngle = num4 * Math.Acos(Math.Sqrt(this.minAngle));
			}
			else
			{
				this.minAngle = 0;
			}
			if (this.maxAngle >= 1)
			{
				this.maxAngle = 180;
				return;
			}
			if (flag)
			{
				this.maxAngle = num4 * Math.Acos(Math.Sqrt(this.maxAngle));
				return;
			}
			this.maxAngle = 180 - num4 * Math.Acos(Math.Sqrt(this.maxAngle));
		}
	}
}