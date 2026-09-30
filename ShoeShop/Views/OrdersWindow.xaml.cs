using System.Windows;
using ShoeShop.Models;
using ShoeShop.Services;

namespace ShoeShop.Views
{
    public partial class OrdersWindow : Window
    {
        public OrdersWindow()
        {
            InitializeComponent();
            ApplyRole();
            LoadOrders();
        }

        private void ApplyRole()
        {
            bool isAdmin = AuthService.Current.RoleName == "Администратор";
            BtnAddOrder.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            BtnDelOrder.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoadOrders()
        {
            LstOrders.ItemsSource = OrderService.GetAll();
        }

        private void BtnAddOrder_Click(object sender, RoutedEventArgs e)
        {
            var w = new OrderEditWindow(null);
            if (w.ShowDialog() == true) LoadOrders();
        }

        private void LstOrders_MouseDoubleClick(object sender,
            System.Windows.Input.MouseButtonEventArgs e)
        {
            if (AuthService.Current.RoleName != "Администратор") return;
            if (LstOrders.SelectedItem is Order o)
            {
                var w = new OrderEditWindow(o);
                if (w.ShowDialog() == true) LoadOrders();
            }
        }

        private void BtnDelOrder_Click(object sender, RoutedEventArgs e)
        {
            if (!(LstOrders.SelectedItem is Order o))
            {
                MessageBox.Show("Выберите заказ.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (MessageBox.Show($"Удалить заказ \"{o.Article}\"?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            OrderService.Delete(o.OrderId);
            LoadOrders();
        }
    }
}