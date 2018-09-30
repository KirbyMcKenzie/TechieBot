﻿using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using BuddyBot.Helpers;
using BuddyBot.Models;
using BuddyBot.Services.Contracts;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;

namespace BuddyBot.Dialogs
{
    public class PreferredWeatherLocationDialog : IDialog<string>
    {
        private readonly IBotDataService _botDataService;
        private readonly IList<EntityRecommendation> _entities;
        private string _extractedCityFromMessage;

        public PreferredWeatherLocationDialog(IBotDataService botDataService, IList<EntityRecommendation> entities)
        {
            _botDataService = botDataService;
            _entities = entities;
        }

        public Task StartAsync(IDialogContext context)
        {
            _extractedCityFromMessage = MessageHelpers.ExtractEntityFromMessage("City.Name", _entities);
            City savedPreferredCity= _botDataService.GetPreferredWeatherLocation(context);

             if (!string.IsNullOrWhiteSpace(_extractedCityFromMessage))
            {
                PromptDialog.Confirm(context, ResumeAfterPreferredCityConfirmation, $"So you'd like to change your preferred weather location to {_extractedCityFromMessage}?", $"Sorry I don't understand - try again! So you'd like to change your preferred weather location to {_extractedCityFromMessage}");
                return Task.CompletedTask;
            }

            if (!string.IsNullOrWhiteSpace(savedPreferredCity.Name))
            {
                PromptDialog.Confirm(context, ResumeAfterConfirmation, $"Your saved weather location is {savedPreferredCity.Name}, {savedPreferredCity.Country}, would you like to change it?", $"Sorry I don't understand - try again! Would you like to change your preferred weather location?");
                return Task.CompletedTask;
            }

            PromptDialog.Text(context, ResumeAfterPromptForPreferredLocation, "What's the name of the city you'd like the weather for?", "Sorry I didn't get that - try again! What's the name of your preferred weather location?");
            return Task.CompletedTask;

        }

        private async Task ResumeAfterPreferredCityConfirmation(IDialogContext context, IAwaitable<bool> result)
        {
            bool confirmation = await result;

            switch (confirmation)
            {
                case true:
                    await ResumeAfterPromptForPreferredLocation(context, new AwaitableFromItem<string>(_extractedCityFromMessage));
                    break;
                default:
                    context.Done(_botDataService.GetPreferredWeatherLocation(context).Name);
                    break;
            }
        }


        private async Task ResumeAfterPromptForPreferredLocation(IDialogContext context, IAwaitable<string> result)
        {
            string cityName = await result;

            IList<City> citySearchResults = MessageHelpers.SearchForCities(cityName);

            if (citySearchResults != null && citySearchResults.Count <= 0)
            {
                context.Done($"I'm sorry, I couldn't find any results for '{cityName}'. " +
                             $"Make sure you've spelt everything correctly and try again 😊");
            }
            else if (citySearchResults != null && citySearchResults.Count >= 2)
            {
                // TODO - Change type of card
                // TODO - Think about limiting amount of cards displayed, see more button? 
                List<CardAction> cityCardActionList = CreateCardActionList(citySearchResults);

                HeroCard card = new HeroCard
                {
                    Title = $"I found {citySearchResults.Count} results for '{cityName}'",
                    Subtitle = "please select your closest location",
                    Buttons = cityCardActionList
                };

                var message = context.MakeMessage();
                message.Attachments.Add(card.ToAttachment());

                await context.PostAsync(message);
                context.Wait(this.MessageReceivedAsync);
            }

        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            IMessageActivity cityId = await result;


            string extractedId = MessageHelpers.ExtractIdFromMessage(cityId.Text);

            City preferredCity = MessageHelpers.GetCityById(extractedId);

            _botDataService.setPreferredWeatherLocation(context, preferredCity);

            context.Done(preferredCity.Name);
        }

        private async Task ResumeAfterConfirmation(IDialogContext context, IAwaitable<bool> result)
        {
            bool confirmation = await result;

            switch (confirmation)
            {
                case true:
                    PromptDialog.Text(context, ResumeAfterPromptForPreferredLocation, "What's the name of the city you'd like the weather for?", "Sorry I didn't get that - try again! What's the name of your preferred weather location?");
                    break;
                default:
                    context.Done(_botDataService.GetPreferredWeatherLocation(context).Name);
                    break;
            }
        }

        // TODO - Move to helpers
        private List<CardAction> CreateCardActionList(IList<City> cityResultList)
        {
            List<CardAction> cardOptionsList = new List<CardAction>();

            foreach (var city in cityResultList)
            {
                cardOptionsList.Add(new CardAction(ActionTypes.ImBack,
                    title: $"{city.Name}, {city.Country}",
                    value: $"{city.Name}, {city.Country}, {city.Id}"));

            }

            return cardOptionsList;
        }
    }
}