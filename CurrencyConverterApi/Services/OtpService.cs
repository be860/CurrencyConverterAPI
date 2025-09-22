using CurrencyConverterApi.Data;
using CurrencyConverterApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace CurrencyConverterApi.Services
{
    public class OtpService : IOtpService
    {
        private readonly AppDbContext _db;
        private readonly Random _rng = new();

        public OtpService(AppDbContext db) => _db = db;

        public async Task<string> GenerateAndStoreOtpAsync(string email)
        {
            // ensure user exists
            var user = await _db.Users
                                .SingleOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                user = new User { Email = email };
                _db.Users.Add(user);
                await _db.SaveChangesAsync();
            }

            // create OTP
            var code = _rng.Next(100000, 999999).ToString();
            var now = DateTime.UtcNow;
            var record = new OtpCode
            {
                UserId = user.Id,
                Code = code,
                CreatedAt = now,
                ExpiresAt = now.AddMinutes(5),
                IsUsed = false
            };

            _db.OtpCodes.Add(record);
            await _db.SaveChangesAsync();
            return code;
        }

        public async Task<bool> ValidateOtpAsync(string email, string code)
        {
            var record = await _db.OtpCodes
                .Include(o => o.User)
                .FirstOrDefaultAsync(o =>
                    o.User.Email == email &&
                    o.Code == code &&
                    !o.IsUsed &&
                    o.ExpiresAt > DateTime.UtcNow);

            if (record is null)
                return false;

            record.IsUsed = true;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
