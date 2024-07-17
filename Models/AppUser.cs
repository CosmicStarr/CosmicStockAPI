namespace Models
{
    public class AppUser:IdentityUser
    {
        public ICollection<WishedProducts> WishiList { get; set; } 
        public int ChangeEmailCount { get; set; } = 0;
        public DateTime ChangeEmailDateInfo { get; set; } = new DateTime();
        public TimeSpan GetWaitTime()
        {
            var timeInfo =  DateTime.Now - ChangeEmailDateInfo;
            return timeInfo;
        }
    }
}