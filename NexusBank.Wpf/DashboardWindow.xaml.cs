
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
        }

        private void LoadAccounts()
        {
            var accountManager = new DataManager<Account>(
                DataPaths.GetPath("accounts.json"));

            var accounts = accountManager.LoadData();

            AccountsGrid.ItemsSource = accounts
                .Where(a => a.CustomerId == currentCustomer.Id)
                .ToList();
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

        private void LogoutButton_Click(
            object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            Close();
        }
    }
}
