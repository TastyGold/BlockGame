﻿using System;

namespace BlockGame.Maths
{
    public struct VecInt2
    {
        public int x;
        public int y;

        public static VecInt2 Up = new VecInt2(0, -1);
        public static VecInt2 Right = new VecInt2(1, 0);
        public static VecInt2 Down = new VecInt2(0, 1);
        public static VecInt2 Left = new VecInt2(-1, 0);

        public static VecInt2 operator +(VecInt2 a, VecInt2 b)
        {
            return new VecInt2(a.x + b.x, a.y + b.y);
        }
        public static VecInt2 operator -(VecInt2 a, VecInt2 b)
        {
            return new VecInt2(a.x - b.x, a.y - b.y);
        }

        public static bool operator ==(VecInt2 a, VecInt2 b)
        {
            return !(a != b);
        }
        public static bool operator !=(VecInt2 a, VecInt2 b)
        {
            return a.x != b.x || a.y != b.y;
        }
        
        public override string ToString()
        {
            return $"<{x}, {y}>";
        }

        public override bool Equals(object obj)
        {
            return obj is VecInt2 @int &&
                   x == @int.x &&
                   y == @int.y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }

        public VecInt2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}