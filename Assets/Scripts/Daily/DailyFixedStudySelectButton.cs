using UnityEngine;

public class DailyFixedStudySelectButton : DailySelectButtonManager, IChoiceHandler
{
    public bool CanHandle(string choiceId)
    {
        return choiceId == "FixedStudy";
    }

    public void Handle(DailySelectButtonManager manager)
    {
        manager.Mode = UIMode.FixedStudy;

        EventBus.Publish(UIManager.OnRestQuestions, false);
    }


}
