using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using AdaptiveCards;
using ChatterBot.Utility;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace ChatterBot.Dialogs
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    [Serializable]
    public class LuisDialog : LuisDialog<object>
    {
        public LuisDialog() : base(new LuisService(new LuisModelAttribute(
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
            var testQnaResult = "DisplayCard:PasswordResetCard.json";
            if (testQnaResult.StartsWith("DisplayCard"))
            {
                var reply = context.MakeMessage();

                try
                {
                    // read the json in from our file
                    string json = File.ReadAllText(HttpContext.Current.Request.MapPath("~\\AdaptiveCards\\TaskCard.json"));
                    // use Newtonsofts JsonConvert to deserialized the json into a C# AdaptiveCard object
                    AdaptiveCard card = JsonConvert.DeserializeObject<AdaptiveCard>(json);
                    // put the adaptive card as an attachment to the reply message
                    reply.Attachments.Add(new Attachment
                    {
                        ContentType = AdaptiveCard.ContentType,
                        Content = card
                    });
                }
                catch (Exception e)
                {
                    // if an error occured add the error text as the message
                    reply.Text = e.Message;
                }

                await context.PostAsync(reply);
            }
            else
            {

            }
            context.Done("support reached");

        }

        #endregion


    }




}