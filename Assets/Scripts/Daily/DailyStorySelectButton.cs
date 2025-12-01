using UnityEngine;

public class DailyStorySelectButton : DailySelectButtonManager, IChoiceHandler
{
    public bool CanHandle(string choiceId)
    {
        return choiceId == "Story";
    }

    public void Handle(DailySelectButtonManager manager)
    {
        manager.Mode = UIMode.None;

        EventBus.Publish(UIManager.OnRestQuestions, false);
    }
}
