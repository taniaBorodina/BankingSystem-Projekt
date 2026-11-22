
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
                c.Login.Equals(login, StringComparison.OrdinalIgnoreCase)
                && c.Pin == pin);

            if (customer == null)
            {
                MessageText.Text = "Nieprawidłowy login lub PIN.";
                return;
            }

            MessageText.Text = "";


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
