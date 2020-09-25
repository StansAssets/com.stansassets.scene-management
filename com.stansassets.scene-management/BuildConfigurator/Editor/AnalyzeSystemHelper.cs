using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.AddressableAssets.Build.AnalyzeRules;

namespace StansAssets.SceneManagement.Build
{
    public static class AnalyzeSystemHelper
    {
        public static void ClearAnalysis(AnalyzeRule rule) {
            InvokeMethod("ClearAnalysis", BindingFlags.NonPublic | BindingFlags.Static,null, new object[] { rule });
        }

        public static List<AnalyzeRule.AnalyzeResult> RefreshRule(AnalyzeRule rule) {
            return (List<AnalyzeRule.AnalyzeResult>)InvokeMethod("RefreshAnalysis",
                BindingFlags.NonPublic | BindingFlags.Static, null, new object[] { rule });
        }

        public static void FixIssues(AnalyzeRule rule) {
            InvokeMethod("FixIssues", BindingFlags.NonPublic | BindingFlags.Static, null, new object[] { rule });
        }

        public static AnalyzeRule FindRule<T>() {
            return (AnalyzeRule)InvokeGenericMethod("FindRule", new Type[] { typeof(T) },
                BindingFlags.NonPublic | BindingFlags.Static, null, null);
        }

        private static object InvokeMethod(string methodName, BindingFlags flags, object obj, object[] parameters) {
            var methods = ReflectedType.GetMethods(flags);
            foreach (var method in methods) {
                if (method.Name.Equals(methodName) && !method.ContainsGenericParameters) {
                    return method.Invoke(obj, parameters);
                }
            }
            return null;
        }

        private static object InvokeGenericMethod(string methodName, Type[] types, BindingFlags flags, object obj, object[] parameters) {
            var methods = ReflectedType.GetMethods(flags);
            foreach (var method in methods) {
                if (method.Name.Equals(methodName) && method.ContainsGenericParameters) {
                    var generic = method.MakeGenericMethod(types);
                    return generic.Invoke(obj, parameters);
                }
            }
            return null;
        }

        private static Type s_type;
        private static Type ReflectedType {
            get {
                if (s_type == null) {
                    s_type = Type.GetType("UnityEditor.AddressableAssets.Build.AnalyzeSystem,Unity.Addressables.Editor.dll");
                }
                return s_type;
            }
        }
    }
}
