using BackEnd.Data;
using BackEnd.Entities;

namespace BackEnd.Services
{
    public static class LocationDataSeeder
    {
        public static async Task SeedLocations(AppDbContext context)
        {
            if (context.Locations.Any())
                return; // Database already seeded

            // Dictionary of provinces and their cities
            var provinceCities = new Dictionary<string, List<string>>
            {
                ["Arezzo"] = new List<string> { "Arezzo", "Castiglion Fiorentino", "Cortona" },
                ["Caserta"] = new List<string> { "Aversa", "Caserta", "Vairano Patenora" },
                ["Chieti"] = new List<string> { "Chieti", "Lanciano", "San Salvo Marina" },
                ["Firenze"] = new List<string> { "Empoli", "Firenze", "Tavarnelle Val di Pesa" },
                ["Frosinone"] = new List<string> { "Acuto", "Alatri", "Anagni", "Arce", "Boville Ernica", "Fiuggi", "Frosinone", "Morolo", "Paliano", "Piglio", "Serrone", "Sgurgola" },
                ["L'Aquila"] = new List<string> { "Campo di Giove", "Carsoli", "L'Aquila", "Rocca di Cambio", "Rocca di Mezzo", "Sulmona", "Tagliacozzo", "Tornimparte" },
                ["Latina"] = new List<string> { "Aprilia", "Cori", "Latina", "Rocca Massima", "Sabaudia", "San Felice Circeo", "Sermoneta", "Terracina" },
                ["Napoli"] = new List<string> { "Forio d'Ischia", "Napoli", "Pompei" },
                ["Perugia"] = new List<string> { "Assisi", "Collazzone", "Paciano", "Perugia" },
                ["Rieti"] = new List<string> { "Amatrice", "Leonessa", "Mompeo", "Petrella Salto", "Poggio Nativo", "Rieti", "Rocca Sinibalda", "Torre in Sabina" },
                ["Roma"] = new List<string> { "Agosta", "Albano Laziale", "Alessandrino", "Allumiere", "Anzio", "Arcinazzo Romano", "Ardea", "Ariccia", "Artena", "Bellegra", "Bracciano", "Campagnano di Roma", "Carpineto Romano", "Casape", "Castel Gandolfo", "Castel Giubileo", "Castel Madama", "Castel San Pietro", "Castel San Pietro Romano", "Cave", "Cerveteri", "Ciampino", "Civitavecchia", "Colleferro", "Colonna", "Fiano Romano", "Fiumicino", "Fonte Nuova", "Frascati", "Gallicano nel Lazio", "Gavignano", "Genazzano", "Genzano di Roma", "Gorga", "Grottaferrata", "Guidonia Montecelio", "Infernetto", "Labico", "Lanuvio", "Lariano", "Marino", "Mentana", "Monte Porzio Catone", "Montecompatri", "Montelanico", "Montelibretti", "Monterotondo", "Nazzano", "Nemi", "Nettuno", "Olevano Romano", "Palestrina", "Palombara Sabina", "Pisoniano", "Poli", "Pomezia", "Portuense", "Riano", "Rocca di Cave", "Rocca di Papa", "Rocca Priora", "Rocca Santo Stefano", "Roiate", "Roma", "Sacrofano", "San Cesareo", "San Gregorio da Sassola", "San Polo dei Cavalieri", "San Vito Romano", "Sant'Angelo Romano", "Sant'Oreste", "Santa Marinella", "Segni", "Subiaco", "Tivoli", "Tolfa", "Torre Maura", "Torre Spaccata", "Tor Tre Teste", "Valmontone", "Velletri", "Zagarolo" },
                ["Sassari"] = new List<string> { "Alghero", "Olbia", "Sassari" },
                ["Terni"] = new List<string> { "Narni", "Narni Scalo", "Terni" },
                ["Trento"] = new List<string> { "Mezzana", "Rovereto", "Trento" },
                ["Viterbo"] = new List<string> { "Bolsena", "Civita Castellana", "Tarquinia", "Viterbo" }
            };

            // Dictionary of cities and their locations
            var cityLocations = new Dictionary<string, List<string>>
            {
                ["Ardea"] = new List<string> { "Centro Storico", "Nuova California", "Tor San Lorenzo", "Marina di Ardea", "Castagnetta" },
                ["Cave"] = new List<string> { "Colle Palme", "San Bartolomeo" },
                ["Fiumicino"] = new List<string> { "Fiumicino Paese", "Isola Sacra", "Focene", "Fregene", "Palidoro", "Parco Leonardo" },
                ["Genazzano"] = new List<string> { "Centro Storico", "San Vito", "Colle Pizzuto", "Ponte Sfondato" },
                ["Guidonia Montecelio"] = new List<string> { "Albuccione", "Casacalda", "Castel Arcione", "Colle Verde", "La Botte", "Marco Simone", "Montecelio", "Setteville", "Villa Nova", "Villalba" },
                ["Labico"] = new List<string> { "Colle Spina" },
                ["Monte Compatri"] = new List<string> { "Colle Mattia", "Laghetto" },
                ["Palestrina"] = new List<string> { "Carchitti", "Valvarino" },
                ["Rocca Priora"] = new List<string> { "Colle di Fuori" },
                ["Roma"] = new List<string> { "Acilia", "Alessandrino", "Axa", "Boccea", "Borghesiana", "Casal Bernocchi", "Casal Palocco", "Casalotti", "Castel Fusano", "Castelverde", "Centocelle", "Centro Giano", "Cinecittà", "Colle Monfortani", "Colle Prenestino", "Colle del Sole", "Corcolle", "Corcolle Alto", "Don Bosco", "Dragona", "Dragoncello", "Eur", "Finocchio", "Fonte Laurentina", "Fosso San Giuliano", "Giardinetti", "Giardini di Corcolle", "Infernetto", "La Pisana", "La Storta", "Longarina", "Lunghezza", "Madonnetta", "Malafede", "Monte Sacro", "Osteria del Curato", "Ostia", "Ostia Antica", "Ostia Levante", "Ostia Ponente", "Pantano Borghese", "Piana del Sole", "Pigneto", "Ponte Galeria", "Ponte di Nona", "Portuense", "Prato Fiorito", "Prima Porta", "Rocca Cencia", "Romanina", "Roma Est", "San Vittorino", "Saxa Rubra", "Settecamini", "Tor Bella Monaca", "Tor Tre Teste", "Tor Vergata", "Tor de Cenci", "Torre Angela", "Torre Gaia", "Torre Maura", "Torre Spaccata", "Tuscolana", "Vallerano", "Vermicino", "Villa Verde", "Villaggio Breda", "Villaggio Prenestino" },
                ["Tivoli"] = new List<string> { "Centro Storico", "Tivoli Terme", "Villa Adriana", "Campolimpido", "Colle di Tivoli", "Ponte Lucano", "San Paolo" },
                ["Zagarolo"] = new List<string> { "Centro Paese", "Valle Martella", "Via Prenestina" }
            };

            // Create provinces
            var provinces = new List<Province>();
            foreach (var provinceName in provinceCities.Keys)
            {
                var province = new Province { Name = provinceName };
                context.Provinces.Add(province);
                provinces.Add(province);
            }
            await context.SaveChangesAsync();

            // Create cities
            var cities = new List<City>();
            foreach (var kvp in provinceCities)
            {
                var province = provinces.First(p => p.Name == kvp.Key);
                foreach (var cityName in kvp.Value)
                {
                    var city = new City { Name = cityName, ProvinceId = province.Id };
                    context.Cities.Add(city);
                    cities.Add(city);
                }
            }
            await context.SaveChangesAsync();

            // Create locations
            var locations = new List<Location>();
            foreach (var kvp in cityLocations)
            {
                var city = cities.FirstOrDefault(c => c.Name == kvp.Key);
                if (city != null)
                {
                    foreach (var locationName in kvp.Value)
                    {
                        var location = new Location 
                        { 
                            Name = locationName, 
                            CityId = city.Id, 
                            CreationDate = DateTime.Now, 
                            UpdateDate = DateTime.Now 
                        };
                        context.Locations.Add(location);
                        locations.Add(location);
                    }
                }
            }
            await context.SaveChangesAsync();
        }
    }
} 