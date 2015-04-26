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
        //public DateTime CurrentTimeSpan { get; set; }
        public TimeSpan CurrentTimeSpan { get; set; }
        public TimeSpan TotalSpendTime { get; set; }        
        public ObservableCollection<JiraTaskTime> SpendTimeCollection { get; set; }

        [IgnoreDataMember]
        public ICommand TimerButtonCommand { get; set; }
        [IgnoreDataMember]
        public ICommand ResetTimerButtonCommand { get; set; }
        [IgnoreDataMember]
        public ICommand ResetTotalSpendTimeButtonCommand { get; set; }      
        [IgnoreDataMember]
        public DispatcherTimer Timer { get; set; }

        public JiraTask()
        {

            TimerButtonCommand = new TimerButtonClick();
            ResetTimerButtonCommand = new ResetTimerButtonClick();
            ResetTotalSpendTimeButtonCommand = new ResetTotalSpendTimeClick();
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
            catch 
            {
                _jiraTask = new ObservableCollection<JiraTask>();                
            }
        }
        //Save Data To Local Storage
        private async Task saveJiraTaskDataAsync()
        {
            calcTotalSpendTime();
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

        //Get One Task
        //public async Task<JiraTask> GetJiraTaskAsync(JiraTask jiraTask)
        //{
        //    await getJiraTaskDataAsync();
        //    var matches = _jiraTask.FirstOrDefault(p => p.ID == jiraTask.ID);

        //    return matches;

        //}



        //Change Task
        public async void ChangeJiraTask(JiraTask jiraTask)
        {
            var jTask = _jiraTask.FirstOrDefault(p => p.ID == jiraTask.ID);

            jTask.ID = jiraTask.ID;
            jTask.Name = jiraTask.Name;
            jTask.Note = jiraTask.Note;

            jiraTask.NotifyPropertyChanged("ID");
            jiraTask.NotifyPropertyChanged("Name");
            jiraTask.NotifyPropertyChanged("Note");
            
            await saveJiraTaskDataAsync();
        }

        //Delete Task
        public async void DeleteJiraTask(JiraTask jiraTask)
        {
            
            _jiraTask.Remove(jiraTask);
            await saveJiraTaskDataAsync();
        }


        //for suspend and shutdown case
        public async Task SaveJiraTask()
        {
            foreach (var jiraTask in _jiraTask)
            {
                await saveCurrentTaskState(jiraTask);
            }
            
        }
        
        // Calculate TotalSpendTime

        private void calcTotalSpendTime()
        {            
            foreach(var jTask in _jiraTask)
	        {
                TimeSpan timeSpan = new TimeSpan();
                if (jTask.SpendTimeCollection != null)
                {
                    foreach (var item in jTask.SpendTimeCollection)
                    {
                        if (item.TimeHowLong != null)
                            timeSpan = timeSpan.Add(item.TimeHowLong);
                        
                    }                    
                }

                jTask.TotalSpendTime = timeSpan;
                jTask.NotifyPropertyChanged("TotalSpendTime");
	        }
            
        }

 
        //Save 
        private async Task saveCurrentTaskState(JiraTask jiraTask)
        {
            if (jiraTask.CurrentTimer != 0)
            {
                if (jiraTask.SpendTimeCollection == null)
                    jiraTask.SpendTimeCollection = new ObservableCollection<JiraTaskTime>();

                JiraTaskTime currentTimer = new JiraTaskTime();
                currentTimer.DateWhen = DateTime.Now;
                currentTimer.TimeHowLong = new TimeSpan();
                currentTimer.TimeHowLong = currentTimer.TimeHowLong.Add(TimeSpan.FromSeconds(jiraTask.CurrentTimer));                

                jiraTask.SpendTimeCollection.Add(currentTimer);
                await saveJiraTaskDataAsync();
                
            }

        }


        //start stop timer
        public async void StartNewTimer(JiraTask jiraTask)
        {
            if (jiraTask.Timer == null)
            {
                jiraTask.Timer = new DispatcherTimer();
                jiraTask.Timer.Interval = new TimeSpan(0, 0, 1);
                jiraTask.Timer.Tick += new EventHandler<object>(jiraTask.Timer_Tick);
            }

            if (jiraTask.Timer.IsEnabled)
            {
                jiraTask.Timer.Stop();
                await saveCurrentTaskState(jiraTask);
                jiraTask.CurrentTimer = 0;
            }
            else
            {                
                jiraTask.Timer.Start();
            }
        }

        //reset current without save timer
        public void ResetTimer(JiraTask jiraTask)
        {
            jiraTask.CurrentTimeSpan = TimeSpan.FromSeconds(0);
            jiraTask.CurrentTimer = 0;
            jiraTask.NotifyPropertyChanged("CurrentTimeSpan");
            
        }

        public async void ResetTotalSpendTime(JiraTask jiraTask)
        {
            ResetTimer(jiraTask);
            jiraTask.TotalSpendTime = TimeSpan.FromSeconds(0);
            jiraTask.SpendTimeCollection = new ObservableCollection<JiraTaskTime>();
            jiraTask.NotifyPropertyChanged("TotalSpendTime");            
            await saveCurrentTaskState(jiraTask);
        }

        
    }

}
