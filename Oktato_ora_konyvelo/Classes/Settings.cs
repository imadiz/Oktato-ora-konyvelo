using Avalonia.Metadata;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oktato_ora_konyvelo.Classes
{
    public partial class Settings : ObservableObject
    {
        [ObservableProperty]
        private int startKm;
        [ObservableProperty]
        private DateTime lastDate;
        [ObservableProperty]
        private string kVARLoginName;
        [ObservableProperty]
        private string kVARPassword;
        public Settings(string kvarlogin, string kvarpwd)
        {
            lastDate = DateTime.Today;
            kVARLoginName = kvarlogin;
            kVARPassword = kvarpwd;
        }
    }
}
