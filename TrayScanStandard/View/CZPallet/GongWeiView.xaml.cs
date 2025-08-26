using System;
using System.Windows.Controls;
using System.Windows.Data;
using TrayScanStandard.Models;

namespace TrayScanStandard.View.CZPallet
{
    /// <summary>
    /// GongWeiView.xaml 的交互逻辑
    /// </summary>
    public partial class GongWeiView : UserControl, IDisposable
    {
        //public GongWeiViewModel ViewModel { get; init; }

        public XYLStation Station { get; set; }
        private bool _disposed = false;

        public string Title { get; set; }
        public GongWeiView(XYLStation station)
        {
            Station = station;
            DataContext = this;
            //ViewModel = App.GetService<GongWeiViewModel>();
            Title = $"{Properties.Resources.DispatchNumber} ：{Station.InsideNum} {Station.Name} {Properties.Resources.WorkstationNumber}{Station.OutsideStationNum}";
            InitializeComponent();
            Pallet1.Station = Station;

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
                    // Dispose PalletView if it implements IDisposable
                    if (Pallet1 is IDisposable disposablePallet)
                    {
                        disposablePallet.Dispose();
                    }
                    
                    // Clear bindings
                    BindingOperations.ClearAllBindings(this);
                    DataContext = null;
                }
                _disposed = true;
            }
        }
    }
}
