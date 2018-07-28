﻿using Microsoft.Bot.Builder.Luis.Models;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BuddyBot.Helpers
{
    public static class MessageHelper
    {
        public static IList<int> ExtractIntegersFromMessage(string message)
        {
            string[] values = Regex.Split(message, @"\D+");
            IList<int> integersList = new List<int>();

            foreach (var value in values)
            {
                int.TryParse(value, out var number);
                integersList.Add(number);
            }

            return integersList;
        }
    }
}