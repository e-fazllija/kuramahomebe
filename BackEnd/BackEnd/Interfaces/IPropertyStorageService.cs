using BackEnd.Models.OutputModels;

namespace BackEnd.Interfaces
{
    /// <summary>
    /// Servizio dedicato alla gestione dello storage per le immagini delle proprietà immobiliari.
    /// Utilizza un container blob pubblico/anonimo per consentire la visualizzazione delle immagini.
    /// </summary>
    public interface IPropertyStorageService
    {
        /// <summary>
        /// Carica un file di immagine nel container pubblico delle proprietà
        /// </summary>
        /// <param name="file">Stream del file da caricare</param>
        /// <param name="fileName">Nome del file (path relativo nel container)</param>
        /// <returns>URL pubblico del file caricato</returns>
        Task<string> UploadPropertyImage(Stream file, string fileName);

        /// <summary>
        /// Elimina un file di immagine dal container delle proprietà
        /// </summary>
        /// <param name="fileName">Nome del file da eliminare</param>
        /// <returns>True se eliminato con successo, False altrimenti</returns>
        Task<bool> DeletePropertyImage(string fileName);

        /// <summary>
        /// Scarica un file di immagine dal container delle proprietà
        /// </summary>
        /// <param name="fileName">Nome del file da scaricare</param>
        /// <returns>FileResponse con stream e metadati del file</returns>
        Task<FileResponse> DownloadPropertyImage(string fileName);
    }
}

