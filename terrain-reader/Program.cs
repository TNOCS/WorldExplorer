using System;
using System.IO;
using System.Text;

namespace terrain_reader
{
  class Program
  {
    static void Main(string[] args)
    {
      Console.WriteLine("Little endian? " + BitConverter.IsLittleEndian);
      var sb = new StringBuilder();

      using (var filestream = File.Open(@"1655628", FileMode.Open))
    //   using (var filestream = File.Open(@"25993", FileMode.Open))
      {
        using (var binaryStream = new BinaryReader(filestream))
        {
          var i = 0;
          var j = 0;
          var pos = 0;
          var length = Math.Min(65 * 65 * 2, (int)binaryStream.BaseStream.Length); // Limit to the 65x65 terrain info, skip rest
          while (pos < length)
          {
            var height = binaryStream.ReadInt16();
            sb.AppendLine(string.Format("Index ({0:D2}, {1}): {2}", i, j++, height / 5 - 1000)); // convert to height
            if (j >= 65)
            {
              j = 0;
              i++;
            }
            pos += 2;
          }
        }
      }
      File.WriteAllText("converted.txt", sb.ToString());
    }
  }
}
