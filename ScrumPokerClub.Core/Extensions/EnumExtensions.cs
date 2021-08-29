using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace ScrumPokerClub.Extensions
{
    public static class EnumExtensions
    {
        public static string Description(this Enum val)
        {
            var member = val.GetType().GetMember(val.ToString());
            var attr = member.Single().GetCustomAttribute<DescriptionAttribute>();
            return attr != null ? attr.Description : val.ToString();
        }
    }
}
