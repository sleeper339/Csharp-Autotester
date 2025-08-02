using System;
using Gtk;
using Npgsql;
using System.Threading.Tasks;

namespace autotester
{
    class Program
    {

        public static void Main(string[] args)
        {
            var connString = "Host=localhost;Username=mine;Database=autotester";
            var dataSource = new NpgsqlDataSourceBuilder(connString).Build();

            var conn = dataSource.OpenConnectionAsync();

            Application.Init();
            new StudentPortal(1);
            Application.Run();

            Console.WriteLine("Goodbye!");
        }
    }
}
