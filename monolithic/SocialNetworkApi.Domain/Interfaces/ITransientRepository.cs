using SocialNetworkApi.Domain.Common;

namespace SocialNetworkApi.Domain.Interfaces;

public interface ITransientRepository<T> : IRepository<T> where T : BaseEntity
{
}