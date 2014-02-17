using System.Collections.Generic;
using FunctionalWeapon.Monads;
using Omnia.Compiller.TypeSystem.Infrastructure;

namespace Omnia.Compiller.Scopes
{
    class Scope
    {
        readonly Dictionary<string, IEntity> _members = new Dictionary<string, IEntity>();

        public void Put(IEntity entity)
        {
            if (!_members.ContainsKey(entity.Name)) _members.Add(entity.Name, entity);
        }

        public Maybe<IEntity> TryGet(string name)
        {
            IEntity value;
            _members.TryGetValue(name, out value);
            return value.ToMaybe();
        }
    }
}
