using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinxUniverse.CST;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TrayScanStandard.Models;
using TrayScanStandard.Models.CZPallet; // 已添加至 LightInfo 和 WcsSaves

namespace TrayScanStandard.ViewModel
{
    public partial class LightManagerViewModel : ObservableRecipient
    {
        private  WcsSaves _wcsSaves => MainStorage.Saves; // 假设 WcsSaves 已被注入或可访问。

        [ObservableProperty]
        private ObservableCollection<LightInfoViewModel> _lightInfos;        // 包装 LightInfo 的视图模型集合，便于数据绑定和 UI 交互

        [ObservableProperty]
        private ObservableCollection<string> _availableComPorts = new ObservableCollection<string>();       // 系统中可用的 COM 端口列表，用于下拉选择

        [ObservableProperty]
        private string _newComPort = string.Empty;      // 新增光源的 COM 端口输入

        [ObservableProperty]
        private string _newValuesString = string.Empty; // 逗号分隔值输入

        [ObservableProperty]
        private LightInfoViewModel? _selectedLightInfo;     // 当前选中的光源信息，用于编辑和删除操作

        [ObservableProperty]
        private string _editComPort = string.Empty;         // 编辑模式下的 COM 端口输入

        [ObservableProperty]
        private string _editValuesString = string.Empty;    // 编辑模式下的逗号分隔值输入

        [ObservableProperty]
        private string _errorMessage = string.Empty;        // 错误消息显示字段

        [ObservableProperty]
        private bool _isEditMode = false;                   // 是否处于编辑模式，控制 UI 显示和行为

        [ObservableProperty]
        private LightType _lightManagerType = LightType.Cognex;    // 当前选中的光源类型

        [ObservableProperty]
        private ObservableCollection<LightType> _lightTypes = new ObservableCollection<LightType>();    // 可选光源类型列表

        // 根据依赖注入配置进行调整
        public LightManagerViewModel()
        {
            LightInfos = new ObservableCollection<LightInfoViewModel>(
                _wcsSaves.LightInfos.Select(li => new LightInfoViewModel(li))
            );
            LightTypes = new ObservableCollection<LightType>(Enum.GetValues<LightType>());
            // 初始化可用的 COM 端口
            RefreshAvailableComPorts();
        }  

        [RelayCommand]
        private void RefreshAvailableComPorts()
        {
            try
            {
                // 获取系统中所有可用的 COM 端口
                var ports = System.IO.Ports.SerialPort.GetPortNames();

                // 清空现有集合并添加新端口
                AvailableComPorts.Clear();

                // 将所有找到的端口添加到集合中
                foreach (var port in ports.OrderBy(p => p))
                {
                    AvailableComPorts.Add(port);
                }
                
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"获取COM端口列表失败: {ex.Message}";
            }
        }

        [RelayCommand]
        private void AddLight()
        {
            if (string.IsNullOrWhiteSpace(NewComPort) || string.IsNullOrWhiteSpace(NewValuesString))
            {
                ErrorMessage = "COM端口和值必须填写";
                return;
            }

            try
            {
                var address = NewComPort.Trim();
                if (!TryValidateAddress(address, out var addressError))
                {
                    ErrorMessage = addressError;
                    return;
                }

                // 将逗号分隔的字符串解析为整数数组
                int[] values = NewValuesString.Split(',')
                                              .Select(s => int.Parse(s.Trim()))
                                              .ToArray();
                
                if (values.Length == 0)
                {
                    ErrorMessage = "至少需要一个光源值";
                    return;
                }

                // 检查通讯地址是否已存在
                if (LightInfos.Any(li => li.Com.Equals(address, StringComparison.OrdinalIgnoreCase)))
                {
                    ErrorMessage = $"COM端口 '{address}' 已经存在";
                    return;
                }

                var newLightInfo = new LightInfo(address, values, LightManagerType);
                LightInfos.Add(new LightInfoViewModel(newLightInfo));

                // 清除输入字段
                NewComPort = string.Empty;
                NewValuesString = string.Empty;
                ErrorMessage = string.Empty;
                SaveChanges();
            }
            catch (FormatException)
            {
                ErrorMessage = "光源值必须是以逗号分隔的整数";
            }
            catch (Exception)
            {
                ErrorMessage = "添加光源时发生错误";
            }
        }        

