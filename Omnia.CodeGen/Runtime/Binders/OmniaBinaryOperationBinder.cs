using System.Dynamic;
using System.Linq.Expressions;

namespace Omnia.CodeGen.Runtime.Binders
{
    public class OmniaBinaryOperationBinder : BinaryOperationBinder
    {
        public OmniaBinaryOperationBinder(ExpressionType operation) : base(operation) 
        { }

        public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
        {
            // Defer if any object has no value so that we evaulate their
            // Expressions and nest a CallSite for the InvokeMember.
            if (!target.HasValue || !arg.HasValue)
            {
                return Defer(target, arg);
            }
            var restrictions = target.Restrictions.Merge(arg.Restrictions)
                .Merge(BindingRestrictions.GetTypeRestriction(
                    target.Expression, target.LimitType))
                .Merge(BindingRestrictions.GetTypeRestriction(
                    arg.Expression, arg.LimitType));
            return new DynamicMetaObject(
                RuntimeHelper.EnsureObjectResult(
                  Expression.MakeBinary(
                    this.Operation,
                    Expression.Convert(target.Expression, target.LimitType),
                    Expression.Convert(arg.Expression, arg.LimitType))),
                restrictions
            );
        }
    }
}
