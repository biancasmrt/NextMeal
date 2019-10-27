// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using Microsoft.Recognizers.Text.Number;

namespace Microsoft.BotBuilderSamples
{
    // This IBot implementation can run any type of Dialog. The use of type parameterization is to allows multiple different bots
    // to be run at different endpoints within the same project. This can be achieved by defining distinct Controller types
    // each with dependency on distinct IBot types, this way ASP Dependency Injection can glue everything together without ambiguity.
    public class CustomPromptBot : ActivityHandler 
    {
        private readonly BotState _userState;
        private readonly BotState _conversationState;
        
        public CustomPromptBot(ConversationState conversationState, UserState userState)
        {
            _conversationState = conversationState;
            _userState = userState;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
           
            var conversationStateAccessors = _conversationState.CreateProperty<ConversationFlow>(nameof(ConversationFlow));
            var flow = await conversationStateAccessors.GetAsync(turnContext, () => new ConversationFlow());

            var userStateAccessors = _userState.CreateProperty<UserProfile>(nameof(UserProfile));
            var profile = await userStateAccessors.GetAsync(turnContext, () => new UserProfile());

            await FillOutUserProfileAsync(flow, profile, turnContext);

            // Save changes.
            await _conversationState.SaveChangesAsync(turnContext);
            await _userState.SaveChangesAsync(turnContext);
        }

        private static async Task FillOutUserProfileAsync(ConversationFlow flow, UserProfile profile, ITurnContext turnContext)
        {
            string input = turnContext.Activity.Text?.Trim();
            string message;

            switch (flow.LastQuestionAsked)
            {
                case ConversationFlow.Question.None:
                    await turnContext.SendActivityAsync("Welcome to FoodMatch!");
                    await turnContext.SendActivityAsync("Where are you?");
                    await turnContext.SendActivityAsync("Enter Atlanta, New York, or Chicago: ");
                    flow.LastQuestionAsked = ConversationFlow.Question.Location;
                    break;
                case ConversationFlow.Question.Location:
                    if (ValidateString(input, out string location, out message))
                    {
                        profile.Location = location;
                        await turnContext.SendActivityAsync($"You're in {profile.Location}.");
                        await turnContext.SendActivityAsync("Do you have any dietary restrictions?");
                        await turnContext.SendActivityAsync("Enter Vegetarian, Vegan, Gluten-Free, Halal, or none");
                        flow.LastQuestionAsked = ConversationFlow.Question.Diet;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.");
                        break;
                    }
                case ConversationFlow.Question.Diet:
                    if (ValidateString(input, out string diet, out message))
                    {
                        profile.Diet = diet;
                        await turnContext.SendActivityAsync($"Great! Next, what meal are you looking for?");
                        await turnContext.SendActivityAsync($"Want Breakfast, Lunch or Dinner?");
                        flow.LastQuestionAsked = ConversationFlow.Question.Meal;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.");
                        break;
                    }
                case ConversationFlow.Question.Meal:
                    if (ValidateString(input, out string meal, out message))
                    {
                        profile.Meal = meal;
                        await turnContext.SendActivityAsync($"Now, what price range are you looking for?");
                        await turnContext.SendActivityAsync($"0-5, 5-10, 10-20, 20+");
                        flow.LastQuestionAsked = ConversationFlow.Question.Range;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.");
                        break;
                    }

                case ConversationFlow.Question.Range:
                    if (ValidateString(input, out string range, out message))
                    {
                        profile.Range = range;
                        await turnContext.SendActivityAsync($"Displaying ");
                        await turnContext.SendActivityAsync($"Type anything to run the bot again.");
                        flow.LastQuestionAsked = ConversationFlow.Question.None;
                        profile = new UserProfile();
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.");
                        break;
                    }
            }
        }

        private static bool ValidateString(string input, out string checker, out string message)
        {
            checker = null;
            message = null;

            if (string.IsNullOrWhiteSpace(input))
            {
                message = "Please enter the exact location.";
            }
            else
            {
                checker = input.Trim();
            }

            return message is null;


        }

        private static bool ValidateDiet(string input, out string diet, out string message)
        {
            diet = null;
            message = null;

            if (string.IsNullOrWhiteSpace(input))
            {
                message = "Please enter the exact diet.";
            }
            else
            {
                diet = input.Trim();
            }

            return message is null;
        }

        private static bool ValidateMeal(string input, out string meal, out string message)
        {
            meal = null;
            message = null;

            if (string.IsNullOrWhiteSpace(input))
            {
                message = "Please enter the exact meal type.";
            }
            else
            {
                meal = input.Trim();
            }

            return message is null;
        }
    }
}
