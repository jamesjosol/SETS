using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SETSDeployer.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
            => value is true ? Visibility.Visible : Visibility.Collapsed;
        public object ConvertBack(object value, Type t, object p, CultureInfo c)
            => value is Visibility.Visible;
    }

    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
            => value is true ? Visibility.Collapsed : Visibility.Visible;
        public object ConvertBack(object value, Type t, object p, CultureInfo c)
            => value is Visibility.Collapsed;
    }

    public class StatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
        {
            return (value?.ToString()) switch
            {
                "Success"   => new SolidColorBrush(System.Windows.Media.Color.FromRgb(46, 204, 113)),
                "Failed"    => new SolidColorBrush(System.Windows.Media.Color.FromRgb(239, 68, 68)),
                "Deploying" => new SolidColorBrush(System.Windows.Media.Color.FromRgb(245, 158, 11)),
                _           => new SolidColorBrush(System.Windows.Media.Color.FromRgb(90, 100, 120))
            };
        }
        public object ConvertBack(object value, Type t, object p, CultureInfo c)
            => System.Windows.Data.Binding.DoNothing;
    }

    public class StatusToLabelConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
        {
            return (value?.ToString()) switch
            {
                "Success"   => "✓  Success",
                "Failed"    => "✗  Failed",
                "Deploying" => "Deploying...",
                _           => "Idle"
            };
        }
        public object ConvertBack(object value, Type t, object p, CultureInfo c)
            => System.Windows.Data.Binding.DoNothing;
    }

    public class ConnStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
        {
            return (value?.ToString()) switch
            {
                "Reachable"   => new SolidColorBrush(System.Windows.Media.Color.FromRgb(46, 204, 113)),
                "Unreachable" => new SolidColorBrush(System.Windows.Media.Color.FromRgb(239, 68, 68)),
                _             => new SolidColorBrush(System.Windows.Media.Color.FromRgb(90, 100, 120))
            };
        }
        public object ConvertBack(object value, Type t, object p, CultureInfo c)
            => System.Windows.Data.Binding.DoNothing;
    }

    public class ConnStatusToLabelConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
        {
            return (value?.ToString()) switch
            {
                "Reachable"   => "REACHABLE",
                "Unreachable" => "UNREACHABLE",
                _             => "UNKNOWN"
            };
        }
        public object ConvertBack(object value, Type t, object p, CultureInfo c)
            => System.Windows.Data.Binding.DoNothing;
    }

    public class CheckedToBgConverter : IValueConverter
    {
        private static readonly SolidColorBrush _checked   = new(System.Windows.Media.Color.FromArgb(35, 79, 142, 247));
        private static readonly SolidColorBrush _unchecked = new(System.Windows.Media.Color.FromRgb(30, 37, 53));
        public object Convert(object value, Type t, object p, CultureInfo c)
            => value is true ? _checked : _unchecked;
        public object ConvertBack(object value, Type t, object p, CultureInfo c)
            => System.Windows.Data.Binding.DoNothing;
    }
}
