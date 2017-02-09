using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace QuickName
{
    public static class QuickName
    {
        public static PropertyInfo GetProperyInfo<T, TPropery>(Func<T, TPropery> func)
        {
            var methodInfo = func.Method;
            PropertyInfo value;
            if (dictionary.TryGetValue(methodInfo, out value)) return value;
            var tuples = IlReader.Read(methodInfo).ToList();
            if (!tuples.Select(_ => _.Item1).SequenceEqual(new[] {OpCodes.Ldarg_1, OpCodes.Callvirt, OpCodes.Ret}))
                throw new ArgumentException($"The{nameof(func)} should encapsulate a method with a body that " +
                    "consists of a sequence of intermediate language instructions " +
                    $"{nameof(OpCodes.Ldarg_1)}, {nameof(OpCodes.Callvirt)}, {nameof(OpCodes.Ret)}.", nameof(func));
            return ResolveAndCachePropertyInfo(methodInfo, tuples[1].Item2.Value);
        }

        public static PropertyInfo GetProperyInfo<TPropery>(Func<TPropery> func)
        {
            var methodInfo = func.Method;
            PropertyInfo value;
            if (dictionary.TryGetValue(methodInfo, out value)) return value;
            var tuples = IlReader.Read(methodInfo).ToList();
            if (!tuples.Select(_ => _.Item1).SequenceEqual(new[] {OpCodes.Call, OpCodes.Ret}))
                throw new ArgumentException($"The{nameof(func)} should encapsulate a method with a body that " +
                    "consists of a sequence of intermediate language instructions " +
                    $"{nameof(OpCodes.Call)}, {nameof(OpCodes.Ret)}.", nameof(func));
            return ResolveAndCachePropertyInfo(methodInfo, tuples[0].Item2.Value);
        }

        private static PropertyInfo ResolveAndCachePropertyInfo(MethodInfo methodInfo, int metadataToken)
        {
            var methodBase = methodInfo.Module.ResolveMethod(metadataToken,
                methodInfo.DeclaringType.GetGenericArguments(), null);
            Dictionary<MethodBase, PropertyInfo> infos;
            if (!propertyDictionary.TryGetValue(methodBase.DeclaringType, out infos))
            {
                infos = methodBase.DeclaringType.GetProperties().ToDictionary(_ => {
                    MethodBase method = _.GetGetMethod();
                    return method;
                });
                propertyDictionary.TryAdd(methodBase.DeclaringType, infos);
            }
            var propertyInfo = infos[methodBase];
            dictionary.TryAdd(methodInfo, propertyInfo);
            return propertyInfo;
        }

        private static readonly ConcurrentDictionary<MethodInfo, PropertyInfo> dictionary =
            new ConcurrentDictionary<MethodInfo, PropertyInfo>();

        private static readonly ConcurrentDictionary<Type, Dictionary<MethodBase, PropertyInfo>> propertyDictionary =
            new ConcurrentDictionary<Type, Dictionary<MethodBase, PropertyInfo>>();
    }
}