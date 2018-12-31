using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Andoromeda.Kyubey.Dex.Hubs
{
    public class SimpleWalletHub : Hub
    {
        public async Task BindUUID(Guid id)
        {
            var _id = id.ToString().ToLower();
            await Groups.AddToGroupAsync(Context.ConnectionId, _id);
        }
    }
}
