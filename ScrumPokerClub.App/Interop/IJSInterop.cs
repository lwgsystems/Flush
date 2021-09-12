using System.Threading.Tasks;

namespace ScrumPokerClub.Interop
{
    public interface IJSInterop
    {
        Task<JSMediaQueryList> MatchMediaAsync(string mediaQuery);
        Task<TValue> GetItemFromStorageAsync<TValue>(string key);
        Task PutItemInStorageAsync<TValue>(string key, TValue value);
        Task SetHtmlAttribute(string attribute, string value);
        Task SetAttribute(string selector, string attribute, string value);
    }
}