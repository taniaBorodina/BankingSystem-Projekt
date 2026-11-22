
using System;
using System.Linq;
using System.Windows;

namespace NexusBank.Wpf
{
    public partial class RegisterWindow : Window
    {
        private readonly DataManager<Customer> customerManager =
            new DataManager<Customer>(
                DataPaths.GetPath("customers.json"));

        private readonly DataManager<Account> accountManager =
            new DataManager<Account>(
                DataPaths.GetPath("accounts.json"));

        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(
            object sender, RoutedEventArgs e)
        {
            string fullName = FullNameTextBox.Text.Trim();
            string login = NewLoginTextBox.Text.Trim();
            string pin = NewPinPasswordBox.Password;
            string confirmPin = ConfirmPinPasswordBox.Password;

            RegisterMessageText.Text = "";

            // Sprawdzamy, czy wszystkie pola są wypełnione
            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(pin) ||
                string.IsNullOrWhiteSpace(confirmPin))
            {
                RegisterMessageText.Text =
                    "Wypełnij wszystkie pola.";
                return;
            }

            // Login musi mieć przynajmniej 3 znaki
            if (login.Length < 3)
            {
                RegisterMessageText.Text =
                    "Login musi mieć minimum 3 znaki.";
                return;
            }

            // Sprawdzamy PIN
            if (pin.Length != 4 ||
                !pin.All(c => c >= '0' && c <= '9'))
            {
                RegisterMessageText.Text =
                    "PIN musi zawierać dokładnie 4 cyfry.";
                return;
            }

            // Sprawdzamy, czy PIN-y są takie same
            if (pin != confirmPin)
            {
                RegisterMessageText.Text =
                    "Podane PIN-y nie są identyczne.";
                return;
            }

            try
            {
                var customers = customerManager.LoadData();
                var accounts = accountManager.LoadData();

                // Sprawdzamy, czy login jest już zajęty
                bool loginExists = customers.Any(c =>
                    string.Equals(
                        c.Login,
                        login,
                        StringComparison.OrdinalIgnoreCase));

                if (loginExists)
                {
                    RegisterMessageText.Text =
                        "Ten login jest już zajęty.";
                    return;
                }

                // Tworzymy nowego klienta

                Customer newCustomer = new Customer
                {
                    Id = Guid.NewGuid(),
                    FullName = fullName,
                    Login = login,
                    Pin = PinSecurity.HashPin(pin)
                };


                // Generujemy unikalny numer konta
                string accountNumber;

                do
                {
                    accountNumber = "PL" +
                        System.Security.Cryptography.RandomNumberGenerator
                            .GetInt32(10000000, 100000000)
                            .ToString() + "00";
                }
                while (accounts.Any(a =>
                    a.AccountNumber == accountNumber));

                // Tworzymy pierwsze konto PLN
                Account newAccount = new Account
                {
                    AccountNumber = accountNumber,
                    Currency = "PLN",
                    CustomerId = newCustomer.Id
                };

                // Zapisujemy klienta i konto
                customers.Add(newCustomer);
                accounts.Add(newAccount);

                customerManager.SaveData(customers);
                accountManager.SaveData(accounts);

                MessageBox.Show(
                    $"Rejestracja zakończona!\n\n" +
                    $"Witaj, {fullName}!\n" +
                    $"Numer konta: {accountNumber}\n" +
                    $"Waluta: PLN\n" +
                    $"Saldo: 0,00 PLN\n\n" +
                    $"Możesz się teraz zalogować.",
                    "NexusBank",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                Close();
            }
            catch (Exception ex)
            {
                RegisterMessageText.Text =
                    $"Błąd rejestracji: {ex.Message}";
            }
        }

        private void BackButton_Click(
            object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
