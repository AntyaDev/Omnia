using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace Omnia.CodeGen.Runtime.Binders
{
    // This class is used for general dotted expressions for setting members.
    public class OmniaSetMemberBinder : SetMemberBinder
    {
        public OmniaSetMemberBinder(string name) : base(name, ignoreCase: false)
        { }

        public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value,
                                                            DynamicMetaObject errorSuggestion)
        {
            // Defer if any object has no value so that we evaulate their
            // Expressions and nest a CallSite for the InvokeMember.
            if (!target.HasValue) return Defer(target);
            
            // Find our own binding.
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public;
            var members = target.LimitType.GetMember(Name, flags);
            if (members.Length == 1)
            {
                var memberInfo = members[0];
                Expression val;
                
                // Should check for member domain type being Type and value being
                // TypeModel, similar to ConvertArguments, and building an
                // expression like GetRuntimeTypeMoFromModel.
                if (memberInfo.MemberType == MemberTypes.Property)
                    val = Expression.Convert(value.Expression, ((PropertyInfo)memberInfo).PropertyType);
                
                else if (memberInfo.MemberType == MemberTypes.Field)
                    val = Expression.Convert(value.Expression, ((FieldInfo)memberInfo).FieldType);
                
                else return (errorSuggestion ?? RuntimeHelper.CreateThrow(
                                target, null,
                                BindingRestrictions.GetTypeRestriction(
                                    target.Expression,
                                    target.LimitType),
                                typeof(InvalidOperationException),
                                "Omnia only supports setting Properties and fields at this time."));

                return new DynamicMetaObject(
                    // Assign returns the stored value, so we're good for Omnia.
                    RuntimeHelper.EnsureObjectResult(
                    Expression.Assign(
                    Expression.MakeMemberAccess(
                    Expression.Convert(target.Expression, members[0].DeclaringType), members[0]), val)),
                    // Don't need restriction test for name since this
                    // rule is only used where binder is used, which is
                    // only used in sites with this binder.Name.                    
                    BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
            }
            else
            {
                return errorSuggestion ?? RuntimeHelper.CreateThrow(target, null,
                    BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType),
                    typeof(MissingMemberException), "IDynObj member name conflict.");
            }
        }
    }
}
