﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Common.Core;
using Microsoft.R.Components.Help;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.Threading;

namespace Microsoft.VisualStudio.R.Interactive.Test.Help {
    class RHostClientHelpTestApp : RHostClientTestApp {
        private IHelpVisualComponent _component;

        public IHelpVisualComponent Component {
            get { return _component; }
            set {
                _component = value;
                _component.Browser.Navigating += Browser_Navigating;
                _component.Browser.Navigated += Browser_Navigated;
            }
        }

        public ManualResetEventSlim Ready { get; }
        public Uri Uri { get; private set; }

        public RHostClientHelpTestApp() {
            Ready = new ManualResetEventSlim();
        }

        public override Task ShowHelpAsync(string url, CancellationToken cancellationToken) {
            Ready.Reset();
            return UIThreadHelper.Instance.InvokeAsync(() => Component.Navigate(url), cancellationToken);
        }

        private void Browser_Navigated(object sender, WebBrowserNavigatedEventArgs e) {
            UIThreadHelper.Instance.InvokeAsync(() => {
                Component.Browser.DocumentCompleted += OnDocumentCompleted;
                Uri = _component.Browser.Url;
            }).DoNotWait();
        }

        private void Browser_Navigating(object sender, WebBrowserNavigatingEventArgs e) {
            Ready.Reset();
        }

        private void OnDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e) {
            UIThreadHelper.Instance.InvokeAsync(() => {
                Component.Browser.DocumentCompleted -= OnDocumentCompleted;
                Ready.Set();
            }).DoNotWait();
        }
    }
}
