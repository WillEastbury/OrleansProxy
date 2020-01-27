using System.Threading.Tasks;
namespace OrleansSharedInterface
{
    public interface IVote : Orleans.IGrainWithStringKey
    {     
        Task<int> CountMyVote();
    }   
  
}
