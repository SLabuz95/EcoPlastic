using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitControl
{
    using AppDataUserProfile =  AppDataProperty<Config.UserProfile>;
    using AppDataRepositoryList = AppDataProperty<
                ObservableCollection<Config.Repository>>;
    internal class AppDataProperty<Type>
    {
        public AppDataProperty(Type? data) {
            this.data = data;
        }
        public Type? data { get; set; } = default(Type?);
    }
    public class AppData
    {
        public AppData() { }
        private AppDataUserProfile SelectedUserProfile { get; set; } = new AppDataUserProfile(null);
        

    }
}
