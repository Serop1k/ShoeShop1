using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using ShoeShop.Models;
using ShoeShop.Services;

namespace ShoeShop.Views
{
    public partial class ProductEditWindow : Window
    {
        private readonly Product _product;
        private string _newImagePath;

        public ProductEditWindow(Product product)
        {
            InitializeComponent();
            _product = product;

            CmbCategory.ItemsSource = ProductService.GetCategories();
            CmbManufacturer.ItemsSource = ProductService.GetManufacturers();
            CmbSupplier.ItemsSource = ProductService.GetSuppliers();
            CmbUnit.ItemsSource = ProductService.GetUnits();

            if (product == null)
            {
                Title = "Добавление товара";
                TxtId.Text = "(автоматически)";
                CmbCategory.SelectedIndex = 0;
                CmbManufacturer.SelectedIndex = 0;
                CmbSupplier.SelectedIndex = 0;
                CmbUnit.SelectedIndex = 0;
            }
            else
            {
                Title = "Редактирование товара";
                TxtId.Text = product.ProductId.ToString();
                TxtArticle.Text = product.Article;
                TxtName.Text = product.Name;
                TxtDesc.Text = product.Description;
                TxtPrice.Text = product.Price.ToString("0.00");
                TxtDiscount.Text = product.Discount.ToString();
                TxtStock.Text = product.StockQty.ToString();
                CmbCategory.SelectedValue = product.CategoryId;
                CmbManufacturer.SelectedValue = product.ManufacturerId;
                CmbSupplier.SelectedValue = product.SupplierId;
                CmbUnit.SelectedValue = product.UnitId;

                if (!string.IsNullOrEmpty(product.ImagePath) && File.Exists(product.ImagePath))
                    ImgPreview.Source = new BitmapImage(new Uri(product.ImagePath, UriKind.Absolute));
                _newImagePath = product.ImagePath;
            }
        }

        private void BtnLoadImage_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Изображения|*.png;*.jpg;*.jpeg;*.bmp"
            };
            if (dlg.ShowDialog() != true) return;

            // Ограничение 300x200 (по ширине/высоте)
            var src = new BitmapImage(new Uri(dlg.FileName));
            if (src.PixelWidth > 300 || src.PixelHeight > 200)
            {
                MessageBox.Show("Изображение должно быть не более 300x200 пикселей.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Копируем в папку приложения
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProductImages");
            Directory.CreateDirectory(dir);
            string target = Path.Combine(dir, Guid.NewGuid() + Path.GetExtension(dlg.FileName));
            File.Copy(dlg.FileName, target, true);

            // Удаляем старое фото, если было
            if (!string.IsNullOrEmpty(_newImagePath) && File.Exists(_newImagePath) &&
                _newImagePath != target)
            {
                try { File.Delete(_newImagePath); } catch { /* ignore */ }
            }

            _newImagePath = target;
            ImgPreview.Source = new BitmapImage(new Uri(target));
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(TxtArticle.Text) ||
                string.IsNullOrWhiteSpace(TxtName.Text))
            {
                MessageBox.Show("Заполните артикул и наименование.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!decimal.TryParse(TxtPrice.Text, out var price) || price < 0)
            {
                MessageBox.Show("Некорректная цена.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!int.TryParse(TxtDiscount.Text, out var disc) || disc < 0 || disc > 100)
            {
                MessageBox.Show("Скидка должна быть от 0 до 100.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!int.TryParse(TxtStock.Text, out var stock) || stock < 0)
            {
                MessageBox.Show("Количество не может быть отрицательным.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (CmbCategory.SelectedValue == null || CmbManufacturer.SelectedValue == null ||
                CmbSupplier.SelectedValue == null || CmbUnit.SelectedValue == null)
            {
                MessageBox.Show("Выберите все справочные значения.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var p = new Product
            {
                ProductId = _product?.ProductId ?? 0,
                Article = TxtArticle.Text.Trim(),
                Name = TxtName.Text.Trim(),
                Description = TxtDesc.Text.Trim(),
                CategoryId = (int)CmbCategory.SelectedValue,
                ManufacturerId = (int)CmbManufacturer.SelectedValue,
                SupplierId = (int)CmbSupplier.SelectedValue,
                UnitId = (int)CmbUnit.SelectedValue,
                Price = price,
                Discount = disc,
                StockQty = stock,
                ImagePath = _newImagePath
            };

            try
            {
                if (_product == null) ProductService.Add(p);
                else ProductService.Update(p);
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