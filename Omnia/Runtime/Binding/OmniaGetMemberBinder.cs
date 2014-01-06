using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace Omnia.Runtime.Binding
{
    // This class is used for general dotted expressions for fetching members.
    class OmniaGetMemberBinder : GetMemberBinder
    {
        public OmniaGetMemberBinder(string name) : base(name, ignoreCase: false)
        { }

        public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            // Defer if any object has no value so that we evaulate their
            // Expressions and nest a CallSite for the InvokeMember.
            if (!target.HasValue) return Defer(target);
            
            // Find our own binding.
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public;
            var members = target.LimitType.GetMember(Name, flags);

            return members.Length == 1
                ? new DynamicMetaObject(Runtime.EnsureObjectResult(
                    Expression.MakeMemberAccess(
                    Expression.Convert(target.Expression,
                    members[0].DeclaringType), members[0])),
                    // Don't need restriction test for name since this
                    // rule is only used where binder is used, which is
                    // only used in sites with this binder.Name.
                    BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType))

                : errorSuggestion ?? Runtime.CreateThrow(target, null,
                        BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType),
                        typeof(MissingMemberException), "cannot bind member, " + Name + ", on object " + target.Value);
        }
    }
}
