# NSN Technology API Price Console App
## Project Overview
The NSN Technology API Price Console App is an extension utility that automatically fetches live cryptocurrency prices and updates the NSN Dashboard database at regular intervals.
It works alongside the main NSN Token Dashboard to ensure that all tokens display the latest USD prices in real time.

## Project Objectives
This console application connects to the CryptoCompare API and retrieves token prices based on their symbols stored in the database.
It is designed to run as a lightweight background service or scheduled task.

# Instructions to Run
1. **Clone the repository**
   ```bash
   git clone https://github.com/weishen1113/PriceUpdater.git
   cd PriceUpdater
   ```
2. **Edit `appsettings.json` to point to your MySQL instance:**
   ```bash
   "ConnectionStrings": {
    "DefaultConnection": "Server=127.0.0.1;Port=3306;Database=nsn_tokens;User ID=nsn_user;Password=<your_password_here>;TreatTinyAsBoolean=false;"
    }
   ```

3. **Ensure database schema is ready**
    - Your MySQL database should contain a tokens table.
    - Add the missing price column if not already present:
    ```bash
    ALTER TABLE tokens
    ADD COLUMN price DECIMAL(18,2) NULL DEFAULT 0;
    ```

4. **Run the console app**
    The process runs every 5 minutes and pulls prices for all tokens:
   ```bash
   dotnet run
   ```
   - **Expected output**:
    ```bash
    === NSN Price Updater Started ===
    Using FIAT: USD
    Fetching prices every 5 minutes...

    11/11/2025 4:31:38 AM: BTC -> USD 106,021.36
    11/11/2025 4:31:38 AM: ABC EDIT -> USD 0.00
    11/11/2025 4:31:39 AM: WS 45 EDIT -> USD 0.00
    11/11/2025 4:31:39 AM: WS ABCD -> USD 0.00
    11/11/2025 4:31:39 AM: DSAFA -> USD 0.00
    11/11/2025 4:31:39 AM: SFGD -> USD 0.00
    11/11/2025 4:36:41 AM: BTC -> USD 106,196.55
    11/11/2025 4:36:41 AM: ABC EDIT -> USD 0.00
    11/11/2025 4:36:41 AM: WS 45 EDIT -> USD 0.00
    11/11/2025 4:36:41 AM: WS ABCD -> USD 0.00
    11/11/2025 4:36:42 AM: DSAFA -> USD 0.00
    11/11/2025 4:36:42 AM: SFGD -> USD 0.00
    ```