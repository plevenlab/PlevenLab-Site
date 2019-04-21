using PlevenLab.Data.Entities;

namespace PlevenLab.Data.DTO
{
    public class LoginResultDTO
    {
        public User User { get; set; }
        public string Token { get; set; }
    }
}
