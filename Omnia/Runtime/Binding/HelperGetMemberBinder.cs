using System.Dynamic;
using System.Linq.Expressions;

namespace Omnia.Runtime.Binding
{
    class HelperGetMemberBinder : GetMemberBinder
    {
        public HelperGetMemberBinder(string name) : base(name, ignoreCase: false)
        { }

        public override DynamicMetaObject FallbackGetMember(
            DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            return errorSuggestion ?? 
                new DynamicMetaObject(
                    Expression.Constant(RuntimeHelper.Sentinel),
                    target.Restrictions.Merge(
                    BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType)));
        }
    }
}
