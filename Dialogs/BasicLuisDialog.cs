using System;
using System.IO;
using System.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using RestSharp;
using Newtonsoft.Json;
using AdaptiveCards;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Sample.LuisBot.Dialogs
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    [Serializable]
    public class BasicLuisDialog : LuisDialog<object>
    {
        public BasicLuisDialog() : base(new LuisService(new LuisModelAttribute(
            ConfigurationManager.AppSettings["LuisAppId"],
            ConfigurationManager.AppSettings["LuisAPIKey"],
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        {
        }

        public QnAMakerService hrKB = new QnAMakerService(
            "https://YOURQNAAPPID.azurewebsites.net",
            "your qna kb id", // example: /knowledgebases/righthereisKB-ID/generateAnswer
            "your qna Endpointkey" // visible after you publish your qna KB
        );

        public QnAMakerService itKB = new QnAMakerService(
            "https://YOURQNAAPPID.azurewebsites.net",
            "your qna kb id", // example: /knowledgebases/righthereisKB-ID/generateAnswer
            "your qna Endpointkey" // visible after you publish your qna KB
        );

        public QnAMakerService generalKB = new QnAMakerService(
            "https://YOURQNAAPPID.azurewebsites.net", // Host portion visible after publishing KB in QNA maker
            "your qna kb id", // example: /knowledgebases/righthereisKB-ID/generateAnswer
            "your qna Endpointkey" // visible after you publish your qna KB
        );

        #region Intents


        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
        }

        [LuisIntent("support")]
        public async Task ChatGreetIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("support reached");
            context.Done("support reached");

        }

        #endregion

        // Entities found in result
        public string BotEntityRecognition(LuisResult result)
        {
            StringBuilder entityResults = new StringBuilder();

            if (result.Entities.Count > 0)
            {
                foreach (EntityRecommendation item in result.Entities)
                {
                    entityResults.Append(item.Type + "=" + item.Entity + ",");
                }
                entityResults.Remove(entityResults.Length - 1, 1);
            }

            return entityResults.ToString();
        }

        private async Task ShowLuisResult(IDialogContext context, LuisResult result)
        {
            // get recognized entities
            string entities = this.BotEntityRecognition(result);

            // round number
            string roundedScore = result.Intents[0].Score != null ? (Math.Round(result.Intents[0].Score.Value, 2).ToString()) : "0";

            await context.PostAsync($"**Query**: {result.Query}, **Intent**: {result.Intents[0].Intent}, **Score**: {roundedScore}. **Entities**: {entities}");
            context.Wait(MessageReceived);
        }
    }

    /// <summary>
    /// QnAMakerService is a wrapper over the QnA Maker REST endpoint
    /// </summary>
    [Serializable]
    public class QnAMakerService
    {
        private string qnaServiceHostName;
        private string knowledgeBaseId;
        private string endpointKey;

        /// <summary>
        /// Initialize a particular endpoint with it's details
        /// </summary>
        /// <param name="hostName">Hostname of the endpoint</param>
        /// <param name="kbId">Knowledge base ID</param>
        /// <param name="ek">Endpoint Key</param>
        public QnAMakerService(string hostName, string kbId, string ek)
        {
            qnaServiceHostName = hostName;
            knowledgeBaseId = kbId;
            endpointKey = ek;
        }

        /// <summary>
        /// Call the QnA Maker endpoint and get a response
        /// </summary>
        /// <param name="query">User question</param>
        /// <returns></returns>
        public string GetAnswer(string query)
        {
            var client = new RestClient(qnaServiceHostName + "/qnamaker/knowledgebases/" + knowledgeBaseId + "/generateAnswer");
            var request = new RestRequest(Method.POST);
            request.AddHeader("authorization", "EndpointKey " + endpointKey);
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json", "{\"question\": \"" + query + "\"}", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            // Deserialize the response JSON
            QnAAnswer answer = JsonConvert.DeserializeObject<QnAAnswer>(response.Content);

            // Return the answer if present
            if (answer.answers.Count > 0)
                return answer.answers[0].answer;
            else
                return "Looks like I need some more training. Update with adaptive card response offering to submit changes or offer feedback or take action to assist further";
        }
    }

    /* START - QnA Maker Response Class */
    public class Metadata
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class Answer
    {
        public IList<string> questions { get; set; }
        public string answer { get; set; }
        public double score { get; set; }
        public int id { get; set; }
        public string source { get; set; }
        public IList<object> keywords { get; set; }
        public IList<Metadata> metadata { get; set; }
    }

    public class QnAAnswer
    {
        public IList<Answer> answers { get; set; }
    }
    /* END - QnA Maker Response Class */

}