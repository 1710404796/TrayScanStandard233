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
using TrayScanStandard.Models.CZPallet;


namespace TrayScanStandard.View.CZPallet
{
    /// <summary>
    /// BatteryRuleControl.xaml 的交互逻辑
    /// </summary>
    public partial class BatteryRuleControl : UserControl, IDisposable
    {
        private bool _disposed = false;

        public Action<BatteryRuleControl> OnDelete { get; set; }
        public BatteryRuleControl(BatteryTypeRule item)
        {
            Item = item;
            DataContext = this;
            InitializeComponent();
        }

        public BatteryTypeRule Item { get; }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show(Properties.Resources.AreYouSureYouWantToDeleteThisRecord, Properties.Resources.DeleteConfirmation, MessageBoxButton.YesNo);
            if (res == MessageBoxResult.Yes)
            {
                OnDelete?.Invoke(this);

            }
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
                    // Clear the action delegate to release potential references
                    OnDelete = null;
                    
                    // Clear bindings
                    BindingOperations.ClearAllBindings(this);
                    DataContext = null;
                }
                _disposed = true;
            }
        }
    }
}
