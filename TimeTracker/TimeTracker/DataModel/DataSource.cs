using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Windows.Storage;
using Windows.UI.Xaml;
using System.Windows.Input;
using System.Runtime.Serialization;
using TimeTracker.Command;
using System.ComponentModel;

namespace TimeTracker.DataModel
{
    public class JiraTask : INotifyPropertyChanged
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public int CurrentTimer { get; set; }        
        public string TotalSpendTime { get; set; }
        
        public ObservableCollection<JiraTaskTime> SpendTime { get; set; }

        [IgnoreDataMember]
        public ICommand TimerButtonCommand { get; set; }
        [IgnoreDataMember]
        public DispatcherTimer Timer { get; set; }

        public JiraTask()
        {
            TotalSpendTime = "00:00:00";
            TimerButtonCommand = new TimerButtonClick();     
        }

        internal void Timer_Tick(object sender, object e)
        {
            //throw new NotImplementedException();
            this.CurrentTimer++;
            NotifyPropertyChanged("CurrentTimer");            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }



    }

    //When and How long tracked
    public class JiraTaskTime
    {
        public DateTime DateWhen { get; set; }
        public TimeSpan TimeHowLong { get; set; }

        //public JiraTaskTime()
        //{
        //    DateTime DateWhen = DateTime.Now;
        //    TimeSpan TimeHowLong = new TimeSpan();
        //}
    }

    public class DataSource
    {
        private ObservableCollection<JiraTask> _jiraTask;
        const string fileName = "taskData.json";

        public DataSource()
        {
            _jiraTask = new ObservableCollection<JiraTask>();
        }

        public async Task<ObservableCollection<JiraTask>> GetJiraTask()
        {
            await ensureDataLoaded();
            return _jiraTask;
        }

        private async Task ensureDataLoaded()
        {
            if (_jiraTask.Count == 0)
            {
                await getJiraTaskDataAsync();                
            }
            return;
        }

        //Read Data From Local Storage
        private async Task getJiraTaskDataAsync()
        {
            if (_jiraTask.Count != 0)
            {
                return;
            }
            var jsonSerializer = new DataContractJsonSerializer(typeof(ObservableCollection<JiraTask>));

            try
            {
                using (var stream = await ApplicationData.Current.LocalFolder.OpenStreamForReadAsync(fileName)) 
                {
                    _jiraTask = (ObservableCollection<JiraTask>)jsonSerializer.ReadObject(stream);                   
                }
            }
            catch (Exception)
            {
                _jiraTask = new ObservableCollection<JiraTask>();                
            }
        }
        //Save Data To Local Storage
        private async Task saveJiraTaskDataAsync()
        {
            //calcTotalSpendTime();
            var jsonSerializer = new DataContractJsonSerializer(typeof(ObservableCollection<JiraTask>));
            using (var stream = await ApplicationData.Current.LocalFolder.OpenStreamForWriteAsync(fileName, CreationCollisionOption.ReplaceExisting))
            {
                jsonSerializer.WriteObject(stream, _jiraTask);
            }

        }
        //Add New Task To Collection
        public async void AddJiraTask(JiraTask jiraTask)
        {
            _jiraTask.Add(jiraTask);
            await saveJiraTaskDataAsync();
        }
        //Change Task
        public async void ChangeJiraTask(JiraTask jiraTask)
        {
            var tmp = _jiraTask.FirstOrDefault(p => p.ID == jiraTask.ID);

            tmp.ID = jiraTask.ID;
            tmp.Name = jiraTask.Name;
            tmp.Note = jiraTask.Note;
            
            await saveJiraTaskDataAsync();
        }
        // Calculate TotalSpendTime
        private void calcTotalSpendTime()
        {            
            foreach(var jTask in _jiraTask)
	        {
                TimeSpan timeSpan = new TimeSpan();
                foreach (var item in jTask.SpendTime)
	            {
                    timeSpan = timeSpan.Add(item.TimeHowLong);                  		 
	            }
                jTask.TotalSpendTime = String.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
	        }	
        }

        public void StartNewTimer(JiraTask jiraTask)
        {

            jiraTask.Timer = new DispatcherTimer();
            jiraTask.Timer.Interval = new TimeSpan(0, 0, 1);
            jiraTask.Timer.Tick += new EventHandler<object>(jiraTask.Timer_Tick);
            if (jiraTask.Timer.IsEnabled)
                jiraTask.Timer.Start();            
            else jiraTask.Timer.Start();
        }



    }

}
