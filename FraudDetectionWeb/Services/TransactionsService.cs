using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Dapper;
using FraudDetectionWeb.DTOs;
using Microsoft.Data.SqlClient; // Adjust namespace as needed

public class TransactionsService : PageModel
{
    private readonly IConfiguration _config;

    public TransactionsService(IConfiguration config)
    {
        _config = config;
    }

    public IActionResult OnGet() => Page();

    public async Task<JsonResult> OnGetTransactionsAsync(
        int draw,
        int start,
        int length,
        string? search,
        float? amount,
        int? type)
    {
        using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        var where = "WHERE pay.SubscribeId = sub.Id";
        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(search))
        {
            where += " AND (pay.OrderId LIKE @search OR sub.BusinessName LIKE @search OR pay.IPAddress LIKE @search)";
            parameters.Add("search", $"%{search}%");
        }

        if (amount.HasValue)
        {
            where += " AND pay.Amount = @Amount";
            parameters.Add("Amount", amount);
        }

        if (type.HasValue)
        {
            where += " AND pay.TransactionType = @type";
            parameters.Add("type", type);
        }

        var sql = $@"
            SELECT COUNT(*) FROM PaymentTransaction pay, Subscription sub  {where};
            SELECT pay.*, sub.BusinessName, CASE WHEN pay.TransactionType = 1 THEN 'Purchase' ELSE 'Return' END as TransactionTypeText
            FROM PaymentTransaction pay, Subscription sub 
            {where}
            ORDER BY pay.CreatedOn DESC
            OFFSET @start ROWS FETCH NEXT @length ROWS ONLY;
        ";

        parameters.Add("start", start);
        parameters.Add("length", length);

        using var multi = await conn.QueryMultipleAsync(sql, parameters);

        var totalFiltered = await multi.ReadFirstAsync<int>();
        var data = (await multi.ReadAsync<TransactionsResponse>()).ToList();

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
}
