using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CredentialManagement;
using GitControl.Config;
using GitControl.Messages;
using GitControl.View.Dialogs;
using LibGit2Sharp;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Xml.Linq;
using WindowsAPICodePack.Dialogs;

namespace GitControl.ViewModel.Dialogs
{
    [ObservableObject]
    public partial class AddExistingRepositoryModel 
    {
        private FileSystemWatcher? watcher;

        private UserProfile profile;
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SelectRepositoryPathCommand))]
        private bool initialized = false;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SelectRepositoryPathCommand))]
        private bool waiting = false;

        private bool IsReady => Initialized == true &&
                                Waiting == false;

        public AddExistingRepositoryModel()
        {
            WeakReferenceMessenger.Default.Register<DataUpdatedMessage>(this, (r, m) =>
            {
                if (m.Type == Data.DialogTypeControl)
                {
                    if(m.Data is GitControlDialogs)
                    {
                        GitControlDialogs gitControlDialogs = (GitControlDialogs)m.Data;
                        if(gitControlDialogs == GitControlDialogs.AddExistingRepository)
                        {
                            Initialize();
                        }
                        else
                        {
                            if (initialized == true)
                            {
                                Deinitialize();
                            }
                        }
                    }
                }

            });
        }

        private void Initialize()
        {
            InitializeFileWatcher();
            Thread.Sleep(10000);
            Initialized = true;
        }

        private void Deinitialize()
        {
            Cleanup();
            Initialized = false;
        }

        private void InitializeFileWatcher()
        {
            FileSystemWatcher watcher = new ();
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = ".git";
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.EnableRaisingEvents = true;
        }
        private void DeinitializeFileWatcher()
        {
            watcher = null;
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            var path = RepositoryPath;
            ValidateRepositoryPath(ref path);
        }

        private void Cleanup()
        {
            RepositoryName = String.Empty;
            RepositoryPath = String.Empty;
            storedRepositoryName = String.Empty;
            UseParentDirectoryAsRepositoryName = false;
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RepositoryPath))]
        private Boolean useParentDirectoryAsRepositoryName;

        partial void OnUseParentDirectoryAsRepositoryNameChanging(Boolean value)
        {
            if (value == true)
            { // Store RepositoryName
                storedRepositoryName = RepositoryName;
                if (String.IsNullOrEmpty(RepositoryPath) == false)
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(RepositoryPath);
                    RepositoryName = directoryInfo.Name;
                }
                else
                {
                    RepositoryName = String.Empty;
                }
            }
            else
            {
                RepositoryName = storedRepositoryName;
            }
        }
        private String storedRepositoryName;
        [ObservableProperty]
        private String repositoryName;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
        private String repositoryNameErrorMessage;

        partial void OnRepositoryNameChanging(String value)
        {
            ValidateRepositoryName(value);
        }

        void ValidateRepositoryName(String name)
        {
            name = name.Trim();
            if (String.IsNullOrEmpty(name))
            {
                RepositoryNameErrorMessage = "Enter repository name";
                return;
            }
            Regex r = new Regex("^[a-zA-Z][a-zA-Z0-9_]*$");
            if (r.IsMatch(name) == false)
            {
                RepositoryNameErrorMessage = "Repository name shall contain only alphanumeric characters";
                return;
            }
            if (profile.repositories.Select(x => x.Name).Contains(name) == true)
            {
                RepositoryNameErrorMessage = "Repository with the same name is already defined";
                return;
            }
            RepositoryNameErrorMessage = String.Empty;
        }

        [ObservableProperty]
        private String repositoryPath;

        partial void OnRepositoryPathChanging(String value)
        {
            ValidateRepositoryPath(ref value);
        }
        void ValidateRepositoryPath(ref String path)
        {
            RepositoryPathErrorMessage = String.Empty;
            path = path.Trim();
            if (String.IsNullOrEmpty(path))
            {
                RepositoryPathErrorMessage = "Enter repository path";
                return;
            }
            if (Directory.Exists(path) == false)
            {
                RepositoryPathErrorMessage = "Provided path doesnt exist";
                return;
            }
            watcher.Path = path;
            profile.ValidateRepositories();
            if (profile.repositories.Select(x => x.Path).Contains(path) == true)
            {
                RepositoryNameErrorMessage = "Repository with the same path is already defined";
                return;
            }
            try
            {
                using (var repo = new LibGit2Sharp.Repository(path)) {
                    if(repo.Network.Remotes.Count() > 1)
                    {
                        RepositoryPathErrorMessage = "Selected git repository contains more than 1 remote repository. It's not supported.";                        
                    }
                }
                if (String.IsNullOrEmpty(RepositoryPathErrorMessage) == false) // Error reported
                    return;
                // Repository exists - ok
            }
            catch (LibGit2Sharp.RepositoryNotFoundException)
            {
                RepositoryPathErrorMessage = "Provided path doesnt contain git repository";
                return;
            } // Repository not found - thats nok

            if (UseParentDirectoryAsRepositoryName == true)
            {
                DirectoryInfo directoryInfo = new (path);
                RepositoryName = directoryInfo.Name;
            }

            RepositoryPathErrorMessage = String.Empty;
        }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
        private String repositoryPathErrorMessage;

        [RelayCommand(CanExecute =nameof(IsReady))]        
        private void SelectRepositoryPath()
        {
            CommonOpenFileDialog dialog = new ();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                RepositoryPath = dialog.FileName;
            }
        }
        [RelayCommand(CanExecute = nameof(CanAccept))]
        private void Accept()
        {
            var path = RepositoryPath;
            var name = RepositoryName;
            // Verify again in case of some file system changes
            ValidateRepositoryPath(ref path); 
            ValidateRepositoryName(name);
            if (CanAccept() == true)
            {
                bool LocalRepo = true;
                string RemoteName = String.Empty;
                string RemoteUrl = String.Empty;
                Remote remote = null;
                try
                {
                    using (var repo = new LibGit2Sharp.Repository(path))
                    {
                        LocalRepo = repo.Network.Remotes.Count() == 0;
                        if (repo.Network.Remotes.Count() > 1)
                        {
                            RepositoryPathErrorMessage = "Selected git repository contains more than 1 remote repository. It's not supported.";
                        }else if (LocalRepo == false)
                        {
                            remote = repo.Network.Remotes.First();
                            RemoteName = remote.Name;
                            RemoteUrl = remote.Url;
                        }
                    }
                    if (String.IsNullOrEmpty(RepositoryPathErrorMessage) == false) // Error reported
                        return;
                    // Repository exists - ok
                }
                catch (LibGit2Sharp.RepositoryNotFoundException)
                {
                    MessageBox.Show("Fail to open git repository");
                } // Repository not found - thats nok
                profile.repositories.Add(new Config.Repository
                {
                    Path = path,
                    Name = name,
                    RemoteName = RemoteName,
                    RemotePath = RemoteUrl
                });
                Cleanup();
                //WeakReferenceMessenger.Default.Send(new GitControl.Messages.RepositoriesUpdated());
                //WeakReferenceMessenger.Default.Send(new Messages.DeactivateDialogMessage());
                
            }
        }
        [RelayCommand]
        private void Cancel()
        {
            Cleanup();
            //WeakReferenceMessenger.Default.Send(new Messages.DeactivateDialogMessage());
        }
        private bool CanAccept()
        {
            return String.IsNullOrEmpty(RepositoryNameErrorMessage) && String.IsNullOrEmpty(RepositoryPathErrorMessage);
        }

    }
}
