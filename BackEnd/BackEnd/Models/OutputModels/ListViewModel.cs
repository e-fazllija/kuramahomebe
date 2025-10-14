namespace BackEnd.Models.OutputModels
{
    public class ListViewModel<T>
    {
        public List<T> Data { get; set; }
        public int Total {  get; set; }
    }
}
