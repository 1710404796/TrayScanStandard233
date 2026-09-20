using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using TrayScanStandard.Models;

namespace TrayScanStandard.ViewModel
{
    public partial class SettingViewModel(MainViewModel mainViewModel) : ObservableRecipient
    {
        public WcsSaves Saves => MainStorage.Saves;
        [ObservableProperty]
        bool _canEditing = true;
        public bool IsBackGround
        {
            get => Saves.BackGroundEnable == System.Windows.Visibility.Hidden;
            set
            {
                Saves.BackGroundEnable = value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                mainViewModel.HasBackGround = Saves.BackGroundEnable;
            }
        }

    }
}
