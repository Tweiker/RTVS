﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Common.Core;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Editor;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Help {
    /// <summary>
    /// Base class for 'Help On Current' type of commands.
    /// </summary>
    internal abstract class HelpOnCurrentCommandBase : PackageCommand {
        private const int MaxHelpItemLength = 128;
        private readonly string _baseCommandName;

        protected readonly IRInteractiveWorkflow _workflow;
        protected readonly IActiveWpfTextViewTracker _textViewTracker;

        private IActiveRInteractiveWindowTracker _activeReplTracker;
        public HelpOnCurrentCommandBase(
            Guid group, int id,
            IRInteractiveWorkflow workflow, 
            IActiveWpfTextViewTracker textViewTracker, 
            IActiveRInteractiveWindowTracker activeReplTracker,
            string baseCommandName) :
            base(group, id) {
            _activeReplTracker = activeReplTracker;
            _workflow = workflow;
            _textViewTracker = textViewTracker;
            _baseCommandName = baseCommandName;
        }

        protected override void SetStatus() {
            string item = GetItemUnderCaret();
            if (!string.IsNullOrEmpty(item)) {
                Enabled = true;
                Text = string.Format(CultureInfo.InvariantCulture, _baseCommandName, item);
            } else {
                Enabled = false;
            }
        }

        protected override void Handle() {
            try {
                if (!_workflow.RSession.IsHostRunning) {
                    return;
                }

                // Fetch identifier under the cursor
                string item = GetItemUnderCaret();
                if (item == null || item.Length >= MaxHelpItemLength) {
                    return;
                }

                Handle(item);
            } catch (Exception ex) {
                Debug.Assert(false, string.Format(CultureInfo.InvariantCulture, "Help on current item failed. Exception: {0}", ex.Message));
                // Catch everything so exceptions don't leave the async void method
                if (ex.IsCriticalException()) {
                    throw;
                }
            }
        }

        protected abstract void Handle(string item);

        protected string GetItemUnderCaret() {
            ITextView textView = GetActiveView();
            if (textView != null) {
                Span span;
                return textView.GetIdentifierUnderCaret(out span);
            }
            return string.Empty;
        }

        protected ITextView GetActiveView() {
            var activeReplWindow = _activeReplTracker.LastActiveWindow;
            if (activeReplWindow != null && _activeReplTracker.IsActive) {
                return activeReplWindow.InteractiveWindow.TextView;
            }
            return _textViewTracker.LastActiveTextView;
        }
    }
}
