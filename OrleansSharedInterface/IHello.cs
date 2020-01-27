using System;
using System.Threading;
using System.Threading.Tasks;

namespace OrleansSharedInterface
{
  public interface IHello : Orleans.IGrainWithStringKey
    {
        Task<string> SayHello(string greeting);
    }   
}
