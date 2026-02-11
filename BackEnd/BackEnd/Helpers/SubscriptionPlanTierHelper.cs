namespace BackEnd.Helpers
{
    /// <summary>
    /// Helper per ricavare il tier del piano (Basic, Pro, Premium) dal nome.
    /// Usa la stessa logica del frontend: i piani prepagati (es. "Basic 3 Months", "Pro 6 Months", "Premium 12 Months")
    /// devono essere trattati come i rispettivi tier per visibilità e accesso.
    /// </summary>
    public static class SubscriptionPlanTierHelper
    {
        /// <summary>Restituisce true se il nome piano è Premium (inclusi "Premium 3 Months", "Premium 12 Months", ecc.).</summary>
        public static bool IsPremiumPlanName(string? planName)
        {
            if (string.IsNullOrWhiteSpace(planName)) return false;
            return planName.Trim().StartsWith("premium", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Restituisce true se il nome piano è Pro (es. "Pro", "Pro 3 Months"). Non considera "Premium" come Pro.</summary>
        public static bool IsProPlanName(string? planName)
        {
            if (string.IsNullOrWhiteSpace(planName)) return false;
            var n = planName.Trim();
            if (n.StartsWith("premium", StringComparison.OrdinalIgnoreCase)) return false;
            return n.StartsWith("pro", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Restituisce true se il nome piano è Basic (es. "Basic", "Basic 3 Months").</summary>
        public static bool IsBasicPlanName(string? planName)
        {
            if (string.IsNullOrWhiteSpace(planName)) return false;
            return planName.Trim().StartsWith("basic", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Restituisce true se il piano è Pro o Premium (per accesso Widget3 e simili).</summary>
        public static bool IsProOrPremiumPlanName(string? planName)
        {
            return IsProPlanName(planName) || IsPremiumPlanName(planName);
        }
    }
}
