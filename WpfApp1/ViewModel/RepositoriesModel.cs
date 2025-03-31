using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitControl.Config;
using GitControl.View.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitControl.Extentions;
using CommunityToolkit.Mvvm.Messaging;
using GitControl.Messages;

namespace GitControl.ViewModel
{
    public partial class RepositoriesModel : ObservableRecipient
    {

        [ObservableProperty]
        private bool initialized = false;

        [ObservableProperty]
        private bool waiting = false;

        private bool IsReady => Initialized == true &&
                                Waiting == false;
        public ObservableCollection<Repository> Repositories { get; set; }
        public Repository? CurrentActiveRepository { get; set; } = null;

        public RepositoriesModel()
        {
            Task task = Task.Factory.StartNew(() =>
            {
                WeakReferenceMessenger.Default.Send(new DataRequestMessage(this, Data.Repositories));

            }
            );

            //WeakReferenceMessenger.Default.Register<Messages.RepositoriesUpdated>(this, (r, m) =>
            //{
            //    UpdateRepositoriesCollection();
            //    OnPropertyChanged(nameof(Repositories));
            //});
        }

        [RelayCommand]
        private void Loaded()
        {
            //MainWindowModel.UserProfile.ValidateRepositories();
           // UpdateRepositoriesCollection();

            //WeakReferenceMessenger.Default.Send(new DataRequestMessage(this, Data.Repositories));

        }
        private void UpdateRepositoriesCollection()
        {
            //Repositories = MainWindowModel.UserProfile.repositories.ToObservableCollection();
            CurrentActiveRepository = Repositories.FirstOrDefault(x => x.active == true);
        }

        [RelayCommand]
        private void RowAction(object parameter)
        {
            if (parameter is Repository)
            {
                var repository = (Repository) parameter;
                if (CurrentActiveRepository != null)
                {
                    CurrentActiveRepository.active = false;
                }
                CurrentActiveRepository = repository;
                CurrentActiveRepository.active = true;
                OnPropertyChanged(nameof(Repositories));
            }

        }
        [RelayCommand]
        private void CreateLocalRepository()
        {
            WeakReferenceMessenger.Default.Send(new DataUpdateMessage(Data.DialogTypeControl, GitControlDialogs.AddLocalRepository));
        }
        [RelayCommand]
        private void AddExistingRepository()
        {
            WeakReferenceMessenger.Default.Send(new DataUpdateMessage(Data.DialogTypeControl, GitControlDialogs.AddExistingRepository));
            // WeakReferenceMessenger.Default.Send(new Messages.ActivateDialogMessage(GitControl.View.Dialogs.GitControlDialogs.AddExistingRepository));
        }
        [RelayCommand]
        private void CloneRemoteRepository()
        {
           // WeakReferenceMessenger.Default.Send(new Messages.ActivateDialogMessage(GitControl.View.Dialogs.GitControlDialogs.CloneRemoteRepository));
        }
    }
}
