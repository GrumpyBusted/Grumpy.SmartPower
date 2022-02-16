using FluentAssertions;
using Grumpy.Common.Extensions;
using Xunit;

namespace Grumpy.Common.UnitTests
{
    public class DoubleExtensionsTests
    {
        [Fact]
        public void ToRadiansShouldReturnCorrectValue()
        {
            double value = 57.2958;

            var res = value.ToRadians();

            res.Should().BeApproximately(1, 0.0001);
        }

        [Fact]
        public void ToDegreesShouldReturnCorrectValue()
        {
            double value = 1;

            var res = value.ToDegrees();

            res.Should().BeApproximately(57.2958, 0.0001);
        }
    }
}
