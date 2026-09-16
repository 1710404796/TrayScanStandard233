using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinxUniverse.Utils;
using Microsoft.EntityFrameworkCore;
using TrayScanStandard.Data;
using TrayScanStandard.Data.Models;
using TrayScanStandard.Models.CZPallet;

namespace TrayScanStandard.ViewModel.CZPallet
{
    public partial class PalletLogViewModel(LinxContext context) : ObservableRecipient
    {
        private const int SinglePageSize = 100;
        private static int _lastPage = 1;

        [ObservableProperty] private ObservableCollection<PalletLogExt> _palletLogs = [];
        private List<PalletLog> _logs = [];

        [ObservableProperty] private int _currentPage = 1;
        [ObservableProperty] private int _totalPages = 1;
        [ObservableProperty] private int _totalCount;
        [ObservableProperty] private string _jumpPageText = "1";

        public DateTime StartTime { get; set; } = DateTime.Today.AddDays(-30);
        public DateTime EndTime { get; set; } = DateTime.Today.AddDays(1);

        public string Code { get; set; } = string.Empty;
        public int LogNum { get; set; } = SinglePageSize;

        public int PageSize => SinglePageSize;
        public bool CanPreviousPage => CurrentPage > 1;
        public bool CanNextPage => CurrentPage < TotalPages;

        public void RefreshContext()
        {
            Search();
        }

        public bool IsNowLog(WarningLog log)
        {
            return log.WarningTime >= StartTime && log.WarningTime <= EndTime;
        }

        [RelayCommand]
        public void ExportAll()
        {
            StringBuilder sb = new(1000);
            string[] titles = ["托盘编号", "组盘时间", "电池数量", "电池条码"];

            sb.AppendLine(string.Join(",", titles));

            foreach (var log in PalletLogs)
            {
                sb.AppendLine($"{log.PalletCode},{log.ZuPanTime:yyyy-MM-dd HH:mm:ss},{log.ChannelCount},[{string.Join("|", log.BatteryInfo.Select(s => s.BatteryCode))}]");
            }

            string fileName = $"InsertLog/exportall-{FilenameHelper.FileName}.csv";
            System.IO.File.WriteAllBytes(fileName, Encoding.GetEncoding("gb2312").GetBytes(sb.ToString()));
            MessageBox.Show($"export to {fileName}");
        }

        [RelayCommand]
        public void Export()
        {
            StringBuilder sb = new(1000);
            string[] titles = ["托盘编号", "组盘时间", "电池数量", "电池条码"];

            sb.AppendLine(string.Join(",", titles));

            foreach (var log in PalletLogs.Where(s => s.IsSelect))
            {
                sb.AppendLine($"{log.PalletCode},{log.ZuPanTime:yyyy-MM-dd HH:mm:ss},{log.ChannelCount},[{string.Join("|", log.BatteryInfo.Select(s => s.BatteryCode))}]");
            }

            string fileName = $"InsertLog/export-{FilenameHelper.FileName}.csv";
            System.IO.File.WriteAllBytes(fileName, Encoding.GetEncoding("gb2312").GetBytes(sb.ToString()));
            MessageBox.Show($"export to {fileName}");
        }

        [RelayCommand]
        public void Search()
        {
            List<PalletLog> source;
            lock (context)
            {
                source = context.PalletLogs.AsNoTracking()
                    .Where(s => s.PalletType == PalletType.组盘)
                    .OrderByDescending(s => s.Id)
                    .ToList();
            }

            var afterFilter = source.Where(s => s.ZuPanTime >= StartTime && s.ZuPanTime <= EndTime);

            if (!string.IsNullOrEmpty(Code))
            {
                afterFilter = afterFilter.Where(s => s.PalletCode.Contains(Code, StringComparison.InvariantCultureIgnoreCase));
            }

            _logs = afterFilter.ToList();
            TotalCount = _logs.Count;
            TotalPages = Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
            CurrentPage = Math.Max(1, Math.Min(_lastPage, TotalPages));
            ApplyPagination();
        }

        [RelayCommand(CanExecute = nameof(CanPreviousPage))]
        private void PreviousPage()
        {
            CurrentPage--;
        }

        [RelayCommand(CanExecute = nameof(CanNextPage))]
        private void NextPage()
        {
            CurrentPage++;
        }

        [RelayCommand]
        private void JumpToPage()
        {
            if (!int.TryParse(JumpPageText, out int targetPage))
            {
                JumpPageText = CurrentPage.ToString();
                return;
            }

            if (targetPage < 1)
            {
                targetPage = 1;
            }

            if (targetPage > TotalPages)
            {
                targetPage = TotalPages;
            }

            CurrentPage = targetPage;
            JumpPageText = CurrentPage.ToString();
        }

        partial void OnCurrentPageChanged(int value)
        {
            _lastPage = CurrentPage;
            ApplyPagination();
            JumpPageText = CurrentPage.ToString();
            OnPropertyChanged(nameof(CanPreviousPage));
            OnPropertyChanged(nameof(CanNextPage));
            PreviousPageCommand.NotifyCanExecuteChanged();
            NextPageCommand.NotifyCanExecuteChanged();
        }

        partial void OnTotalPagesChanged(int value)
        {
            OnPropertyChanged(nameof(CanNextPage));
            NextPageCommand.NotifyCanExecuteChanged();
        }

        private void ApplyPagination()
        {
            if (CurrentPage < 1)
            {
                CurrentPage = 1;
                return;
            }

            if (CurrentPage > TotalPages)
            {
                CurrentPage = TotalPages;
                return;
            }

            PalletLogs = new(_logs
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .Select(s => new PalletLogExt(s)));
        }
    }
}
