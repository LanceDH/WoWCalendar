﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace Calendar
{
    public class CalendarDay
    {
        private static DropShadowEffect textShadow = new DropShadowEffect();
        private static FontFamily defaultFont = new FontFamily("../../Fonts/#Friz Quadrata TT");
        private static int SHADOW_VISIBLE = 1;
            
        private static Random rng = new Random();
        private int dayNumber;
        private Grid grid;
        private Label dayLabel;
        private Label temperature;
        private Label eventLabel;
        private Image shadeOverlayTop;
        private Image shadeOverlayBottom;
        private Image EventOverlay;
        private Image Highlight;
        private DateTime date;
        private MainWindow parent;
        private List<BLL.CalendarEvent> events;
        private Boolean isSelected = false;

        public CalendarDay(MainWindow parent, int day)
        {
            this.dayNumber = day;
            this.parent = parent;
            events = new List<BLL.CalendarEvent>();

            textShadow.Color = Color.FromRgb(0, 0, 0);
            textShadow.ShadowDepth = 2;

            grid = new Grid();
            grid.Width = 90;
            grid.Height = 90;
            grid.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            grid.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            ImageBrush br = new ImageBrush();
            br.ImageSource = new BitmapImage(new Uri("../../Images/DayBG" + rng.Next(1, 5) + ".png", UriKind.Relative));
            grid.Background = br;

            grid.MouseDown += new System.Windows.Input.MouseButtonEventHandler(DayClicked);
            grid.MouseEnter += new System.Windows.Input.MouseEventHandler(MouseEnter);
            grid.MouseLeave += new System.Windows.Input.MouseEventHandler(MouseLeave);

            EventOverlay = new Image();
            EventOverlay.Width = 90;
            EventOverlay.Height = 90;
            EventOverlay.Source = new BitmapImage(new Uri("../../Images/DayEventOverlay.png", UriKind.Relative));
            EventOverlay.Opacity = 0;
            grid.Children.Add(EventOverlay);

            

            this.temperature = new Label();
            this.temperature.FontFamily = defaultFont;
            //this.temperature.FontWeight = FontWeights.Bold;
            this.temperature.Foreground = Brushes.AliceBlue;
            this.temperature.Effect = textShadow;
            this.temperature.HorizontalAlignment = HorizontalAlignment.Right;
            this.temperature.Margin = new System.Windows.Thickness(0, 30, 7, 0);
            grid.Children.Add(this.temperature);

            this.eventLabel = new Label();
            this.eventLabel.FontFamily = defaultFont;
            //this.eventLabel.FontWeight = FontWeights.Bold;
            this.eventLabel.Foreground = Brushes.AliceBlue;
            this.eventLabel.Effect = textShadow;
            this.eventLabel.VerticalAlignment = VerticalAlignment.Bottom;
            this.eventLabel.HorizontalAlignment = HorizontalAlignment.Left;
            this.eventLabel.Margin = new System.Windows.Thickness(7, 0, 0, 7);
            grid.Children.Add(this.eventLabel);

            Highlight = new Image();
            Highlight.Width = 90;
            Highlight.Height = 90;
            Highlight.Source = new BitmapImage(new Uri("../../Images/DayHighlight.png", UriKind.Relative));
            Highlight.Opacity = 0;
            grid.Children.Add(Highlight);

            dayLabel = new Label();
            dayLabel.Content = dayNumber;
            dayLabel.FontWeight = FontWeights.Bold;
            dayLabel.Foreground = Brushes.Moccasin;
            dayLabel.FontFamily = defaultFont;
            dayLabel.Effect = textShadow;
            dayLabel.Margin = new System.Windows.Thickness(7, 7, 0, 0);
            grid.Children.Add(dayLabel);

            shadeOverlayTop = new Image();
            shadeOverlayTop.Width = 90;
            shadeOverlayTop.Height = 45;
            shadeOverlayTop.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            shadeOverlayTop.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            shadeOverlayTop.Source = new BitmapImage(new Uri("../../Images/ShadeTop0.png", UriKind.Relative));
            grid.Children.Add(shadeOverlayTop);
            shadeOverlayTop.Opacity = 0;

            shadeOverlayBottom = new Image();
            shadeOverlayBottom.Width = 90;
            shadeOverlayBottom.Height = 45;
            shadeOverlayBottom.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            shadeOverlayBottom.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            shadeOverlayBottom.Source = new BitmapImage(new Uri("../../Images/ShadeTop0.png", UriKind.Relative));
            grid.Children.Add(shadeOverlayBottom);
            shadeOverlayBottom.Opacity = 0;

        }

        public void Reset()
        {
            DayNumber = 0;
            HideShadow();
            SetTemperature(-1);
            Events.Clear();
            EventOverlay.Opacity = 0;
        }

        public void SetTemperature(int temp)
        {
            if (temp == -1) 
            {
                temperature.Content = "";
            }
            else
            {
                temperature.Content = temp + "°C";
            }
            
        }

        private void MouseEnter(object sender, MouseEventArgs e)
        {
            Highlight.Opacity = (isSelected) ? 1 : SHADOW_VISIBLE;
            if (events.Count == 0) { return; }

            var location = grid.PointToScreen(new Point(0, 0));
            MainWindow._tooltip.SetDay(this);
            MainWindow._tooltip.Left = location.X + 90;
            MainWindow._tooltip.Top = location.Y - MainWindow._tooltip.Height;
            MainWindow._tooltip.Show();
            parent.Focus();
        }

        private void MouseLeave(object sender, MouseEventArgs e)
        {
            Highlight.Opacity = (isSelected) ? 1 : 0;
            MainWindow._tooltip.Hide();
        }

        private void DayClicked(object sender, MouseButtonEventArgs e)
        {
            MainWindow._dayOverview.ShowDay(this);
            MainWindow._dayOverview.Show();
            parent.ResetDayHighlights();
            IsSelected = true;
        }

        public DateTime Date
        {
            get { return date; }
            set
            {
                date = new DateTime(value.Year, value.Month, value.Day);
            }
        }

        public List<BLL.CalendarEvent> Events
        {
            get { return events; }
            set { 
                events = value;
                if (events.Count == 0)
                {
                    EventOverlay.Opacity = 0;
                    eventLabel.Content = "";
                }
                else
                {
                    events = events.OrderBy(e => e.CalendarDate).ToList();
                    EventOverlay.Opacity = 1;
                    BLL.CalendarEvent calEvent = (BLL.CalendarEvent)events.ElementAt(0);
                    eventLabel.Content = calEvent.CalendarDate.ToString("HH:mm");
                }
            }
        }

        public int DayNumber
        {
            get { return dayNumber; }
            set { 
                dayNumber = value;
                dayLabel.Content = dayNumber;
            }
        }

        public Boolean IsSelected
        {
            get { return isSelected; }
            set { 
                isSelected = value;
                if (isSelected)
                {
                    Highlight.Opacity = 1;
                }
                else { Highlight.Opacity = 0; }
            }
        }

        public Grid GetGrid()
        {
            return grid;
        }

        public void ShowShadow()
        {
            shadeOverlayTop.Opacity = SHADOW_VISIBLE;
            shadeOverlayBottom.Opacity = SHADOW_VISIBLE;
        }

        public void HideShadow()
        {
            shadeOverlayTop.Opacity = 0;
            shadeOverlayBottom.Opacity = 0;
        }

        public Boolean ShadowIsVisible()
        {
            return shadeOverlayTop.Opacity == SHADOW_VISIBLE;
        }

        public void SetTopShade(int variant)
        {
            shadeOverlayTop.Source = new BitmapImage(new Uri("../../Images/ShadeTop" + variant + ".png", UriKind.Relative));
        }

        public void SetBottomShade(int variant)
        {
            shadeOverlayBottom.Source = new BitmapImage(new Uri("../../Images/ShadeBottom" + variant + ".png", UriKind.Relative));
        }
        
    }
}
