using System;
using System.ComponentModel;
using System.Reflection;

namespace CodeRedCreations.Models
{
    public enum PartTypeEnum
    {
        [Description("Aero & Body")]
        Aero_Body,
        [Description("Air Intake")]
        Air_Intake,
        [Description("Brakes")]
        Brakes,
        [Description("Cooling")]
        Cooling,
        [Description("Clutch & Flywheel")]
        Clutch_Flywheel,
        [Description("Engine Management")]
        Engine_Management,
        [Description("Electronics")]
        Electroncis,
        [Description("Engine Dress-Up")]
        Endine_Dress_Up,
        [Description("Engine Parts")]
        Engine_Parts,
        [Description("Exhaust")]
        Exhaust,
        [Description("Forced Induction")]
        Forced_Induction,
        [Description("Fuel System")]
        Fuel_System,
        [Description("Guages & Pods")]
        Guages_Pods,
        [Description("Intercooler")]
        Intercooler,
        [Description("Interior")]
        Interior,
        [Description("Lights & Emblems")]
        Lights_Emblems,
        [Description("Nitrus")]
        Nitrus,
        [Description("Oils & Lubricants")]
        Oils_Lubricants,
        [Description("Other")]
        Other,
        [Description("Suspension")]
        Suspension,
        [Description("Transmission & Drivetrain")]
        Transmission_Drivetrain,
        [Description("Wheels & Tires")]
        Wheels_Tires
    }

    public class EnumString
    {
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }
    }
}
