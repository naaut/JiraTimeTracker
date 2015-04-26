using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TimeTracker.DataModel;

namespace TimeTracker.Command
{
    class ResetTotalSpendTimeClick : ICommand

    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            App.DataModel.ResetTotalSpendTime((JiraTask)parameter);
        }
    }
}
