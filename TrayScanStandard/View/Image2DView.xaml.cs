using LinxUniverse.Algo.Common;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using TrayScanStandard.Attritubes;
using TrayScanStandard.Data;
using TrayScanStandard.Models;
using TrayScanStandard.ViewModel;

namespace TrayScanStandard.View
{
    /// <summary>
    /// Image2DView.xaml 的交互逻辑
    /// </summary>
    [PowerView(PowerEnum.扫码枪设置)]
    public partial class Image2DView : UserControl
    {

        public Image2DViewModel ViewModel { get; set; }

        private List<(Border, BarCodeRegionInfo)> _rois = [];
        private bool _isUnloading;

        Border? _nowBorder;


        public Image2DView(Image2DViewModel image2DViewModel, bool hidden = false)
        {
            DataContext = this;
            ViewModel = image2DViewModel;
            ViewModel.RefreshBatteryInfos();

            _context = App.GetService<LinxContext>();

            //ViewModel = App.GetService<Image2DViewModel>();
            InitializeComponent();
            if (hidden)
            {
                ImgBorder.Visibility = Visibility.Collapsed;
                Grid.SetColumnSpan(ImgGrid, 2);
            }

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _isUnloading = false;

            RefreshBorder();

            ViewModel.ColorUpdate += ViewModel_ColorUpdate;
            ViewModel_ColorUpdate();

            ViewModel.ResultUpdate += ViewModel_ResultUpdate;

        }

        private void ViewModel_ResultUpdate()
        {
            Dispatcher.Invoke(() =>
            {
                img2d.ResultCanvas.Children.Clear();
                resultRects.Clear();
                //foreach (var item in resultRects)
                //{
                //    img2d.ResultCanvas.Children.Remove(item);
                //}
                ViewModel.TempResult.IfSome(s => // 这里显示有点问题
                {
                    s.Codes.Iter(
                        c =>
                        {
                            var border = new Border()
                            {
                                Width = c.Rect.Rect.Width,
                                Height = c.Rect.Rect.Height,
                                LayoutTransform = new RotateTransform(c.Rect.Angle),
                                BorderBrush = Brushes.Aqua,
                                BorderThickness = new Thickness(5),
                                Margin = new Thickness(c.Rect.Rect.X, c.Rect.Rect.Y, 0 , 0),
                            };
                            img2d.ResultCanvas.Children.Add(border);
                            resultRects.Add(border);
                        }
                        );
                 
                });


            });
        }

        List<TextBlock> codes = [];
        List<Border> resultRects = [];
        private void ViewModel_ColorUpdate()
        {
            Dispatcher.Invoke(() =>
            {
                foreach (var item in codes)
                {
                    img2d.BorderCanvas.Children.Remove(item);
                }
                codes.Clear();
                foreach (var region in _rois)
                {
                    region.Item1.BorderBrush = ViewModel.Colors[region.Item2.ChannelIdx];
                    (region.Item1.Child as TextBlock).Foreground = ViewModel.Colors[region.Item2.ChannelIdx];
                    TextBlock textBlock = new TextBlock()
                    {
                        Text = ViewModel.Codes[region.Item2.ChannelIdx],
                        FontSize = 60,
                        TextWrapping = TextWrapping.Wrap,
                         Width = 500,
                        Foreground = ViewModel.Colors[region.Item2.ChannelIdx],
                        Margin = new Thickness(region.Item2.Left, region.Item2.Top + region.Item2.Height, 0, 0)
                    };

                    codes.Add(textBlock);
                    img2d.BorderCanvas.Children.Add(textBlock);
                }
            });
           
        }

        private void Capture_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SetCam_Click(object sender, RoutedEventArgs e)
        {
            if ( ViewModel.CameraSetting is not null)
            {
                //new BcrSettingWindow(ViewModel.BcrInfo).ShowDialog();
            if ( new BcrSettingWindow(ViewModel.CameraSetting).ShowDialog()??false) MainStorage.SaveManager.Save();

            }
        }

        private void AddBorder_Click(object sender, RoutedEventArgs e)
        {
            Border border = CreateBorder();
            img2d.BorderCanvas.Children.Add(border);
            BarCodeRegionInfo barCodeRegionInfo = new();

            var lastBordor = _rois.LastOrDefault().Item2;
            if (lastBordor is not null) {
                barCodeRegionInfo.Width = lastBordor.Width;
                barCodeRegionInfo.Height = lastBordor.Height;
                border.Width = lastBordor.Width;
                border.Height = lastBordor.Height;
            }


            border.Tag = barCodeRegionInfo;
            barCodeRegionInfo.ChannelIdx = (_rois.LastOrDefault().Item2?.ChannelIdx + 1) ?? + 1;
            (border.Child as TextBlock).Text = barCodeRegionInfo.ChannelIdx.ToString();
            UpdateBorderThickness(border);
            _rois.Add((border, barCodeRegionInfo));
        }       
        
        
        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectBattery is null)
            {
                return;
            }

