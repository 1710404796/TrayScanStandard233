using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using TrayScanStandard.Models;
using TrayScanStandard.Models.CZPallet;

namespace TrayScanStandard.View.CZPallet
{
    /// <summary>
    /// YMStageView.xaml 的交互逻辑
    /// </summary>
    public partial class YWStageView : UserControl, IDisposable
    {
        private readonly List<GongWeiView> _createdViews = new List<GongWeiView>();
        private bool _disposed = false;

        public YWStageView()
        {
            DataContext = this;
            //ViewModel = App.GetService<YWStageViewModel>();
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var station in MainStorage.Saves.Stage.Stations.OrderBy(s => s is Pallet ? 0 : 1).ThenBy(s =>
            {
                var p = s as Pallet;
                if (p != null)
                {
                    if (p.Type == PalletType.组盘)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return 2;
                }

            }))
            {
                if (station.IsEnable)
                {
                    var view = new GongWeiView(station);
                    SetViewStyle(view, station);
                }


            }

        }

        private void SetViewStyle(GongWeiView view, XYLStation xYLStation)
        {
            GongWeiSim.Children.Add(view);
            _createdViews.Add(view); // Track created views for cleanup
            
            if (xYLStation != null)
            {
                if (xYLStation is Pallet p)
                {
                    view.Width = 800;
                    view.Height = 800;
                }
                else
                {
                    view.Width = 300;
                    view.Height = 300;
                    view.GongweiTitle.FontSize = 12;
                }
            }
            else
            {
                view.Width = 550;
                view.Height = 550;
            }

            view.Margin = new Thickness(5, 10, 5, 10);
            view.Background = Brushes.Transparent;
            view.MouseDown += View_MouseDown;
            //view.ViewModel
        }

        private void View_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var obj = sender as GongWeiView;
            //var aa = new sta
            //{
            //    PalletViewModel = new PalletViewModel
            //    { Pallet = obj.ViewModel.DiaoDuStage }
            //};
            MainWindow.NageTo(new StationDetail(
                obj!.Station

            )
            );


        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show(Properties.Resources.DeleteConfirmation, "", MessageBoxButton.YesNo);
            if (res == MessageBoxResult.Yes)
            {
                foreach (var item in MainStorage.Saves.Stage.Stations)
                {
                    item.ClearStage();
                }
            }
        }

        private void CleanupViews()
        {
            foreach (var view in _createdViews)
            {
                if (view != null)
                {
                    // Remove event handler to prevent memory leaks
                    view.MouseDown -= View_MouseDown;
                    
                    // Dispose PalletView if it implements IDisposable
                    if (view.Pallet1 is IDisposable disposablePallet)
                    {
                        disposablePallet.Dispose();
                    }
                    
                    // Clear bindings
                    BindingOperations.ClearAllBindings(view);
                }
            }
            _createdViews.Clear();
            GongWeiSim?.Children.Clear();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    CleanupViews();
                    BindingOperations.ClearAllBindings(this);
                }
                _disposed = true;
            }
        }
    }
}
