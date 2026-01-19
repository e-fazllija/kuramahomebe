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
        /// - Agency: proprie + suoi Agent + Admin + altre Agency dello stesso Admin + tutti gli Agent dello stesso Admin
        /// - Agent: proprie + Agency/Admin + colleghi + altri Agent dello stesso Admin + altre Agency dello stesso Admin
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
                // Agency: proprie + suoi Agent + Admin + altre Agency dello stesso Admin + tutti gli Agent dello stesso Admin
                
                // Aggiungi l'Admin se esiste
                if (!string.IsNullOrEmpty(currentUser.AdminId))
                {
                    circleIds.Add(currentUser.AdminId);
                }

                // Aggiungi tutti gli Agent della propria Agency
                var agents = await _userManager.GetUsersInRoleAsync("Agent");
                var myAgents = agents.Where(x => x.AdminId == currentUserId);
                circleIds.AddRange(myAgents.Select(x => x.Id));

                // Se l'Agency ha un Admin, aggiungi tutte le altre Agency dello stesso Admin
                if (!string.IsNullOrEmpty(currentUser.AdminId))
                {
                    var agencies = await _userManager.GetUsersInRoleAsync("Agency");
                    var otherAgencies = agencies.Where(x => x.AdminId == currentUser.AdminId && x.Id != currentUserId);
                    circleIds.AddRange(otherAgencies.Select(x => x.Id));

                    // Aggiungi tutti gli Agent di tutte le Agency dello stesso Admin
                    var allAgentsInAdminCircle = agents.Where(x => 
                        x.AdminId == currentUser.AdminId || // Agent diretti dell'Admin
                        otherAgencies.Select(a => a.Id).Contains(x.AdminId) // Agent delle altre Agency
                    );
                    circleIds.AddRange(allAgentsInAdminCircle.Select(x => x.Id));
                }
            }
            else if (currentUserRoles.Contains("Agent"))
            {
                // Agent: proprie + Agency/Admin + colleghi + altri Agent dello stesso Admin + altre Agency dello stesso Admin
                var myAgencyId = currentUser.AdminId;

                if (!string.IsNullOrEmpty(myAgencyId))
                {
                    // Aggiungi l'Agency/Admin stesso
                    circleIds.Add(myAgencyId);

                    // Trova tutti gli Agent con stesso AgencyId (colleghi della stessa Agency)
                    var agents = await _userManager.GetUsersInRoleAsync("Agent");
                    var colleagues = agents.Where(x => x.AdminId == myAgencyId && x.Id != currentUserId);
                    circleIds.AddRange(colleagues.Select(x => x.Id));

                    // Se l'Agency ha un Admin, aggiungi tutte le altre Agency dello stesso Admin
                    var myAgency = await _userManager.FindByIdAsync(myAgencyId);
                    if (myAgency != null && !string.IsNullOrEmpty(myAgency.AdminId))
                    {
                        var agencies = await _userManager.GetUsersInRoleAsync("Agency");
                        var otherAgencies = agencies.Where(x => x.AdminId == myAgency.AdminId && x.Id != myAgencyId);
                        circleIds.AddRange(otherAgencies.Select(x => x.Id));

                        // Aggiungi tutti gli Agent delle altre Agency dello stesso Admin
                        var otherAgents = agents.Where(x => 
                            otherAgencies.Select(a => a.Id).Contains(x.AdminId) && 
                            x.Id != currentUserId
                        );
                        circleIds.AddRange(otherAgents.Select(x => x.Id));

                        // Aggiungi anche l'Admin stesso
                        circleIds.Add(myAgency.AdminId);
                    }
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

        /// <summary>
        /// Determina il livello di accesso per un'entità creata da entityCreatorId.
        /// Livelli:
        /// - 1: Lista + Dettaglio + Modifica (permessi completi)
        /// - 2: Lista + Dettaglio (solo lettura, no modifica)
        /// - 3: Solo lista (popup informativo al click, no dettaglio, no modifica)
        /// 
        /// Regole per AGENT:
        /// - Livello 1: entità proprie (entityCreatorId == currentUserId)
        /// - Livello 2: entità della sua Agency (entityCreatorId == suaAgencyId) o colleghi stessa Agency
        /// - Livello 3: entità di altre Agency, Agent di altre Agency, Admin, resto della cerchia
        /// 
        /// Regole per AGENCY:
        /// - Livello 1: entità proprie o dei suoi Agent (entityCreatorId == currentUserId o User.AdminId == currentUserId)
        /// - Livello 2: entità di altre Agency e Agent nella cerchia
        /// 
        /// Regole per ADMIN:
        /// - Livello 1: tutto nella cerchia
        /// </summary>
        public async Task<int> GetAccessLevel(string currentUserId, string entityCreatorId)
        {
            // Se l'entità non ha un creatore, ritorna livello 3 (accesso limitato)
            if (string.IsNullOrEmpty(entityCreatorId))
                return 3;

            // Stesso utente - sempre livello 1
            if (currentUserId == entityCreatorId)
                return 1;

            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser == null)
                return 3;

            var currentUserRoles = await _userManager.GetRolesAsync(currentUser);
            var creator = await _userManager.FindByIdAsync(entityCreatorId);
            if (creator == null)
                return 3;

            // Verifica che l'entità sia nella cerchia
            var circleIds = await GetCircleUserIdsFor(currentUserId);
            if (!circleIds.Contains(entityCreatorId))
                return 3; // Non nella cerchia = nessun accesso (ma per sicurezza ritorniamo 3)

            if (currentUserRoles.Contains("Admin"))
            {
                // Admin: tutto livello 1
                return 1;
            }
            else if (currentUserRoles.Contains("Agency"))
            {
                // Agency: livello 1 su propri dati e dati dei suoi Agent
                if (entityCreatorId == currentUserId || creator.AdminId == currentUserId)
                    return 1;
                
                // Livello 2 su altre Agency e Agent nella cerchia
                return 2;
            }
            else if (currentUserRoles.Contains("Agent"))
            {
                // Agent: livello 1 solo sui propri dati
                if (entityCreatorId == currentUserId)
                    return 1;

                // Verifica se l'entità appartiene alla sua Agency
                var creatorRoles = await _userManager.GetRolesAsync(creator);
                
                // Se il creatore è la sua Agency
                if (entityCreatorId == currentUser.AdminId)
                    return 2;

                // Se il creatore è un Agent della stessa Agency
                if (creatorRoles.Contains("Agent") && creator.AdminId == currentUser.AdminId)
                    return 2;

                // Se il creatore è un'Agency (dello stesso Admin) -> Livello 3 (solo lista)
                // Se il creatore è un Agent di altre Agency -> Livello 3 (solo lista)
                // Se il creatore è l'Admin -> Livello 3 (solo lista)
                // Resto della cerchia = livello 3
                return 3;
            }

            // Default: livello 3 (accesso limitato)
            return 3;
        }
    }
}

