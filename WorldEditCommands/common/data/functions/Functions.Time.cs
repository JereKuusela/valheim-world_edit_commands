// Shared code: keep identical in EWD, EWP and WEC (common/). Sync changes to all three.
using System;
using System.Globalization;
using Common;

namespace Data;

public partial class Functions
{
  private string HandleTime(string value)
  {
    var format = value;
    var dayLength = EnvMan.instance.m_dayLengthSec;
    var hourLength = dayLength / 24.0;
    var minuteLength = hourLength / 60.0;
    var day = (int)(time / dayLength);
    var hours = time - (day * dayLength);
    var hour = (int)(hours / hourLength);
    var minute = (int)((hours - (hour * hourLength)) / minuteLength);
    var second = (int)((hours - (hour * hourLength) - (minute * minuteLength)) / (minuteLength / 60.0));

    // Create a DateTimeOffset representing the game time
    var dt = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero)
      .AddDays(day)
      .AddHours(hour)
      .AddMinutes(minute)
      .AddSeconds(second);

    return dt.ToString(format, CultureInfo.InvariantCulture);
  }
  private string HandleRealtime(string value)
  {
    var parts = value.Split(Separator);
    var format = parts[0];
    var timezoneOffset = parts.Length > 1 ? Parse.Float(parts[1], 0f) : (float)TimeZoneInfo.Local.BaseUtcOffset.TotalHours;
    var utcNow = DateTimeOffset.UtcNow;
    var offsetTime = utcNow.AddHours(timezoneOffset);
    return offsetTime.ToString(format, CultureInfo.InvariantCulture);
  }
}
