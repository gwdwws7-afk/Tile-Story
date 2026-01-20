using System.Collections.Generic;

namespace MySelf.Model
{
    public class StoryModelData
    {
        public List<string> RecordShowStoryIDs = new List<string>();
    }

    public class StoryModel : BaseModel<StoryModel, StoryModelData>
    {
        public bool IsHaveShowStory(int chapterId, int buildSchedule)
        {
            return Data.RecordShowStoryIDs.Contains($"{chapterId}_{buildSchedule}");
        }

        public void RecordShowStoryData(int chapterId,int buildSchedule)
        {
            string id = $"{chapterId}_{buildSchedule}";
            if (!Data.RecordShowStoryIDs.Contains(id))
            {
                Data.RecordShowStoryIDs.Add(id);
                SaveToLocal();
            }
        }
    }
}