using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Runtime.Serialization;
using System.Windows.Shapes;
using LibGit2Sharp;
using GitControl.Extentions;


namespace GitControl.Config
{
    [DataContract]
    public class UserSettings
    {
        [DataMember]
        public List<UserProfile> userProfiles { get; set; }
    }
    [DataContract]
    public class UserProfile
    {
        
        [DataMember]
        public string NetId { get; set; }
        [DataMember]
        public List<Repository> repositories { get; set; } = new List<Repository>();
        public void ActiveRepository(string name)
        {
            var previousActiveRepository = repositories.FirstOrDefault(x=>x.active == true);
            if (previousActiveRepository != null)
                previousActiveRepository.active = false;
            var newActiveRepository = repositories.FirstOrDefault(x => x.Name.Equals(name));
            if (newActiveRepository != null)
                newActiveRepository.active = true;
        }
        public void ValidateRepositories()
        {
            foreach (var repository in repositories)
            {
                repository.MarkedToRemove = ValidateRepository(repository) == false;
            }
            repositories = repositories.Where(x => x.MarkedToRemove == false).ToList();
        }
        private bool ValidateRepository(Repository repository)
        {
            // Check if path exists
            if (Directory.Exists(repository.Path) == false)
            {
                return false;
            }
            // Check if repository exists
            try
            {
                using (var repo = new LibGit2Sharp.Repository(repository.Path))
                {
                    if (repo.Network.Remotes.Count() > 1)
                    {
                        //RepositoryPathErrorMessage = "Selected git repository contains more than 1 remote repository. It's not supported.";
                        return false;
                    }
                }
                // Repository exists - ok
            }
            catch (LibGit2Sharp.RepositoryNotFoundException)
            {
                // RepositoryPathErrorMessage = "Provided path doesnt contain git repository";
                return false;
            } 
            return true;
        }
    }
    [DataContract]
    public class Repository
    {
        [DataMember]
        public bool active = false;
        public string Active
        {
            get
            {
                return active ? "Active" : "Inactive";
            }
        }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Path { get; set; }
        public string RemoteName { get; set; }
        public string RemotePath { get; set; }
        public bool MarkedToRemove { get; set; } = false;
    }

    public class SettingsManager<T> where T : class
    {
        private readonly string _filePath;

        public SettingsManager(string fileName)
        {
            _filePath = GetLocalFilePath(fileName);
        }

        private string GetLocalFilePath(string fileName)
        {
            //string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return System.IO.Path.GetFullPath(fileName);
        }
        public bool SettingsExist() => File.Exists(_filePath);
        public T LoadSettings()
        {
            return File.Exists(_filePath) ?
            JsonConvert.DeserializeObject<T>(File.ReadAllText(_filePath)):
            null;
        }

        public void SaveSettings(T settings)
        {
            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(_filePath, json);
            File.SetAttributes(_filePath, FileAttributes.ReadOnly);
        }
    }
}
