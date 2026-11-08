using System;

//Sealed no class can inherit from it but it's publicly accessible
public sealed class ImprovedNoise
{
	public readonly byte[] p = new byte[512];

	public ImprovedNoise(ulong seed){
		var rng = new SplitMix64(seed);
		for(int i = 0; i < 256; i++) p[i] = (byte)i;

		for(int i = 255; i > 0; i--){
			int k = (int)(rng.Next() % (ulong)(i + 1));
			(p[i], p[k]) = (p[k], p[i]);
		}
		for(int i=0 ; i < 256; i++) p[256 + i] = p[i];
	} 

	private static double Fade(double t) => t * t * t * (t * (t * 6 - 15) + 10);
	private static double Lerp(double t, double a, double b) => a + t * (b-a);

	private static double Grad(int hash, double x, double y, double z){
		int h = hash & 15;
		double u = h < 8 ? x : y;
		double v = h < 4 ? y : (h ==12 || h ==14? x : z);
		return ((h & 1) == 0 ? u : -u) + ((h & 2) ==0 ? v : -v);
	}

	public double Noise(double x, double y, double z){
		double fx = Math.Floor(x), fy = Math.Floor(y), fz =Math.Floor(z);
		int X = (int)fx & 255, Y = (int)fy & 255, Z = (int)fz & 255;
		x -= fx; y -=fy; z -=fz;

		double u = Fade(x), v = Fade(y), w = Fade(z);

		int A = p[X] + Y, AA = p[A] + Z, AB = p[A + 1] + Z;
		int B = p[X + 1] + Y, BA = p[B] + Z, BB = p[B + 1] + Z;

		return Lerp(w, Lerp(v, Lerp(u, Grad(p[AA], x, y, z),Grad(p[BA], x -1, y, z)),
								Lerp(u, Grad(p[AB], x, y-1, z), Grad(p[BB], x-1, y-1, z))),
								Lerp(v, Lerp(u, Grad(p[AA + 1], x, y, z-1), Grad(p[BA+1], x-1, y, z-1)),
								Lerp(u, Grad(p[AB+1], x, y-1, z-1), Grad(p[BB+1], x-1, y-1, z-1))));
	}

	public double Noise(double x, double y) => Noise(x, y, 0.5f);

	public double Fbm(double x, double y, double z, int octaves = 5, double lacunarity = 2.0f, double gain = 0.7f){
		double sum = 0, amp = 1, norm = 0, f = 1;
		for(int i = 0; i < octaves; i++){
			double o = i * 31.7;
			sum += amp * Noise(x * f + o, y * f + o, z * f + o);
			norm += amp;
			amp *= gain;
			f *= lacunarity;
		}
		return sum / norm;
	}

	private struct SplitMix64{
		private ulong s;
		public SplitMix64(ulong seed) { s = seed;}
		public ulong Next(){
			ulong z = ( s += 0x9E3779B97F4A7C15UL);
			z = (z ^ (z >>30)) * 0xBF58476D1CE4E5B9UL;
			z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
			return z ^ (z >> 31);
		}
	}
}
