using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Windows.Input;
using GitControl.Config;
using System.Windows.Controls;
using LibGit2Sharp;
using System.Net;
using CredentialManagement;
using LibGit2Sharp.Handlers;
using GitControl.View.Dialogs;
using CommunityToolkit.Mvvm.Messaging;
using System.Runtime.CompilerServices;
using System.Windows.Documents;
using GitControl.Messages;
using System.CodeDom;
using System.Windows;
using System.Xml;

namespace GitControl.ViewModel
{
    using DataRequestMainWindowModelFunc = Func<MainWindowModel, DataRequestMessage, DataResponseMessage?>;
    using DataUpdateMainWindowModelFunc = Func<MainWindowModel, DataUpdateMessage, bool>;
    [ObservableObject]
    public partial class MainWindowModel
    {
        public ObservableCollection<GitFeature> GitFeatures { get; set; }

        [ObservableProperty]
        public GitFeature selectedGitFeature;

        [ObservableProperty]
        private GitControlDialogs selectedDialog = GitControlDialogs.None;

       partial void OnSelectedDialogChanged(GitControlDialogs value)
        {
            WeakReferenceMessenger.Default.Send(new DataUpdatedMessage(Data.DialogTypeControl, value));
        }

        //static public UserSettings _userSettings { get; set; }
        //static public UserSettings _userSettings { get; set; }
        private UserProfile _currentUserProfile {  get; set; }
        
        private UserProfile currentUserProfile {
            get { return _currentUserProfile; }
            set 
            {
                _currentUserProfile = value;
                WeakReferenceMessenger.Default.Send(new DataUpdatedMessage(Data.UserProfile, _currentUserProfile));
            } 
        }

        private SettingsManager<UserSettings> settingsManager { get; set; }

        
        readonly private Dictionary<Data, DataRequestMainWindowModelFunc> dataRequestFuncMap =
            new ()
            {
                { 
                    Data.Repositories,
                    (MainWindowModel model, DataRequestMessage message) =>
                    {
                        return message.responseToMessage(model.currentUserProfile.repositories);
                    }
                }
            };
        readonly private Dictionary<Data, DataUpdateMainWindowModelFunc> dataUpdateFuncMap =
            new()
            {
                {
                    Data.Repositories,
                    (MainWindowModel model, DataUpdateMessage m) =>
                    {
                        if(m.Data is List<Config.Repository>)
                        {
                            var data = m.Data as List<Config.Repository>;
                            model.currentUserProfile.repositories = data;

                            return true;
                        }
                        return false;
                    }
                },
                {
                    Data.DialogTypeControl,
                    (MainWindowModel model, DataUpdateMessage m) =>
                    {
                        if(m.Data is GitControlDialogs && m.Data != null)
                        {
                            var data = m.Data as GitControlDialogs?;
                            if(data != null)
                            {
                                model.SelectedDialog = data.Value;
                            }

                            return true;
                        }
                        return false;
                    }
                }
            };

        public MainWindowModel()
        {
            try
            {
                //XmlReaderSettings settings = new XmlReaderSettings();
                //settings.XmlResolver = new XmlUrlResolver();  // Umożliwia wczytywanie zewnętrznych plików (np. DTD, XSD)
                //settings.DtdProcessing = DtdProcessing.Parse;  // Umożliwia wczytywanie encji zewnętrznych
                //XmlReader reader = XmlReader.Create("Logging\\exceptions.xml", settings);
                //XmlDocument doc = new XmlDocument();
                //doc.Load(reader);
                
                //if (Application.Current != null)
                //{
                //    Application.Current.DispatcherUnhandledException += (s, a) =>
                //    {
                //        //Reporter.AddLog(a.Exception);

                //        //DisplayAppropriateNotification(a);

                //        a.Handled = true;
                //    };
                //}

                WeakReferenceMessenger.Default.Register<DataRequestMessage>(this, (r, m) =>
                {
                    DataRequestMainWindowModelFunc? func = null;
                    if (dataRequestFuncMap.TryGetValue(m.Type, out func) == true && func != null && r is MainWindowModel)
                    {
                        var Response = func((MainWindowModel)r, m);
                        if (Response != null)
                        {
                            WeakReferenceMessenger.Default.Send(Response);
                        }
                    }
                    else
                    {
                        // Error
                    }

                });
                WeakReferenceMessenger.Default.Register<DataUpdateMessage>(this, (r, m) =>
                {
                    DataUpdateMainWindowModelFunc? func = null;
                    if (dataUpdateFuncMap.TryGetValue(m.Type, out func) == true && func != null && r is MainWindowModel)
                    {
                        var success = func((MainWindowModel)r, m);
                        if (success == false)
                        {
                           // No success in update
                        }
                    }
                    else
                    {
                        // Error
                    }

                });
                GitFeatures = new ()
                {
                };

                settingsManager = new SettingsManager<UserSettings>("UserSettings.json");
                UserSettings? Settings = null;//LoadSettings();
                // ValidateRepositories();
                if (Settings == null)
                {
                    Settings = new ()
                    {
                        userProfiles = new List<UserProfile>()
                        {
                            new UserProfile {
                                NetId = "pjtf85",
                                repositories =
                                new List<Config.Repository>
                                {
                                    
                                }
                            }

                        }
                    };
                    currentUserProfile = Settings.userProfiles[0];
                    SaveSettings();
                }
                //using (var repo = new LibGit2Sharp.Repository(@"C:\Users\Seba\Documents\TclParser"))
                //{
                //    Trace.WriteLine(repo.ToString());
                //    Trace.WriteLine(repo.Network.Remotes.ToList()[0].Name);
                    //    // Object lookup
                    //    var obj = repo.Lookup("sha");
                    //    var commit = repo.Lookup<Commit>("sha");
                    //    var tree = repo.Lookup<Tree>("sha");
                    //    //var tag = repo.Lookup("sha", ObjectType.Tag) as Tag;

                    //    // Rev walking
                    //    var commits = repo.Commits.ToList();
                    //    var trees = commits[0].Tree.ToList();
                    //    var tree2 = commits[0].Parents.ToList();
                    //    var sortedCommits = repo.Commits.QueryBy("sha", new CommitFilter { SortBy = CommitSortStrategies.Topological }).ToList();

                    //    //var creds = new UsernamePasswordCredentials()
                    //    //{
                    //    //    Username = "user",
                    //    //    Password = "pass"
                    //    //};
                    //    //CredentialsHandler credHandler = (_url, _user, _cred) => creds;
                    //    var remotes = repo.Network.Remotes.ToList();
                    //    var br = repo.Branches.ToList();
                    //    var remote2 = remotes[0].TagFetchMode.ToString();
                    //    var remote3 = remotes[0].FetchRefSpecs.ToList();
                    //    var remote4 = remotes[0].PushRefSpecs.ToList();
                   // }

            }catch (Exception e)
            {
                //throw;
            }
        
        }

        [RelayCommand]
        private void LoadSettings()
        {
            //_userSettings = _settingsManager.LoadSettings();
            //Apply(_userSettings);
        }
              
        [RelayCommand]
        private void SaveSettings()
        {
            //_settingsManager.SaveSettings(_userSettings);
        }

        private void Apply(UserSettings userSettings)
        {
            
        }

        [RelayCommand]
        private void RepositoriesClicked()
        {
            var tempFeature = new GitFeature("Repositories", GitFeatureType.Repositories);
            GitFeatures.Add(tempFeature);
            SelectedGitFeature = tempFeature;

        }
    }
}
