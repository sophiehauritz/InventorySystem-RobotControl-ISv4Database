using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

using Avalonia.Threading;
using MySqlConnector; // <— make sure the package is installed: MySqlConnector

namespace InventorySystem.ViewModels
{
    // -----------------------------
    // Simple ICommand for buttons
    // -----------------------------
    public sealed class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute    = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object? parameter)    => _execute(parameter);
        public void RaiseCanExecuteChanged()      => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    // -----------------------------
    // View models for the grids
    // -----------------------------
    public sealed class ItemRow
    {
        public int Id { get; init; }
        public string Name { get; init; } = "";
        public decimal PricePerUnit { get; init; }
    }

    public sealed class OrderDisplay
    {
        public int Id { get; init; }
        public DateTime Time { get; init; }
        public int Quantity { get; init; }
        public string ItemName { get; init; } = "";
    }

    // -----------------------------
    // Main Window VM
    // -----------------------------
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        // ---- DB connection (match your local Docker container) ----
        // If you set a password, add ;Password=yourpass
        private const string ConnStr = "Server=127.0.0.1;Port=3306;Database=inventory;User ID=root;";

        private static async Task<MySqlConnection> OpenAsync()
        {
            var c = new MySqlConnection(ConnStr);
            await c.OpenAsync();
            return c;
        }

        // ---- Catalog for ComboBox ----
        public ObservableCollection<ItemRow> CatalogItems { get; } = new();

        private ItemRow? _selectedItem;
        public ItemRow? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (!Equals(_selectedItem, value))
                {
                    _selectedItem = value;
                    OnPropertyChanged();
                    UpdateButtons();
                }
            }
        }

        private int _newQuantity = 1;
        public int NewQuantity
        {
            get => _newQuantity;
            set
            {
                if (_newQuantity != value)
                {
                    _newQuantity = value;
                    OnPropertyChanged();
                    UpdateButtons();
                }
            }
        }

        // ---- Grids ----
        public ObservableCollection<OrderDisplay> QueuedOrders { get; } = new();
        public ObservableCollection<OrderDisplay> ProcessedOrders { get; } = new();

        // ---- Revenue label ----
        private decimal _totalRevenue;
        public decimal TotalRevenue
        {
            get => _totalRevenue;
            private set
            {
                if (_totalRevenue != value)
                {
                    _totalRevenue = value;
                    OnPropertyChanged();
                }
            }
        }

        // ---- Commands ----
        public RelayCommand AddOrderCommand { get; }
        public RelayCommand ProcessNextCommand { get; }

        public MainWindowViewModel()
        {
            AddOrderCommand    = new RelayCommand(_ => _ = AddOrderAsync(),    _ => CanAddOrder());
            ProcessNextCommand = new RelayCommand(_ => _ = ProcessNextAsync(), _ => CanProcessNext());

            // initial load
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                await LoadCatalogAsync();
                await RefreshUiFromDbAsync();
            }
            catch
            {
                // Keep the UI responsive even if DB momentarily fails.
            }
        }

        // -----------------------------
        // Loading data
        // -----------------------------
        private async Task LoadCatalogAsync()
        {
            var list = new System.Collections.Generic.List<ItemRow>();
            using (var conn = await OpenAsync())
            using (var cmd = new MySqlCommand("SELECT Id, Name, PricePerUnit FROM items ORDER BY Id;", conn))
            using (var r = await cmd.ExecuteReaderAsync())
            {
                while (await r.ReadAsync())
                {
                    list.Add(new ItemRow
                    {
                        Id = r.GetInt32(0),
                        Name = r.GetString(1),
                        PricePerUnit = r.GetDecimal(2)
                    });
                }
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                CatalogItems.Clear();
                foreach (var it in list) CatalogItems.Add(it);

                // pick first by default
                if (CatalogItems.Count > 0 && SelectedItem is null)
                    SelectedItem = CatalogItems[0];
            });
        }

        private async Task RefreshUiFromDbAsync()
        {
            var queued = new System.Collections.Generic.List<OrderDisplay>();
            var processed = new System.Collections.Generic.List<OrderDisplay>();
            decimal revenue = 0m;

            using (var conn = await OpenAsync())
            {
                // queued
                const string qSql = @"
                  SELECT o.Id, o.Time, ol.Quantity, i.Name
                  FROM orders o
                  JOIN order_lines ol ON ol.OrderId = o.Id
                  JOIN items i       ON i.Id = ol.ItemId
                  WHERE o.Status='Queued'
                  ORDER BY o.Time;
                ";
                using (var cmd = new MySqlCommand(qSql, conn))
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                    {
                        queued.Add(new OrderDisplay
                        {
                            Id = r.GetInt32(0),
                            Time = r.GetDateTime(1),
                            Quantity = r.GetInt32(2),
                            ItemName = r.GetString(3)
                        });
                    }
                }

                // processed
                const string pSql = @"
                  SELECT o.Id, o.Time, ol.Quantity, i.Name
                  FROM orders o
                  JOIN order_lines ol ON ol.OrderId = o.Id
                  JOIN items i       ON i.Id = ol.ItemId
                  WHERE o.Status='Processed'
                  ORDER BY o.Time DESC;
                ";
                using (var cmd = new MySqlCommand(pSql, conn))
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                    {
                        processed.Add(new OrderDisplay
                        {
                            Id = r.GetInt32(0),
                            Time = r.GetDateTime(1),
                            Quantity = r.GetInt32(2),
                            ItemName = r.GetString(3)
                        });
                    }
                }

                // revenue
                const string revSql = @"
                  SELECT COALESCE(SUM(ol.Quantity * i.PricePerUnit),0)
                  FROM orders o
                  JOIN order_lines ol ON ol.OrderId = o.Id
                  JOIN items i       ON i.Id = ol.ItemId
                  WHERE o.Status='Processed';
                ";
                using (var cmd = new MySqlCommand(revSql, conn))
                {
                    var val = await cmd.ExecuteScalarAsync();
                    revenue = Convert.ToDecimal(val ?? 0m);
                }
            }

            // ✅ Update UI on UI thread
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                QueuedOrders.Clear();
                foreach (var x in queued) QueuedOrders.Add(x);

                ProcessedOrders.Clear();
                foreach (var x in processed) ProcessedOrders.Add(x);

                TotalRevenue = revenue;
                UpdateButtons();
            });
        }

        // -----------------------------
        // Add & process orders
        // -----------------------------
        private bool CanAddOrder() => SelectedItem != null && NewQuantity > 0;

        private async Task AddOrderAsync()
        {
            if (SelectedItem is null || NewQuantity <= 0) return;

            try
            {
                using var conn = await OpenAsync();
                using var tx = await conn.BeginTransactionAsync();

                // create order
                var cmdOrder = new MySqlCommand(
                    "INSERT INTO orders(Time, Status) VALUES (NOW(), 'Queued'); SELECT LAST_INSERT_ID();",
                    conn, (MySqlTransaction)tx);
                var orderId = Convert.ToInt32(await cmdOrder.ExecuteScalarAsync());

                // add its single order line
                var cmdLine = new MySqlCommand(
                    "INSERT INTO order_lines(OrderId, ItemId, Quantity) VALUES (@o, @i, @q);",
                    conn, (MySqlTransaction)tx);
                cmdLine.Parameters.AddWithValue("@o", orderId);
                cmdLine.Parameters.AddWithValue("@i", SelectedItem.Id);
                cmdLine.Parameters.AddWithValue("@q", NewQuantity);
                await cmdLine.ExecuteNonQueryAsync();

                await tx.CommitAsync();
            }
            catch
            {
                // ignore for the demo
            }

            NewQuantity = 1;
            await RefreshUiFromDbAsync();
        }

        private bool CanProcessNext() => true; // enabled when there is at least one queued (we’ll recheck after refresh)

        private async Task ProcessNextAsync()
        {
            try
            {
                using var conn = await OpenAsync();

                // get next queued
                int? nextId = null;
                using (var cmd = new MySqlCommand(
                    "SELECT Id FROM orders WHERE Status='Queued' ORDER BY Time LIMIT 1;", conn))
                {
                    var v = await cmd.ExecuteScalarAsync();
                    if (v != null && v != DBNull.Value) nextId = Convert.ToInt32(v);
                }

                if (nextId is null)
                    return;

                // mark processed
                using (var cmd = new MySqlCommand(
                    "UPDATE orders SET Status='Processed' WHERE Id=@id;", conn))
                {
                    cmd.Parameters.AddWithValue("@id", nextId.Value);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch
            {
                // keep UI responsive
            }

            await RefreshUiFromDbAsync();
        }

        // -----------------------------
        // Helpers
        // -----------------------------
        private void UpdateButtons()
        {
            AddOrderCommand.RaiseCanExecuteChanged();
            ProcessNextCommand.RaiseCanExecuteChanged();
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
