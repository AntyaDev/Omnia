using System;
using System.Collections.Generic;
using Omnia.Compiller.TypeSystem.Infrastructure;

namespace Omnia.Compiller.TypeSystem
{
    class Module : IModule
    {
        public string Name { get; private set; }

        public string FullName { get; private set; }

        public EntityType EntityType { get { return EntityType.Module; } }

        public IModule ParentModule { get; private set; }

        public Module(string fullName)
        {
            FullName = fullName;
        }

        public bool Resolve(IEnumerable<IEntity> resultingSet, string name, EntityType typesToConsider)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IEntity> GetMembers()
        {
            throw new NotImplementedException();
        }
    }
}
