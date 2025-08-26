using System;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LinxUniverse.DI;
using MediatR;
using TrayScanStandard.Attritubes;
using TrayScanStandard.Mediator.Queries;
using TrayScanStandard.View;
using TrayScanStandard.View.CZPallet;
using TrayScanStandard.ViewModel;

namespace TrayScanStandard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainViewModel ViewModel
        {
            get; set;
        }
        public MainWindow()
        {
            MainStorage.SaveManager.Load();
            ChangeLanguage();

            DataContext = this;
            ViewModel = App.GetService<MainViewModel>();
            InitializeComponent();
        }

        private static void ChangeLanguage()
        {
            switch (MainStorage.Saves.Lang)
            {
                case 0:
                    Properties.Resources.Culture = new System.Globalization.CultureInfo("zh-CN");
                    break;
                case 1:
                    Properties.Resources.Culture = new System.Globalization.CultureInfo("en-US");
                    break;
                case 2:
                    Properties.Resources.Culture = new System.Globalization.CultureInfo("hu");
                    break;
                default:
                    break;
            }
        }

        //public static void NageTo<T>() where T : FrameworkElement
        //{
        //    (App.Current.MainWindow as MainWindow)!.ContentFrame.Content = App.GetService<T>();
        //}
        public static async void NageTo(FrameworkElement frameworkElement)
        {
            var mediator1 = App.GetService<IMediator>();
            var mainWindow = (App.Current.MainWindow as MainWindow)!;

            var tt = frameworkElement.GetType().GetCustomAttribute<PowerViewAttribute>();
            if (tt != null)
            {
                if (await mediator1.Send(new CheckPowerQuery(tt.Power)))
                {

                }
                else
                {
                    MessageBox.Show("当前没有权限访问此页面!");
                    DisposeCurrentContent(mainWindow);
                    mainWindow.ContentFrame.Content = null;
                    return;
                }
            }
            
            // Dispose of the previous content to prevent memory leaks
            DisposeCurrentContent(mainWindow);
            mainWindow.ContentFrame.Content = frameworkElement;
        }

        public static async void NageTo<T>() where T : FrameworkElement
        {
            var mediator1 = App.GetService<IMediator>();
            var mainWindow = (App.Current.MainWindow as MainWindow)!;

            var tt = typeof(T).GetCustomAttribute<PowerViewAttribute>();
            if (tt != null)
            {
                if (await mediator1.Send(new CheckPowerQuery(tt.Power)))
                {

                }
                else
                {
                    MessageBox.Show("当前没有权限访问此页面!");
                    DisposeCurrentContent(mainWindow);
                    mainWindow.ContentFrame.Content = null;
                    return;
                }
            }
            
            if (mainWindow.ContentFrame.Content is not T)
            {
                // Dispose of the previous content to prevent memory leaks
                DisposeCurrentContent(mainWindow);
                mainWindow.ContentFrame.Content = App.GetService<T>();
            }
        }

        /// <summary>
        /// Helper method to properly dispose of current content to prevent memory leaks
        /// </summary>
        /// <param name="mainWindow">The main window instance</param>
        private static void DisposeCurrentContent(MainWindow mainWindow)
        {
            if (mainWindow.ContentFrame.Content is IDisposable disposableContent)
            {
                disposableContent.Dispose();
            }
            else if (mainWindow.ContentFrame.Content is FrameworkElement element)
            {
                // Clear data context and bindings for non-disposable elements
                BindingOperations.ClearAllBindings(element);
                element.DataContext = null;
            }
        }


        private void LogDash_Click(object sender, RoutedEventArgs e)
        {
            NageTo<LogDashBoardView>();
        }

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            NageTo<SettingView>();

        }

        private void PowerManager_Click(object sender, RoutedEventArgs e)
        {
            NageTo<UserManagerView>();

        }
        private void Window_Closed(object sender, EventArgs e)
        {
            // Cleanup ViewModel resources
            ViewModel.Cleanup();
            ViewModel.CacheService.Cancel();

            // Dispose current content to prevent memory leaks
            DisposeCurrentContent(this);

            MainStorage.SaveManager.Save();
        }

        private void SelectLangBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainStorage.Saves.Lang == SelectLangBox.SelectedIndex)
            {
                return;
            }



            MainStorage.Saves.Lang = SelectLangBox.SelectedIndex;
            MainStorage.SaveManager.Save();

            switch (SelectLangBox.SelectedIndex)
            {
                case 0:
                    Properties.Resources.Culture = new System.Globalization.CultureInfo("zh-CN");
                    break;
                case 1:
                    Properties.Resources.Culture = new System.Globalization.CultureInfo("en-US");
                    break;
                case 2:
                    Properties.Resources.Culture = new System.Globalization.CultureInfo("hu");
                    break;
                default:
                    break;
            }

            MessageBox.Show(Properties.Resources.PleaseRestartTheSoftwareForTheChangesToTakeEffect, Properties.Resources.Prompt, MessageBoxButton.OK, MessageBoxImage.Information);

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SelectLangBox.SelectedIndex = MainStorage.Saves.Lang;
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            App.GetService<CacheService>().Cancel();

            var mediator = App.GetService<IMediator>();
            var cc = MessageBox.Show("确定要关闭吗", "关闭确认", MessageBoxButton.OKCancel);
            if (cc == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            if (await mediator.Send(new CheckPowerQuery(PowerEnum.关闭软件)))
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
                MessageBox.Show("您当前没有权限关闭软件");
            }
        }

        private void StationView_Click(object sender, RoutedEventArgs e)
        {
            NageTo<YWStageView>();

        }
    }
}