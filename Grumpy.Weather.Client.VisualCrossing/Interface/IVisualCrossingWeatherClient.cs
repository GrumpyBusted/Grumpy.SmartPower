using Grumpy.SmartPower.Core.Model;

namespace Grumpy.Weather.Client.VisualCrossing.Interface
{
    public interface IVisualCrossingWeatherClient
    {
        public IEnumerable<WeatherItem> Get(DateOnly from, DateOnly to);
    }
}