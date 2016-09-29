using System;
using System.Collections.Generic;
using System.Linq;
using TriangleNet.Data;

namespace TriangleNet.Geometry
{
	public class InputGeometry
	{
		internal List<Vertex> points;

		internal List<Edge> segments;

		internal List<Point> holes;

		internal List<RegionPointer> regions;

		private BoundingBox bounds;

		private int pointAttributes = -1;

		public BoundingBox Bounds
		{
			get
			{
				return this.bounds;
			}
		}

		public int Count
		{
			get
			{
				return this.points.Count;
			}
		}

		public bool HasSegments
		{
			get
			{
				return this.segments.Count > 0;
			}
		}

		public ICollection<Point> Holes
		{
			get
			{
				return this.holes;
			}
		}

		public IEnumerable<Point> Points
		{
			get
			{
				return this.points.Cast<Point>();
			}
		}

		public ICollection<RegionPointer> Regions
		{
			get
			{
				return this.regions;
			}
		}

		public ICollection<Edge> Segments
		{
			get
			{
				return this.segments;
			}
		}

		public InputGeometry() : this(3)
		{
		}

		public InputGeometry(int capacity)
		{
			this.points = new List<Vertex>(capacity);
			this.segments = new List<Edge>();
			this.holes = new List<Point>();
			this.regions = new List<RegionPointer>();
			this.bounds = new BoundingBox();
			this.pointAttributes = -1;
		}

		public void AddHole(double x, double y)
		{
			this.holes.Add(new Point(x, y));
		}

		public void AddPoint(double x, double y)
		{
			this.AddPoint(x, y, 0);
		}

		public void AddPoint(double x, double y, int boundary)
		{
			this.points.Add(new Vertex(x, y, boundary));
			this.bounds.Update(x, y);
		}

		public void AddPoint(double x, double y, int boundary, double attribute)
		{
			this.AddPoint(x, y, 0, new double[] { attribute });
		}

		public void AddPoint(double x, double y, int boundary, double[] attribs)
		{
			if (this.pointAttributes >= 0)
			{
				if (attribs == null && this.pointAttributes > 0)
				{
					throw new ArgumentException("Inconsitent use of point attributes.");
				}
				if (attribs != null && this.pointAttributes != (int)attribs.Length)
				{
					throw new ArgumentException("Inconsitent use of point attributes.");
				}
			}
			else
			{
				this.pointAttributes = (attribs == null ? 0 : (int)attribs.Length);
			}
			this.points.Add(new Vertex(x, y, boundary)
			{
				attributes = attribs
			});
			this.bounds.Update(x, y);
		}

		public void AddRegion(double x, double y, int id)
		{
			this.regions.Add(new RegionPointer(x, y, id));
		}

		public void AddSegment(int p0, int p1)
		{
			this.AddSegment(p0, p1, 0);
		}

		public void AddSegment(int p0, int p1, int boundary)
		{
			if (p0 == p1 || p0 < 0 || p1 < 0)
			{
				throw new NotSupportedException("Invalid endpoints.");
			}
			this.segments.Add(new Edge(p0, p1, boundary));
		}

		public void Clear()
		{
			this.points.Clear();
			this.segments.Clear();
			this.holes.Clear();
			this.regions.Clear();
			this.pointAttributes = -1;
		}
	}
}