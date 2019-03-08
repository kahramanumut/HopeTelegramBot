public interface IRepository
{
    bool SaveQuestion(Question entity);
    bool SaveUser(User entity);
    UserStepTemp GetStep(long userId);
    User GetUser(long userId);
    bool SaveUpdateStep(long userId, int step);
    bool DeleteStep(long userId);
    bool CheckQuestionLimit(long userId,int questionLimit);
}