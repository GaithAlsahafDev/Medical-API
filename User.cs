namespace MedicalAppAPI;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    
    // هذا الحقل سيحتوي الآن على Hash طويل وليس كلمة المرور الأصلية
    public string PasswordHash { get; set; } 

    public string Role { get; set; }
}