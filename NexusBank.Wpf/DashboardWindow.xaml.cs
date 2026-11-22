
using System;
using System.Linq;
using System.Windows;

namespace NexusBank.Wpf
{
    public partial class DashboardWindow : Window
    {
        private readonly Customer currentCustomer;

        public DashboardWindow(Customer customer)
        {
            InitializeComponent();

            currentCustomer = customer;

            WelcomeText.Text =
                $"Witaj, {currentCustomer.FullName}!";

            LoadAccounts();
            LoadTransactions();
        }

        private void LoadAccounts()
        {
            var accountManager = new DataManager<Account>(
                DataPaths.GetPath("accounts.json"));

            var accounts = accountManager.LoadData();


            var userAccounts = accounts
                .Where(a => a.CustomerId == currentCustomer.Id)
                .ToList();

            AccountsGrid.ItemsSource = userAccounts;
            SourceAccountComboBox.ItemsSource = userAccounts;
            TargetAccountComboBox.ItemsSource = userAccounts;

        }

        private void DepositButton_Click(
            object sender, RoutedEventArgs e)
        {
            PerformOperation(true);
        }

        private void WithdrawButton_Click(
            object sender, RoutedEventArgs e)
        {
            PerformOperation(false);
        }

        private void PerformOperation(bool isDeposit)
        {
            if (AccountsGrid.SelectedItem is not Account selectedAccount)
            {
                MessageBox.Show("Najpierw wybierz konto z tabeli.");
                return;
            }

            if (!decimal.TryParse(
                AmountTextBox.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show(
                    "Wpisz poprawną kwotę większą od zera.");
                return;
            }

            try
            {
                var accountManager = new DataManager<Account>(
                    DataPaths.GetPath("accounts.json"));

                var transactionManager = new DataManager<Transaction>(
                    DataPaths.GetPath("transactions.json"));

                var accounts = accountManager.LoadData();
                var transactions = transactionManager.LoadData();

                var account = accounts.FirstOrDefault(a =>
                    a.AccountNumber == selectedAccount.AccountNumber
                    && a.CustomerId == currentCustomer.Id);

                if (account == null)
                {
                    MessageBox.Show(
                        "Nie znaleziono wybranego konta.");
                    return;
                }

                if (isDeposit)
                {
                    account.Deposit(amount);
                }
                else
                {
                    account.Withdraw(amount);
                }

                accountManager.SaveData(accounts);

                transactions.Add(new Transaction
                {
                    AccountNumber = account.AccountNumber,
                    Amount = amount,
                    Type = isDeposit ? "WPŁATA" : "WYPŁATA"
                });

                transactionManager.SaveData(transactions);

                MessageBox.Show(
                    $"Operacja zakończona.\nNowe saldo: {account.Balance:F2} {account.Currency}");

                AmountTextBox.Clear();
                LoadAccounts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd: {ex.Message}");
            }
        }

        private decimal GetExchangeRate(string from, string to)
        {
            if (from == to)
                throw new ArgumentException("Wybierz dwie różne waluty.");

            decimal plnPerUnit = from switch
            {
                "PLN" => 1m,
                "EUR" => 4.30m,
                "USD" => 4.00m,
                "GBP" => 5.00m,
                _ => throw new ArgumentException("Nieobsługiwana waluta źródłowa.")
            };

            decimal targetPlnPerUnit = to switch
            {
                "PLN" => 1m,
                "EUR" => 4.30m,
                "USD" => 4.00m,
                "GBP" => 5.00m,
                _ => throw new ArgumentException("Nieobsługiwana waluta docelowa.")
            };

            return plnPerUnit / targetPlnPerUnit;
        }

        private void ExchangeButton_Click(object sender, RoutedEventArgs e)
        {
            if (SourceAccountComboBox.SelectedItem is not Account source ||
                TargetAccountComboBox.SelectedItem is not Account target)
            {
                MessageBox.Show("Wybierz oba konta.");
                return;
            }

            if (source.AccountNumber == target.AccountNumber ||
                source.Currency == target.Currency)
            {
                MessageBox.Show("Wybierz dwa konta w różnych walutach.");
                return;
            }

            if (!decimal.TryParse(ExchangeAmountTextBox.Text, out decimal amount)
                || amount <= 0)
            {
                MessageBox.Show("Wpisz poprawną kwotę.");
                return;
            }

            try
            {
                var accountManager = new DataManager<Account>(
                    DataPaths.GetPath("accounts.json"));

                var transactionManager = new DataManager<Transaction>(
                    DataPaths.GetPath("transactions.json"));

                var accounts = accountManager.LoadData();
                var transactions = transactionManager.LoadData();

                var sourceAccount = accounts.FirstOrDefault(a =>
                    a.AccountNumber == source.AccountNumber &&
                    a.CustomerId == currentCustomer.Id);

                var targetAccount = accounts.FirstOrDefault(a =>
                    a.AccountNumber == target.AccountNumber &&
                    a.CustomerId == currentCustomer.Id);

                if (sourceAccount == null || targetAccount == null)
                {
                    MessageBox.Show("Nie znaleziono kont.");
                    return;
                }

                decimal rate = GetExchangeRate(
                    sourceAccount.Currency, targetAccount.Currency);

                decimal convertedAmount = Math.Round(
                    amount * rate, 2, MidpointRounding.AwayFromZero);

                if (convertedAmount <= 0)
                {
                    MessageBox.Show("Kwota po przeliczeniu jest zbyt mała.");
                    return;
                }

                sourceAccount.Withdraw(amount);
                targetAccount.Deposit(convertedAmount);

                transactions.Add(new Transaction
                {
                    AccountNumber = sourceAccount.AccountNumber,
                    Amount = -amount,
                    Type = $"WYMIANA NA {targetAccount.Currency}"
                });

                transactions.Add(new Transaction
                {
                    AccountNumber = targetAccount.AccountNumber,
                    Amount = convertedAmount,
                    Type = $"WYMIANA Z {sourceAccount.Currency}"
                });

                accountManager.SaveData(accounts);
                transactionManager.SaveData(transactions);

                LoadAccounts();
                ExchangeAmountTextBox.Clear();

                MessageBox.Show(
                    $"Wymieniono {amount:F2} {sourceAccount.Currency} " +
                    $"na {convertedAmount:F2} {targetAccount.Currency}.\n" +
                    $"Kurs: {rate:F4}",
                    "Wymiana zakończona");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd wymiany: {ex.Message}");
            }
        }


        private void LogoutButton_Click(
            object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            Close();
        }


        private void OpenAccountButton_Click(
            object sender, RoutedEventArgs e)
        {
            if (CurrencyComboBox.SelectedItem
                is not System.Windows.Controls.ComboBoxItem item)
            {
                MessageBox.Show("Wybierz walutę.");
                return;
            }

            string currency = item.Content.ToString()!;

            try
            {
                var accountManager = new DataManager<Account>(
                    DataPaths.GetPath("accounts.json"));

                var accounts = accountManager.LoadData();

                // Dla uproszczenia jedno konto w danej walucie.
                bool alreadyExists = accounts.Any(a =>
                    a.CustomerId == currentCustomer.Id &&
                    a.Currency == currency);

                if (alreadyExists)
                {
                    MessageBox.Show(
                        $"Masz już konto w walucie {currency}.");
                    return;
                }

                string prefix = currency switch
                {
                    "EUR" => "EU",
                    "USD" => "US",
                    "GBP" => "GB",
                    _ => "PL"
                };

                string accountNumber;

                // Sprawdź, czy wygenerowany numer nie istnieje.
                do
                {
                    accountNumber = prefix +
                        System.Security.Cryptography.RandomNumberGenerator
                            .GetInt32(10000000, 100000000)
                            .ToString() + "00";
                }
                while (accounts.Any(a =>
                    a.AccountNumber == accountNumber));

                var newAccount = new Account
                {
                    AccountNumber = accountNumber,
                    CustomerId = currentCustomer.Id,
                    Currency = currency
                };

                accounts.Add(newAccount);
                accountManager.SaveData(accounts);

                LoadAccounts();

                MessageBox.Show(
                    $"Utworzono konto {currency}!\nNumer: {accountNumber}",
                    "Sukces",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Nie udało się utworzyć konta: {ex.Message}");
            }
        }

        private void LoadTransactions()
        {
            var transactionManager = new DataManager<Transaction>(
                DataPaths.GetPath("transactions.json"));

            var accountManager = new DataManager<Account>(
                DataPaths.GetPath("accounts.json"));

            var accounts = accountManager.LoadData();
            var transactions = transactionManager.LoadData();

            var userAccountNumbers = accounts
                .Where(a => a.CustomerId == currentCustomer.Id)
                .Select(a => a.AccountNumber)
                .ToHashSet();

            TransactionsGrid.ItemsSource = transactions
                .Where(t => userAccountNumbers.Contains(t.AccountNumber))
                .OrderByDescending(t => t.Date)
                .ToList();
        }

        private void RefreshHistoryButton_Click(
            object sender, RoutedEventArgs e)
        {
            LoadTransactions();
        }


    }
}
