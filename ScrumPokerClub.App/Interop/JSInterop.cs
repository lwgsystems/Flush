using Microsoft.JSInterop;
using System.Text.Json;
using System.Threading.Tasks;

namespace ScrumPokerClub.Interop
{
    public class JSInterop : IJSInterop
    {
        private static readonly string module_name = "./js/spcinterop.js";

        private IJSRuntime JSRuntime { get; init; }

        public JSInterop(IJSRuntime jSRuntime)
        {
            JSRuntime = jSRuntime;
        }

        public async Task<JSMediaQueryList> MatchMediaAsync(string mediaQuery)
        {
            var module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", module_name);
            return await module.InvokeAsync<JSMediaQueryList>("spc.matchMedia", mediaQuery);
        }

        public async Task<TValue> GetItemFromStorageAsync<TValue>(string key)
        {
            var item = await JSRuntime.InvokeAsync<string>("localStorage.getItem", key);
            if (item is null)
                return default;

            return JsonSerializer.Deserialize<TValue>(item);
        }

        public async Task PutItemInStorageAsync<TValue>(string key, TValue value)
        {
            var item = JsonSerializer.Serialize(value);
            await JSRuntime.InvokeVoidAsync("localStorage.setItem", key, item);
        }

        public async Task SetHtmlAttribute(string attribute, string value)
        {
            var module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", module_name);
            await module.InvokeVoidAsync("spc.setHtmlAttr", attribute, value);
        }

        public async Task SetAttribute(string selector, string attribute, string value)
        {
            var module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", module_name);
            await module.InvokeVoidAsync("spc.setAttr", selector, attribute, value);
        }
    }
}