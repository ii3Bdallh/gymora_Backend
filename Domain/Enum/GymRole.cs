namespace Domain.Enum
{
    public enum GymRole
    {
        Owner = 0,
        Manager = 1,
        Coach = 2,
        Receptionist = 3,
        Cleaner = 4,
        Other = 5,
        Member = 10
    }

    public static class GymRoleExtensions
    {
        public static string ToRoleString(this GymRole role)
        {
            return role switch
            {
                GymRole.Owner => "Owner",
                GymRole.Manager => "Manager",
                GymRole.Coach => "Coach",
                GymRole.Receptionist => "Receptionist",
                GymRole.Cleaner => "Cleaner",
                GymRole.Other => "Other",
                GymRole.Member => "Member",
                _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
            };
        }
    }


    public static class GymRoleString
    {
        public const string Owner = "Owner";
        public const string Manager = "Manager";
        public const string Coach = "Coach";
        public const string Receptionist = "Receptionist";
        public const string Cleaner = "Cleaner";
        public const string Other = "Other";
        public const string Member = "Member";
    }


}


