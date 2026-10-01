using OutletControl.Contracts.Auth;

namespace OutletControl.Application.Interfaces.Auth;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
}