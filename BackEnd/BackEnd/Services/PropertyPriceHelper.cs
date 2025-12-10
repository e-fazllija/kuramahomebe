using BackEnd.Entities;
using BackEnd.Models.RealEstatePropertyModels;

namespace BackEnd.Services
{
    /// <summary>
    /// Helper per gestire la logica del prezzo degli immobili.
    /// Se PriceReduced > 0, usa PriceReduced, altrimenti usa Price.
    /// </summary>
    public static class PropertyPriceHelper
    {
        /// <summary>
        /// Restituisce il prezzo da usare per l'entità RealEstateProperty.
        /// Se PriceReduced > 0, restituisce PriceReduced, altrimenti Price.
        /// </summary>
        public static double GetPriceToUse(this RealEstateProperty property)
        {
            return (property.PriceReduced > 0) ? property.PriceReduced : property.Price;
        }

        /// <summary>
        /// Restituisce il prezzo da usare per il modello RealEstatePropertySelectModel.
        /// Se PriceReduced > 0, restituisce PriceReduced, altrimenti Price.
        /// </summary>
        public static double GetPriceToUse(this RealEstatePropertySelectModel property)
        {
            return (property.PriceReduced > 0) ? property.PriceReduced : property.Price;
        }

        /// <summary>
        /// Versione statica per quando non hai l'oggetto completo.
        /// Restituisce il prezzo da usare basato su Price e PriceReduced.
        /// </summary>
        public static double GetPriceToUse(double price, double priceReduced)
        {
            return (priceReduced > 0) ? priceReduced : price;
        }
    }
}


