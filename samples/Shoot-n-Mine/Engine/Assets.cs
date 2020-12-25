using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Shoot_n_Mine.Engine
{
	public static class Assets
	{
		private static string s_AssetPath;

		public static string NameToPath(string name)
		{
			if(string.IsNullOrEmpty(s_AssetPath))
				SearchAssetPath();

			return Path.Combine(s_AssetPath, name);
		}

		private static void SearchAssetPath()
		{
			var basePath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
			
			while(!string.IsNullOrEmpty(basePath))
			{
				foreach(var dir in Directory.GetDirectories(basePath))
				{
					if(dir.EndsWith("Assets"))
					{
						s_AssetPath = dir;
						return;
					}
				}

				basePath = new DirectoryInfo(basePath).Parent.FullName;
			}
		}
	}
}
