
namespace Omnia.Compiller.TypeSystem.Infrastructure
{
    interface IEntity
    {
        string Name { get; }
        string FullName { get; }
        EntityType EntityType { get; }
    }
}
