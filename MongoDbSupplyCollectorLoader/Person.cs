using System.Collections.Generic;

namespace MongoDbSupplyCollectorLoader
{
    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Dictionary<string, Address> Addresses { get; set; }
        public List<Phone> PhoneNumbers { get; set; }

        public Person()
        {
            Addresses = new Dictionary<string, Address>();
            PhoneNumbers = new List<Phone>();
        }

        public Person(int suffix, int AddressCount, int PhoneCount) : this()
        {
            FirstName = "First" + suffix;
            LastName = "Last" + suffix;
            for (int i = 0; i < AddressCount; i++)
            {
                var address = new Address(i);
                Addresses["type" + i] = address;
            }
            for (int i = 0; i < PhoneCount; i++)
            {
                PhoneNumbers.Add(new Phone(i));
            }
        }
    }

    public class Address
    {
        public string Street1 { get; set; }
        public string Street2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }

        public Address() { }
        public Address(int suffix)
        {
            Street1 = "Street1" + suffix;
            Street2 = "Street2" + suffix;
            City = "City" + suffix;
            State = "State" + suffix;
            Zip = "Zip" + suffix;
        }
    }

    public class Phone
    {
        public string Type { get; set; }
        public string CountryCode { get; set; }
        public string Number { get; set; }

        public Phone() { }

        public Phone(int suffix)
        {
            Type = "Type" + suffix;
            CountryCode = "CountryCode" + suffix;
            Number = "Number" + suffix;
        }
    }
}
