using System.Net.Http;
using System.Text.Json;
using MySqlConnector;

var cfg = JsonDocument.Parse(File.ReadAllText("appsettings.json")).RootElement;
string connStr = cfg.GetProperty("ConnectionStrings").GetProperty("MySql").GetString()!;
string baseUrl = cfg.GetProperty("Api").GetProperty("BaseUrl").GetString()!;
string defaultFiat = cfg.GetProperty("Api").GetProperty("DefaultFiat").GetString()!;
var supportedFiats = cfg.GetProperty("Api").GetProperty("SupportedFiats").EnumerateArray()
                        .Select(x => x.GetString()!).ToList();

// Allow override:  dotnet run -- USD   (or set FIAT=USD as env var)
string selectedFiat = Environment.GetEnvironmentVariable("FIAT")
                      ?? (args.Length > 0 ? args[0] : defaultFiat);

// Validate selection
if (!supportedFiats.Contains(selectedFiat, StringComparer.OrdinalIgnoreCase))
{
    Console.WriteLine($"Unsupported FIAT '{selectedFiat}'. Falling back to {defaultFiat}.");
    selectedFiat = defaultFiat;
}

using var http = new HttpClient();

Console.WriteLine("=== NSN Price Updater Started ===");
Console.WriteLine($"Using FIAT: {selectedFiat}");
Console.WriteLine("Fetching prices every 5 minutes...\n");

while (true)
{
    try
    {
        var symbols = new List<string>();
        await using (var conn = new MySqlConnection(connStr))
        {
            await conn.OpenAsync();
            var cmd = new MySqlCommand("SELECT Symbol FROM Tokens", conn);
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                var s = rdr.GetString(0);
                if (!string.IsNullOrWhiteSpace(s)) symbols.Add(s.ToUpperInvariant());
            }
        }

        foreach (var sym in symbols)
        {
            decimal price = 0;
            try
            {
                var url = $"{baseUrl}?fsym={sym}&tsyms={selectedFiat}";
                var json = await http.GetStringAsync(url);

                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty(selectedFiat, out var v) &&
                    v.TryGetDecimal(out var dec))
                    price = dec;
            }
            catch { price = 0; }

            await using (var conn = new MySqlConnection(connStr))
            {
                await conn.OpenAsync();
                var update = new MySqlCommand(
                    "UPDATE Tokens SET Price = @p WHERE UPPER(Symbol) = @s", conn);
                update.Parameters.AddWithValue("@p", price);
                update.Parameters.AddWithValue("@s", sym);
                await update.ExecuteNonQueryAsync();
            }

            Console.WriteLine($"{DateTime.Now}: {sym} -> {selectedFiat} {price:N2}");
        }
    }
    catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }

    await Task.Delay(TimeSpan.FromMinutes(5));
}