using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// Interaction logic for DayOverviewWindow.xaml
    /// </summary>
    public partial class DayOverviewWindow : Window
    {
        private MainWindow parent;
        private BLL.CalendarDay currentDay;
        private ObservableCollection<BLL.CalendarEvent> oEvents = new ObservableCollection<BLL.CalendarEvent>();
        private readonly int OPEN_HEIGHT = 130;

        public DayOverviewWindow(MainWindow parent)
        {
            InitializeComponent();

            this.parent = parent;
        }

        private void img_CloseButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Hide();
        }

        private void wdw_DayOverview_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var location = parent.PointToScreen(new Point(0, 0));
            MainWindow._dayOverview.Left = location.X + parent.Width;
            MainWindow._dayOverview.Top = location.Y + 20;
        }

        public void ShowDay(BLL.CalendarDay day)
        {
            currentDay = day;
            lbl_Date.Content = day.Date.ToString("MMMM d yyyy");

            UpdateEvents();

            txbl_EditButtonText.Text = "Edit";
            grd_EditEvent.Height = 0;
            grd_CreateEvent.Height = 0;
        }

        private void txt_AddTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox target = (TextBox)sender;
            string s = target.Name;
            int longTime = 0;
            string input = target.Text;
            
            input = input.Replace(":", "");
            
            // input wasn't numbers
            if (!Int32.TryParse(input, out longTime)) {
                target.Text = "";
                return;
            }
            if (input.Length >= 2) {
                input = input.Insert(2, ":");
            }
            target.Text = input;
            target.CaretIndex = target.Text.Length;
        }

        private void btn_CreateEvent_Click(object sender, RoutedEventArgs e)
        {
            if (txt_AddTitle.Text.Equals(""))
            {
                return;
            }

            BLL.CalendarEvent calEvent = new BLL.CalendarEvent(0);
            calEvent.Title = txt_AddTitle.Text;
            calEvent.Detail = txt_AddDetail.Text;
            DateTime eventTime = new DateTime(currentDay.Date.Year, currentDay.Date.Month, currentDay.Date.Day);
            string input = txt_AddTime.Text;
            String[] time = input.Split(':');

            if (Int32.Parse(time[0]) >= 24 || Int32.Parse(time[1]) >= 60)
            {
                return;
            }

            eventTime = eventTime.AddHours(Int32.Parse(time[0]));
            eventTime = eventTime.AddMinutes(Int32.Parse(time[1]));
            calEvent.CalendarDate = eventTime;

            if (Model.CalendarEvent.Insert(calEvent) > 0)
            {
                txt_AddTitle.Text = "";
                txt_AddDetail.Text = "";
                txt_AddTime.Text = "00:00";
                grd_CreateEvent.Height = 0;
            }

            parent.UpdateCalendarDates();
            UpdateEvents();
            
        }

        private void txt_AddTime_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox target = (TextBox)sender;
            string input = target.Text;

            input = input.Replace(":", "");

            for (int i = input.Length; i < 4; i++)
            {
                input += "0";
            }
            target.Text = input;
        }

        private void btn_ShowCreateEvent_Click(object sender, RoutedEventArgs e)
        {
            if (grd_CreateEvent.Height == 0)
            {
                grd_CreateEvent.Height = OPEN_HEIGHT;
                grd_EditEvent.Height = 0;
                txbl_EditButtonText.Text = "Edit";
            }
            else
            {
                grd_CreateEvent.Height = 0;
            }
        }

        private void btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (lst_Events.SelectedValue == null) { return; }

            BLL.CalendarEvent calEvent = (BLL.CalendarEvent)lst_Events.SelectedValue;

            Model.CalendarEvent.Delete(calEvent.GetId());
            parent.UpdateCalendarDates();
            UpdateEvents();
        }

        private void UpdateEvents()
        {
            oEvents.Clear();
            foreach (BLL.CalendarEvent cEv in currentDay.Events)
            {
                oEvents.Add(cEv);
            }

            lst_Events.ItemsSource = oEvents;
        }

        private void btn_EditEvent_Click(object sender, RoutedEventArgs e)
        {
            if (lst_Events.SelectedValue == null) { return; }

            if (grd_EditEvent.Height == 0)
            {
                grd_EditEvent.Height = OPEN_HEIGHT;
                grd_CreateEvent.Height = 0;
                txbl_EditButtonText.Text = "Cancel";
            }
            else
            {
                grd_EditEvent.Height = 0;
                txbl_EditButtonText.Text = "Edit";
            }
        }

        private void lst_Events_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lst_Events.SelectedValue == null) { return; }

            BLL.CalendarEvent calEvent = (BLL.CalendarEvent)lst_Events.SelectedValue;

            txt_EditTitle.Text = calEvent.Title;
            txt_EditDetail.Text = calEvent.Detail;
            txt_EditTime.Text = calEvent.CalendarDate.ToString("HH:mm");
        }

        private void btn_EditEventConfirm_Click(object sender, RoutedEventArgs e)
        {
            //shouldn't be possible but hey..
            if (lst_Events.SelectedValue == null) { return; }

            BLL.CalendarEvent selectedEvent = (BLL.CalendarEvent)lst_Events.SelectedValue;
            BLL.CalendarEvent editEvent = new BLL.CalendarEvent(selectedEvent.GetId());
            editEvent.Title = txt_EditTitle.Text;
            editEvent.Detail = txt_EditDetail.Text;

            DateTime eventTime = new DateTime(currentDay.Date.Year, currentDay.Date.Month, currentDay.Date.Day);
            string input = txt_EditTime.Text;
            String[] time = input.Split(':');

            if (Int32.Parse(time[0]) >= 24 || Int32.Parse(time[1]) >= 60)
            {
                return;
            }

            eventTime = eventTime.AddHours(Int32.Parse(time[0]));
            eventTime = eventTime.AddMinutes(Int32.Parse(time[1]));
            editEvent.CalendarDate = eventTime;

            if (Model.CalendarEvent.Update(editEvent))
            {
                grd_EditEvent.Height = 0;
                txbl_EditButtonText.Text = "Edit";
            }

            parent.UpdateCalendarDates();
            UpdateEvents();
        }


    }
}
