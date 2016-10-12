using System;
using System.Collections.Generic;

namespace TriangleNet.Log
{
	public interface ILog<T>
	where T : ILogItem
	{
		IList<T> Data
		{
			get;
		}

		TriangleNet.Log.LogLevel Level
		{
			get;
		}

		void Add(T item);

		void Clear();

		void Error(string message, string info);

		void Info(string message);

		void Warning(string message, string info);
	}
}