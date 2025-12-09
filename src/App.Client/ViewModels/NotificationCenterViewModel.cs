using System.Collections.ObjectModel;
using System.Windows.Input;
using DeskAppKit.Core.Enums;
using DeskAppKit.Core.Interfaces;
using DeskAppKit.Core.Models;

namespace DeskAppKit.Client.ViewModels;

/// <summary>
/// 通知センター画面のViewModel
/// </summary>
public class NotificationCenterViewModel : ViewModelBase
{
    private readonly INotificationCenter _notificationCenter;
    private ObservableCollection<Notification> _notifications = new();
    private Notification? _selectedNotification;
    private bool _isLoading;
    private NotificationLevel? _selectedLevel;
    private bool? _isReadFilter;
    private string _searchText = string.Empty;
    private int _unreadCount;

    public NotificationCenterViewModel(INotificationCenter notificationCenter)
    {
        _notificationCenter = notificationCenter;

        // コマンド初期化
        LoadNotificationsCommand = new RelayCommand(async () => await LoadNotificationsAsync());
        MarkAsReadCommand = new RelayCommand(async () => await MarkAsReadAsync());
        MarkAllAsReadCommand = new RelayCommand(async () => await MarkAllAsReadAsync());
        DeleteNotificationCommand = new RelayCommand(async () => await DeleteNotificationAsync());
        ClearFilterCommand = new RelayCommand(ClearFilter);
        SearchCommand = new RelayCommand(async () => await LoadNotificationsAsync());

        // 通知追加イベントのハンドリング
        _notificationCenter.NotificationAdded += OnNotificationAdded;
    }

    public ObservableCollection<Notification> Notifications
    {
        get => _notifications;
        set
        {
            _notifications = value;
            OnPropertyChanged();
        }
    }

    public Notification? SelectedNotification
    {
        get => _selectedNotification;
        set
        {
            _selectedNotification = value;
            OnPropertyChanged();
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    public NotificationLevel? SelectedLevel
    {
        get => _selectedLevel;
        set
        {
            _selectedLevel = value;
            OnPropertyChanged();
        }
    }

    public bool? IsReadFilter
    {
        get => _isReadFilter;
        set
        {
            _isReadFilter = value;
            OnPropertyChanged();
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            OnPropertyChanged();
        }
    }

    public int UnreadCount
    {
        get => _unreadCount;
        set
        {
            _unreadCount = value;
            OnPropertyChanged();
        }
    }

    public ICommand LoadNotificationsCommand { get; }
    public ICommand MarkAsReadCommand { get; }
    public ICommand MarkAllAsReadCommand { get; }
    public ICommand DeleteNotificationCommand { get; }
    public ICommand ClearFilterCommand { get; }
    public ICommand SearchCommand { get; }

    /// <summary>
    /// 通知一覧を読み込む
    /// </summary>
    public async Task LoadNotificationsAsync()
    {
        try
        {
            IsLoading = true;

            var filter = new NotificationFilter
            {
                Level = SelectedLevel,
                IsRead = IsReadFilter,
                SearchText = SearchText
            };

            var notifications = await _notificationCenter.GetNotificationsAsync(filter);
            Notifications = new ObservableCollection<Notification>(notifications);

            UnreadCount = await _notificationCenter.GetUnreadCountAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"通知読み込みエラー: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 選択した通知を既読にする
    /// </summary>
    private async Task MarkAsReadAsync()
    {
        if (SelectedNotification == null) return;

        try
        {
            await _notificationCenter.MarkAsReadAsync(SelectedNotification.Id);
            await LoadNotificationsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"既読処理エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// すべての通知を既読にする
    /// </summary>
    private async Task MarkAllAsReadAsync()
    {
        try
        {
            await _notificationCenter.MarkAllAsReadAsync();
            await LoadNotificationsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"一括既読処理エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// 選択した通知を削除
    /// </summary>
    private async Task DeleteNotificationAsync()
    {
        if (SelectedNotification == null) return;

        try
        {
            await _notificationCenter.DeleteNotificationAsync(SelectedNotification.Id);
            await LoadNotificationsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"削除処理エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// フィルタをクリア
    /// </summary>
    private void ClearFilter()
    {
        SelectedLevel = null;
        IsReadFilter = null;
        SearchText = string.Empty;
    }

    /// <summary>
    /// 通知追加イベントハンドラ
    /// </summary>
    private async void OnNotificationAdded(object? sender, NotificationEventArgs e)
    {
        await LoadNotificationsAsync();
    }
}
