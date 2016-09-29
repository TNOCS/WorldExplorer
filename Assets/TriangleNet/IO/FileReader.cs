using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TriangleNet;
using TriangleNet.Geometry;
using TriangleNet.Log;

namespace TriangleNet.IO
{
	public static class FileReader
	{
		private static NumberFormatInfo nfi;

		private static int startIndex;

		static FileReader()
		{
			FileReader.nfi = CultureInfo.InvariantCulture.NumberFormat;
			FileReader.startIndex = 0;
		}

		public static void Read(string filename, out InputGeometry geometry)
		{
			geometry = null;
			string str = Path.ChangeExtension(filename, ".poly");
			if (File.Exists(str))
			{
				geometry = FileReader.ReadPolyFile(str);
				return;
			}
			str = Path.ChangeExtension(filename, ".node");
			geometry = FileReader.ReadNodeFile(str);
		}

		public static void Read(string filename, out InputGeometry geometry, out List<ITriangle> triangles)
		{
			triangles = null;
			FileReader.Read(filename, out geometry);
			string str = Path.ChangeExtension(filename, ".ele");
			if (File.Exists(str) && geometry != null)
			{
				triangles = FileReader.ReadEleFile(str);
			}
		}

		public static InputGeometry Read(string filename)
		{
			InputGeometry inputGeometry = null;
			FileReader.Read(filename, out inputGeometry);
			return inputGeometry;
		}

		private static double[] ReadAreaFile(string areafilename, int intriangles)
		{
			string[] strArrays;
			double[] numArray;
			double[] numArray1 = null;
			using (StreamReader streamReader = new StreamReader(new FileStream(areafilename, FileMode.Open)))
			{
				if (!FileReader.TryReadLine(streamReader, out strArrays))
				{
					throw new Exception("Can't read input file (area).");
				}
				if (int.Parse(strArrays[0]) == intriangles)
				{
					numArray1 = new double[intriangles];
					for (int i = 0; i < intriangles; i++)
					{
						if (!FileReader.TryReadLine(streamReader, out strArrays))
						{
							throw new Exception("Can't read input file (area).");
						}
						if ((int)strArrays.Length != 2)
						{
							throw new Exception("Triangle has no nodes.");
						}
						numArray1[i] = double.Parse(strArrays[1], FileReader.nfi);
					}
					return numArray1;
				}
				else
				{
					SimpleLog.Instance.Warning("Number of area constraints doesn't match number of triangles.", "ReadAreaFile()");
					numArray = null;
				}
			}
			return numArray;
		}

		public static List<Edge> ReadEdgeFile(string edgeFile, int invertices)
		{
			string[] strArrays;
			List<Edge> edges = null;
			FileReader.startIndex = 0;
			using (StreamReader streamReader = new StreamReader(new FileStream(edgeFile, FileMode.Open)))
			{
				if (!FileReader.TryReadLine(streamReader, out strArrays))
				{
					throw new Exception("Can't read input file (segments).");
				}
				int num = int.Parse(strArrays[0]);
				int num1 = 0;
				if ((int)strArrays.Length > 1)
				{
					num1 = int.Parse(strArrays[1]);
				}
				if (num > 0)
				{
					edges = new List<Edge>(num);
				}
				for (int i = 0; i < num; i++)
				{
					if (!FileReader.TryReadLine(streamReader, out strArrays))
					{
						throw new Exception("Can't read input file (segments).");
					}
					if ((int)strArrays.Length < 3)
					{
						throw new Exception("Segment has no endpoints.");
					}
					int num2 = int.Parse(strArrays[1]) - FileReader.startIndex;
					int num3 = int.Parse(strArrays[2]) - FileReader.startIndex;
					int num4 = 0;
					if (num1 > 0 && (int)strArrays.Length > 3)
					{
						num4 = int.Parse(strArrays[3]);
					}
					if (num2 < 0 || num2 >= invertices)
					{
						if (Behavior.Verbose)
						{
							SimpleLog.Instance.Warning("Invalid first endpoint of segment.", "MeshReader.ReadPolyfile()");
						}
					}
					else if (num3 >= 0 && num3 < invertices)
					{
						edges.Add(new Edge(num2, num3, num4));
					}
					else if (Behavior.Verbose)
					{
						SimpleLog.Instance.Warning("Invalid second endpoint of segment.", "MeshReader.ReadPolyfile()");
					}
				}
			}
			return edges;
		}

		public static List<ITriangle> ReadEleFile(string elefilename)
		{
			return FileReader.ReadEleFile(elefilename, false);
		}

