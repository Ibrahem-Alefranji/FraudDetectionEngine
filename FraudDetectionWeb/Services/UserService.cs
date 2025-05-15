using Dapper;
using FraudDetectionWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Text;

public class UserService
{
    private readonly IConfiguration _config;
    public UserService(IConfiguration config) => _config = config;

    private IDbConnection CreateConnection() =>
        new SqlConnection(_config.GetConnectionString("DefaultConnection"));

    public async Task<int> CreateUserAsync(User user)
    {
        using var conn = CreateConnection();
        var sql = @"
            INSERT INTO Users (FullName, PhoneNumber, Username, PasswordHash, Email, Country, Region, CreatedOn, IsAdmin)
            VALUES (@FullName,@PhoneNumber, @Username, @PasswordHash, @Email, @Country, @Region, @CreatedOn, @IsAdmin);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        user.PasswordHash = HashPassword(user.PasswordHash);
        user.CreatedOn = DateTime.UtcNow;

        return await conn.ExecuteScalarAsync<int>(sql, user);
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Id = @Id", new { Id = id });
    }

    public async Task<IEnumerable<User>> GetAllSubscribUsersAsync()
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<User>("SELECT * FROM Users WHERE IsAdmin = 0");
    }

    public async Task<JsonResult> GetAllUsersAsync(int draw, int start, int length, string query)
    {
        using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        var where = "WHERE 1=1";
        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(query))
        {
            where += " AND (FullName LIKE @search OR Username LIKE @search OR Email LIKE @search OR PhoneNumber LIKE @search)";
            parameters.Add("search", $"%{query}%");
        }

        var sql = $@"
            SELECT COUNT(*) FROM Users {where};
            SELECT *
            FROM Users
            {where}
            ORDER BY Id DESC
            OFFSET @start ROWS FETCH NEXT @length ROWS ONLY;
        ";

        parameters.Add("start", start);
        parameters.Add("length", length);

        using var multi = await conn.QueryMultipleAsync(sql, parameters);

        var totalFiltered = await multi.ReadFirstAsync<int>();
        var data = (await multi.ReadAsync<User>()).ToList();

        // Optionally: get total records if different from filtered
        var totalRecords = totalFiltered; // Or: await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Transactions");

        return new JsonResult(new
        {
            draw,
            recordsTotal = totalRecords,
            recordsFiltered = totalFiltered,
            data
        });
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        using var conn = CreateConnection();
        var sql = @"
            UPDATE Users SET
                FullName = @FullName,
                PhoneNumber = @PhoneNumber,
                Country = @Country,
                Region = @Region,
                Username = @Username,
                Email = @Email
            WHERE Id = @Id";

        return await conn.ExecuteAsync(sql, user) > 0;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        using var conn = CreateConnection();
        return await conn.ExecuteAsync("DELETE FROM Users WHERE Id = @Id", new { Id = id }) > 0;
    }

    public async Task<User?> SignInAsync(string username, string password)
    {
        using var conn = CreateConnection();
        var user = await conn.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Username = @Username", new { Username = username });

        if (user is null) return null;

        return VerifyPassword(password, user.PasswordHash) ? user : null;
    }

    // Hashing utilities
    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }
}
