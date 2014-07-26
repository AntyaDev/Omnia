using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Omnia.CodeGen.Runtime.Binders
{
    public class OmniaInvokeMemberBinder : InvokeMemberBinder
    {
        public OmniaInvokeMemberBinder(string name, CallInfo callinfo) : base(name, true, callinfo)
        { }

        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
        {
            var argexprs = new Expression[args.Length + 1];
            for (int i = 0; i < args.Length; i++)
            {
                argexprs[i + 1] = args[i].Expression;
            }
            argexprs[0] = target.Expression;
            // Just "defer" since we have code in OmniaInvokeBinder that knows
            // what to do, and typically this fallback is from a language like
            // Python that passes a DynamicMetaObject with HasValue == false.
            return new DynamicMetaObject(
                           Expression.Dynamic(
                // This call site doesn't share any L2 caching
                // since we don't call GetInvokeBinder from Omnia.
                // We aren't plumbed to get the runtime instance here.
                               new OmniaInvokeBinder(new CallInfo(args.Length)),
                               typeof(object), // ret type
                               argexprs),
                // No new restrictions since OmniaInvokeBinder will handle it.
                           target.Restrictions.Merge(BindingRestrictions.Combine(args)));
        }

        public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
        {
            // Defer if any object has no value so that we evaulate their
            // Expressions and nest a CallSite for the InvokeMember.
            if (!target.HasValue || args.Any((a) => !a.HasValue))
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
            // Could consider allowing invoking static members from an instance.
            var flags = BindingFlags.IgnoreCase | BindingFlags.Instance |
                        BindingFlags.Public;
            var members = target.LimitType.GetMember(this.Name, flags);
            if ((members.Length == 1) && (members[0] is PropertyInfo ||
                                          members[0] is FieldInfo))
            {
                // NEED TO TEST, should check for delegate value too
                var mem = members[0];
                throw new NotImplementedException();
                //return new DynamicMetaObject(
                //    Expression.Dynamic(
                //        new OmniaInvokeBinder(new CallInfo(args.Length)),
                //        typeof(object),
                //        args.Select(a => a.Expression).AddFirst(
                //               Expression.MakeMemberAccess(this.Expression, mem)));

                // Don't test for eventinfos since we do nothing with them now.
            }
            else
            {
                // Get MethodInfos with right arg counts.
                var mi_mems = members.
                    Select(m => m as MethodInfo).
                    Where(m => m is MethodInfo &&
                               ((MethodInfo)m).GetParameters().Length ==
                                   args.Length);
                // Get MethodInfos with param types that work for args.  This works
                // except for value args that need to pass to reftype params. 
                // We could detect that to be smarter and then explicitly StrongBox
                // the args.
                List<MethodInfo> res = new List<MethodInfo>();
                foreach (var mem in mi_mems)
                {
                    if (RuntimeHelper.ParametersMatchArguments(
                                           mem.GetParameters(), args))
                    {
                        res.Add(mem);
                    }
                }
                // False below means generate a type restriction on the MO.
                // We are looking at the members targetMO's Type.
                var restrictions = RuntimeHelper.GetTargetArgsRestrictions(
                                                      target, args, false);
                if (res.Count == 0)
                {
                    return errorSuggestion ??
                        RuntimeHelper.CreateThrow(
                            target, args, restrictions,
                            typeof(MissingMemberException),
                            "Can't bind member invoke -- " + args.ToString());
                }
                // restrictions and conversion must be done consistently.
                var callArgs = RuntimeHelper.ConvertArguments(
                                                 args, res[0].GetParameters());
                return new DynamicMetaObject(
                   RuntimeHelper.EnsureObjectResult(
                     Expression.Call(
                        Expression.Convert(target.Expression,
                                           target.LimitType),
                        res[0], callArgs)),
                   restrictions);
                // Could hve tried just letting Expr.Call factory do the work,
                // but if there is more than one applicable method using just
                // assignablefrom, Expr.Call throws.  It does not pick a "most
                // applicable" method or any method.
            }
        }
    }
}
