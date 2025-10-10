# Currency Converter API

A .NET 8 Web API that converts US Dollars (USD) to Sierra Leone Leones (SLE).
This project demonstrates modern backend practices including:

✅ Entity Framework Core with SQL Server

✅ JWT Authentication with Bearer tokens

✅ Email OTP Verification via SMTP (Gmail by default)

✅ External API Integration with Apilayer Exchange Rates

✅ Swagger UI for interactive API testing

# 🚀 Features

**User Authentication**

1. **Register with email**

2. **OTP sent automatically to verify account**

3. **Login and secured endpoints with JWT**



**Currency Conversion**

Convert USD → SLE using real-time exchange rates


**Security**

1. JWT Bearer token authentication

2. OTP verification before access



**Documentation**

1. Swagger UI pre-configured to accept JWT tokens

   

**Persistence**

1. SQL Server database via EF Core (Users, OTPs, Logs, etc.)


# 🛠️ Tech Stack

1. .NET 8 Web API

2. Entity Framework Core (SQL Server)

3. JWT Authentication

4. SMTP (Gmail)

5. Swagger / Swashbuckle

6. Apilayer Exchange Rates API

---

# 📂 Project Structure
```
CurrencyConverterApi/
│── Controllers/
│   ├── AuthController.cs       # Handles Register, OTP, Login
│   ├── ConversionController.cs # USD → SLE conversion
│
│── Data/
│   ├── AppDbContext.cs         # EF Core DB context
│
│── Models/
│   ├── User.cs
│   ├── OtpRecord.cs
│   ├── ConversionRequest.cs
│   ├── ConversionResponse.cs
│
│── Services/
│   ├── ICurrencyConverterService.cs
│   ├── CurrencyConverterService.cs
│   ├── IEmailService.cs / EmailService.cs
│   ├── IOtpService.cs / OtpService.cs
│
│── Program.cs                  # App entrypoint
│── appsettings.json            # Configuration
│── README.md                   # Project documentation
```

# ⚙️ Setup & Installation

1. Clone Repo

```
git clone https://github.com/yourusername/CurrencyConverterApi.git
cd CurrencyConverterApi
```

3. Configure Database
```
Edit appsettings.json:

"ConnectionStrings": {
  "Default": "Server=YOUR_SERVER;Database=CurrencyConverter;Trusted_Connection=True;TrustServerCertificate=True;"
}

```

3. Apply Migrations
```
dotnet tool install --global dotnet-ef
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet ef database update
```

5. Configure Email (SMTP)
```
Update appsettings.json with your SMTP provider:

"Smtp": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "Username": "your-email@gmail.com",
  "Password": "your-app-password",
  "From": "your-email@gmail.com"
}
```

5. Configure Exchange Rates API
 ```
"ExchangeRates": {
  "BaseUrl": "https://api.apilayer.com/exchangerates_data",
  "ApiKey": "YOUR_REAL_KEY",
  "Symbol": "SLE"
}
```

7. Run the API
```
dotnet run
```

Swagger will be available at:
```
👉 https://localhost:5001/swagger
```

# 🔑 Authentication Flow

1. Register
```
POST /api/Auth/register → sends OTP to email
```

3. Verify OTP
```
POST /api/Auth/verify-otp → activates account
```

4. Login
```
POST /api/Auth/login → receive JWT token
```

6. Use JWT Token
```
Copy token into Swagger Authorize dialog (no need to type "Bearer ")
```

7. Access Protected Endpoint
```
Example: POST /api/Conversion/usd-to-sll
```


# 📬 Example Request (Conversion)
```
curl -X POST "https://localhost:5001/api/Conversion/usd-to-sll" \
  -H "Authorization: Bearer <YOUR_TOKEN>" \
  -H "Content-Type: application/json" \
  -d '{"amount": 100}'


Response:

{
  "amountUsd": 100,
  "exchangeRate": 22.45,
  "amountSll": 2245,
  "timestamp": "2025-07-03T16:17:27Z"
}
```

---

# 📖 Roadmap

 1. Add Refresh Tokens for longer sessions

 2. Save conversion history per user

 3. Add multi-currency support (EUR, GBP, etc.)

4. Deploy to Azure App Service

---

# 📝 License

MIT License. Free to use and modify.
