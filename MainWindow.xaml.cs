using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Calendar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BLL.CalendarDay[,] _calendarDays = new BLL.CalendarDay[7, 6];
        private DateTime _shownDate;
        public static Tooltip _tooltip = new Tooltip();
        public static DayOverviewWindow _dayOverview;

        public MainWindow()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            InitializeComponent();

            this.Icon = new BitmapImage(new Uri("../../Images/TaskbarIcon.png", UriKind.Relative));

            DAL.Package.URL = "http://api.openweathermap.org/data/2.5/weather";

            App.Current.MainWindow = this;
            _tooltip.Topmost = true;
            _tooltip.ShowInTaskbar = false;
            _tooltip.Show();
            _tooltip.Hide();

            _dayOverview = new DayOverviewWindow(this);
            _dayOverview.ShowInTaskbar = false;
            _dayOverview.Hide();


            DateTime today = DateTime.Today;
            _shownDate = new DateTime(today.Year, today.Month, 1);
            DateTime startDay = new DateTime(today.Year, today.Month, 1);
            int weekDayStart = (int)startDay.DayOfWeek;
            startDay = startDay.AddDays(-weekDayStart + 1);

            for (int i = 0; i < 42; i++)
            {
                BLL.CalendarDay cDay = new BLL.CalendarDay(this, startDay.Day);
                Grid dayGrid = cDay.GetGrid();
                dayGrid.Margin = new Thickness(90 * (i % 7), 29 + 90 * (i / 7), 0, 0);
                grid_DayGrid.Children.Add(dayGrid);
                cDay.SetEventListTemplate((DataTemplate)this.FindResource("dayEventItem") as DataTemplate);

                _calendarDays[i % 7, i / 7] = cDay;

                startDay = startDay.AddDays(1);
            }


            UpdateCalendarDates();

        }

        public void ResetDayHighlights()
        {
            for (int i = 0; i < 42; i++)
            {
                _calendarDays[i % 7, i / 7].IsSelected = false;

            }
        }

        public void UpdateCalendarDates()
        {
            DateTime today = DateTime.Today;
            DateTime startDay = new DateTime(_shownDate.Year, _shownDate.Month, 1);
            int weekDayStart = (int)(startDay.DayOfWeek + 6) % 7 + 1;
            startDay = startDay.AddDays(1 - weekDayStart);

            List<BLL.CalendarEvent> events = Model.CalendarEvent.GetEventsVisibleMonth(startDay);

            for (int i = 0; i < 42; i++)
            {
                BLL.CalendarDay cDay = _calendarDays[i % 7, i / 7];
                cDay.Reset();
                cDay.Date = startDay;
                cDay.DayNumber = startDay.Day;
                cDay.SetTemperature(-1);
                cDay.Events = Model.CalendarEvent.FilterEventsForDay(startDay, events);

                if (startDay.Month != _shownDate.Month)
                {
                    cDay.ShowShadow();
                }
                else
                {
                    cDay.HideShadow();
                }

                if (_shownDate.Month == today.Month && _shownDate.Year == today.Year)
                {
                    img_CurrentDay.Opacity = 1;
                    if (startDay.Date == today.Date)
                    {
                        //cDay.SetTemperature(DAL.Weather.GetTodayTemp());
                        int centerX = 90 * (i % 7) + 45;
                        int centerY = 29 + 90 * (i / 7) + 45;

                        img_CurrentDay.Margin = new Thickness(centerX - 70, centerY - 70, 0, 0);
                    }
                }
                else
                {
                    img_CurrentDay.Opacity = 0;
                }



                startDay = startDay.AddDays(1);
            }

            lbl_HeaderMonth.Content = _shownDate.ToString("MMMM");
            txbl_HeaderYear.Text = _shownDate.ToString("yyyy");

            UpdateDayShadows();
        }

        private void UpdateDayShadows()
        {
            // Next month days
            for (int i = 0; i < 42; i++)
            {
                int x = i % 7;
                int y = i / 7;
                int top = 0;
                int bottom = 0;
                BLL.CalendarDay cDay = _calendarDays[x, y];

                if (_calendarDays[x, y].ShadowIsVisible())
                {
                    top = GetTopShadowVariant(x, y);
                    bottom = GetBottomShadowVariant(x, y);

                    //System.Console.WriteLine(x + "," + y + "(" + cDay.DayNumber + "): " + top + " " + bottom);

                    cDay.SetTopShade(top);
                    cDay.SetBottomShade(bottom);
                }

                
            }
        }

        private int GetBottomShadowVariant(int x, int y)
        {
            int variant = 0;

            // check bottom 
            if (y != 5)
            {
                variant += (!_calendarDays[x, y + 1].ShadowIsVisible()) ? 1 : 0;
            }
            else
            {
                variant += 1;
            }

            // check right 
            if (x != 6)
            {
                variant += (!_calendarDays[x + 1, y].ShadowIsVisible()) ? 2 : 0;
            }
            else
            {
                variant += 2;
            }
            
            // check left
            if (x != 0)
            {
                variant += (!_calendarDays[x - 1, y].ShadowIsVisible()) ? 8 : 0;
            }
            else
            {
                variant += 8;
            }

            // roundings for fancyness
            if ((variant == 3 || variant == 11) && x != 6)
            {
                variant += 2;
            }

            return variant;
        }

        private int GetTopShadowVariant(int x, int y)
        {
            int variant = 0;

            // check top 
            if (y != 0)
            {
                variant += (!_calendarDays[x, y - 1].ShadowIsVisible()) ? 1 : 0;
            }
            else
            {
                variant += 1;
            }

            //check left
            if (x != 0)
            {
                variant += (!_calendarDays[x - 1, y].ShadowIsVisible()) ? 2 : 0;
            }
            else
            {
                variant += 2;
            }

            // if top and left shaded, check topleft for corner
            if (x != 0 && variant == 0)
            {
                variant += (!_calendarDays[x - 1, y - 1].ShadowIsVisible()) ? 4 : 0;
            }

            // check right
            if (x != 6)
            {
                variant += (!_calendarDays[x + 1, y].ShadowIsVisible()) ? 8 : 0;
            }
            else
            {
                variant += 8;
            }

            // roundings for fancyness
            if ((variant == 3 || variant == 11) && x != 0)
            {
                variant += 2;
            }

            return variant;
        }

        private void img_Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)

                Application.Current.MainWindow.DragMove();
        }

        private void img_CloseBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            BitmapImage imgS = new BitmapImage(new Uri("../../Images/BtnCloseDown.png", UriKind.Relative));
            img_CloseBtn.Source = imgS;
        }

        private void img_CloseBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            BitmapImage imgS = new BitmapImage(new Uri("../../Images/BtnCloseUp.png", UriKind.Relative));
            img_CloseBtn.Source = imgS;
        }

        private void img_CloseBtn_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            BitmapImage imgS = new BitmapImage(new Uri("../../Images/BtnCloseUp.png", UriKind.Relative));
            img_CloseBtn.Source = imgS;
            Application.Current.Shutdown();
        }

        private void img_MiniBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            BitmapImage imgS = new BitmapImage(new Uri("../../Images/BtnMiniDown.png", UriKind.Relative));
            img_MiniBtn.Source = imgS;
        }

        private void img_MiniBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            BitmapImage imgS = new BitmapImage(new Uri("../../Images/BtnMiniUp.png", UriKind.Relative));
            img_MiniBtn.Source = imgS;
        }

        private void img_MiniBtn_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            BitmapImage imgS = new BitmapImage(new Uri("../../Images/BtnMiniUp.png", UriKind.Relative));
            img_MiniBtn.Source = imgS;
            _dayOverview.Hide();
            this.WindowState = WindowState.Minimized;
        }

        private void img_MonthPrev_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _shownDate = _shownDate.AddMonths(-1);
            UpdateCalendarDates();
            BitmapImage imgS = new BitmapImage(new Uri("../../Images/BtnPrevUp.png", UriKind.Relative));
            img_MonthPrev.Source = imgS;
        }

        private void img_MonthNext_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _shownDate = _shownDate.AddMonths(1);
            UpdateCalendarDates();
            BitmapImage imgS = new BitmapImage(new Uri("../../Images/BtnNextUp.png", UriKind.Relative));
            img_MonthNext.Source = imgS;
        }

        private void img_MonthPrev_MouseDown(object sender, MouseButtonEventArgs e)
        {
            BitmapImage imgS = new BitmapImage(new Uri("../../Images/BtnPrevDown.png", UriKind.Relative));
            img_MonthPrev.Source = imgS;
        }

        private void img_MonthNext_MouseDown(object sender, MouseButtonEventArgs e)
        {
            BitmapImage imgS = new BitmapImage(new Uri("../../Images/BtnNextDown.png", UriKind.Relative));
            img_MonthNext.Source = imgS;
        }

        private void img_MonthNext_MouseLeave(object sender, MouseEventArgs e)
        {
            BitmapImage imgS = new BitmapImage(new Uri("../../Images/BtnNextUp.png", UriKind.Relative));
            img_MonthNext.Source = imgS;
        }

        private void img_MonthPrev_MouseLeave(object sender, MouseEventArgs e)
        {
            BitmapImage imgS = new BitmapImage(new Uri("../../Images/BtnPrevUp.png", UriKind.Relative));
            img_MonthPrev.Source = imgS;
        }

        private void wdw_Main_LocationChanged(object sender, EventArgs e)
        {

            var location = this.PointToScreen(new Point(0, 0));
            MainWindow._dayOverview.Left = location.X + this.Width;
            MainWindow._dayOverview.Top = location.Y + 20;
            MainWindow._dayOverview.Activate();

        }

        private void wdw_Main_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _dayOverview.Hide();
        }

        private void wdw_Main_GotFocus(object sender, RoutedEventArgs e)
        {
        }

        private void wdw_Main_Loaded(object sender, RoutedEventArgs e)
        {
            _dayOverview.Owner = MainWindow.GetWindow(this);
        }



    }
}
