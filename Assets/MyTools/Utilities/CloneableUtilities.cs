using System;
using System.Reflection;

namespace VS.Utilities {
    public static class CloneableUtilities {
        public static void CopyValues<T>(T @base, T copy) {
            Type type = @base.GetType();

            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
                object value = field.GetValue(@base);

                if (value is ICloneable cloneable) {
                    field.SetValue(copy, cloneable.Clone());
                }
                else {
                    field.SetValue(copy, value);
                }
            }
        }
    }
}