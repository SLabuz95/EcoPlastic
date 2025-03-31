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
using WindowsAPICodePack.Dialogs;

namespace GitControl.ViewModel.Dialogs
{
    [ObservableObject]
    public partial class CloneRemoteRepositoryModel 
    {
        private FileSystemWatcher? watcher;
        private UserProfile profile;
        [ObservableProperty]
        private bool initialized = false;
        public CloneRemoteRepositoryModel()
        {
            WeakReferenceMessenger.Default.Register<DataUpdatedMessage>(this, (r, m) =>
            {
                if (m.Type == Data.DialogTypeControl)
                {
                    if (m.Data is GitControlDialogs)
                    {
                        GitControlDialogs gitControlDialogs = (GitControlDialogs)m.Data;
                        if (gitControlDialogs == GitControlDialogs.AddLocalRepository)
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
            Initialized = true;
        }

        private void Deinitialize()
        {
            Cleanup();
            Initialized = false;
        }
        private void InitializeFileWatcher()
        {
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = ".git";
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.EnableRaisingEvents = true;
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
            CreateNewDirectoryAsRepositoryName = false;
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
        [ObservableProperty]
        private Boolean createNewDirectoryAsRepositoryName;

        partial void OnCreateNewDirectoryAsRepositoryNameChanging(Boolean value)
        {
            ValidateRepositoryName(RepositoryName);
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
            if (CreateNewDirectoryAsRepositoryName == true && String.IsNullOrEmpty(RepositoryPathErrorMessage))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(RepositoryPath);
                if (directoryInfo.EnumerateDirectories().FirstOrDefault(x => x.Name.Equals(name)) != null)
                {
                    RepositoryPathErrorMessage = "Provided path already contains directory with repository name";
                    return;
                }
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
            FileInfo fileInfo;
            // Repositories validation (in case of removing repotories by user)


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
            if(CreateNewDirectoryAsRepositoryName == true)
            {
                if (String.IsNullOrEmpty(RepositoryNameErrorMessage))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(path);
                    if (directoryInfo.EnumerateDirectories().FirstOrDefault(x => x.Name.Equals(RepositoryName)) != null)
                    {
                        RepositoryPathErrorMessage = "Provided path already contains directory with repository name";
                        return;
                    }
                }
            }
            else
            {
                try
                {
                    using (var repo = new LibGit2Sharp.Repository(path)) { }
                    // Repository exist - nok
                    RepositoryPathErrorMessage = "Provided path already contains git repository";
                    return;
                }
                catch (LibGit2Sharp.RepositoryNotFoundException ex)
                { } // Repository not found - thats ok
            }
            if (profile.repositories.Select(x => x.Path).Contains(path) == true)
            {
                RepositoryNameErrorMessage = "Repository with the same path is already defined";
                return;
            }
            if (CreateNewDirectoryAsRepositoryName == false)
            {
                
            }
            if (UseParentDirectoryAsRepositoryName == true)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                RepositoryName = directoryInfo.Name;
            }

            RepositoryPathErrorMessage = String.Empty;
        }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
        private String repositoryPathErrorMessage;

        [RelayCommand]
        private void SelectRepositoryPath()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
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
                Trace.WriteLine("Clean up");
                // Cleanup();
            }
        }
        [RelayCommand]
        private void Cancel()
        {
            Cleanup();
            //WeakReferenceMessenger.Default.Send(new GitControl.Messages.DeactivateDialogMessage());
        }

        private bool CanAccept()
        {
            return String.IsNullOrEmpty(RepositoryNameErrorMessage) && String.IsNullOrEmpty(RepositoryPathErrorMessage);
        }

    }
}
