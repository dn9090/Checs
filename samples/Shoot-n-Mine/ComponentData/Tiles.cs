using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Numerics;
using SFML.Graphics;
using Checs;

namespace Shoot_n_Mine
{
	public enum TileType : short
	{
		None = 0,
		Dirt = 1,
		Grass = 2,
		Stone = 3,
		Bedrock = 4,
		Derylium = 5,
		Ziklium = 6,
		Dud = 7,
		Brick = 8,
		Spawn = 100,
		RandomBlock = 200,
		RandomNatural = 201,
		RandomResource = 202
	}

	public static class TileGroup
	{
		public static TileType[] blocks = {
			TileType.Dirt, TileType.Stone, TileType.Derylium,
			TileType.Ziklium, TileType.Dud
		};

		public static TileType[] natural = {
			TileType.Dirt, TileType.Stone
		};

		public static TileType[] resources = {
			TileType.Derylium, TileType.Ziklium
		};

		public static TileType[] GetTypesFromTileType(TileType type)
		{
			switch(type)
			{
				case TileType.RandomBlock:
					return blocks;
				case TileType.RandomNatural:
					return natural;
				case TileType.RandomResource:
					return resources;
				default:
					return null;
			}
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct Tile
	{
		public const int PixelSize = 24;

		public static Vector2 OriginOffset = new Vector2(PixelSize * 0.5f, PixelSize * 0.5f);

		public TileType type
		{
			get => (TileType)this.m_Type;
			set => this.m_Type = (short)value;
		}

		public int health
		{
			get => (int)this.m_Health;
			set => this.m_Health = (short)value;
		}

		[FieldOffset(0)]
		private int m_Value;

		[FieldOffset(0)]
		private short m_Health;

		[FieldOffset(2)]
		private short m_Type;

		public Tile(TileType type)
		{
			this.m_Value = 0;
			this.m_Type = (short)type;

			switch(type)
			{
				case TileType.Dirt:
				case TileType.Grass:
				case TileType.Dud:
					this.m_Health = 3;
					break;
				case TileType.Stone:
					this.m_Health = 6;
					break;
				case TileType.Derylium:
					this.m_Health = 4;
					break;
				case TileType.Ziklium:
					this.m_Health = 4;
					break;
				default:
					this.m_Health = 10;
					break;
			}
		}

		public Tile(TileType type, int health)
		{
			this.m_Value = 0;
			this.m_Health = (short)health;
			this.m_Type = (short)type;
		}

		private Tile(int value)
		{
			this.m_Health = 0;
			this.m_Type = 0;
			this.m_Value = value;
		}

		public static explicit operator Tile(int value) => new Tile(value);

		public static explicit operator int(Tile value) => value.m_Value;
	}

	[Flags]
	public enum TileChunkFlags
	{
		NoneDestructible = 1 << 0,
		StaticNotEmpty = 1 << 1,
		IncludesSpawn = 1 << 2
	}

	public unsafe struct TileChunk : IComponentData
	{
		public const int PixelSize = Tile.PixelSize * 24;

		public const int Size = 24;

		public const int Tiles = Size * Size;

		public TileChunkFlags flags;

		public fixed int tiles[Tiles];

		public Tile this[int index]
		{
			get => (Tile)tiles[index];
			set => tiles[index] = (int)value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetAll(Tile tile)
		{
			for(int i = 0; i < TileChunk.Tiles; ++i)
				tiles[i] = (int)tile;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 PositionOffsetOfIndex(int index)
		{
			var row = index / TileChunk.Size;
			var col = index % TileChunk.Size;
			return new Vector2(Tile.PixelSize * col, Tile.PixelSize * row);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Intersects(Vector2 position, FloatRect rect)
		{
			var chunkRect = new FloatRect(position.X, position.Y, TileChunk.PixelSize, TileChunk.PixelSize);
			return rect.Intersects(chunkRect);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Intersects(Vector2 position, Vector2 circleOrigin, float radius)
		{
			float closestX = Math.Clamp(circleOrigin.X, position.X, position.X + TileChunk.PixelSize);
			float closestY = Math.Clamp(circleOrigin.Y, position.Y, position.Y + TileChunk.PixelSize);

			float distanceX = circleOrigin.X - closestX;
			float distanceY = circleOrigin.Y - closestY;

			float distanceSqr = (distanceX * distanceX) + (distanceY * distanceY);

			return distanceSqr < (radius * radius);
		}
	}
}
