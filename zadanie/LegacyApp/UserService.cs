using System;
using System.Reflection.Metadata.Ecma335;

namespace LegacyApp
{
    public class UserService
    {
        public User user { get; set; }
        public Client client { get; set; }
        public ClientRepository clientRepository { get; set; }

        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                return false;
            }

            if (!email.Contains("@") && !email.Contains("."))
            {
                return false;
            }

            if (isUnder21(dateOfBirth))
            {
                return false;
            }

            clientRepository = new ClientRepository();
            client = clientRepository.GetById(clientId);
            user = new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName
            };

            ClientTypeCredit();

            if (user.HasCreditLimit && user.CreditLimit < 500)
            {
                return false;
            }

            UserDataAccess.AddUser(user);
            return true;
        }

        private bool isUnder21(DateTime dateOfBirth)
        {
            var now = DateTime.Now;
            int age = now.Year - dateOfBirth.Year;
            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day)) age--;

            return (age < 21) ? true : false;
        }

        private void ClientTypeCredit()
        {
            switch (client.Type)
            {
                case "VeryImportantClient":
                    user.HasCreditLimit = false;

                    break;
                case "ImportantClient":
                    using (var userCreditService = new UserCreditService())
                    {
                        user.CreditLimit = userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth) * 2;
                    }

                    break;
                case "NormalClient":
                    user.HasCreditLimit = true;
                    using (var userCreditService = new UserCreditService())
                    {
                        user.CreditLimit = userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                    }

                    break;
            }
        }
    }
}