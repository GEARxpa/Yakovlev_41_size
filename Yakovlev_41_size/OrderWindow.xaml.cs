using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Yakovlev_41_size
{
    public partial class OrderWindow : Window
    {
        private List<OrderProduct> _selectedOrderProducts;
        private List<Product> _selectedProducts;
        private Order _currentOrder = new Order();
        private readonly int _orderNumber;

        public OrderWindow(List<OrderProduct> selectedOrderProducts, List<Product> selectedProducts, string fio)
        {
            InitializeComponent();

            _selectedOrderProducts = selectedOrderProducts;
            _selectedProducts = selectedProducts;
            _orderNumber = GenerateOrderNumber();

            InitializeComponents(fio);
        }

        private void InitializeComponents(string fio)
        {
            try
            {
                DataContext = _currentOrder;
                LoadPickupPoints();
                InitializeProductList();
                SetOrderInfo(fio);
                SetDeliveryDate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadPickupPoints()
        {
            var pickups = Yakovlev41Entities.GetContext().PickUpPoint.ToList();
            PickUpPointCombo.ItemsSource = pickups;
        }

        private void InitializeProductList()
        {
            OrderedProductList.ItemsSource = _selectedProducts;

            foreach (var product in _selectedProducts)
            {
                var orderProduct = _selectedOrderProducts
                    .FirstOrDefault(op => op.ProductArticleNumber == product.ProductArticleNumber);

                if (orderProduct != null)
                {
                    product.ProductQuantityInStock = orderProduct.ProductQuantity;
                }
            }
        }

        private void SetOrderInfo(string fio)
        {
            FIOBox.Text = fio;
            OrderNumBox.Text = _orderNumber.ToString();
            OrderFormDate.SelectedDate = DateTime.Now;
        }

        private int GenerateOrderNumber()
        {
            var random = new Random();
            return random.Next(920, 1000);
        }

        private void SetDeliveryDate()
        {
            if (OrderFormDate.SelectedDate.HasValue)
            {
                var hasStockOverThree = _selectedProducts.Any(p => p.ProductQuantityInStock > 3);
                var deliveryDays = hasStockOverThree ? 3 : 6;
                OrderDeliveryDate.SelectedDate = OrderFormDate.SelectedDate.Value.AddDays(deliveryDays);

                if (OrderDeliveryDate.SelectedDate.HasValue)
                {
                    _currentOrder.OrderDeliveryDate = OrderDeliveryDate.SelectedDate.Value;
                }
            }
        }

        private bool ValidateOrder()
        {
            var errors = new StringBuilder();

            if (!_selectedProducts.Any())
                errors.AppendLine("Ваш заказ пустой");

            if (PickUpPointCombo.SelectedItem == null)
                errors.AppendLine("Выберите пункт выдачи");

            if (!OrderFormDate.SelectedDate.HasValue)
                errors.AppendLine("Укажите дату заказа");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateOrder()) return;

            try
            {
                SaveOrderToDatabase();
                MessageBox.Show("Заказ успешно сохранен", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveOrderToDatabase()
        {
            if (_currentOrder.OrderID == 0)
            {
                _currentOrder.OrderDate = OrderFormDate.SelectedDate.Value;
                _currentOrder.OrderStatus = "новый";

                if (PickUpPointCombo.SelectedItem is PickUpPoint selectedPoint)
                {
                    _currentOrder.OrderPickupPoint = selectedPoint.PickUpPointID;
                }

                Yakovlev41Entities.GetContext().Order.Add(_currentOrder);
            }

            Yakovlev41Entities.GetContext().SaveChanges();
        }

        private void MinusBtn_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Product product && product.ProductQuantityInStock > 0)
            {
                product.ProductQuantityInStock--;
                UpdateOrderProductQuantity(product, -1);

                if (product.ProductQuantityInStock == 0)
                {
                    RemoveProductFromOrder(product);
                }

                SetDeliveryDate();
                OrderedProductList.Items.Refresh();
            }
        }

        private void PlusBtn_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Product product)
            {
                product.ProductQuantityInStock++;
                UpdateOrderProductQuantity(product, 1);
                SetDeliveryDate();
                OrderedProductList.Items.Refresh();
            }
        }

        private void UpdateOrderProductQuantity(Product product, int change)
        {
            var orderProduct = _selectedOrderProducts
                .FirstOrDefault(op => op.ProductArticleNumber == product.ProductArticleNumber);

            if (orderProduct != null)
            {
                orderProduct.ProductQuantity += change;

                if (orderProduct.ProductQuantity <= 0)
                {
                    _selectedOrderProducts.Remove(orderProduct);
                }
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Product product)
            {
                var result = MessageBox.Show("Вы точно хотите убрать товар из заказа?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    RemoveProductFromOrder(product);
                    OrderedProductList.Items.Refresh();
                }
            }
        }

        private void RemoveProductFromOrder(Product product)
        {
            _selectedProducts.Remove(product);

            var orderProduct = _selectedOrderProducts
                .FirstOrDefault(op => op.ProductArticleNumber == product.ProductArticleNumber);

            if (orderProduct != null)
            {
                _selectedOrderProducts.Remove(orderProduct);
            }
        }

        private void PickUpPointCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PickUpPointCombo.SelectedItem is PickUpPoint selectedPoint)
            {
                _currentOrder.OrderPickupPoint = selectedPoint.PickUpPointID;
            }
        }

        private void OrderFormDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            SetDeliveryDate();
        }
    }
}