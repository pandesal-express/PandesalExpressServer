using PandesalExpress.Infrastructure.Abstractions;
using Shared.Dtos;

namespace PandesalExpress.Stores.Features.GetStoreByKey;

public record GetStoreByKeyQuery(string StoreKey) : IQuery<StoreDto?>;
