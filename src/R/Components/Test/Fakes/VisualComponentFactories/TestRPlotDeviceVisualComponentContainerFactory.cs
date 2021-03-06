// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Plots;
using Microsoft.R.Components.Plots.Implementation;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.View;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.Test.Fakes.VisualComponentFactories {
    [ExcludeFromCodeCoverage]
    [Export(typeof (IRPlotDeviceVisualComponentContainerFactory))]
    [Export(typeof (TestRPlotDeviceVisualComponentContainerFactory))]
    internal sealed class TestRPlotDeviceVisualComponentContainerFactory : ContainerFactoryBase<IRPlotDeviceVisualComponent>, IRPlotDeviceVisualComponentContainerFactory {
        private readonly IRSettings _settings;
        private readonly ICoreShell _coreShell;

        [ImportingConstructor]
        public TestRPlotDeviceVisualComponentContainerFactory(IRSettings settings, ICoreShell coreShell) {
            _settings = settings;
            _coreShell = coreShell;
        }

        public Nullable<PlotDeviceProperties> DeviceProperties { get; set; } = new PlotDeviceProperties(360, 360, 96);

        public IVisualComponentContainer<IRPlotDeviceVisualComponent> GetOrCreate(IRPlotManager plotManager, IRSession session, int instanceId = 0) {
            return GetOrCreate(instanceId, container => new RPlotDeviceVisualComponent(plotManager, null, instanceId, container, _coreShell) { TestDeviceProperties=DeviceProperties });
        }
    }
}
