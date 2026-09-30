using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ShoeShop.Models;
using ShoeShop.Services;

namespace ShoeShop.Views
{
    public partial class MainWindow : Window
    {
        private List<Product> _all = new List<Product>();

        public MainWindow()
        {
            InitializeComponent();
            ApplyRole();
            LoadProducts();
        }

        private void ApplyRole()
        {
            var u = AuthService.Current;
            TxtUserInfo.Text = u.FullName;

            bool isManager = u.RoleName == "Менеджер";
            bool isAdmin = u.RoleName == "Администратор";

            if (isManager || isAdmin)
            {
                PanelFilters.Visibility = Visibility.Visible;
                BtnOrders.Visibility = Visibility.Visible;
                LoadSuppliers();
            }
            if (isAdmin)
            {
                BtnAddProduct.Visibility = Visibility.Visible;
                BtnDelete.Visibility = Visibility.Visible;
            }
        }

        private void LoadSuppliers()
        {
            var list = new List<LookupItem>
            {
                new LookupItem { Id = -1, Name = "Все поставщики" }
            };
            list.AddRange(ProductService.GetSuppliers());
            CmbSupplier.ItemsSource = list;
            CmbSupplier.SelectedIndex = 0;
        }

        private void LoadProducts()
        {
            try
            {
                _all = ProductService.GetAll();
                System.Diagnostics.Debug.WriteLine($"Загружено товаров: {_all?.Count ?? -1}");
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show(
                    "Ошибка загрузки товаров:\n\n" + ex.Message +
                    "\n\n" + ex.StackTrace,
                    "Ошибка БД");
                _all = new List<Product>();
            }
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (LstProducts == null) return;

            IEnumerable<Product> q = _all;

            // Панель фильтров видна только менеджеру и админу
            if (PanelFilters.Visibility == Visibility.Visible)
            {
                // --- Поиск ---
                string search = TxtSearch.Text.Trim().ToLower();
                if (!string.IsNullOrEmpty(search))
                {
                    q = q.Where(p =>
                        (p.Name ?? "").ToLower().Contains(search) ||
                        (p.Article ?? "").ToLower().Contains(search) ||
                        (p.CategoryName ?? "").ToLower().Contains(search) ||
                        (p.ManufacturerName ?? "").ToLower().Contains(search) ||
                        (p.SupplierName ?? "").ToLower().Contains(search) ||
                        (p.Description ?? "").ToLower().Contains(search) ||
                        (p.UnitName ?? "").ToLower().Contains(search));
                }

                // --- Фильтр по поставщику ---
                if (CmbSupplier.SelectedItem is LookupItem sel && sel.Id != -1)
                {
                    q = q.Where(p => p.SupplierId == sel.Id);
                }

                // --- Сортировка по количеству на складе ---
                switch (CmbSort.SelectedIndex)
                {
                    case 1: q = q.OrderBy(p => p.StockQty); break;
                    case 2: q = q.OrderByDescending(p => p.StockQty); break;
                }
            }

            LstProducts.ItemsSource = q.ToList();
        }
        private void Filter_Changed(object sender, RoutedEventArgs e) => ApplyFilter();

        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            var w = new ProductEditWindow(null);
            if (w.ShowDialog() == true) LoadProducts();
        }

        private void LstProducts_MouseDoubleClick(object sender,
            System.Windows.Input.MouseButtonEventArgs e)
        {
            if (AuthService.Current.RoleName != "Администратор") return;
            if (LstProducts.SelectedItem is Product p)
            {
                var w = new ProductEditWindow(p);
                if (w.ShowDialog() == true) LoadProducts();
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!(LstProducts.SelectedItem is Product p))
            {
                MessageBox.Show("Выберите товар.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (ProductService.IsInOrders(p.ProductId))
            {
                MessageBox.Show("Товар присутствует в заказе, удаление невозможно.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (MessageBox.Show($"Удалить товар \"{p.Name}\"?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            ProductService.Delete(p.ProductId);
            LoadProducts();
        }

        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            new OrdersWindow().ShowDialog();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            Close();
        }
    }
}