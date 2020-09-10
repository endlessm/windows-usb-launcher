// © 2019–2020 Endless OS Foundation LLC
//
// This file is part of Endless Launcher.
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EndlessLauncher.service
{
    public class FrameNavigationService : IFrameNavigationService, INotifyPropertyChanged
    {
        private readonly Dictionary<string, Uri> pagesByKey;
        private readonly List<string> historic;
        private string currentPageKey;
                                        
        public string CurrentPageKey
        {
            get
            {
                return currentPageKey;
            }

            private set
            {
                if (currentPageKey == value)
                {
                    return;
                }

                currentPageKey = value;
                OnPropertyChanged("CurrentPageKey");
            }
        }

        public object Parameter { get; private set; }

        public FrameNavigationService()
        {
            pagesByKey = new Dictionary<string, Uri>();
            historic = new List<string>();
        }
        public void GoBack()
        {
            if (historic.Count > 1)
            {
                historic.RemoveAt(historic.Count - 1);
                NavigateTo(historic.Last(), null);
            }
        }

        public void NavigateTo(string pageKey)
        {
            NavigateTo(pageKey, null);
        }

        public void NavigateTo(string pageKey, object parameter)
        {
            lock (pagesByKey)
            {
                if (!pagesByKey.ContainsKey(pageKey))
                {
                    throw new ArgumentException(string.Format("No such page: {0} ", pageKey), "pageKey");
                }

                if (GetDescendantFromName(Application.Current.MainWindow, "MainFrame") is Frame frame)
                {
                    frame.Source = pagesByKey[pageKey];
                }
                Parameter = parameter;
                historic.Add(pageKey);
                CurrentPageKey = pageKey;
            }
        }

        public void Configure(string key, Uri pageType)
        {
            lock (pagesByKey)
            {
                if (pagesByKey.ContainsKey(key))
                {
                    pagesByKey[key] = pageType;
                }
                else
                {
                    pagesByKey.Add(key, pageType);
                }
            }
        }

        private static FrameworkElement GetDescendantFromName(DependencyObject parent, string name)
        {
            var count = VisualTreeHelper.GetChildrenCount(parent);

            if (count < 1)
            {
                return null;
            }

            for (var i = 0; i < count; i++)
            {
                if (VisualTreeHelper.GetChild(parent, i) is FrameworkElement frameworkElement)
                {
                    if (frameworkElement.Name == name)
                    {
                        return frameworkElement;
                    }

                    frameworkElement = GetDescendantFromName(frameworkElement, name);
                    if (frameworkElement != null)
                    {
                        return frameworkElement;
                    }
                }
            }
            return null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
