﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Telerik.Sitefinity.Events.Model;
using Telerik.Sitefinity.Frontend.Events.Mvc.StringResources;
using Telerik.Sitefinity.Frontend.Mvc.Models;
using Telerik.Sitefinity.Localization;
using Telerik.Sitefinity.Modules.Events;
using Telerik.Sitefinity.RecurrentRules;

namespace Telerik.Sitefinity.Frontend.Events.Mvc.Helpers
{
    /// <summary>
    /// Helper class for events and related widgets
    /// </summary>
    public static class EventHelper
    {
        /// <summary>
        /// The calendar color in hex format depending on the event calendar.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The calendar color in hex format depending on the event calendar.</returns>
        public static string EventCalendarColour(this ItemViewModel item)
        {
            var ev = item.DataItem as Event;
            if (ev == null || ev.Parent == null)
                return string.Empty;

            return ev.Parent.Color;
        }

        /// <summary>
        /// The event basic date description.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The event dates text.</returns>
        public static string EventDates(this ItemViewModel item)
        {
            var ev = item.DataItem as Event;
            if (ev == null)
                return string.Empty;

            if (ev.IsRecurrent && !string.IsNullOrEmpty(ev.RecurrenceExpression))
                return BuildRecurringEvent(ev);
            else
                return BuildNonRecurringEvent(ev);
        }
       
        /// <summary>
        /// Generates the google URL.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The google URL.</returns>
        public static string GenerateGoogleUrl(this ItemViewModel item)
        {
            var ev = item.DataItem as Event;
            if (ev == null)
                return string.Empty;

            var url = GenerateGoogleUrlMethodInfo.Value.Invoke(null, new object[] { ev });
            return url as string;
        }

        /// <summary>
        /// Generates the outlook URL.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The outlook URL.</returns>
        public static string GenerateOutlookUrl(this ItemViewModel item)
        {
            var ev = item.DataItem as Event;
            if (ev == null)
                return string.Empty;

            var url = GenerateOutlookUrlMethodInfo.Value.Invoke(GenerateOutlookUrlInstance.Value, new object[] { ev });
            return url as string;
        }

        /// <summary>
        /// Generates the iCal URL.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The iCal URL.</returns>
        public static string GenerateICalUrl(this ItemViewModel item)
        {
            var ev = item.DataItem as Event;
            if (ev == null)
                return string.Empty;

            var url = GenerateICalUrlMethodInfo.Value.Invoke(GenerateICalUrlInstance.Value, new object[] { ev });
            return url as string;
        }

        private static string BuildHourMinute(DateTime time)
        {
            var format = string.Empty;
            if (time.Minute == 0)
                format = "hh tt";
            else
                format = "hh mm tt";

            return time.ToString(format, CultureInfo.InvariantCulture).TrimStart('0');
        }

        private static string BuildDayMonthYear(DateTime time)
        {
            var format = string.Empty;
            if (time.Year == DateTime.UtcNow.Year)
                format = "dd MMMM";
            else
                format = "dd MMMM, yyyy";

            return time.ToString(format, CultureInfo.InvariantCulture).TrimStart('0');
        }

        private static string BuildRecurringEvent(Event ev)
        {
            var result = new StringBuilder();

            var start = ev.EventStart.ToSitefinityUITime();
            var recurrenceDescriptor = GetRecurrenceDescriptor(ev.RecurrenceExpression);
            result.Append(BuildRecurringEvent(recurrenceDescriptor));
            result.Append(Comma);
            result.Append(WhiteSpace);

            result.Append(BuildDayMonthYear(start));

            result.Append(WhiteSpace);
            result.Append(Res.Get<EventResources>().At);
            result.Append(WhiteSpace);

            if (ev.AllDayEvent)
                result.Append(Midnight);
            else
                result.Append(BuildHourMinute(start));

            if (ev.EventEnd.HasValue)
            {
                var end = ev.EventEnd.Value.ToSitefinityUITime();
                result.Append(Dash);

                result.Append(BuildDayMonthYear(end));

                result.Append(WhiteSpace);
                result.Append(Res.Get<EventResources>().At);
                result.Append(WhiteSpace);

                if (ev.AllDayEvent)
                    result.Append(Midnight);
                else
                    result.Append(BuildHourMinute(end));
            }

            return result.ToString();
        }

        private static string BuildNonRecurringEvent(Event ev)
        {
            var result = new StringBuilder();

            var start = ev.EventStart.ToSitefinityUITime(); 
            result.Append(BuildDayMonthYear(start));

            if (!ev.AllDayEvent)
            {
                result.Append(WhiteSpace);
                result.Append(Res.Get<EventResources>().At);
                result.Append(WhiteSpace);
                result.Append(BuildHourMinute(start));
            }

            if (ev.EventEnd.HasValue)
            {
                var end = ev.EventEnd.Value.ToSitefinityUITime();
                result.Append(Dash);

                if (!ev.AllDayEvent && start.Date == end.Date)
                {
                    result.Append(BuildHourMinute(end));
                }
                else
                {
                    result.Append(BuildDayMonthYear(end));

                    if (!ev.AllDayEvent)
                    {
                        result.Append(WhiteSpace);
                        result.Append(Res.Get<EventResources>().At);
                        result.Append(WhiteSpace);
                        result.Append(BuildHourMinute(end));
                    }
                }
            }

            return result.ToString();
        }

