using System.Threading.Tasks;
using Domain.DTOs;

namespace Application.Abstractions.Services;

public interface IFleetHub
{
    Task JoinFleetGroup(string fleetId);
    Task LeaveFleetGroup(string fleetId);
    Task SendToAll(string method, object data);
    Task SendToGroup(string groupName, string method, object data);
}
