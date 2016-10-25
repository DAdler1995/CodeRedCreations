using System.Threading.Tasks;

namespace CodeRedCreations.Services
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }
}
