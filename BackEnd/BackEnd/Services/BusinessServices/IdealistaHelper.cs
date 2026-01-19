using BackEnd.Entities;
using Microsoft.AspNetCore.Identity;

namespace BackEnd.Services.BusinessServices
{
    public static class IdealistaHelper
    {
        /// <summary>
        /// Trova l'utente admin di riferimento per un utente.
        /// Se l'utente è admin, ritorna se stesso.
        /// Se l'utente non è admin, cerca l'admin tramite AdminId.
        /// </summary>
        public static async Task<ApplicationUser?> GetAdminUserAsync(
            ApplicationUser user, 
            UserManager<ApplicationUser> userManager)
        {
            var userRoles = await userManager.GetRolesAsync(user);
            
            // Se l'utente è admin, ritorna se stesso
            if (userRoles.Contains("Admin"))
            {
                return user;
            }
            
            // Se l'utente ha un AdminId, cerca l'admin
            if (!string.IsNullOrEmpty(user.AdminId))
            {
                var admin = await userManager.FindByIdAsync(user.AdminId);
                if (admin != null)
                {
                    var adminRoles = await userManager.GetRolesAsync(admin);
                    if (adminRoles.Contains("Admin"))
                    {
                        return admin;
                    }
                }
            }
            
            return null;
        }

        /// <summary>
        /// Verifica se la sincronizzazione con Idealista è abilitata per l'utente.
        /// Controlla SyncToIdealista dell'admin di riferimento.
        /// </summary>
        public static async Task<bool> ShouldSyncToIdealistaAsync(
            ApplicationUser user,
            UserManager<ApplicationUser> userManager)
        {
            var adminUser = await GetAdminUserAsync(user, userManager);
            
            if (adminUser == null)
            {
                return false;
            }
            
            // Verifica che l'admin abbia ClientId e ClientSecret
            if (string.IsNullOrEmpty(adminUser.ClientId) || string.IsNullOrEmpty(adminUser.ClientSecret))
            {
                return false;
            }
            
            // Verifica SyncToIdealista
            return adminUser.SyncToIdealista == true;
        }

        /// <summary>
        /// Ottiene le credenziali Idealista dall'admin di riferimento.
        /// </summary>
        public static async Task<(string? clientId, string? clientSecret, string? feedKey)> GetIdealistaCredentialsAsync(
            ApplicationUser user,
            UserManager<ApplicationUser> userManager)
        {
            var adminUser = await GetAdminUserAsync(user, userManager);
            
            if (adminUser == null)
            {
                return (null, null, null);
            }
            
            // Il feedKey è il ClientId stesso secondo la documentazione
            return (adminUser.ClientId, adminUser.ClientSecret, adminUser.ClientId);
        }
    }
}

