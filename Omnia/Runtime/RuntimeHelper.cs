using Omnia.Compiller;
using System;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Omnia.Runtime
{
    static class RuntimeHelper
    {
        public static object Sentinel { get; private set; }

        static RuntimeHelper()
        {
            Sentinel = new object();
        }

        // EnsureObjectResult wraps expr if necessary so that any binder or
        // DynamicMetaObject result expression returns object.  This is required
        // by CallSites.
        public static Expression EnsureObjectResult(Expression expr)
        {
            if (!expr.Type.IsValueType) return expr;

            if (expr.Type == typeof(void)) return Expression.Block(expr, Expression.Default(typeof(object)));

            return Expression.Convert(expr, typeof(object));
        }

        // CreateThrow is a convenience function for when binders cannot bind.
        // They need to return a DynamicMetaObject with appropriate restrictions
        // that throws.  Binders never just throw due to the protocol since
        // a binder or MO down the line may provide an implementation.
        //
        // It returns a DynamicMetaObject whose expr throws the exception, and 
        // ensures the expr's type is object to satisfy the CallSite return type
        // constraint.
        //
        // A couple of calls to CreateThrow already have the args and target
        // restrictions merged in, but BindingRestrictions.Merge doesn't add 
        // duplicates.
        public static DynamicMetaObject CreateThrow(DynamicMetaObject target, DynamicMetaObject[] args,
                                                    BindingRestrictions moreTests, Type exception,
                                                    params object[] exceptionArgs)
        {
            Expression[] argExprs = null;
            var argTypes = Type.EmptyTypes;
            if (exceptionArgs != null)
            {
                int i = exceptionArgs.Length;
                argExprs = new Expression[i];
                argTypes = new Type[i];
                i = 0;
                foreach (object o in exceptionArgs)
                {
                    Expression e = Expression.Constant(o);
                    argExprs[i] = e;
                    argTypes[i] = e.Type;
                    i++;
                }
            }

            var constructor = exception.GetConstructor(argTypes);

            if (constructor == null) throw new ArgumentException("Type doesn't have constructor with a given signature");

            return new DynamicMetaObject(
                Expression.Throw(
                Expression.New(constructor, argExprs),
                // Force expression to be type object so that DLR CallSite
                // code things only type object flows out of the CallSite.
                typeof(object)),
                target.Restrictions.Merge(BindingRestrictions.Combine(args)).Merge(moreTests));
        }

        // ParamsMatchArgs returns whether the args are assignable to the parameters.
        // We specially check for our TypeModel that wraps .NET's RuntimeType, and
        // elsewhere we detect the same situation to convert the TypeModel for calls.
        //
        // Consider checking p.IsByRef and returning false since that's not CLS.
        //
        // Could check for a.HasValue and a.Value is None and
        // ((paramtype is class or interface) or (paramtype is generic and
        // nullable<t>)) to support passing nil anywhere.
        public static bool ParametersMatchArguments(ParameterInfo[] parameters, DynamicMetaObject[] args)
        {
            // We only call this after filtering members by this constraint.
            Debug.Assert(args.Length == parameters.Length, "Internal: args are not same len as params.");
            for (int i = 0; i < args.Length; i++)
            {
                var paramType = parameters[i].ParameterType;

                // We consider arg of TypeModel and param of Type to be compatible.
                if (paramType == typeof(Type) && (args[i].LimitType == typeof(TypeModel))) continue;

                // Could check for HasValue and Value==null AND
                // (paramtype is class or interface) or (is generic
                // and nullable<T>) ... to bind nullables and null.
                if (!paramType.IsAssignableFrom(args[i].LimitType)) return false;
            }
            return true;
        }

        // Returns a DynamicMetaObject with an expression that fishes the .NET
        // RuntimeType object from the TypeModel MO.
        public static DynamicMetaObject GetRuntimeTypeMoFromModel(DynamicMetaObject typeModel)
        {
            Debug.Assert((typeModel.LimitType == typeof(TypeModel)), "Internal: MO is not a TypeModel?!");

            var pi = typeof(TypeModel).GetProperty("ReflType");
            Debug.Assert(pi != null);

            return new DynamicMetaObject(
                Expression.Property(
                Expression.Convert(typeModel.Expression, typeof(TypeModel)), pi),
                typeModel.Restrictions.Merge(BindingRestrictions.GetTypeRestriction(typeModel.Expression, typeof(TypeModel))));

            // Must supply a value to prevent binder FallbackXXX methods
            // from infinitely looping if they do not check this MO for
            // HasValue == false and call Defer.  After Omnia added Defer
            // checks, we could verify, say, FallbackInvokeMember by no
            // longer passing a value here.
            //((TypeModel)typeModel.Value).ReflType
        }

        // GetTargetArgsRestrictions generates the restrictions needed for the
        // MO resulting from binding an operation. This combines all existing
        // restrictions and adds some for arg conversions. targetInst indicates
        // whether to restrict the target to an instance (for operations on type
        // objects) or to a type (for operations on an instance of that type).
        //
        // NOTE, this function should only be used when the caller is converting
        // arguments to the same types as these restrictions.
        public static BindingRestrictions GetTargetArgsRestrictions(DynamicMetaObject target, DynamicMetaObject[] args,
                                                                    bool instanceRestrictionOnTarget)
        {
            // Important to add existing restriction first because the
            // DynamicMetaObjects (and possibly values) we're looking at depend
            // on the pre-existing restrictions holding true.
            var restrictions = target.Restrictions.Merge(BindingRestrictions.Combine(args));

            restrictions = restrictions.Merge(instanceRestrictionOnTarget
                ? BindingRestrictions.GetInstanceRestriction(target.Expression, target.Value)
                : BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));

            return args.Select(arg => arg.HasValue && arg.Value == null
                               ? BindingRestrictions.GetInstanceRestriction(arg.Expression, null)
                               : BindingRestrictions.GetTypeRestriction(arg.Expression, arg.LimitType))
                        .Aggregate(restrictions, (current, restriction) => current.Merge(restriction));
        }

        // Returns list of Convert exprs converting args to param types.  If an arg
        // is a TypeModel, then we treat it special to perform the binding.  We need
        // to map from our runtime model to .NET's RuntimeType object to match.
        //
        // To call this function, args and pinfos must be the same length, and param
        // types must be assignable from args.
        //
        // NOTE, if using this function, then need to use GetTargetArgsRestrictions
        // and make sure you're performing the same conversions as restrictions.
        //
        public static Expression[] ConvertArguments(DynamicMetaObject[] args, ParameterInfo[] ps)
        {
            Debug.Assert(args.Length == ps.Length, "Internal: args are not same len as params?!");

            var callArgs = new Expression[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                var argExpr = args[i].Expression;

                if (args[i].LimitType == typeof(TypeModel) && ps[i].ParameterType == typeof(Type))
                {
                    argExpr = GetRuntimeTypeMoFromModel(args[i]).Expression;
                }
                argExpr = Expression.Convert(argExpr, ps[i].ParameterType);
                callArgs[i] = argExpr;
            }
            return callArgs;
        }
    }
}
