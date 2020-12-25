using System;
using System.Collections.Generic;
using System.Numerics;
using SFML.Window;
using SFML.System;

namespace Shoot_n_Mine.Engine
{
	public static class PerlinNoise
	{
		private static readonly int[] p = {
			151,160,137,91,90,15,
			131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
			190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
			88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
			77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
			102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
			135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
			5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
			223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
			129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
			251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
			49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
			138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
		};

		private static readonly int[] permutation;

		static PerlinNoise()
		{
			permutation = new int[512];
			for(int x = 0; x < permutation.Length; ++x)
				permutation[x] = p[x%256];
		}

		public static double Generate(double x, double y, double z)
		{
			int xi = ((int)(Math.Floor(x) % 256) + 256) % 256;
			int yi = ((int)(Math.Floor(y) % 256) + 256) % 256;
			int zi = ((int)(Math.Floor(z) % 256) + 256) % 256;

			double xf = x - (int)x;
			if (x < 0)
				xf = 1 + xf;
			
			double yf = y - (int)y;
			if (y < 0)
				yf = 1 + yf;
			
			double zf = z - (int)z;
			if (z < 0)
				zf = 1 + zf;

			double u = fade(xf);
			double v = fade(yf);
			double w = fade(zf);
			
			int a  = permutation[xi]   + yi;
			int aa = permutation[a]    + zi;
			int ab = permutation[a+1]  + zi;
			int b  = permutation[xi+1] + yi;
			int ba = permutation[b]    + zi;
			int bb = permutation[b+1]  + zi;
			
			double x1, x2, y1, y2;
			x1 = lerp(	grad(permutation[aa  ], xf  , yf  , zf),
						grad(permutation[ba  ], xf-1, yf  , zf),
						u);
			x2 = lerp(	grad(permutation[ab  ], xf  , yf-1, zf),
						grad(permutation[bb  ], xf-1, yf-1, zf),
						u);
			y1 = lerp(x1, x2, v);

			x1 = lerp(	grad(permutation[aa+1], xf  , yf  , zf-1),
						grad(permutation[ba+1], xf-1, yf  , zf-1),
						u);
			x2 = lerp(  grad(permutation[ab+1], xf  , yf-1, zf-1),
						grad(permutation[bb+1], xf-1, yf-1, zf-1),
						u);
			y2 = lerp(x1, x2, v);
			
			return (lerp(y1, y2, w) + 1) / 2;
		}
		
		private static double grad(int hash, double x, double y, double z)
		{
			int h = hash & 15;
			double u = h < 8 ? x : y;
			double v;

			if(h < 4)
				v = y;
			else if(h == 12 || h == 14)
				v = x;
			else
				v = z;
			
			return ((h&1) == 0 ? u : -u)+((h&2) == 0 ? v : -v);
		}
	
		private static double fade(double t) => t * t * t * (t * (t * 6 - 15) + 10);
	
		private static double lerp(double a, double b, double x) => a + x * (b - a);
	}
}
