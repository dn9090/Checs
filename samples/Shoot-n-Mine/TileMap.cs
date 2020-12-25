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
	public class TileMap
	{
		public TileChunk[] chunks;

		public int chunksPerRow;

		public TileMap(int capacity, int chunksPerRow)
		{
			this.chunks = new TileChunk[capacity];
			this.chunksPerRow = chunksPerRow;
		}

		public FloatRect Instantiate(World world, Position position) => Instantiate(world, position, out ReadOnlySpan<Entity> chunks);

		public FloatRect Instantiate(World world, Position position, out ReadOnlySpan<Entity> chunks)
		{
			var tileChunks = world.CreateEntity(
				world.CreateArchetype(typeof(TileChunk), typeof(Position)),
				this.chunks.Length);

			chunks = tileChunks;

			var currentPos = position.value;

			for(int i = 0; i < this.chunks.Length;)
			{
				for(int col = 0; col < this.chunksPerRow; ++col)
				{
					ref var chunk = ref world.RefComponentData<TileChunk>(tileChunks[i]);
					chunk = this.chunks[i];

					world.SetComponentData<Position>(tileChunks[i], new Position(currentPos));

					currentPos.X += TileChunk.PixelSize;
					++i;
				}

				currentPos.X = position.value.X;
				currentPos.Y += TileChunk.PixelSize;
			}

			return new FloatRect(
				position.value.X,
				position.value.Y,
				TileChunk.PixelSize * this.chunksPerRow,
				currentPos.Y - position.value.Y);
		}

		public static TileMap Load(string fileName)
		{
			List<Tile[]> tiles = new List<Tile[]>();

			string[] lines = File.ReadAllLines(fileName);

			int maxSizeX = 0;

			foreach(var line in lines)
			{
				var tileTypes = line.Split(' ');
				var generatedTiles = new Tile[tileTypes.Length];

				if(tileTypes.Length > maxSizeX)
					maxSizeX = tileTypes.Length;

				for(int i = 0; i < tileTypes.Length; ++i)
					generatedTiles[i] = ParseTile(tileTypes[i]);

				tiles.Add(generatedTiles);
			}

			int maxSizeY = tiles.Count;

			int tileChunksX = (maxSizeX + TileChunk.Size - 1) / TileChunk.Size;
			int tileChunksY = (maxSizeY + TileChunk.Size - 1) / TileChunk.Size;

			TileMap map = new TileMap(tileChunksX * tileChunksY, tileChunksX);

			for(int row = 0; row < tileChunksY; ++row)
			{
				int startYIndex = row * TileChunk.Size;
				int endYIndex = Math.Min(startYIndex + TileChunk.Size, startYIndex + (tiles.Count - startYIndex));

				for(int col = 0; col < tileChunksX; ++col)
				{
					int offset = 0;

					for(int y = startYIndex; y < endYIndex; ++y)
					{
						int startXIndex = col * TileChunk.Size;
						int endXIndex = Math.Min(startXIndex + TileChunk.Size, startXIndex + (tiles[y].Length - startXIndex));

						for(int x = startXIndex; x < endXIndex; ++x)
							map.chunks[row * tileChunksX + col][offset++] = tiles[y][x];
					}
				}
			}

			return map;
		}

		private static Tile ParseTile(string tileType)
		{
			if(char.IsDigit(tileType, 0))
				return new Tile(Enum.Parse<TileType>(tileType));

			switch(tileType[0])
			{
				case '?':
					return new Tile(TileType.RandomBlock);
				case '-':
					return new Tile(TileType.RandomNatural);
				case '+':
					return new Tile(TileType.RandomResource);
				case 'x':
					return new Tile(TileType.Spawn);
			}

			return new Tile(TileType.None);
		}
	}
}
