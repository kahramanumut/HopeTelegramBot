using System;

public class Question
{

    public int Id { get; set; }
    public string Topic { get; set; }
    public string Description { get; set;}
    public DateTime Timestamp { get; set;}
    public string PhotoUrl { get; set;}
    public string Priority{ get; set;}
    public bool EmailSending {get;set;}
    public User User { get; set; }
}