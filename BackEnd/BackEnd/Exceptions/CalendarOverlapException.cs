namespace BackEnd.Exceptions
{
    /// <summary>
    /// Eccezione lanciata quando si tenta di creare/modificare un appuntamento
    /// per una casa già occupata nello stesso orario. Il messaggio è pensato per essere mostrato all'utente.
    /// </summary>
    public class CalendarOverlapException : Exception
    {
        public CalendarOverlapException(string message)
            : base(message)
        {
        }
    }
}