            ViewModel.SelectBattery.Regions[ViewModel.CameraIdx - 1].Clear();
            //_context.SaveChanges();
            ViewModel.SelectBattery.Regions[ViewModel.CameraIdx - 1].AddRange(_rois.Select(s => s.Item2).ToList());
            var cnt = ViewModel.LinxContext.SaveChanges();

            MainStorage.SelectBattery = ViewModel.SelectBattery;
            
            // 标记已保存
            ViewModel.MarkAsSaved();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUnloading)
            {
                return;
            }

            // 记得保存一下
            RefreshBorder();
        }

        private void RefreshBorder()
        {
            if (_isUnloading || ViewModel.SelectBattery is null)
            {
                return;
            }

            img2d.BorderCanvas.Children.Clear();
            CreateBorder();
            _rois.Clear();
            foreach (var regionInfo in ViewModel.SelectBattery.Regions[ViewModel.CameraIdx - 1])
            {
                var border = CreateBorder();

                border.Margin = new Thickness(regionInfo.Left, regionInfo.Top, 0, 0);
                border.Width = regionInfo.Width;
                border.Height = regionInfo.Height;

                (border.Child as TextBlock).Text = regionInfo.ChannelIdx.ToString();

                border.Tag = regionInfo; // 考虑加上颜色绑定

                img2d.BorderCanvas.Children.Add(border);
                UpdateBorderThickness(border);

                _rois.Add((border, regionInfo));
            }

           

        }



        private Border CreateBorder()
        {
            Border border = new()
            {
                BorderBrush = Brushes.Red,
                BorderThickness = new Thickness(20),
                Width = 600,
                Height = 600,
                Margin = new Thickness(100, 100, 100, 100),
                Background = Brushes.Transparent,
                Cursor = Cursors.Hand,

            };

            border.MouseRightButtonDown += Border_MouseRightButtonDown;
            border.MouseRightButtonUp  += Border_MouseRightButtonUp;

            border.MouseLeftButtonDown += Border_MouseLeftButtonDown;
            TextBlock textBlock = new()
            {
                Text = "0",
                Foreground = Brushes.Red,
                FontSize = 80,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            border.Child = textBlock;
            UpdateBorderThickness(border);
            
            // 可能需要直接显示通道号

            return border;
        }
        private void Border_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            // 阻止事件冒泡到父级控件
            e.Handled = true;

            if (_cancellationTokenSource is null)
                return;
            _cancellationTokenSource.Cancel();

            var border = (sender as Border);
            if (border?.Tag is BarCodeRegionInfo region)
            {
                _nowBorder = border;
                ViewModel.SelectBarCodeRegionInfo = region;

                region.Top = (int)border.Margin.Top;
                region.Left = (int)border.Margin.Left;
                region.Width = (int)border.Width;
                region.Height = (int)border.Height;

                ViewModel.RefreshBarCodeRegionData();
            }
        }
        private static void UpdateBorderThickness(Border border)
        {
            border.BorderThickness = new Thickness(Math.Max(border.Width, border.Height) / 100 * 3 + 6);
            (border.Child as TextBlock).FontSize = Math.Min(border.Width * 2 / 3, border.Height * 2 / 3) + 1;
        }
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //throw new NotImplementedException();
            var border = (sender as Border);
            var region = border.Tag as BarCodeRegionInfo;

            _nowBorder = border;
            ViewModel.SelectBarCodeRegionInfo = region;

            //ViewModel.RefreshBarCodeRegionData();

        }

        CancellationTokenSource _cancellationTokenSource;
        CancellationToken _cancellationToken;
        private LinxContext _context;

        private async void Border_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 阻止事件冒泡到父级控件（防止影响大图拖动）
            e.Handled = true;

            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
            Border? button = (sender as Border);

            if (button == null) return;

            // 获取相对于 Canvas 的初始位置
            var canvas = button.Parent as Canvas;
            if (canvas == null) return;

            var initialPosition = e.GetPosition(canvas);
            var initialMargin = button.Margin;

            while (!_cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // 获取当前鼠标位置（相对于 Canvas）
                    var p = e.GetPosition(button.Parent as Canvas);
                    Debug.WriteLine(p);
                    var mar = button.Margin;
                    mar.Left = p.X - button.Width / 2;
                    mar.Top = p.Y - button.Height / 2;
                    button.Margin = mar;

                    await Task.Delay(10, _cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }        private async void TopBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUnloading)
            {
                return;
            }

            if (ViewModel.SelectBarCodeRegionInfo is null)
            {
                return;
            }

            if (_nowBorder is null)
            {
                return;
            }

            await Task.Delay(20);
            if (_isUnloading || ViewModel.SelectBarCodeRegionInfo is null || _nowBorder is null)
            {
                return;
            }

            _nowBorder.Margin = new Thickness(ViewModel.SelectBarCodeRegionInfo.Left, ViewModel.SelectBarCodeRegionInfo.Top, 0, 0);

            _nowBorder.Width = ViewModel.SelectBarCodeRegionInfo.Width;
            _nowBorder.Height = ViewModel.SelectBarCodeRegionInfo.Height;
            UpdateBorderThickness(_nowBorder);
            
            // 标记有未保存的修改
            ViewModel.MarkAsChanged();
        }        private async void ChannelBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUnloading)
            {
                return;
            }

            if (ViewModel.SelectBarCodeRegionInfo is null)
            {
                return;
            }
            if (_nowBorder is null)
            {
                return;
            }

            await Task.Delay(20);
            if (_isUnloading || ViewModel.SelectBarCodeRegionInfo is null || _nowBorder is null)
            {
                return;
            }

            (_nowBorder.Child as TextBlock).Text = ViewModel.SelectBarCodeRegionInfo.ChannelIdx.ToString();
            
            // 标记有未保存的修改
            ViewModel.MarkAsChanged();
        }private void Delete_Border_Click(object sender, RoutedEventArgs e)
        {
            DeleteBorder(_nowBorder);
            _nowBorder = null;
            ViewModel.SelectBarCodeRegionInfo = null;
        }     
        
        private void ClearAllBoxes_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("确定要清理所有选定框吗？此操作将删除所有ROI选择框。", "确认操作", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            // 清除所有边框
            var bordersToRemove = _rois.ToList(); // 创建副本以避免在迭代过程中修改集合
            foreach (var (border, regionInfo) in bordersToRemove)
            {
                DeleteBorder(border);
            }

            // 清除选中状态
            _nowBorder = null;
            ViewModel.SelectBarCodeRegionInfo = null;
        }

        private void DeleteBorder(Border? border)
        {
            if (border is null)
            {
                return;
            }
            img2d.BorderCanvas.Children.Remove(border);


            border.MouseRightButtonDown -= Border_MouseRightButtonDown;
            border.MouseRightButtonUp -= Border_MouseRightButtonUp;
            border.MouseLeftButtonDown -= Border_MouseLeftButtonDown;
            _rois.Remove(_rois.Find(s => s.Item1 == border));

        }        
        
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _isUnloading = true;
            _cancellationTokenSource?.Cancel();

            // 检查是否有未保存的修改
            if (ViewModel.HasUnsavedChanges)
            {
                var result = MessageBox.Show(
                    "检测到有未保存的修改，是否保存设置？", 
                    "保存确认", 
                    MessageBoxButton.YesNoCancel, 
                    MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    // 自动保存
                    SaveBtn_Click(this, new RoutedEventArgs());
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    // 用户取消，这里我们只能记录，因为Unloaded事件无法阻止页面切换
                    // 但我们可以提醒用户修改已丢失
                    MessageBox.Show(
                        "注意：未保存的修改将丢失！", 
                        "提示", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Warning);
                }
                ViewModel.MarkAsSaved(); // 标记为已保存，避免重复提示
                // 如果选择No，则不保存，直接离开
            }
            img2d.Source = null!;
            img2d.BorderCanvas.Children.Clear();
            img2d.ResultCanvas.Children.Clear();


            //foreach (var item in _rois)
            //{
            //    DeleteBorder(item.Item1);
            //}
            _rois.Clear();
            _nowBorder = null!;

            ViewModel.ColorUpdate -= ViewModel_ColorUpdate;
            ViewModel.ResultUpdate -= ViewModel_ResultUpdate;
        }
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            //MainStorage.Saves.ScanRatios[ViewModel.CameraIdx - 1].OkCnt = MainStorage.Saves.ScanRatios[ViewModel.CameraIdx - 1].ScanCnt = 0;
            //ViewModel.UpdateRatio();
        }

        private void Capture_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private async void AutoRoi_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("确定要自动生成ROI吗？此操作将覆盖当前所有ROI设置。", "确认操作", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }
            await ViewModel.AutoROI();
            RefreshBorder();
        }

        private async void SortRoi_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("确定要自动排序ROI吗？此操作将根据当前设置的通道顺序重新排列所有ROI。", "确认操作", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }
            await ViewModel.AutoSortROI();
            RefreshBorder();
        }        private void ApplyBatchSize_Click(object sender, RoutedEventArgs e)
        {
            // 批量应用宽高到所有边框
            foreach (var (border, regionInfo) in _rois)
            {
                border.Width = ViewModel.BatchWidth;
                border.Height = ViewModel.BatchHeight;
                
                regionInfo.Width = ViewModel.BatchWidth;
                regionInfo.Height = ViewModel.BatchHeight;
                
                UpdateBorderThickness(border);
            }
            
            // 如果当前有选中的边框，更新ViewModel中的数据
            if (ViewModel.SelectBarCodeRegionInfo != null)
            {
                ViewModel.RefreshBarCodeRegionData();
            }
            
            // 标记有未保存的修改
            ViewModel.MarkAsChanged();
        }

        // In Image2DView class (if it contains images)
        public void Dispose()
        {
            // Clear any image sources
            if (img2d != null)
            {
                img2d.Source = null;
            }
            
            // Remove event handlers
            // ...
            
            // Clear references to viewmodels
            DataContext = null;
        }
    }
}
