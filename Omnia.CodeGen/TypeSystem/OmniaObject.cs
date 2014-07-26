using System.Collections.Generic;
using System.Text;

namespace Omnia.CodeGen.TypeSystem
{
    public class OmniaObject
    {
        readonly Dictionary<string, object> _members = new Dictionary<string, object>();
        
        public bool HasMember(string name)
        {
            return _members.ContainsKey(name);
        }
        
        public virtual void Set(string name, object value)
        {
            if (_members.ContainsKey(name)) _members.Remove(name);
            
            _members.Add(name, value);
        }
        
        public virtual object Get(string name)
        {
            return !_members.ContainsKey(name) ? null : _members[name];
        }
        
        public override string ToString()
        {
            var res = new StringBuilder("{");
            foreach (var member in _members)
            {
                res.Append(member.Key);
                res.Append(":");
                res.Append(member.Value == this ? "this" : member.Value.ToString());
                res.Append(", ");
            }
            res.Append("}");
            return res.ToString();
        }
    }
}
