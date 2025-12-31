using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace FunctionAssignment
{
    public enum SearchType
    {
        Country = 1,
        Company = 2,
        ContactPerson = 3
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Company { get; set; }
        public string ContactPerson { get; set; }
        public string Country { get; set; }
    }

    public interface ICustomerFilter
    {
        List<Customer> Filter(List<Customer> customerList, string criteria);
    }

    public class CountryCustomerFilter : ICustomerFilter
    {
        public List<Customer> Filter(List<Customer> customerList, string country)
        {
            return customerList.Where(customer => customer.Country.Contains(country)).OrderBy(customer => customer.Id).ToList();
        }
    }

    public class CompanyCustomerFilter : ICustomerFilter
    {
        public List<Customer> Filter(List<Customer> customerList, string company)
        {
            return customerList.Where(customer => customer.Company.Contains(company)).OrderBy(customer => customer.Id).ToList();
        }
    }

    public class ContactPersonCustomerFilter : ICustomerFilter
    {
        public List<Customer> Filter(List<Customer> customerList, string contactPerson)
        {
            return customerList.Where(customer => customer.ContactPerson.Contains(contactPerson)).OrderBy(customer => customer.Id).ToList();
        }
    }

    public class CustomerRepository
    {
        private readonly List<Customer> customerDatabase;
        public CustomerRepository(List<Customer> customerList)
        {
            customerDatabase = customerList;
        }
        public List<Customer> GetCustomers(ICustomerFilter filter, string criteria)
        {
            return filter.Filter(customerDatabase, criteria);
        }
    }

    public class CsvExporter
    {
        public string ConvertCustomersToCsv(List<Customer> customerList)
        {
            StringBuilder csvContent = new StringBuilder();
            csvContent.AppendLine("CustomerID,Company,ContactPerson,Country");

            foreach (var customer in customerList)
            {
                csvContent.AppendFormat("{0},{1},{2},{3}", customer.Id, customer.Company, customer.ContactPerson, customer.Country);
                csvContent.AppendLine();
            }

            return csvContent.ToString();
        }
    }
    class CustomerSearch
    {
        private static ICustomerFilter CreateFilter(SearchType searchType)
        {
            switch (searchType)
            {
                case SearchType.Country: return new CountryCustomerFilter();
                case SearchType.Company: return new CompanyCustomerFilter();
                case SearchType.ContactPerson: return new ContactPersonCustomerFilter();
            }
            return null;
        }

        private static void ShowMenu()
        {
            Console.WriteLine("Choose search option:");
            Console.WriteLine("1. Country");
            Console.WriteLine("2. Company");
            Console.WriteLine("3. Contact Person");
        }

        private static int GetValidChoice()
        {
            while (true)
            {
                Console.Write("Enter choice (1-3): ");
                if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= 3)
                {
                    return choice;
                }
                Console.WriteLine("Invalid choice. Please enter 1 to 3.");
            }
        }
        static void Main(string[] args)
        {
            List<Customer> customers = new List<Customer>
            {
                new Customer { Id = 1, Company = "TCS", ContactPerson = "Amit", Country = "India" },
                new Customer { Id = 2, Company = "ITT", ContactPerson = "Sarah", Country = "USA" },
                new Customer { Id = 3, Company = "Nav", ContactPerson = "Ravi", Country = "India" },
                new Customer { Id = 4, Company = "Infosys", ContactPerson = "Ritika", Country = "Spain" }
            };

            CustomerRepository repository = new CustomerRepository(customers);
            CsvExporter exporter = new CsvExporter();
            ShowMenu();
            int choice = GetValidChoice();
            SearchType searchType = (SearchType)choice;
            ICustomerFilter filter = CreateFilter(searchType);
            Console.Write("Enter search text: ");
            string criteria = Console.ReadLine();
            var result = repository.GetCustomers(filter, criteria);
            if (result.Count == 0)
            {
                Console.WriteLine("\nNo matching records found.");
                return;
            }
            string csvOutput = exporter.ConvertCustomersToCsv(result);
            Console.WriteLine("\nResult:");
            Console.WriteLine(csvOutput);
        }

    }
}
