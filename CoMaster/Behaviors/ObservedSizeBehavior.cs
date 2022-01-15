using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CoMaster.Behaviors
{
    internal class ObservedSizeBehavior
    {
        // Using a DependencyProperty as the backing store for Observe.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ObserveProperty =
            DependencyProperty.RegisterAttached("Observe", typeof(bool), typeof(ObservedSizeBehavior), new PropertyMetadata(false, OnObserveChanged));
        // Using a DependencyProperty as the backing store for GetActualHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GetActualHeightProperty =
            DependencyProperty.RegisterAttached("GetActualHeight", typeof(double), typeof(ObservedSizeBehavior), new PropertyMetadata(0.0));
        // Using a DependencyProperty as the backing store for ActualWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActualWidthProperty =
            DependencyProperty.RegisterAttached("ActualWidth", typeof(double), typeof(ObservedSizeBehavior), new PropertyMetadata(0.0));

        public static bool GetObserve(DependencyObject obj)
        {
            return (bool)obj.GetValue(ObserveProperty);
        }

        public static void SetObserve(DependencyObject obj, bool value)
        {
            obj.SetValue(ObserveProperty, value);
        }

        public static double GetGetActualHeight(DependencyObject obj)
        {
            return (double)obj.GetValue(GetActualHeightProperty);
        }

        public static void SetGetActualHeight(DependencyObject obj, double value)
        {
            obj.SetValue(GetActualHeightProperty, value);
        }

        public static double GetActualWidth(DependencyObject obj)
        {
            return (double)obj.GetValue(ActualWidthProperty);
        }

        public static void SetActualWidth(DependencyObject obj, double value)
        {
            obj.SetValue(ActualWidthProperty, value);
        }

        private static void OnObserveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is FrameworkElement)) throw new InvalidOperationException("Only implemented for FrameworkElements!");
            FrameworkElement fe = (FrameworkElement)d;
            if ((bool)e.NewValue)
            {
                fe.SizeChanged += OnSizeChanged;
                UpdateValues(d);
            }
            else
            {
                fe.SizeChanged -= OnSizeChanged;
            }
        }

        private static void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateValues(sender as DependencyObject);
        }

        private static void UpdateValues(DependencyObject d)
        {
            FrameworkElement fe = (FrameworkElement)d;
            SetActualWidth(d, fe.ActualHeight);
            SetActualWidth(d, fe.ActualWidth);
        }
    }
}