		private static List<ITriangle> ReadEleFile(string elefilename, bool readArea)
		{
			List<ITriangle> triangles;
			string[] strArrays;
			int num = 0;
			int num1 = 0;
			using (StreamReader streamReader = new StreamReader(new FileStream(elefilename, FileMode.Open)))
			{
				bool flag = false;
				if (!FileReader.TryReadLine(streamReader, out strArrays))
				{
					throw new Exception("Can't read input file (elements).");
				}
				num = int.Parse(strArrays[0]);
				num1 = 0;
				if ((int)strArrays.Length > 2)
				{
					num1 = int.Parse(strArrays[2]);
					flag = true;
				}
				if (num1 > 1)
				{
					SimpleLog.Instance.Warning("Triangle attributes not supported.", "FileReader.Read");
				}
				triangles = new List<ITriangle>(num);
				for (int i = 0; i < num; i++)
				{
					if (!FileReader.TryReadLine(streamReader, out strArrays))
					{
						throw new Exception("Can't read input file (elements).");
					}
					if ((int)strArrays.Length < 4)
					{
						throw new Exception("Triangle has no nodes.");
					}
					InputTriangle inputTriangle = new InputTriangle(int.Parse(strArrays[1]) - FileReader.startIndex, int.Parse(strArrays[2]) - FileReader.startIndex, int.Parse(strArrays[3]) - FileReader.startIndex);
					if (num1 > 0 & flag)
					{
						int num2 = 0;
						flag = int.TryParse(strArrays[4], out num2);
						inputTriangle.region = num2;
					}
					triangles.Add(inputTriangle);
				}
			}
			if (readArea)
			{
				string str = Path.ChangeExtension(elefilename, ".area");
				if (File.Exists(str))
				{
					FileReader.ReadAreaFile(str, num);
				}
			}
			return triangles;
		}

		public static InputGeometry ReadNodeFile(string nodefilename)
		{
			return FileReader.ReadNodeFile(nodefilename, false);
		}

		public static InputGeometry ReadNodeFile(string nodefilename, bool readElements)
		{
			InputGeometry inputGeometry;
			string[] strArrays;
			FileReader.startIndex = 0;
			int num = 0;
			int num1 = 0;
			int num2 = 0;
			using (StreamReader streamReader = new StreamReader(new FileStream(nodefilename, FileMode.Open)))
			{
				if (!FileReader.TryReadLine(streamReader, out strArrays))
				{
					throw new Exception("Can't read input file.");
				}
				num = int.Parse(strArrays[0]);
				if (num < 3)
				{
					throw new Exception("Input must have at least three input vertices.");
				}
				if ((int)strArrays.Length > 1 && int.Parse(strArrays[1]) != 2)
				{
					throw new Exception("Triangle only works with two-dimensional meshes.");
				}
				if ((int)strArrays.Length > 2)
				{
					num1 = int.Parse(strArrays[2]);
				}
				if ((int)strArrays.Length > 3)
				{
					num2 = int.Parse(strArrays[3]);
				}
				inputGeometry = new InputGeometry(num);
				if (num > 0)
				{
					for (int i = 0; i < num; i++)
					{
						if (!FileReader.TryReadLine(streamReader, out strArrays))
						{
							throw new Exception("Can't read input file (vertices).");
						}
						if ((int)strArrays.Length < 3)
						{
							throw new Exception("Invalid vertex.");
						}
						if (i == 0)
						{
							FileReader.startIndex = int.Parse(strArrays[0], FileReader.nfi);
						}
						FileReader.ReadVertex(inputGeometry, i, strArrays, num1, num2);
					}
				}
			}
			if (readElements)
			{
				string str = Path.ChangeExtension(nodefilename, ".ele");
				if (File.Exists(str))
				{
					FileReader.ReadEleFile(str, true);
				}
			}
			return inputGeometry;
		}

		public static InputGeometry ReadPolyFile(string polyfilename)
		{
			return FileReader.ReadPolyFile(polyfilename, false, false);
		}

		public static InputGeometry ReadPolyFile(string polyfilename, bool readElements)
		{
			return FileReader.ReadPolyFile(polyfilename, readElements, false);
		}

