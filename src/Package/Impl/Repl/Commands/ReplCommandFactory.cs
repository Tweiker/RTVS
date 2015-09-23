﻿using System.Collections.Generic;
using Microsoft.Languages.Editor.Controller;
using Microsoft.R.Editor.Commands;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands
{
    internal sealed class ReplCommandFactory: ICommandFactory
    {
        public IEnumerable<ICommand> GetCommands(ITextView textView, ITextBuffer textBuffer)
        {
            List<ICommand> commands = new List<ICommand>();

            commands.Add(new RTypingCommandHandler(textView));
            commands.Add(new RCompletionCommandHandler(textView, textBuffer));

            return commands;
        }
    }
}
