using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;


namespace TimeTracker.DataModel
{
    class Jira
    {
        public string sessionName { get; set; }
        public string sessionValue { get; set; }

        public async Task<String> LoginToJira(JiraUser jiraUser)
        {
            // server to POST to
            string url = jiraUser.ServerName + "rest/auth/1/session/";

            // HTTP web request
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);      

            
            string jsonRequest = "{ \"username\" : \"" + jiraUser.UserName + "\", \"password\" : \"" + jiraUser.Password + "\" }";

            try
            {
                JObject jsonResponse = JObject.Parse(await GetHttpPostResponse(httpWebRequest, jsonRequest));
                try
                {
                    this.sessionName = (string)jsonResponse["session"]["name"];
                    this.sessionValue = (string)jsonResponse["session"]["value"];
                    //return String.Format("name: {0},\n value: {1}", this.sessionName, this.sessionValue);
                    return "Connection Successful.";
                }
                catch (Exception)
                {
                    return "Login failed!";
                }
            }
            catch (Exception)
            {

                return "Can't connect to Jira server!";
            }
                       
        }
        
        internal static string ConverCredential(JiraUser jiraUser)
        {
            string mergedCredentials = string.Format("{0}:{1}", jiraUser.UserName, jiraUser.Password);
            byte[] byteCredentials = UTF8Encoding.UTF8.GetBytes(mergedCredentials);
            string cred = Convert.ToBase64String(byteCredentials);
            return cred;
        }




        internal static async Task<String> CreateCommitRequest(JiraUser jiraUser, string taskID, string timeSpentSeconds, string started, string comment)        
        {
            // server to POST to
            string url = jiraUser.ServerName + "rest/api/2/issue/" + taskID + "/worklog";
            string cred = Jira.ConverCredential(jiraUser); 

            // HTTP web request
            HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            // Add Header for authorization
            httpWebRequest.Headers[HttpRequestHeader.Authorization] = string.Format("Basic {0}", cred);
            



            // Json format for add worklog spent time
            //{
            //"timeSpent": "1h 30m",
            //"started": "2013-09-01T10:30:18.932+0530",
            //"comment": "logging via TimeTracker"
            //}

            string jsonRequest = "{ \"timeSpentSeconds\" : \"" + timeSpentSeconds + "\", \"started\" : \"" + started + "\", \"comment\" : \"" + comment + "\" }";

            string answer = await GetHttpPostResponse(httpWebRequest, jsonRequest);

            return answer;
        }


        public async Task<String> CommitToJira(JiraUser jiraUser)
        {
            var jiraTasks = await App.DataModel.GetJiraTask();

            if (jiraTasks.Count > 0)
            {
                string answer = "";

                foreach (JiraTask jiraTask in jiraTasks)
                {
                    //string timeSpent = String.Format("{0}h {1}m", jiraTask.TotalSpentTime.Hours, jiraTask.TotalSpentTime.Minutes);
                    string timeSpentSeconds = String.Format("{0}", jiraTask.TotalSpentTime.TotalSeconds);

                    string started = String.Format("{0:yyyy-MM-ddTH:mm:ss.000zz00}", DateTime.Now);

                    answer = await Jira.CreateCommitRequest(jiraUser, jiraTask.ID, timeSpentSeconds, started, jiraTask.Note);

                    

                    //foreach (var item in jiraTask.SpentTimeCollection)
                    //{

                    //}

                }

                return answer;

            }
            else
            {
                return "Tasks list is empty";
            }
                


            //return null;

        }




        internal static async Task<String> GetHttpPostResponse(HttpWebRequest request, string postData)
        {
            String received = null; 

            request.Method = "POST";
            request.ContentType = "application/json";
            
            byte[] requestBody = Encoding.UTF8.GetBytes(postData);

            // ASYNC: using awaitable wrapper to get request stream
            using (var postStream = await request.GetRequestStreamAsync())
            {
                // Write to the request stream.
                // ASYNC: writing to the POST stream can be slow
                
                await postStream.WriteAsync(requestBody, 0, requestBody.Length);

            }

            try
            {
                // ASYNC: using awaitable wrapper to get response
                var response = (HttpWebResponse)await request.GetResponseAsync();
                if (response != null)
                {
                    var reader = new StreamReader(response.GetResponseStream());
                    // ASYNC: using StreamReader's async method to read to end, in case
                    // the stream i slarge.
                    received = await reader.ReadToEndAsync();
                    

                }
            }
            catch (WebException we)
            {
                var reader = new StreamReader(we.Response.GetResponseStream());
                string responseString = reader.ReadToEnd();
                Debug.WriteLine(responseString);
                return responseString;
            }

            return received;
        }

    
    }

   
}
