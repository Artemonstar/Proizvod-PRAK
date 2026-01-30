using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace DeliveryServiceManager
{
    public partial class MainWindow : Window
    {
        private DataService dataService = new DataService();
        private string currentUserRole;

        public MainWindow(string userRole)
        {
            InitializeComponent();
            currentUserRole = userRole;

            // Настраиваем интерфейс в зависимости от роли
            SetupInterfaceForRole();

            // Загружаем данные
            LoadClients();
            LoadOrders();
            LoadStatistics();
        }

        private void SetupInterfaceForRole()
        {
            if (currentUserRole == "admin")
            {
                tabAdmin.Visibility = Visibility.Visible;
                tabStats.Visibility = Visibility.Visible;
                gbNewOrder.Visibility = Visibility.Visible;
                spCourier.Visibility = Visibility.Collapsed;
                Title += " (Администратор)";
            }
            else
            {
                tabAdmin.Visibility = Visibility.Collapsed;
                tabStats.Visibility = Visibility.Collapsed;
                gbNewOrder.Visibility = Visibility.Collapsed;
                spCourier.Visibility = Visibility.Visible;
                Title += " (Курьер)";
            }
        }

        // === Работа с клиентами ===
        private void LoadClients()
        {
            dgClients.ItemsSource = dataService.GetClients();
            cmbClients.ItemsSource = dataService.GetClients();
        }

        private void AddClient_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtClientName.Text))
            {
                MessageBox.Show("Введите ФИО клиента");
                return;
            }

            var client = new Client
            {
                FullName = txtClientName.Text,
                Phone = txtClientPhone.Text,
                Address = txtClientAddress.Text
            };

            dataService.AddClient(client);
            LoadClients();

            // Очищаем поля
            txtClientName.Text = "";
            txtClientPhone.Text = "";
            txtClientAddress.Text = "";

            MessageBox.Show("Клиент добавлен");
        }

        private void DeleteClient_Click(object sender, RoutedEventArgs e)
        {
            if (dgClients.SelectedItem is Client selectedClient)
            {
                if (MessageBox.Show($"Удалить клиента {selectedClient.FullName}?",
                    "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    dataService.DeleteClient(selectedClient.Id);
                    LoadClients();
                }
            }
            else
            {
                MessageBox.Show("Выберите клиента для удаления");
            }
        }

        private void RefreshClients_Click(object sender, RoutedEventArgs e)
        {
            LoadClients();
        }

        // === Работа с заказами ===
        private void LoadOrders()
        {
            if (currentUserRole == "admin")
            {
                dgOrders.ItemsSource = dataService.GetOrders();
            }
            else
            {
                dgOrders.ItemsSource = dataService.GetCourierOrders();
            }
        }

        private void CreateOrder_Click(object sender, RoutedEventArgs e)
        {
            if (cmbClients.SelectedItem is Client selectedClient)
            {
                if (string.IsNullOrWhiteSpace(txtProducts.Text))
                {
                    MessageBox.Show("Введите товары");
                    return;
                }

                if (!decimal.TryParse(txtTotalPrice.Text, out decimal totalPrice))
                {
                    MessageBox.Show("Введите корректную сумму");
                    return;
                }

                var order = new Order
                {
                    ClientId = selectedClient.Id,
                    ClientName = selectedClient.FullName,
                    Products = txtProducts.Text,
                    TotalPrice = totalPrice,
                    Status = OrderStatus.Оформлен
                };

                dataService.AddOrder(order);
                LoadOrders();

                // Очищаем поля
                txtProducts.Text = "";
                txtTotalPrice.Text = "";

                MessageBox.Show("Заказ создан");
            }
            else
            {
                MessageBox.Show("Выберите клиента");
            }
        }

        private void UpdateStatus_Click(object sender, RoutedEventArgs e)
        {
            if (dgOrders.SelectedItem is Order selectedOrder)
            {
                if (cmbStatus.SelectedItem is ComboBoxItem selectedStatus)
                {
                    string statusText = selectedStatus.Content.ToString();
                    OrderStatus newStatus = OrderStatus.Оформлен;

                    switch (statusText)
                    {
                        case "В процессе": newStatus = OrderStatus.ВПроцессе; break;
                        case "Доставлен": newStatus = OrderStatus.Доставлен; break;
                        case "Отменён": newStatus = OrderStatus.Отменён; break;
                    }

                    dataService.UpdateOrderStatus(selectedOrder.Id, newStatus);
                    LoadOrders();

                    MessageBox.Show($"Статус заказа #{selectedOrder.Id} изменен на {statusText}");
                }
                else
                {
                    MessageBox.Show("Выберите новый статус");
                }
            }
            else
            {
                MessageBox.Show("Выберите заказ");
            }
        }

        // === Статистика и отчеты ===
        private void LoadStatistics()
        {
            runTotalOrders.Text = dataService.GetTotalOrders().ToString();
            runTotalRevenue.Text = dataService.GetTotalRevenue().ToString("C");
            runActiveClients.Text = dataService.GetActiveClients().ToString();
        }

        private void ExportJson_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "JSON файлы (*.json)|*.json",
                FileName = $"report_{DateTime.Now:yyyyMMdd}.json"
            };

            if (saveDialog.ShowDialog() == true)
            {
                var data = new
                {
                    ExportDate = DateTime.Now,
                    TotalOrders = dataService.GetTotalOrders(),
                    TotalRevenue = dataService.GetTotalRevenue(),
                    ActiveClients = dataService.GetActiveClients(),
                    Clients = dataService.GetClients(),
                    Orders = dataService.GetOrders()
                };

                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(saveDialog.FileName, json, Encoding.UTF8);
                MessageBox.Show("Отчет сохранен в JSON");
            }
        }

        private void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "CSV файлы (*.csv)|*.csv",
                FileName = $"orders_{DateTime.Now:yyyyMMdd}.csv"
            };

            if (saveDialog.ShowDialog() == true)
            {
                var orders = dataService.GetOrders();
                var csv = new StringBuilder();

                // Заголовок
                csv.AppendLine("ID;Клиент;Товары;Сумма;Статус;Дата");

                // Данные
                foreach (var order in orders)
                {
                    csv.AppendLine($"{order.Id};{order.ClientName};{order.Products};{order.TotalPrice};{order.Status};{order.Date:dd.MM.yyyy}");
                }

                File.WriteAllText(saveDialog.FileName, csv.ToString(), Encoding.UTF8);
                MessageBox.Show("Отчет сохранен в CSV");
            }
        }
    }
}