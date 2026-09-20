
using System.Windows;
using System.Windows.Controls;
using TrayScanStandard;
using TrayScanStandard.Attritubes;
using TrayScanStandard.Data.Models;
using TrayScanStandard.ViewModel;
using TextBox = System.Windows.Controls.TextBox;

namespace TrayScanStandard.View;
[PowerView(PowerEnum.电芯信息管理)] // 迁移回主模板
public partial class BatteryManager : Page
{
    public BatteryManagerViewModel ViewModel { get; }
    public BatteryManager()
    {
        DataContext = this;
        ViewModel = App.GetService<BatteryManagerViewModel>();
        InitializeComponent();
    }


    private void DataGrid_OnAddingNewItem(object? sender, AddingNewItemEventArgs e)
    {
        // var np = new BatteryInfo();
        // ViewModel.RpContext.Pallets.Add(np);
        // e.NewItem = np;
    }

    private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        BatteryTypeInfo info = (BatteryTypeInfo)(sender as Button).Tag;

        //var nw = new PlcPointSettingWindow(info);
        //if (nw.ShowDialog() ?? false)
        //{
        //    info.Positions = info.Positions[..];
        //    ViewModel.SavePallet();
        //}
    }

    private void AlgoSetting_Click(object sender, RoutedEventArgs e)
    {
        Button button = sender as Button;
        var algo = button?.Tag as BatteryTypeInfo;
        if (algo == null)
        {
            return;
        }
        //AutoClassEditWindow autoClassEditWindow = new AutoClassEditWindow(algo.Param);
        //if (autoClassEditWindow.ShowDialog() ?? false)
        //{
        //    algo.Param = FormGenerator.GetUpdatedObject(autoClassEditWindow.AutoFormGeneratorControl, algo.Param);
        //}

        ViewModel.SavePallet();
    }

    //private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
    //{
    //    ViewModel.FilterBattery((sender as TextBox).Text);
    //}

    private void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
    {
        ViewModel.SavePallet();
    }

    private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        //ViewModel.SavePallet();

    }

    private void GZJBtn_OnClick(object sender, RoutedEventArgs e)
    {
        //new GZJRegionSettingView((sender as Button).Tag as BatteryInfo).ShowDialog();
        //ViewModel.SavePallet();
    }
}