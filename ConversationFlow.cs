﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.BotBuilderSamples
{
    public class ConversationFlow
{
    // Identifies the last question asked.
    public enum Question
    {
        Location,
        Diet,
        Meal,
        Range,
        None, // Our last action did not involve a question.
    }

    // The last question asked.
    public Question LastQuestionAsked { get; set; } = Question.None;
}
}
