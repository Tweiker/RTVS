﻿using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Microsoft.VisualStudio.R.Package.DataInspect
{
    public partial class VariableView : UserControl
    {
        private readonly VariableProvider _variableProvider;
        private Variable _globalEnv;

        public VariableView()
        {
            InitializeComponent();

            _variableProvider = new VariableProvider();
            _variableProvider.SessionsChanged += VariableProvider_SessionsChanged;

            InitializeData();
        }

        private void VariableProvider_SessionsChanged(object sender, EventArgs e)
        {
            InitializeData();
        }

        public void InitializeData()
        {
            Task t = Task.Run(async () =>   // no await
            {
                var globalEnvVar = await _variableProvider.EvaluateVariable(
                    new VariableEvaluationContext()
                    {
                        Environment = VariableEvaluationContext.GlobalEnv,
                        VariableName = VariableEvaluationContext.GlobalEnv
                    });

                if (globalEnvVar != null)
                {
                    DispatchInvoke(() => SetVariable(globalEnvVar), DispatcherPriority.Normal);
                }
            });
        }

        private void SetVariable(Variable variable)
        {
            if (_globalEnv != null)
            {
                _globalEnv.Children.CollectionChanged -= VariableChildren_CollectionChanged;
            }

            _globalEnv = variable;
            if (_globalEnv != null)
            {
                EnvironmentTextBlock.Text = _globalEnv.VariableValue;

                _globalEnv.Children.CollectionChanged += VariableChildren_CollectionChanged;
                foreach (var child in _globalEnv.Children)
                {
                    RootTreeView.Items.Add(child);
                }
            }
        }

        private int itemIndexOffset = 0;
        private void VariableChildren_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        int index = e.NewStartingIndex + itemIndexOffset; // offset to Column header
                        foreach (var child in e.NewItems)
                        {
                            RootTreeView.Items.Insert(index++, child);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        int index = e.OldStartingIndex + itemIndexOffset;
                        foreach (var child in e.OldItems)
                        {
                            RootTreeView.Items.Insert(index++, child);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        while (RootTreeView.Items.Count > itemIndexOffset)
                        {
                            RootTreeView.Items.RemoveAt(RootTreeView.Items.Count - itemIndexOffset);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                default:
                    throw new NotSupportedException();
            }
        }

        private static void DispatchInvoke(Action toInvoke, DispatcherPriority priority)
        {
            Action guardedAction =
                () =>
                {
                    try
                    {
                        toInvoke();
                    }
                    catch (Exception e)
                    {
                        Debug.Assert(false, "Guarded invoke caught exception", e.Message);
                    }
                };

            Application.Current.Dispatcher.BeginInvoke(guardedAction, priority);    // TODO: acquiring Application.Current.Dispatcher, create utility class for UI thread and use it
        }
    }
}