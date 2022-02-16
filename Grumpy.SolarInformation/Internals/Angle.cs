using Grumpy.Common.Extensions;

namespace Grumpy.SolarInformation.Internals
{
    internal class Angle
    {
        private readonly double _value;

        public Angle(double degrees)
        {
            _value = degrees;
        }

        public double Radians => _value.ToRadians();

        public static implicit operator double(Angle angle)
        {
            return angle._value;
        }

        public static implicit operator Angle(double degrees)
        {
            return new Angle(degrees);
        }

        public static Angle operator +(Angle a, Angle b)
        {
            return a._value + b._value;
        }

        public static Angle operator -(Angle a, Angle b)
        {
            return a._value - b._value;
        }

        public static Angle operator *(Angle a, Angle b)
        {
            return a._value * b._value;
        }

        public static Angle operator /(Angle a, Angle b)
        {
            return a._value / b._value;
        }

        public static bool operator >(Angle a, Angle b)
        {
            return a._value > b._value;
        }

        public static bool operator <(Angle a, Angle b)
        {
            return a._value < b._value;
        }

        public static Angle FromRadians(double radians)
        {
            return new Angle(radians.ToDegrees());
        }

        public static Angle Reduce(Angle angle)
        {
            return new Angle(angle - Math.Floor(angle / 360.0) * 360.0);
        }
    }
}