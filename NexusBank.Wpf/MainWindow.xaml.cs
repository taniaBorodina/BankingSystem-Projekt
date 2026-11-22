
using System;
using System.Linq;
using System.Windows;

namespace NexusBank.Wpf
{
    public partial class MainWindow : Window
    {
        private readonly DataManager<Customer> customerManager =
    new DataManager<Customer>(
        DataPaths.GetPath("customers.json"));

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string pin = PinPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(pin))
            {
                MessageText.Text = "Wpisz login oraz PIN.";
                return;
            }

            var customers = customerManager.LoadData();


            var customer = customers.FirstOrDefault(c =>
                string.Equals(
                    c.Login,
                    login,
                    StringComparison.OrdinalIgnoreCase)
                && PinSecurity.VerifyPinOrLegacy(pin, c.Pin));


            if (customer == null)
            {
                MessageText.Text = "Nieprawidłowy login lub PIN.";
                return;
            }

            MessageText.Text = "";

            if (PinSecurity.IsLegacyPin(customer.Pin))
            {
                customer.Pin = PinSecurity.HashPin(pin);
                customerManager.SaveData(customers);
            }



            DashboardWindow dashboard = new DashboardWindow(customer);

            dashboard.Show();

            Close();

        }

        private void RegisterButton_Click(
            object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();

            registerWindow.Owner = this;

            registerWindow.ShowDialog();
        }



    }



}
