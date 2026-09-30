using System.Configuration;
using MySql.Data.MySqlClient;

namespace ShoeShop.Data
{
    public static class DatabaseContext
    {
        private static string ConnStr
        {
            get
            {
                var s = ConfigurationManager.ConnectionStrings["ShoeShopDb"];
                if (s == null)
                    throw new System.InvalidOperationException(
                        "Строка подключения 'ShoeShopDb' не найдена в App.config.\n\n" +
                        "Проверьте:\n" +
                        "1. App.config в корне проекта\n" +
                        "2. <connectionStrings> с name=\"ShoeShopDb\"\n" +
                        "3. Copy to Output = Copy if newer\n" +
                        "4. bin\\Debug\\ShoeShop.exe.config существует");
                return s.ConnectionString;
            }
        }

        public static MySqlConnection GetConnection()
        {
            var conn = new MySqlConnection(ConnStr);
            conn.Open();
            return conn;
        }
    }
}