        private static string BuildRecurringEvent(IRecurrenceDescriptor descriptor)
        {
            if (descriptor == null)
                return string.Empty;

            var result = string.Empty;

            switch (descriptor.Frequency)
            {
                case RecurrenceFrequency.Daily: result = BuildFromDaily(descriptor);
                    break;
                case RecurrenceFrequency.Weekly: result = BuildFromWeekly(descriptor);
                    break;
                case RecurrenceFrequency.Monthly: result = BuildFromMonthly(descriptor);
                    break;
                case RecurrenceFrequency.Yearly: result = BuildFromYearly(descriptor);
                    break;
                default:
                    break;
            }

            return result;
        }

        private static IRecurrenceDescriptor GetRecurrenceDescriptor(string recurrenceExpression)
        {
            if (string.IsNullOrEmpty(recurrenceExpression))
                return null;

            var descriptor = ICalRecurrenceSerializerDeserializeMethodInfo.Value.Invoke(ICalRecurrenceSerializerInstance.Value, new object[] { recurrenceExpression });
            return descriptor as IRecurrenceDescriptor;
        }

        private static string BuildFromDaily(IRecurrenceDescriptor recurrenceDescriptor)
        {
            var result = string.Empty;

            if (recurrenceDescriptor.DaysOfWeek == RecurrenceDay.WeekDays)
            {
                result = Res.Get<Labels>("EveryWeekday");
            }
            else if (recurrenceDescriptor.DaysOfWeek == RecurrenceDay.EveryDay)
            {
                if (recurrenceDescriptor.Interval == 1)
                {
                    result = Res.Get<Labels>("Every") + Res.Get<Labels>("Day").ToLower();
                }
                else
                {
                    result = Res.Get<Labels>("Every") + " " + recurrenceDescriptor.Interval + " " + Res.Get<Labels>("Days").ToLower();
                }
            }

            return result;
        }

        private static string BuildFromWeekly(IRecurrenceDescriptor recurrenceDescriptor)
        {
            var result = string.Empty;

            if (recurrenceDescriptor.Interval == 1)
            {
                result = Res.Get<Labels>("Every") + " " + Res.Get<EventsResources>("Week").ToLower() + " " + Res.Get<Labels>("On").ToLower();
            }
            else
            {
                result = Res.Get<Labels>("Every") + " " + recurrenceDescriptor.Interval + " " + Res.Get<EventsResources>("Weeks").ToLower() + " " + Res.Get<Labels>("On").ToLower();
            }

            var days = new List<string>() { };
            if (recurrenceDescriptor.DaysOfWeek.HasFlag(RecurrenceDay.Monday))
            {
                days.Add(Res.Get<Labels>(RecurrenceDay.Monday.ToString()));
            }

            if (recurrenceDescriptor.DaysOfWeek.HasFlag(RecurrenceDay.Tuesday))
            {
                days.Add(Res.Get<Labels>(RecurrenceDay.Tuesday.ToString()));
            }

            if (recurrenceDescriptor.DaysOfWeek.HasFlag(RecurrenceDay.Wednesday))
            {
                days.Add(Res.Get<Labels>(RecurrenceDay.Wednesday.ToString()));
            }

            if (recurrenceDescriptor.DaysOfWeek.HasFlag(RecurrenceDay.Thursday))
            {
                days.Add(Res.Get<Labels>(RecurrenceDay.Thursday.ToString()));
            }

            if (recurrenceDescriptor.DaysOfWeek.HasFlag(RecurrenceDay.Friday))
            {
                days.Add(Res.Get<Labels>(RecurrenceDay.Friday.ToString()));
            }

            if (recurrenceDescriptor.DaysOfWeek.HasFlag(RecurrenceDay.Saturday))
            {
                days.Add(Res.Get<Labels>(RecurrenceDay.Saturday.ToString()));
            }

            if (recurrenceDescriptor.DaysOfWeek.HasFlag(RecurrenceDay.Sunday))
            {
                days.Add(Res.Get<Labels>(RecurrenceDay.Sunday.ToString()));
            }

            result += string.Concat(" ", string.Join(", ", days));

            return result;
        }

        private static string BuildFromMonthly(IRecurrenceDescriptor recurrenceDescriptor)
        {
            var result = string.Empty;

            if (recurrenceDescriptor.DayOfMonth == 0)
            {
                var occurrenceOrdinal = BuildOccurrenceOrdinal(recurrenceDescriptor);
                result = string.Concat(occurrenceOrdinal, " ", Res.Get<Labels>(recurrenceDescriptor.DaysOfWeek.ToString()), " ", Res.Get<Labels>("OfEvery"));
            }
            else
            {
                result = string.Concat(Res.Get<Labels>("Day"), " ", recurrenceDescriptor.DayOfMonth, " ", Res.Get<Labels>("OfEvery"));
            }

            if (recurrenceDescriptor.Interval == 1)
            {
                result += string.Concat(" ", Res.Get<Labels>("MonthOrMonths").ToLower());
            }
            else
            {
                result += string.Concat(" ", recurrenceDescriptor.Interval, " ", Res.Get<Labels>("MonthOrMonths").ToLower());
            }

            return result;
        }

