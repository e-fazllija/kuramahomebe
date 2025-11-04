using BackEnd.Entities;
using Microsoft.AspNetCore.Identity;

namespace BackEnd.Services
{
    /// <summary>
    /// Servizio per gestire i controlli di accesso e visibilità nella cerchia di utenti
    /// </summary>
    public class AccessControlService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AccessControlService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Ottiene tutti gli ID degli utenti nella cerchia dell'utente corrente.
        /// La cerchia include:
        /// - Admin: proprie + Agency + tutti gli Agent (diretti e delle Agency)
        /// - Agency: proprie + tutti i propri Agent
        /// - Agent: proprie + colleghi (stessa Agency o stesso Admin)
        /// </summary>
        public async Task<List<string>> GetCircleUserIdsFor(string currentUserId)
        {
            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser == null)
                return new List<string> { currentUserId };

            var currentUserRoles = await _userManager.GetRolesAsync(currentUser);
            var circleIds = new List<string> { currentUserId };

            if (currentUserRoles.Contains("Admin"))
            {
                // Admin: proprie + Agency + tutti gli Agent della cerchia
                var agencies = await _userManager.GetUsersInRoleAsync("Agency");
                var myAgencies = agencies.Where(x => x.AdminId == currentUserId).ToList();
                circleIds.AddRange(myAgencies.Select(x => x.Id));

                var myAgencyIds = myAgencies.Select(x => x.Id).ToList();

                var agents = await _userManager.GetUsersInRoleAsync("Agent");
                var myAgents = agents.Where(x =>
                    x.AdminId == currentUserId ||  // Agent diretti dell'Admin
                    myAgencyIds.Contains(x.AdminId) // Agent delle Agency dell'Admin
                );
                circleIds.AddRange(myAgents.Select(x => x.Id));
            }
            else if (currentUserRoles.Contains("Agency"))
            {
                // Agency: proprie + suoi Agent
                var agents = await _userManager.GetUsersInRoleAsync("Agent");
                var myAgents = agents.Where(x => x.AdminId == currentUserId);
                circleIds.AddRange(myAgents.Select(x => x.Id));
            }
            else if (currentUserRoles.Contains("Agent"))
            {
                // Agent: colleghi (stessa Agency o stesso Admin)
                var myAgencyId = currentUser.AdminId;

                if (!string.IsNullOrEmpty(myAgencyId))
                {
                    // Aggiungi l'Agency/Admin stesso
                    circleIds.Add(myAgencyId);

                    // Trova tutti gli Agent con stesso AgencyId (colleghi)
                    var agents = await _userManager.GetUsersInRoleAsync("Agent");
                    var colleagues = agents.Where(x => x.AdminId == myAgencyId && x.Id != currentUserId);
                    circleIds.AddRange(colleagues.Select(x => x.Id));
                }
            }

            return circleIds.Distinct().ToList();
        }

        /// <summary>
        /// Verifica se l'utente corrente può modificare un'entità creata da entityCreatorId.
        /// Regole:
        /// - Admin: può modificare tutto nella sua cerchia
        /// - Agency: può modificare solo entità proprie o dei propri Agent
        /// - Agent: può modificare SOLO le proprie entità
        /// </summary>
        public async Task<bool> CanModifyEntity(string currentUserId, string entityCreatorId)
        {
            // Stesso utente - sempre OK
            if (currentUserId == entityCreatorId)
                return true;

            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser == null)
                return false;

            var currentUserRoles = await _userManager.GetRolesAsync(currentUser);

            // Agent non può mai modificare entità di altri
            if (currentUserRoles.Contains("Agent"))
                return false;

            var creator = await _userManager.FindByIdAsync(entityCreatorId);
            if (creator == null)
                return false;

            if (currentUserRoles.Contains("Admin"))
            {
                // Admin può modificare tutto nella sua cerchia
                var circleIds = await GetCircleUserIdsFor(currentUserId);
                return circleIds.Contains(entityCreatorId);
            }
            else if (currentUserRoles.Contains("Agency"))
            {
                // Agency può modificare solo entità dei propri Agent
                return creator.AdminId == currentUserId;
            }

            return false;
        }

        /// <summary>
        /// Verifica se l'utente corrente può accedere (visualizzare) un'entità.
        /// Controlla se l'entità è nella cerchia dell'utente.
        /// </summary>
        public async Task<bool> CanAccessEntity(string currentUserId, string entityCreatorId)
        {
            var circleIds = await GetCircleUserIdsFor(currentUserId);
            return circleIds.Contains(entityCreatorId);
        }

        /// <summary>
        /// Determina se l'utente corrente è superiore (Admin/Agency) del creatore dell'entità.
        /// Usato per decidere se mostrare dati sensibili.
        /// </summary>
        public async Task<bool> IsSuperiorOf(string currentUserId, string entityCreatorId)
        {
            if (currentUserId == entityCreatorId)
                return false; // Stesso utente = owner, non superiore

            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser == null)
                return false;

            var currentUserRoles = await _userManager.GetRolesAsync(currentUser);

            // Agent non è mai superiore
            if (currentUserRoles.Contains("Agent"))
                return false;

            var creator = await _userManager.FindByIdAsync(entityCreatorId);
            if (creator == null)
                return false;

            if (currentUserRoles.Contains("Admin"))
            {
                // Admin è superiore se il creatore è nella sua cerchia
                var circleIds = await GetCircleUserIdsFor(currentUserId);
                return circleIds.Contains(entityCreatorId);
            }
            else if (currentUserRoles.Contains("Agency"))
            {
                // Agency è superiore se il creatore è un suo Agent
                return creator.AdminId == currentUserId;
            }

            return false;
        }
    }
}

