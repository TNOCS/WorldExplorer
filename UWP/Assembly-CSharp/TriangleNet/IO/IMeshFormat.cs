using System;
using TriangleNet;

namespace TriangleNet.IO
{
	public interface IMeshFormat
	{
		Mesh Import(string filename);

		void Write(Mesh mesh, string filename);
	}
}