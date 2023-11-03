using System.Windows ;
using System.Windows.Controls ;
using System.Windows.Input ;

namespace HcBimUtils.WPFUtils.Behaviors
{
   public static class NumberOnlyBehavior
   {
      public static readonly DependencyProperty IsEnabledProperty =
                  DependencyProperty.RegisterAttached("IsEnabled", typeof(bool),
                  typeof(NumberOnlyBehavior), new UIPropertyMetadata(false, OnValueChanged));

      public static bool GetIsEnabled(Control o)
      {
         return (bool)o.GetValue(IsEnabledProperty);
      }

      public static void SetIsEnabled(Control o, bool value)
      {
         o.SetValue(IsEnabledProperty, value);
      }

      private static void OnValueChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
      {
         var uiElement = dependencyObject as Control;
         if (uiElement == null) return;
         if (e.NewValue is bool && (bool)e.NewValue)
         {
            uiElement.PreviewTextInput += OnTextInput;
            uiElement.PreviewKeyDown += OnPreviewKeyDown;

            DataObject.AddPastingHandler(uiElement, OnPaste);
         }
         else
         {
            uiElement.PreviewTextInput -= OnTextInput;
            uiElement.PreviewKeyDown -= OnPreviewKeyDown;

            DataObject.RemovePastingHandler(uiElement, OnPaste);
         }
      }

      private static void OnTextInput(object sender, TextCompositionEventArgs e)
      {
         //if (e.Text.Any(c => !char.IsDigit(c))) { e.Handled = true; }
         if (!char.IsNumber(e.Text, e.Text.Length - 1))
         {
            e.Handled = true;
         }

         if (((e.Text).ToCharArray()[e.Text.Length - 1] == '.') || ((e.Text).ToCharArray()[e.Text.Length - 1] == ','))
         {
            e.Handled = true;
            if (!(((TextBox)sender).Text.Contains('.')))
            {
               if (((TextBox)sender).Text.Length == 0) { ((TextBox)sender).Text = "0."; ((TextBox)sender).CaretIndex = ((TextBox)sender).Text.Length; }
               else { ((TextBox)sender).Text += "."; ((TextBox)sender).CaretIndex = ((TextBox)sender).Text.Length; }
            }
         }
         if ((e.Text).ToCharArray()[e.Text.Length - 1] == '-' & !((TextBox)sender).Text.Contains('-')) { e.Handled = true; ((TextBox)sender).Text = "-" + ((TextBox)sender).Text; ((TextBox)sender).CaretIndex = ((TextBox)sender).Text.Length; }
         if ((e.Text).ToCharArray()[e.Text.Length - 1] == '+' & ((TextBox)sender).Text.Contains('-')) { e.Handled = true; ((TextBox)sender).Text = ((TextBox)sender).Text.Substring(1); ((TextBox)sender).CaretIndex = ((TextBox)sender).Text.Length; }
      }

      private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
      {
         if (e.Key == Key.Space) e.Handled = true;
      }

      private static void textBox1_PreviewTextInput(object sender, TextCompositionEventArgs e)
      {
         if (!char.IsNumber(e.Text, e.Text.Length - 1))
         {
            e.Handled = true;
         }

         if (((e.Text).ToCharArray()[e.Text.Length - 1] == '.') || ((e.Text).ToCharArray()[e.Text.Length - 1] == ','))
         {
            e.Handled = true;
            if (!(((TextBox)sender).Text.Contains('.')))
            {
               if (((TextBox)sender).Text.Length == 0) { ((TextBox)sender).Text = "0."; ((TextBox)sender).CaretIndex = ((TextBox)sender).Text.Length; }
               else { ((TextBox)sender).Text += "."; ((TextBox)sender).CaretIndex = ((TextBox)sender).Text.Length; }
            }
         }
         if ((e.Text).ToCharArray()[e.Text.Length - 1] == '-' & !((TextBox)sender).Text.Contains('-')) { e.Handled = true; ((TextBox)sender).Text = "-" + ((TextBox)sender).Text; ((TextBox)sender).CaretIndex = ((TextBox)sender).Text.Length; }
         if ((e.Text).ToCharArray()[e.Text.Length - 1] == '+' & ((TextBox)sender).Text.Contains('-')) { e.Handled = true; ((TextBox)sender).Text = ((TextBox)sender).Text.Substring(1); ((TextBox)sender).CaretIndex = ((TextBox)sender).Text.Length; }
      }

      private static void OnPaste(object sender, DataObjectPastingEventArgs e)
      {
         if (e.DataObject.GetDataPresent(DataFormats.Text))
         {
            var text = Convert.ToString(e.DataObject.GetData(DataFormats.Text)).Trim();
            if (text.Any(c => !char.IsDigit(c))) { e.CancelCommand(); }
         }
         else
         {
            e.CancelCommand();
         }
      }
   }
}