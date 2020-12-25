using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using SFML.System;
using SFML.Graphics;
using Checs;
using Shoot_n_Mine.Engine;

namespace Shoot_n_Mine
{
	public class RandomTileMapCollection
	{
		private struct TileMapEntry
		{
			public TileMap tileMap;

			public int weight;

			public int cumulativeSum;

			public int choosen;
		}

		private List<TileMapEntry> m_Entries;

		private int m_Sum;

		private Random m_Random;

		private int m_DrawCount;

		private float m_maxDuplication;

		private int m_LastSelection;

		public RandomTileMapCollection(Random random, int drawCount, float maxDuplication = 0.6f)
		{
			this.m_Entries = new List<TileMapEntry>();
			this.m_Random = random;
			this.m_DrawCount = drawCount;
			this.m_maxDuplication = maxDuplication;
			this.m_LastSelection = -1;
		}

		public void Add(TileMap map, int weight)
		{
			this.m_Sum += weight;

			this.m_Entries.Add(new TileMapEntry {
				tileMap = map,
				weight = weight,
				cumulativeSum = this.m_Sum
			});
		}

		public TileMap Choose()
		{
			TileMap tileMap = null;
			int draws = 0;

			while(!SelectNext(out tileMap))
			{
				++draws;

				if(draws > 5)
					return tileMap;
			}

			return tileMap;
		}

		private bool SelectNext(out TileMap tileMap)
		{
			double prop = this.m_Random.NextDouble() * this.m_Sum;
			tileMap = default;

			for(int i = 0; i < this.m_Entries.Count; ++i)
			{
				var entry = this.m_Entries[i];
				tileMap = entry.tileMap;

				if(entry.cumulativeSum >= prop)
				{
					if(entry.choosen / this.m_DrawCount > 0.5f || this.m_LastSelection == i)
						return false;

					++entry.choosen;
					tileMap = entry.tileMap;
					this.m_LastSelection = i;
					return true;
				}
			}

			return false;
		}
	}
}
