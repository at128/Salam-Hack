namespace SalamHack.Domain.Common.Constants;

public static class ApplicationConstants
{
    public const string Timezone = "UTC";

    public static class Roles
    {
        public const string Admin = "Admin";
        public const string User = "User";
    }

    public static class FieldLengths
    {
        public const int EmailMaxLength = 256;
        public const int PasswordMinLength = 8;
        public const int FirstNameMaxLength = 100;
        public const int LastNameMaxLength = 100;
        public const int PhoneNumberMaxLength = 32;
    }
}
