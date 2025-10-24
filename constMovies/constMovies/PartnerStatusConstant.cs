namespace helperMovies.constMovies
{
    public static class PartnerStatusConstant
    {
        public static string Active = "ACTIVE";
        public static string Inactive = "INACTIVE";
        public static string Suspended = "SUSPENDED";


        public static readonly List<string> All = new() { Active, Inactive, Suspended };
    }
}
