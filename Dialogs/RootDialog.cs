using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;

namespace ChatterBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            try
            {
                if (activity.Value != null)
                {
                    JToken valueToken = JObject.Parse(activity.Value.ToString());
                    string actionValue = valueToken.SelectToken("action") != null ? valueToken.SelectToken("action").ToString() : string.Empty;

                    if (!string.IsNullOrEmpty(actionValue))
                    {
                        switch (valueToken.SelectToken("action").ToString())
                        {
                            case "task":
                                context.Call(new LuisDialog(), AfterSupport);
                                break;
                            case "support":
                                activity.Text = "support";
                                await context.Forward(new LuisDialog(), AfterSupport, activity, CancellationToken.None);
                                break;
                            case "guide":
                                context.Call(new LuisDialog(), AfterSupport);
                                break;
                            default:
                                await context.PostAsync($"I don't know how to handle the action \"{actionValue}\".");
                                context.Wait(MessageReceivedAsync);
                                break;
                        }
                    }
                    else
                    {
                        await context.PostAsync("It looks like no \"data\" was defined for this.  Check your adaptive cards JSON definition.");
                        context.Wait(MessageReceivedAsync);
                    }
                }
                else
                {
                    await context.Forward(new LuisDialog(), AfterSupport, activity, CancellationToken.None);
                }
            }
            catch (Exception e)
            {
                // IF an error occured with QnAMaker, post it out to the user
                await context.PostAsync(e.Message);
                // Wait for the next message from the user
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task AfterDo(IDialogContext context, IAwaitable<string> result)
        {
            context.Wait(MessageReceivedAsync);
        }

        private async Task AfterSupport(IDialogContext context, IAwaitable<object> result)
        {
            string message = null;

            try
            {
                message = (string)await result;
            }
            catch (Exception e)
            {
                await context.PostAsync($"BasicLuisDialog: {e.Message}");
                // Wait for the next message
                context.Wait(MessageReceivedAsync);
            }

            // Display the answer from QnA Maker Service
            var answer = message;

            if (((IMessageActivity)context.Activity).Text.ToLowerInvariant().Contains("support"))
            {
                // Since we are not needing to pass any message to start support, we can use call instead of forward
                //context.Call(new BasicLuisDialog(), AfterSupport);
            }
            else if (((IMessageActivity)context.Activity).Text.ToLowerInvariant().Contains("task"))
            {
                // Since we are not needing to pass any message to start trivia, we can use call instead of forward
                //context.Call(new BasicLuisDialog(), AfterSupport);
            }
            else if (!string.IsNullOrEmpty(answer))
            {
                Activity reply = ((Activity)context.Activity).CreateReply();

                string[] qnaAnswerData = answer.Split('|');
                string title = qnaAnswerData[0];
                string description = qnaAnswerData[1];
                string url = qnaAnswerData[2];
                string imageURL = qnaAnswerData[3];

                if (title == "")
                {
                    char charsToTrim = '|';
                    await context.PostAsync(answer.Trim(charsToTrim));
                }

                else
                {
                    HeroCard card = new HeroCard
                    {
                        Title = title,
                        Subtitle = description,
                    };
                    card.Buttons = new List<CardAction>
            {
                new CardAction(ActionTypes.OpenUrl, "Learn More", value: url)
            };
                    card.Images = new List<CardImage>
            {
                new CardImage( url = imageURL)
            };
                    reply.Attachments.Add(card.ToAttachment());
                    await context.PostAsync(reply);
                }
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                await context.PostAsync("No good match found in KB.");
                int length = (((IMessageActivity)context.Activity).Text ?? string.Empty).Length;
                await context.PostAsync($"You sent {((IMessageActivity)context.Activity).Text} which was {length} characters");
                context.Wait(MessageReceivedAsync);
            }
        }
    }
}