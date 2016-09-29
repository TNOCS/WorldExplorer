using System;
using System.Collections.Generic;
using TriangleNet;
using TriangleNet.Data;
using TriangleNet.Geometry;
using TriangleNet.Tools;

namespace TriangleNet.Smoothing
{
	public class SimpleSmoother : ISmoother
	{
		private Mesh mesh;

		public SimpleSmoother(Mesh mesh)
		{
			this.mesh = mesh;
		}

		private InputGeometry Rebuild()
		{
			InputGeometry inputGeometry = new InputGeometry(this.mesh.vertices.Count);
			foreach (Vertex value in this.mesh.vertices.Values)
			{
				inputGeometry.AddPoint(value.x, value.y, value.mark);
			}
			foreach (Segment segment in this.mesh.subsegs.Values)
			{
				inputGeometry.AddSegment(segment.P0, segment.P1, segment.Boundary);
			}
			foreach (Point hole in this.mesh.holes)
			{
				inputGeometry.AddHole(hole.x, hole.y);
			}
			foreach (RegionPointer region in this.mesh.regions)
			{
				inputGeometry.AddRegion(region.point.x, region.point.y, region.id);
			}
			return inputGeometry;
		}

		public void Smooth()
		{
			this.mesh.behavior.Quality = false;
			for (int i = 0; i < 5; i++)
			{
				this.Step();
				this.mesh.Triangulate(this.Rebuild());
			}
		}

		private void Step()
		{
			foreach (VoronoiRegion region in (new BoundedVoronoi(this.mesh, false)).Regions)
			{
				int num = 0;
				double num1 = 0;
				double num2 = num1;
				double num3 = num1;
				foreach (Point vertex in region.Vertices)
				{
					num++;
					num3 = num3 + vertex.x;
					num2 = num2 + vertex.y;
				}
				region.Generator.x = num3 / (double)num;
				region.Generator.y = num2 / (double)num;
			}
		}
	}
}