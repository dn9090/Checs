using System;
using System.Collections.Generic;
using System.Numerics;
using SFML.System;
using SFML.Graphics;
using Checs;
using Shoot_n_Mine.Engine;
using Shoot_n_Mine.Net;

namespace Shoot_n_Mine
{
	public struct LevelGeneratorData
	{
		public int seed;

		public int maxSize;

		public LevelGeneratorData(int seed, int maxSize)
		{
			this.seed = seed;
			this.maxSize = maxSize;
		}
	}

	public class LevelGenerator
	{
		public static unsafe LevelGenerator ReadFromNetwork()
		{
			byte[] data = new byte[sizeof(LevelGeneratorData)];

			Networking.client.stream.Read(data);

			fixed(byte* ptr = data)
			{
				var msg = ((LevelGeneratorData*)ptr)[0];
				return new LevelGenerator(msg.seed, msg.maxSize);
			}
		}

		public int seed => this.m_Seed;

		private int m_Seed;

		private int m_MaxSize;

		private Random m_Random;

		public LevelGenerator(int maxSize = 20)
		{
			this.m_Seed = new Random().Next();
			this.m_MaxSize = maxSize;
			this.m_Random = new Random(this.m_Seed);
		}

		public LevelGenerator(int seed, int maxSize = 20)
		{
			this.m_Seed = seed;
			this.m_MaxSize = maxSize;
			this.m_Random = new Random(this.m_Seed);
		}

		public unsafe void SendParametersOverNetwork()
		{
			var msg = new LevelGeneratorData(this.m_Seed, this.m_MaxSize);
			byte[] data = new byte[sizeof(LevelGeneratorData)];

			fixed(byte* ptr = data)
				((LevelGeneratorData*)ptr)[0] = msg;

			Networking.client.stream.Write(new Span<byte>(data));
		}

		public void Generate(World world)
		{
			int size = Math.Clamp(this.m_Random.Next() % this.m_MaxSize, this.m_MaxSize / 2, this.m_MaxSize);

			var archetype = world.CreateArchetype(typeof(Position), typeof(TileChunk));

			var tileCollection = new RandomTileMapCollection(this.m_Random, size);
			tileCollection.Add(TileMap.Load(Assets.NameToPath("tm_flatland.tiles")), 30);
			tileCollection.Add(TileMap.Load(Assets.NameToPath("tm_hill.tiles")), 20);
			tileCollection.Add(TileMap.Load(Assets.NameToPath("tm_mountain.tiles")), 15);
			tileCollection.Add(TileMap.Load(Assets.NameToPath("tm_cliff.tiles")), 15);
			tileCollection.Add(TileMap.Load(Assets.NameToPath("tm_valley.tiles")), 15);

			var spawnLeft = GenerateSpawn(
				world,
				TileMap.Load(Assets.NameToPath("tm_spawn_left.tiles")),
				new Position(Vector2.Zero),
				new Spawn { damage = 0f, playerId = 0 });

			var position = new Vector2(spawnLeft.Width, 0f);

			float maxDeepness = 0f;

			for(int i = 0; i < size; ++i)
			{
				var map = tileCollection.Choose();
				var rect = map.Instantiate(world, new Position(position));
				
				position.X += rect.Width;

				float deepness = rect.Top + rect.Height;

				if(deepness > maxDeepness)
					maxDeepness = deepness;
			}

			var spawnRight = GenerateSpawn(
				world,
				TileMap.Load(Assets.NameToPath("tm_spawn_right.tiles")),
				new Position(position.X, 0f),
				new Spawn { damage = 0f, playerId = 1 });

			position.X += spawnRight.Width;

			world.ForEach(archetype, (batch) => {
				var chunks = batch.GetComponentDataReadWrite<TileChunk>();
				var positions = batch.GetComponentData<Position>();

				for(int i = 0; i < batch.length; ++i)
					GenerateRandomizedTiles(positions[i].value, ref chunks[i]);
			});

			Vector2 bedrockPosition = new Vector2(0f, maxDeepness);

			while(bedrockPosition.X < position.X)
			{
				var entity = world.CreateEntity(archetype);
				world.SetComponentData<Position>(entity, new Position(bedrockPosition));
				ref var chunk = ref world.RefComponentData<TileChunk>(entity);
				chunk.SetAll(new Tile(TileType.Bedrock));
				chunk.flags = chunk.flags | TileChunkFlags.NoneDestructible | TileChunkFlags.StaticNotEmpty;

				bedrockPosition.X += TileChunk.PixelSize;
			}
		}

		private FloatRect GenerateSpawn(World world, TileMap tileMap, Position position, Spawn data)
		{
			var archetype = world.CreateArchetype(typeof(Position), typeof(Spawn));
			var bounds = tileMap.Instantiate(world, position, out ReadOnlySpan<Entity> chunks);
			
			for(int i = 0; i < chunks.Length; ++i)
			{
				ref var chunk = ref world.RefComponentData<TileChunk>(chunks[i]);

				for(int t = 0; t < TileChunk.Tiles; ++t)
				{
					if(chunk[t].type == TileType.Spawn)
					{
						var spawn = world.CreateEntity(archetype);
						var spawnPos = position.value + TileChunk.PositionOffsetOfIndex(t);

						world.SetComponentData<Spawn>(spawn, data);
						world.SetComponentData<Position>(spawn, new Position(spawnPos));
						chunk.flags = chunk.flags | TileChunkFlags.IncludesSpawn;
						
						return bounds;
					}
				}
			}

			throw new InvalidOperationException("No spawn found.");
		}

		private void GenerateRandomizedTiles(Vector2 position, ref TileChunk chunk)
		{
			for(int i = 0; i < TileChunk.Tiles; ++i)
			{
				var tile = chunk[i];

				if((int)tile.type < 200)
					continue;

				var tilePos = position + TileChunk.PositionOffsetOfIndex(i);
				var rand = PerlinNoise.Generate(tilePos.X * 0.01, tilePos.Y * 0.01, (double)i * 0.1);
				var tiles = TileGroup.GetTypesFromTileType(tile.type);

				for(int t = 0; t < tiles.Length; ++t)
				{
					if(rand < ((double)t + 1)/(double)tiles.Length)
					{
						chunk[i] = new Tile(tiles[t]);
						break;
					}
				}
			}
		}
	}
}
