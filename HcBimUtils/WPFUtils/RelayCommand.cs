using System.Windows.Input ;

namespace HcBimUtils.WPFUtils
{
   public class RelayCommand<T> : ICommand
   {
      private readonly Predicate<T> _canExecute;
      private readonly Action<T> _execute;

      public RelayCommand(Predicate<T> canExecute, Action<T> execute)
      {
         _canExecute = canExecute;
         _execute = execute ?? throw new ArgumentNullException("execute");
      }

      public bool CanExecute(object parameter)
      {
         return _canExecute == null ? true : _canExecute((T)parameter);
      }

      public void Execute(object parameter)
      {
         _execute((T)parameter);
      }

      public event EventHandler CanExecuteChanged
      {
         add { CommandManager.RequerySuggested += value; }
         remove { CommandManager.RequerySuggested -= value; }
      }

      public static implicit operator RelayCommand<T>(RelayCommand v)
      {
         throw new NotImplementedException();
      }
   }

   public class RelayCommand : ICommand
   {
      #region Fields

      private readonly Action<object> _execute;
      private readonly Predicate<object> _canExecute;

      #endregion Fields

      #region Constructors

      public RelayCommand(Action<object> execute)
          : this(execute, null)
      {
      }

      public RelayCommand(Action<object> execute, Predicate<object> canExecute)
      {
         _execute = execute ?? throw new ArgumentNullException("execute");
         _canExecute = canExecute;
      }

      #endregion Constructors

      #region ICommand Members

      public bool CanExecute(object parameter)
      {
         return _canExecute == null ? true : _canExecute(parameter);
      }

      public event EventHandler CanExecuteChanged
      {
         add { CommandManager.RequerySuggested += value; }
         remove { CommandManager.RequerySuggested -= value; }
      }

      public void Execute(object parameter)
      {
         _execute(parameter);
      }

      #endregion ICommand Members
   }
}