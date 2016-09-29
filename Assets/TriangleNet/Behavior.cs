using System;
using System.Runtime.CompilerServices;
using TriangleNet.Log;

namespace TriangleNet
{
	public class Behavior
	{
		private bool poly;

		private bool quality;

		private bool varArea;

		private bool usertest;

		private bool convex;

		private bool jettison;

		private bool boundaryMarkers = true;

		private bool noHoles;

		private bool conformDel;

		private TriangulationAlgorithm algorithm;

		private int noBisect;

		private int steiner = -1;

		private double minAngle;

		private double maxAngle;

		private double maxArea = -1;

		internal bool fixedArea;

		internal bool useSegments = true;

		internal bool useRegions;

		internal double goodAngle;

		internal double maxGoodAngle;

		internal double offconstant;

		public TriangulationAlgorithm Algorithm
		{
			get
			{
				return this.algorithm;
			}
			set
			{
				this.algorithm = value;
			}
		}

		public bool ConformingDelaunay
		{
			get
			{
				return this.conformDel;
			}
			set
			{
				this.conformDel = value;
			}
		}

		public bool Convex
		{
			get
			{
				return this.convex;
			}
			set
			{
				this.convex = value;
			}
		}

		public bool Jettison
		{
			get
			{
				return this.jettison;
			}
			set
			{
				this.jettison = value;
			}
		}

		public double MaxAngle
		{
			get
			{
				return this.maxAngle;
			}
			set
			{
				this.maxAngle = value;
				this.Update();
			}
		}

		public double MaxArea
		{
			get
			{
				return this.maxArea;
			}
			set
			{
				this.maxArea = value;
				this.fixedArea = value > 0;
			}
		}

		public double MinAngle
		{
			get
			{
				return this.minAngle;
			}
			set
			{
				this.minAngle = value;
				this.Update();
			}
		}

		public int NoBisect
		{
			get
			{
				return this.noBisect;
			}
			set
			{
				this.noBisect = value;
				if (this.noBisect < 0 || this.noBisect > 2)
				{
					this.noBisect = 0;
				}
			}
		}

		public static bool NoExact
		{
			get;
			set;
		}

		public bool NoHoles
		{
			get
			{
				return this.noHoles;
			}
			set
			{
				this.noHoles = value;
			}
		}

		public bool Poly
		{
			get
			{
				return this.poly;
			}
			set
			{
				this.poly = value;
			}
		}

		public bool Quality
		{
			get
			{
				return this.quality;
			}
			set
			{
				this.quality = value;
				if (this.quality)
				{
					this.Update();
				}
			}
		}

		public int SteinerPoints
		{
			get
			{
				return this.steiner;
			}
			set
			{
				this.steiner = value;
			}
		}

		public bool UseBoundaryMarkers
		{
			get
			{
				return this.boundaryMarkers;
			}
			set
			{
				this.boundaryMarkers = value;
			}
		}

		public bool Usertest
		{
			get
			{
				return this.usertest;
			}
			set
			{
				this.usertest = value;
			}
		}

		public bool VarArea
		{
			get
			{
				return this.varArea;
			}
			set
			{
				this.varArea = value;
			}
		}

		public static bool Verbose
		{
			get;
			set;
		}

		public Behavior(bool quality = false, double minAngle = 20)
		{
			if (quality)
			{
				this.quality = true;
				this.minAngle = minAngle;
				this.Update();
			}
		}

		private void Update()
		{
			this.quality = true;
			if (this.minAngle < 0 || this.minAngle > 60)
			{
				this.minAngle = 0;
				this.quality = false;
				SimpleLog.Instance.Warning("Invalid quality option (minimum angle).", "Mesh.Behavior");
			}
			if (this.maxAngle != 0 && this.maxAngle < 90 || this.maxAngle > 180)
			{
				this.maxAngle = 0;
				this.quality = false;
				SimpleLog.Instance.Warning("Invalid quality option (maximum angle).", "Mesh.Behavior");
			}
			this.useSegments = (this.Poly || this.Quality ? true : this.Convex);
			this.goodAngle = Math.Cos(this.MinAngle * 3.14159265358979 / 180);
			this.maxGoodAngle = Math.Cos(this.MaxAngle * 3.14159265358979 / 180);
			if (this.goodAngle != 1)
			{
				this.offconstant = 0.475 * Math.Sqrt((1 + this.goodAngle) / (1 - this.goodAngle));
			}
			else
			{
				this.offconstant = 0;
			}
			this.goodAngle = this.goodAngle * this.goodAngle;
		}
	}
}