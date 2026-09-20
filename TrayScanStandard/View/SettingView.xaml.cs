using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.ComponentModel;
using TrayScanStandard.View.CZPallet;
using TrayScanStandard.View.User;
using TrayScanStandard.ViewModel;

namespace TrayScanStandard.View
{
    /// <summary>
    /// SettingView.xaml 的交互逻辑
    /// </summary>
    public partial class SettingView : Page
    {

        public SettingViewModel ViewModel
        {
            get;
        }

        public SettingView()
        {
            DataContext = this;
            ViewModel = App.GetService<SettingViewModel>();
            InitializeComponent();
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            MainStorage.SaveManager.Save();
            SaveButton.Content = "√ 保存成功";
            SaveButton.IsEnabled = false;
            await Task.Delay(1000);
            SaveButton.Content = "保存设定";

            SaveButton.IsEnabled = true;
        }

        /// <summary>
        /// 权限配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetPower_Click(object sender, RoutedEventArgs e)
        {
            new PowerSettingWindows().ShowDialog();

        }

        /// <summary>
        /// 工位配置
        /// </summary>
        /// <param name = "sender" ></ param >
        /// < param name="e"></param>
        //private void StationSetting_Click(object sender, RoutedEventArgs e)
        //{
        //    App.GetService<StationSettingView>().ShowDialog();

        //}
    }
}
