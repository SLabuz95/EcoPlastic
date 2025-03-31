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
    public partial class AddLocalRepositoryModel 
    {
        #region Properties
        private MessageRequestsController.RequestControl? RequestControl { get; set; } = null;
        private FileSystemWatcher watcher;
        private List<Config.Repository> repositories;

        private DataRequestMessage? finalValidationMessage = null;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SelectRepositoryPathCommand))]
        private bool initialized = false;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SelectRepositoryPathCommand))]
        private bool waiting = false;
                
        private bool IsReady => Initialized == true &&
                                Waiting == false;
        private bool finalValidation = false;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RepositoryPath))]
        private Boolean useParentDirectoryAsRepositoryName;

        private Task? validateRepositoryPathTask = null;

        [ObservableProperty]
        private Boolean createNewDirectoryAsRepositoryName;

        private String storedRepositoryName;
        [ObservableProperty]
        private String repositoryName;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
        private String repositoryNameErrorMessage;

        [ObservableProperty]
        private String repositoryPath;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
        private String repositoryPathErrorMessage;

        #endregion
        #region Commands
        [RelayCommand(CanExecute = nameof(IsReady))]
        private void SelectRepositoryPath()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                RepositoryPath = dialog.FileName;
            }
        }
        [RelayCommand/*(CanExecute = nameof(CanAccept))*/]
        private void Accept()
        {

            RequestControl = MessageRequestsController.SendRequest(new DataRequestMessage(this, Data.UserProfile), () => { });
            return;
            var path = RepositoryPath;
            var name = RepositoryName.Trim();
            // Verify again in case of some file system changes
            //ValidateRepositoryPath(ref path);
            ValidateRepositoryName(name);
            if (CanAccept() == true)
            {
                try
                {
                    LibGit2Sharp.Repository.Init(path);
                }
                catch (Exception)
                {
                    MessageBox.Show("Fail to init git repository");
                }
                //profile.repositories.Add(new Config.Repository
                //{
                //    Path = path,
                //    Name = name,
                //    RemotePath = String.Empty
                //});
                Cleanup();
                //WeakReferenceMessenger.Default.Send(new GitControl.Messages.RepositoriesUpdated());
                // WeakReferenceMessenger.Default.Send(new GitControl.Messages.DeactivateDialogMessage());
            }
        }
        [RelayCommand]
        private void Cancel()
        {
            //RequestControl.Cancel();


            //MessageRequestsController.SendRequest(new DataRequestMessage(this, Data.UserProfile), () => { });
            //WeakReferenceMessenger.Default.Send(new Messages.DataUpdateMessage(Data.DialogTypeControl, GitControlDialogs.None));
        }
        #endregion

        public AddLocalRepositoryModel()
        {
            WeakReferenceMessenger.Default.Register<DataUpdatedMessage>(this, (r, m) =>
            {
                switch (m.Type) {
                case Data.DialogTypeControl:
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
                break;
                }
            });
            WeakReferenceMessenger.Default.Register<DataResponseMessage>(this, (r, m) =>
            {
                
                switch (m.Type)
                {
                case Data.Repositories:
                {
                    var repositories = m.Data as List<Config.Repository>;
                    if (repositories != null)
                    {
                        this.repositories = repositories;
                        if (finalValidationMessage != null)
                        { // Final validation 

                        }
                        else
                        { // Validation on fly - while filling dialog informations / form
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
                    }
                }
                break;
                }
            });
            // First initalization required cause message is not detected
            Initialize();
        }

        private async void Initialize()
        {

            var task = new Task(() =>
            {
                InitializeFileWatcher();
                Cleanup();
                Task.Delay(5000).Wait();
                throw new Exception("In task");
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Initialized = true;
                });
            });
            task.Start();
        }

        private void Deinitialize()
        {
            Cleanup();
            Initialized = false;
        }
        private void InitializeFileWatcher()
        {
            if(watcher == null){
                watcher = new FileSystemWatcher();
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                watcher.Filter = ".git";
                watcher.Changed += new FileSystemEventHandler(OnChanged);
            }
        }
        private void InitializeRepositoryPathValidationTask()
        {
            //validateRepositoryPathTask = new (() =>
            //{

            //}, );
        }

        private void StartValidateRepositoryPathTask()
        {
            //if(validateRepositoryPathTask.IsCompleted)
        }

        private void StartPathWatching()
        {
            watcher.EnableRaisingEvents = true;
        }
        private void StopPathWatching()
        {
            watcher.EnableRaisingEvents = false;
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            var path = RepositoryPath;
            //ValidateRepositoryPath(ref path);
        }

        private void Cleanup()
        {
            RepositoryName = String.Empty;
            RepositoryPath = String.Empty;
            storedRepositoryName = String.Empty;
            UseParentDirectoryAsRepositoryName = false;
            CreateNewDirectoryAsRepositoryName = false;
        }


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
                    RepositoryName = String.Empty ;
                }
            }
            else
            {
                RepositoryName = storedRepositoryName;
            }
        }

        partial void OnCreateNewDirectoryAsRepositoryNameChanging(Boolean value)
        {
            ValidateRepositoryName(RepositoryName);
        }

        
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
            Regex r = new ("^[a-zA-Z][a-zA-Z0-9_]*$");
            if (r.IsMatch(name) == false)
            {
                RepositoryNameErrorMessage = "Repository name shall contain only alphanumeric characters";
                return;
            }
            //if(profile.repositories.Select(x => x.Name).Contains(name) == true)
            //{
            //    RepositoryNameErrorMessage = "Repository with the same name is already defined";
            //    return;
            //}
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


        partial void OnRepositoryPathChanging(String value)
        {
            //if(validateRepositoryPathTask.Status == TaskStatus.)
            //{
            //    ValidateRepositoryPath(value);
            //}
        }
        async void ValidateRepositoryPath(String path)
        {
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
            //if (profile.repositories.Select(x => x.Path).Contains(path) == true)
            //{
            //    RepositoryNameErrorMessage = "Repository with the same path is already defined";
            //    return;
            //}
            if (CreateNewDirectoryAsRepositoryName == true && String.IsNullOrEmpty(RepositoryNameErrorMessage))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                if (directoryInfo.EnumerateDirectories().FirstOrDefault(x => x.Name.Equals(RepositoryName)) != null)
                {
                    RepositoryPathErrorMessage = "Provided path already contains directory with repository name";
                    return;
                }
            }
            if (CreateNewDirectoryAsRepositoryName == false)
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
            if (UseParentDirectoryAsRepositoryName == true)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                RepositoryName = directoryInfo.Name;
            }

            RepositoryPathErrorMessage = String.Empty;
        }

        

        private bool CanAccept()
        {
            return String.IsNullOrEmpty(RepositoryNameErrorMessage) && String.IsNullOrEmpty(RepositoryPathErrorMessage);
        }

    }
}
