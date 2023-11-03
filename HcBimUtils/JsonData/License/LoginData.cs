namespace HcBimUtils.JsonData.License
{
    public class LoginData
    {
        public string Email { get; set; }
        public string PassWord { get; set; }
        public bool IsCompany { get; set; }
        public string EmailCompany { get; set; }

        public LoginData()
        {
        }
    }
}