        [RelayCommand(CanExecute = nameof(CanDeleteLight))]
        private void DeleteLight()
        {
            if (SelectedLightInfo != null)
            {
                LightInfos.Remove(SelectedLightInfo);
                ErrorMessage = string.Empty;
                SaveChanges(); // 由 CollectionChanged 处理程序调用
            }
        }
        
        [RelayCommand(CanExecute = nameof(CanEditLight))]
        private void EditLight()
        {
            if (SelectedLightInfo == null || string.IsNullOrWhiteSpace(EditComPort) 
                || string.IsNullOrWhiteSpace(EditValuesString))
            {
                ErrorMessage = "请选择要编辑的光源并填写有效的值";
                return;
            }

            try
            {
                var address = EditComPort.Trim();
                if (!TryValidateAddress(address, out var addressError))
                {
                    ErrorMessage = addressError;
                    return;
                }

                // 将逗号分隔的字符串解析为整数数组
                int[] values = EditValuesString.Split(',')
                                               .Select(s => int.Parse(s.Trim()))
                                               .ToArray();
                
                if (values.Length == 0)
                {
                    ErrorMessage = "至少需要一个光源值";
                    return;
                }

                // 检查通讯地址是否已存在（当前选定项除外）
                if (LightInfos.Any(li => li != SelectedLightInfo && 
                                         li.Com.Equals(address, StringComparison.OrdinalIgnoreCase)))
                {
                    ErrorMessage = $"COM端口 '{address}' 已被其他光源使用";
                    return;
                }

                // 更新所选项目
                SelectedLightInfo.Com = address;
                SelectedLightInfo.Values = values;
                SelectedLightInfo.Type = LightManagerType; // 兼容无单独编辑UI：用当前下拉类型更新所选光源类型
                var a = SelectedLightInfo;
                // 强制刷新更新后的项目用户界面
                var index = LightInfos.IndexOf(SelectedLightInfo);

                LightInfos.RemoveAt(index);
                LightInfos.Insert(index, a);
                
                IsEditMode = false;
                ErrorMessage = string.Empty;

                SaveChanges();

            }
            catch (FormatException)
            {
                ErrorMessage = "光源值必须是以逗号分隔的整数";
            }
            catch (Exception)
            {
                ErrorMessage = "编辑光源时发生错误";
            }
        }
        
        private bool CanEditLight() => SelectedLightInfo != null;private bool CanDeleteLight() => SelectedLightInfo != null;

        partial void OnSelectedLightInfoChanged(LightInfoViewModel? value)
        {
            // 选择发生变化时更新 CanExecute 状态
            DeleteLightCommand.NotifyCanExecuteChanged();
            EditLightCommand.NotifyCanExecuteChanged();
            
            if (value != null)
            {
                // 加载所选灯光的数值至编辑字段
                EditComPort = value.Com;
                EditValuesString = value.ValuesString;
                LightManagerType = value.Type; // 选择行时同步下拉类型，避免误以为类型未生效
            }
        }
        
        private void SaveChanges()
        {
            // 使用当前列表更新 WcsSaves 实例
            _wcsSaves.LightInfos = LightInfos.Select(vm => vm.ToModel()).ToArray();
            // 保存更改以保持其状态
            MainStorage.SaveManager.Save();
        }

        private static bool TryValidateAddress(string address, out string errorMessage)
        {
            if (!Enum.TryParse<SerialPortType>(address, true, out _))
            {
                errorMessage = $"光源端口仅支持 COM1-COM8，当前值: '{address}'";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }
    }

    // 用于LightInfo的简单视图模型封装器，用于处理未来可能出现的特定用户界面逻辑
    // 若后续需要内联编辑，则可能触发 INotifyPropertyChanged
    public partial class LightInfoViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _com;

        [ObservableProperty]
        private int[] _values;

        [ObservableProperty]
        private LightType _type = LightType.Cognex;

        // 值数组的显示字符串
        public string ValuesString => string.Join(", ", Values);

        public LightInfoViewModel(LightInfo model)
        {
            _com = model.Com;
            _values = model.Values;
            _type = model.Type;
        }

        public LightInfo ToModel() => new LightInfo(Com, Values, Type);

        // 如有需要，可覆盖 `对外部对象` 以在列表中获得更佳显示效果；不过 `DataGrid` 的列显示效果更为出色。
        public override string ToString() => $"COM: {Com}, Values: {ValuesString}";
    }
}