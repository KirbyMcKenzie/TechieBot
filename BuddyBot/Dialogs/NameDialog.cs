﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using BuddyBot.Helpers;
using BuddyBot.Helpers.Contracts;
using BuddyBot.Services.Contracts;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Luis.Models;

namespace BuddyBot.Dialogs
{
    [Serializable]
    public class NameDialog : IDialog<string>
    {
        private string _preferredName;
        private readonly IBotDataService _botDataService;
        private readonly IMessageHelper _messageHelpers;
        private readonly IList<EntityRecommendation> _entities;

        public NameDialog(IBotDataService botDataService, IMessageHelper messageHelpers, IList<EntityRecommendation> entities)
        {
            SetField.NotNull(out _botDataService, nameof(botDataService), botDataService);
            SetField.NotNull(out _messageHelpers, nameof(messageHelpers), messageHelpers);
            _entities = entities;
        }


        /// <summary>
        /// Execution for the <see cref="NameDialog"/> starts here. 
        /// </summary>
        /// <param name="context">Mandatory. The context for the execution of a dialog's conversational process.</param>
        public Task StartAsync(IDialogContext context)
        {

            if (_entities != null)
            {
                _preferredName = _messageHelpers.ExtractEntityFromMessage("User.PreferredName", _entities);
            }
           
            string name = _botDataService.GetPreferredName(context);

            if (!string.IsNullOrWhiteSpace(_preferredName))
            {
                PromptDialog.Confirm(context, ResumeAfterPreferredNameConfirmation, 
                    $"So you'd like me to call you {_preferredName}?", $"Sorry I don't understand - try again! Should I call you {_preferredName}?");
                return Task.CompletedTask;
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                PromptDialog.Confirm(context, ResumeAfterConfirmation,
                    $"Do you want me to keep calling you {name}?", $"Sorry I don't understand - try again! Should I call you {name}?");
                return Task.CompletedTask;
            }

            PromptDialog.Text(context, ResumeAfterNameFilled, 
                "What is your name?", "Sorry I didn't get that - try again! What should I call you?");
            return Task.CompletedTask;
        }


        /// <summary>
        /// Called when LUIS picks up a preferred name entity from the users utterance and 
        /// confirms if they'd like to be called by that name.
        /// </summary>
        /// <param name="context">Mandatory. The context for the execution of a dialog's conversational process.</param>
        /// <param name="result">Mandatory. The result of the users confirmation. </param>
        private async Task ResumeAfterPreferredNameConfirmation(IDialogContext context, IAwaitable<bool> result)
        {

            bool confirmation = await result;

            switch (confirmation)
            {
                case true:
                    _botDataService.SetPreferredName(context, _preferredName);
                    context.Done(_preferredName);
                    break;
                default:
                    PromptDialog.Text(context, ResumeAfterNameFilled, "Okay, what should I call you?", "Sorry I didn't get that - try again! What should I call you?");
                    break;
            }
        }


        /// <summary>
        /// Called when the user has entered their preferred name after being prompted by <see cref="PromptDialog"/>.
        /// </summary>
        /// <param name="context">Mandatory. The context for the execution of a dialog's conversational process.</param>
        /// <param name="result">Mandatory. The result of the users preferred name. </param>
        private async Task ResumeAfterNameFilled(IDialogContext context, IAwaitable<string> result)
        {
            _preferredName = await result;
            PromptDialog.Confirm(context, ResumeAfterPreferredNameConfirmation, $"So you'd like me to call you {_preferredName}?", $"Sorry I don't understand - try again! Should I call you {_preferredName}?");
        }


        /// <summary>
        /// Prompts the user for comfirmation using <see cref="PromptDialog"/> after they have entered their name.
        /// </summary>
        /// <param name="context">Mandatory. The context for the execution of a dialog's conversational process.</param>
        /// <param name="result">Mandatory. The result of the users confirmation. </param>
        private async Task ResumeAfterConfirmation(IDialogContext context, IAwaitable<bool> result)
        {
            bool confirmation = await result;

            switch (confirmation)
            {
                case true:
                    context.Done(_botDataService.GetPreferredName(context));
                    break;
                default:
                    PromptDialog.Text(context, ResumeAfterNameFilled, "Okay, what should I call you?", "Sorry I didn't get that - try again! What should I call you?");
                    break;
            }
        }
    }
}