using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TimeTracker.Command;
using Windows.UI.Xaml;

namespace TimeTracker.DataModel
{
    public class JiraTask : INotifyPropertyChanged
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public int CurrentTimer { get; set; }
        //public DateTime CurrentTimeSpan { get; set; }
        public TimeSpan CurrentTimeSpan { get; set; }
        public TimeSpan TotalSpentTime { get; set; }
        public ObservableCollection<JiraTaskTime> SpentTimeCollection { get; set; }

        [IgnoreDataMember]
        public ICommand TimerButtonCommand { get; set; }
        [IgnoreDataMember]
        public ICommand ResetTimerButtonCommand { get; set; }
        [IgnoreDataMember]
        public ICommand ResetTotalSpentTimeButtonCommand { get; set; }
        [IgnoreDataMember]
        public DispatcherTimer Timer { get; set; }

        public JiraTask()
        {
            TimerButtonCommand = new TimerButtonClick();
            ResetTimerButtonCommand = new ResetTimerButtonClick();
            ResetTotalSpentTimeButtonCommand = new ResetTotalSpendTimeClick();
        }

        internal void Timer_Tick(object sender, object e)
        {
            this.CurrentTimer++;
            this.CurrentTimeSpan = this.CurrentTimeSpan.Add(TimeSpan.FromSeconds(1));

            NotifyPropertyChanged("CurrentTimer");
            NotifyPropertyChanged("CurrentTimeSpan");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }



    }
}
