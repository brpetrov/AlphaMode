using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AlphaMode.Helpers
{
    public static class DisplayHelper
    {
        public static string GetEnumDisplayName(Enum value)
        {
            if (value == null) return string.Empty;

            var member = value.GetType().GetMember(value.ToString());
            if (member.Length > 0)
            {
                var displayAttr = member[0].GetCustomAttribute<DisplayAttribute>();
                if (displayAttr != null && !string.IsNullOrEmpty(displayAttr.Name))
                    return displayAttr.Name;
            }
            return value.ToString();
        }
    }
}
