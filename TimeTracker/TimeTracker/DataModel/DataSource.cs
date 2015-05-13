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
using System.Net;
using System.Diagnostics;


namespace TimeTracker.DataModel
{


    //When and How long tracked


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
            calcTotalSpentTime();
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
                ResetTimer(jiraTask);
            }
            
        }
        
        // Calculate TotalSpendTime

        private void calcTotalSpentTime()
        {            
            foreach(var jTask in _jiraTask)
	        {
                TimeSpan timeSpan = new TimeSpan();
                if (jTask.SpentTimeCollection != null)
                {
                    foreach (var item in jTask.SpentTimeCollection)
                    {
                        if (item.TimeHowLong != null)
                            timeSpan = timeSpan.Add(item.TimeHowLong);
                        
                    }                    
                }

                jTask.TotalSpentTime = timeSpan;
                jTask.NotifyPropertyChanged("TotalSpentTime");
	        }
            
        }

 
        //Save 
        private async Task saveCurrentTaskState(JiraTask jiraTask)
        {
            if (jiraTask.CurrentTimer != 0)
            {
                if (jiraTask.SpentTimeCollection == null)
                    jiraTask.SpentTimeCollection = new ObservableCollection<JiraTaskTime>();

                JiraTaskTime currentTimer = new JiraTaskTime();
                currentTimer.DateWhen = DateTime.Now;
                currentTimer.TimeHowLong = new TimeSpan();
                currentTimer.TimeHowLong = currentTimer.TimeHowLong.Add(TimeSpan.FromSeconds(jiraTask.CurrentTimer));
                
                jiraTask.SpentTimeCollection.Add(currentTimer);
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

        public async void ResetTotalSpentTime(JiraTask jiraTask)
        {
            ResetTimer(jiraTask);
            jiraTask.TotalSpentTime = TimeSpan.FromSeconds(0);
            jiraTask.SpentTimeCollection = new ObservableCollection<JiraTaskTime>();
            jiraTask.NotifyPropertyChanged("TotalSpentTime");            
            await saveCurrentTaskState(jiraTask);
        }

        

        
    }



}
