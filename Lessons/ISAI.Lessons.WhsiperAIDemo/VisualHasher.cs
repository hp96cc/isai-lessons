using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ISAI.Lessons.WhsiperAIDemo
{
    public class VisualHasher
    {
        // A 64-bit hash is perfect for detecting slide changes
        public static ulong ComputeDifferenceHash(Image<Rgb24> image)
        {
            // 1. Shrink to a tiny 9x8 grid and grayscale it
            // The extra pixel in width allows us to compare 8 horizontal neighbors
            using var tiny = image.Clone(x => x
                .Resize(9, 8)
                .Grayscale());

            ulong hash = 0;
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    // If left pixel is brighter than right pixel, set a bit
                    var left = tiny[x, y].R;
                    var right = tiny[x + 1, y].R;
                    if (left > right)
                    {
                        hash |= (1UL << (y * 8 + x));
                    }
                }
            }
            return hash;
        }

        // Hamming Distance: How many bits are different?
        public static int GetSimilarityDistance(ulong hash1, ulong hash2)
        {
            // XOR gives us 1s where bits differ, then we count them
            return System.Numerics.BitOperations.PopCount(hash1 ^ hash2);
        }
    }
}
