﻿using System.Text.Json.Serialization;

namespace Grumpy.Weather.Client.VisualCrossing.Api.Timeline
{
    public class Root
    {
        [JsonPropertyName("days")]
        public List<Day> Days { get; set; } = new List<Day>();
    }
}