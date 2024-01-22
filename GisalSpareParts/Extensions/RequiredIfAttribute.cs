using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

namespace GisalSpareParts.Extensions
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class RequiredIfAttribute : RequiredAttribute
    {
        public bool IsInverted { get; set; } = false;
        public string OtherProperty { get; private set; }
        public object OtherPropertyValue { get; private set; }
        public RequiredIfAttribute(string otherProperty, object otherPropertyValue) : base()
        {
            OtherProperty = otherProperty;
            OtherPropertyValue = otherPropertyValue;
        }
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            PropertyInfo otherPropertyInfo = validationContext.ObjectType.GetProperty(OtherProperty);
            if (otherPropertyInfo == null)
            {
                return new ValidationResult(string.Format(CultureInfo.CurrentCulture, "Не удалось найти свойство с именем {0}", validationContext.ObjectType, OtherProperty));
            }
            object actualOtherPropertyValue = otherPropertyInfo.GetValue(validationContext.ObjectInstance, null);
            if (!IsInverted && Equals(actualOtherPropertyValue, OtherPropertyValue) || IsInverted && !Equals(actualOtherPropertyValue, OtherPropertyValue))
            {
                return base.IsValid(value, validationContext);
            }
            return default;
        }
    }
}
