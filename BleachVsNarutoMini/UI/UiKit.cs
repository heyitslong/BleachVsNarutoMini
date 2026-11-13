using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BleachVsNarutoMini.UI
{
    //  TIỆN ÍCH GIAO DIỆN (style nút bấm, nhãn, radio)
    internal static class UiKit
    {
        private const string ButtonStyleXaml = @"
<Style xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
       xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
       TargetType='Button'>
  <Setter Property='Foreground' Value='White'/>
  <Setter Property='Background' Value='#3A3A55'/>
  <Setter Property='FontSize' Value='18'/>
  <Setter Property='FontWeight' Value='SemiBold'/>
  <Setter Property='Cursor' Value='Hand'/>
  <Setter Property='Template'>
    <Setter.Value>
      <ControlTemplate TargetType='Button'>
        <Border x:Name='bd' Background='{TemplateBinding Background}'
                BorderBrush='Gold' BorderThickness='1' CornerRadius='8'>
          <ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/>
        </Border>
        <ControlTemplate.Triggers>
          <Trigger Property='IsMouseOver' Value='True'>
            <Setter TargetName='bd' Property='Background' Value='#5A5A85'/>
          </Trigger>
          <Trigger Property='IsPressed' Value='True'>
            <Setter TargetName='bd' Property='Background' Value='#2A2A40'/>
          </Trigger>
        </ControlTemplate.Triggers>
      </ControlTemplate>
    </Setter.Value>
  </Setter>
</Style>";

        private static Style _buttonStyle;

        public static Style ButtonStyle
        {
            get
            {
                if (_buttonStyle == null)
                    _buttonStyle = (Style)XamlReader.Parse(ButtonStyleXaml);
                return _buttonStyle;
            }
        }

        public static Button MakeButton(string text, RoutedEventHandler onClick, Thickness margin)
        {
            var b = new Button
            {
                Content = text,
                Height = 45,
                Margin = margin,
                Style = ButtonStyle
            };
            b.Click += onClick;
            return b;
        }

        public static TextBlock MakeLabel(string text)
        {
            return new TextBlock
            {
                Text = text,
                Foreground = Brushes.White,
                FontSize = 16,
                Margin = new Thickness(0, 0, 0, 8)
            };
        }

        public static RadioButton MakeRadio(string text, string group, bool isChecked, Thickness margin)
        {
            return new RadioButton
            {
                Content = text,
                GroupName = group,
                IsChecked = isChecked,
                Foreground = Brushes.White,
                FontSize = 15,
                Margin = margin
            };
        }

        public static Brush DimBackground()
        {
            return new SolidColorBrush(Color.FromArgb(0xCC, 0, 0, 0));
        }
    }
}
