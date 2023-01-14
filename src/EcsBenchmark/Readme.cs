using System;
using System.Collections.Generic;
using System.IO;
using BenchmarkDotNet;
using BenchmarkDotNet.Reports;

namespace EcsBenchmark
{
	public class Readme
	{
		internal Dictionary<string, string> urls;

		internal Summary[] summaries;

		public Readme(Summary[] summaries)
		{
			this.urls = new Dictionary<string, string>();
			this.summaries = summaries;
		}

		public Readme WithUrl(string category, string url)
		{
			this.urls.Add(category.ToLower(), url);
			return this;
		}

		public void WriteTo(string filePath)
		{
			using var fs = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite);
			using var sw = new StreamWriter(fs);

			WriteHeader(sw);

			for(int i = 0; i < this.summaries.Length; ++i)
				WriteSummary(sw, this.summaries[i]);
		}
		
		internal void WriteHeader(StreamWriter writer)
		{
			writer.WriteLine("# The super inoffical .NET ECS benchmark");
			writer.Write("This projects compares different ECS frameworks in various benchmarks. ");
			writer.Write("The goal is to get a quick overview of the performance of the ECS frameworks in selected situations.");
			writer.WriteLine();
		}

		internal void WriteSummary(StreamWriter writer, Summary summary)
		{
			writer.WriteLine("\n### " + GetBenchmarkName(summary.Title));
			
			WriteInfo(writer, summary);
			WriteUrls(writer, summary);

			writer.WriteLine();

			WriteTable(writer, summary.Table);
		}

		internal void WriteInfo(StreamWriter writer, Summary summary)
		{
			writer.WriteLine("<details>");
			writer.WriteLine("\t<summary>Environment and runtimes</summary>");
			writer.WriteLine();

			foreach(string info in summary.HostEnvironmentInfo.ToFormattedString())
			{
				writer.WriteLine(info);
				writer.WriteLine();
			}

			writer.WriteLine(summary.AllRuntimes);
			writer.WriteLine("</details>");
		}

		internal void WriteUrls(StreamWriter writer, Summary summary)
		{
			writer.WriteLine("<details>");
			writer.WriteLine("\t<summary>Tested frameworks</summary>");
			writer.WriteLine();

			var names = new HashSet<string>();

			foreach(var line in summary.Table.FullContent)
				names.Add(line[0]);
			
			foreach(var name in names)
			{
				if(this.urls.TryGetValue(name.ToLower(), out var url))
					writer.WriteLine($"* [{name}]({url})");
				else
					writer.WriteLine("* " + name);
			}

			writer.WriteLine("</details>");
		}

		internal void WriteTable(StreamWriter writer, SummaryTable table)
		{
			var header = table.FullHeader;

			writer.Write("| ");

			for(int i = 0; i < header.Length; ++i)
			{
				if(!table.Columns[i].NeedToShow)
					continue;

				PadLeft(writer, header[i], table.Columns[i].Width);
				writer.Write(header[i]);
				writer.Write(" |");
			}

			writer.WriteLine();
			writer.Write("| ");

			for(int i = 0; i < table.ColumnCount; ++i)
			{
				if(!table.Columns[i].NeedToShow)
					continue;

				Line(writer, table.Columns[i].Width);

				writer.Write(" |");
			}

			writer.WriteLine();

			foreach(var line in table.FullContent)
			{
				writer.Write("| ");

				for(int i = 0; i < table.ColumnCount; ++i)
				{
					if(!table.Columns[i].NeedToShow)
						continue;
					
					PadLeft(writer, line[i], table.Columns[i].Width);
					writer.Write(line[i]);
					writer.Write(" |");
				}

				writer.WriteLine();
			}
		}

		internal static void PadLeft(StreamWriter writer, string line, int width)
		{
			var count = width + 2 - line.Length - 2;

			for(int i = 0; i < count; ++i)
				writer.Write(' ');
		}

		internal static void Line(StreamWriter writer, int width)
		{
			for(int i = 0; i < width; ++i)
				writer.Write('-');
		}

		internal static string GetBenchmarkName(string title)
		{
			var startIndex = title.IndexOf('.') + 1;
			var endIndex   = title.IndexOf('-');
			return title.Substring(startIndex, endIndex - startIndex);
		}
	}
}
