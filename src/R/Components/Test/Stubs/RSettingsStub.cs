﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Logging;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.Settings;

namespace Microsoft.R.Components.Test.Stubs {
    [ExcludeFromCodeCoverage]
    public sealed class RSettingsStub : IRSettings {
        public bool AlwaysSaveHistory { get; set; }
        public bool ClearFilterOnAddHistory { get; set; }
        public bool MultilineHistorySelection { get; set; }
        public ConnectionInfo[] Connections { get; set; }
        public ConnectionInfo LastActiveConnection { get; set; }
        public string CranMirror { get; set; }
        public string WorkingDirectory { get; set; }
        public bool ShowPackageManagerDisclaimer { get; set; }
        public HelpBrowserType HelpBrowserType { get; set; }
        public int RCodePage { get; set; }
        public bool EvaluateActiveBindings { get; set; }
        public LogVerbosity LogVerbosity { get; set; }

#pragma warning disable 67
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
