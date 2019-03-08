using System.Collections.Generic;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }  
    public long UserId  {get;set;}
    public string Firstname{ get;set; }
    public string Lastname{ get;set; }

    public List<Question> Questions { get; set; }
}