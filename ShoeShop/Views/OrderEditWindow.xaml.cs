using System;
using System.Windows;
using ShoeShop.Models;
using ShoeShop.Services;

namespace ShoeShop.Views
{
    public partial class OrderEditWindow : Window
    {
        private readonly Order _order;

        public OrderEditWindow(Order order)
        {
            InitializeComponent();
            _order = order;
            CmbStatus.ItemsSource = OrderService.GetStatuses();

            if (order == null)
            {
                Title = "Добавление заказа";
                CmbStatus.SelectedIndex = 0;
                DpOrderDate.SelectedDate = DateTime.Today;
            }
            else
            {
                Title = "Редактирование заказа";
                TxtArticle.Text = order.Article;
                CmbStatus.SelectedValue = order.StatusId;
                TxtPickup.Text = order.PickupPoint;
                DpOrderDate.SelectedDate = order.OrderDate;
                DpDeliveryDate.SelectedDate = order.DeliveryDate;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtArticle.Text) ||
                string.IsNullOrWhiteSpace(TxtPickup.Text) ||
                CmbStatus.SelectedValue == null ||
                DpOrderDate.SelectedDate == null)
            {
                MessageBox.Show("Заполните все обязательные поля.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var o = new Order
            {
                OrderId = _order?.OrderId ?? 0,
                Article = TxtArticle.Text.Trim(),
                StatusId = (int)CmbStatus.SelectedValue,
                PickupPoint = TxtPickup.Text.Trim(),
                OrderDate = DpOrderDate.SelectedDate.Value,
                DeliveryDate = DpDeliveryDate.SelectedDate
            };

            try
            {
                if (_order == null) OrderService.Add(o);
                else OrderService.Update(o);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}