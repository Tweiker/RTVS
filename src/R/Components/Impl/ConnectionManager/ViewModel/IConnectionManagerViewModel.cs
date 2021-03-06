// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Microsoft.R.Components.ConnectionManager.ViewModel {
    public interface IConnectionManagerViewModel : INotifyPropertyChanged, IDisposable {
        ReadOnlyObservableCollection<IConnectionViewModel> LocalConnections { get; }
        ReadOnlyObservableCollection<IConnectionViewModel> RemoteConnections { get; }
        IConnectionViewModel EditedConnection { get; }
        bool HasLocalConnections { get; }
        bool IsEditingNew { get; }
        bool IsConnected { get; }
        
        void Edit(IConnectionViewModel connection);
        void EditNew();
        void CancelEdit();
        void Save(IConnectionViewModel connectionViewModel);

        void BrowseLocalPath(IConnectionViewModel connection);
        Task TestConnectionAsync(IConnectionViewModel connection);
        void CancelTestConnection(IConnectionViewModel connection);
        bool TryDelete(IConnectionViewModel connection);

        void Connect(IConnectionViewModel connection);
    }
}