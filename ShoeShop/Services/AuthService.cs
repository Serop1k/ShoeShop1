using MySql.Data.MySqlClient;
using ShoeShop.Data;

namespace ShoeShop.Services
{
    public class CurrentUser
    {
        public int UserId { get; set; }
        public string Login { get; set; }
        public string FullName { get; set; }
        public string RoleName { get; set; }
    }

    public static class AuthService
    {
        public static CurrentUser Current { get; private set; }

        public static bool TryLogin(string login, string password, out string error)
        {
            error = null;
            using (var conn = DatabaseContext.GetConnection())
            using (var cmd = new MySqlCommand(
                @"SELECT u.user_id, u.login, u.full_name, r.role_name
                  FROM users u JOIN roles r ON r.role_id = u.role_id
                  WHERE u.login = @l AND u.password = @p", conn))
            {
                cmd.Parameters.AddWithValue("@l", login);
                cmd.Parameters.AddWithValue("@p", password);
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        Current = new CurrentUser
                        {
                            UserId = rdr.GetInt32(0),
                            Login = rdr.GetString(1),
                            FullName = rdr.GetString(2),
                            RoleName = rdr.GetString(3)
                        };
                        return true;
                    }
                }
            }
            error = "Неверный логин или пароль.";
            return false;
        }

        public static void LoginAsGuest()
        {
            Current = new CurrentUser
            {
                UserId = 0,
                Login = "guest",
                FullName = "Гость",
                RoleName = "Гость"
            };
        }
    }
}