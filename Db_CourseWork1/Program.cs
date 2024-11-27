using System;
using System.Data.SqlClient;
using System.Collections.Generic; 
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Dapper;

namespace Db_CourseWork1
{
    public class Purchase
    {
        public int Id { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal TotalAmount { get; set; }
        public ICollection<Discount> Discounts { get; set; } = new List<Discount>();
    }

    public class Discount
    {
        public int Id { get; set; }
        public decimal DiscountAmount { get; set; }
        public int PurchaseId { get; set; }

        [ForeignKey("PurchaseId")]
        public Purchase Purchase { get; set; }
    }

    public class AppContext : DbContext
    {
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Discount> Discounts { get; set; }

        public AppContext()
        {
            //Database.EnsureDeleted();
            //Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=DESKTOP-1FH1SUV;Database=PurchaseManager;Trusted_Connection=True;TrustServerCertificate=True;");
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }

    public class PurchaseRepository
    {
        private readonly string _connectionString;
        public PurchaseRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        public void AddPurchase(Purchase purchase)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                var query = "INSERT INTO Purchases (PurchaseDate, TotalAmount) VALUES (@PurchaseDate, @TotalAmount)";
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PurchaseDate", purchase.PurchaseDate);
                    command.Parameters.AddWithValue("@TotalAmount", purchase.TotalAmount);
                    command.ExecuteNonQuery();
                }
            }
        }
        public List<Purchase> GetAllPurchases()
        {
            var purchaseList = new List<Purchase>();
            var query = "SELECT Id, PurchaseDate, TotalAmount FROM Purchases";
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            purchaseList.Add(new Purchase
                            {
                                Id = reader.GetInt32(0),
                                PurchaseDate = reader.GetDateTime(1),
                                TotalAmount = reader.GetDecimal(2)
                            });
                        }
                    }
                }
            }
            return purchaseList;
        }
        public void UpdatePurchase(Purchase purchase)
        {
            var query = "UPDATE Purchases SET PurchaseDate = @PurchaseDate, TotalAmount = @TotalAmount WHERE Id = @Id";
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", purchase.Id);
                    command.Parameters.AddWithValue("@PurchaseDate", purchase.PurchaseDate);
                    command.Parameters.AddWithValue("@TotalAmount", purchase.TotalAmount);
                    command.ExecuteNonQuery();
                }

            }
        }
        public void DeletePurchase(int purchaseId)
        {
            var query = "DELETE FROM Purchases WHERE Id = @Id";
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", purchaseId);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
    public class DiscountRepository
    {
        private readonly string _connectionString;
        public DiscountRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<Discount> GetAllDiscounts()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                return connection.Query<Discount>("SELECT Id, DiscountAmount, PurchaseId FROM Discounts");
            }
        }

        public void UpdateDiscount(Discount discount)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var sql = "UPDATE Discounts SET DiscountAmount = @DiscountAmount WHERE Id = @Id";
                connection.Execute(sql, new { discount.DiscountAmount, discount.Id });
            }
        }

        public void DeleteDiscount(int discountId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var sql = "DELETE FROM Discounts WHERE Id = @Id";
                connection.Execute(sql, new { Id = discountId });
            }
        }
    }
    public class Program
    {
        public static void InsertData()
        {
            using (AppContext context = new AppContext())
            {
                Purchase purchase = new Purchase()
                {
                    PurchaseDate = new DateTime(2024, 10, 10),
                    TotalAmount = 20000M
                };
                Purchase purchase1 = new Purchase()
                {
                    PurchaseDate = new DateTime(2024, 11, 11),
                    TotalAmount = 210000M
                };

                context.Purchases.Add(purchase);
                context.Purchases.Add(purchase1);
                context.SaveChanges();

                Discount discount = new Discount()
                {
                    DiscountAmount = 1421M,
                    PurchaseId = purchase.Id
                };
                Discount discount1 = new Discount()
                {
                    DiscountAmount = 14444M,
                    PurchaseId = purchase1.Id
                };

                context.Discounts.Add(discount);
                context.Discounts.Add(discount1);
                context.SaveChanges();
            }
        }

        public static void Main(string[] args)
        {
            string connectionString = "Server=DESKTOP-1FH1SUV; Database=PurchaseManager; Trusted_Connection=True; TrustServerCertificate=True; ";
            PurchaseRepository purchaseRepository = new PurchaseRepository(connectionString);
            DiscountRepository discountRepository = new DiscountRepository(connectionString);
            InsertData();
            Console.WriteLine("allright");

            

            while (!false)
            {
                Console.WriteLine($"\nВыберите сущность:\n1 - Purchases\n2 - Discounts\n3 - Объединение");
                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        Console.WriteLine("\n---Вы выбрали Purchases---\n1 - Добавить новую покупку\n2 - Вывести все покупки\n3 - Обновить строку\n4 - Удалить строку\n9 - Выйти из меню");
                        var purchaseChoice = Console.ReadLine();
                        switch (purchaseChoice)
                        {
                            case "1":
                                Console.WriteLine("Введите стоимость покупки: ");
                                var AmountOfPurchase = Convert.ToDecimal(Console.ReadLine());
                                Purchase newPurchase = new Purchase
                                {
                                    PurchaseDate = DateTime.Now,
                                    TotalAmount = AmountOfPurchase
                                };
                                purchaseRepository.AddPurchase(newPurchase);
                                Console.WriteLine("Данные успешно добавлены");
                                break;
                            case "2":
                                var purchases = purchaseRepository.GetAllPurchases();
                                Console.WriteLine("Все покупки:");
                                foreach (var purchase in purchases)
                                {
                                    Console.WriteLine($"Id: {purchase.Id}, Дата: {purchase.PurchaseDate}, Сумма: {purchase.TotalAmount}");
                                }
                                break;
                            case "3":
                                Console.WriteLine("Введите Id покупки для обновления: ");
                                int updateId = Convert.ToInt32(Console.ReadLine());
                                Console.WriteLine("Введите новую стоимость покупки: ");
                                var newAmount = Convert.ToDecimal(Console.ReadLine());
                                Purchase updatedPurchase = new Purchase
                                {
                                    Id = updateId,
                                    PurchaseDate = DateTime.Now,
                                    TotalAmount = newAmount
                                };
                                purchaseRepository.UpdatePurchase(updatedPurchase);
                                Console.WriteLine("Данные успешно обновлены");
                                break;
                            case "4":
                                Console.WriteLine("Введите Id покупки для удаления: ");
                                int deleteId = Convert.ToInt32(Console.ReadLine());
                                purchaseRepository.DeletePurchase(deleteId);
                                Console.WriteLine("Данные успешно удалены");
                                break;
                            case "9":
                                return;
                        }
                        break;
                    case "2":
                        Console.WriteLine("\n---Вы выбрали Discounts---\n1 - Вывести все скидки\n2 - Обновить сумму скидки\n3 - Удалить скидку\n9 - Выйти из меню");
                        var discountChoice = Console.ReadLine();
                        switch (discountChoice)
                        {
                            case "1":
                                var discounts = discountRepository.GetAllDiscounts();
                                Console.WriteLine("Все скидки:");
                                foreach (var discount in discounts)
                                {
                                    Console.WriteLine($"Id: {discount.Id}, Сумма скидки: {discount.DiscountAmount}, Id покупки: {discount.PurchaseId}");
                                }
                                break;
                            case "2":
                                Console.WriteLine("Введите Id скидки для обновления: ");
                                int updateId = Convert.ToInt32(Console.ReadLine());
                                Console.WriteLine("Введите новую сумму скидки: ");
                                var newDiscountAmount = Convert.ToDecimal(Console.ReadLine());
                                Discount updatedDiscount = new Discount
                                {
                                    Id = updateId,
                                    DiscountAmount = newDiscountAmount
                                };
                                discountRepository.UpdateDiscount(updatedDiscount);
                                Console.WriteLine("Сумма скидки успешно обновлена");
                                break;
                            case "3":
                                Console.WriteLine("Введите Id скидки для удаления: ");
                                int deleteId = Convert.ToInt32(Console.ReadLine());
                                discountRepository.DeleteDiscount(deleteId);
                                Console.WriteLine("Скидка успешно удалена");
                                break;
                            case "9":
                                return; 
                        }
                        break;
                case "3":
                    using (var context = new AppContext())
                    {
                        context.Purchases.Load();
                        context.Discounts.Load();

                        var purchasesWithDiscounts = context.Purchases
                            .Include(p => p.Discounts)
                            .Select(p => new
                            {
                                PurchaseId = p.Id,
                                PurchaseDate = p.PurchaseDate,
                                TotalAmount = p.TotalAmount,
                                DiscountAmount = p.Discounts.Sum(d => d.DiscountAmount),
                                DiscountedAmount = p.TotalAmount - p.Discounts.Sum(d => d.DiscountAmount)
                            });

                        foreach (var purchase in purchasesWithDiscounts)
                        {
                            Console.WriteLine($"Id: {purchase.PurchaseId}, Дата: {purchase.PurchaseDate}, Сумма: {purchase.TotalAmount}, Скидка: {purchase.DiscountAmount}, Сумма с учетом скидки: {purchase.DiscountedAmount}");
                        }
                    }
                    break;
            }
            }
        }
    }
}