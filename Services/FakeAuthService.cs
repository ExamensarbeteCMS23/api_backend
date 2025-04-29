using System;

namespace api_backend.Services
{
    public class FakeAuthService
    {
        // För testning, sätt önskad roll här
        private readonly string _currentRole = "Admin"; // Byt till "Cleaner" för att testa den rollen

        public bool IsAdmin() => _currentRole == "Admin";
        public bool IsCleaner() => _currentRole == "Cleaner";
        public string GetCurrentRole() => _currentRole;

        // För att simulera en städare som är inloggad
        public int GetCurrentCleanerId()
        {
            // I en riktig implementation skulle detta hämtas från den autentiserade användaren
            // För nu returnerar vi bara ett hårdkodat ID (anpassa till ett ID som finns i din databas)
            return 2; // Använd ID för en städare som finns i databasen
        }
    }
}