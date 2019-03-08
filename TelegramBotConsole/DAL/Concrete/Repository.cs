using System;
using System.Linq;

public class Repository : IRepository
{
    private readonly BotDbContext dbContext;
    public Repository(BotDbContext _dbContext)
    {
        dbContext = _dbContext;
    }

    public bool SaveQuestion(Question entity)
    {
        try
        {
            dbContext.Question.Add(entity);
            dbContext.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool SaveUser(User entity)
    {
        try
        {
            dbContext.User.Add(entity);
            dbContext.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool DeleteStep(long userId)
    {
        try
        {
            var userStep = GetStep(userId);
            dbContext.Remove(userStep);
            dbContext.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public UserStepTemp GetStep(long userId)
    {
        return dbContext.UserStepTemp.FirstOrDefault(x => x.UserId == userId);
    }

    public bool SaveUpdateStep(long userId, int step)
    {
        try
        {
            var userStep = GetStep(userId);

            if (userStep == null)
                dbContext.UserStepTemp.Add(new UserStepTemp() { UserId = userId, QuestionStep = 0 });
            else
            {
                userStep.QuestionStep = step;
                dbContext.UserStepTemp.Update(userStep);
            }

            dbContext.SaveChanges();
            return true;
        }
        catch
        {
            //Todo , maybe add Exception logger
            return false;
        }
    }

    public User GetUser(long userId)
    {
        return dbContext.User.FirstOrDefault(x=>x.UserId == userId);
    }

    public bool CheckQuestionLimit(long userId,int questionLimit)
    {
        int dailyQuestion = dbContext.Question.Where(x=>x.User.UserId==userId && x.Timestamp.Date == DateTime.Now.Date).Count();
        if(questionLimit>=dailyQuestion)
            return true;
        
        return false;
    }
}