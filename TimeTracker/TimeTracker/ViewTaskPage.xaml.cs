﻿using TimeTracker.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using TimeTracker.DataModel;
using Windows.UI.Popups;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace TimeTracker
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ViewTaskPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private JiraTask jiraTask;

        public ViewTaskPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);

            jiraTask = (JiraTask)e.Parameter;
            this.defaultViewModel["Task"] = jiraTask;

            taskNameHeadTextBox.Text = jiraTask.Name;
            idTextBox.Text = jiraTask.ID;
            nameTextBox.Text = jiraTask.Name;
            noteTextBox.Text = jiraTask.Note;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));           
        }

        private async void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            var messageDialog = new MessageDialog("Are You Sure?");
            messageDialog.Commands.Add(new UICommand("Delete", new UICommandInvokedHandler(this.CommandIvokedHandler)));
            messageDialog.Commands.Add(new UICommand("Cancel", new UICommandInvokedHandler(this.CommandIvokedHandler)));
            messageDialog.DefaultCommandIndex = 0;
            messageDialog.CancelCommandIndex = 1;
            await messageDialog.ShowAsync();           
        }

        private void CommandIvokedHandler(IUICommand command)
        {
            if (command.Label == "Delete")
            {
                App.DataModel.DeleteJiraTask(jiraTask);
                Frame.Navigate(typeof(MainPage));
            }
            else if (command.Label == "Cancel")
            {
                //
            }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            jiraTask.ID = idTextBox.Text;
            jiraTask.Name = nameTextBox.Text;
            jiraTask.Note = noteTextBox.Text;
           
            App.DataModel.ChangeJiraTask(jiraTask);

            Frame.Navigate(typeof(MainPage));
        }

        private void nameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            taskNameHeadTextBox.Text = nameTextBox.Text;
        }

        private async void resetButton_Click(object sender, RoutedEventArgs e)
        {
            var messageDialog = new MessageDialog("Are You Sure?");
            messageDialog.Commands.Add(new UICommand("Delete", new UICommandInvokedHandler(this.ResetCommandIvokedHandler)));
            messageDialog.Commands.Add(new UICommand("Cancel", new UICommandInvokedHandler(this.ResetCommandIvokedHandler)));
            messageDialog.DefaultCommandIndex = 0;
            messageDialog.CancelCommandIndex = 1;
            await messageDialog.ShowAsync(); 

        }

        private void ResetCommandIvokedHandler(IUICommand command)
        {
            if (command.Label == "Delete")
            {
                App.DataModel.ResetTotalSpentTime(jiraTask);
               
            }
            else if (command.Label == "Cancel")
            {
                //
            }
        }
    }
}
