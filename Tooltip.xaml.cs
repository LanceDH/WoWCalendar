using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Calendar
{
    /// <summary>
    /// Interaction logic for Tooltip.xaml
    /// </summary>
    public partial class Tooltip : Window
    {
        
        public Tooltip()
        {
            InitializeComponent();
        }

        public void SetDay(BLL.CalendarDay day)
        {

            lbl_Date.Content = day.Date.ToString("dddd, MMMM d yyyy");

            txbl_EventList.Text = "";
            
            ListEvents(day.Events);


        }

        private void ListEvents(List<BLL.CalendarEvent> events)
        {
            if (events.Count == 0) { return; }

            BLL.CalendarEvent calEvent;

            for (int i = 0; i < events.Count; i++)
			{
                calEvent = events[i];
			    txbl_EventList.Text += calEvent.CalendarDate.ToString("HH:mm");
                txbl_EventList.Text += "     " + calEvent.Title;
                txbl_EventList.Text += "\n";

                if (i < events.Count - 1 )
                {
                    txbl_EventList.Text += "\n";
                }

			}

        }


    }
}
