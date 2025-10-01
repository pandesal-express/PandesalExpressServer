using PandesalExpress.Auth.Dtos;
using PandesalExpress.Infrastructure.Abstractions;

namespace PandesalExpress.Auth.Features.FaceRegister;

public record FaceRegisterCommand(RegisterRequestDto Dto) : ICommand<AuthResponseDto>;
