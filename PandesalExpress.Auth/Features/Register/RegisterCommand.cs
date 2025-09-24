using PandesalExpress.Auth.Dtos;
using PandesalExpress.Infrastructure.Abstractions;

namespace PandesalExpress.Auth.Features.Register;

public record RegisterCommand(RegisterRequestDto Dto) : ICommand<AuthResponseDto>;