using System;
using System.Collections.Generic;
using TriangleNet.Data;
using TriangleNet.Geometry;

namespace TriangleNet.Tools
{
	public class VoronoiRegion
	{
		private int id;

		private Point generator;

		private List<Point> vertices;

		private bool bounded;

		public bool Bounded
		{
			get
			{
				return this.bounded;
			}
			set
			{
				this.bounded = value;
			}
		}

		public Point Generator
		{
			get
			{
				return this.generator;
			}
		}

		public int ID
		{
			get
			{
				return this.id;
			}
		}

		public ICollection<Point> Vertices
		{
			get
			{
				return this.vertices;
			}
		}

		public VoronoiRegion(Vertex generator)
		{
			this.id = generator.id;
			this.generator = generator;
			this.vertices = new List<Point>();
			this.bounded = true;
		}

		public void Add(Point point)
		{
			this.vertices.Add(point);
		}

		public void Add(List<Point> points)
		{
			this.vertices.AddRange(points);
		}

		public override string ToString()
		{
			return string.Format("R-ID {0}", this.id);
		}
	}
}