using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

public static class RAHR_ExtensionMethods {
    public static int ToInt32(this long i) {
        return Convert.ToInt32(i);
    }

    public static int ToInt32(this float i) {
        return Convert.ToInt32(i);
    }

    public static long ToLong(this float i) {
        return Convert.ToInt64(i);
    }

    public static bool IsSubcategoryOf(this RAHR_SubCategory sub, RAHR_Category cat) {
        Type t = typeof(RAHR_SubCategory);
        MemberInfo mi = t.GetMember(sub.ToString()).FirstOrDefault(m => m.GetCustomAttributes(typeof(RAHR_SubCategoryOf), true) != null);
        if(mi == null) throw new ArgumentException("SubCategory " + sub + " has no category.");
        RAHR_SubCategoryOf subAttr = (RAHR_SubCategoryOf)mi.GetCustomAttributes(typeof(RAHR_SubCategoryOf), true)[0];
        return subAttr.Category == cat;
    }

    public static RAHR_Category GetCategoryOf(this RAHR_SubCategory sub) {
        Type t = typeof(RAHR_SubCategory);
        MemberInfo mi = t.GetMember(sub.ToString()).FirstOrDefault(m => m.GetCustomAttributes(typeof(RAHR_SubCategoryOf), true) != null);
        if(mi == null) throw new ArgumentException("SubCategory " + sub + " has no category.");
        RAHR_SubCategoryOf subAttr = (RAHR_SubCategoryOf)mi.GetCustomAttributes(typeof(RAHR_SubCategoryOf), true)[0];
        return subAttr.Category;
    }

    public static T ToEnum<T>(this string value) {
        return (T)Enum.Parse(typeof(T), value, true);
    }

    public static string GetDescription(this Enum value) {
        FieldInfo field = value.GetType().GetField(value.ToString());
        DescriptionAttribute attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;

        return attribute == null ? value.ToString() : attribute.Description;
    }

    public static T GetValueFromDescription<T>(string description) {
        var type = typeof(T);

        if(!type.IsEnum) throw new InvalidOperationException();

        foreach(var field in type.GetFields()) {
            var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;

            if(attribute != null) {
                if(attribute.Description == description) return (T)field.GetValue(null);
            } else {
                if(field.Name == description) return (T)field.GetValue(null);
            }
        }

        throw new ArgumentException("Not found.", description);
    }
}