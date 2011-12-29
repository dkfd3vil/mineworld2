using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MineWorld
{
    public class HeightMap
    {
        public float[,] Heights { get; set; }
        public int Size { get; set; }

        public HeightMap(int size)
        {
            Size = size;
            Heights = new float[Size, Size];
            Perlin = new PerlinGenerator(new Random().Next(-50000, 50000));
            AddPerlinNoise(6f);
            Perturb(10, 10);
            for (int i = 0; i < 10; i++)
            {
                Erode(8);
            }
            Smoothen();
        }

        private PerlinGenerator Perlin { get; set; }

        public void AddPerlinNoise(float f)
        {
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    Heights[i, j] += Perlin.Noise(f * i / (float)Size, f * j / (float)Size, 0);
                }
            }
        }

        public void Perturb(float f, float d)
        {
            int u, v;
            float[,] temp = new float[Size, Size];
            for (int i = 0; i < Size; ++i)
            {
                for (int j = 0; j < Size; ++j)
                {
                    u = i + (int)(Perlin.Noise(f * i / (float)Size, f * j / (float)Size, 0) * d);
                    v = j + (int)(Perlin.Noise(f * i / (float)Size, f * j / (float)Size, 1) * d);
                    if (u < 0) u = 0; if (u >= Size) u = Size - 1;
                    if (v < 0) v = 0; if (v >= Size) v = Size - 1;
                    temp[i, j] = Heights[u, v];
                }
            }
            Heights = temp;
        }

        public void Erode(float smoothness)
        {
            for (int i = 1; i < Size - 1; i++)
            {
                for (int j = 1; j < Size - 1; j++)
                {
                    float d_max = 0.0f;
                    int[] match = { 0, 0 };

                    for (int u = -1; u <= 1; u++)
                    {
                        for (int v = -1; v <= 1; v++)
                        {
                            if (Math.Abs(u) + Math.Abs(v) > 0)
                            {
                                float d_i = Heights[i, j] - Heights[i + u, j + v];
                                if (d_i > d_max)
                                {
                                    d_max = d_i;
                                    match[0] = u; match[1] = v;
                                }
                            }
                        }
                    }

                    if (0 < d_max && d_max <= (smoothness / (float)Size))
                    {
                        float d_h = 0.5f * d_max;
                        Heights[i, j] -= d_h;
                        Heights[i + match[0], j + match[1]] += d_h;
                    }
                }
            }
        }

        public void Smoothen()
        {
            for (int i = 1; i < Size - 1; ++i)
            {
                for (int j = 1; j < Size - 1; ++j)
                {
                    float total = 0.0f;
                    for (int u = -1; u <= 1; u++)
                    {
                        for (int v = -1; v <= 1; v++)
                        {
                            total += Heights[i + u, j + v];
                        }
                    }

                    Heights[i, j] = total / 9.0f;
                }
            }
        }
    }
}
