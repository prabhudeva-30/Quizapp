using System.Security.Cryptography;
using System.Text;

namespace Quizapp.Common
{
    public static class PasswordManager
    {

        public static void CreatePassword(string Password, out string PasswordSaltString, out string HashedPasswordString)
        {
            byte[] PasswordSalt;
            byte[] HashedPassword;

            using (var hmac = new HMACSHA512())
            {
                PasswordSalt = hmac.Key;
                HashedPassword = hmac.ComputeHash(Encoding.UTF8.GetBytes(Password));
            }

            PasswordSaltString = Convert.ToBase64String(PasswordSalt);
            HashedPasswordString = Convert.ToBase64String(HashedPassword);
        }

        public static bool VerifyPassword(string Password, string HashedPassword, string PasswordSalt)
        {
            using (var hmac = new HMACSHA512(Convert.FromBase64String(PasswordSalt)))
            {
                var ComputedPassword = hmac.ComputeHash(Encoding.UTF8.GetBytes(Password));
                return ComputedPassword.SequenceEqual(Convert.FromBase64String(HashedPassword));
            }
        }


    }
}
