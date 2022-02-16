using Grumpy.HouseBattery.Client.Sonnen.Dtos;

namespace Grumpy.HouseBattery.Client.Sonnen.Interface
{
    public interface ISonnenBatteryClient
    {
        /// <summary>
        /// Current production from solar panels
        /// </summary>
        /// <returns>Production in watt</returns>
        int GetProduction();

        /// <summary>
        /// Current consumption in installation
        /// </summary>
        /// <returns>Consumption in watt</returns>
        int GetConsumption();

        /// <summary>
        /// Current battery level
        /// </summary>
        /// <returns>Current battery level in procent of total capacity</returns>
        int GetBatteryLevel();

        /// <summary>
        /// Current capacity of battery
        /// </summary>
        /// <returns>Current capacity in watt/hours (Wh)</returns>
        int GetBatteryCapacity();

        /// <summary>
        /// Size of battery 
        /// </summary>
        /// <returns>Total battery capacity in watt/hours (Wh)</returns>
        int GetBatterySize();

        OperatingMode GetOperatingMode();
        IEnumerable<TimeOfUseEvent> GetSchedule();
        void SetOperatingMode(OperatingMode operatingMode);
        void SetSchedule(IEnumerable<TimeOfUseEvent> schedule);
    }
}