        private static string BuildFromYearly(IRecurrenceDescriptor recurrenceDescriptor)
        {
            var result = string.Empty;

            if (recurrenceDescriptor.DayOrdinal == 0)
            {
                result = string.Concat(Res.Get<Labels>("Every"), " ", recurrenceDescriptor.DayOfMonth, " ", Res.Get<Labels>("Day").ToLower(), " ", Res.Get<Labels>("Of").ToLower(), " ", Res.Get<Labels>(recurrenceDescriptor.Month.ToString()));
            }
            else
            {
                var occurrenceOrdinal = BuildOccurrenceOrdinal(recurrenceDescriptor);
                result = string.Concat(Res.Get<Labels>("The"), " ", occurrenceOrdinal.ToLower());
                result += string.Concat(" ", Res.Get<Labels>(recurrenceDescriptor.DaysOfWeek.ToString()), " ", Res.Get<Labels>("Of").ToLower(), " ", Res.Get<Labels>(recurrenceDescriptor.Month.ToString()));
            }

            return result;
        }

        private static string BuildOccurrenceOrdinal(IRecurrenceDescriptor recurrenceDescriptor)
        {
            switch (recurrenceDescriptor.DayOrdinal)
            {
                case 1: return char.ToUpper(Res.Get<Labels>("First")[0]) + Res.Get<Labels>("First").Substring(1);
                case 2: return char.ToUpper(Res.Get<Labels>("Second")[0]) + Res.Get<Labels>("Second").Substring(1);
                case 3: return char.ToUpper(Res.Get<Labels>("Third")[0]) + Res.Get<Labels>("Third").Substring(1);
                case 4: return char.ToUpper(Res.Get<Labels>("Fourth")[0]) + Res.Get<Labels>("Fourth").Substring(1);
                default: return char.ToUpper(Res.Get<Labels>("Last")[0]) + Res.Get<Labels>("Last").Substring(1);
            }
        }

        private static readonly Lazy<MethodInfo> GenerateGoogleUrlMethodInfo =
            new Lazy<MethodInfo>(() => Type.GetType("Telerik.Sitefinity.Modules.Events.Web.UI.Export.GoogleEventExporterHelper, Telerik.Sitefinity.ContentModules").GetMethod("GenerateGoogleUrl", BindingFlags.Public | BindingFlags.Static));

        private static readonly Lazy<object> GenerateOutlookUrlInstance =
            new Lazy<object>(() => Activator.CreateInstance(Type.GetType("Telerik.Sitefinity.Modules.Events.Web.UI.Export.OutlookEventExporter, Telerik.Sitefinity.ContentModules")));

        private static readonly Lazy<MethodInfo> GenerateOutlookUrlMethodInfo =
            new Lazy<MethodInfo>(() => Type.GetType("Telerik.Sitefinity.Modules.Events.Web.UI.Export.OutlookEventExporter, Telerik.Sitefinity.ContentModules").GetMethod("GenerateOutlookUrl", BindingFlags.NonPublic | BindingFlags.Instance));

        private static readonly Lazy<object> GenerateICalUrlInstance =
            new Lazy<object>(() => Activator.CreateInstance(Type.GetType("Telerik.Sitefinity.Modules.Events.Web.UI.Export.ICalEventExporter, Telerik.Sitefinity.ContentModules")));

        private static readonly Lazy<MethodInfo> GenerateICalUrlMethodInfo =
            new Lazy<MethodInfo>(() => Type.GetType("Telerik.Sitefinity.Modules.Events.Web.UI.Export.ICalEventExporter, Telerik.Sitefinity.ContentModules").GetMethod("GenerateICalUrl", BindingFlags.NonPublic | BindingFlags.Instance));

        private static readonly Lazy<object> ICalRecurrenceSerializerInstance =
            new Lazy<object>(() => Activator.CreateInstance(Type.GetType("Telerik.Sitefinity.RecurrentRules.ICalRecurrenceSerializer, Telerik.Sitefinity.RecurrentRules")));

        private static readonly Lazy<MethodInfo> ICalRecurrenceSerializerDeserializeMethodInfo =
            new Lazy<MethodInfo>(() => Type.GetType("Telerik.Sitefinity.RecurrentRules.ICalRecurrenceSerializer, Telerik.Sitefinity.RecurrentRules").GetMethod("Deserialize", BindingFlags.Instance | BindingFlags.Public));

        private const string Dash = "-";
        private const string Comma = ",";
        private const string WhiteSpace = " ";
        private const string Midnight = "0:00 AM";
    }
}
