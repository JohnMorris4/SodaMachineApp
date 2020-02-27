using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SodaMachineLibrary.DataAccess;
using SodaMachineLibrary.Logic;
using SodaMachineLibrary.Models;

namespace SodaMachineConsoleUI
{
    
    
    class Program
    {
        private static IServiceProvider _serviceProvider;
        private static ISodaMachineLogic _sodaMachine;
        private static string userId;
        
        static void Main(string[] args)
        {
            RegisterServices();
            _sodaMachine = _serviceProvider.GetService<ISodaMachineLogic>();
            userId = new Guid().ToString();
            
            string userSelection = "";
            Console.WriteLine("Welcome to our Soda Machine");
            Console.WriteLine("What product would you like today:");
            do
            {
                userSelection = ShowMenu();
                switch (userSelection)
                {
                    //test git
                    case "1": // Show Soda Price
                        ShowSodaPrice();
                        break;
                    case "2": // List Soda Options
                        ListSodaOptions();
                        break;
                    case "3": // Show Amount Deposited
                        ShowAmountDeposited();
                        break;
                    case "4": // Deposit Money
                        DepositMoney();
                        break;
                    case "5": // Cancel Transaction
                        CancelTransaction();
                        break;
                    case "6": // Request Soda
                        RequestSoda();
                        break;
                    case "9": // Close Machine
                        // Don't do anything - allow this to go to the while, which will exit the loop
                        break;
                    default:
                        // Don't do anything - allow this to go to the while, which will restart the loop
                        break;
                }
                Console.Clear();
            } while (userSelection != "9");

            Console.WriteLine("Thanks have a nice day");
            Console.WriteLine("Press return to quit");
            Console.ReadLine();
        }

        private static void RequestSoda()
        {
            var sodaOptions = _sodaMachine.ListTypesOfSoda();
            var i = 1;
            Console.Clear();

            Console.WriteLine($"The soda options are ");
            
            sodaOptions.ForEach(x => Console.WriteLine($"{i++} - {x.Name}"));

            string sodaIndentifier = Console.ReadLine();
            bool isValidNumber = int.TryParse(sodaIndentifier, out int sodaNumber);
            SodaModel soda;

            try
            {
                soda = sodaOptions[sodaNumber - 1];
            }
            catch 
            {
                Console.WriteLine("That was not a correct response");
                Message();
                return;
            }
            
            var results = _sodaMachine.RequestSoda(soda, userId);
            if (results.errorMessage.Length > 0)
            {
                Console.WriteLine(results.errorMessage);
            }
            else
            {
                Console.WriteLine($"Here is your {results.soda.Name}");
                if (results.change.Count > 0)
                {
                    Console.WriteLine("Here is your change: ");
                    results.change.ForEach(x=> Console.WriteLine(x.Name));
                }
                else
                {
                    Console.WriteLine("There is now change");
                }
            }
            
            Message();
            
        }

        private static void CancelTransaction()
        {
            var moneyInserted = _sodaMachine.GetMoneyInsertedTotal(userId);
            _sodaMachine.IssueFullRefund(userId);
            Console.Clear();
            Console.WriteLine($"You have been refunded  {string.Format("{0:$ #.00}", moneyInserted)}");
            Message();
            
        }

        private static void ShowAmountDeposited()
        {
            var amountDeposited = _sodaMachine.GetMoneyInsertedTotal(userId);
            Console.Clear();
            Console.WriteLine($"You have deposited  {string.Format("{0:$ #.00}", amountDeposited)} thus far..");
            Message();
        }

        private static void DepositMoney()
        {
            Console.WriteLine("How much would you like to add to the machine?");
            
            string amountText = Console.ReadLine();
            
            bool isValidAmount = decimal.TryParse(amountText, out decimal amountAdded);
            
            _sodaMachine.MoneyInserted(userId, amountAdded);
        }

        private static void ListSodaOptions()
        {
            var sodaOptions = _sodaMachine.ListTypesOfSoda();
            Console.Clear();

            Console.WriteLine($"The soda options are ");
            
            sodaOptions.ForEach(x => Console.WriteLine(x.Name));
            Message();
        }

        private static void ShowSodaPrice()
        {
            var sodaPrice = _sodaMachine.GetSodaPrice();
            Console.Clear();
            Console.WriteLine($"The price of the soda is {string.Format("{0:$ #.00}", sodaPrice)}");
            Message();
        }

        private static string ShowMenu()
        {
            Console.WriteLine("Please make a selection from the following options:");
            Console.WriteLine("1: Show Soda Price");
            Console.WriteLine("2: List Soda Options");
            Console.WriteLine("3: Show Amount Deposited");
            Console.WriteLine("4: Deposit Money");
            Console.WriteLine("5: Cancel Transaction");
            Console.WriteLine("6: Request Soda");
            Console.WriteLine("9: Close Machine");

            return Console.ReadLine();
        }
        private static void RegisterServices()
        {
            var collection = new ServiceCollection();
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            collection.AddSingleton(config);
            collection.AddTransient<IDataAccess, TextFileDataAccess>();
            collection.AddTransient<ISodaMachineLogic, SodaMachineLogic>();
            _serviceProvider = collection.BuildServiceProvider();
        }


        private static void Message()
        {
            Console.WriteLine();
            Console.WriteLine("Press enter to continue");
            Console.ReadLine();

        }
    }
}