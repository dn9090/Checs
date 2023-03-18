using System;
using System.Collections.Generic;
using System.IO;
using BenchmarkDotNet;
using BenchmarkDotNet.Reports;

namespace EcsBenchmark
{
	public class Readme
	{
		public string[] skipColumns = { "Error", "StdDev" };

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

			sw.WriteLine("## Results");

			for(int i = 0; i < this.summaries.Length; ++i)
				WriteSummary(sw, this.summaries[i]);

			WriteContributing(sw);
		}
		
		internal void WriteHeader(StreamWriter writer)
		{
			writer.WriteLine("# The super inofficial .NET ECS benchmark");
			writer.Write("This projects compares different ECS frameworks and data structures in various benchmarks. ");
			writer.Write("The goal is to get a quick overview of the performance of ECS designs in selected situations.");
			writer.WriteLine();
			writer.WriteLine();

			writer.Write("> Note that the benchmarks try to achieve the best possible execution time. ");
			writer.Write("However, APIs may change or other APIs may be better suited for the benchmark. ");
			writer.Write("In such cases, please contact the author.");
			writer.WriteLine();
			writer.WriteLine();
		}

		internal void WriteContributing(StreamWriter writer)
		{
			writer.WriteLine("## Contributing");
			writer.WriteLine("### Adding a framework");
			writer.WriteLine("To add a framework to existing tests, copy the test and replace the setup, execution and cleanup methods. ");
			writer.WriteLine("If the framework is new, also add an entry to the `Categories` class. ");
			writer.WriteLine("The name of the execution method should match the name of the framework/category.");
			
			writer.WriteLine("### Create a new benchmark");
			writer.WriteLine("...");
		}

		internal void WriteSummary(StreamWriter writer, Summary summary)
		{
			writer.WriteLine("\n### " + GetBenchmarkName(summary.Title));
			
			WriteInfo(writer, summary);
			WriteUrls(writer, summary);

			writer.WriteLine();

			WriteTable(writer, summary.Table);

			writer.WriteLine();
		}

		internal void WriteInfo(StreamWriter writer, Summary summary)
		{
			writer.WriteLine("<details>");
			writer.WriteLine("\t<summary>Environment and runtimes</summary>");
			writer.WriteLine();
			writer.WriteLine("```");

			foreach(string info in summary.HostEnvironmentInfo.ToFormattedString())
				writer.WriteLine(info);

			writer.WriteLine(summary.AllRuntimes);
			writer.WriteLine("```");
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
			var columns = new HashSet<int>();
			var header = table.FullHeader;

			writer.Write("| ");

			for(int i = 0; i < header.Length; ++i)
			{
				if(!table.Columns[i].NeedToShow)
					continue;

				if(Array.IndexOf(skipColumns, header[i].Trim()) != -1)
					continue;

				columns.Add(i);
				
				PadLeft(writer, header[i], table.Columns[i].Width);
				writer.Write(header[i]);
				writer.Write(" |");
			}

			writer.WriteLine();
			writer.Write("| ");

			for(int i = 0; i < table.ColumnCount; ++i)
			{
				if(!columns.Contains(i))
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
					if(!columns.Contains(i))
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
