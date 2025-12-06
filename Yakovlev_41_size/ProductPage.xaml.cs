using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Yakovlev_41_size
{
    public partial class ProductPage : Page
    {
        private User _currentUser;
        private List<OrderProduct> _selectedOrderProducts = new List<OrderProduct>();
        private List<Product> _selectedProducts = new List<Product>();
        private List<Product> _allProducts;

        public ProductPage(User user)
        {
            InitializeComponent();
            _currentUser = user;
            InitializeUserInfo();
            LoadProducts();
            UpdateDisplay();
        }

        private void InitializeUserInfo()
        {
            if (_currentUser != null)
            {
                FIOTB.Text = _currentUser.UserSurname + " " + _currentUser.UserName + " " + _currentUser.UserPatronymic;
                switch (_currentUser.UserRole)
                {
                    case 1:
                        RoleTB.Text = "Клиент";
                        break;
                    case 2:
                        RoleTB.Text = "Менеджер";
                        break;
                    case 3:
                        RoleTB.Text = "Администратор";
                        break;
                }
            }
            else
            {
                FIOTB.Text = "Не авторизован";
                RoleTB.Text = "Гость";
            }
        }

        private void LoadProducts()
        {
            try
            {
                _allProducts = Yakovlev41Entities.GetContext().Product.ToList();
                ProductListView.ItemsSource = _allProducts;
                ComboType.SelectedIndex = 0;
                UpdateCounters(_allProducts.Count, _allProducts.Count);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateDisplay()
        {
            if (_allProducts == null) return;

            var filteredProducts = FilterProducts();
            var sortedProducts = SortProducts(filteredProducts);

            ProductListView.ItemsSource = sortedProducts;
            UpdateCounters(sortedProducts.Count, _allProducts.Count);
        }

        private List<Product> FilterProducts()
        {
            var filtered = _allProducts.Where(p =>
                p.ProductName.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();

            if (ComboType.SelectedIndex == 1)
                filtered = filtered.Where(p => p.ProductDiscountAmount >= 0 && p.ProductDiscountAmount < 10).ToList();
            else if (ComboType.SelectedIndex == 2)
                filtered = filtered.Where(p => p.ProductDiscountAmount >= 10 && p.ProductDiscountAmount < 15).ToList();
            else if (ComboType.SelectedIndex == 3)
                filtered = filtered.Where(p => p.ProductDiscountAmount >= 15 && p.ProductDiscountAmount <= 100).ToList();

            return filtered;
        }

        private List<Product> SortProducts(List<Product> products)
        {
            if (RButtonDown.IsChecked == true)
                return products.OrderByDescending(p => p.ProductCost).ToList();
            else if (RButtonUp.IsChecked == true)
                return products.OrderBy(p => p.ProductCost).ToList();

            return products;
        }

        private void UpdateCounters(int currentCount, int totalCount)
        {
            TBCount.Text = currentCount.ToString();
            TBAllProducts.Text = totalCount.ToString();
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateDisplay();
        }

        private void ComboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDisplay();
        }

        private void RButtonDown_Checked(object sender, RoutedEventArgs e)
        {
            UpdateDisplay();
        }

        private void RButtonUp_Checked(object sender, RoutedEventArgs e)
        {
            UpdateDisplay();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ProductListView.SelectedItem is Product selectedProduct)
            {
                AddProductToOrder(selectedProduct);
                ProductListView.SelectedIndex = -1;
                OrderMenuBtn.Visibility = Visibility.Visible;
            }
        }

        private void AddProductToOrder(Product product)
        {
            _selectedProducts.Add(product);

            var existingOrderProduct = _selectedOrderProducts
                .FirstOrDefault(op => op.ProductArticleNumber == product.ProductArticleNumber);

            if (existingOrderProduct != null)
            {
                existingOrderProduct.ProductQuantity++;
            }
            else
            {
                var newOrderProduct = new OrderProduct
                {
                    ProductArticleNumber = product.ProductArticleNumber,
                    ProductQuantity = 1
                };
                _selectedOrderProducts.Add(newOrderProduct);
            }
        }

        private void OrderMenu_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProducts.Any())
            {
                var distinctProducts = _selectedProducts.Distinct().ToList();
                var orderWindow = new OrderWindow(_selectedOrderProducts, distinctProducts, FIOTB.Text);
                orderWindow.ShowDialog();
                OrderMenuBtn.Visibility = _selectedProducts.Any() ? Visibility.Visible : Visibility.Collapsed;
            }
            else
                MessageBox.Show("Корзина пуста");
        }
    }
}