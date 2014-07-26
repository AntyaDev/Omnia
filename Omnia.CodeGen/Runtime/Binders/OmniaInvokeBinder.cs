using System;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace Omnia.CodeGen.Runtime.Binders
{
    public class OmniaInvokeBinder : InvokeBinder
    {
        public OmniaInvokeBinder(CallInfo callinfo) : base(callinfo)
        { }

        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args,
                                                         DynamicMetaObject errorSuggestion)
        {
            // Defer if any object has no value so that we evaulate their
            // Expressions and nest a CallSite for the InvokeMember.
            if (!target.HasValue || args.Any(a => !a.HasValue))
            {
                var deferArgs = new DynamicMetaObject[args.Length + 1];
                for (int i = 0; i < args.Length; i++)
                {
                    deferArgs[i + 1] = args[i];
                }
                deferArgs[0] = target;
                return Defer(deferArgs);
            }
            // Find our own binding.
            if (target.LimitType.IsSubclassOf(typeof(Delegate)))
            {
                var parms = target.LimitType.GetMethod("Invoke").GetParameters();
                if (parms.Length == args.Length)
                {
                    // Don't need to check if argument types match parameters.
                    // If they don't, users get an argument conversion error.
                    var callArgs = RuntimeHelper.ConvertArguments(args, parms);
                    var expression = Expression.Invoke(
                        Expression.Convert(target.Expression, target.LimitType),
                        callArgs);
                    return new DynamicMetaObject(
                        RuntimeHelper.EnsureObjectResult(expression),
                        BindingRestrictions.GetTypeRestriction(target.Expression,
                                                               target.LimitType));
                }
            }
            return errorSuggestion ??
                RuntimeHelper.CreateThrow(
                    target, args,
                    BindingRestrictions.GetTypeRestriction(target.Expression,
                                                           target.LimitType),
                    typeof(InvalidOperationException),
                    "Wrong number of arguments for function -- " +
                    target.LimitType + " got " + args);
        }
    }
}
