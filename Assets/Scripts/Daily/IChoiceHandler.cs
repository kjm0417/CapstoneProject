using UnityEngine;

public interface IChoiceHandler
{
    bool CanHandle(string choiceId);
    void Handle(DailySelectButtonManager manager);

}
