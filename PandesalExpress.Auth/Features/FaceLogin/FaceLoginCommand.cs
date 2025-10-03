using PandesalExpress.Auth.Dtos;
using PandesalExpress.Infrastructure.Abstractions;

namespace PandesalExpress.Auth.Features.FaceLogin;

public record FaceLoginCommand(Ulid UserId, DateTime TimeLogged) : ICommand<AuthResponseDto>;
