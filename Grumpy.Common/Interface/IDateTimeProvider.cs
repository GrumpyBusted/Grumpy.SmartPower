namespace Grumpy.Common.Interface;

public interface IDateTimeProvider
{
    DateTime Now { get; }
    DateTime Today { get; }
}