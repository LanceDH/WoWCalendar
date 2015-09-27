using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calendar.Model
{
    class CalendarEvent
    {
        public static List<BLL.CalendarEvent> GetEventsVisibleMonth(DateTime start)
        {
            DateTime end = new DateTime(start.Year, start.Month, start.Day);
            // Add 1 too many days from the start
            end = end.AddDays(42);
            // Remove 1 milisecond to go back to the past day at night
            end = end.AddMilliseconds(-1);

            return DAL.CalendarEvent.GetAllBetweenDates(start, end);
        }

        public static List<BLL.CalendarEvent> FilterEventsForDay(DateTime day, List<BLL.CalendarEvent> events)
        {
            List<BLL.CalendarEvent> filter = new List<BLL.CalendarEvent>();

            foreach (BLL.CalendarEvent calEvent in events)
            {
                if (calEvent.CalendarDate.Date == day.Date)
                {
                    filter.Add(calEvent);
                    //events.Remove(calEvent);
                }
            }

            return filter;
        }

        public static int Insert(BLL.CalendarEvent calEvent)
        {
            return DAL.CalendarEvent.InsertOne(calEvent);
        }

        public static void Delete(int id)
        {
            DAL.CalendarEvent.DeleteOne(id);
        }

        public static Boolean Update(BLL.CalendarEvent calEvent)
        {
            return DAL.CalendarEvent.UpdateOne(calEvent);
        }
    }
}
