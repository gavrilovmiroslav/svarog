using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace svarog.Algorithms
{
    public static class Lerp
    {
        public static float Linear(float x, float y, float t)
        {
            return x + (y - x) * t;
        }

        public static float Cubic(float x, float y, float t)
        {
            return x + (y - x) * t * t * t;
        }

        public static Vector2f Linear(Vector2f x, Vector2f y, float t)
        {
            return x + (y - x) * t;
        }

        public static Vector2f Cubic(Vector2f x, Vector2f y, float t)
        {
            return x + (y - x) * t * t * t;
        }
    }
}
