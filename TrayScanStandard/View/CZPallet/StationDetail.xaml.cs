using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MediatR;
using Microsoft.Extensions.Logging;
using TrayScanStandard.Models;
using TrayScanStandard.Models.CZPallet;


namespace TrayScanStandard.View.CZPallet
{
    /// <summary>
    /// DiaoSuDetail.xaml 的交互逻辑
    /// </summary>
    public partial class StationDetail : UserControl, IDisposable
    {
        private readonly IMediator meditor;
        //private readonly ILogger<StationDetail> logger;
        private bool _disposed = false;

        public XYLStation Station { get; set; }

        public StationDetail(XYLStation station)
        {
            Station = station;
            DataContext = this;
            InitializeComponent();
            PalletD.Station = station;
            meditor = App.GetService<IMediator>();
            //logger = App.GetService<ILogger<StationDetail>>();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //PalletD
            if (Station is not Pallet)
            {
                Pikachu.Visibility = Visibility.Collapsed;
                //BindWithHuman.Visibility = Visibility.Collapsed;
                ScanAgain.Visibility = Visibility.Collapsed;
                PalletCodeBlock.Visibility = Visibility.Collapsed;
                //Confirm.Visibility = Visibility.Collapsed;
                BanSaoma.Visibility = Visibility.Collapsed;
                Force.Visibility = Visibility.Collapsed;
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.NageTo<YWStageView>();
        }

        private void BindWithHuman_Click(object sender, RoutedEventArgs e)
        {
            // 可能要提供专门的界面
            if (MessageBox.Show("是否要开始人工绑定盘流程？", "组盘确认", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                //Station.ClearStage();
                //var dia = new ReadOtherInput("请扫托盘条码");
                //dia.ShowDialog();
                //// 此处还需校验托盘种类 // Todo：托盘种类可能对应电芯数量
                //Station.PalletCode = dia.Result;

                //for (int i = 0; i < Station.ChannelNum; i++)
                //{
                //    dia = new ReadOtherInput($"请扫通道{i + 1}电芯条码");
                //    dia.ShowDialog();
                //    // 失败提示 应该是，而不该直接ng
                //    Station.BindBattery(i, dia.Result, true);
                //}

                //MessageBox.Show("绑定完成！");

                //// 如果需要复核 则在复核时上传
                //// 考虑保存
                //this.Focus();
            }


            // 任务流程 1.扫托盘码 2. 阻盘开始 3. 无限绑定 4. 阻盘完成

            // WaitforPalletCode -> 检测pallet类型
            // 
        }

        private async void ScanAgain_Click(object sender, RoutedEventArgs e)
        {
            if (Station is Pallet aa)
            {
                // Todo: 提取方法
                ScanAgain.IsEnabled = false;
                // Todo： 思考
                //aa.PalletCode = await meditor.Send(new ScanPalletCodeCommand(aa.PalletId));
                ScanAgain.IsEnabled = true;

                //aa.Palletcode = await YWUtils.GetPalletCode(aa.PalletId);
                //if (aa.Palletcode == string.Empty)
                //{
                //    MessageBox.Show($"{aa.Name} 扫码失败，请重试...");

                //}
                //else
                //{
                //    (ViewModel.PalletViewModel.Pallet as Pallet)?.ReCheckCode();
                //    // 这里通知一下pallet重扫了
                //}
                //ScanAgain.IsEnabled = true;

            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show($"确认要清除此{Station.Name}记忆吗？", " 写入确认", MessageBoxButton.OKCancel);
            if (res != MessageBoxResult.OK) return;

            //App.GetService<ILogger<MainViewModel>>().LogInformation($"清除了{ViewModel.PalletViewModel.Pallet.Name}记忆");

            Station.ClearStage();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            //App.GetService<ILogger<MainViewModel>>().LogInformation($"人工确认异常条码点击");

            //(ViewModel.PalletViewModel.Pallet as Pallet)?.ReCheckCode();

        }

        private void Force_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("确认要强制写入条码吗？", " 写入确认", MessageBoxButton.OKCancel);
            if (res != MessageBoxResult.OK) return;
            var pall = Station as Pallet;
            if (pall != null)
            {
                //YWUtils.plcVM.FMSPLC.WritePalletCheckForce(pall.PalletId, pall.Palletcode);
            }
        }

        //private void BanSaoma_Click(object sender, RoutedEventArgs e)
        //{
        //    if ( ViewModel.PalletViewModel.Pallet is Pallet p)
        //    {
        //        p.BanSaoma = !(sender as CheckBox).IsChecked ?? false;
        //    }
        //}

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
                    // Dispose PalletD if it implements IDisposable
                    if (PalletD is IDisposable disposablePallet)
                    {
                        disposablePallet.Dispose();
                    }
                    
                    // Clear bindings
                    BindingOperations.ClearAllBindings(this);
                }
                _disposed = true;
            }
        }
    }
}
