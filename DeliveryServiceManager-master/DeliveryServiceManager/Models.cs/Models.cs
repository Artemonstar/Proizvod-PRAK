using System;
using System.Collections.Generic;

namespace DeliveryServiceManager
{
    // Модель клиента
    public class Client
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }

    // Модель заказа
    public enum OrderStatus
    {
        Оформлен,
        ВПроцессе,
        Доставлен,
        Отменён
    }

    public class Order
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public string Products { get; set; } // Просто строка для упрощения
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime Date { get; set; }
    }

    // Модель пользователя
    public class User
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } // "admin" или "courier"
    }

    // Главная модель данных
    public class AppData
    {
        public List<Client> Clients { get; set; } = new List<Client>();
        public List<Order> Orders { get; set; } = new List<Order>();
        public List<User> Users { get; set; } = new List<User>();
    }
}