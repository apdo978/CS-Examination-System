using System.Security.Cryptography;
using System.Text;

namespace ConsoleApp2
{
    public enum UserRole
    {
        Student,
        Teacher
    }

    public class UserAccount
    {
        public string Email { get; }
        public string PasswordHash { get; }
        public UserRole Role { get; }
        public string Name { get; }

        public UserAccount(string email, string passwordHash, UserRole role, string name)
        {
            Email = email ?? throw new ArgumentNullException(nameof(email));
            PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
            Role = role;
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override string ToString() => $"{Email}|{PasswordHash}|{Role}|{Name}";

        public static UserAccount? FromLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return null;
            string[] parts = line.Split('|');
            if (parts.Length < 4) return null;
            if (!Enum.TryParse<UserRole>(parts[2], out var role)) return null;
            return new UserAccount(parts[0], parts[1], role, parts[3]);
        }
    }

    public static class AuthService
    {
        private const string UsersFile = "users.dat";

        public static bool Register(string email, string password, string name, UserRole role)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(name))
                return false;

            if (FindByEmail(email) is not null)
                return false;

            string hash = HashPassword(password);
            var account = new UserAccount(email, hash, role, name);

            try
            {
                using var writer = new StreamWriter(UsersFile, true);
                writer.WriteLine(account);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        public static UserAccount? Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return null;

            var account = FindByEmail(email);
            if (account is null) return null;

            string hash = HashPassword(password);
            return hash == account.PasswordHash ? account : null;
        }

        public static UserAccount? FindByEmail(string email)
        {
            if (!File.Exists(UsersFile)) return null;

            try
            {
                using var reader = new StreamReader(UsersFile);
                string? line;
                while ((line = reader.ReadLine()) is not null)
                {
                    var account = UserAccount.FromLine(line);
                    if (account is not null && account.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
                        return account;
                }
            }
            catch (IOException) { }

            return null;
        }

        public static UserAccount[] GetAllUsers(UserRole? roleFilter = null)
        {
            if (!File.Exists(UsersFile)) return [];

            UserAccount[] buffer = new UserAccount[16];
            int count = 0;

            try
            {
                using var reader = new StreamReader(UsersFile);
                string? line;
                while ((line = reader.ReadLine()) is not null)
                {
                    var account = UserAccount.FromLine(line);
                    if (account is null) continue;
                    if (roleFilter is not null && account.Role != roleFilter) continue;

                    if (count >= buffer.Length)
                    {
                        var newBuf = new UserAccount[buffer.Length * 2];
                        Array.Copy(buffer, newBuf, count);
                        buffer = newBuf;
                    }
                    buffer[count++] = account;
                }
            }
            catch (IOException) { }

            var result = new UserAccount[count];
            Array.Copy(buffer, result, count);
            return result;
        }

        private static string HashPassword(string password)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }
    }
}
