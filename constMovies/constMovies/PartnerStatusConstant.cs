namespace helperMovies.constMovies
{
    public static class PartnerStatusConstant
    {
        public static string ACTIVE = "ACTIVE";
        public static string INACTIVE = "INACTIVE";
        public static string SUSPENDED = "SUSPENDED";


        public static readonly List<string> All = new() { ACTIVE, INACTIVE, SUSPENDED };
    }
}