		public static InputGeometry ReadPolyFile(string polyfilename, bool readElements, bool readArea)
		{
			InputGeometry inputGeometry;
			string[] strArrays;
			FileReader.startIndex = 0;
			int count = 0;
			int num = 0;
			int num1 = 0;
			using (StreamReader streamReader = new StreamReader(new FileStream(polyfilename, FileMode.Open)))
			{
				if (!FileReader.TryReadLine(streamReader, out strArrays))
				{
					throw new Exception("Can't read input file.");
				}
				count = int.Parse(strArrays[0]);
				if ((int)strArrays.Length > 1 && int.Parse(strArrays[1]) != 2)
				{
					throw new Exception("Triangle only works with two-dimensional meshes.");
				}
				if ((int)strArrays.Length > 2)
				{
					num = int.Parse(strArrays[2]);
				}
				if ((int)strArrays.Length > 3)
				{
					num1 = int.Parse(strArrays[3]);
				}
				if (count <= 0)
				{
					inputGeometry = FileReader.ReadNodeFile(Path.ChangeExtension(polyfilename, ".node"));
					count = inputGeometry.Count;
				}
				else
				{
					inputGeometry = new InputGeometry(count);
					for (int i = 0; i < count; i++)
					{
						if (!FileReader.TryReadLine(streamReader, out strArrays))
						{
							throw new Exception("Can't read input file (vertices).");
						}
						if ((int)strArrays.Length < 3)
						{
							throw new Exception("Invalid vertex.");
						}
						if (i == 0)
						{
							FileReader.startIndex = int.Parse(strArrays[0], FileReader.nfi);
						}
						FileReader.ReadVertex(inputGeometry, i, strArrays, num, num1);
					}
				}
				if (inputGeometry.Points == null)
				{
					throw new Exception("No nodes available.");
				}
				if (!FileReader.TryReadLine(streamReader, out strArrays))
				{
					throw new Exception("Can't read input file (segments).");
				}
				int num2 = int.Parse(strArrays[0]);
				int num3 = 0;
				if ((int)strArrays.Length > 1)
				{
					num3 = int.Parse(strArrays[1]);
				}
				for (int j = 0; j < num2; j++)
				{
					if (!FileReader.TryReadLine(streamReader, out strArrays))
					{
						throw new Exception("Can't read input file (segments).");
					}
					if ((int)strArrays.Length < 3)
					{
						throw new Exception("Segment has no endpoints.");
					}
					int num4 = int.Parse(strArrays[1]) - FileReader.startIndex;
					int num5 = int.Parse(strArrays[2]) - FileReader.startIndex;
					int num6 = 0;
					if (num3 > 0 && (int)strArrays.Length > 3)
					{
						num6 = int.Parse(strArrays[3]);
					}
					if (num4 < 0 || num4 >= count)
					{
						if (Behavior.Verbose)
						{
							SimpleLog.Instance.Warning("Invalid first endpoint of segment.", "MeshReader.ReadPolyfile()");
						}
					}
					else if (num5 >= 0 && num5 < count)
					{
						inputGeometry.AddSegment(num4, num5, num6);
					}
					else if (Behavior.Verbose)
					{
						SimpleLog.Instance.Warning("Invalid second endpoint of segment.", "MeshReader.ReadPolyfile()");
					}
				}
				if (!FileReader.TryReadLine(streamReader, out strArrays))
				{
					throw new Exception("Can't read input file (holes).");
				}
				int num7 = int.Parse(strArrays[0]);
				if (num7 > 0)
				{
					for (int k = 0; k < num7; k++)
					{
						if (!FileReader.TryReadLine(streamReader, out strArrays))
						{
							throw new Exception("Can't read input file (holes).");
						}
						if ((int)strArrays.Length < 3)
						{
							throw new Exception("Invalid hole.");
						}
						inputGeometry.AddHole(double.Parse(strArrays[1], FileReader.nfi), double.Parse(strArrays[2], FileReader.nfi));
					}
				}
				if (FileReader.TryReadLine(streamReader, out strArrays))
				{
					int num8 = int.Parse(strArrays[0]);
					if (num8 > 0)
					{
						for (int l = 0; l < num8; l++)
						{
							if (!FileReader.TryReadLine(streamReader, out strArrays))
							{
								throw new Exception("Can't read input file (region).");
							}
							if ((int)strArrays.Length < 4)
							{
								throw new Exception("Invalid region attributes.");
							}
							inputGeometry.AddRegion(double.Parse(strArrays[1], FileReader.nfi), double.Parse(strArrays[2], FileReader.nfi), int.Parse(strArrays[3]));
						}
					}
				}
			}
			if (readElements)
			{
				string str = Path.ChangeExtension(polyfilename, ".ele");
				if (File.Exists(str))
				{
					FileReader.ReadEleFile(str, readArea);
				}
			}
			return inputGeometry;
		}

		private static void ReadVertex(InputGeometry data, int index, string[] line, int attributes, int marks)
		{
			double[] numArray;
			double num = double.Parse(line[1], FileReader.nfi);
			double num1 = double.Parse(line[2], FileReader.nfi);
			int num2 = 0;
			if (attributes == 0)
			{
				numArray = null;
			}
			else
			{
				numArray = new double[attributes];
			}
			double[] numArray1 = numArray;
			for (int i = 0; i < attributes; i++)
			{
				if ((int)line.Length > 3 + i)
				{
					numArray1[i] = double.Parse(line[3 + i]);
				}
			}
			if (marks > 0 && (int)line.Length > 3 + attributes)
			{
				num2 = int.Parse(line[3 + attributes]);
			}
			data.AddPoint(num, num1, num2, numArray1);
		}

		private static bool TryReadLine(StreamReader reader, out string[] token)
		{
			string i;
			token = null;
			if (reader.EndOfStream)
			{
				return false;
			}
			for (i = reader.ReadLine().Trim(); string.IsNullOrEmpty(i) || i.StartsWith("#"); i = reader.ReadLine().Trim())
			{
				if (reader.EndOfStream)
				{
					return false;
				}
			}
			token = i.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			return true;
		}
	}
}